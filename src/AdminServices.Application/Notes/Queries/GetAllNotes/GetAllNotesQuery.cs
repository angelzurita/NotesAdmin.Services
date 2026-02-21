using AdminServices.Application.Common.Models;
using MediatR;

namespace AdminServices.Application.Notes.Queries.GetAllNotes;

/// <summary>
/// Query to get all notes
/// </summary>
public record GetAllNotesQuery : IRequest<ApiResponse<List<NoteDto>>>;

/// <summary>
/// Note DTO
/// </summary>
public class NoteDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public Guid? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string? Image1 { get; set; }
    public string? Image2 { get; set; }
    public string? Metadata { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
}
