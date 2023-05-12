using FluentValidation;

namespace IdentityService.WebAPI.Controllers.UserAdmin.Request
{
    public record EditAdminUserRequest(string PhoneNum);
    public class EditAdminUserRequestValidator : AbstractValidator<EditAdminUserRequest>
    {
        public EditAdminUserRequestValidator()
        {
            RuleFor(e => e.PhoneNum).NotNull().NotEmpty();
        }
    }
}
