using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenAiChat.Security.Jwt;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace OpenAiChat.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SecurityController : ControllerBase
    {
        private readonly ITokenService _tokenService;
        public SecurityController(ITokenService tokenService)
        {
            _tokenService = tokenService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> CreateJwtToken()
        {
            var claims = new[]
            {
                //new Claim(JwtRegisteredClaimNames.Sub, dto.Username),
                new Claim("name", "Alice Smith"),
                new Claim("role", "Admin"), // use "role" if RoleClaimType = "role"
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var accessToken = _tokenService.GenerateAccessToken(claims);
            var refreshToken = _tokenService.GenerateRefreshToken();

            return Ok(new { accessToken = accessToken, refreshToken = refreshToken });
        }
    }
}
