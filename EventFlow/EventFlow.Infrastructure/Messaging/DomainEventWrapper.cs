using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EventFlow.Infrastructure.Messaging
{
    public  class DomainEventWrapper
    {
        public string EventType { get; set; }
        public JsonElement Data { get; set; }
    }
}
