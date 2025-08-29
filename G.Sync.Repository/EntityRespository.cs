using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ComponentModel.DataAnnotations.Schema;
using Dapper;
using G.Sync.DatabaseManagment;

namespace G.Sync.Repository
{
    public class EntityRepository<T> where T : class, new()
    {
        private readonly T _entity;

        public EntityRepository()
        {
            _entity = new T();
        }

        public void Create()
        {
            foreach (var prop in typeof(T)
                         .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                         .Where(p => p.GetCustomAttribute<ColumnAttribute>() != null))
            {
                if (prop.PropertyType == typeof(int))
                    prop.SetValue(_entity, 1);
                else if (prop.PropertyType == typeof(string))
                    prop.SetValue(_entity, string.Empty);
            }
        }

        public T Entity => _entity;

        public void Save()
        {
            Save(null);
        }

        public void Save(T? entity)
        {
            var type = typeof(T);
            var tableAttr = type.GetCustomAttribute<TableAttribute>();
            var table = tableAttr?.Name ?? type.Name;

            var props = type
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.GetCustomAttribute<ColumnAttribute>() != null)
                .ToList();

            var columns = string.Join(", ", props.Select(p => p.GetCustomAttribute<ColumnAttribute>()!.Name));
            var parameters = string.Join(", ", props.Select(p => "@" + p.Name));

            var sql = $"INSERT INTO {table} ({columns}) VALUES ({parameters});";

            using var conn = DataBaseContext.Instance.GetConnection();
            conn.Execute(sql, entity ?? _entity);
        }

        // Static helpers for querying
        public  T? Get(object id)
        {
            var type = typeof(T);
            var tableAttr = type.GetCustomAttribute<TableAttribute>();
            var table = tableAttr?.Name ?? type.Name;

            var keyProp = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(p => p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase));

            if (keyProp == null)
                throw new Exception("No property 'Id' found");

            var column = keyProp.GetCustomAttribute<ColumnAttribute>()?.Name ?? keyProp.Name;
            var sql = $"SELECT * FROM {table} WHERE {column} = @Id LIMIT 1;";

            using var conn = DataBaseContext.Instance.GetConnection();
            return conn.QueryFirstOrDefault<T>(sql, new { Id = id });
        }

        public T? Get(string sql, DynamicParameters parameters)
        {
            using var conn = DataBaseContext.Instance.GetConnection();
            return conn.QueryFirstOrDefault<T>(sql, parameters);
        }

        public static IEnumerable<T> GetMany()
        {
            var type = typeof(T);
            var tableAttr = type.GetCustomAttribute<TableAttribute>();
            var table = tableAttr?.Name ?? type.Name;

            var sql = $"SELECT * FROM {table};";

            using var conn = DataBaseContext.Instance.GetConnection();
            return conn.Query<T>(sql);
        }
    }
}
