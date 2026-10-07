using ClinicApi.DTOs.Doctors;
using FluentValidation;

namespace ClinicApi.Validators.Doctors;

public class CreateDoctorDtoValidator : AbstractValidator<CreateDoctorDto>
{
    public CreateDoctorDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(150);

        RuleFor(x => x.Specialty)
            .NotEmpty().WithMessage("Specialty is required.")
            .MaximumLength(100);

        RuleFor(x => x.LicenseNumber)
            .NotEmpty().WithMessage("License number (CRM) is required.")
            .MaximumLength(20);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");
    }
}
