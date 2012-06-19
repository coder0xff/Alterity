using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Alterity.Models
{
    public class User
    {
        [Key]
        public string UserName { get; set; }
        public string EmailOrIPAddress { get; set; }
        public virtual ICollection<Document> Documents { get; set; }
        public virtual ICollection<Document> AdministratorOf { get; set; }
        public virtual ICollection<Document> ModeratorOf { get; set; }

        public static User GetUserByUserName(String userName)
        {
            return EntityMappingContext.Current.Users.FirstOrDefault(_ => _.UserName == userName);
        }

        public static User Create(String userName, String emailOrIPAddress)
        {
            User user = new User();
            user.UserName = userName;
            user.EmailOrIPAddress = emailOrIPAddress;
            return EntityMappingContext.Current.Users.Add(user);
        }
    }

    public static class UserDataExtensionMethods
    {
        static User GetUser(this System.Security.Principal.IPrincipal user)
        {
            return User.GetUserByUserName(user.Identity.Name);
        }
    }
}
