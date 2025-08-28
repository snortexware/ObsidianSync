using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace G.Sync.Entities.Interfaces
{
    public interface ITaskRepository
    {
        Task Create();
        Task Get(int id);
        Task Get(string where);
        void Save();
    }
}
