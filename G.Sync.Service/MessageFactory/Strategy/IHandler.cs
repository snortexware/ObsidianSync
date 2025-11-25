using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace G.Sync.Service.MessageFactory.Strategy
{
    public interface IHandler
    {
        public void HandleMessage(long taskId);
    }
}
