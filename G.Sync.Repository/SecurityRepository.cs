using G.Sync.Entities;
using G.Sync.Entities.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace G.Sync.Repository
{
    public class SecurityRepository : ISecurityRepository
    {
        public void CreateOrUpdateToken(string key, string token)
        {
            var dbContext = new GSyncContext();
            var existingEntity = dbContext.Securities.AsNoTracking().FirstOrDefault(s => s.Key == key);

            if (existingEntity != null)
            {
                existingEntity.Token = token;
                dbContext.Securities.Update(existingEntity);
            }
            else
            {
                var newEntity = new SecurityEntity
                {
                    Key = key,
                    Token = token,
                    CreatedAt = DateTime.UtcNow,
                };
                dbContext.Securities.Add(newEntity);
            }
            dbContext.SaveChanges();
        }

        public void CreateTokenTableIfNotExists()
        {
            var dbContext = new GSyncContext();
            dbContext.Database.EnsureCreated();
            dbContext.SaveChanges();
        }

        public void DeleteAllTokens()
        {
            var dbContext = new GSyncContext();
            dbContext.Securities.RemoveRange(dbContext.Securities);
            dbContext.SaveChanges();
        }

        public void DeleteTokenByKey(string key)
        {
            var dbContext = new GSyncContext();
            dbContext.Securities.RemoveRange(dbContext.Securities.Where(s => s.Key == key));
            dbContext.SaveChanges();
        }

        public SecurityEntity? GetTokenByKey(string key)
        {
            var dbContext = new GSyncContext();
            return dbContext.Securities.AsNoTracking().FirstOrDefault(s => s.Key == key);
        }

        public void SaveToken(SecurityEntity entity)
        {
            var dbContext = new GSyncContext();
            dbContext.Securities.Add(entity);
            dbContext.SaveChanges();
        }
    }
}
