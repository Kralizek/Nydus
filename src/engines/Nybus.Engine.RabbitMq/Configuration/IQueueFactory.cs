﻿using System;
using RabbitMQ.Client;
using QueueDeclareOk = RabbitMQ.Client.QueueDeclareOk;

namespace Nybus.Configuration
{
    public interface IQueueFactory
    {
        QueueDeclareOk CreateQueue(IModel model);
    }

    public class StaticQueueFactory : IQueueFactory
    {
        public StaticQueueFactory(string queueName)
        {
            QueueName = queueName ?? throw new ArgumentNullException(nameof(queueName));
        }

        public QueueDeclareOk CreateQueue(IModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            return model.QueueDeclare(QueueName, durable: true, exclusive: false, autoDelete: false);
        }

        public string QueueName { get; }
    }

    public class TemporaryQueueFactory : IQueueFactory
    {
        private TemporaryQueueFactory() { }

        public QueueDeclareOk CreateQueue(IModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            return model.QueueDeclare(durable: true);
        }

        public static readonly IQueueFactory Instance = new TemporaryQueueFactory();
    }

}