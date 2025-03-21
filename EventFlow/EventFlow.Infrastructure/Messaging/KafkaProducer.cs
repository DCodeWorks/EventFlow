using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EventFlow.Infrastructure.Messaging
{
    public class KafkaProducer : IKafkaProducer
    {
        private readonly IProducer<Null, string> _producer;
        private readonly string _topic;

        public KafkaProducer(IConfiguration configuration)
        {
            var config = new ProducerConfig
            {
                BootstrapServers = configuration["Kafka:BootstrapServers"]
            };

            _producer = new ProducerBuilder<Null,string>(config).Build();
            _topic = configuration["Kafka:Topic"];
        }
        public async Task PublishEventAsync(object domainEvent)
        {
            // Serialize the event to JSON
            var jsonEvent = JsonSerializer.Serialize(domainEvent);
            await _producer.ProduceAsync(_topic, new Message<Null, string> { Value = jsonEvent });
        }
    }
}
