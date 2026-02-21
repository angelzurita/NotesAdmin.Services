using AdminServices.Application.Common.Models;
using MediatR;

namespace AdminServices.Application.Notes.Commands.UpdateNote;

/// <summary>
/// Command to update an existing note
/// </summary>
public record UpdateNoteCommand : IRequest<ApiResponse<Guid>>
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public Guid? CategoryId { get; init; }
    public string? Image1 { get; init; }
    public string? Image2 { get; init; }
    public string? Metadata { get; init; }
}
