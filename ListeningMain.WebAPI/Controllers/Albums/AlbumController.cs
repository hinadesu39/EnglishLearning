using ListeningDomain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace ListeningMain.WebAPI.Controllers.Albums
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class AlbumController : ControllerBase
    {
        private readonly IListeningRepository repository;
        private readonly IMemoryCache cache;

        public AlbumController(IListeningRepository repository, IMemoryCache cache)
        {
            this.repository = repository;
            this.cache = cache;
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<ActionResult<AlbumVM>> FindById(Guid id)
        {
            var Album = await cache.GetOrCreateAsync($"AlbumController.FindById.{id}",
                async (e) =>
                {
                    e.SetSlidingExpiration(TimeSpan.FromSeconds(Random.Shared.Next(60, 80)));
                    return AlbumVM.Create(await repository.GetAlbumByIdAsync(id));
                }
            );
            if (Album == null)
            {
                return NotFound();
            }
            return Album;
        }

        [HttpGet]
        [Route("{categoryId}")]

        public async Task<ActionResult<AlbumVM[]>> FindByCategoryId(Guid categoryId)
        {
            var Albums = await cache.GetOrCreateAsync($"AlbumController.FindByCategoryId.{categoryId}",
                async (e) => AlbumVM.Create(await repository.GetAlbumsByCategoryIdAsync(categoryId)));
            return Albums;

        }
    }
}
