using CommonHelper;
using FileServiceDomain;
using FileServiceInfrastrucure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UserMgrWebApi;

namespace FileService.WebAPI.Uploader
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class UpLoaderController : ControllerBase
    {
        private readonly FSDBContext fSDBContext;
        private readonly FSDomainService fSDomainService;
        private readonly IFSRepository fSRepository;

        public UpLoaderController(FSDBContext fSDBContext, FSDomainService fSDomainService, IFSRepository fSRepository)
        {
            this.fSDBContext = fSDBContext;
            this.fSDomainService = fSDomainService;
            this.fSRepository = fSRepository;
        }

        [HttpGet]
        public async Task<FileExistsResponse> FileExists(long fileSize, string sha256Hash)
        {
            var item = await fSRepository.FindFileAsync(fileSize, sha256Hash);
            if (item == null)
            {
                return new FileExistsResponse(false, null);
            }
            else
            {
                return new FileExistsResponse(true, item.RemoteUrl);
            }
        }
        [UnitOfWork(typeof(FSDBContext))]
        [RequestSizeLimit(60_000_000)]
        [HttpPost]
        public async Task<ActionResult<Uri>> Upload([FromForm] UploadRequest request, CancellationToken cancellationToken)
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
