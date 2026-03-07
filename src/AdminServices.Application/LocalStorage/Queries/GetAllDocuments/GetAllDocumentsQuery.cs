using AdminServices.Application.Common.Models;
using MediatR;

namespace AdminServices.Application.LocalStorage.Queries.GetAllDocuments;

/// <summary>
/// Query to get all stored documents (metadata only, without file data)
/// </summary>
public record GetAllDocumentsQuery : IRequest<ApiResponse<List<LocalDocumentDto>>>;

/// <summary>
/// DTO for document metadata listing (excludes base64 data for performance)
/// </summary>
public class LocalDocumentDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string? Description { get; set; }
    public string? Tags { get; set; }
    public Guid? FolderId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
}
