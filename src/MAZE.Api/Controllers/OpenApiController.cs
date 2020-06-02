using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;

namespace MAZE.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class OpenApiController : ControllerBase
    {
        private readonly IHostEnvironment _environment;

        public OpenApiController(IHostEnvironment environment)
        {
            _environment = environment;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var openApiFilePath = _environment.IsDevelopment()
                ? "../MAZE.Specification/openapi.yaml"
                : Path.Combine(_environment.ContentRootPath, "openapi.yaml");
            var openApiStream = new FileStream(openApiFilePath, FileMode.Open);
            return File(openApiStream, "application/octet-stream", "openapi.yaml");
        }
    }
}
