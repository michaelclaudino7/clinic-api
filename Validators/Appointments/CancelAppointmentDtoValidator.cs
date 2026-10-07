using ClinicApi.DTOs.Appointments;
using FluentValidation;

namespace ClinicApi.Validators.Appointments;

public class CancelAppointmentDtoValidator : AbstractValidator<CancelAppointmentDto>
{
    public CancelAppointmentDtoValidator()
    {
        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Cancellation reason is required.")
            .MaximumLength(300);
    }
}
