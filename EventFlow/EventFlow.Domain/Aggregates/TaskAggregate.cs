using EventFlow.Domain.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventFlow.Domain.Aggregates
{
    public class TaskAggregate
    {
        public Guid TaskId { get; private set; }
        public string Title { get; private set; }
        public string? Description { get; private set; }
        public bool IsCompleted { get; private set; }
        public DateTime CreatedAt { get; private set; }


        private readonly List<object> _domainEvents = new();
        public IReadOnlyCollection<object> DomainEvents => _domainEvents.AsReadOnly();

        private TaskAggregate() { }

        public TaskAggregate(Guid taskId, string title, string? description)
        {
            TaskId = taskId;
            Title = title;
            Description = description;
            CreatedAt = DateTime.UtcNow;
            IsCompleted = false;

            _domainEvents.Add(new TaskCreatedEvent(taskId, title, CreatedAt));
        }
    }
}
