using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TestApplication.DataAccessLayer;
using TestApplication.DataAccessLayer.Dto;
using TestApplication.DataAccessLayer.Enums;
using TestApplication.DataAccessLayer.Exceptions;
using TestApplication.DataAccessLayer.Interfaces;
using TestApplication.Domain.Entities;

namespace TestApplication.Services
{
    public class UserService: IUserService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;

        public UserService(AppDbContext dbContext, IMapper mapper, IConfiguration configuration)
        {
            _context = dbContext;
            _mapper = mapper;
            _configuration = configuration;

            EnsureAdminExist();
        }

        public async Task<User> CreateUser(UserCreateDto userDto, string createdBy) 
        {
            if (await _context.Users.AnyAsync(u => u.Login == userDto.Login))
                throw new BadRequestException("this login is already occupied");

            var user = _mapper.Map<User>(userDto);

            user.Guid = Guid.NewGuid();
            user.CreatedOn = DateTime.UtcNow;
            user.CreatedBy = createdBy;
            user.ModifiedOn = DateTime.UtcNow;
            user.ModifiedBy = createdBy;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }
        public async Task<User> UpdateUser(Guid userId, UserUpdateDto userDto, string modifiedBy)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Guid == userId && u.RevokedOn == null);
            if (user == null)
                throw new NotFoundException("user not found or inactive");

            _mapper.Map(userDto, user);
            user.ModifiedOn = DateTime.UtcNow;
            user.ModifiedBy = modifiedBy;

            await _context.SaveChangesAsync();
            return user;
        }
        public async Task<User> UpdatePassword(Guid userId, string newPassword, string modifiedBy)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Guid == userId && u.RevokedOn == null);
            if (user == null)
                throw new NotFoundException("user not found or inactive");

            user.Password = newPassword;
            user.ModifiedOn = DateTime.UtcNow;
            user.ModifiedBy = modifiedBy;

            await _context.SaveChangesAsync();
            return user;
        }
        public async Task<User> UpdateLogin(Guid userId, string newLogin, string modifiedBy)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Guid == userId && u.RevokedOn == null);
            if (user == null)
                throw new NotFoundException("user not found or inactive");

            if (await _context.Users.AnyAsync(u => u.Login == newLogin && u.Guid != userId))
                throw new BadRequestException("this login is already occupied");

            user.Login = newLogin;
            user.ModifiedOn = DateTime.UtcNow;
            user.ModifiedBy = modifiedBy;

            await _context.SaveChangesAsync();
            return user;
        }
        public async Task<List<User>> GetAllActiveUsers()
        {
            return await _context.Users.Where(u => u.RevokedOn == null).ToListAsync();
        }
        public async Task<User> GetUserByLogin(string login)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Login == login);
        }
        public async Task<User> GetUserByLoginAndPassword(string login, string password)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Login == login && u.Password == password && u.RevokedOn == null);
        }
        public async Task<List<User>> GetUsersOlderThan(int age)
        {
            var date = DateTime.UtcNow.AddYears(-age);
            return await _context.Users.Where(u => u.Birthday.HasValue && u.Birthday.Value < date && u.RevokedOn == null).ToListAsync();
        }
        public async Task DeleteUser(Guid userId, string revokedBy, bool softDelete = true)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Guid == userId);
            if (user == null)
                throw new NotFoundException("user not found or inactive");

            if (softDelete)
            {
                user.RevokedOn = DateTime.UtcNow;
                user.RevokedBy = revokedBy;
            }
            else
            {
                _context.Users.Remove(user);
            }

            await _context.SaveChangesAsync();
        }
        public async Task RestoreUser(Guid userId, string modifiedBy)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Guid == userId);
            if (user == null || user.RevokedOn == null)
                throw new NotFoundException("user not found or already active");

            user.RevokedOn = null;
            user.RevokedBy = null;
            user.ModifiedOn = DateTime.UtcNow;
            user.ModifiedBy = modifiedBy;

            await _context.SaveChangesAsync();
        }
        public async Task<TokenDto> Login(LoginDto loginDto)
        {
            var user = await GetUserByLoginAndPassword(loginDto.Login, loginDto.Password);
            if (user == null)
                throw new InvalidLoginException("invalid login or password");

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Guid.ToString()),
                    new Claim(ClaimTypes.Name, user.Login),
                    new Claim("isadmin", user.Admin.ToString()),
                }),
                Expires = DateTime.UtcNow.AddMinutes(double.Parse(_configuration["Jwt:AccessLifeTime"])),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return new TokenDto { AccessToken = tokenHandler.WriteToken(token) };
        }

        private void EnsureAdminExist()
        {
            if (!_context.Users.Any(u => u.Admin))
            {
                var admin = new User
                {
                    Guid = Guid.NewGuid(),
                    Login = "admin",
                    Password = "admin123",
                    Name = "Admin",
                    Gender = Gender.Unknown,
                    Admin = true,
                    CreatedOn = DateTime.UtcNow,
                    CreatedBy = "system",
                    ModifiedOn = DateTime.UtcNow,
                    ModifiedBy = "system"
                };
                _context.Users.Add(admin);
                _context.SaveChanges();
            }
        }
    }
}
