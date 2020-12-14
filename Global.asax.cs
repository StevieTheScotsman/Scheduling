using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Data.Common;
using System.Web.Optimization;
using System.Net.Mail;


namespace Scheduling
{
    //Can be changed and application restarted.
    public class Enums : System.Web.HttpApplication
    {
        //Project Range
        public enum Timeline { JAN = 1, FEB = 2, MAR = 3, APR = 4, MAY = 5, JUN = 6, JUL = 7, AUG = 8, SEP = 9, OCT = 10, NOV = 11, DEC = 12 }
        public enum Role { User = 1, Approver, Admin, SuperAdmin };

        public enum ChangeRequestStatus { Created = 1, Accepted, Rejected };

        public enum ProjectStatus { Created = 1, OnSaleDateInReview, OnSaleDateRejected, OnSaleDateApproved, ScheduleCreated, PendingChanges, PendingReview, Approved, ApprovedPendingReview };

        public enum BaselineType { Magazine = 1, Distribution, Digital };

        public enum MagazineNoteLabel { QuadJobNumber = 1, KPCJobNumber, NextIssueAndCrossPromo };
    }

}



namespace Scheduling
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    // http://colinmackay.co.uk/2011/05/02/custom-error-pages-and-error-handling-in-asp-net-mvc-3-2/

    public class MvcApplication : System.Web.HttpApplication
    {
        public override string GetVaryByCustomString(HttpContext context, string arg)
        {
            if (arg == "cachetimestamp")
            {
                return Scheduling.FileAccess.Utility.ReadCacheFile();

            }

            return base.GetVaryByCustomString(context, arg);
        }

        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            //remove filter 12/9/2014 but still incldue LogExceptionFilterAttribute

            filters.Add(new LogExceptionFilterAttribute());
            //filters.Add(new HandleErrorAttribute());
        }

        public class LogExceptionFilterAttribute : IExceptionFilter
        {
            public void OnException(ExceptionContext filterContext)
            {
                if (filterContext.Exception != null)
                {
                    //get controller info
                    string ControllerName = filterContext.RouteData.Values["controller"].ToString();
                    string ActionName = filterContext.RouteData.Values["action"].ToString();
                    string Message = string.Format("Message is {2}  Action ={0}  Controller is {1}", ActionName, ControllerName, filterContext.Exception.Message);
                    //Ensure insert statement works ok. SB2
                    Message = Message.Replace("'", string.Empty);
                    Scheduling.Database.Utility.CreateApplicationErrorLoggingEntry(Message);
                }
            }
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );
            
            //Version 3.2 Steve 1.5.2015
            routes.MapRoute(
                 "Project", // Route name
                 "Project/ManageSingleProject/{id}", // URL with parameters
                 new { controller = "Project", action = "ManageSingleProject"} // Parameter defaults
             );


        }


        protected void Application_Start()
        {
            //license
            EO.Pdf.Runtime.AddLicense(
            "/KXg5/YZ8p61kZt14+30EO2s3MKetZ9Zl6TNF+ic3PIEEMidtbrC3LVtqbTB" +
            "3rF1pvD6DuSn6unaD71GgaSxy5914+30EO2s3OnP566l4Of2GfKe3MKetZ9Z" +
            "l6TNDOul5vvPuIlZl6Sxy59Zl8DyD+NZ6/0BELxbvNO/++OfmaQHEPGs4PP/" +
            "6KFupbSzy653hI6xy59Zs7PyF+uo7sKetZ9Zl6TNGvGd3PbaGeWol+jyH+R2" +
            "mbjA3bJoqbTC36FZ7ekDHuio5cGz4KFZpsKetZ9Zl6TNHuig5eUFIPGetev7" +
            "HMeN6+j1I7KMp+7l78WbvscEG+Z2tMDAHuig5eUFIPGetZGb564=");


            AreaRegistration.RegisterAllAreas();
            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);

        }


    }
}