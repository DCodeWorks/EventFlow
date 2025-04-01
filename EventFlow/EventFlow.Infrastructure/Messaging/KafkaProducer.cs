using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Polly;
using Polly.Retry;
using System.Text.Json;

namespace EventFlow.Infrastructure.Messaging
{
    public class KafkaProducer : IKafkaProducer
    {
        private readonly IProducer<Null, string> _producer;
        private readonly string _topic;
        private readonly string _dlqTopic;

        private readonly AsyncRetryPolicy _retryPolicy;

        public KafkaProducer(IConfiguration configuration)
        {
            var config = new ProducerConfig
            {
                BootstrapServers = configuration["Kafka:BootstrapServers"]
            };

            _producer = new ProducerBuilder<Null, string>(config).Build();
            _topic = configuration["Kafka:Topic"];
            _dlqTopic = configuration["Kafka:DLQTopic"];

            _retryPolicy = Policy
                .Handle<ProduceException<Null, string>>()
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (exception, timeSpan, retryCount, context) =>
                {
                    Console.WriteLine($"Retry {retryCount} after {timeSpan.TotalSeconds}s due to: {exception.Message}");
                });
        }
        public async Task PublishEventAsync(object domainEvent)
        {
            var wrapper = new DomainEventWrapper
            {
                EventType = domainEvent.GetType().Name,
                Data = JsonSerializer.SerializeToElement(domainEvent)
            };
            // Serialize the event to JSON
            var jsonWrapper = JsonSerializer.Serialize(wrapper);

            var message = new Message<Null, string> { Value = jsonWrapper };

            try
            {
                await _retryPolicy.ExecuteAsync(async () =>
                {
                    await _producer.ProduceAsync(_topic, message);
                });
            }
            catch (ProduceException<Null, string> ex)
            {
                Console.WriteLine($"Publishing to DLQ due to: {ex.Message}");
                await PublishToDLQAsync(message);
            }
        }

        private async Task PublishToDLQAsync(Message<Null, string> message)
        {
            try
            {
                
                await _producer.ProduceAsync(_dlqTopic, message);
                Console.WriteLine("Message published to Dead Letter Queue.");
            }
            catch (ProduceException<Null, string> ex)
            {
                Console.WriteLine($"Failed to publish to DLQ: {ex.Message}");
            }
        }
    }
}
