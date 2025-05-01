using TestApplication.DataAccessLayer.Dto;
using TestApplication.Domain.Entities;

namespace TestApplication.DataAccessLayer.Interfaces
{
    public interface IUserService
    {
        Task<User> CreateUser(UserCreateDto userDto, string createdBy);
        Task<User> UpdateUser(Guid userId, UserUpdateDto userDto, string modifiedBy);
        Task<User> UpdatePassword(Guid userId, string newPassword, string modifiedBy);
        Task<User> UpdateLogin(Guid userId, string newLogin, string modifiedBy);
        Task<List<User>> GetAllActiveUsers();
        Task<User> GetUserByLogin(string login);
        Task<User> GetUserByLoginAndPassword(string login, string password);
        Task<List<User>> GetUsersOlderThan(int age);
        Task DeleteUser(Guid userId, string revokedBy, bool softDelete = true);
        Task RestoreUser(Guid userId, string modifiedBy);
        Task<TokenDto> Login(LoginDto loginDto);
    }
}
