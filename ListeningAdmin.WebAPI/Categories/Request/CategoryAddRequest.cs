using CommonHelper;
using FluentValidation;

namespace ListeningAdmin.WebAPI.Categories.Request
{
    public record CategoryAddRequest(MultilingualString Name, Uri CoverUrl);
    public class CategoryAddRequestValidator : AbstractValidator<CategoryAddRequest>
    {
        public CategoryAddRequestValidator() 
        { 
            RuleFor(x=>x.Name).NotEmpty();
            RuleFor(x=>x.Name.Chinese).NotNull().Length(1,200);
            RuleFor(x=>x.Name.English).NotNull().Length(1,200);
            RuleFor(x=>x.CoverUrl).Length(5,500);
        }

    }
}
