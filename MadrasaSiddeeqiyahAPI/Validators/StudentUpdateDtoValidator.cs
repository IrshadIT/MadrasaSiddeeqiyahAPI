using FluentValidation;
using MadrasaSiddeeqiyahAPI.DTOs;

namespace MadrasaSiddeeqiyahAPI.Validators
{
    public class StudentUpdateDtoValidator : AbstractValidator<StudentUpdateDto>
    {
        public StudentUpdateDtoValidator()
        {
            RuleFor(x => x.FullName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.FatherName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Phone).NotEmpty().Matches(@"^\d{10}$").WithMessage("Phone number must be exactly 10 digits.");
            RuleFor(x => x.Course).NotEmpty();
            RuleFor(x => x.Status).NotEmpty().Must(status => status == "Pending" || status == "Approved" || status == "Rejected")
                .WithMessage("Status must be Pending, Approved, or Rejected.");
        }
    }
}
