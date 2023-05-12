using FluentValidation;

namespace IdentityService.WebAPI.Controllers.Login.Request
{
    public record LoginByPhoneAndPwdRequest(string PhoneNum, string Password);
    public class LoginByPhoneAndPwdRequestValidator : AbstractValidator<LoginByPhoneAndPwdRequest>
    {
        public LoginByPhoneAndPwdRequestValidator()
        {
            RuleFor(e => e.PhoneNum).NotNull().NotEmpty();
            RuleFor(e => e.Password).NotNull().NotEmpty();
        }
    }
}
