using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;

namespace Ouranos.Pantheon.Core.Infra.Postgres;

public abstract class OuranosDbContext : DbContext
{
    private readonly string _schema;

    protected OuranosDbContext(DbContextOptions options, string schema) : base(options)
    {
        Guard.Against.NullOrWhiteSpace(schema);
        _schema = schema;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema(_schema.ToLower());
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
    }
}