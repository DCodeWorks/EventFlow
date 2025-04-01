using EventFlow.Domain.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
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


        public int Version { get; private set; } = 0;   // Version (number of applied events) for snapshot purposes


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

        public static TaskAggregate Rehydrate(IEnumerable<object> events)
        {
            var aggregate = new TaskAggregate();
            foreach (var domainEvent in events)
            {
                aggregate.Apply(domainEvent);
            }
            return aggregate;
        }

        public static TaskAggregate RehydrateFromSnapshot(TaskSnapshot snapshot)
        {
            var aggregate = new TaskAggregate();
            aggregate.ApplySnapshot(snapshot);
            return aggregate;
        }

        private void RaiseEvent(object domainEvent)
        {
            Apply(domainEvent);
            _domainEvents.Add(domainEvent);
        }

        public TaskSnapshot CreateSnapshot()
        {
            return new TaskSnapshot
            {
                AggregateId = this.TaskId,
                SnapshotData = JsonSerializer.Serialize(this),
                LastEventSequence = this.Version,
                CreatedAt = DateTime.UtcNow
            };
        }

        public void ApplySnapshot(TaskSnapshot snapshot)
        {
            var restored = JsonSerializer.Deserialize<TaskAggregate>(snapshot.SnapshotData);
            if (restored != null)
            {
                this.TaskId = restored.TaskId;
                this.Title = restored.Title;
                this.Description = restored.Description;
                this.IsCompleted = restored.IsCompleted;
                this.CreatedAt = restored.CreatedAt;
                this.Version = snapshot.LastEventSequence;
            }
        }

        // Apply events to update the state of the aggregate
        private void Apply(object domainEvent)
        {
            switch (domainEvent)
            {
                case TaskCreatedEvent created:
                    TaskId = created.TaskId;
                    Title = created.Title;
                    CreatedAt = created.CreatedAt;
                    break;
                case TaskUpdatedEvent updated:
                    Title = updated.Title;
                    Description = updated.Description;
                    break;
                case TaskCompletedEvent completed:
                    IsCompleted = true;
                    break;
            }
            Version++;
        }

        public static TaskAggregate Create(Guid taskId, string title, string? description)
        {
            var aggregate = new TaskAggregate();
            aggregate.RaiseEvent(new TaskCreatedEvent(taskId, title, DateTime.UtcNow));
            return aggregate;
        }

        public void Update(string title, string? description)
        {
            RaiseEvent(new TaskUpdatedEvent(TaskId, title, description, DateTime.UtcNow));
        }

        public void Complete()
        {

            RaiseEvent(new TaskCompletedEvent(TaskId, DateTime.UtcNow));

        }
    }
}
