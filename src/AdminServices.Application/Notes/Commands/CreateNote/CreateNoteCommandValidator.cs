using FluentValidation;

namespace AdminServices.Application.Notes.Commands.CreateNote;

public class CreateNoteCommandValidator : AbstractValidator<CreateNoteCommand>
{
    public CreateNoteCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Note title is required")
            .MaximumLength(500).WithMessage("Note title must not exceed 500 characters");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Note content is required");
    }
}
