using System.Data.Entity.Migrations.Infrastructure;
using System.Data.Entity.Migrations.Model;

namespace Keylol.Models.Migrations
{
    internal static class Extensions
    {
        /// <summary>
        /// 为指定列添加默认值约束
        /// </summary>
        public static void AddDefaultContraint(this IDbMigration migration, string tableName, string colName,
            string defaultValue, bool suppressTransaction = false)
        {
            migration.AddOperation(
                new SqlOperation($"ALTER TABLE {tableName} ADD DEFAULT {defaultValue} FOR {colName};")
                {
                    SuppressTransaction = suppressTransaction
                });
        }

        /// <summary>
        /// 删除指定列的默认值约束
        /// </summary>
        public static void DeleteDefaultContraint(this IDbMigration migration, string tableName, string colName,
            bool suppressTransaction = false)
        {
            migration.AddOperation(
                new SqlOperation(
                    $@"DECLARE @SQL varchar(1000)
                    SET @SQL='ALTER TABLE {tableName} DROP CONSTRAINT ['+(SELECT name
                    FROM sys.default_constraints
                    WHERE parent_object_id = object_id('{tableName}')
                    AND col_name(parent_object_id, parent_column_id) = '{colName}')+']';
                    PRINT @SQL;
                    EXEC(@SQL);")
                {
                    SuppressTransaction = suppressTransaction
                });
        }

        /// <summary>
        /// 创建一个 No Cache 的 Sequence
        /// </summary>
        public static void CreateSequence(this IDbMigration migration, string sequenceName,
            bool suppressTransaction = false)
        {
            migration.AddOperation(
                new SqlOperation($"CREATE SEQUENCE {sequenceName} AS int START WITH 1 INCREMENT BY 1 NO CACHE")
                {
                    SuppressTransaction = suppressTransaction
                });
        }

        /// <summary>
        /// 删除指定 Sequence
        /// </summary>
        public static void DropSequence(this IDbMigration migration, string sequenceName,
            bool suppressTransaction = false)
        {
            migration.AddOperation(
                new SqlOperation($"DROP SEQUENCE {sequenceName}")
                {
                    SuppressTransaction = suppressTransaction
                });
        }

        /// <summary>
        /// 重命名 Sequence
        /// </summary>
        public static void RenameSequence(this IDbMigration migration, string name, string newName,
            bool suppressTransaction = false)
        {
            migration.AddOperation(new SqlOperation($"exec sp_rename '{name}', '{newName}'")
            {
                SuppressTransaction = suppressTransaction
            });
        }
    }
}