using EventFlow.Domain.Aggregates;
using EventFlow.Domain.Events;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EventFlow.Infrastructure.Persistence
{
    public class SqlTaskRepository : ITaskRepository
    {
        private readonly EventFlowDbContext _dbContext;
        public SqlTaskRepository(EventFlowDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<TaskAggregate> GetAsync(Guid taskId)
        {
            var storedEvents = await _dbContext.StoredEvents
                .Where(e => e.AggregateId == taskId)
                .OrderBy(e => e.CreatedAt)
                .ToListAsync();

            if (!storedEvents.Any())
            {
                return null; // Or throw an exception if preferred
            }

            var domainEvents = new List<object>();

            foreach (var storedEvent in storedEvents)
            {
                // Determine the event type from the stored EventType
                Type eventType = storedEvent.EventType switch
                {
                    "TaskCreatedEvent" => typeof(TaskCreatedEvent),
                    "TaskUpdatedEvent" => typeof(TaskUpdatedEvent),
                    "TaskCompletedEvent" => typeof(TaskCompletedEvent),
                    _ => null
                };

                if (eventType == null)
                {
                    // Optionally handle unknown event types
                    continue;
                }

                // Deserialize the stored JSON into the domain event object
                var domainEvent = JsonSerializer.Deserialize(storedEvent.Data, eventType);
                if (domainEvent != null)
                {
                    domainEvents.Add(domainEvent);
                }
            }

            // Use the rehydration method on TaskAggregate to rebuild state from events
            var aggregate = TaskAggregate.Rehydrate(domainEvents);
            return aggregate;
        }

        public async Task SaveAsync(TaskAggregate taskAggregate)
        {
            foreach (var domainEvent in taskAggregate.DomainEvents)
            {
                var storedEvent = new StoredEvent
                {
                    AggregateId = taskAggregate.TaskId,
                    EventType = domainEvent.GetType().Name,
                    Data = JsonSerializer.Serialize(domainEvent),
                    CreatedAt = DateTime.UtcNow
                };

                _dbContext.StoredEvents.Add(storedEvent);
            }

            await _dbContext.SaveChangesAsync();
        }
    }
}
