using EventFlow.Domain.Aggregates;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventFlow.Infrastructure.Persistence
{
    public class InMemoryTaskRepository : ITaskRepository
    {
        private readonly ConcurrentDictionary<Guid, TaskAggregate> _storage = new();

        public Task SaveAsync(TaskAggregate taskAggregate)
        {
            _storage[taskAggregate.TaskId] = taskAggregate;
            return Task.CompletedTask;
        }

        public Task<TaskAggregate> GetAsync(Guid taskId)
        {
            _storage.TryGetValue(taskId, out var taskAggregate);
            return Task.FromResult(taskAggregate);
        }
    }
}
