using TestApplication.DataAccessLayer.Enums;

namespace TestApplication.DataAccessLayer.Dto
{
    public class UserReadDto
    {
        public Guid Guid { get; set; }
        public string Login { get; set; }
        public string Name { get; set; }
        public Gender Gender { get; set; }
        public DateTime? Birthday { get; set; }
        public bool Admin { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
        public string ModifiedBy { get; set; }
        public bool isRevoked { get; set; }
    }
}
