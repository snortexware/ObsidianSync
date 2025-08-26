using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace G.Sync.Entities.Interfaces
{
    public interface ITransaction : IDisposable
    {
        void Complete();
        void Rollback();
    }
}
