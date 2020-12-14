using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Scheduling.Models;

namespace Scheduling.ActionFilter
{
    //in operation
    public class OnProjectStatusChangeActionFilter : ActionFilterAttribute
    {
        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            //At this point the record has been updated in the database.We will use hidden fields to obtain original field values.

            NameValueCollection nvc = filterContext.HttpContext.Request.Params;
            int SentProjectStatus=Convert.ToInt32(nvc["ProjectStatus"]);
            int OriginalProjectStatus = Convert.ToInt32(nvc["OriginalProjectStatus"]);

            int CurrentProjectID=Convert.ToInt32(nvc["ID"]);

            ProjectDisplay pd = Scheduling.Database.Utility.GetAllProjects().Where(x => x.ID == CurrentProjectID).First();
            string ProjectName = pd.Name;
            int CurrentProjectStatus = pd.CurrentProjectStatus;

            //pubcode
            int CurrentPubCode=0;
            if(pd.PubCodeFK.HasValue)
            {
                CurrentPubCode=(int)pd.PubCodeFK;
            }


            if (OriginalProjectStatus != SentProjectStatus)
            {
                  string TemplateBody = string.Empty;
                  string TemplateDirectory = Scheduling.StringFunctions.Utility.GetAppSettingValue("EmailTemplateDirectory");
                  string ProjectStatusFile = Scheduling.StringFunctions.Utility.GetAppSettingValue("ProjectStatusTemplateFile");
                  string path =Path.Combine(HttpContext.Current.Request.PhysicalApplicationPath,string.Format("{0}{1}{2}",TemplateDirectory,Path.DirectorySeparatorChar,ProjectStatusFile));

                  bool FileExists = File.Exists(path);
                  if (FileExists)
                  {
                      using (StreamReader sr = new StreamReader(path))
                      {
                          TemplateBody = sr.ReadToEnd();
                      }

                        string OriginalStatusStr = Scheduling.Database.Utility.GetAllProjectStatuses().Where(x => x.ID == OriginalProjectStatus).First().Description;
                        string SentStatusStr = Scheduling.Database.Utility.GetAllProjectStatuses().Where(x => x.ID == SentProjectStatus).First().Description;
                        string CurrentSubject =string.Format("Project Status Change Notification for {0}",ProjectName);

                        if(!string.IsNullOrWhiteSpace(TemplateBody))
                        {
                            string CurrentBody = TemplateBody.Replace("#OldStatus#", OriginalStatusStr).Replace("#NewStatus#", SentStatusStr);
                            Scheduling.Email.Utility.SendNotificationEmailBasedOnActionFilterAndPubCode("OnProjectStatusChange", CurrentSubject, CurrentBody, CurrentPubCode,true);
                        }

                  }
                
            }
        }

    }
}