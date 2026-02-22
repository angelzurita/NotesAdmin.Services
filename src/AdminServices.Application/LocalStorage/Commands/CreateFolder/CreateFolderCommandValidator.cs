using FluentValidation;

namespace AdminServices.Application.LocalStorage.Commands.CreateFolder;

public class CreateFolderCommandValidator : AbstractValidator<CreateFolderCommand>
{
    public CreateFolderCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Folder name is required")
            .MaximumLength(255).WithMessage("Folder name must not exceed 255 characters");
    }
}
