using CommonHelper;
using ListeningDomain.Entities;

namespace ListeningMain.WebAPI.Controllers.Categories
{
    public record CategoryVM(Guid Id, MultilingualString Name, Uri url)
    {
        public static CategoryVM? create(Category category)
        {
            if (category == null)
            {
                return null;
            }
            return new CategoryVM(category.Id, category.Name, category.CoverUrl);
        }

        public static CategoryVM[] create(Category[] categories)
        {
            return categories.Select(c => create(c)!).ToArray();
        }
    }
}
