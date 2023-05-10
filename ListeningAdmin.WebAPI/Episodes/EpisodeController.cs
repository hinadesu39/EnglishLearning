using ListeningInfrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ListeningAdmin.WebAPI.Episodes
{
    [Route("[controller]/[action]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    [UnitOfWork(typeof(ListeningDbContext))]
    public class EpisodeController : ControllerBase
    {

    }
}
