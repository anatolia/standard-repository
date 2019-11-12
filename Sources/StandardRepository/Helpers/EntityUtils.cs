using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

using StandardRepository.Models.Entities;

namespace StandardRepository.Helpers
{
    public class EntityUtils
    {
        private readonly TypeLookup _typeLookup;

        public Dictionary<string, string> FieldNameCache { get; }
        public Dictionary<Type, PropertyInfo[]> AllPropertiesCache { get; }
        public PropertyInfo[] BaseProperties { get; }
        public Assembly[] AssembliesForEntities { get; }

        public List<Type> EntityTypes { get; }

        public EntityUtils(TypeLookup typeLookup, params Assembly[] assemblyOfEntities)
        {
            _typeLookup = typeLookup;

            AssembliesForEntities = assemblyOfEntities;
            FieldNameCache = new Dictionary<string, string>();
            AllPropertiesCache = new Dictionary<Type, PropertyInfo[]>();
            BaseProperties = GetBaseProperties();

            EntityTypes = GetEntityTypes();
        }

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
            return entityType.Name.GetDelimitedName();
        }

        public string GetTableFullName(Type entityType)
        {
            return $"{GetSchemaName(entityType)}.{GetTableName(entityType)}";
        }
        #endregion

        #region Property Helpers

        /// <summary>
        /// returns properties including coming from base.
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        public PropertyInfo[] GetAllProperties(Type entityType)
        {
            if (AllPropertiesCache.ContainsKey(entityType))
            {
                return AllPropertiesCache[entityType];
            }

            var fields = entityType.GetProperties();
            
            var properFields = new List<PropertyInfo>();
            for (var i = 0; i < fields.Length; i++)
            {
                var propertyInfo = fields[i];
                if (!propertyInfo.PropertyType.IsPublic)
                {
                    continue;
                }

                if (!_typeLookup.HasDbType(propertyInfo.PropertyType))
                {
                    continue;
                }

                properFields.Add(propertyInfo);
            }

            var propertyInfos = properFields.ToArray();
            AllPropertiesCache.Add(entityType, propertyInfos);
            return propertyInfos;
        }
        
        /// <summary>
        /// Returns properties but excludes coming from base.
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        public PropertyInfo[] GetPropertiesExceptBase(Type entityType)
        {
            if (AllPropertiesCache.ContainsKey(entityType))
            {
                return AllPropertiesCache[entityType];
            }

            var fields = entityType.GetProperties();
            
            var properFields = new List<PropertyInfo>();
            for (var i = 0; i < fields.Length; i++)
            {
                var propertyInfo = fields[i];
                if (!propertyInfo.PropertyType.IsPublic)
                {
                    continue;
                }
                
                if (propertyInfo.DeclaringType == typeof(BaseEntity))
                {
                    continue;
                }

                if (!_typeLookup.HasDbType(propertyInfo.PropertyType))
                {
                    continue;
                }

                properFields.Add(propertyInfo);
            }
            
            return properFields.ToArray();
        }

        private PropertyInfo[] GetBaseProperties()
        {
            var baseFields = typeof(BaseEntity).GetProperties()
                                               .Where(x => _typeLookup.HasDbType(x.PropertyType) && x.PropertyType.IsPublic).ToArray();
            return baseFields;
        }

        public List<Type> GetRelatedEntityTypes(Type entityType)
        {
            var entityTypes = new List<Type>();
            
            for (var i = 0; i < EntityTypes.Count; i++)
            {
                var entity = EntityTypes[i];
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

        private List<Type> GetEntityTypes()
        {
            var entityTypes = new List<Type>();

            for (var i = 0; i < AssembliesForEntities.Length; i++)
            {
                var assembly = AssembliesForEntities[i];
                var types = assembly.GetTypes();
                var entities = types.Where(x => x.BaseType == typeof(BaseEntity)
                                                || x.BaseType?.BaseType == typeof(BaseEntity));

                entityTypes.AddRange(entities);
            }

            entityTypes = entityTypes.Distinct().ToList();

            var removeList = new List<Type>();
            for (var i = 0; i < entityTypes.Count; i++)
            {
                var entityType = entityTypes[i];
                if (entityTypes.Any(x => x.BaseType == entityType))
                {
                    removeList.Add(entityType);
                }
            }

            for (var i = 0; i < removeList.Count; i++)
            {
                var type = removeList[i];
                entityTypes.Remove(type);
            }

            if (!entityTypes.Any())
            {
                throw new ApplicationException("please add your entities to project!");
            }

            return entityTypes;
        }
        #endregion

        public void MapFields<T>(IDataRecord reader, PropertyInfo[] properties, string entityTypeName, T entity) where T : new()
        {
            for (var i = 0; i < reader.FieldCount; i++)
            {
                var value = reader[i];
                if (value == DBNull.Value)
                {
                    continue;
                }

                var fieldName = reader.GetName(i);
                var fieldNameCacheKey = entityTypeName + fieldName;

                string propName;
                if (FieldNameCache.ContainsKey(fieldNameCacheKey))
                {
                    propName = FieldNameCache[fieldNameCacheKey];
                }
                else
                {
                    propName = fieldName.GetPropNameFromFieldName(entityTypeName);
                    FieldNameCache.Add(fieldNameCacheKey, propName);
                }

                for (var j = 0; j < properties.Length; j++)
                {
                    var property = properties[j];
                    if (property.Name != propName)
                    {
                        continue;
                    }

                    property.SetValue(entity, value, null);
                    break;
                }
            }
        }

        public void MapFieldsRevision<T>(IDataRecord reader, PropertyInfo[] properties, string entityTypeName,
                                         EntityRevision<T> revision) where T : BaseEntity, new()
        {
            revision.Id = Convert.ToInt64(reader[entityTypeName.GetDelimitedName() + "_id"]);
            revision.Revision = Convert.ToInt32(reader["revision"]);
            revision.RevisionedBy = Convert.ToInt64(reader["revisioned_by"]);
            revision.RevisionedAt = Convert.ToDateTime(reader["revisioned_at"]);

            revision.Entity = new T();
            MapFields(reader, properties, entityTypeName, revision.Entity);
        }

    }
}