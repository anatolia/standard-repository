using System.Linq;

using NUnit.Framework;
using Shouldly;

using StandardRepository.Models.Entities;

namespace StandardRepository.Tests.UnitTests.Models.Entities
{
    [TestFixture]
    public class EntityRevisionTests : EntityTestHelper
    {
        [Test]
        public void EntityRevision()
        {
            var entity = new EntityRevision<BaseEntity>();

            var entityType = entity.GetType();
            var properties = entityType.GetProperties();

            AssertLongProperty(properties, "Id", entity.Id);
            AssertIntegerProperty(properties, "Revision", entity.Revision);
            AssertLongProperty(properties, "RevisionedBy", entity.RevisionedBy);
            AssertInstantProperty(properties, "RevisionedAt", entity.RevisionedAt);

            var propFirstName = properties.First(x => x.Name == "Entity");
            propFirstName.PropertyType.Name.ShouldBe(nameof(BaseEntity));
        }
    }
}