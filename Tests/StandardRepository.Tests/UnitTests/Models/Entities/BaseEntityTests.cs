using NUnit.Framework;
using Shouldly;

using StandardRepository.Models.Entities;

namespace StandardRepository.Tests.UnitTests.Models.Entities
{
    [TestFixture]
    public class BaseEntityTests : EntityTestHelper
    {
        [Test]
        public void BaseEntity_Constructor()
        {
            var entity = new BaseEntity();

            entity.CreatedAt.ShouldNotBeNull();
            entity.Uid.ShouldNotBeNull();
            entity.Name.ShouldBe(string.Empty);
        }

        [Test]
        public void BaseEntity_Property()
        {
            var entity = new BaseEntity();

            var entityType = entity.GetType();
            var properties = entityType.GetProperties();

            AssertLongProperty(properties, "Id", entity.Id);
            AssertGuidProperty(properties, "Uid");
            AssertStringProperty(properties, "Name", entity.Name);

            AssertLongProperty(properties, "CreatedBy", entity.CreatedBy);
            AssertInstantProperty(properties, "CreatedAt", entity.CreatedAt);

            AssertLongProperty(properties, "UpdatedBy", entity.UpdatedBy);
            AssertNullableInstantProperty(properties, "UpdatedAt", entity.UpdatedAt);

            AssertLongProperty(properties, "DeletedBy", entity.DeletedBy);
            AssertNullableInstantProperty(properties, "DeletedAt", entity.DeletedAt);
            AssertBooleanProperty(properties, "IsDeleted", entity.IsDeleted);
        }
    }
}