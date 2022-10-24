using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using StockAPI.Data;
using StockAPI.Model;
using System.ComponentModel.Design;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace StockAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly DataContext _context;

        public LoginController(DataContext context, IConfiguration configuration)
        {
            _config = configuration;
            _context = context;
        }

        [HttpGet]
        [Route("echoping")]
        public IActionResult EchoPing()
        {
            return Ok(true);
        }

        /*
        [HttpGet]
        [Route("echouser")]
        public async Task<IActionResult> EchoUser()
        {
            var identity = Thread.CurrentPrincipal.Identity;
            return Ok($" IPrincipal-user: {identity.Name} - IsAuthenticated: {identity.IsAuthenticated}");
        }*/

        public readonly record struct Login(string email, string password);

        [HttpPost]
        [Route("authenticate")]
        public IActionResult Authenticate(Login credentials)
        {
            var user = _context.Users.FirstOrDefault(x => x.Email == credentials.email && x.Password == credentials.password);

            if (user != null)
            {
                var issuer = _config.GetValue<string>("Jwt:Issuer");
                var audience = _config.GetValue<string>("Jwt:Audience");
                var key = Encoding.ASCII.GetBytes(_config.GetValue<string>("Jwt:Key"));
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                new Claim("Id", Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, user.Name),
                new Claim(JwtRegisteredClaimNames.Email, credentials.email),
                new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString())
             }),
                    Expires = DateTime.UtcNow.AddMinutes(5),
                    Issuer = issuer,
                    Audience = audience,
                    SigningCredentials = new SigningCredentials
                    (new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha512Signature)
                };
                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var jwtToken = tokenHandler.WriteToken(token);
                var stringToken = tokenHandler.WriteToken(token);
                return Ok(new { token = stringToken, user = new { user.Name, user.Email, user.Admin }});
            }
            return Unauthorized();
        }
    }
}
