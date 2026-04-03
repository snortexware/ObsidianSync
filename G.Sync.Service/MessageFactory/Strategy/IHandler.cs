using G.Sync.Service.MessageFactory.Strategy.Enumerators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace G.Sync.Service.MessageFactory.Strategy
{
    public interface IHandler
    {
        HandlerType Type { get; }
        Task HandleAsync(object data);
    }
}
