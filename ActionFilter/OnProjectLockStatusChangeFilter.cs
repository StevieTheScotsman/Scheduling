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
    public class OnProjectLockStatusChangeActionFilter : ActionFilterAttribute
    {

        public static string GetLockDescription(int i)

        {
            string RetStr="UnLocked";
            if(i==1) RetStr="Locked";
            return RetStr;
        }

        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            //At this point the record has been updated in the database.We will use hidden fields to obtain original field values.

            NameValueCollection nvc = filterContext.HttpContext.Request.Params;
            int CurrentLockStatus = Convert.ToInt32(nvc["lock"]);
            int OriginalLockStatus = Convert.ToInt32(nvc["OriginalLockStatus"]);

            int CurrentProjectID=Convert.ToInt32(nvc["ID"]);

            ProjectDisplay pd = Scheduling.Database.Utility.GetAllProjects().Where(x => x.ID == CurrentProjectID).First();
            string ProjectName = pd.Name;
            



            //pubcode
            int CurrentPubCode=0;
            if(pd.PubCodeFK.HasValue)
            {
                CurrentPubCode=(int)pd.PubCodeFK;
            }


            if (CurrentLockStatus != OriginalLockStatus)
            {
                  string TemplateBody = string.Empty;
                  string TemplateDirectory = Scheduling.StringFunctions.Utility.GetAppSettingValue("EmailTemplateDirectory");
                  string LockFile = Scheduling.StringFunctions.Utility.GetAppSettingValue("ProjectLockStatusTemplateFile");
                  string path = Path.Combine(HttpContext.Current.Request.PhysicalApplicationPath, string.Format("{0}{1}{2}", TemplateDirectory, Path.DirectorySeparatorChar, LockFile));

                  bool FileExists = File.Exists(path);
                  if (FileExists)
                  {
                      using (StreamReader sr = new StreamReader(path))
                      {
                          TemplateBody = sr.ReadToEnd();
                      }

                        string CurrentSubject =string.Format("Project Lock Status Change Notification for {0}",ProjectName);

                        if(!string.IsNullOrWhiteSpace(TemplateBody))
                        {
                            string CurrentBody = TemplateBody.Replace("#OldLockStatus#", GetLockDescription(OriginalLockStatus)).Replace("#NewLockStatus#",GetLockDescription(CurrentLockStatus)).Replace("#ProjectName#",ProjectName);
                            Scheduling.Email.Utility.SendNotificationEmailBasedOnActionFilterAndPubCode("OnProjectStatusChange", CurrentSubject, CurrentBody, CurrentPubCode,true);
                        }

                  }
                
            }
        }

    }
}