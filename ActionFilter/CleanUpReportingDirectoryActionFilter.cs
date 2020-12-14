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
    public class CleanUpReportingDirectoryActionFilter : ActionFilterAttribute
    {
        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {   string CsvExportDir=Scheduling.StringFunctions.Utility.GetAppSettingValue("CsvExportDirectory");
            string RemovalAge = Scheduling.StringFunctions.Utility.GetAppSettingValue("CsvFilesRemovalAge");
            string DirName = string.Format("{0}{1}{2}", HttpContext.Current.Request.PhysicalApplicationPath,Path.DirectorySeparatorChar, CsvExportDir);

            string[] files = Directory.GetFiles(DirName);
            int Parameter = Convert.ToInt32(RemovalAge) * -1;

            foreach (string s in  files)
            {

                FileInfo fi = new FileInfo(s);
                if (fi.LastAccessTime < DateTime.Now.AddDays(Parameter))
                    fi.Delete();

            }
 
        }

    }
}