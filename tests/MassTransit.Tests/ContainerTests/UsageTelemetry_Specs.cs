namespace MassTransit.Tests.ContainerTests
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;
    using UsageTracking;


    /// <summary>
    /// ForkForward Cargo turns usage telemetry off by default, because it is reported to the original MassTransit
    /// project. None of these tests starts a bus, so nothing is reported.
    /// </summary>
    [TestFixture]
    public class Usage_telemetry
    {
        const string Variable = "MASSTRANSIT_USAGE_TELEMETRY";

        [Test]
        public async Task Should_be_off_by_default()
        {
            AssumeVariableNotSet();

            Assert.That(await IsEnabled(x =>
            {
            }), Is.False);
        }

        [Test]
        public async Task Should_be_off_when_the_options_change_something_else()
        {
            AssumeVariableNotSet();

            Assert.That(await IsEnabled(x => x.ConfigureUsageTelemetryOptions(options => options.CustomerId = "test")), Is.False);
        }

        [Test]
        public async Task Should_be_off_when_disabled()
        {
            AssumeVariableNotSet();

            Assert.That(await IsEnabled(x => x.DisableUsageTelemetry()), Is.False);
        }

        [Test]
        public async Task Should_be_on_when_enabled_in_the_options()
        {
            AssumeVariableNotSet();
            AssumeNoDebugger();

            Assert.That(await IsEnabled(x => x.ConfigureUsageTelemetryOptions(options => options.Enabled = true)), Is.True);
        }

        [Test]
        public async Task Should_be_on_when_enabled_by_the_environment_variable()
        {
            AssumeNoDebugger();

            var previous = Environment.GetEnvironmentVariable(Variable);
            Environment.SetEnvironmentVariable(Variable, "true");
            try
            {
                Assert.That(await IsEnabled(x =>
                {
                }), Is.True);
            }
            finally
            {
                Environment.SetEnvironmentVariable(Variable, previous);
            }
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

        static void AssumeVariableNotSet()
        {
            Assume.That(Environment.GetEnvironmentVariable(Variable), Is.Null.Or.Empty, $"{Variable} is set in this environment");
        }

        static void AssumeNoDebugger()
        {
            Assume.That(Debugger.IsAttached, Is.False, "Usage telemetry is always off while a debugger is attached");
        }
    }
}
