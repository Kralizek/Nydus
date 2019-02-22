﻿using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nybus.Utils;
using RabbitMQ.Client;

namespace Nybus.Configuration
{
    public interface IConfigurationFactory
    {
        IRabbitMqConfiguration Create(RabbitMqOptions options);
    }

    public class RabbitMqOptions
    {
        public IConfigurationSection ConnectionString { get; set; }

        public IConfigurationSection Connection { get; set; }

        public string OutboundEncoding { get; set; }

        public IConfigurationSection CommandQueue { get; set; }

        public IConfigurationSection EventQueue { get; set; }

        public ExchangeOptions CommandExchange { get; set; }

        public ExchangeOptions EventExchange { get; set; }
    }

    public class ConfigurationFactory : IConfigurationFactory
    {
        private readonly ILogger<ConfigurationFactory> _logger;
        private readonly IReadOnlyDictionary<string, IQueueFactoryProvider> _queueFactoryProviders;
        private readonly IConnectionFactoryProviders _connectionFactoryProviders;

        public ConfigurationFactory(IEnumerable<IQueueFactoryProvider> queueFactoryProviders, IConnectionFactoryProviders connectionFactoryProviders, ILogger<ConfigurationFactory> logger)
        {
            _connectionFactoryProviders = connectionFactoryProviders ?? throw new ArgumentNullException(nameof(connectionFactoryProviders));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _queueFactoryProviders = CreateDictionary(queueFactoryProviders ?? throw new ArgumentNullException(nameof(queueFactoryProviders)));
        }

        IReadOnlyDictionary<string, IQueueFactoryProvider> CreateDictionary(IEnumerable<IQueueFactoryProvider> providers)
        {
            var result = new Dictionary<string, IQueueFactoryProvider>(StringComparer.OrdinalIgnoreCase);

            foreach (var provider in providers)
            {
                if (!result.ContainsKey(provider.ProviderName))
                {
                    result.Add(provider.ProviderName, provider);
                }
            }

            return result;
        }

        public IRabbitMqConfiguration Create(RabbitMqOptions options)
        {
            var outboundEncoding = GetOutboundEncoding();
            var commandQueueFactory = GetQueueFactory(options.CommandQueue);
            var eventQueueFactory = GetQueueFactory(options.EventQueue);
            var connectionFactory = GetConnectionFactory();
            var commandExchangeManager = GetExchangeManager(options.CommandExchange);
            var eventExchangeManager = GetExchangeManager(options.EventExchange);

            return new RabbitMqConfiguration
            {
                OutboundEncoding = outboundEncoding,
                CommandQueueFactory = commandQueueFactory,
                EventQueueFactory = eventQueueFactory,
                ConnectionFactory = connectionFactory,
                CommandExchangeManager = commandExchangeManager,
                EventExchangeManager = eventExchangeManager
            };

            IExchangeManager GetExchangeManager(ExchangeOptions exchangeOptions)
            {
                return new ExchangeManager(exchangeOptions ?? new ExchangeOptions());
            }

            IQueueFactory GetQueueFactory(IConfigurationSection section)
            {
                if (section != null && section.TryGetValue("ProviderName", out var providerName) && _queueFactoryProviders.TryGetValue(providerName, out var provider))
                {
                    return provider.CreateFactory(section);
                }

                return TemporaryQueueFactory.Instance;
            }

            Encoding GetOutboundEncoding()
            {
                if (options.OutboundEncoding != null)
                {
                    try
                    {
                        var encoding = Encoding.GetEncoding(options.OutboundEncoding);

                        return encoding;
                    }
                    catch (ArgumentException ex)
                    {
                        _logger.LogError(new{outboundEncoding = options.OutboundEncoding}, ex,  (s,e) =>$"Encoding not valid: {s.outboundEncoding}");
                        throw new ConfigurationException($"Encoding not valid: {options.OutboundEncoding}", ex);
                    }
                }

                return Encoding.UTF8;
            }

            IConnectionFactory GetConnectionFactory()
            {
                if (options.ConnectionString != null && options.ConnectionString.Exists())
                {
                    return _connectionFactoryProviders.ConnectionString.CreateFactory(options.ConnectionString);
                }

                if (options.Connection != null && options.Connection.Exists())
                {
                    return _connectionFactoryProviders.ConnectionNode.CreateFactory(options.Connection);
                }

                return new ConnectionFactory { HostName = "localhost" };
            }
        }
    }
}