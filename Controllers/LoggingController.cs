using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Scheduling.Models;

namespace Scheduling.Controllers
{
    public class LoggingController : Controller
    {
        //Create Functions 
        public ActionResult ShowApplicationLog()
        {
            List<Log> LogList=Scheduling.Database.Utility.GetAllApplicationLogs();
            return View(LogList);
        }


        public ActionResult ShowApplicationErrorLog()
        {
            List<Log> LogList = Scheduling.Database.Utility.GetAllApplicationErrorLogs();
            return View(LogList);
        }
         
       
        //Reset Functions 
        public ActionResult ResetApplicationErrorLog()
        {
           string ComStr = "delete from dbo.ApplicationErrorLogging where 1=1;";
           Scheduling.Database.Utility.ExecuteNonQueryWrapper(ComStr);
           ViewBag.Message = "Application Error Log Records Have Been Removed";
           return View("ResetConfirmation");
        }

        public ActionResult ResetApplicationLog()
        {
            string ComStr = "delete from dbo.ApplicationLogging where 1=1;";
            Scheduling.Database.Utility.ExecuteNonQueryWrapper(ComStr);
            ViewBag.Message = "Application Log Records Have Been Removed";
            return View("ResetConfirmation");
        }

    }
}
