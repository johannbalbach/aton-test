using TestApplication.DataAccessLayer.Dto;
using TestApplication.Domain.Entities;

namespace TestApplication.DataAccessLayer.Interfaces
{
    public interface IUserService
    {
        Task<UserReadDto> CreateUser(UserCreateDto userDto, string createdBy);
        Task<UserReadDto> UpdateUser(Guid userId, UserUpdateDto userDto, string modifiedBy, Guid modifiedById);
        Task UpdatePassword(Guid userId, string newPassword, string modifiedBy, Guid modifiedById);
        Task UpdateLogin(Guid userId, string newLogin, string modifiedBy, Guid modifiedById);
        Task<List<UserReadDto>> GetAllActiveUsers(bool Revoked);
        Task<UserReadDto> GetUserByLogin(string login);
        Task<UserReadDto> GetUserByLoginAndPassword(string login, string password);
        Task<List<UserReadDto>> GetUsersOlderThan(int age);
        Task DeleteUser(Guid userId, string revokedBy, bool softDelete = true);
        Task RestoreUser(Guid userId, string modifiedBy);
        Task<TokenDto> Login(LoginDto loginDto);
    }
}
