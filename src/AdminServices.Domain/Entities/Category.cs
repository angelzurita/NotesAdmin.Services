using AdminServices.Domain.Primitives;

namespace AdminServices.Domain.Entities;

/// <summary>
/// Category entity for organizing notes
/// </summary>
public class Category : Entity
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }

    private readonly List<Note> _notes = new();
    public IReadOnlyCollection<Note> Notes => _notes.AsReadOnly();

    private Category() { }

    public Category(string name, string? description)
    {
        Name = name;
        Description = description;
        IsActive = true;
    }

    public void Update(string name, string? description)
    {
        Name = name;
        Description = description;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
