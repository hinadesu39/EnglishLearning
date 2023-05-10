using FluentValidation;

namespace ListeningAdmin.WebAPI.Categories.Request
{
    public record CategorySortRequest(Guid[] Guids);
    public class CategorySortRequestValidator : AbstractValidator<CategorySortRequest>
    {
        public CategorySortRequestValidator() 
        {
            RuleFor(x=>x.Guids).NotNull().NotEmpty().NotContains(Guid.Empty).NotDuplicated();
        }
    }


}
