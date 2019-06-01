using StandardRepository.Models.Entities;

namespace StandardRepository.Helpers
{
    public static class EntityExtensions
    {
        public static bool IsExist(this BaseEntity entity)
        {
            return entity.Id > 0;
        }

        public static bool IsNotExist(this BaseEntity entity)
        {
            return !IsExist(entity);
        }
    }
}