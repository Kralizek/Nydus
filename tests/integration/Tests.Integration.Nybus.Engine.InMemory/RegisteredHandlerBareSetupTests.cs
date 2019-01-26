using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Nybus;

namespace Tests
{
    [TestFixture]
    public class RegisteredHandlerBareSetupTests
    {
        [Test, AutoMoqData]
        public async Task Host_can_loopback_commands(ServiceCollection services, SecondTestCommand testCommand, [Frozen] CommandReceivedAsync<SecondTestCommand> commandReceived, SecondTestCommandHandler handler)
        {
            services.AddLogging(l => l.AddDebug());

            services.AddSingleton(commandReceived);
            services.AddSingleton(handler);

            services.AddNybus(nybus =>
            {
                nybus.UseInMemoryBusEngine();

                nybus.SubscribeToCommand<SecondTestCommand, SecondTestCommandHandler>();
            });

            var serviceProvider = services.BuildServiceProvider();

            var host = serviceProvider.GetRequiredService<IBusHost>();

            var bus = serviceProvider.GetRequiredService<IBus>();

            await host.StartAsync();

            await bus.InvokeCommandAsync(testCommand);

            await host.StopAsync();

            Mock.Get(commandReceived).Verify(p => p(It.IsAny<IDispatcher>(), It.IsAny<ICommandContext<SecondTestCommand>>()));
        }

        [Test, AutoMoqData]
        public async Task Host_can_loopback_events(ServiceCollection services, SecondTestEvent testEvent, [Frozen] EventReceivedAsync<SecondTestEvent> eventReceived, SecondTestEventHandler handler)
        {
            services.AddLogging(l => l.AddDebug());

            services.AddSingleton(eventReceived);
            services.AddSingleton(handler);
            
            services.AddNybus(nybus =>
            {
                nybus.UseInMemoryBusEngine();

                nybus.SubscribeToEvent<SecondTestEvent, SecondTestEventHandler>();
            });

            var serviceProvider = services.BuildServiceProvider();

            var host = serviceProvider.GetRequiredService<IBusHost>();

            var bus = serviceProvider.GetRequiredService<IBus>();

            await host.StartAsync();

            await bus.RaiseEventAsync(testEvent);

            await host.StopAsync();

            Mock.Get(eventReceived).Verify(p => p(It.IsAny<IDispatcher>(), It.IsAny<IEventContext<SecondTestEvent>>()));

        }

    }
}