using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace EventFlow.Domain.Commands
{
    public record CreateTaskCommand(Guid TaskId, string Title, string? Description) : IRequest<bool>;

    public record UpdateTaskCommand(Guid TaskId, string Title, string? Description) : IRequest<bool>;

    public record CompleteTaskCommand(Guid TaskId) : IRequest<bool>;
}
