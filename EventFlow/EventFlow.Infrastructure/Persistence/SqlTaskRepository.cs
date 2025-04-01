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

        public async Task<TaskAggregate> GetAsyncWithSnapshot(Guid taskId)
        {
            var snapshot = await _dbContext.TaskSnapshots
                .Where(s => s.AggregateId == taskId)
                .OrderByDescending(s => s.CreatedAt)
                .FirstOrDefaultAsync();

            List<StoredEvent> events;
            if (snapshot != null)
            {
                // Get events that occurred after the snapshot's last event sequence
                events = await _dbContext.StoredEvents
                    .Where(e => e.AggregateId == taskId && e.CreatedAt > snapshot.CreatedAt)
                    .OrderBy(e => e.CreatedAt)
                    .ToListAsync();

                // Start with the state from the snapshot
                var aggregate = TaskAggregate.RehydrateFromSnapshot(snapshot);
                // Replay only events after the snapshot
                var domainEvents = new List<object>();
                foreach (var storedEvent in events)
                {
                    // Use your existing logic to deserialize events
                    Type eventType = storedEvent.EventType switch
                    {
                        "TaskCreatedEvent" => typeof(TaskCreatedEvent),
                        "TaskUpdatedEvent" => typeof(TaskUpdatedEvent),
                        "TaskCompletedEvent" => typeof(TaskCompletedEvent),
                        _ => null
                    };

                    if (eventType != null)
                    {
                        var domainEvent = JsonSerializer.Deserialize(storedEvent.Data, eventType);
                        if (domainEvent != null)
                        {
                            domainEvents.Add(domainEvent);
                        }
                    }
                }
                aggregate = TaskAggregate.Rehydrate(domainEvents);
               
                return aggregate;
            }
            else
            {
                // No snapshot exists: load all events as before
                events = await _dbContext.StoredEvents
                    .Where(e => e.AggregateId == taskId)
                    .OrderBy(e => e.CreatedAt)
                    .ToListAsync();

                if (!events.Any()) return null;

                var domainEvents = new List<object>();
                foreach (var storedEvent in events)
                {
                    Type eventType = storedEvent.EventType switch
                    {
                        "TaskCreatedEvent" => typeof(TaskCreatedEvent),
                        "TaskUpdatedEvent" => typeof(TaskUpdatedEvent),
                        "TaskCompletedEvent" => typeof(TaskCompletedEvent),
                        _ => null
                    };

                    if (eventType != null)
                    {
                        var domainEvent = JsonSerializer.Deserialize(storedEvent.Data, eventType);
                        if (domainEvent != null)
                        {
                            domainEvents.Add(domainEvent);
                        }
                    }
                }
                return TaskAggregate.Rehydrate(domainEvents);
            }
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

            // Define a snapshot threshold (for example, every 5 events).
            int snapshotThreshold = 5; // Adjust this value as needed.

            // If the aggregate's version is a multiple of the threshold, create a snapshot.
            if (taskAggregate.Version % snapshotThreshold == 0)
            {
                var snapshot = taskAggregate.CreateSnapshot();
                _dbContext.TaskSnapshots.Add(snapshot);
                await _dbContext.SaveChangesAsync();
            }

            // Optionally clear pending domain events so they aren't processed again.
            //taskAggregate.ClearDomainEvents();
        }
    }
}
