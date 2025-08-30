using G.Sync.Common;
using G.Sync.Entities;
using G.Sync.Entities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace G.Sync.Repository
{
    public class SecurityRepository : EntityRepository<SecurityEntity>, ISecurityRepository
    {
        public void CreateOrUpdateToken(string token, string key)
        {
            const string sqlCommand = @"INSERT INTO SECURITY (KEY, TOKEN,  CREATEDAT)
                                        VALUES (@key, @token, @created)
                                        ON CONFLICT(KEY) DO UPDATE SET
                                        TOKEN = excluded.TOKEN,
                                        CREATEDAT = excluded.CREATEDAT";

            var rawCommand = new RawEntityCommand(sqlCommand);

            rawCommand.Parameters.Add("key", key);
            rawCommand.Parameters.Add("token", token);
            rawCommand.Parameters.Add("created", DateTime.UtcNow.ToString());

            Execute(rawCommand);
        }

        public void CreateTokenTableIfNotExists()
        {
            throw new NotImplementedException();
        }

        public void DeleteAllTokens() => Delete(new Criteria());

        public void DeleteTokenByKey(string key) => Delete(CreateKeyCriteria(key));

        public SecurityEntity? GetTokenByKey(string key) => Get(CreateKeyCriteria(key));

        public void SaveToken(SecurityEntity entity) => Save(entity);

        private Criteria CreateKeyCriteria(string key)
        {
            var criteria = new Criteria("KEY = @key");
            criteria.Parameters.Add("key", key);

            return criteria;
        }
    }
}
