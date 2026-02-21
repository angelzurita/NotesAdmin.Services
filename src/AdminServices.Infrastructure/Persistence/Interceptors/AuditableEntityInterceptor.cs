using AdminServices.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace AdminServices.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Interceptor to automatically set audit fields
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

        var currentUser = _httpContextService.GetCurrentUserId() ?? "System";

        foreach (var entry in context.ChangeTracker.Entries<Domain.Primitives.Entity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.GetType().GetProperty("CreatedBy")?.SetValue(entry.Entity, currentUser);
                entry.Entity.GetType().GetProperty("CreatedAt")?.SetValue(entry.Entity, DateTime.UtcNow);
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.GetType().GetProperty("UpdatedBy")?.SetValue(entry.Entity, currentUser);
                entry.Entity.GetType().GetProperty("UpdatedAt")?.SetValue(entry.Entity, DateTime.UtcNow);
            }
        }
    }
}
