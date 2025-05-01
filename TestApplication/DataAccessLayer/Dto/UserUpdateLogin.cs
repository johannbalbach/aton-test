using TestApplication.DataAccessLayer.Validators;

namespace TestApplication.DataAccessLayer.Dto
{
    public class UserUpdateLogin
    {
        [PasswordLoginValidator]
        public string NewLogin { get; set; }
    }
}
