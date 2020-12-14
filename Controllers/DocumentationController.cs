using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Scheduling.Models;

namespace Scheduling.Controllers
{
    public class DocumentationController : Controller
    {
        public ActionResult AppSettings()
        {
            return View();
        }

        public ActionResult DeveloperNotes()
        {
            return View();
        }


        public ActionResult UserInformation()
        {
            return View();
        }

    }
}
