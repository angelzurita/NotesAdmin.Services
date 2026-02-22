using FluentValidation;

namespace AdminServices.Application.LocalStorage.Commands.UploadDocument;

public class UploadDocumentCommandValidator : AbstractValidator<UploadDocumentCommand>
{
    public UploadDocumentCommandValidator()
    {
        RuleFor(x => x.FileName)
            .NotEmpty().WithMessage("File name is required")
            .MaximumLength(255).WithMessage("File name must not exceed 255 characters");

        RuleFor(x => x.ContentType)
            .NotEmpty().WithMessage("Content type is required")
            .MaximumLength(100).WithMessage("Content type must not exceed 100 characters");

        RuleFor(x => x.FileDataBase64)
            .NotEmpty().WithMessage("File data is required")
            .Must(BeValidBase64).WithMessage("File data must be a valid base64 string");

        RuleFor(x => x.FileSizeBytes)
            .GreaterThan(0).WithMessage("File size must be greater than 0");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters")
            .When(x => x.Description is not null);

        RuleFor(x => x.Tags)
            .MaximumLength(500).WithMessage("Tags must not exceed 500 characters")
            .When(x => x.Tags is not null);
    }

    private static bool BeValidBase64(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var buffer = new Span<byte>(new byte[value.Length]);
        return Convert.TryFromBase64String(value, buffer, out _);
    }
}
