using ClinicApi.DTOs.Doctors;
using FluentValidation;

namespace ClinicApi.Validators.Doctors;

public class UpdateDoctorDtoValidator : AbstractValidator<UpdateDoctorDto>
{
    public UpdateDoctorDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(150);

        RuleFor(x => x.Specialty)
            .NotEmpty().WithMessage("Specialty is required.")
            .MaximumLength(100);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");
    }
}
