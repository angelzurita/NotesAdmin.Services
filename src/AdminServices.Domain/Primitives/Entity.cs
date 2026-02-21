namespace AdminServices.Domain.Primitives;

/// <summary>
/// Base class for all entities
/// </summary>
public abstract class Entity
{
    /// <summary>
    /// Entity identifier
    /// </summary>
    public Guid Id { get; protected set; }

    /// <summary>
    /// Creation date
    /// </summary>
    public DateTime CreatedAt { get; protected set; }

    /// <summary>
    /// Last update date
    /// </summary>
    public DateTime? UpdatedAt { get; protected set; }

    /// <summary>
    /// User who created the entity
    /// </summary>
    public string? CreatedBy { get; protected set; }

    /// <summary>
    /// User who last updated the entity
    /// </summary>
    public string? UpdatedBy { get; protected set; }

    /// <summary>
    /// Indicates if the entity is deleted (soft delete)
    /// </summary>
    public bool IsDeleted { get; protected set; }

    protected Entity()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
    }

    protected Entity(Guid id)
    {
        Id = id;
        CreatedAt = DateTime.UtcNow;
    }
}
