using AsmResolver.PE;
using ListeningAdmin.WebAPI.Categories.Request;
using ListeningDomain;
using ListeningDomain.Entities;
using ListeningInfrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace ListeningAdmin.WebAPI.Categories
{
    [Route("[controller]/[action]")]
    [Authorize(Roles = "Admin")]
    [ApiController]
    [UnitOfWork(typeof(ListeningDbContext))]
    public class CategoryController : ControllerBase
    {
        private readonly ListeningDbContext dbContext;
        private IListeningRepository repository;
        private readonly ListeningDomainService domainService;

        public CategoryController(ListeningDbContext dbContext, IListeningRepository repository, ListeningDomainService domainService)
        {
            this.dbContext = dbContext;
            this.repository = repository;
            this.domainService = domainService;
        }
        [HttpGet]
        public Task<Category[]> FindAll()
        {
            return repository.GetCategoriesAsync();
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<ActionResult<Category>> FindById(Guid id)
        {
            var category = await repository.GetCategoryByIdAsync(id);
            if(category == null)
            {
                return NotFound($"没有Id={id}的Category");
            }
            return category;
        }

        [HttpPost]
        public async Task<ActionResult<Guid>> Add(CategoryAddRequest req)
        {
            var category = await domainService.AddCategory(req.Name, req.CoverUrl);
            dbContext.Add(category);
            return category.Id;
        }

        [HttpPut]
        [Route("{id}")]
        public async Task<ActionResult> Update([RequiredGuid] Guid id, CategoryUpdateRequest request)
        {
            var cat = await repository.GetCategoryByIdAsync(id);
            if (cat == null)
            {
                return NotFound("id不存在");
            }
            cat.ChangeName(request.Name);
            cat.ChangeCoverUrl(request.CoverUrl);
            return Ok();
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<ActionResult> DeleteById([RequiredGuid] Guid id)
        {
            var cat = await repository.GetCategoryByIdAsync(id);
            if (cat == null)
            { 
                return NotFound($"没有Id={id}的Category");
            }
            cat.SoftDelete();//软删除
            return Ok();
        }

        [HttpPut]
        public async Task<ActionResult> Sort(CategorySortRequest req)
        {
            await domainService.SortCategory(req.Guids);
            return Ok();
        }
    }
}
