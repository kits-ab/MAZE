namespace MAZE.Api.Contracts
{
    public class NewToken
    {
        public NewToken(string token)
        {
            Token = token;
        }

        public string Token { get; }
    }
}
