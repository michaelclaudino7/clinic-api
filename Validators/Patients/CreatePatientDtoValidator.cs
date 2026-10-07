using ClinicApi.DTOs.Patients;
using FluentValidation;

namespace ClinicApi.Validators.Patients;

public class CreatePatientDtoValidator : AbstractValidator<CreatePatientDto>
{
    public CreatePatientDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(150);

        RuleFor(x => x.Cpf)
            .NotEmpty().WithMessage("CPF is required.")
            .Matches(@"^\d{11}$").WithMessage("CPF must contain exactly 11 digits.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(x => x.Phone)
            .MaximumLength(20)
            .When(x => !string.IsNullOrEmpty(x.Phone));

        RuleFor(x => x.BirthDate)
            .NotEmpty().WithMessage("Birth date is required.")
            .Must(d => d <= DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Birth date cannot be in the future.");
    }
}
