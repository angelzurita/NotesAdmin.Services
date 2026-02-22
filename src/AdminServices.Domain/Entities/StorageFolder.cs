using AdminServices.Domain.Primitives;

namespace AdminServices.Domain.Entities;

/// <summary>
/// Represents a folder (or subfolder) in the local storage system.
/// Self-referencing for infinite recursive nesting.
/// </summary>
public class StorageFolder : Entity
{
    public string Name { get; private set; } = string.Empty;
    public Guid? ParentFolderId { get; private set; }
    public StorageFolder? ParentFolder { get; private set; }

    private readonly List<StorageFolder> _children = new();
    public IReadOnlyCollection<StorageFolder> Children => _children.AsReadOnly();

    private readonly List<LocalDocument> _documents = new();
    public IReadOnlyCollection<LocalDocument> Documents => _documents.AsReadOnly();

    private StorageFolder() { }

    public StorageFolder(string name, Guid? parentFolderId = null)
    {
        Name = name;
        ParentFolderId = parentFolderId;
    }

    public void Rename(string name)
    {
        Name = name;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Move(Guid? newParentFolderId)
    {
        ParentFolderId = newParentFolderId;
        UpdatedAt = DateTime.UtcNow;
    }
}
