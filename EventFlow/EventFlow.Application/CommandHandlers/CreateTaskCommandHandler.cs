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
    public class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, bool>
    {
        private readonly ITaskRepository _taskRepository;
        private readonly IKafkaProducer _kafkaProducer;
        public CreateTaskCommandHandler(ITaskRepository taskRepository, IKafkaProducer kafkaProducer)
        {
            _taskRepository = taskRepository;
            _kafkaProducer = kafkaProducer;
        }
        public async Task<bool> Handle(CreateTaskCommand command, CancellationToken cancellationToken)
        {
            // Create a new TaskAggregate using the command data
            var taskAggregate = new TaskAggregate(command.TaskId, command.Title, command.Description);

            // Persist the aggregate to the event store (repository)
            await _taskRepository.SaveAsync(taskAggregate);

            // Publish all recorded domain events to Kafka
            foreach (var domainEvent in taskAggregate.DomainEvents)
            {
                await _kafkaProducer.PublishEventAsync(domainEvent);
            }

            return true;
        }
    }
}
