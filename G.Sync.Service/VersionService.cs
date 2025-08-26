using G.Sync.Entities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace G.Sync.Service
{
    public class VersionService(IVersionRepository versionRepository)
    {
        private readonly IVersionRepository _versionRepository = versionRepository;



    }
}
