using System.Configuration;
using System.Reflection;

using StandardRepository.Helpers;
using StandardRepository.Models;
using StandardRepository.PostgreSQL.DbGenerator;
using StandardRepository.PostgreSQL.Factories;
using StandardRepository.PostgreSQL.Helpers;
using StandardRepository.PostgreSQL.Helpers.SqlExecutor;

namespace ExampleProject
{
    public class DbGenerator
    {
        public (PostgreSQLTypeLookup, EntityUtils, ConnectionSettings, PostgreSQLExecutor) Generate()
        {
            var connectionSettings = new ConnectionSettings();
            connectionSettings.DbName = ConfigurationManager.AppSettings["DbName"];
            connectionSettings.DbHost = ConfigurationManager.AppSettings["DbHost"];
            connectionSettings.DbUser = ConfigurationManager.AppSettings["DbUser"];
            connectionSettings.DbPassword = ConfigurationManager.AppSettings["DbPass"];
            connectionSettings.DbPort = ConfigurationManager.AppSettings["DbPort"];

            var typeLookup = new PostgreSQLTypeLookup();
            var entityUtils = new EntityUtils(typeLookup, Assembly.GetExecutingAssembly());

            var masterConnectionString = PostgreSQLConnectionFactory.GetConnectionString(connectionSettings.DbHost, connectionSettings.DbNameMaster, connectionSettings.DbUser, connectionSettings.DbPassword, connectionSettings.DbPort);
            var masterConnectionFactory = new PostgreSQLConnectionFactory(masterConnectionString);
            var sqlExecutorMaster = new PostgreSQLExecutor(masterConnectionFactory, entityUtils);

            var connectionString = PostgreSQLConnectionFactory.GetConnectionString(connectionSettings);
            var connectionFactory = new PostgreSQLConnectionFactory(connectionString);
            var sqlExecutor = new PostgreSQLExecutor(connectionFactory, entityUtils);
            
            var dbGenerator = new PostgreSQLDbGenerator(typeLookup, entityUtils, sqlExecutorMaster, sqlExecutor);
            if (!dbGenerator.IsDbExistsDb(connectionSettings.DbName))
            {
                dbGenerator.CreateDb(connectionSettings.DbName);
                dbGenerator.Generate().Wait();
            }

            return (typeLookup, entityUtils, connectionSettings, sqlExecutor);
        }
    }
}