using Confluent.Kafka;
using EventFlow.Domain.Events;
using EventFlow.Infrastructure.Persistence;
using EventFlow.Infrastructure.ReadModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EventFlow.Infrastructure.Messaging
{
    public class TaskReadModelConsumer : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _configuration;
        public TaskReadModelConsumer(IServiceScopeFactory scopeFactory, IConfiguration configuration)
        {
            _scopeFactory = scopeFactory;
            _configuration = configuration;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Run(() =>
            {
                var consumerConfig = new ConsumerConfig
                {
                    BootstrapServers = _configuration["Kafka:BootstrapServers"],
                    GroupId = "readmodel-consumer-group",
                    AutoOffsetReset = AutoOffsetReset.Earliest
                };

                using var consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
                consumer.Subscribe(_configuration["Kafka:Topic"]);

                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var result = consumer.Consume(TimeSpan.FromMilliseconds(100));
                        if (result != null)
                        {
                            Console.WriteLine("Processing message from TaskReadModelConsumer...");
                            ProcessEventAsync(result.Message.Value).GetAwaiter().GetResult();
                            Console.WriteLine("Processed!");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error consuming message: {ex.Message}");
                    }
                }

                consumer.Close();
            }, stoppingToken);
        }


        private async Task ProcessEventAsync(string message)
        {
            DomainEventWrapper wrapper;
            try
            {
                wrapper = JsonSerializer.Deserialize<DomainEventWrapper>(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deserializing wrapper: {ex.Message}");
                return;
            }

            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<EventFlowDbContext>();

            switch (wrapper.EventType)
            {
                case "TaskCreatedEvent":
                    var createdEvent = wrapper.Data.Deserialize<TaskCreatedEvent>();
                    if (createdEvent != null && createdEvent.TaskId != Guid.Empty)
                    {
                        // Insert new record
                        dbContext.TaskReadModels.Add(new TaskReadModel
                        {
                            TaskId = createdEvent.TaskId,
                            Title = createdEvent.Title,
                            Description = "",
                            IsCompleted = false,
                            CreatedAt = createdEvent.CreatedAt
                        });
                        await dbContext.SaveChangesAsync();
                    }
                    break;
                case "TaskUpdatedEvent":
                    var updatedEvent = wrapper.Data.Deserialize<TaskUpdatedEvent>();
                    if (updatedEvent != null && updatedEvent.TaskId != Guid.Empty)
                    {
                        var task = await dbContext.TaskReadModels.FirstOrDefaultAsync(t => t.TaskId == updatedEvent.TaskId);
                        if (task != null)
                        {
                            task.Title = updatedEvent.Title;
                            task.Description = updatedEvent.Description;
                            task.UpdatedAt = updatedEvent.UpdatedAt;
                            await dbContext.SaveChangesAsync();
                        }
                    }
                    break;
                case "TaskCompletedEvent":
                    var completedEvent = wrapper.Data.Deserialize<TaskCompletedEvent>();
                    if (completedEvent != null && completedEvent.TaskId != Guid.Empty)
                    {
                        var task = await dbContext.TaskReadModels.FirstOrDefaultAsync(t => t.TaskId == completedEvent.TaskId);
                        if (task != null)
                        {
                            task.IsCompleted = true;
                            task.UpdatedAt = completedEvent.CompletedAt;
                            await dbContext.SaveChangesAsync();
                        }
                    }
                    break;
                default:
                    Console.WriteLine($"Unknown event type: {wrapper.EventType}");
                    break;
            }
        }
    }
}
