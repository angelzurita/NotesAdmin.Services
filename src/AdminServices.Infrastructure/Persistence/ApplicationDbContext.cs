using AdminServices.Application.Common.Interfaces;
using AdminServices.Domain.Entities;
using AdminServices.Domain.Primitives;
using Microsoft.EntityFrameworkCore;

namespace AdminServices.Infrastructure.Persistence;

/// <summary>
/// Application Database Context
/// </summary>
public class ApplicationDbContext : DbContext
{
    // Owner used by the query filters. Read on every query (not captured when the context is
    // created: the context is already built while the request is still being authenticated), so
    // every query on an owned entity only returns rows of the authenticated user.
    private readonly IHttpContextService? _httpContextService;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IHttpContextService httpContextService)
        : base(options)
    {
        _httpContextService = httpContextService;
    }

    /// <summary>Id of the user whose data this context can see (null = nobody's)</summary>
    public string? CurrentUserId => _httpContextService?.GetCurrentUserId();

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Note> Notes => Set<Note>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserSession> UserSessions => Set<UserSession>();
    public DbSet<LocalDocument> LocalDocuments => Set<LocalDocument>();
    public DbSet<StorageFolder> StorageFolders => Set<StorageFolder>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        modelBuilder.Entity<Note>().HasQueryFilter(e => e.OwnerId == CurrentUserId && !e.IsDeleted);
        modelBuilder.Entity<Category>().HasQueryFilter(e => e.OwnerId == CurrentUserId && !e.IsDeleted);
        modelBuilder.Entity<LocalDocument>().HasQueryFilter(e => e.OwnerId == CurrentUserId && !e.IsDeleted);
        modelBuilder.Entity<StorageFolder>().HasQueryFilter(e => e.OwnerId == CurrentUserId && !e.IsDeleted);
    }

    /// <summary>Owned entities can only be written by their owner (and are created for the current user)</summary>
    internal void StampOwners()
    {
        var currentUserId = CurrentUserId;
        foreach (var entry in ChangeTracker.Entries<IOwned>())
        {
            var owner = entry.Property(nameof(IOwned.OwnerId));
            if (entry.State == EntityState.Added)
            {
                if (string.IsNullOrEmpty(currentUserId))
                    throw new InvalidOperationException("Cannot create owned data without an authenticated user.");
                owner.CurrentValue = currentUserId;
            }
            else if ((entry.State is EntityState.Modified or EntityState.Deleted)
                     && (string)owner.OriginalValue! != currentUserId)
            {
                throw new InvalidOperationException("Cannot change data owned by another user.");
            }
        }
    }
}
