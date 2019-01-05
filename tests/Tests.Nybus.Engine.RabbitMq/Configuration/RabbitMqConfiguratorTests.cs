﻿using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Nybus.Configuration;

namespace Tests.Configuration
{
    [TestFixture]
    public class RabbitMqConfiguratorTests
    {
        [Test, AutoMoqData]
        public void RegisterQueueFactoryProvider_adds_provider_with_default_setup(RabbitMqConfigurator sut, TestNybusConfigurator configurator)
        {
            sut.RegisterQueueFactoryProvider<TestQueueFactoryProvider>();

            sut.Apply(configurator);

            var services = new ServiceCollection();

            configurator.ApplyServiceConfigurations(services);

            var serviceProvider = services.BuildServiceProvider();

            var provider = serviceProvider.GetService<IQueueFactoryProvider>();

            Assert.That(provider, Is.InstanceOf<TestQueueFactoryProvider>());
        }

        [Test, AutoMoqData]
        public void RegisterQueueFactoryProvider_adds_provider_with_custom_setup(RabbitMqConfigurator sut, TestNybusConfigurator configurator, TestQueueFactoryProvider factoryProvider)
        {
            var setup = new Mock<Func<IServiceProvider, IQueueFactoryProvider>>();
            setup.Setup(p => p(It.IsAny<IServiceProvider>())).Returns(factoryProvider);

            sut.RegisterQueueFactoryProvider<TestQueueFactoryProvider>(setup.Object);

            sut.Apply(configurator);

            var services = new ServiceCollection();

            configurator.ApplyServiceConfigurations(services);

            var serviceProvider = services.BuildServiceProvider();

            var provider = serviceProvider.GetService<IQueueFactoryProvider>();

            setup.Verify(s => s(It.IsAny<IServiceProvider>()), Times.Once);
            Assert.That(provider, Is.SameAs(factoryProvider));
        }

        [Test, AutoMoqData]
        public void Configure_sets_action_to_be_used(RabbitMqConfigurator sut, TestNybusConfigurator configurator, IConfigurationFactory configurationFactory, RabbitMqOptions options)
        {
            var configurationSetup = new Mock<Action<IRabbitMqConfiguration>>();
            configurationSetup.Setup(p => p(It.IsAny<IRabbitMqConfiguration>()));

            sut.Configure(configurationSetup.Object);

            sut.Apply(configurator);

            var services = new ServiceCollection();
            services.AddSingleton(configurationFactory);
            services.AddSingleton(options);

            configurator.ApplyServiceConfigurations(services);

            var serviceProvider = services.BuildServiceProvider();

            var configuration = serviceProvider.GetService<IRabbitMqConfiguration>();

            configurationSetup.Verify(p => p(configuration), Times.Once);
        }

        [Test, AutoMoqData]
        public void UseConfiguration_binds_values_to_options(RabbitMqConfigurator sut, TestNybusConfigurator configurator, IConfigurationFactory configurationFactory, string nybusSectionName, string rabbitMqSectionName)
        {
            var values = new Dictionary<string, string>
            {
                [$"{nybusSectionName}:{rabbitMqSectionName}:OutboundEncoding"] = "utf-8"
            };

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(values);

            var settings = configurationBuilder.Build();

            configurator.UseConfiguration(settings, nybusSectionName);

            sut.UseConfiguration(rabbitMqSectionName);

            sut.Apply(configurator);
            
            var services = new ServiceCollection();
            services.AddSingleton(configurationFactory);

            configurator.ApplyServiceConfigurations(services);

            var serviceProvider = services.BuildServiceProvider();

            var configuration = serviceProvider.GetService<IRabbitMqConfiguration>();

            Assert.That(configuration.OutboundEncoding, Is.SameAs(Encoding.UTF8));
        }
    }

    public class TestQueueFactoryProvider : IQueueFactoryProvider
    {
        public string ProviderName { get; }

        public IQueueFactory CreateFactory(IConfigurationSection settings)
        {
            throw new NotImplementedException();
        }
    }
}
