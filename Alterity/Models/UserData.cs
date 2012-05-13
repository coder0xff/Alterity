using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Alterity.Models
{
    public class UserData
    {
        [Key]
        public string UserName { get; set; }
        public string EmailOrIPAddress { get; set; }
        public virtual ICollection<Document> Documents { get; set; }
        public virtual ICollection<Document> AdministratorOf { get; set; }
        public virtual ICollection<Document> ModeratorOf { get; set; }
    }

    public static class UserDataExtensionMethods
    {
        static UserData CreateUserData(this System.Security.Principal.IPrincipal user, EntityMappingContext dbContext)
        {
            return dbContext.UserData.Add(new UserData());
        }
        static UserData ReadUserData(this System.Security.Principal.IPrincipal user, EntityMappingContext dbContext)
        {
            UserData result = dbContext.UserData.FirstOrDefault(x => x.UserName == user.Identity.Name);
            if (result == null) result = user.CreateUserData(dbContext);
            return result;
        }
    }
}
