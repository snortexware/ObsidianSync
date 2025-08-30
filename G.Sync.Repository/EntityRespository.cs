using System;
using System.Collections.Generic;
using System.Reflection;
using System.ComponentModel.DataAnnotations.Schema;
using Dapper;
using G.Sync.Common;

namespace G.Sync.Repository
{
    public class EntityRepository<T> : EntityRepositoryBase<T> where T : class, new()
    {
        private readonly T _entity;

        public EntityRepository()
        {
            _entity = new T();
        }

        public void Create()
        {
            CreateInternal(_entity);
        }

        public T Entity => _entity;

        public void Save()
        {
            Save(null);
        }

        public void Save(T? entity)
        {
            using (var tc = new TransactionContext())
            {
                SaveInternal(entity, _entity);
                tc.Complete();
            }
        }

        public void Execute(RawEntityCommand command)
        {
            using (var tc = new TransactionContext())
            {
                ExecuteInternal(command, tc);
                tc.Complete();
            }
        }

        public T? Get(object id)
        {
            using (var tc = new TransactionContext())
            {
                var result = GetInternal(id, tc);
                tc.Complete();
                return result;
            }
        }

        public void Delete(Criteria criteria)
        {
            using (var tc = new TransactionContext())
            {
                DeleteInternal(criteria, tc);
                tc.Complete();
            }
        }

        public T? Get(string sql, DynamicParameters parameters)
        {
            using (var tc = new TransactionContext())
            {
                var result = GetInternal(sql, parameters, tc);
                tc.Complete();
                return result;
            }
        }

        public void CreateEntityTable(string sqlCommand)
        {
            using (var tc = new TransactionContext())
            {
                CreateEntityTableInternal(sqlCommand, tc);
                tc.Complete();
            }
        }

        public IEnumerable<T> GetMany()
        {
            using (var tc = new TransactionContext())
            {
                var results = GetManyInternal(tc);
                tc.Complete();
                return results;
            }
        }
    }
}
