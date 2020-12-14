using System;
using System.Web.Mvc;

/* This is stated as obsolete as it was never used in production . I decided to keep it here for future reference. */

namespace Scheduling.Caching
{
    public class Utility
    {
        [Obsolete]
        public static void InvalidateCacheFromControllerContext(ResultExecutedContext filterContext)
        {
            try
            {
                string Message = "Invalidating Cache Timestamp..No Filter Context Information";
                if (filterContext != null)
                {
                    string ControllerInfo = filterContext.RequestContext.RouteData.Values["controller"].ToString();
                    string ActionInfo = filterContext.RequestContext.RouteData.Values["action"].ToString();
                    Message = string.Format("Invalidating Cache Timestamp..Updating File Dependency from Controller {0} on Action Method {1}", ControllerInfo, ActionInfo);
                }

                Scheduling.FileAccess.Utility.UpdateCacheFile();
                Scheduling.Database.Utility.CreateApplicationLoggingEntry(Message);

            }


            catch (Exception e)
            {
                Scheduling.Database.Utility.CreateApplicationLoggingEntry(e.Message.ToString());
            }

            
        }

    }
}