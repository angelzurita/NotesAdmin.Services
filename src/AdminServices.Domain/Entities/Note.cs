using AdminServices.Domain.Primitives;

namespace AdminServices.Domain.Entities;

/// <summary>
/// Note entity for notes management
/// </summary>
public class Note : Entity
{
    public string Title { get; private set; } = string.Empty;
    public Guid? CategoryId { get; private set; }
    public Category? Category { get; private set; }
    public string Content { get; private set; } = string.Empty;
    public string? Image1 { get; private set; }
    public string? Image2 { get; private set; }
    public string? Metadata { get; private set; }

    private Note() { }

    public Note(string title, string content, Guid? categoryId = null, string? image1 = null, string? image2 = null, string? metadata = null)
    {
        Title = title;
        Content = content;
        CategoryId = categoryId;
        Image1 = image1;
        Image2 = image2;
        Metadata = metadata;
    }

    public void Update(string title, string content, Guid? categoryId, string? image1, string? image2, string? metadata)
    {
        Title = title;
        Content = content;
        CategoryId = categoryId;
        Image1 = image1;
        Image2 = image2;
        Metadata = metadata;
        UpdatedAt = DateTime.UtcNow;
    }
}
