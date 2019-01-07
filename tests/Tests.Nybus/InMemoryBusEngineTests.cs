﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Nybus;

namespace Tests
{
    [TestFixture]
    public class InMemoryBusEngineTests
    {
        [Test, AutoMoqData]
        public void SubscribeToCommand_adds_type_to_AcceptedTypes_list(InMemoryBusEngine sut)
        {
            sut.SubscribeToCommand<FirstTestCommand>();

            Assert.That(sut.IsTypeAccepted(typeof(FirstTestCommand)));
        }

        [Test, AutoMoqData]
        public void SubscribeToEvent_adds_type_AcceptedTypes_list(InMemoryBusEngine sut)
        {
            sut.SubscribeToEvent<FirstTestEvent>();

            Assert.That(sut.IsTypeAccepted(typeof(FirstTestEvent)));
        }

        [Test, AutoMoqData]
        public async Task Sent_commands_are_received(InMemoryBusEngine sut, CommandMessage<FirstTestCommand> testMessage)
        {
            sut.SubscribeToCommand<FirstTestCommand>();

            var sequence = await sut.StartAsync().ConfigureAwait(false);

            var items = sequence.DumpInList();

            await sut.SendCommandAsync(testMessage);
            
            Assert.That(items.First(), Is.SameAs(testMessage));
        }

        [Test, AutoMoqData]
        public async Task Sent_events_are_received(InMemoryBusEngine sut, EventMessage<FirstTestEvent> testMessage)
        {
            sut.SubscribeToEvent<FirstTestEvent>();

            var sequence = await sut.StartAsync().ConfigureAwait(false);

            var items = sequence.DumpInList();

            await sut.SendEventAsync(testMessage);

            Assert.That(items.First(), Is.SameAs(testMessage));
        }

        [Test, AutoMoqData]
        public void Stop_completes_the_sequence_if_started(InMemoryBusEngine sut)
        {
            var sequence = sut.StartAsync().Result;

            var isCompleted = false;

            sequence.Subscribe(
                onNext: _ => { },
                onError: _ => { },
                onCompleted: () => isCompleted = true
            );

            sut.StopAsync().Wait();

            Assert.That(isCompleted, Is.True);
        }

        [Test, AutoMoqData]
        public void Stop_is_ignored_if_not_started(InMemoryBusEngine sut)
        {
            sut.StopAsync().Wait();
        }

        [Test, AutoMoqData]
        public void NotifySuccess_returns_completed_task(InMemoryBusEngine sut, CommandMessage<FirstTestCommand> testMessage)
        {
            Assert.That(sut.NotifySuccessAsync(testMessage), Is.SameAs(Task.CompletedTask));
        }

        [Test, AutoMoqData]
        public void NotifySuccess_returns_completed_task(InMemoryBusEngine sut, EventMessage<FirstTestEvent> testMessage)
        {
            Assert.That(sut.NotifySuccessAsync(testMessage), Is.SameAs(Task.CompletedTask));
        }

        [Test, AutoMoqData]
        public void NotifyFail_returns_completed_task(InMemoryBusEngine sut, CommandMessage<FirstTestCommand> testMessage)
        {
            Assert.That(sut.NotifyFailAsync(testMessage), Is.SameAs(Task.CompletedTask));
        }

        [Test, AutoMoqData]
        public void NotifyFail_returns_completed_task(InMemoryBusEngine sut, EventMessage<FirstTestEvent> testMessage)
        {
            Assert.That(sut.NotifyFailAsync(testMessage), Is.SameAs(Task.CompletedTask));
        }
    }

    public static class ObservableTestExtensions
    {
        public static IReadOnlyList<T> DumpInList<T>(this IObservable<T> sequence)
        {
            var incomingItems = new List<T>();

            sequence.Subscribe(incomingItems.Add);

            return incomingItems;
        }
    }
}
