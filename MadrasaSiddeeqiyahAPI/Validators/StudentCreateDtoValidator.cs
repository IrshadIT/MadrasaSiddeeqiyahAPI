using FluentValidation;
using MadrasaSiddeeqiyahAPI.DTOs;

namespace MadrasaSiddeeqiyahAPI.Validators
{
    public class StudentCreateDtoValidator : AbstractValidator<StudentCreateDto>
    {
        public StudentCreateDtoValidator()
        {
            RuleFor(x => x.FullName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.FatherName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Phone).NotEmpty().Matches(@"^\d{10}$").WithMessage("Phone number must be exactly 10 digits.");
            RuleFor(x => x.Course).NotEmpty();
            RuleFor(x => x.AadhaarNumber)
                .Must(a => string.IsNullOrWhiteSpace(a) || Services.AadhaarValidator.Validate(a).IsValid)
                .WithMessage("Invalid Aadhaar number. Must be 12 digits passing UIDAI Verhoeff verification.");
        }
    }
}
