using TestApplication.DataAccessLayer.Validators;

namespace TestApplication.DataAccessLayer.Dto
{
    public class LoginDto
    {
        [PasswordLoginValidator]
        public string Login { get; set; }
        [PasswordLoginValidator]
        public string Password { get; set; }
    }
}
