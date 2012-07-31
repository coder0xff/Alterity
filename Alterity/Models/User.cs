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
        public virtual ICollection<ChangeSet> ChangeSets { get; set; }

        public User()
        {
            Documents = new List<Document>();
            AdministratorOf = new List<Document>();
            ModeratorOf = new List<Document>();
            ChangeSets = new List<ChangeSet>();
        }

        public static User GetUserByUserName(String userName)
        {
            return EntityMappingContext.Current.Users.FirstOrDefault(_ => _.UserName == userName);
        }

        public static User Create(String userName, String emailAddress)
        {
            User user = new User();
            user.UserName = userName;
            user.EmailOrIPAddress = emailAddress;
            return EntityMappingContext.Current.Users.Add(user);
        }

        public static User CreateAnonymous(String IPAddress)
        {
            User user = new User();
            user.UserName = Guid.NewGuid().ToString();
            user.EmailOrIPAddress = IPAddress;
            return EntityMappingContext.Current.Users.Add(user);
        }

        public bool IsAnonymous { get { return !EmailOrIPAddress.Contains('@'); } }

        public void DeleteIfEmptyAnonymous()
        {
            if (IsAnonymous && Documents.Count == 0 && ChangeSets.Count == 0)
            {
                EntityMappingContext.Current.Users.Remove(this);
            }
        }
    }

    public static class UserExtensionMethods
    {
        static User GetUser(this System.Security.Principal.IPrincipal user)
        {
            return User.GetUserByUserName(user.Identity.Name);
        }
    }
}
