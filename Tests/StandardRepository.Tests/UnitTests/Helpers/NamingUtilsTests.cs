using NUnit.Framework;
using Shouldly;

using StandardRepository.Helpers;

namespace StandardRepository.Tests.UnitTests.Helpers
{
    [TestFixture]
    public class NamingUtilsTests
    {
        [TestCase("OrganizationId", "organization_id"),
         TestCase("Ip", "ip"),
         TestCase("IP", "ip"),
         TestCase("Name", "name")]
        public void GetDelimitedName_Length2_Delimited(string name, string expected)
        {
            // Arrange, Act
            var result = name.GetDelimitedName();

            // Assert
            result.ShouldBe(expected);
        }
    }
}