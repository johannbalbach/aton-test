using TestApplication.DataAccessLayer.Enums;
using TestApplication.DataAccessLayer.Validators;

namespace TestApplication.DataAccessLayer.Dto
{
    public class UserUpdateDto
    {
        [NameValidator]
        public string Name { get; set; }
        public Gender Gender { get; set; }
        public DateTime? Birthday { get; set; }
    }
}
