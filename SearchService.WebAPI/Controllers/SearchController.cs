using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nest;
using SearchService.WebAPI.Controllers.Request;
using SearchServiceDomain;
using SearchServiceDomain.Entities;
using Zack.EventBus;

namespace SearchService.WebAPI.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class SearchController : ControllerBase
    {

        private readonly ISearchRepository repository;
        private readonly IEventBus eventBus;

        public SearchController(ISearchRepository repository, IEventBus eventBus)
        {
            this.repository = repository;
            this.eventBus = eventBus;
        }

        [HttpGet]
        public Task<SearchEpisodesResponse> SearchEpisodes([FromQuery] SearchEpisodesRequest req)
        {
            return repository.SearchEpisodes(req.Keyword, req.PageIndex, req.PageSize);
        }

        [HttpPut]
        public async Task<IActionResult> ReIndexAll()
        {
            //避免耦合，这里发送ReIndexAll的集成事件
            //所有向搜索系统贡献数据的系统都可以响应这个事件，重新贡献数据
            eventBus.Publish("SearchService.ReIndexAll", null);
            return Ok();
        }

        /// <summary>
        /// 测试用，上线后不该出现以下代码
        /// </summary>
        /// <param name="episode"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> UpsertEpisode(Episode episode)
        {
            await repository.UpsertAsync(episode);
            return Ok();
        }

        [HttpPost]
        public async Task<ActionResult> DeleteEpisode(Guid id)
        {
            await repository.DeleteAsync(id);
            return Ok();    
        }
    }
}
