using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

using StandardRepository.Helpers;
using StandardRepository.Helpers.SqlExecutor;

namespace StandardRepository.DbGenerator
{
    public abstract class DbGenerator<TConnection, TCommand, TParameter> : IDbGenerator
    where TConnection : DbConnection, new()
    where TCommand : DbCommand, new()
    where TParameter : DbParameter, new()
    {
        protected readonly EntityUtils _entityUtils;

        protected readonly SQLExecutor<TConnection, TCommand, TParameter> _sqlExecutorMaster;
        protected readonly SQLExecutor<TConnection, TCommand, TParameter> _sqlExecutor;

        /// <summary>
        /// this list keeps procedures and creation scripts for testing purposes
        /// </summary>
        public Dictionary<string, string> Procedures = new Dictionary<string, string>();

        protected DbGenerator(EntityUtils entityUtils,
                              SQLExecutor<TConnection, TCommand, TParameter> sqlExecutorMaster,
                              SQLExecutor<TConnection, TCommand, TParameter> sqlExecutor)
        {
            _entityUtils = entityUtils;
            _sqlExecutorMaster = sqlExecutorMaster;
            _sqlExecutor = sqlExecutor;
        }

        public bool IsDbExistsDb(string dbName)
        {
            var isDbExist = _sqlExecutorMaster.ExecuteSqlReturningValue<bool>($"SELECT true FROM pg_database WHERE datname = '{dbName}';").Result;
            return isDbExist;
        }

        public void CreateDb(string dbName)
        {
            if (!IsDbExistsDb(dbName))
            {
                _sqlExecutorMaster.ExecuteSql($"CREATE DATABASE {dbName};").Wait();
            }
        }

        public async Task Generate()
        {
            GenerateSchemas();
            GenerateTables();

            FillProceduresDictionary();
            await RunProcedureGenerationQueries();
        }
        
        public abstract List<string> GenerateSchemas();
        public abstract List<string> GenerateTables();

        public void FillProceduresDictionary()
        {
            PrepareRevisionsProcedures();
            PrepareSaveRevisionProcedures();
            PrepareRestoreRevisionProcedures();
            PrepareInsertProcedures();
            PrepareUpdateProcedures();
            PrepareSelectByIdProcedures();
            PrepareDeleteProcedures();
            PrepareUndoDeleteProcedures();
            PrepareHardDeleteProcedures();
        }

        public async Task RunProcedureGenerationQueries()
        {
            foreach (var item in Procedures)
            {
                var procedureScript = item.Value;
                await _sqlExecutor.ExecuteSql(procedureScript);
            }
        }

        protected void PrepareStoredProcedure(Type entityType, string procedureNamePostFix, Func<Type, string> getProcedureSql)
        {
            var tableFullName = _entityUtils.GetTableFullName(entityType);
            var procedureName = $"{tableFullName}{procedureNamePostFix}";
            var procedureSql = getProcedureSql(entityType);

            Procedures.Add(procedureName, procedureSql);
        }

        protected abstract void PrepareInsertProcedures();
        protected abstract void PrepareUpdateProcedures();
        protected abstract void PrepareDeleteProcedures();
        protected abstract void PrepareUndoDeleteProcedures();
        protected abstract void PrepareHardDeleteProcedures();
        protected abstract void PrepareSelectByIdProcedures();
        protected abstract void PrepareRevisionsProcedures();
        protected abstract void PrepareSaveRevisionProcedures();
        protected abstract void PrepareRestoreRevisionProcedures();
    }
}