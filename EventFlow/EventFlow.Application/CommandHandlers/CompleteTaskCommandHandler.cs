using EventFlow.Domain.Aggregates;
using EventFlow.Domain.Commands;
using EventFlow.Infrastructure.Messaging;
using EventFlow.Infrastructure.Persistence;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventFlow.Application.CommandHandlers
{
    public class CompleteTaskCommandHandler : IRequestHandler<CompleteTaskCommand, bool>
    {
        private readonly ITaskRepository _taskRepository;
        private readonly IKafkaProducer _kafkaProducer;
        public CompleteTaskCommandHandler(ITaskRepository taskRepository, IKafkaProducer kafkaProducer)
        {
            _taskRepository = taskRepository;
            _kafkaProducer = kafkaProducer;
        }
        public async Task<bool> Handle(CompleteTaskCommand command, CancellationToken cancellationToken)
        {
            Console.WriteLine("Handle started");
            // Create a new TaskAggregate using the command data
            var taskAggregate = new TaskAggregate(command.TaskId,"","");

            // Persist the aggregate to the event store (repository)
            await _taskRepository.SaveAsync(taskAggregate);
            Console.WriteLine("task agregate saved");

            // Publish all recorded domain events to Kafka
            foreach (var domainEvent in taskAggregate.DomainEvents)
            {
                await _kafkaProducer.PublishEventAsync(domainEvent);
                Console.WriteLine("Event published to Kafka producer");
            }

            return true;
        }
    }
}
