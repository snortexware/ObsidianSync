using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace G.Sync.Entities.Interfaces
{
    public interface ISecurityRepository
    {
        void CreateOrUpdateToken(string token, string key);
        SecurityEntity? GetTokenByKey(string key);
        void DeleteTokenByKey(string key);
        void DeleteAllTokens();
        void CreateTokenTableIfNotExists();
        void SaveToken(SecurityEntity entity);
    }
}
