using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Nybus;

namespace Tests
{
    [TestFixture]
    public class DelegateHandlerBareSetupTests
    {
        [Test, AutoMoqData]
        public async Task Host_can_loopback_commands(ServiceCollection services, FirstTestCommand testCommand)
        {
            var commandReceived = Mock.Of<CommandReceivedAsync<FirstTestCommand>>();

            services.AddLogging(l => l.AddDebug());

            services.AddNybus(nybus =>
            {
                nybus.UseInMemoryBusEngine();

                nybus.SubscribeToCommand(commandReceived);
            });

            var serviceProvider = services.BuildServiceProvider();

            var host = serviceProvider.GetRequiredService<IBusHost>();

            var bus = serviceProvider.GetRequiredService<IBus>();

            await host.StartAsync();

            await bus.InvokeCommandAsync(testCommand);

            await host.StopAsync();

            Mock.Get(commandReceived).Verify(p => p(It.IsAny<IDispatcher>(), It.IsAny<ICommandContext<FirstTestCommand>>()), Times.Once);
        }

        [Test, AutoMoqData]
        public async Task Host_can_loopback_events(ServiceCollection services, FirstTestEvent testEvent)
        {
            var eventReceived = Mock.Of<EventReceivedAsync<FirstTestEvent>>();

            services.AddLogging(l => l.AddDebug());

            services.AddNybus(nybus =>
            {
                nybus.UseInMemoryBusEngine();

                nybus.SubscribeToEvent(eventReceived);
            });

            var serviceProvider = services.BuildServiceProvider();

            var host = serviceProvider.GetRequiredService<IBusHost>();

            var bus = serviceProvider.GetRequiredService<IBus>();

            await host.StartAsync();

            await bus.RaiseEventAsync(testEvent);

            await host.StopAsync();

            Mock.Get(eventReceived).Verify(p => p(It.IsAny<IDispatcher>(), It.IsAny<IEventContext<FirstTestEvent>>()), Times.Once);
        }
    }
}