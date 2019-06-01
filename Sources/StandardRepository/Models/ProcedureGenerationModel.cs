using System;

namespace StandardRepository.Models
{
    public class ProcedureGenerationModel
    {
        public string SchemaName { get; }
        public string TableName { get; }
        public string RevisionTableName { get; }
        public string TableFullName { get; }
        public string RevisionTableFullName { get; }
        public string IdFieldName { get; }
        public string IdParameterName { get; }

        public ProcedureGenerationModel(Type entityType, Func<Type, string> getSchemaName, Func<string, string, bool, string> getNameForDb)
        {
            SchemaName = getSchemaName(entityType);
            TableName = getNameForDb(entityType.Name, null, false);
            RevisionTableName = $"{TableName}_revision";
            TableFullName = $"{SchemaName}.{TableName}";
            RevisionTableFullName = $"{SchemaName}.{RevisionTableName}";
            IdFieldName = $"{TableName}_id";
            IdParameterName = $"prm_{IdFieldName}";
        }
    }
}