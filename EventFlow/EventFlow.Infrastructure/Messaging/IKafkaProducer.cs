using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventFlow.Infrastructure.Messaging
{
    public interface IKafkaProducer
    {
        Task PublishEventAsync(object domainEvent);
    }
}
