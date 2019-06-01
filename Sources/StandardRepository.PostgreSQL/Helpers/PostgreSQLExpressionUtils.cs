using System;
using System.Data;
using StandardRepository.Helpers;

namespace StandardRepository.PostgreSQL.Helpers
{
    public class PostgreSQLExpressionUtils : ExpressionUtils
    {
        private readonly PostgreSQLTypeLookup _typeLookup = new PostgreSQLTypeLookup();

        public PostgreSQLExpressionUtils() : base(SQLConstants.PARAMETER_PREFIX, PostgreSQLConstants.PARAMETER_PRESIGN)
        {
        }

        public override DbType GetDbType(Type type)
        {
            return _typeLookup.GetDbType(type);
        }

        public override string GetFieldNameFromPropertyName(string propertyName, string entityTypeName, bool isRelatedEntityProperty)
        {
            return propertyName.GetFieldNameFromPropertyName(entityTypeName, isRelatedEntityProperty);
        }
    }
}