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
        public int Id { get; set; }
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

        /// <summary>
        /// Get a user by login details, or get an anonymous user by
        /// session. If an existing anonymous user session doesn't exist,
        /// it will be created.
        /// </summary>
        /// <param name="userPrincipal"></param>
        /// <param name="sessionState"></param>
        /// <param name="hostAddress"></param>
        /// <returns></returns>
        static internal User GetUser(System.Security.Principal.IPrincipal userPrincipal, dynamic sessionState, string hostAddress)
        {
            User result = null;
            if (sessionState.UserName == null)
            {
                if (userPrincipal.Identity.IsAuthenticated)
                {
                    string UserName = userPrincipal.Identity.Name;
                    sessionState.UserName = UserName;
                    result = User.GetUserByUserName(UserName);
                    if (result == null)
                    {
                        //SimpleMembershipProvider thinks we're logged in, but the account doesn't exist!
                        //Logout and return a new anonymous user
                        Alterity.Controllers.AccountController.InternalLogout();
                        result = User.CreateAnonymous(hostAddress);
                        sessionState.UserName = result.UserName;
                    }
                }
                else
                {
                    result = User.CreateAnonymous(hostAddress);
                    sessionState.UserName = result.UserName;
                }
            }
            else
            {
                string userName = sessionState.UserName;
                System.Diagnostics.Debug.Assert(!userPrincipal.Identity.IsAuthenticated || userPrincipal.Identity.Name == userName);
                result = User.GetUserByUserName(sessionState.UserName);
                if (result == null)
                {
                    sessionState.UserName = null;
                    return GetUser(userPrincipal, sessionState, hostAddress);
                }
            }
            return result;
        }
    }

}
