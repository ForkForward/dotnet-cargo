namespace MassTransit.Tests.ContainerTests
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Internals;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using NUnit.Framework;
    using UsageTracking;


    /// <summary>
    /// ForkForward Cargo turns usage telemetry off by default, and posts the report only to an address that is
    /// configured. The reports in these tests go to a receiver on the loopback interface, never to the network.
    /// </summary>
    [TestFixture]
    public class Usage_telemetry
    {
        const string EnabledVariable = "MASSTRANSIT_USAGE_TELEMETRY";
        const string AddressVariable = "MASSTRANSIT_USAGE_TELEMETRY_ADDRESS";

        [Test]
        public async Task Should_be_off_by_default()
        {
            AssumeVariablesNotSet();

            Assert.That(await IsEnabled(x =>
            {
            }), Is.False);
        }

        [Test]
        public async Task Should_be_off_when_the_options_change_something_else()
        {
            AssumeVariablesNotSet();

            Assert.That(await IsEnabled(x => x.ConfigureUsageTelemetryOptions(options => options.CustomerId = "test")), Is.False);
        }

        [Test]
        public async Task Should_be_off_when_disabled()
        {
            AssumeVariablesNotSet();

            Assert.That(await IsEnabled(x => x.DisableUsageTelemetry()), Is.False);
        }

        [Test]
        public async Task Should_be_on_when_enabled_in_the_options()
        {
            AssumeVariablesNotSet();
            AssumeNoDebugger();

            Assert.That(await IsEnabled(x => x.ConfigureUsageTelemetryOptions(options => options.Enabled = true)), Is.True);
        }

        [Test]
        public async Task Should_be_on_when_enabled_by_the_environment_variable()
        {
            AssumeNoDebugger();

            using (new EnvironmentVariable(EnabledVariable, "true"))
            {
                Assert.That(await IsEnabled(x =>
                {
                }), Is.True);
            }
        }

        [Test]
        public async Task Should_not_warn_when_off()
        {
            AssumeVariablesNotSet();

            List<string> warnings = await CreateBus(options =>
            {
            });

            Assert.That(warnings, Is.Empty);
        }

        [Test]
        public async Task Should_warn_when_on_without_an_address()
        {
            AssumeVariablesNotSet();
            AssumeNoDebugger();

            List<string> warnings = await CreateBus(options => options.Enabled = true);

            Assert.That(warnings, Has.Exactly(1).Contains("no report address is set"));
        }

        [Test]
        public async Task Should_warn_when_the_address_is_not_http()
        {
            AssumeVariablesNotSet();
            AssumeNoDebugger();

            List<string> warnings = await CreateBus(options =>
            {
                options.Enabled = true;
                options.ReportAddress = new Uri("ftp://127.0.0.1/usage");
            });

            Assert.That(warnings, Has.Exactly(1).Contains("ftp://127.0.0.1/usage is not an absolute http or https address"));
        }

        [Test]
        public async Task Should_post_the_report_to_the_address()
        {
            AssumeVariablesNotSet();
            AssumeNoDebugger();

            using var receiver = new ReportReceiver(HttpStatusCode.Accepted);

            List<string> warnings = await RunBus(options =>
            {
                options.Enabled = true;
                options.ReportAddress = receiver.Address;
            });

            var report = await receiver.Report.OrTimeout(TimeSpan.FromSeconds(15));

            Assert.Multiple(() =>
            {
                Assert.That(report.Method, Is.EqualTo("POST"));
                Assert.That(report.Path, Is.EqualTo("/usage"));
                Assert.That(report.ContentType, Does.StartWith("application/json"));
                Assert.That(report.Body, Does.Contain("\"mass_transit_version\""));
                Assert.That(report.Body, Does.Contain("\"name\":\"IBus\""));
                Assert.That(warnings, Is.Empty);
            });
        }

        [Test]
        public async Task Should_post_to_the_address_from_the_environment_variable()
        {
            AssumeVariablesNotSet();
            AssumeNoDebugger();

            using var receiver = new ReportReceiver(HttpStatusCode.OK);
            using var ignored = new ReportReceiver(HttpStatusCode.OK);

            using (new EnvironmentVariable(AddressVariable, receiver.Address.ToString()))
            {
                await RunBus(options =>
                {
                    options.Enabled = true;
                    options.ReportAddress = ignored.Address;
                });
            }

            var report = await receiver.Report.OrTimeout(TimeSpan.FromSeconds(15));

            Assert.Multiple(() =>
            {
                Assert.That(report.Method, Is.EqualTo("POST"));
                Assert.That(ignored.Report.IsCompleted, Is.False);
            });
        }

        [Test]
        public async Task Should_warn_when_the_receiver_fails()
        {
            AssumeVariablesNotSet();
            AssumeNoDebugger();

            using var receiver = new ReportReceiver(HttpStatusCode.InternalServerError);

            List<string> warnings = await RunBus(options =>
            {
                options.Enabled = true;
                options.ReportAddress = receiver.Address;
            });

            await receiver.Report.OrTimeout(TimeSpan.FromSeconds(15));

            Assert.That(warnings, Has.Exactly(1).Contains($"Failed to report usage telemetry to {receiver.Address}"));
        }

        [Test]
        public async Task Should_warn_when_nobody_listens_at_the_address()
        {
            AssumeVariablesNotSet();
            AssumeNoDebugger();

            // A port that was free a moment ago: the connection is refused.
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var address = new Uri($"http://127.0.0.1:{((IPEndPoint)listener.LocalEndpoint).Port}/usage");
            listener.Stop();

            List<string> warnings = await RunBus(options =>
            {
                options.Enabled = true;
                options.ReportAddress = address;
            });

            Assert.That(warnings, Has.Exactly(1).Contains($"Failed to report usage telemetry to {address}"));
        }

        static async Task<bool> IsEnabled(Action<IBusRegistrationConfigurator> configure)
        {
            await using var provider = new ServiceCollection()
                .AddMassTransit(x =>
                {
                    configure(x);

                    x.UsingInMemory();
                })
                .BuildServiceProvider(true);

            var tracker = (UsageTracker)provider.GetRequiredService<IUsageTracker>();

            return tracker.Enabled;
        }

        /// <summary>
        /// Creates the bus, which configures usage telemetry, without starting it. Returns the usage telemetry warnings.
        /// </summary>
        static Task<List<string>> CreateBus(Action<UsageTelemetryOptions> configure)
        {
            return WithLog(async () =>
            {
                await using var provider = CreateProvider(configure);

                provider.GetRequiredService<IBusControl>();
            });
        }

        /// <summary>
        /// Starts and stops the bus. ReportOnShutdown makes the report go out when the provider is disposed, instead of
        /// minutes after the start. Returns the usage telemetry warnings.
        /// </summary>
        static Task<List<string>> RunBus(Action<UsageTelemetryOptions> configure)
        {
            return WithLog(async () =>
            {
                await using var provider = CreateProvider(options =>
                {
                    options.ReportOnShutdown = true;
                    configure(options);
                });

                var bus = provider.GetRequiredService<IBusControl>();

                using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));

                await bus.StartAsync(timeout.Token);
                try
                {
                    // The report task starts once the bus has started.
                    await Task.Delay(TimeSpan.FromSeconds(1), timeout.Token);
                }
                finally
                {
                    await bus.StopAsync(timeout.Token);
                }
            });
        }

        static ServiceProvider CreateProvider(Action<UsageTelemetryOptions> configure)
        {
            return new ServiceCollection()
                .AddMassTransit(x =>
                {
                    x.ConfigureUsageTelemetryOptions(configure);

                    x.UsingInMemory();
                })
                .BuildServiceProvider(true);
        }

        static async Task<List<string>> WithLog(Func<Task> action)
        {
            var previous = LogContext.Current;
            var loggerFactory = new CapturingLoggerFactory();

            LogContext.ConfigureCurrentLogContext(loggerFactory);
            try
            {
                await action();
            }
            finally
            {
                LogContext.Current = previous;
            }

            return loggerFactory.Entries
                .Where(x => x.Level == LogLevel.Warning && x.Message.IndexOf("usage telemetry", StringComparison.OrdinalIgnoreCase) >= 0)
                .Select(x => x.Message)
                .ToList();
        }

        static void AssumeVariablesNotSet()
        {
            Assume.That(Environment.GetEnvironmentVariable(EnabledVariable), Is.Null.Or.Empty, $"{EnabledVariable} is set in this environment");
            Assume.That(Environment.GetEnvironmentVariable(AddressVariable), Is.Null.Or.Empty, $"{AddressVariable} is set in this environment");
        }

        static void AssumeNoDebugger()
        {
            Assume.That(Debugger.IsAttached, Is.False, "Usage telemetry is always off while a debugger is attached");
        }


        sealed class EnvironmentVariable :
            IDisposable
        {
            readonly string _name;
            readonly string _previous;

            public EnvironmentVariable(string name, string value)
            {
                _name = name;
                _previous = Environment.GetEnvironmentVariable(name);
                Environment.SetEnvironmentVariable(name, value);
            }

            public void Dispose()
            {
                Environment.SetEnvironmentVariable(_name, _previous);
            }
        }


        sealed class CapturingLoggerFactory :
            ILoggerFactory
        {
            readonly ConcurrentQueue<(LogLevel Level, string Message)> _entries = new ConcurrentQueue<(LogLevel, string)>();

            public IEnumerable<(LogLevel Level, string Message)> Entries => _entries;

            public ILogger CreateLogger(string categoryName)
            {
                return new Logger(_entries);
            }

            public void AddProvider(ILoggerProvider provider)
            {
            }

            public void Dispose()
            {
            }


            sealed class Logger :
                ILogger
            {
                readonly ConcurrentQueue<(LogLevel, string)> _entries;

                public Logger(ConcurrentQueue<(LogLevel, string)> entries)
                {
                    _entries = entries;
                }

                public IDisposable BeginScope<TState>(TState state)
                {
                    return null;
                }

                public bool IsEnabled(LogLevel logLevel)
                {
                    return true;
                }

                public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
                {
                    _entries.Enqueue((logLevel, formatter(state, exception)));
                }
            }
        }


        /// <summary>
        /// Accepts one HTTP request on the loopback interface and answers it with the given status. A plain TCP listener
        /// keeps it independent of HttpListener, which needs a URL reservation on Windows.
        /// </summary>
        sealed class ReportReceiver :
            IDisposable
        {
            readonly TcpListener _listener;
            readonly TaskCompletionSource<ReceivedReport> _report;
            readonly HttpStatusCode _status;

            public ReportReceiver(HttpStatusCode status)
            {
                _status = status;
                _report = new TaskCompletionSource<ReceivedReport>(TaskCreationOptions.RunContinuationsAsynchronously);
                _listener = new TcpListener(IPAddress.Loopback, 0);
                _listener.Start();

                Address = new Uri($"http://127.0.0.1:{((IPEndPoint)_listener.LocalEndpoint).Port}/usage");

                _ = Task.Run(Receive);
            }

            public Uri Address { get; }

            public Task<ReceivedReport> Report => _report.Task;

            public void Dispose()
            {
                // Cancelled first, so the exception from stopping the listener is not left unobserved.
                _report.TrySetCanceled();
                _listener.Stop();
            }

            async Task Receive()
            {
                try
                {
                    using var client = await _listener.AcceptTcpClientAsync();
                    using var stream = client.GetStream();

                    var head = await ReadHead(stream);
                    string[] lines = head.Split(new[] { "\r\n" }, StringSplitOptions.None);
                    string[] requestLine = lines[0].Split(' ');

                    var headers = lines.Skip(1)
                        .Where(x => x.Contains(":"))
                        .ToDictionary(x => x.Substring(0, x.IndexOf(':')).Trim(), x => x.Substring(x.IndexOf(':') + 1).Trim(),
                            StringComparer.OrdinalIgnoreCase);

                    // .NET Framework asks before sending the body.
                    if (headers.TryGetValue("Expect", out var expect) && expect.Equals("100-continue", StringComparison.OrdinalIgnoreCase))
                        await Write(stream, "HTTP/1.1 100 Continue\r\n\r\n");

                    var length = headers.TryGetValue("Content-Length", out var value) ? int.Parse(value) : 0;
                    var body = await ReadBody(stream, length);

                    await Write(stream, $"HTTP/1.1 {(int)_status} {_status}\r\nContent-Length: 0\r\nConnection: close\r\n\r\n");

                    _report.TrySetResult(new ReceivedReport(requestLine[0], requestLine[1],
                        headers.TryGetValue("Content-Type", out var contentType) ? contentType : null, body));
                }
                catch (Exception exception)
                {
                    _report.TrySetException(exception);
                }
            }

            static async Task<string> ReadHead(Stream stream)
            {
                var bytes = new List<byte>();
                var buffer = new byte[1];
                while (!(bytes.Count >= 4 && bytes[bytes.Count - 4] == '\r' && bytes[bytes.Count - 3] == '\n'
                           && bytes[bytes.Count - 2] == '\r' && bytes[bytes.Count - 1] == '\n'))
                {
                    if (await stream.ReadAsync(buffer, 0, 1) == 0)
                        throw new EndOfStreamException("The request ended before its headers did");

                    bytes.Add(buffer[0]);
                }

                return Encoding.ASCII.GetString(bytes.ToArray(), 0, bytes.Count - 4);
            }

            static async Task<string> ReadBody(Stream stream, int length)
            {
                var buffer = new byte[length];
                var read = 0;
                while (read < length)
                {
                    var count = await stream.ReadAsync(buffer, read, length - read);
                    if (count == 0)
                        throw new EndOfStreamException("The request ended before its body did");

                    read += count;
                }

                return Encoding.UTF8.GetString(buffer);
            }

            static Task Write(Stream stream, string text)
            {
                var bytes = Encoding.ASCII.GetBytes(text);

                return stream.WriteAsync(bytes, 0, bytes.Length);
            }
        }


        sealed class ReceivedReport
        {
            public ReceivedReport(string method, string path, string contentType, string body)
            {
                Method = method;
                Path = path;
                ContentType = contentType;
                Body = body;
            }

            public string Method { get; }
            public string Path { get; }
            public string ContentType { get; }
            public string Body { get; }
        }
    }
}
