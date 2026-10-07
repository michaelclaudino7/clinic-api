using ClinicApi.DTOs.Appointments;
using FluentValidation;

namespace ClinicApi.Validators.Appointments;

public class CreateAppointmentDtoValidator : AbstractValidator<CreateAppointmentDto>
{
    public CreateAppointmentDtoValidator()
    {
        RuleFor(x => x.PatientId)
            .GreaterThan(0).WithMessage("PatientId must be a valid id.");

        RuleFor(x => x.DoctorId)
            .GreaterThan(0).WithMessage("DoctorId must be a valid id.");

        RuleFor(x => x.ScheduledAt)
            .NotEmpty().WithMessage("ScheduledAt is required.");

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}
