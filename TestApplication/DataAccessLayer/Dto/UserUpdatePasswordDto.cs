using TestApplication.DataAccessLayer.Validators;

namespace TestApplication.DataAccessLayer.Dto
{
    public class UserUpdatePasswordDto
    {
        [PasswordLoginValidator]
        public string NewPassword { get; set; }
    }
}
