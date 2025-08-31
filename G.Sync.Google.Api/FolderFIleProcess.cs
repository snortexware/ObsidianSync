using G.Sync.DataContracts;
using G.Sync.Entities;
using Google.Apis.Drive.v3;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using static Dapper.SqlMapper;
using static System.Net.Mime.MediaTypeNames;
using File = Google.Apis.Drive.v3.Data.File;

namespace G.Sync.Google.Api
{
    public class FolderFileProcess : ApiFolderFileHelper
    {
        private readonly DriveService _service = ApiContext.Instance.Connection;

        public string EnsureDriveFolder(ApiPathDto dto)
        {
            var parts = dto.RelPath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            string parentId = dto.RootId;

            foreach (var part in parts)
            {
                if (string.IsNullOrWhiteSpace(part))
                    continue;

                parentId = EnsureDriverFolderInternal(_service, new ApiPathDto { PathPart = part, ParentId = parentId});
            }

            return parentId;
        }

        public string GetOrCreateRootFolder(SettingsEntity settings) => GetOrCreateRootFolderInternal(_service, settings);
        
       
    }
}
