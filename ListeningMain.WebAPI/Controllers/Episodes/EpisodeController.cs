using ListeningDomain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace ListeningMain.WebAPI.Controllers.Episodes
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class EpisodeController : ControllerBase
    {
        private readonly IListeningRepository repository;
        private readonly IMemoryCache cache;

        public EpisodeController(IListeningRepository repository, IMemoryCache cache)
        {
            this.repository = repository;
            this.cache = cache;
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<ActionResult<EpisodeVM>> FindById(Guid id)
        {
            var episode = await cache.GetOrCreateAsync($"EpisodeController.FindById.{id}",
                async (e) => {
                e.SetSlidingExpiration(TimeSpan.FromSeconds(Random.Shared.Next(60, 80)));
                return EpisodeVM.create(await repository.GetEpisodeByIdAsync(id), true);
            });
            if(episode == null)
            {
                return NotFound();
            }
            return episode;
        }
        [HttpGet]
        [Route("{albumId}")]
        public async Task<ActionResult<EpisodeVM[]>> FindByAlbumId(Guid albumId)
        {
            //加载Episode列表的，默认不加载Subtitle，这样降低流量大小
            var episodes = await cache.GetOrCreateAsync($"EpisodeController.FindByAlbumId.{albumId}",
                 async (e) => EpisodeVM.create(await repository.GetEpisodesByAlbumIdAsync(albumId),false));
            return episodes;
        }

    }
}
