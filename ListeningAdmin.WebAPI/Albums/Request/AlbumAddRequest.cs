using CommonHelper;
using FluentValidation;
using ListeningDomain.Entities;
using ListeningInfrastructure;
using Microsoft.EntityFrameworkCore;

namespace ListeningAdmin.WebAPI.Albums.Request
{
    public record AlbumAddRequest(MultilingualString Name,Guid categoryId);
    public class AlbumAddRequestValidator : AbstractValidator<AlbumAddRequest>
    {
        public AlbumAddRequestValidator(ListeningDbContext dbCtx)
        {
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.Name.Chinese).NotNull().Length(1, 200);
            RuleFor(x => x.Name.English).NotNull().Length(1, 200);
            ///验证CategoryId是否存在
            RuleFor(x => x.categoryId).MustAsync(async (cId, ct) => await dbCtx.Categories.AnyAsync(c=>c.Id == cId))
                .WithMessage(c => $"CategoryId={c.categoryId}不存在");
        }
    }
}
