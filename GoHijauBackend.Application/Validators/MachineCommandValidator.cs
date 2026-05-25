using FluentValidation;
using GoHijauBackend.Application.Dto;

namespace GoHijauBackend.Application.Validators
{
    public class MachineCommandValidator : AbstractValidator<MachineCommand>
    {
        public MachineCommandValidator()
        {
            RuleFor(x => x.MachineId)
                .NotEmpty().WithMessage("MachineId is required.")
                .MaximumLength(50).WithMessage("MachineId must not exceed 50 characters.");

            RuleFor(x => x.ManufacturedDate)
                .LessThanOrEqualTo(DateTime.UtcNow)
                .WithMessage("Manufactured date cannot be in the future.");
        }
    }
}
