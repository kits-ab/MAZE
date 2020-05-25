using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace MAZE.Api
{
    public class AutomaticGameCreator : BackgroundService
    {
        private readonly GameService _gameService;

        public AutomaticGameCreator(GameService gameService)
        {
            _gameService = gameService;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _gameService.NewGame("Castle");
            return Task.CompletedTask;
        }
    }
}
