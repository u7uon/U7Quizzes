using FluentValidation;
using U7Quizzes.DTOs.Auth;
using U7Quizzes.Models;

namespace U7Quizzes.Validators
{
    public class UserValidator : AbstractValidator<RegisterDTO>
    {
        public UserValidator()
        {
            RuleFor(x => x.DisplayName)
            .NotEmpty().WithMessage("Tên hiển thị không được để trống")
            .MaximumLength(100);

            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Tên đăng nhập là bắt buộc")
                .MinimumLength(4)
                .MaximumLength(30);

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email là bắt buộc")
                .EmailAddress().WithMessage("Email không hợp lệ");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Mật khẩu là bắt buộc")
                .MinimumLength(6).WithMessage("Mật khẩu phải ít nhất 6 ký tự");

            RuleFor(x => x.ConfirmPassword)
                .Equal(x => x.Password).WithMessage("Xác nhận mật khẩu không khớp");
        }
    }
}
