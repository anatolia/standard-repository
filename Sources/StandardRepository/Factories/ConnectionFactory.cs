using System.Data.Common;

using StandardRepository.Models;

namespace StandardRepository.Factories
{
    public class ConnectionFactory<TConnection> : IConnectionFactory<TConnection> where TConnection : DbConnection, new()
    {
        private readonly string _connectionString;

        public ConnectionFactory(ConnectionSettings connectionSettings)
        {
            _connectionString = GetConnectionString(connectionSettings.DbHost, connectionSettings.DbName, connectionSettings.DbUser, connectionSettings.DbPassword);
        }

        public ConnectionFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public static string GetConnectionString(string dbHost, string dbName, string dbUser, string dbPassword)
        {
            return $"Server={dbHost};Database={dbName};User Id={dbUser};Password={dbPassword};";
        }

        public static string GetConnectionString(ConnectionSettings connectionSettings)
        {
            return GetConnectionString(connectionSettings.DbHost, connectionSettings.DbName, connectionSettings.DbUser, connectionSettings.DbPassword);
        }

        public TConnection Create()
        {
            var connection = new TConnection
            {
                ConnectionString = _connectionString
            };
            connection.Open();
            return connection;
        }
    }
}