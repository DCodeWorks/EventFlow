using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventFlow.Domain.Events
{
    public record TaskCreatedEvent(Guid TaskId, string Title, DateTime CreatedAt);
    public record TaskUpdatedEvent(Guid TaskId, string Title, string? Description, DateTime UpdatedAt);
    public record TaskCompletedEvent(Guid TaskId, DateTime CompletedAt);
}
