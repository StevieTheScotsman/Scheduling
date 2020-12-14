using System;
using System.Collections.Specialized;
using System.Linq;
using System.Web.Mvc;
using Scheduling.Models;

namespace Scheduling.ActionFilter
{
    //in flux TBD
    public class OnNewsStandDateRejectionActionFilter : ActionFilterAttribute
    {
        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            //build up subject and message for this event
            NameValueCollection nvc = filterContext.HttpContext.Request.Params;
            int CurrentProjectID =Convert.ToInt32(nvc["ProjectID"]);
            string CurrentComment = nvc["RequestorComment"];

            ProjectDisplay pd = Scheduling.Database.Utility.GetAllProjects().Where(x => x.ID == CurrentProjectID).First();
            string ProjectName = pd.Name;

            //pubcode
            int CurrentPubCode=0;
            if(pd.PubCodeFK.HasValue)
            {
                CurrentPubCode=(int)pd.PubCodeFK;
            }
            
            string CurrentSubject="Project Change Request Notification";
            string CurrentBody = string.Format("The following change request has been generated for Project {0}", ProjectName);
            CurrentBody += string.Format("\n\n {0}", CurrentComment);
            Scheduling.Email.Utility.SendNotificationEmailBasedOnActionFilterAndPubCode("OnCreateChangeRequest",CurrentSubject,CurrentBody, CurrentPubCode,false);
        }

    }
}