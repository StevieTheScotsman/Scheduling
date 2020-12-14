using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Net.Mail;


namespace Scheduling.Email
{
    public partial class Utility
    {
        public static void SendExceptionEmailToOnline(System.Exception exc,string Source)
        {
            
            string TemplateDirectory = Scheduling.StringFunctions.Utility.GetAppSettingValue("EmailTemplateDirectory");
            string ExcFile = Scheduling.StringFunctions.Utility.GetAppSettingValue("EmailExceptionFile");
            

            string CurrentBody = string.Empty;

            string path = Path.Combine(HttpContext.Current.Request.PhysicalApplicationPath, string.Format("{0}{1}{2}", TemplateDirectory, Path.DirectorySeparatorChar,ExcFile));

            bool FileExists = File.Exists(path); 
            
            if(FileExists)
            {

                using (StreamReader sr = new StreamReader(path))
                {

                    CurrentBody = sr.ReadToEnd();

                }

                if(!string.IsNullOrWhiteSpace(CurrentBody))
                {

                    string EmailFrom = Scheduling.StringFunctions.Utility.GetAppSettingValue("ExceptionEmailFrom");
                    string EmailTo = Scheduling.StringFunctions.Utility.GetAppSettingValue("ExceptionEmailTo");
                    string EmailSubject = Scheduling.StringFunctions.Utility.GetAppSettingValue("EmailExceptionSubject");

                    string CurrentMessage = string.Empty;
                    if(!string.IsNullOrEmpty(exc.Message)) CurrentMessage=exc.Message;

                    string CurrentStackTrace=string.Empty;
                    if(!string.IsNullOrEmpty(exc.StackTrace)) CurrentStackTrace=exc.StackTrace;

                    string CurrentExcType = string.Empty;
                    if(!string.IsNullOrEmpty(exc.GetType().ToString())) CurrentExcType=exc.GetType().ToString();

                    CurrentBody = CurrentBody.Replace("xType", CurrentExcType);
                    CurrentBody = CurrentBody.Replace("xStack", CurrentStackTrace);
                    CurrentBody = CurrentBody.Replace("xMessage", CurrentMessage);
                    CurrentBody = CurrentBody.Replace("xSource", Source);

                    MailMessage mm = new MailMessage(EmailFrom, EmailTo, EmailSubject, CurrentBody);

                    SmtpClient c = new SmtpClient();
                    try
                    {
                        c.Host = GetSmtpHost();
                        c.Send(mm);
                    }



                    finally
                    {
                        if (c != null) c.Dispose();

                    }


                }


            }
        }
    }
   
}