using System;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Nybus;
using RabbitMQ.Client;

namespace Tests.External
{
    [ExternalTestFixture]
    public class RegisteredHandlerBareSetupTests
    {
        [TearDown]
        public void OnTestComplete()
        {
            var connectionFactory = new ConnectionFactory();
            var connection = connectionFactory.CreateConnection();
            var model = connection.CreateModel();

            model.ExchangeDelete(ExchangeName(typeof(FirstTestCommand)));
            model.ExchangeDelete(ExchangeName(typeof(SecondTestCommand)));
            model.ExchangeDelete(ExchangeName(typeof(ThirdTestCommand)));
            model.ExchangeDelete(ExchangeName(typeof(FirstTestEvent)));
            model.ExchangeDelete(ExchangeName(typeof(SecondTestEvent)));
            model.ExchangeDelete(ExchangeName(typeof(ThirdTestEvent)));

            connection.Close();
        }

        private string ExchangeName(Type type) => $"{type.Namespace}:{type.Name}";

        [Test, AutoMoqData]
        public async Task Host_can_loopback_commands(SecondTestCommand testCommand, [Frozen] CommandReceivedAsync<SecondTestCommand> commandReceived, SecondTestCommandHandler handler)
        {
            var host = TestUtils.CreateNybusHost(nybus =>
            {
                nybus.SubscribeToCommand(commandReceived);

                nybus.UseRabbitMqBusEngine(rabbitMq =>
                {
                    rabbitMq.Configure(c => c.ConnectionFactory = new ConnectionFactory());
                });

                nybus.SubscribeToCommand<SecondTestCommand, SecondTestCommandHandler>();
            },
            services =>
            {
                services.AddSingleton(commandReceived);
                services.AddSingleton(handler);
            });

            await host.StartAsync();

            await host.Bus.InvokeCommandAsync(testCommand);

            await Task.Delay(TimeSpan.FromMilliseconds(50));

            await host.StopAsync();

            Mock.Get(commandReceived).Verify(p => p(It.IsAny<IDispatcher>(), It.IsAny<ICommandContext<SecondTestCommand>>()));
        }

        [Test, AutoMoqData]
        public async Task Host_can_loopback_events(SecondTestEvent testEvent, [Frozen] EventReceivedAsync<SecondTestEvent> eventReceived, SecondTestEventHandler handler)
        {
            var host = TestUtils.CreateNybusHost(nybus =>
            {
                nybus.SubscribeToEvent(eventReceived);

                nybus.UseRabbitMqBusEngine(rabbitMq =>
                {
                    rabbitMq.Configure(c => c.ConnectionFactory = new ConnectionFactory());
                });

                nybus.SubscribeToEvent<SecondTestEvent, SecondTestEventHandler>();
            },
            services =>
            {
                services.AddSingleton(eventReceived);
                services.AddSingleton(handler);

            });

            await host.StartAsync();

            await host.Bus.RaiseEventAsync(testEvent);

            await Task.Delay(TimeSpan.FromMilliseconds(50));

            await host.StopAsync();

            Mock.Get(eventReceived).Verify(p => p(It.IsAny<IDispatcher>(), It.IsAny<IEventContext<SecondTestEvent>>()));

        }

    }
}