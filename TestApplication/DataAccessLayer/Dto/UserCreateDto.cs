using TestApplication.DataAccessLayer.Enums;
using TestApplication.DataAccessLayer.Validators;

namespace TestApplication.DataAccessLayer.Dto
{
    public class UserCreateDto
    {
        [PasswordLoginValidator]
        public string Login { get; set; }
        [PasswordLoginValidator]
        public string Password { get; set; }
        [NameValidator]
        public string Name { get; set; }
        public Gender Gender { get; set; }
        public DateTime? Birthday { get; set; }
        public bool Admin { get; set; }
    }
}
