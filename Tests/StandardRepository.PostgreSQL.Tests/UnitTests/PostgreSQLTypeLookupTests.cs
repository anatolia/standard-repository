using System;

using NUnit.Framework;

using StandardRepository.PostgreSQL.Helpers;

namespace StandardRepository.PostgreSQL.Tests.UnitTests
{
    [TestFixture]
    public class PostgreSQLTypeLookupTests
    {
        [TestCase(typeof(byte))]
        [TestCase(typeof(sbyte))]
        [TestCase(typeof(short))]
        [TestCase(typeof(ushort))]
        [TestCase(typeof(int))] 
        [TestCase(typeof(uint))]
        [TestCase(typeof(long))]
        [TestCase(typeof(ulong))] 
        [TestCase(typeof(float))] 
        [TestCase(typeof(double))]
        [TestCase(typeof(decimal))] 
        [TestCase(typeof(bool))]
        [TestCase(typeof(string))] 
        [TestCase(typeof(object))] 
        [TestCase(typeof(char))] 
        [TestCase(typeof(Guid))] 
        [TestCase(typeof(DateTime))] 
        [TestCase(typeof(DateTimeOffset))] 
        [TestCase(typeof(byte[]))]
        [TestCase(typeof(byte?))] 
        [TestCase(typeof(sbyte?))] 
        [TestCase(typeof(short?))] 
        [TestCase(typeof(ushort?))] 
        [TestCase(typeof(int?))] 
        [TestCase(typeof(uint?))] 
        [TestCase(typeof(long?))] 
        [TestCase(typeof(ulong?))]
        [TestCase(typeof(float?))] 
        [TestCase(typeof(double?))] 
        [TestCase(typeof(decimal?))] 
        [TestCase(typeof(bool?))] 
        [TestCase(typeof(char?))] 
        [TestCase(typeof(Guid?))] 
        [TestCase(typeof(DateTime?))] 
        [TestCase(typeof(DateTimeOffset?))]
        public void PostgreSQLTypeLookup_HasDbType(Type type)
        {
            var typeLookup = new PostgreSQLTypeLookup();
            Assert.IsTrue(typeLookup.HasDbType(type));
        }
    }
}