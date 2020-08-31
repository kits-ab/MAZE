using PlayerId = System.Int32;

namespace MAZE.Api.Contracts
{
    public class NewPlayer
    {
        public NewPlayer(PlayerId playerId, string token)
        {
            PlayerId = playerId;
            Token = token;
        }

        public PlayerId PlayerId { get; }

        public string Token { get; }
    }
}
