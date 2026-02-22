using AdminServices.Application.Common.Models;
using AdminServices.Application.LocalStorage.Queries.GetDocumentById;
using MediatR;

namespace AdminServices.Application.LocalStorage.Commands.UploadDocument;

/// <summary>
/// Command to upload and store a document as base64 in the database
/// </summary>
public record UploadDocumentCommand : IRequest<ApiResponse<LocalDocumentDetailDto>>
{
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public string FileDataBase64 { get; init; } = string.Empty;
    public long FileSizeBytes { get; init; }
    public string? Description { get; init; }
    public string? Tags { get; init; }
    public Guid? FolderId { get; init; }
}
