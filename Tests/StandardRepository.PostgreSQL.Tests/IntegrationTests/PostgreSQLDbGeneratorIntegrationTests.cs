using System;

using NUnit.Framework;

using StandardRepository.Helpers;
using StandardRepository.PostgreSQL.DbGenerator;
using StandardRepository.PostgreSQL.Factories;
using StandardRepository.PostgreSQL.Helpers;
using StandardRepository.PostgreSQL.Helpers.SqlExecutor;
using StandardRepository.Tests.Base.Entities;
using StandardRepository.Tests.IntegrationTests.Helpers;

namespace StandardRepository.PostgreSQL.Tests.IntegrationTests
{
    public class PostgreSQLDbGeneratorIntegrationTests : BaseRepositoryIntegrationTests
    {
        [Test]
        public void PostgreSQLDbGenerator_Generate()
        {
            var connectionSettings = GetConnectionSettings();
            connectionSettings.DbName += "_" + Guid.NewGuid().ToString("N");
            
            var typeLookup = new PostgreSQLTypeLookup();
            var entityUtils = new EntityUtils(typeLookup, typeof(Project).Assembly);

            var masterConnectionString = PostgreSQLConnectionFactory.GetConnectionString(connectionSettings.DbHost, connectionSettings.DbNameMaster, connectionSettings.DbUser, 
                                                                                         connectionSettings.DbPassword, connectionSettings.DbPort);
            var masterConnectionFactory = new PostgreSQLConnectionFactory(masterConnectionString);
            var sqlExecutorMaster = new PostgreSQLExecutor(masterConnectionFactory, entityUtils);

            var connectionString = PostgreSQLConnectionFactory.GetConnectionString(connectionSettings);
            var connectionFactory = new PostgreSQLConnectionFactory(connectionString);
            var sqlExecutor = new PostgreSQLExecutor(connectionFactory, entityUtils);

            var dbGenerator = new PostgreSQLDbGenerator(typeLookup, entityUtils, sqlExecutorMaster, sqlExecutor);
            dbGenerator.CreateDb(connectionSettings.DbName);
            dbGenerator.Generate().Wait();
            
            Assert.True(dbGenerator.IsDbExistsDb(connectionSettings.DbName));
            
            sqlExecutorMaster.ExecuteSql($@"SELECT Pg_terminate_backend(pg_stat_activity.pid)
                                            FROM   pg_stat_activity
                                            WHERE  pg_stat_activity.datname = '{connectionSettings.DbName}'
                                                   AND pid <> Pg_backend_pid();

                                            DROP DATABASE {connectionSettings.DbName};").Wait();
        }
    }
}