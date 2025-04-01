using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventFlow.Domain
{
    public class TaskSnapshot
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid AggregateId { get; set; }
        public string SnapshotData { get; set; }//json data of a task/event
        public int LastEventSequence { get; set; }// The version (or sequence number) of the last applied event in the snapshot
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
