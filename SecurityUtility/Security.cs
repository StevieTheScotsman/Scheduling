using System;
using System.Configuration;
using System.Linq;

namespace Scheduling.Security
{
    public class Utility
    {
        const string AnonStr = "Anonymous";



        public static string GetCurrentLoggedInUser()
        {
            string CurrentUserWithDomain=System.Web.HttpContext.Current.User.Identity.Name;
            if(string.IsNullOrWhiteSpace(CurrentUserWithDomain))
            {
                return AnonStr;

            }

            else

            {
                string[] sa =CurrentUserWithDomain.Split(new string[] { "\\" }, StringSplitOptions.None);
                return sa[1];

            }
          
        }

        public static bool IsAnon()
        {
            return GetCurrentLoggedInUser() ==AnonStr;
        }

        public static string GetCurrentLoggedInUserEmail()
        {
            string RetStr = string.Empty;
            bool IsAnonUser = IsAnon();

            if (!IsAnonUser)
            {
                string CurrentUserName = GetCurrentLoggedInUser().Trim().ToLower();
                int Count = Scheduling.Database.Utility.GetAllUsers().Where(x => x.UserName == CurrentUserName).Count();
                if (Count > 0)
                {
                    RetStr = Scheduling.Database.Utility.GetAllUsers().Where(x => x.UserName == CurrentUserName).First().Email;

                }

            }

            return RetStr;

        }

        public static int GetCurrentLoggedInUserRoleID()
        {
            int IntRes = 0;

            bool IsAnonUser = IsAnon();

            if (!IsAnonUser)
            {
                string CurrentUserName = GetCurrentLoggedInUser().Trim().ToLower();
                int Count = Scheduling.Database.Utility.GetAllUsers().Where(x => x.UserName == CurrentUserName).Count();
                if (Count > 0)
                {
                    IntRes = Scheduling.Database.Utility.GetAllUsers().Where(x => x.UserName == CurrentUserName).First().RoleFK;

                }

            }

            return IntRes;

        }

        public static int GetCurrentIDForSpecifiedRole(string s)
        {
            string CheckStr = string.Format("{0}RoleValue", s);
            return Convert.ToInt32(ConfigurationManager.AppSettings[CheckStr]);
        }

        public static string GetDatabaseConnection()
        {
            string CurrentConnection = ConfigurationManager.ConnectionStrings["ProdScheduleDB"].ToString();
            string[] sa = CurrentConnection.Split(';');
            return sa[1];

        }
    }
}