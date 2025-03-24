using EventFlow.Domain.Aggregates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventFlow.Infrastructure.Persistence
{
    public interface ITaskRepository
    {
        Task SaveAsync(TaskAggregate taskAggregate);
        Task<TaskAggregate> GetAsync(Guid taskId);
    }
}
