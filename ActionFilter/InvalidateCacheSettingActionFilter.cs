using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Scheduling.Models;

namespace Scheduling.ActionFilter
{
   //caching is not currently applied to this app
   //The commented out method has been set to obsolete but could be used going forward.
    public class InvalidateCacheSettingActionFilter : ActionFilterAttribute
    {
        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
           //Scheduling.Caching.Utility.InvalidateCacheFromControllerContext(filterContext);
        }

    }
}