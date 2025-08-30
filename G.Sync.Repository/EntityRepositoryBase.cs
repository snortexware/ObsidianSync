using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ComponentModel.DataAnnotations.Schema;
using Dapper;
using G.Sync.DatabaseManagment;
using G.Sync.Common;

namespace G.Sync.Repository
{
    public abstract class EntityRepositoryBase<T> where T : class, new()
    {
        protected void CreateInternal(T entity)
        {
            foreach (var prop in typeof(T)
                         .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                         .Where(p => p.GetCustomAttribute<ColumnAttribute>() != null))
            {
                if (prop.PropertyType == typeof(int))
                    prop.SetValue(entity, 1);
                else if (prop.PropertyType == typeof(string))
                    prop.SetValue(entity, string.Empty);
            }
        }

        protected void SaveInternal(T? entity, T defaultEntity)
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
            using var conn = DataBaseContext.Instance.GetConnection();

            var registerExist = conn.QueryFirstOrDefault<int>(
                $"SELECT COUNT(1) FROM {table} WHERE Id = @Id;",
                new { Id = props.FirstOrDefault(p => p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase))?.GetValue(entity ?? defaultEntity) });

            if(registerExist <= 0)
            {
                var sql = $"INSERT INTO {table} ({columns}) VALUES ({parameters});";
                conn.Execute(sql, entity ?? defaultEntity);
            }
            else
            {
                var setClause = string.Join(", ", props.Where(p => !p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase))
                    .Select(p => $"{p.GetCustomAttribute<ColumnAttribute>()!.Name} = @{p.Name}"));
                var sql = $"UPDATE {table} SET {setClause} WHERE Id = @Id;";
                conn.Execute(sql, entity ?? defaultEntity);
            }
        }

        protected T? GetInternal(object id, TransactionContext tc)
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

            var conn = tc.Connection;

            return conn.QueryFirstOrDefault<T>(sql, new { Id = id });
        }

        protected T? GetInternal(string sql, DynamicParameters parameters, TransactionContext tc)
        {
            var conn = tc.Connection;
            return conn.QueryFirstOrDefault<T>(sql, parameters);
        }

        protected void CreateEntityTableInternal(string sqlCommand, TransactionContext tc)
        {
            var conn = tc.Connection;
            conn.Execute(sqlCommand);
        }

        protected IEnumerable<T> GetManyInternal(TransactionContext tc)
        {
            var type = typeof(T);
            var tableAttr = type.GetCustomAttribute<TableAttribute>();
            var table = tableAttr?.Name ?? type.Name;

            var sql = $"SELECT * FROM {table};";

            using var conn = tc.Connection;
            return conn.Query<T>(sql);
        }
    }
}
