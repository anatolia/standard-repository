using System;
using System.Collections.Generic;
using System.Reflection;

using NUnit.Framework;

using StandardRepository.Helpers;
using StandardRepository.PostgreSQL.DbGenerator;
using StandardRepository.PostgreSQL.Factories;
using StandardRepository.PostgreSQL.Helpers;
using StandardRepository.PostgreSQL.Helpers.SqlExecutor;
using StandardRepository.PostgreSQL.Tests.Base.Repositories;
using StandardRepository.Tests.Base.Entities;
using StandardRepository.Tests.IntegrationTests.Helpers;

namespace StandardRepository.PostgreSQL.Tests.Base
{
    public class PostgresqlBaseRepositoryIntegrationTests : BaseRepositoryIntegrationTests
    {
        [SetUp]
        public virtual void Setup()
        {
            var dbName = GetTestDBName();
            EnsureDbGenerated(dbName);
        }

        [TearDown]
        public void TearDown()
        {
            var dbName = GetTestDBName();
            DropDb(dbName);
        }

        public string POSTGRES_DB_NAME => "postgres";

        public Organization GetOrganization()
        {
            var organization = new Organization
            {
                Name = "Org " + Guid.NewGuid(),
                Email = "email." + Guid.NewGuid() + "@email.com",
                IsActive = true,
                ProjectCount = 5
            };
            return organization;
        }

        public Organization GetOrganization(Organization entity)
        {
            var organization = new Organization
            {
                Id = entity.Id,
                Name = entity.Name,
                Email = entity.Email,
                IsActive = entity.IsActive,
                ProjectCount = entity.ProjectCount
            };
            return organization;
        }

        public Project GetProject(Organization organization)
        {
            var project = new Project
            {
                Name = "Project " + Guid.NewGuid(),
                IsActive = true,
                OrganizationUid = organization.Uid,
                OrganizationId = organization.Id,
                OrganizationName = organization.Name
            };
            return project;
        }

        public OrganizationRepository GetOrganizationRepository()
        {
            var postgreSQLTypeLookup = GetTypeLookup();
            var entityUtils = GetEntityUtils(postgreSQLTypeLookup, GetAssemblyOfEntities());
            var sqlExecutor = GetSQLExecutor(GetTestDBName());
            var repository = new OrganizationRepository(postgreSQLTypeLookup, new PostgreSQLConstants<Organization>(entityUtils), entityUtils,
                                                        new PostgreSQLExpressionUtils(), sqlExecutor, new List<string>());

            return repository;
        }

        private static Assembly GetAssemblyOfEntities()
        {
            return typeof(Organization).Assembly;
        }

        public ProjectRepository GetProjectRepository()
        {
            var postgreSQLTypeLookup = GetTypeLookup();
            var entityUtils = GetEntityUtils(postgreSQLTypeLookup, GetAssemblyOfEntities());
            var sqlExecutor = GetSQLExecutor(GetTestDBName());
            var repository = new ProjectRepository(postgreSQLTypeLookup, new PostgreSQLConstants<Project>(entityUtils), entityUtils,
                                                   new PostgreSQLExpressionUtils(), sqlExecutor, new List<string>());

            return repository;
        }

        public PostgreSQLTypeLookup GetTypeLookup()
        {
            return new PostgreSQLTypeLookup();
        }

        public void EnsureDbGenerated(string dbName)
        {
            var masterExecutor = GetSQLExecutor(POSTGRES_DB_NAME);
            var isDbExist = masterExecutor.ExecuteSqlReturningValue<bool>($"SELECT true FROM pg_database WHERE datname = '{dbName}';").Result;
            if (!isDbExist)
            {
                masterExecutor.ExecuteSql($"CREATE DATABASE {dbName};").Wait();
            }

            Sleep();

            if (!isDbExist)
            {
                var typeLookup = new PostgreSQLTypeLookup();
                var entityUtils = new EntityUtils(typeLookup, GetAssemblyOfEntities());
                var executor = GetSQLExecutor(dbName);
                var dbGenerator = new PostgreSQLDbGenerator(typeLookup, entityUtils, (PostgreSQLExecutor)masterExecutor, (PostgreSQLExecutor)executor);
                dbGenerator.Generate().Wait();
            }
        }

        public void DropDb(string dbName)
        {
            Sleep();

            var utils = GetSQLExecutor(POSTGRES_DB_NAME);

            utils.ExecuteSql($@"SELECT Pg_terminate_backend(pg_stat_activity.pid)
                                FROM   pg_stat_activity
                                WHERE  pg_stat_activity.datname = '{dbName}'
                                       AND pid <> Pg_backend_pid();

                                DROP DATABASE {dbName};").Wait();
        }

        public PostgreSQLExecutor GetSQLExecutor(string dbName)
        {
            var typeLookup = new PostgreSQLTypeLookup();
            var entityUtils = new EntityUtils(typeLookup, GetAssemblyOfEntities());
            var sqlExecutor = new PostgreSQLExecutor(new PostgreSQLConnectionFactory(GetConnectionSettings(dbName)), entityUtils);
            return sqlExecutor;
        }
    }
}