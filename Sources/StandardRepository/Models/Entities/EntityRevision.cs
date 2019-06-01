using NodaTime;

namespace StandardRepository.Models.Entities
{
    public class EntityRevision<T> where T : BaseEntity
    {
        public long Id { get; set; }
        public int Revision { get; set; }
        public long RevisionedBy { get; set; }
        public Instant RevisionedAt { get; set; }

        public T Entity { get; set; }
    }
}