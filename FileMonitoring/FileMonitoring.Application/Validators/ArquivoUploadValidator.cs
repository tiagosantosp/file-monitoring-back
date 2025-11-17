using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace FileMonitoring.Application.Validators;

public class ArquivoUploadValidator : AbstractValidator<IFormFile>
{
    private const long MaxFileSize = 10 * 1024 * 1024; // 10MB

    public ArquivoUploadValidator()
    {
        RuleFor(file => file)
            .NotNull()
            .WithMessage("Arquivo não pode ser nulo");

        RuleFor(file => file.Length)
            .GreaterThan(0)
            .WithMessage("Arquivo está vazio")
            .LessThanOrEqualTo(MaxFileSize)
            .WithMessage($"Arquivo não pode ser maior que {MaxFileSize / 1024 / 1024}MB");

        RuleFor(file => file.FileName)
            .NotEmpty()
            .WithMessage("Nome do arquivo é obrigatório")
            .Must(BeValidFileName)
            .WithMessage("Nome do arquivo contém caracteres inválidos");
    }

    private bool BeValidFileName(string fileName)
    {
        return !string.IsNullOrWhiteSpace(fileName) &&
               fileName.IndexOfAny(Path.GetInvalidFileNameChars()) < 0;
    }
}