using AdminServices.Application.Common.Models;
using MediatR;

namespace AdminServices.Application.Notes.Commands.CreateNote;

/// <summary>
/// Command to create a new note
/// </summary>
public record CreateNoteCommand : IRequest<ApiResponse<Guid>>
{
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public Guid? CategoryId { get; init; }
    public string? Image1 { get; init; }
    public string? Image2 { get; init; }
    public string? Metadata { get; init; }
}
