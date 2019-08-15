using System;

namespace StandardRepository.Helpers
{
    public class SQLConstants
    {
        public const string TIMESTAMP_FORMAT = "yyyy-MM-dd HH:mm:ss.ffffff";

        public const string INSERT_INTO = "INSERT INTO";
        public const string SELECT = "SELECT";
        public const string DELETE = "DELETE";
        public const string UPDATE = "UPDATE";
        public const string FROM = "FROM";
        public const string JOIN = "JOIN";
        public const string VALUES = "VALUES";
        public const string INTO = "INTO";
        public const string SET = "SET";
        public const string ORDER_BY = "ORDER BY";
        public const string DISTINCT = "DISTINCT";
        public const string WHERE = "WHERE";
        public const string COUNT = "COUNT";
        public const string MIN = "MIN";
        public const string MAX = "MAX";
        public const string SUM = "SUM";
        public const string BEGIN = "BEGIN";
        public const string END = "END";
        public const string COMMIT = "COMMIT";
        public const string ASC = "ASC";
        public const string DESC = "DESC";
        public const string AND = "AND";
        public const string OR = "OR";

        public const string PARAMETER_PRESIGN = "@";
        public const string PARAMETER_PREFIX = "prm_";
        public const string UPDATED_BY_PARAMETER_NAME = "prm_updated_by";
        public const string REVISION_PARAMETER_NAME = "prm_revision";
        public const string NAME_PARAMETER_NAME = "prm_name";
        public const string UID_PARAMETER_NAME = "prm_uid";
        public const string LAST_UID_PARAMETER_NAME = "prm_last_uid";
        public const string SKIP_PARAMETER_NAME = "prm_skip";
        public const string TAKE_PARAMETER_NAME = "prm_take";

        public const string PROCEDURE_INSERT_POSTFIX = "_insert";
        public const string PROCEDURE_UPDATE_POSTFIX = "_update";
        public const string PROCEDURE_DELETE_POSTFIX = "_delete";
        public const string PROCEDURE_UNDO_DELETE_POSTFIX = "_undo_delete";
        public const string PROCEDURE_HARD_DELETE_POSTFIX = "_hard_delete";
        public const string PROCEDURE_SELECT_BY_ID_POSTFIX = "_select_by_id";
        public const string PROCEDURE_SELECT_REVISIONS_POSTFIX = "_select_revisions";
        public const string PROCEDURE_SAVE_REVISION_POSTFIX = "_save_revision";
        public const string PROCEDURE_RESTORE_REVISION_POSTFIX = "_restore_revision";

        public string SchemaName { get; private set; }
        public string TableName { get; private set; }
        public string TableFullName { get; private set; }
        public string IdFieldName { get; private set; }
        public string IdParameterName { get; private set; }

        public string QueryBaseAny { get; private set; }
        public string QueryBaseCount { get; private set; }

        public string ProcedureNameInsert { get; private set; }
        public string ProcedureNameUpdate { get; private set; }
        public string ProcedureNameDelete { get; private set; }
        public string ProcedureNameUndoDelete { get; private set; }
        public string ProcedureNameHardDelete { get; private set; }
        public string ProcedureNameSelectById { get; private set; }
        public string ProcedureNameSelectRevisions { get; private set; }
        public string ProcedureNameSaveRevision { get; private set; }
        public string ProcedureNameRestoreRevision { get; private set; }

        public virtual string ParameterSign { get; }

        public SQLConstants(string schemaName, string tableName)
        {
            SetProperties(schemaName, tableName);
            ParameterSign = PARAMETER_PRESIGN + PARAMETER_PREFIX;
        }

        public SQLConstants(Type entityType, EntityUtils entityUtils)
        {
            var schemaName = entityUtils.GetSchemaName(entityType);
            var tableName = entityUtils.GetTableName(entityType);

            SetProperties(schemaName, tableName);
        }

        private void SetProperties(string schemaName, string tableName)
        {
            SchemaName = schemaName;
            TableName = tableName;
            TableFullName = $"{schemaName}.{tableName}";
            IdFieldName = $"{tableName}_id";
            IdParameterName = $"{PARAMETER_PREFIX}{tableName}_id";

            QueryBaseAny = $"{SELECT} {COUNT}(*) > 0 {FROM} {TableFullName}";
            QueryBaseCount = $"{SELECT} {COUNT}(*) {FROM} {TableFullName}";

            ProcedureNameInsert = $"{TableFullName}{PROCEDURE_INSERT_POSTFIX}";
            ProcedureNameUpdate = $"{TableFullName}{PROCEDURE_UPDATE_POSTFIX}";
            ProcedureNameDelete = $"{TableFullName}{PROCEDURE_DELETE_POSTFIX}";
            ProcedureNameUndoDelete = $"{TableFullName}{PROCEDURE_UNDO_DELETE_POSTFIX}";
            ProcedureNameHardDelete = $"{TableFullName}{PROCEDURE_HARD_DELETE_POSTFIX}";
            ProcedureNameSelectById = $"{TableFullName}{PROCEDURE_SELECT_BY_ID_POSTFIX}";
            ProcedureNameSelectRevisions = $"{TableFullName}{PROCEDURE_SELECT_REVISIONS_POSTFIX}";
            ProcedureNameSaveRevision = $"{TableFullName}{PROCEDURE_SAVE_REVISION_POSTFIX}";
            ProcedureNameRestoreRevision = $"{TableFullName}{PROCEDURE_RESTORE_REVISION_POSTFIX}";
        }
    }
}