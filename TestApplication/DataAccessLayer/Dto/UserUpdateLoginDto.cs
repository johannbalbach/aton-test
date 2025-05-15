using TestApplication.DataAccessLayer.Validators;

namespace TestApplication.DataAccessLayer.Dto
{
    public class UserUpdateLoginDto
    {
        [PasswordLoginValidator]
        public string NewLogin { get; set; }
    }
}
