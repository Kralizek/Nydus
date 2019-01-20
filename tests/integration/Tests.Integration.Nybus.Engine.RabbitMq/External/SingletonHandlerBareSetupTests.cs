using System;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Nybus;
using RabbitMQ.Client;

namespace Tests.External
{
    [ExternalTestFixture]
    public class SingletonHandlerBareSetupTests
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
        public async Task Host_can_loopback_commands(FirstTestCommand testCommand)
        {
            var handler = Mock.Of<FirstTestCommandHandler>();

            var host = TestUtils.CreateNybusHost(nybus =>
            {
                nybus.SubscribeToCommand<FirstTestCommand, FirstTestCommandHandler>(handler);

                nybus.UseRabbitMqBusEngine(rabbitMq =>
                {
                    rabbitMq.Configure(c => c.ConnectionFactory = new ConnectionFactory());
                });
            });

            await host.StartAsync();

            await host.Bus.InvokeCommandAsync(testCommand);

            await Task.Delay(TimeSpan.FromMilliseconds(50));

            await host.StopAsync();

            Mock.Get(handler).Verify(p => p.HandleAsync(It.IsAny<IDispatcher>(), It.IsAny<ICommandContext<FirstTestCommand>>()), Times.Once);
        }

        [Test, AutoMoqData]
        public async Task Host_can_loopback_events(FirstTestEvent testEvent)
        {
            var handler = Mock.Of<FirstTestEventHandler>();

            var host = TestUtils.CreateNybusHost(nybus =>
            {
                nybus.SubscribeToEvent<FirstTestEvent, FirstTestEventHandler>(handler);

                nybus.UseRabbitMqBusEngine(rabbitMq =>
                {
                    rabbitMq.Configure(c => c.ConnectionFactory = new ConnectionFactory());
                });
            });

            await host.StartAsync();

            await host.Bus.RaiseEventAsync(testEvent);

            await Task.Delay(TimeSpan.FromMilliseconds(50));

            await host.StopAsync();

            Mock.Get(handler).Verify(p => p.HandleAsync(It.IsAny<IDispatcher>(), It.IsAny<IEventContext<FirstTestEvent>>()), Times.Once);
        }

    }
}