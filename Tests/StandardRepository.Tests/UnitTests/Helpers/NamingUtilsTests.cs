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
         TestCase("XAxisTitle", "x_axis_title"),
         TestCase("TheTitle", "the_title"),
         TestCase("IP", "ip"),
         TestCase("SSL", "s_s_l"),
         TestCase("Ssl", "ssl"),
         TestCase("IPLog", "i_p_log"),
         TestCase("IpLog", "ip_log"),
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