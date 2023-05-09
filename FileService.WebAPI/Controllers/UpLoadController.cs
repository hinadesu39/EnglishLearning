using CommonHelper;
using FileServiceDomain;
using FileServiceInfrastrucure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UserMgrWebApi;

namespace FileService.WebAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UpLoadController : ControllerBase
    {
        private readonly FSDBContext fSDBContext;
        private readonly FSDomainService fSDomainService;
        private readonly IFSRepository fSRepository;

        public UpLoadController(FSDBContext fSDBContext, FSDomainService fSDomainService, IFSRepository fSRepository)
        {
            this.fSDBContext = fSDBContext;
            this.fSDomainService = fSDomainService;
            this.fSRepository = fSRepository;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<FileExistsResponse> FileExists(long fileSize,string sha256)
        {
            var item = await fSRepository.FindFileAsync(fileSize, sha256);
            if (item == null)
            {
                return new FileExistsResponse(false, null);
            }
            else
            {
                return new FileExistsResponse(true, item.RemoteUrl);
            }
        }
        [Authorize(Roles = "Admin")]
        [UnitOfWork(typeof(FSDBContext))]
        [RequestSizeLimit(60_000_000)]
        [HttpPost]
        public async Task<ActionResult<Uri>> Upload([FromForm] UploadRequest request,CancellationToken cancellationToken)
        {
            var file = request.File;
            string fileName = file.FileName;
            using Stream stream = file.OpenReadStream();
            var upItem = await fSDomainService.UploadAsync(stream, fileName, cancellationToken);
            fSDBContext.Add(upItem);
            return upItem.RemoteUrl;
        }


    }
}
