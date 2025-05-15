using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TestApplication.DataAccessLayer.Dto;
using TestApplication.DataAccessLayer.Exceptions;
using TestApplication.DataAccessLayer.Interfaces;

namespace TestApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService, IMapper mapper)
        {
            _userService = userService;
        }

        #region create
        [HttpPost]
        [Authorize(Policy = "Admin")]
        public async Task<UserReadDto> CreateUser([FromBody] UserCreateDto userDto)
        {
            return await _userService.CreateUser(userDto, GetCurrentLogin());
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<TokenDto> Login([FromBody] LoginDto loginDto)
        {
            return await _userService.Login(loginDto);
        }
        #endregion

        #region update

        [HttpPut("{userId}")]
        [Authorize]
        public async Task<UserReadDto> UpdateUser(Guid userId, [FromBody] UserUpdateDto userDto)
        {
            return await _userService.UpdateUser(userId, userDto, GetCurrentLogin(), GetCurrentGuid());
        }

        [HttpPut("{userId}/password")]
        [Authorize]
        public async Task UpdatePassword(Guid userId, [FromBody] UserUpdatePasswordDto passwordDto)
        {
            await _userService.UpdatePassword(userId, passwordDto.NewPassword, GetCurrentLogin(), GetCurrentGuid());
        }

        [HttpPut("{userId}/login")]
        [Authorize]
        public async Task UpdateLogin(Guid userId, [FromBody] UserUpdateLoginDto loginDto)
        {
            await _userService.UpdateLogin(userId, loginDto.NewLogin, GetCurrentLogin(), GetCurrentGuid());
        }

        [HttpPut("{userId}/restore")]
        [Authorize(Policy = "Admin")]
        public async Task RestoreUser(Guid userId)
        {
            await _userService.RestoreUser(userId, GetCurrentLogin());
        }

        #endregion

        #region read
        [HttpGet("list/all")]
        [Authorize(Policy = "Admin")]
        public async Task<List<UserReadDto>> GetAllActiveUsers([FromQuery] bool Revoked = false)
        {
            return await _userService.GetAllActiveUsers(Revoked);
        }

        [HttpGet("{login}")]
        [Authorize(Policy = "Admin")]
        public async Task<UserReadDto> GetUserByLogin(string login)
        {
            return await _userService.GetUserByLogin(login);
        }

        [HttpPost("me")]
        [Authorize]
        public async Task<UserReadDto> GetUserByLoginAndPassword([FromBody] LoginDto loginDto)
        {
            return await _userService.GetUserByLoginAndPassword(loginDto.Login, loginDto.Password);
        }

        [HttpGet("list/older-than/{age}")]
        [Authorize(Policy = "Admin")]
        public async Task<List<UserReadDto>> GetUsersOlderThan(int age)
        {
            return await _userService.GetUsersOlderThan(age);
        }
        #endregion

        [HttpDelete("{userId}")]
        [Authorize(Policy = "Admin")]
        public async Task DeleteUser(Guid userId, [FromQuery] bool softDelete = true)
        {
            await _userService.DeleteUser(userId, GetCurrentLogin(), softDelete);
        }

        private string GetCurrentLogin()
        {
            var login = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(login))
            {
                throw new InvalidLoginException("User login not found in token");
            }
            return login;
        }

        private Guid GetCurrentGuid()
        {
            var guid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(guid))
            {
                throw new InvalidLoginException("User Guid not found in token");
            }
            return Guid.Parse(guid);
        }
    }
}
