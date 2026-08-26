using Common.Entities;
using System.Data;

namespace BusinessLogic.Mappers
{
    internal static class UserReaderMapper
    {
        public static User Map(IDataReader reader)
        {
            return new User
            {
                UserPK = reader.GetValue<System.Guid>("UserPK"),
                UserName = reader.GetValue<string>("UserName"),
                Email = reader.GetValue<string>("Email"),
                Password = reader.GetValue<string>("Password"),
                ActiveStatus = reader.GetValue<bool>("ActiveStatus"),
                RoleTypePK = reader.GetValue<System.Guid>("RoleTypePK"),
                RoleName = reader.GetValue<string>("RoleName"),
                FirstName = reader.GetValue<string>("FirstName"),
                SecondName = reader.GetValue<string?>("SecondName"),
                BirthDate = reader.GetValue<System.DateTime?>("BirthDate")
            };
        }
    }
}
