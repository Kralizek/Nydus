﻿using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Nybus;
using Nybus.Configuration;
using Nybus.Filters;
using NybusConfiguratorExtensions = Nybus.NybusConfiguratorExtensions;

// ReSharper disable InvokeAsExtensionMethod

namespace Tests
{
    [TestFixture]
    public class NybusConfiguratorExtensionsTests
    {
        [Test, CustomAutoMoqData]
        public void SubscribeToCommand_registers_handler_for_command(TestNybusConfigurator nybus, ISubscriptionBuilder subscriptionBuilder)
        {
            NybusConfiguratorExtensions.SubscribeToCommand<FirstTestCommand, FirstTestCommandHandler>(nybus);

            nybus.ApplySubscriptions(subscriptionBuilder);

            Mock.Get(subscriptionBuilder).Verify(p => p.SubscribeToCommand<FirstTestCommand>(typeof(FirstTestCommandHandler)));
        }

        [Test, CustomAutoMoqData]
        public void SubscribeToCommand_registers_handler_type(TestNybusConfigurator nybus, IServiceCollection services)
        {
            NybusConfiguratorExtensions.SubscribeToCommand<FirstTestCommand, FirstTestCommandHandler>(nybus);

            nybus.ApplyServiceConfigurations(services);

            Mock.Get(services).Verify(p => p.Add(It.Is<ServiceDescriptor>(sd => sd.ServiceType == typeof(FirstTestCommandHandler))));
        }

        [Test, CustomAutoMoqData]
        public void SubscribeToCommand_registers_delegate_handler_for_command(TestNybusConfigurator nybus, ISubscriptionBuilder subscriptionBuilder)
        {
            var testHandler = Mock.Of<CommandReceived<FirstTestCommand>>();

            NybusConfiguratorExtensions.SubscribeToCommand(nybus, testHandler);

            nybus.ApplySubscriptions(subscriptionBuilder);

            Mock.Get(subscriptionBuilder).Verify(p => p.SubscribeToCommand<FirstTestCommand>(typeof(DelegateWrapperCommandHandler<FirstTestCommand>)));
        }

        [Test, CustomAutoMoqData]
        public void SubscribeToCommand_registers_delegate_handler_type(TestNybusConfigurator nybus, IServiceCollection services)
        {
            var testHandler = Mock.Of<CommandReceived<FirstTestCommand>>();

            NybusConfiguratorExtensions.SubscribeToCommand(nybus, testHandler);

            nybus.ApplyServiceConfigurations(services);

            Mock.Get(services).Verify(p => p.Add(It.Is<ServiceDescriptor>(sd => sd.ServiceType == typeof(DelegateWrapperCommandHandler<FirstTestCommand>))));
        }

        [Test, CustomAutoMoqData]
        public void SubscribeToCommand_subscribes_to_command_type(TestNybusConfigurator nybus, ISubscriptionBuilder subscriptionBuilder)
        {
            NybusConfiguratorExtensions.SubscribeToCommand<FirstTestCommand>(nybus);

            nybus.ApplySubscriptions(subscriptionBuilder);

            Mock.Get(subscriptionBuilder).Verify(p => p.SubscribeToCommand<FirstTestCommand>(typeof(ICommandHandler<FirstTestCommand>)));
        }

        [Test, CustomAutoMoqData]
        public void SubscribeToCommand_registers_handler_instance_for_command(TestNybusConfigurator nybus, ISubscriptionBuilder subscriptionBuilder, FirstTestCommandHandler handler)
        {
            NybusConfiguratorExtensions.SubscribeToCommand<FirstTestCommand, FirstTestCommandHandler>(nybus, handler);

            nybus.ApplySubscriptions(subscriptionBuilder);

            Mock.Get(subscriptionBuilder).Verify(p => p.SubscribeToCommand<FirstTestCommand>(handler.GetType()));
        }

        [Test, CustomAutoMoqData]
        public void SubscribeToCommand_registers_handler_instance(TestNybusConfigurator nybus, IServiceCollection services, FirstTestCommandHandler handler)
        {
            NybusConfiguratorExtensions.SubscribeToCommand<FirstTestCommand, FirstTestCommandHandler>(nybus, handler);

            nybus.ApplyServiceConfigurations(services);

            Mock.Get(services).Verify(p => p.Add(It.Is<ServiceDescriptor>(sd => sd.ServiceType == handler.GetType())));
        }

        [Test, CustomAutoMoqData]
        public void SubscribeToEvent_registers_handler_for_command(TestNybusConfigurator nybus, ISubscriptionBuilder subscriptionBuilder)
        {
            NybusConfiguratorExtensions.SubscribeToEvent<FirstTestEvent, FirstTestEventHandler>(nybus);

            nybus.ApplySubscriptions(subscriptionBuilder);

            Mock.Get(subscriptionBuilder).Verify(p => p.SubscribeToEvent<FirstTestEvent>(typeof(FirstTestEventHandler)));
        }

        [Test, CustomAutoMoqData]
        public void SubscribeToEvent_registers_handler_type(TestNybusConfigurator nybus, IServiceCollection services)
        {
            NybusConfiguratorExtensions.SubscribeToEvent<FirstTestEvent, FirstTestEventHandler>(nybus);

            nybus.ApplyServiceConfigurations(services);

            Mock.Get(services).Verify(p => p.Add(It.Is<ServiceDescriptor>(sd => sd.ServiceType == typeof(FirstTestEventHandler))));
        }

        [Test, CustomAutoMoqData]
        public void SubscribeToEvent_registers_delegate_handler_for_command(TestNybusConfigurator nybus, ISubscriptionBuilder subscriptionBuilder)
        {
            var testHandler = Mock.Of<EventReceived<FirstTestEvent>>();

            NybusConfiguratorExtensions.SubscribeToEvent(nybus, testHandler);

            nybus.ApplySubscriptions(subscriptionBuilder);

            Mock.Get(subscriptionBuilder).Verify(p => p.SubscribeToEvent<FirstTestEvent>(typeof(DelegateWrapperEventHandler<FirstTestEvent>)));
        }

        [Test, CustomAutoMoqData]
        public void SubscribeToEvent_registers_delegate_handler_type(TestNybusConfigurator nybus, IServiceCollection services)
        {
            var testHandler = Mock.Of<EventReceived<FirstTestEvent>>();

            NybusConfiguratorExtensions.SubscribeToEvent(nybus, testHandler);

            nybus.ApplyServiceConfigurations(services);

            Mock.Get(services).Verify(p => p.Add(It.Is<ServiceDescriptor>(sd => sd.ServiceType == typeof(DelegateWrapperEventHandler<FirstTestEvent>))));
        }

        [Test, CustomAutoMoqData]
        public void SubscribeToEvent_subscribes_to_command_type(TestNybusConfigurator nybus, ISubscriptionBuilder subscriptionBuilder)
        {
            NybusConfiguratorExtensions.SubscribeToEvent<FirstTestEvent>(nybus);

            nybus.ApplySubscriptions(subscriptionBuilder);

            Mock.Get(subscriptionBuilder).Verify(p => p.SubscribeToEvent<FirstTestEvent>(typeof(IEventHandler<FirstTestEvent>)));
        }

        [Test, CustomAutoMoqData]
        public void SubscribeToEvent_registers_handler_instance_for_command(TestNybusConfigurator nybus, ISubscriptionBuilder subscriptionBuilder, FirstTestEventHandler handler)
        {
            NybusConfiguratorExtensions.SubscribeToEvent<FirstTestEvent, FirstTestEventHandler>(nybus, handler);

            nybus.ApplySubscriptions(subscriptionBuilder);

            Mock.Get(subscriptionBuilder).Verify(p => p.SubscribeToEvent<FirstTestEvent>(handler.GetType()));
        }

        [Test, CustomAutoMoqData]
        public void SubscribeToEvent_registers_handler_instance(TestNybusConfigurator nybus, IServiceCollection services, FirstTestEventHandler handler)
        {
            NybusConfiguratorExtensions.SubscribeToEvent<FirstTestEvent, FirstTestEventHandler>(nybus, handler);

            nybus.ApplyServiceConfigurations(services);

            Mock.Get(services).Verify(p => p.Add(It.Is<ServiceDescriptor>(sd => sd.ServiceType == handler.GetType())));
        }

        [Test, CustomAutoMoqData]
        public void RegisterErrorFilterProvider_adds_provider_with_default_setup(TestNybusConfigurator nybus, IServiceCollection services)
        {
            NybusConfiguratorExtensions.RegisterErrorFilterProvider<TestErrorFilterProvider>(nybus);

            nybus.ApplyServiceConfigurations(services);

            Mock.Get(services).Verify(p => p.Add(It.Is<ServiceDescriptor>(sd => sd.ServiceType == typeof(IErrorFilterProvider) && sd.ImplementationType == typeof(TestErrorFilterProvider))));
        }

        [Test, CustomAutoMoqData]
        public void RegisterErrorFilterProvider_adds_provider_with_custom_setup(TestNybusConfigurator nybus, IServiceCollection services)
        {
            var providerFactory = Mock.Of<Func<IServiceProvider, IErrorFilterProvider>>();

            NybusConfiguratorExtensions.RegisterErrorFilterProvider<TestErrorFilterProvider>(nybus, providerFactory);

            nybus.ApplyServiceConfigurations(services);

            Mock.Get(services).Verify(p => p.Add(It.Is<ServiceDescriptor>(sd => sd.ServiceType == typeof(IErrorFilterProvider) && sd.ImplementationFactory == providerFactory)));
        }

    }

    public class TestErrorFilterProvider : IErrorFilterProvider
    {
        public string ProviderName { get; }

        public IErrorFilter CreateErrorFilter(IConfigurationSection settings)
        {
            throw new NotImplementedException();
        }
    }
}
