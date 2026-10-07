using ClinicApi.DTOs.Patients;
using FluentValidation;

namespace ClinicApi.Validators.Patients;

public class UpdatePatientDtoValidator : AbstractValidator<UpdatePatientDto>
{
    public UpdatePatientDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(150);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(x => x.Phone)
            .MaximumLength(20)
            .When(x => !string.IsNullOrEmpty(x.Phone));
    }
}
