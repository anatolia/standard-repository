using System.Reflection;
using System.Threading;

using NUnit.Framework;

using StandardRepository.Helpers;
using StandardRepository.Models;

namespace StandardRepository.Tests.IntegrationTests.Helpers
{
    [TestFixture]
    public class BaseRepositoryIntegrationTests
    {
        public long CURRENT_USER_ID => 123;

        public string GetTestDBName(string postfix = "")
        {
            var testMethodName = TestContext.CurrentContext.Test.MethodName;
            return $"test_db_{testMethodName.ToLowerInvariant()}_{postfix}";
        }

        public EntityUtils GetEntityUtils(TypeLookup typeLookup, Assembly assemblyOfEntities)
        {
            return new EntityUtils(typeLookup, assemblyOfEntities);
        }

        public ConnectionSettings GetConnectionSettings(string dbName = null)
        {
            var connectionSettings = new ConnectionSettings();
            connectionSettings.DbHost = "localhost";
            connectionSettings.DbUser = "postgres";
            connectionSettings.DbPassword = "root";
            connectionSettings.DbPort = "5432";

            if (dbName == null)
            {
                connectionSettings.DbName = "test_db";
            }
            else
            {
                connectionSettings.DbName = dbName;
            }

            return connectionSettings;
        }

        public void Sleep()
        {
            Thread.Sleep(1234);
        }
    }
}