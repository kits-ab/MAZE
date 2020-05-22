using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace MAZE.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class OpenApiController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            var openApiStream = new FileStream("openapi.yaml", FileMode.Open);
            return File(openApiStream, "application/octet-stream", "openapi.yaml");
        }
    }
}
