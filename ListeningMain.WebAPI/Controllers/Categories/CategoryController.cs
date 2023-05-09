using ListeningDomain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace ListeningMain.WebAPI.Controllers.Categories
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly IListeningRepository repository;
        private readonly IMemoryCache cache;

        public CategoryController(IListeningRepository repository, IMemoryCache cache)
        {
            this.repository = repository;
            this.cache = cache;
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<ActionResult<CategoryVM>> FindById(Guid id)
        {
            var category = await cache.GetOrCreateAsync($"CategoryController.FindById.{id}",
                async (e) => { 
                e.SetSlidingExpiration(TimeSpan.FromSeconds(Random.Shared.Next(60,80)));
                return CategoryVM.create(await repository.GetCategoryByIdAsync(id));
                });

            if (category == null)
            {
                return NotFound();
            }
            return category;

        }

        [HttpGet]
        public async Task<ActionResult<CategoryVM[]>> FindAll()
        {
            var categoris = await cache.GetOrCreateAsync($"CategoryController.FindAll",
                async (e) => CategoryVM.create(await repository.GetCategoriesAsync()));
            return categoris;
        }
    }
}
