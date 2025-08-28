using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace G.Sync.Entities.Interfaces
{
    public interface IRepository<T> where T : class
    {
        T Create();
        void Save();
        T? Get(object id);
        IEnumerable<T> GetMany();
    }
}
