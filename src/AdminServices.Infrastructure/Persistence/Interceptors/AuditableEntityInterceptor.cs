using AdminServices.Application.Common.Interfaces;
using AdminServices.Domain.Primitives;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace AdminServices.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Interceptor to automatically set audit fields and the owner of new records
/// </summary>
public class AuditableEntityInterceptor : SaveChangesInterceptor
{
    private readonly IHttpContextService _httpContextService;

    public AuditableEntityInterceptor(IHttpContextService httpContextService)
    {
        _httpContextService = httpContextService;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void UpdateEntities(DbContext? context)
    {
        if (context == null) return;

        (context as ApplicationDbContext)?.StampOwners();

        var currentUser = _httpContextService.GetCurrentUserId() ?? "System";
        var now = DateTime.UtcNow;

        foreach (var entry in context.ChangeTracker.Entries<Entity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Property(e => e.CreatedBy).CurrentValue = currentUser;
                entry.Property(e => e.CreatedAt).CurrentValue = now;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Property(e => e.UpdatedBy).CurrentValue = currentUser;
                entry.Property(e => e.UpdatedAt).CurrentValue = now;
            }
        }
    }
}
