using Microsoft.EntityFrameworkCore;
using PassKeys.Business.Models;

namespace PassKeys.Business;

public class PassKeysDbContext : DbContext
{
    public PassKeysDbContext(DbContextOptions<PassKeysDbContext> options) : base(options)
    {
        SavingChanges += HandleAuditableEntities;
    }
    void HandleAuditableEntities(object? sender, SavingChangesEventArgs args)
    {
        var auditableEntityEntries = ChangeTracker.Entries<IAuditable>();
        foreach (var auditableEntityEntry in auditableEntityEntries)
        {
            var now = DateTime.UtcNow;
            if (auditableEntityEntry.State == EntityState.Added)
            {
                auditableEntityEntry.Entity.CreatedAtUtc = now;
                auditableEntityEntry.Entity.UpdatedAtUtc = now;
            }
            else if (auditableEntityEntry.State == EntityState.Modified)
            {
                auditableEntityEntry.Entity.UpdatedAtUtc = now;
            }
        }
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Credential> Credentials { get; set; }
}