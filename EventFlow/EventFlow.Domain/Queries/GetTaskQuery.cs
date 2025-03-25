using EventFlow.Domain.Aggregates;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventFlow.Domain.Queries
{
    public record GetTaskQuery(Guid taskId) : IRequest<TaskAggregate>;
}
