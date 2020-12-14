using System;
using System.Net.Mail;
using System.Web.Mvc;

namespace Scheduling.Controllers
{
    public class TestController : Controller
    {
       // [Obsolete]
        public ActionResult Exception()
        {
            throw new Exception("This is a test exception to test the global exception handler");
        }
        
        public ActionResult Email()
        {
            MailMessage mm = new MailMessage("test-production-schedule-email@kalmbach.com", "scurran@kalmbach.com","Testing Email Delivery","We are good");

            SmtpClient c = new SmtpClient();

            try
            {
                c.Host = Scheduling.Email.Utility.GetSmtpHost();
                c.Send(mm);
                Scheduling.Database.Utility.CreateApplicationLoggingEntry("Test EMAIL PASSED");
            }

            catch(System.Exception e)
            {
                Scheduling.Database.Utility.CreateApplicationErrorLoggingEntry("Test EMAIL FAILED.." + e.Message);

            }

            finally
            {
                if (c != null) c.Dispose();

            }

            return View("_EmailTest");
            
        }
            
    }
}
