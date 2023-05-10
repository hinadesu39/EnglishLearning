using ListeningAdmin.WebAPI.Albums.Request;
using ListeningDomain;
using ListeningDomain.Entities;
using ListeningInfrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace ListeningAdmin.WebAPI.Albums
{
    [Route("api/[controller]/[action]")]
    //[Authorize(Roles = "Admin")]
    [ApiController]
    [UnitOfWork(typeof(ListeningDbContext))]
    public class AlbumController : ControllerBase
    {
        private readonly ListeningDbContext dbContext;
        private IListeningRepository repository;
        private readonly ListeningDomainService domainService;

        public AlbumController(ListeningDbContext dbContext, IListeningRepository repository, ListeningDomainService domainService)
        {
            this.dbContext = dbContext;
            this.repository = repository;
            this.domainService = domainService;
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<ActionResult<Album?>> FindById([RequiredGuid] Guid id)
        {
            return await repository.GetAlbumByIdAsync(id);
        }

        [HttpGet]
        [Route("{categoryId}")]
        public async Task<ActionResult<Album[]>> FindByCategoryId(Guid categoryId)
        {
            return await repository.GetAlbumsByCategoryIdAsync(categoryId);
        }

        [HttpPost]
        public async Task<ActionResult<Guid>> Add(AlbumAddRequest request)
        {
            Album album = await domainService.AddAlbumAsync(request.categoryId, request.Name);
            dbContext.Add(album);
            return album.Id;
        }

        [HttpPut]
        [Route("{id}")]
        public async Task<ActionResult> Update(Guid id, AlbumUpdateRequest request)
        {
            var album = await repository.GetAlbumByIdAsync(id);
            if (album == null)
            {
                return NotFound("没找到");
            }
            album.ChangeName(request.Name);
            return Ok();
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<ActionResult> DeleteById(Guid id)
        {
            var album = await repository.GetAlbumByIdAsync(id);
            if (album == null)
            {
                return NotFound("未找到该album");
            }
            album.SoftDelete();
            return Ok();
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<ActionResult> Hide(Guid id)
        {
            var album = await repository.GetAlbumByIdAsync(id);
            if (album == null)
            {
                return NotFound("未找到该album");
            }
            album.Hide();
            return Ok();
        }
        [HttpGet]
        [Route("{id}")]
        public async Task<ActionResult> Show(Guid id)
        {
            var album = await repository.GetAlbumByIdAsync(id);
            if (album == null)
            {
                return NotFound("未找到该album");
            }
            album.Show();
            return Ok();
        }
        [HttpPut]
        [Route("{categoryId}")]
        public async Task<ActionResult> Sort([RequiredGuid] Guid categoryId, AlbumsSortRequest req)
        {
            await domainService.SortAlbumsAsync(categoryId, req.SortedAlbumIds);
            return Ok();
        }
    }
}
