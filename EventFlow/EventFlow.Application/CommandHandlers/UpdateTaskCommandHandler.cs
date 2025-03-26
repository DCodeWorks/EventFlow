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
    public class UpdateTaskCommandHandler : IRequestHandler<UpdateTaskCommand, bool>
    {
        private readonly ITaskRepository _taskRepository;
        private readonly IKafkaProducer _kafkaProducer;
        public UpdateTaskCommandHandler(ITaskRepository taskRepository, IKafkaProducer kafkaProducer)
        {
            _taskRepository = taskRepository;
            _kafkaProducer = kafkaProducer;
        }
        public async Task<bool> Handle(UpdateTaskCommand command, CancellationToken cancellationToken)
        {
            var taskAggregate = await _taskRepository.GetAsync(command.TaskId);
            if (taskAggregate != null)
            {
                taskAggregate.Update(command.Title, command.Description);

                await _taskRepository.SaveAsync(taskAggregate);

                foreach (var domainEvent in taskAggregate.DomainEvents)
                {
                    await _kafkaProducer.PublishEventAsync(domainEvent);
                }
                return true;
            }
            else return false;

        }
    }
}
