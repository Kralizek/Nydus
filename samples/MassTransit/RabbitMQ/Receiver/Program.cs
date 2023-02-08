﻿using System;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nybus;
using Types;

namespace Receiver
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var services = new ServiceCollection();

            services.AddLogging(logging => logging.AddConsole().SetMinimumLevel(LogLevel.Trace));

            services.AddNybus(nybus =>
            {
                nybus.UseMassTransitWithRabbitMq(c =>
                {
                    c.ConfigureMassTransit(mt =>
                    {
                        mt.Host(new Uri("rabbitmq://localhost"), h =>
                        {
                            h.Username("guest");
                            h.Password("guest");
                        });
                    });
                });

                nybus.SubscribeToCommand<DoSomethingCommand>(async (dispatcher, context) =>
                {
                    await Console.Out.WriteLineAsync($"Doing something: {context.Command.WhatToDo}");

                    await dispatcher.RaiseEventAsync<SomethingDoneEvent>(new SomethingDoneEvent
                    {
                        WhatWasDone = context.Command.WhatToDo
                    });
                });
            });

            var serviceProvider = services.BuildServiceProvider();

            var host = serviceProvider.GetRequiredService<IBusHost>();

            await host.StartAsync();

            Console.WriteLine("Press ENTER to exit...");
            Console.ReadLine();

            await host.StopAsync();
        }
    }
}
