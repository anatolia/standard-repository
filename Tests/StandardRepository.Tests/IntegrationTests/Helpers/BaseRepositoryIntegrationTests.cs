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
        protected long CURRENT_USER_ID => 123;

        protected string GetTestDBName(string postfix = "")
        {
            var testMethodName = TestContext.CurrentContext.Test.MethodName;
            return $"integration_test_db_{testMethodName.ToLowerInvariant()}_{postfix}";
        }

        protected EntityUtils GetEntityUtils(TypeLookup typeLookup, Assembly assemblyOfEntities)
        {
            return new EntityUtils(typeLookup, assemblyOfEntities);
        }

        protected ConnectionSettings GetConnectionSettings(string dbName = null)
        {
            var connectionSettings = new ConnectionSettings();
            connectionSettings.DbHost = "localhost";
            connectionSettings.DbUser = "local_user";
            connectionSettings.DbPassword = "local_user+-2019*";
            connectionSettings.DbPort = "5432";

            if (dbName == null)
            {
                connectionSettings.DbName = "integration_test_db";
            }
            else
            {
                connectionSettings.DbName = dbName;
            }

            return connectionSettings;
        }

        protected void Sleep()
        {
            Thread.Sleep(1234);
        }
    }
}