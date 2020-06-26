using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using QRCoder;
using GameId = System.String;

namespace MAZE.Web.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class GamesController : Controller
    {
        private readonly IConfiguration _configuration;

        public GamesController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("{gameId}/Join/QR")]
        public IActionResult GetQrCodeForJoiningGame(GameId gameId)
        {
            var qrGenerator = new QRCodeGenerator();
            var host = Request.Host;
            var payload = new PayloadGenerator.Url($"{host}/gameControl/{gameId}/Anonymous");
            var qrCodeData = qrGenerator.CreateQrCode(payload);
            var qrCode = new SvgQRCode(qrCodeData);
            var qrCodeText = qrCode.GetGraphic(10);
            var qrCodeBytes = Encoding.UTF8.GetBytes(qrCodeText);
            return File(qrCodeBytes, "image/svg+xml");
        }
    }
}
