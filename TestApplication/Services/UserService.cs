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

        public async Task<UserReadDto> CreateUser(UserCreateDto userDto, string createdBy) 
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

            var userReadDto = _mapper.Map<UserReadDto>(user);

            return userReadDto;
        }
        public async Task<UserReadDto> UpdateUser(Guid userId, UserUpdateDto userDto, string modifiedBy, Guid modifiedById)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Guid == userId && u.RevokedOn == null);
            if (user == null)
                throw new NotFoundException("user not found or inactive");

            var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Guid == modifiedById);
            if (!currentUser.Admin && currentUser.Guid != userId)
                throw new ForbiddenException("u dont have enough rights to do it action");

            _mapper.Map(userDto, user);
            user.ModifiedOn = DateTime.UtcNow;
            user.ModifiedBy = modifiedBy;

            await _context.SaveChangesAsync();
            var userReadDto = _mapper.Map<UserReadDto>(user);

            return userReadDto;
        }
        public async Task UpdatePassword(Guid userId, string newPassword, string modifiedBy, Guid modifiedById)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Guid == userId && u.RevokedOn == null);
            if (user == null)
                throw new NotFoundException("user not found or inactive");

            var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Guid == modifiedById);
            if (!currentUser.Admin && currentUser.Guid != userId)
                throw new ForbiddenException("u dont have enough rights to do it action");

            user.Password = newPassword;
            user.ModifiedOn = DateTime.UtcNow;
            user.ModifiedBy = modifiedBy;

            await _context.SaveChangesAsync();

            return;
        }
        public async Task UpdateLogin(Guid userId, string newLogin, string modifiedBy, Guid modifiedById)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Guid == userId && u.RevokedOn == null);
            if (user == null)
                throw new NotFoundException("user not found or inactive");

            if (await _context.Users.AnyAsync(u => u.Login == newLogin && u.Guid != userId))
                throw new BadRequestException("this login is already occupied");

            var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Guid == modifiedById);
            if (!currentUser.Admin && currentUser.Guid != userId)
                throw new ForbiddenException("u dont have enough rights to do it action");

                
            user.Login = newLogin;
            user.ModifiedOn = DateTime.UtcNow;
            user.ModifiedBy = modifiedBy;

            await _context.SaveChangesAsync();

            return;
        }
        public async Task<List<UserReadDto>> GetAllActiveUsers(bool Revoked)
        {
            return _mapper.Map<List<UserReadDto>>(await _context.Users.Where(u => Revoked ? u.RevokedOn != null : u.RevokedOn == null).OrderBy(u => u.CreatedOn).ToListAsync());
        }
        public async Task<UserReadDto> GetUserByLogin(string login)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Login == login && u.RevokedOn == null);
            if (user == null)
                throw new NotFoundException("user not found or inactive");

            var userReadDto = _mapper.Map<UserReadDto>(user);

            return userReadDto;
        }
        public async Task<UserReadDto> GetUserByLoginAndPassword(string login, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Login == login && u.Password == password && u.RevokedOn == null);
            if (user == null)
                throw new NotFoundException("user not found or inactive");

            var userReadDto = _mapper.Map<UserReadDto>(user);

            return userReadDto;
        }
        public async Task<List<UserReadDto>> GetUsersOlderThan(int age)
        {
            var date = DateTime.UtcNow.AddYears(-age);
            var userList = await _context.Users.Where(u => u.Birthday.HasValue && u.Birthday.Value < date && u.RevokedOn == null).ToListAsync();
            if (userList == null)
                throw new NotFoundException("user not found or inactive");

            return _mapper.Map<List<UserReadDto>>(userList);
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
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Login == loginDto.Login && u.Password == loginDto.Password && u.RevokedOn == null);
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
                    new Claim("UserRole", RoleConverter.Convert(user.Admin).ToString()),
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
