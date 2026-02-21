using FluentValidation;

namespace AdminServices.Application.Notes.Commands.UpdateNote;

public class UpdateNoteCommandValidator : AbstractValidator<UpdateNoteCommand>
{
    public UpdateNoteCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Note ID is required");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Note title is required")
            .MaximumLength(500).WithMessage("Note title must not exceed 500 characters");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Note content is required");
    }
}
