using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using GameId = System.Int32;
using PlayerId = System.Int32;

namespace MAZE.Api
{
    public class TokenFactory
    {
        private readonly IConfiguration _configuration;

        public TokenFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string CreateJwtToken(GameId gameId, PlayerId playerId)
        {
            var tokenSecret = _configuration.GetValue<string>("TokenSecret");
            var securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(tokenSecret));

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("gameId", gameId.ToString()),
                    new Claim("playerId", playerId.ToString()),
                }),
                SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature),
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
