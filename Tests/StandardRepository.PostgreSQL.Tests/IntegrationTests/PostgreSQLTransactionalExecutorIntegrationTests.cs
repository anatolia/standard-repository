using System;
using System.Transactions;

using Npgsql;
using NUnit.Framework;
using Shouldly;

using StandardRepository.PostgreSQL.Factories;
using StandardRepository.PostgreSQL.Tests.Base;

namespace StandardRepository.PostgreSQL.Tests.IntegrationTests
{
    [TestFixture]
    public class PostgreSQLTransactionalExecutorIntegrationTests : PostgresqlBaseRepositoryIntegrationTests
    {
        [Test]
        public void Commits()
        {
            // arrange
            var transactionalExecutor = GetPostgreSQLTransactionalExecutor();
            var organizationRepository = GetOrganizationRepository();
            var projectRepository = GetProjectRepository();

            var organization = GetOrganization();
            var project = GetProject(organization);

            // act
            var result = transactionalExecutor.ExecuteAsync<bool>(async cnn =>
            {
                organizationRepository.SetSqlExecutorForTransaction(cnn);
                projectRepository.SetSqlExecutorForTransaction(cnn);

                var orgIdOther = await organizationRepository.Insert(1, organization);
                var projectIdOther = await projectRepository.Insert(1, project);

                return true;

            }).Result;

            // assert
            organizationRepository.Count().Result.ShouldBe(1);
            projectRepository.Count().Result.ShouldBe(1);
        }

        [Test]
        public void Rollbacks()
        {
            // arrange
            var transactionalExecutor = GetPostgreSQLTransactionalExecutor();
            var organizationRepository = GetOrganizationRepository();
            var projectRepository = GetProjectRepository();

            var organization = GetOrganization();
            var project = GetProject(organization);

            try
            {
                // act
                var result = transactionalExecutor.ExecuteAsync<bool>(async cnn =>
                {
                    organizationRepository.SetSqlExecutorForTransaction(cnn);
                    projectRepository.SetSqlExecutorForTransaction(cnn);

                    var orgIdOther = await organizationRepository.Insert(1, organization);

                    throw new TransactionAbortedException();

                }).Result;
            }
            catch (Exception e)
            {
                e.ShouldBeOfType<AggregateException>();
                e.InnerException.ShouldBeOfType<TransactionAbortedException>();
            }

            // assert
            organizationRepository.Count().Result.ShouldBe(0);
            projectRepository.Count().Result.ShouldBe(0);
        }

        private PostgreSQLTransactionalExecutor GetPostgreSQLTransactionalExecutor()
        {
            var npgsqlConnection = new NpgsqlConnection(PostgreSQLConnectionFactory.GetConnectionString(GetConnectionSettings(GetTestDBName())));
            var transactionalExecutor = new PostgreSQLTransactionalExecutor(npgsqlConnection);
            return transactionalExecutor;
        }
    }
}