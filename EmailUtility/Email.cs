using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Web.Mvc;
using Scheduling.Models;


namespace Scheduling.Email
{
    public partial class Utility
    {
        public static void SendNotificationEmailOnNewsStandDateMultipleApproval(string MethodName, string Subject, string MessageBody)
        {
            int ActionID = Convert.ToInt32(ConfigurationManager.AppSettings["EmailActionValue"]);
            string CurrentMethod = MethodName.Trim();

            int EventCount = Scheduling.Database.Utility.GetAllMessagingEvents().Where(x => x.Method.Trim() == MethodName).Count();

            if (EventCount > 0)
            {

                int EventID = Scheduling.Database.Utility.GetAllMessagingEvents().Where(x => x.Method == MethodName).First().ID;

                bool CanContinue = Scheduling.Database.Utility.IsEmailActionViable(EventID, ActionID);

                if (CanContinue)
                {
                    List<User> UserEmailList =GetUniqueEmailUserRecipientsBasedOnNoPubCode(EventID, ActionID);
                    MailAddressCollection mac = GetEmailSenderListFromUserList(UserEmailList);
                    SendTextOrHtmlEmail(mac, Subject, MessageBody,false);

                }
            }

        }

        public static void SendNotificationForSingleProjectNewsStandDateApproval(int ProjectID,string Comment)
        {
            ProjectDisplay pd = Scheduling.Database.Utility.GetCurrentProjectByProjectID(ProjectID);

            string ProjName = pd.Name;
            string CurrentUser = Scheduling.Security.Utility.GetCurrentLoggedInUser();
            string Subject = string.Format("Newstand Date Approval For Project {0}", pd.Name);
            string CurrentNsDate=Scheduling.Database.Utility.GetNewstandDateForListedProject(ProjectID);
            string MessageBody = string.Format("Project {0} with NewsStand Date {1} has been approved by {2}", pd.Name, CurrentNsDate, CurrentUser);

            //Add optional Comment
            if (!string.IsNullOrEmpty(Comment))
            {
                MessageBody += string.Format("\n\nUser Comment -{0}", Comment);

            }
            string MethodName = Scheduling.StringFunctions.Utility.GetAppSettingValue("EventMethodNewsStandDateApproval");

            int EventID = Scheduling.Database.Utility.GetAllMessagingEvents().Where(x => x.Method == MethodName).First().ID;
            int ActionID = Convert.ToInt32(ConfigurationManager.AppSettings["EmailActionValue"]);

            List<User> UserEmailList = GetUniqueEmailUserRecipientsBasedOnNoPubCode(EventID, ActionID);

            if (UserEmailList.Count > 0)
            {
                MailAddressCollection mac = GetEmailSenderListFromUserList(UserEmailList);
                SendTextOrHtmlEmail(mac, Subject, MessageBody,false);
            }
            
        }

        public static void SendNotificationForSingleProjectNewsStandDateRejection(int ProjectID,string Comment)
        {
            ProjectDisplay pd = Scheduling.Database.Utility.GetCurrentProjectByProjectID(ProjectID);

            string ProjName = pd.Name;
            string CurrentUser = Scheduling.Security.Utility.GetCurrentLoggedInUser();
            string Subject = string.Format("Newstand Date Rejection For Project {0}", pd.Name);
            string CurrentNsDate = Scheduling.Database.Utility.GetNewstandDateForListedProject(ProjectID);
            string MessageBody = string.Format("Project {0} with NewsStand Date {1} has been rejected by {2}", pd.Name, CurrentNsDate, CurrentUser);

            if (!string.IsNullOrEmpty(Comment))
            {
                MessageBody += string.Format("\n\nComment -{0}", Comment);

            }

            string MethodName = Scheduling.StringFunctions.Utility.GetAppSettingValue("EventMethodNewsStandDateRejection");

            int EventID = Scheduling.Database.Utility.GetAllMessagingEvents().Where(x => x.Method == MethodName).First().ID;
            int ActionID = Convert.ToInt32(ConfigurationManager.AppSettings["EmailActionValue"]);

            List<User> UserEmailList = GetUniqueEmailUserRecipientsBasedOnNoPubCode(EventID, ActionID);

            if (UserEmailList.Count > 0)
            {
                MailAddressCollection mac = GetEmailSenderListFromUserList(UserEmailList);
                SendTextOrHtmlEmail(mac, Subject, MessageBody,false);
            }
            
        }

        public static void SendNotificationEmailBasedOnActionFilterAndPubCode(string MethodName,string Subject,string MessageBody,int PubCodeID,bool IsHtml)
        {
            //Using MethodName as the glue in order to use Convention Over Configuration
            int ActionID = Convert.ToInt32(ConfigurationManager.AppSettings["EmailActionValue"]);
            string CurrentMethod = MethodName.Trim();

            int EventCount = Scheduling.Database.Utility.GetAllMessagingEvents().Where(x => x.Method.Trim() == MethodName).Count();

            if (EventCount > 0)
            {
                int EventID = Scheduling.Database.Utility.GetAllMessagingEvents().Where(x => x.Method == MethodName).First().ID;
                
                bool CanContinue = Scheduling.Database.Utility.IsEmailActionViable(EventID, ActionID);

                //Records Exist Get a list of Users
                if (CanContinue)
                {
                    
                    List<User> UserEmailList = GetUniqueEmailUserRecipientsBasedOnPubCode(EventID, ActionID,PubCodeID);
                    MailAddressCollection mac = GetEmailSenderListFromUserList(UserEmailList);                                       
                    SendTextOrHtmlEmail(mac, Subject, MessageBody,IsHtml);
                }

            }


        }

        public static string GetSmtpHost()
        {
            return ConfigurationManager.AppSettings["SmtpHost"];
        }

        public static List<User> GetUniqueEmailUserRecipientsBasedOnNoPubCode(int EventID, int ActionID)
        {
            List<User> UserList = Scheduling.Database.Utility.GetUniqueEmailRecipientsBasedOnNoPubCode(EventID, ActionID);

            return UserList;

        }

        public static List<User> GetUniqueEmailUserRecipientsBasedOnPubCode(int EventID, int ActionID,int PubCodeID)
        {
            List<User> UserList = Scheduling.Database.Utility.GetUniqueEmailRecipientsBasedOnPubCode(EventID, ActionID,PubCodeID);
            return UserList;

        }

        public static MailAddressCollection GetEmailSenderListFromUserList(List<User> UserList)
        {
            MailAddressCollection mac = new MailAddressCollection();

            foreach (User u in UserList)
            {
                mac.Add(u.Email);

            }
            return mac;
        }

        [HandleError(View = "_EmailException", ExceptionType = typeof(SmtpException))]
        public static void SendTextOrHtmlEmail(MailAddressCollection CurrentMac, string CurrentSubject, string CurrentBody,bool IsHtml)
        {
            string CurrentFrom = Scheduling.StringFunctions.Utility.GetAppSettingValue("EmailSender");
            MailMessage mm = new MailMessage();
            foreach (MailAddress ma in CurrentMac)
            {
                mm.To.Add(ma);
            }

            mm.IsBodyHtml =IsHtml;
            mm.Subject = CurrentSubject;
            mm.Body = CurrentBody;

            mm.From = new MailAddress(CurrentFrom);

            SmtpClient c = new SmtpClient();
            try
            {
                c.Host = GetSmtpHost();
                c.Send(mm);
            }

            

            finally
            {
                if(c !=null)  c.Dispose();

            }

        }

    }
}