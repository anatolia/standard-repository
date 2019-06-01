using System;

using NodaTime;

namespace StandardRepository.Models.Entities
{
    public class BaseEntity
    {
        public long Id { get; set; }
        public Guid Uid { get; set; }
        public string Name { get; set; }

        public long CreatedBy { get; set; }
        public Instant CreatedAt { get; set; }

        public long? UpdatedBy { get; set; }
        public Instant? UpdatedAt { get; set; }

        public long? DeletedBy { get; set; }
        public Instant? DeletedAt { get; set; }
        public bool IsDeleted { get; set; }

        public BaseEntity()
        {
            CreatedAt = SystemClock.Instance.GetCurrentInstant();
            Uid = Guid.NewGuid();
            Name = string.Empty;
        }
    }
}