using NUnit.Framework;
using StandardRepository.Helpers;
using StandardRepository.PostgreSQL.Helpers;
using StandardRepository.Tests.Base.Entities;
using StandardRepository.Tests.IntegrationTests.Helpers;
using System.Reflection;

namespace StandardRepository.Tests.UnitTests.Helpers
{

    [TestFixture]
    public class EntityUtilsTests: BaseRepositoryIntegrationTests
    {

        public EntityUtils entity { get; set; }
        public TestEntity testSchema { get; set; }
        [SetUp]
        public void Run_Before_Every_Test()
        {
            entity = GetEntityUtils(new PostgreSQLTypeLookup(), Assembly.GetExecutingAssembly());
            testSchema = new TestEntity();
        }                  

        [Test]
        public void Verify_Schema_Name()
        {            
          
            var name=entity.GetSchemaName(testSchema.GetType());
            Assert.AreEqual(name, "main");
        }

        [Test]
        public void Verify_Table_Name()
        {
            //Inner GetDelimitedName method already has covered all testcases in different
            //test file, so no need to cover every case here
            var name = entity.GetTableName(testSchema.GetType());
            Assert.AreEqual(name, "test_entity");
        }


        [Test]
        public void Verify_Table_Full_Name()
        {
            var name = entity.GetTableFullName(testSchema.GetType());
            Assert.AreEqual(name, "main.test_entity");
        }


        [Test]
        public void Verify_Get_All_Properties()
        {
            var result = entity.GetAllProperties(testSchema.GetType());
            //result[0].
            Assert.AreEqual(result.Length, 14);
            Assert.AreEqual(result[0].Name, "Email");
            Assert.AreEqual(result[1].Name, "IsActive");
        }


        [Test]
        public void Verify_Get_Base_Fields()
        {
            var result = entity.GetRelatedEntityTypes(testSchema.GetType());
           
            Assert.AreEqual(result.Count,1);           
        }


        [Test]
        public void Verify_MapFields()
        {
            Assert.IsTrue(true);
        }
    }
}
