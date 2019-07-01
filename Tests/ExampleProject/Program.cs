using System;
using System.Collections.Generic;

using Npgsql;
using StandardRepository.PostgreSQL;
using StandardRepository.PostgreSQL.Factories;
using StandardRepository.PostgreSQL.Helpers;

using ExampleProject.Entities;
using ExampleProject.Repositories;

namespace ExampleProject
{
    class Program
    {
        static void Main()
        {
            var dbGenerator = new DbGenerator();
            var (typeLookup, entityUtils, connectionSettings, sqlExecutor) = dbGenerator.Generate();
            Console.WriteLine("Db Generated!");
            
            var organizationRepository = new OrganizationRepository(typeLookup, new PostgreSQLConstants<Organization>(entityUtils),
                                                                    entityUtils, new PostgreSQLExpressionUtils(), sqlExecutor, new List<string>());

            var organization = new Organization
            {
                Name = "test",
                Email = "test@test.com"
            };
            var orgId = organizationRepository.Insert(1, organization).Result;

            Console.WriteLine("Organization inserted, " + orgId);

            var projectRepository = new ProjectRepository(typeLookup, new PostgreSQLConstants<Project>(entityUtils),
                                                          entityUtils, new PostgreSQLExpressionUtils(), sqlExecutor, new List<string>());

            var project = new Project
            {
                Name = "test",
                Description = "test description",
                Cost = 1,
                IsActive = false
            };
            var projectId = projectRepository.Insert(1, project).Result;
            project.Id = projectId;

            Console.WriteLine("Project inserted, " + projectId);

            project.Name = "other test";
            projectRepository.Update(1, project).Wait();

            project.Name = "more other test";
            projectRepository.Update(1, project).Wait();

            var updatedProject = projectRepository.Select(x => x.Id == projectId).Result;

            Console.WriteLine("Project name updated to " + updatedProject.Name);

            Console.WriteLine("Project revisions;");
            var projectRevisions = projectRepository.SelectRevisions(projectId).Result;
            foreach (var projectRevision in projectRevisions)
            {
                Console.WriteLine(projectRevision.Revision + " - " + projectRevision.Entity.Name);
            }

            try
            {
                var transactionalExecutor = new PostgreSQLTransactionalExecutor(new NpgsqlConnection(PostgreSQLConnectionFactory.GetConnectionString(connectionSettings)));
                var result = transactionalExecutor.ExecuteAsync<bool>(async cnn =>
                {
                    organizationRepository.SetSqlExecutorForTransaction(cnn);
                    projectRepository.SetSqlExecutorForTransaction(cnn);

                    var orgIdOther = organizationRepository.Insert(1, organization).Result;
                    
                    project.OrganizationId = orgIdOther;
                    project.OrganizationUid = organization.Uid;
                    project.OrganizationName = organization.Name;

                    var projectIdOther = projectRepository.Insert(1, project).Result;

                    return true;

                }).Result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
            Console.WriteLine("project count " + projectRepository.Count().Result);
            Console.WriteLine("organization count " + organizationRepository.Count().Result);
        }
    }
}
