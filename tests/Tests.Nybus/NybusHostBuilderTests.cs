﻿using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Nybus;
using Nybus.Configuration;

namespace Tests
{
    [TestFixture]
    public class NybusHostBuilderTests
    {
        [Test, CustomAutoMoqData]
        public void BuildHost_assembles_host(NybusHostBuilder sut, IBusEngine engine, IServiceProvider serviceProvider, NybusConfiguration configuration)
        {
            var host = sut.BuildHost(engine, serviceProvider, configuration);

            Assert.That(host, Is.Not.Null);
        }

        [Test]
        [InlineAutoMoqData(typeof(FirstTestCommandHandler))]
        [InlineAutoMoqData(typeof(ICommandHandler<FirstTestCommand>))]
        [InlineAutoMoqData(typeof(DelegateWrapperCommandHandler<FirstTestCommand>))]
        public void BuildHost_assembles_host_with_subscribed_command(Type handlerType, NybusHostBuilder sut, IBusEngine engine, IServiceProvider serviceProvider, NybusConfiguration configuration)
        {
            sut.SubscribeToCommand<FirstTestCommand>(handlerType);

            var host = sut.BuildHost(engine, serviceProvider, configuration);

            Assert.That(host, Is.Not.Null);
        }

        [Test]
        [InlineAutoMoqData(typeof(FirstTestEventHandler))]
        [InlineAutoMoqData(typeof(IEventHandler<FirstTestEvent>))]
        [InlineAutoMoqData(typeof(DelegateWrapperEventHandler<FirstTestEvent>))]
        public void BuildHost_assembles_host_with_subscribed_event(Type handlerType, NybusHostBuilder sut, IBusEngine engine, IServiceProvider serviceProvider, NybusConfiguration configuration)
        {
            sut.SubscribeToEvent<FirstTestEvent>(handlerType);

            var host = sut.BuildHost(engine, serviceProvider, configuration);

            Assert.That(host, Is.Not.Null);
        }

        [Test, CustomAutoMoqData]
        public void SubscribeToCommand_throws_if_type_is_not_valid(NybusHostBuilder sut, Type type)
        {
            Assert.Throws<ArgumentException>(() => sut.SubscribeToCommand<FirstTestCommand>(type));
        }

        [Test, CustomAutoMoqData]
        public void SubscribeToEvent_throws_if_type_is_not_valid(NybusHostBuilder sut, Type type)
        {
            Assert.Throws<ArgumentException>(() => sut.SubscribeToEvent<FirstTestEvent>(type));
        }
    }
}
