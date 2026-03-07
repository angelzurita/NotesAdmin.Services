using AdminServices.Domain.Primitives;

namespace AdminServices.Domain.Entities;

/// <summary>
/// Entity to store documents in the database as base64
/// </summary>
public class LocalDocument : Entity
{
    public string FileName { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public string FileDataBase64 { get; private set; } = string.Empty;
    public long FileSizeBytes { get; private set; }
    public string? Description { get; private set; }
    public string? Tags { get; private set; }
    public Guid? FolderId { get; private set; }
    public StorageFolder? Folder { get; private set; }

    private LocalDocument() { }

    public LocalDocument(
        string fileName,
        string contentType,
        string fileDataBase64,
        long fileSizeBytes,
        string? description = null,
        string? tags = null,
        Guid? folderId = null)
    {
        FileName = fileName;
        ContentType = contentType;
        FileDataBase64 = fileDataBase64;
        FileSizeBytes = fileSizeBytes;
        Description = description;
        Tags = tags;
        FolderId = folderId;
    }

    public void Update(string fileName, string contentType, string fileDataBase64, long fileSizeBytes, string? description, string? tags, Guid? folderId = null)
    {
        FileName = fileName;
        ContentType = contentType;
        FileDataBase64 = fileDataBase64;
        FileSizeBytes = fileSizeBytes;
        Description = description;
        Tags = tags;
        FolderId = folderId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MoveToFolder(Guid? folderId)
    {
        FolderId = folderId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateMetadata(string? description, string? tags)
    {
        Description = description;
        Tags = tags;
        UpdatedAt = DateTime.UtcNow;
    }
}
