using System.ComponentModel;

namespace TestApplication.DataAccessLayer.Enums
{
    public enum Role
    {
        None = 0,
        Admin = 1
    }

    public static class RoleConverter
    {
        public static Role Convert(bool IsAdmin)
        {
            if (IsAdmin)
                return Role.Admin;
            else
                return Role.None;
        }
    }
}
