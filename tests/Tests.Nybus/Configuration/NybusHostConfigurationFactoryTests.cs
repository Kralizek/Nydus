﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoFixture.NUnit3;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using Nybus.Configuration;
using Nybus.Policies;

namespace Tests.Configuration
{
    [TestFixture]
    public class NybusHostConfigurationFactoryTests
    {
        [Test]
        public void ErrorPolicyProviders_are_required()
        {
            Assert.Throws<ArgumentNullException>(() => new NybusHostConfigurationFactory(null));
        }

        [Test, CustomAutoMoqData]
        public void CreateConfiguration_uses_NoopErrorPolicy_if_no_policy_is_specified(NybusHostConfigurationFactory sut, NybusHostOptions options)
        {
            var configuration = sut.CreateConfiguration(options);

            Assert.That(configuration.ErrorPolicy, Is.InstanceOf<NoopErrorPolicy>());
        }

        [Test, CustomAutoMoqData]
        public void CreateConfiguration_uses_selected_provider([Frozen] IEnumerable<IErrorPolicyProvider> errorPolicyProviders, NybusHostConfigurationFactory sut, NybusHostOptions options)
        {
            Mock.Get(options.ErrorPolicy.GetSection("ProviderName")).SetupGet(p => p.Value).Returns(errorPolicyProviders.First().ProviderName);

            var configuration = sut.CreateConfiguration(options);

            Mock.Get(errorPolicyProviders.First()).Verify(p => p.CreatePolicy(options.ErrorPolicy), Times.Once);
            Assert.That(configuration.ErrorPolicy, Is.SameAs(errorPolicyProviders.First().CreatePolicy(options.ErrorPolicy)));
        }
    }
}
