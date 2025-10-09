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
        /// <summary>
        ///  Create access token
        /// </summary>
        /// <param name="userNamePasswd">username and password</param>
        /// <returns>status code</returns>
        /// [ProducesResponseType(StatusCodes.Status200OK)] // Access token created
        /// [ProducesResponseType(StatusCodes.Status400BadRequest)] // 400: wrong username or password
        [HttpPost("login")]
        public async Task<IActionResult> CreateJwtToken([FromBody] RegisterDto userNamePasswd)
        {
            if (userNamePasswd == null ||
                string.IsNullOrEmpty(userNamePasswd.UserName) ||
                string.IsNullOrEmpty(userNamePasswd.Password))
            {
                return BadRequest("Empty login or pwd!");
            }

            var user = userNamePasswd.UserName;
            var pwd = userNamePasswd.Password;

            var existingLogins = await _unitOfWork.UserLogin
                    .GetAllAsync()
                    .ConfigureAwait(false);

            var existingLogin = existingLogins.FirstOrDefault(login => login.Username.Equals(user) && login.Password.Equals(pwd));
            if (existingLogin == null)
            {
                return BadRequest("Invalid login!");
            }

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

        /// <summary>
        ///  Register username and password for login
        /// </summary>
        /// <param name="userNamePasswd"></param>
        /// <returns>status code</returns>
        /// [ProducesResponseType(StatusCodes.Status200OK)] // Access token created
        /// [ProducesResponseType(StatusCodes.Status400BadRequest)] // 400: empty username or password
        /// [ProducesResponseType(StatusCodes.Status500InternalServerError)] // 500: internal server error
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
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }

        }

    }
}
