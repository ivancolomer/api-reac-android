using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using MySql.Data.MySqlClient;

namespace REAC_AndroidApi.Utils.Storage
{
    public static class SqlDatabaseManager
    {
        private static string ConnectionString = "";

        public static void Initialize()
        {
            GenerateConnectionString();
        }

        public static SqlDatabaseClient GetClient()
        {
            MySqlConnection Connection = new MySqlConnection(GenerateConnectionString());
            Connection.Open();

            return new SqlDatabaseClient(Connection);
        }

        private static string GenerateConnectionString()
        {
            if (ConnectionString != "")
                return ConnectionString;

            MySqlConnectionStringBuilder ConnectionStringBuilder = new MySqlConnectionStringBuilder();
            ConnectionStringBuilder.Server = DotNetEnv.Env.GetString("DB_HOST");
            ConnectionStringBuilder.Port = (uint)DotNetEnv.Env.GetInt("DB_PORT");
            ConnectionStringBuilder.UserID = DotNetEnv.Env.GetString("DB_USER");
            ConnectionStringBuilder.Password = DotNetEnv.Env.GetString("DB_PASS");
            ConnectionStringBuilder.Database = DotNetEnv.Env.GetString("DB_NAME");
            ConnectionStringBuilder.MinimumPoolSize = (uint)DotNetEnv.Env.GetInt("DB_POOL_MIN");
            ConnectionStringBuilder.MaximumPoolSize = (uint)DotNetEnv.Env.GetInt("DB_POOL_MAX");
            ConnectionStringBuilder.Pooling = true;

            ConnectionString = ConnectionStringBuilder.ToString();
            return ConnectionString;
        }

        
    }
}
