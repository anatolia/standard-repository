using System;

namespace StandardRepository.Models.Entities
{
    public class BaseEntity
    {
        public long Id { get; set; }
        public Guid Uid { get; set; }
        public string Name { get; set; }

        public long CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }

        public long? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public long? DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public bool IsDeleted { get; set; }

        public BaseEntity()
        {
            CreatedAt = DateTime.UtcNow;
            Uid = Guid.NewGuid();
            Name = string.Empty;
        }
    }
}