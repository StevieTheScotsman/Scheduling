using System.Web.Mvc;

namespace Scheduling.Controllers
{
    public class ErrorController : Controller
    {
        
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult NotFound()
        {
            return View();

        }

    }
}
