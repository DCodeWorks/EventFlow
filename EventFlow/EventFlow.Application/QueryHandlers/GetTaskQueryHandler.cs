using EventFlow.Domain.Aggregates;
using EventFlow.Domain.Queries;
using EventFlow.Infrastructure.Persistence;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventFlow.Application.QueryHandlers
{
    public class GetTaskQueryHandler : IRequestHandler<GetTaskQuery, TaskAggregate>
    {
        private readonly ITaskRepository _taskRepository;
        public GetTaskQueryHandler(ITaskRepository taskRepository)
        {
            _taskRepository = taskRepository;
        }
        public async Task<TaskAggregate> Handle(GetTaskQuery request, CancellationToken cancellationToken)
        {
            return await _taskRepository.GetAsyncWithSnapshot(request.taskId);
        }
    }
}
