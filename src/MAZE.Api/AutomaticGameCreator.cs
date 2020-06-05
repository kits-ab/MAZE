using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace MAZE.Api
{
    public class AutomaticGameCreator : BackgroundService
    {
        private readonly GameService _gameService;
        private readonly CharacterService _characterService;

        public AutomaticGameCreator(GameService gameService, CharacterService characterService)
        {
            _gameService = gameService;
            _characterService = characterService;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _gameService.NewGame("Castle");
            return Task.CompletedTask;
        }
    }
}
