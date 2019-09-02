using StandardRepository.Models.Entities;
using StandardRepository.Models.Entities.Schemas;
using System;

namespace StandardRepository.Tests.Base.Entities
{
    public class TestEntity : BaseEntity, ISchemaMain
    {       
        public string Email { get; set; }        
        public bool IsActive { get; set; }
        public Guid TestEntityUid { get; set; }
        public string TestEntityId { get; set; }
        public string TestEntityName { get; set; }
        public int Age { get; set; }
        public double Salary { get; set; }

    }
}
