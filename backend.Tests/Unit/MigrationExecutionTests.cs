using System;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Xunit;
using Errando.Data;

namespace backend.Tests.Unit
{
    public class MigrationExecutionTests
    {
        [Fact]
        public void ExecuteAllBackendMigrations_UpAndDown_DoesNotThrow()
        {
            var asm = typeof(AppDbContext).Assembly;
            var migrationTypes = asm.GetTypes()
                .Where(type => type.Namespace == "backend.Migrations" && !type.IsAbstract &&
                    type.GetMethod("Up", BindingFlags.Instance | BindingFlags.NonPublic) != null)
                .ToList();

            Console.WriteLine($"Found {migrationTypes.Count} migration types");
            Assert.NotEmpty(migrationTypes);

            foreach (var migrationType in migrationTypes)
            {
                var parameterlessCtor = migrationType.GetConstructor(Type.EmptyTypes);
                Assert.NotNull(parameterlessCtor);

                var migration = (Migration)Activator.CreateInstance(migrationType)!;
                var builder = new MigrationBuilder("Microsoft.EntityFrameworkCore.Sqlite");

                var upMethod = migrationType.GetMethod("Up", BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.NotNull(upMethod);
                upMethod!.Invoke(migration, new object[] { builder });

                var downMethod = migrationType.GetMethod("Down", BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.NotNull(downMethod);
                downMethod!.Invoke(migration, new object[] { builder });

                var buildTargetModel = migrationType.GetMethod("BuildTargetModel", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (buildTargetModel != null)
                {
                    var modelBuilder = new ModelBuilder(new ConventionSet());
                    buildTargetModel.Invoke(migration, new object[] { modelBuilder });
                    Assert.NotNull(modelBuilder.Model);
                }
            }
        }
    }
}
