﻿using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nybus.Configuration;

namespace Nybus
{
    public class NybusHostBuilder : ISubscriptionBuilder
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IList<Action<IBusHost>> _subscriptions = new List<Action<IBusHost>>();

        public NybusHostBuilder(IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        public NybusHost BuildHost(IBusEngine engine, NybusHostOptions options)
        {
            var bus = new NybusHost(engine, options, _loggerFactory.CreateLogger<NybusHost>());

            foreach (var subscription in _subscriptions)
            {
                subscription(bus);
            }

            return bus;
        }

        public void SubscribeToCommand<TCommand>(Type commandHandlerType)
            where TCommand : class, ICommand
        {
            if (!typeof(ICommandHandler<TCommand>).GetTypeInfo().IsAssignableFrom(commandHandlerType.GetTypeInfo()))
            {
                throw new ArgumentNullException(nameof(commandHandlerType));
            }

            _subscriptions.Add(host => host.SubscribeToCommand<TCommand>(async (b, ctx) =>
            {
                var handler = (ICommandHandler<TCommand>)_serviceProvider.GetRequiredService(commandHandlerType);

                await handler.HandleAsync(b, ctx).ConfigureAwait(false);
            }));
        }

        public void SubscribeToEvent<TEvent>(Type eventHandlerType)
            where TEvent : class, IEvent
        {
            if (!typeof(IEventHandler<TEvent>).GetTypeInfo().IsAssignableFrom(eventHandlerType.GetTypeInfo()))
            {
                throw new ArgumentNullException(nameof(eventHandlerType));
            }

            _subscriptions.Add(host => host.SubscribeToEvent<TEvent>(async (b, ctx) =>
            {
                var handler = (IEventHandler<TEvent>)_serviceProvider.GetRequiredService(eventHandlerType);

                await handler.HandleAsync(b, ctx).ConfigureAwait(false);
            }));
        }
    }
}
