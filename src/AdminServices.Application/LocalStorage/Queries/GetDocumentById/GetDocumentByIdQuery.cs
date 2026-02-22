using AdminServices.Application.Common.Models;
using MediatR;

namespace AdminServices.Application.LocalStorage.Queries.GetDocumentById;

/// <summary>
/// Query to get a document by ID including its base64 data
/// </summary>
public record GetDocumentByIdQuery(Guid Id) : IRequest<ApiResponse<LocalDocumentDetailDto>>;

/// <summary>
/// Detailed DTO that includes the base64 file data
/// </summary>
public class LocalDocumentDetailDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string FileDataBase64 { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string? Description { get; set; }
    public string? Tags { get; set; }
    public Guid? FolderId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }

}
