using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Migrations;

namespace soromaps_api.Data
{
    public class RlsMigrationsSqlGenerator : NpgsqlMigrationsSqlGenerator
    {
        public RlsMigrationsSqlGenerator(
            MigrationsSqlGeneratorDependencies dependencies,
            INpgsqlSingletonOptions npgsqlSingletonOptions)
            : base(dependencies, npgsqlSingletonOptions)
        {
        }

        protected override void Generate(
            CreateTableOperation operation,
            IModel? model,
            MigrationCommandListBuilder builder,
            bool terminate = true)
        {
            base.Generate(operation, model, builder, terminate);

            var tableName = Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name, operation.Schema);

            builder
                .Append($"ALTER TABLE {tableName} ENABLE ROW LEVEL SECURITY;")
                .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator)
                .EndCommand();
        }
    }
}
