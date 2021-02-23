using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.Mvc;
using System.Data.SqlClient;
using System.Data;

namespace iCore_User.Modules.SecurityAuthentication
{
    public class EMASRoleProvider : RoleProvider
    {
        public override string ApplicationName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override void CreateRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            throw new NotImplementedException();
        }

        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            throw new NotImplementedException();
        }

        public override string[] GetAllRoles()
        {
            throw new NotImplementedException();
        }

        public override string[] GetRolesForUser(string username)
        {
            try
            {
                string GRFU = "";
                if (username.ToUpper() == "ICADLG") { GRFU = "AdminEmas"; }
                if (username.ToUpper() == "ICULFP") { GRFU = "UserEmas"; }
                return new string[] { GRFU };
            }
            catch (Exception)
            {
                return new string[] { };
            }
        }

        public override string[] GetUsersInRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public override bool IsUserInRole(string username, string roleName)
        {
            try
            {
                if ((username.ToUpper() == "ICADLG") && (roleName == "AdminEmas")) { return true; }
                if ((username.ToUpper() == "ICULFP") && (roleName == "UserEmas")) { return true; }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override bool RoleExists(string roleName)
        {
            throw new NotImplementedException();
        }
    }
}