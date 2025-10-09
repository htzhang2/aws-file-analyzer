using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenAiChat.Dto;
using OpenAiChat.Models;
using OpenAiChat.Repository;
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
        private readonly IUnitOfWork _unitOfWork;

        public SecurityController(ITokenService tokenService, IUnitOfWork uow)
        {
            _tokenService = tokenService;
            _unitOfWork = uow;
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

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto userNamePasswd)
        {
            if (string.IsNullOrEmpty(userNamePasswd.UserName) || string.IsNullOrEmpty(userNamePasswd.Password))
            {
                return BadRequest("Empty username or password!");
            }
            string user = userNamePasswd.UserName;
            string pwd = userNamePasswd.Password;

            var existingLogins = await _unitOfWork.UserLogin
                    .GetAllAsync()
                    .ConfigureAwait(false);

            var duplicateUser = existingLogins.FirstOrDefault(user => user.Username.Equals(user));

            if (duplicateUser != null)
            {
                return BadRequest("User exist!");
            }

            // Save username and password
            var login = new UserLoginModel
            {
                Username = user,
                Password = pwd
            };
            _unitOfWork.UserLogin.Add(login);

            try
            {
                await _unitOfWork.CompleteAsync().ConfigureAwait(false);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

    }
}
