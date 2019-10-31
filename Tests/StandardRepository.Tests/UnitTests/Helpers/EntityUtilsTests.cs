using System;
using System.Data;
using System.Reflection;

using Moq;
using NUnit.Framework;

using StandardRepository.Helpers;
using StandardRepository.Models.Entities;
using StandardRepository.PostgreSQL.Helpers;
using StandardRepository.Tests.Base.Entities;
using StandardRepository.Tests.IntegrationTests.Helpers;

namespace StandardRepository.Tests.UnitTests.Helpers
{
    [TestFixture]
    public class EntityUtilsTests : BaseRepositoryIntegrationTests
    {
        public EntityUtils SystemUnderTest { get; set; }

        [SetUp]
        public void Run_Before_Every_Test()
        {
            SystemUnderTest = GetEntityUtils(new PostgreSQLTypeLookup(), Assembly.GetExecutingAssembly());
        }

        [Test]
        public void Verify_Schema_Name()
        {
            var organization = new Organization();

            var name = SystemUnderTest.GetSchemaName(organization.GetType());

            Assert.AreEqual(name, "main");
        }

        [Test]
        public void Verify_Table_Name()
        {
            var organization = new Organization();

            var name = SystemUnderTest.GetTableName(organization.GetType());

            Assert.AreEqual(name, "organization");
        }

        [Test]
        public void Verify_Table_Full_Name()
        {
            var organization = new Organization();

            var name = SystemUnderTest.GetTableFullName(organization.GetType());

            Assert.AreEqual(name, "main.organization");
        }

        [Test]
        public void Verify_Get_All_Properties()
        {
            var organization = new Organization();
            var result = SystemUnderTest.GetAllProperties(organization.GetType());           
            
            Assert.AreEqual(result.Length, 18);
            
            Assert.AreEqual(result[0].Name, nameof(Organization.Email));
            Assert.AreEqual(result[1].Name, nameof(Organization.Description));
            Assert.AreEqual(result[2].Name, nameof(Organization.IsActive));
            Assert.AreEqual(result[3].Name, nameof(Organization.IsSuperOrganization));
            Assert.AreEqual(result[4].Name, nameof(Organization.ProjectCount));
            Assert.AreEqual(result[5].Name, nameof(Organization.StartDate));
            Assert.AreEqual(result[6].Name, nameof(Organization.LongField));
            Assert.AreEqual(result[7].Name, nameof(Organization.XAxisTitle));
            Assert.AreEqual(result[8].Name, nameof(Organization.Id));
            Assert.AreEqual(result[9].Name, nameof(Organization.Uid));
            Assert.AreEqual(result[10].Name, nameof(Organization.Name));
            Assert.AreEqual(result[11].Name, nameof(Organization.CreatedBy));
            Assert.AreEqual(result[12].Name, nameof(Organization.CreatedAt));
            Assert.AreEqual(result[13].Name, nameof(Organization.UpdatedBy));
            Assert.AreEqual(result[14].Name, nameof(Organization.UpdatedAt));
            Assert.AreEqual(result[15].Name, nameof(Organization.DeletedBy));
            Assert.AreEqual(result[16].Name, nameof(Organization.DeletedAt));
            Assert.AreEqual(result[17].Name, nameof(Organization.IsDeleted));
        }
        
        [Test]
        public void Verify_Get_Properties()
        {
            var organization = new Organization();
            var result = SystemUnderTest.GetProperties(organization.GetType());           
            
            Assert.AreEqual(result.Length, 8);
            
            Assert.AreEqual(result[0].Name, nameof(Organization.Email));
            Assert.AreEqual(result[1].Name, nameof(Organization.Description));
            Assert.AreEqual(result[2].Name, nameof(Organization.IsActive));
            Assert.AreEqual(result[3].Name, nameof(Organization.IsSuperOrganization));
            Assert.AreEqual(result[4].Name, nameof(Organization.ProjectCount));
            Assert.AreEqual(result[5].Name, nameof(Organization.StartDate));
            Assert.AreEqual(result[6].Name, nameof(Organization.LongField));
            Assert.AreEqual(result[7].Name, nameof(Organization.XAxisTitle));
        }

        [Test]
        public void Verify_Get_Base_Fields()
        {
            var project = new Project();
            var result1 = SystemUnderTest.GetRelatedEntityTypes(project.GetType());

            var organization = new Organization();
            var result2 = SystemUnderTest.GetRelatedEntityTypes(organization.GetType());

            Assert.AreEqual(result1.Count, 0);
            Assert.AreEqual(result2.Count, 1);
        }

        [Test]
        public void Verify_MapFields()
        {
            var organization = new Organization();
            var mockDataReader = GetMockDataReaderForMapFieldsRevisions();
            var allProperties = SystemUnderTest.GetAllProperties(organization.GetType());
            var entityTypeName = SystemUnderTest.GetTableName(organization.GetType());

            SystemUnderTest.MapFields(mockDataReader.Object, allProperties, entityTypeName, organization);

            Assert.AreEqual(organization.Email, "test@gmail.com");
            Assert.AreEqual(organization.Description, "this is test case");
            Assert.IsTrue(organization.IsActive);
        }

        [Test]
        public void Verify_MapFields_Revision()
        {
            var organization = new Organization();
            var revisionAt = DateTime.UtcNow;
            var revision = new EntityRevision<Organization>();
            var mockDataReader = GetMockDataReaderForMapFieldsRevisions(true, revisionAt);
            var allProperties = SystemUnderTest.GetAllProperties(organization.GetType());
            var entityTypeName = SystemUnderTest.GetTableName(organization.GetType());

            SystemUnderTest.MapFieldsRevision(mockDataReader.Object, allProperties, entityTypeName, revision);

            Assert.AreEqual(revision.Entity.Email, "test@gmail.com");
            Assert.AreEqual(revision.Entity.Description, "this is test case");
            Assert.IsTrue(revision.Entity.IsActive);
            Assert.AreEqual(revision.Id, 1);
            Assert.AreEqual(revision.Revision, 2);
            Assert.AreEqual(revision.RevisionedAt, revisionAt);
        }

        private Mock<IDataReader> GetMockDataReaderForMapFieldsRevisions(bool isRevisionTest = false, DateTime? revisionAt = null)
        {
            var dataRecord = new Mock<IDataReader>();

            dataRecord.Setup(reader => reader.FieldCount).Returns(3);
            dataRecord.Setup(reader => reader[0]).Returns("test@gmail.com");
            dataRecord.Setup(reader => reader.GetName(0)).Returns("Email");
            dataRecord.Setup(reader => reader[1]).Returns("this is test case");
            dataRecord.Setup(reader => reader.GetName(1)).Returns("Description");
            dataRecord.Setup(reader => reader[2]).Returns(true);
            dataRecord.Setup(reader => reader.GetName(2)).Returns("IsActive");

            if (isRevisionTest)
            {
                dataRecord.Setup(reader => reader["organization_id"]).Returns(1);
                dataRecord.Setup(reader => reader["revision"]).Returns(2);
                dataRecord.Setup(reader => reader["revisioned_by"]).Returns(10000);
                dataRecord.Setup(reader => reader["revisioned_at"]).Returns(revisionAt.Value);
            }
            return dataRecord;
        }
    }
}
