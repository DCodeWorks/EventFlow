using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventFlow.Infrastructure.Persistence
{
    public class StoredEvent
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid AggregateId { get; set; }
        public string EventType { get; set; }
        public string Data { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
