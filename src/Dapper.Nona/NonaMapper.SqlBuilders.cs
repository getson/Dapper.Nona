using System;
using System.Collections.Generic;
using System.Data;
using Dapper.Nona.Abstractions;
using Dapper.Nona.Internal.QueryBuilders;

namespace Dapper.Nona
{
    public static partial class NonaMapper
    {
        private static readonly Dictionary<string, ISqlBuilder> SqlBuilders = new Dictionary<string, ISqlBuilder>
        {
            ["sqlconnection"] = new SqlServerSqlBuilder(),
            ["sqlceconnection"] = new SqlServerCeSqlBuilder(),
            ["sqliteconnection"] = new SqliteSqlBuilder(),
            ["npgsqlconnection"] = new PostgresSqlBuilder(),
            ["mysqlconnection"] = new MySqlSqlBuilder()
        };

        /// <summary>
        /// Adds a custom implementation of <see cref="ISqlBuilder"/>
        /// for the specified ADO.NET connection type.
        /// </summary>
        /// <param name="connectionType">
        /// The ADO.NET conncetion type to use the <paramref name="builder"/> with.
        /// Example: <c>typeof(SqlConnection)</c>.
        /// </param>
        /// <param name="builder">An implementation of the <see cref="ISqlBuilder"/> interface.</param>
        public static void AddSqlBuilder(Type connectionType, ISqlBuilder builder)
        {
            SqlBuilders[connectionType.Name.ToLower()] = builder;
        }

        /// <summary>
        /// Gets the configured <see cref="ISqlBuilder"/> for the specified <see cref="IDbConnection"/> instance.
        /// </summary>
        /// <param name="connection">The database connection instance.</param>
        /// <returns>The <see cref="ISqlBuilder"/> interface for the specified <see cref="IDbConnection"/> instance.</returns>
        public static ISqlBuilder GetSqlBuilder(IDbConnection connection)
        {
            var connectionTypeName = connection.GetType().Name;
            var builder =SqlBuilders.TryGetValue(connectionTypeName.ToLower(), out var b) ? b : new SqlServerSqlBuilder();
            LogReceived?.Invoke($"Selected SQL Builder '{builder.GetType().Name}' for connection type '{connectionTypeName}'");
            return builder;
        }
    }
}
