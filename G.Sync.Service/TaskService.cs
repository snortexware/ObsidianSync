using G.Sync.Entities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace G.Sync.Service
{
    public class TaskService(ITaskRepository versionRepository)
    {
        private readonly ITaskRepository _versionRepository = versionRepository;



    }
}
