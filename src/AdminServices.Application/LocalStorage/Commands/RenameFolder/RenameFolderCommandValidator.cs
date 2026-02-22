using FluentValidation;

namespace AdminServices.Application.LocalStorage.Commands.RenameFolder;

public class RenameFolderCommandValidator : AbstractValidator<RenameFolderCommand>
{
    public RenameFolderCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Folder name is required")
            .MaximumLength(255).WithMessage("Folder name must not exceed 255 characters");
    }
}
