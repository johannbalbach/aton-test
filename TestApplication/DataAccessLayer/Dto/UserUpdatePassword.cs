using TestApplication.DataAccessLayer.Validators;

namespace TestApplication.DataAccessLayer.Dto
{
    public class UserUpdatePassword
    {
        [PasswordLoginValidator]
        public string NewPassword { get; set; }
    }
}
