using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

using NodaTime;

using StandardRepository.Models.Entities;

namespace StandardRepository.Helpers
{
    public class EntityUtils
    {
        private readonly TypeLookup _typeLookup;

        public EntityUtils(TypeLookup typeLookup, params Assembly[] assemblyOfEntities)
        {
            _typeLookup = typeLookup;
            AssembliesForEntities = assemblyOfEntities;
        }

        public Assembly[] AssembliesForEntities { get; }

        public virtual string GetFieldNameFromPropertyName(string propertyName, string entityTypeName = null) => propertyName.GetFieldNameFromPropertyName(entityTypeName);

        #region Name Helpers
        public string GetSchemaName(Type entityType)
        {
            const string schemaPrefix = "ISchema";
            var schemaInterface = entityType.GetInterfaces().First(x => x.Name.StartsWith(schemaPrefix));
            if (schemaInterface == null)
            {
                throw new Exception("please define schema interface of entity > " + entityType.Name);
            }

            return schemaInterface.Name.Replace(schemaPrefix, string.Empty).ToLowerInvariant();
        }

        public string GetTableName(Type entityType)
        {
            return GetFieldNameFromPropertyName(entityType.Name);
        }

        public string GetTableFullName(Type entityType)
        {
            return $"{GetSchemaName(entityType)}.{GetTableName(entityType)}";
        }

        public string GetParameterNameFromPropertyName(string propertyName)
        {
            return $"prm_{GetFieldNameFromPropertyName(propertyName)}";
        }
        #endregion

        #region Property Helpers
        public PropertyInfo[] GetAllProperties(Type entityType)
        {
            var fields = entityType.GetProperties()
                                   .Where(x => x.PropertyType.IsPublic
                                               && (_typeLookup.HasDbType(x.PropertyType)
                                                   || x.PropertyType.BaseType == typeof(BaseEntity))).ToArray();

            return fields;
        }

        public PropertyInfo[] GetProperties(Type entityType)
        {
            var fields = entityType.GetProperties()
                                   .Where(x => x.PropertyType.IsPublic
                                               && x.DeclaringType != typeof(BaseEntity)
                                               && (_typeLookup.HasDbType(x.PropertyType)
                                                   || x.PropertyType.BaseType == typeof(BaseEntity))).ToArray();

            return fields;
        }

        public PropertyInfo[] GetBaseProperties()
        {
            var baseFields = typeof(BaseEntity).GetProperties()
                                               .Where(x => _typeLookup.HasDbType(x.PropertyType)
                                                           && x.PropertyType.IsPublic).ToArray();
            return baseFields;
        }

        public PropertyInfo[] GetBasePropertiesExceptId()
        {
            var baseFields = GetBaseProperties().Where(x => x.Name != "Id").ToArray();
            return baseFields;
        }

        public List<Type> GetRelatedEntityTypes(Type entityType)
        {
            var entityTypes = new List<Type>();

            var entities = GetEntityTypes();
            foreach (var entity in entities)
            {
                var fields = GetAllProperties(entity);

                if (fields.Any(x => x.Name == entityType.Name + "Id")
                    && fields.Any(x => x.Name == entityType.Name + "Uid")
                    && fields.Any(x => x.Name == entityType.Name + "Name"))
                {
                    entityTypes.Add(entity);
                }
            }

            return entityTypes;
        }

        public List<Type> GetEntityTypes()
        {
            var entityTypes = new List<Type>();

            foreach (var assembly in AssembliesForEntities)
            {
                var types = assembly.GetTypes();
                var entities = types.Where(x => x.BaseType == typeof(BaseEntity)
                                                || x.BaseType?.BaseType == typeof(BaseEntity));

                entityTypes.AddRange(entities);
            }

            var removeList = new List<Type>();
            foreach (var entityType in entityTypes)
            {
                if (entityTypes.Any(x => x.BaseType == entityType))
                {
                    removeList.Add(entityType);
                }
            }

            foreach (var type in removeList)
            {
                entityTypes.Remove(type);
            }

            if (!entityTypes.Any())
            {
                throw new ApplicationException("please add your entities to project!");
            }

            return entityTypes;
        }
        #endregion

        public void MapFields<T>(IDataRecord reader, PropertyInfo[] properties, string entityTypeName,
                                 T entity) where T : new()
        {
            for (var i = 0; i < reader.FieldCount; i++)
            {
                var fieldName = reader.GetName(i);
                var prop = properties.FirstOrDefault(x => GetFieldNameFromPropertyName(x.Name, entityTypeName) == fieldName);

                var value = reader[i];
                if (value == DBNull.Value)
                {
                    continue;
                }

                if (prop != null)
                {
                    if ((prop.PropertyType == typeof(Instant)
                        || prop.PropertyType == typeof(Instant?))
                        && value is DateTime datetimeValue)
                    {
                        datetimeValue = DateTime.SpecifyKind(datetimeValue, DateTimeKind.Utc);
                        var instant = Instant.FromDateTimeUtc(datetimeValue);
                        prop.SetValue(entity, instant, null);
                    }
                    else
                    {
                        prop.SetValue(entity, value, null);
                    }

                    continue;
                }

                prop = properties.FirstOrDefault(x => GetFieldNameFromPropertyName(x.Name, entityTypeName) == fieldName.Replace((GetFieldNameFromPropertyName(entityTypeName) + "_"), string.Empty));
                if (prop != null)
                {
                    prop.SetValue(entity, value, null);
                }
            }
        }

        public void MapFieldsRevision<T>(IDataRecord reader, PropertyInfo[] properties, string entityTypeName,
                                         EntityRevision<T> revision) where T : BaseEntity, new()
        {
            revision.Id = Convert.ToInt64(reader[entityTypeName.GetFieldNameFromPropertyName() + "_id"]);
            revision.Revision = Convert.ToInt32(reader["revision"]);
            revision.RevisionedBy = Convert.ToInt64(reader["revisioned_by"]);
            var revisionedAt = Convert.ToDateTime(reader["revisioned_at"]);
            revisionedAt = DateTime.SpecifyKind(revisionedAt, DateTimeKind.Utc);
            revision.RevisionedAt = Instant.FromDateTimeUtc(revisionedAt);

            revision.Entity = new T();
            MapFields(reader, properties, entityTypeName, revision.Entity);
        }

    }
}