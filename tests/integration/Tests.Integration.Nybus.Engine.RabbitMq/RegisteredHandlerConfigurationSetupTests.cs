using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FakeRabbitMQ;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Nybus;
using static Tests.TestUtils;

namespace Tests
{
    [TestFixture]
    public class RegisteredHandlerConfigurationSetupTests
    {
        [Test, AutoMoqData]
        public async Task Host_can_loopback_commands(FakeServer server, SecondTestCommand testCommand, [Frozen] CommandReceivedAsync<SecondTestCommand> commandReceived, SecondTestCommandHandler handler)
        {
            var settings = new Dictionary<string, string>
            {
                ["Nybus:ErrorPolicy:ProviderName"] = "retry",
                ["Nybus:ErrorPolicy:MaxRetries"] = "5",
            };

            var configurationBuilder = new ConfigurationBuilder().AddInMemoryCollection(settings);
            var configuration = configurationBuilder.Build();

            var host = CreateNybusHost(nybus =>
            {
                nybus.UseRabbitMqBusEngine(rabbitMq =>
                {
                    rabbitMq.Configure(c => c.ConnectionFactory = server.CreateConnectionFactory());
                });

                nybus.UseConfiguration(configuration);

                nybus.SubscribeToCommand<SecondTestCommand, SecondTestCommandHandler>();
            },
            services =>
            {
                services.AddSingleton(commandReceived);
                services.AddSingleton(handler);
            });

            await host.StartAsync();

            await host.Bus.InvokeCommandAsync(testCommand);

            await host.StopAsync();

            Mock.Get(commandReceived).Verify(p => p(It.IsAny<IDispatcher>(), It.IsAny<ICommandContext<SecondTestCommand>>()));
        }

        [Test, AutoMoqData]
        public async Task Host_can_loopback_events(FakeServer server, SecondTestEvent testEvent, [Frozen] EventReceivedAsync<SecondTestEvent> eventReceived, SecondTestEventHandler handler)
        {
            var settings = new Dictionary<string, string>
            {
                ["Nybus:ErrorPolicy:ProviderName"] = "retry",
                ["Nybus:ErrorPolicy:MaxRetries"] = "5",
            };

            var configurationBuilder = new ConfigurationBuilder().AddInMemoryCollection(settings);
            var configuration = configurationBuilder.Build();

            var host = CreateNybusHost(nybus =>
            {
                nybus.UseRabbitMqBusEngine(rabbitMq =>
                {
                    rabbitMq.Configure(c => c.ConnectionFactory = server.CreateConnectionFactory());
                });

                nybus.UseConfiguration(configuration);

                nybus.SubscribeToEvent<SecondTestEvent, SecondTestEventHandler>();
            },
            services =>
            {
                services.AddSingleton(eventReceived);
                services.AddSingleton(handler);
            });

            await host.StartAsync();

            await host.Bus.RaiseEventAsync(testEvent);

            await host.StopAsync();

            Mock.Get(eventReceived).Verify(p => p(It.IsAny<IDispatcher>(), It.IsAny<IEventContext<SecondTestEvent>>()));

        }

    }
}