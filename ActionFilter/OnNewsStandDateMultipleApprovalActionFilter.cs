using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web.Mvc;


namespace Scheduling.ActionFilter
{
    //in flux TBD
    public class OnNewsStandDateMultipleApprovalActionFilter : ActionFilterAttribute
    {
        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            int NewsstandFieldID=Convert.ToInt32(Scheduling.StringFunctions.Utility.GetAppSettingValue("NewsstandOnSaleDateMilestoneValue"));

            //build up subject and message for this event
            NameValueCollection nvc = filterContext.HttpContext.Request.Params;
            
            string CurrentValues =nvc["input"];
            List<string> ProjList = Scheduling.StringFunctions.Utility.GetStringListFromStringWithPossibleCommaSeperator(CurrentValues);

            string CurrentBody="The Following Projects/NewsStand Dates Have Been Approved\n\n";
            foreach(string s in ProjList)
            {
              int CurrentProjID=Convert.ToInt32(s);
              string ProjectName=Scheduling.Database.Utility.GetAllProjects().Where(x=>x.ID==CurrentProjID).First().Name;
              string CurrentDueDate=Scheduling.Database.Utility.GetMilestoneValuesByProjectID(CurrentProjID).Where(x=>x.MilestoneFieldFK==NewsstandFieldID).First().DueDate;
              string LongCurrentDueDate=Convert.ToDateTime(CurrentDueDate).ToLongDateString();
              CurrentBody=CurrentBody + string.Format("Project {0} With Due Date Of {1}\n\n",ProjectName,LongCurrentDueDate);
              
            }

            string CurrentSubject = "Project NewsStand Date(s) Approval Notification";
           
            Scheduling.Email.Utility.SendNotificationEmailOnNewsStandDateMultipleApproval("OnNewsStandDateMultipleApproval", CurrentSubject, CurrentBody);
        }

    }
}