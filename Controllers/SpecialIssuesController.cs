using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Scheduling.Models;

namespace Scheduling.Controllers
{
    public class SpecialIssuesController : Controller
    {
        //
        // GET: /SpecialIssues/

        public ActionResult Index()
        {

            List<SpecialIssue> SipList=Scheduling.Database.Utility.GetAllSpecialIssues();
            ViewBag.YearList = Scheduling.Database.Utility.GetAllYears();
            ViewBag.PubcodeList = Scheduling.Database.Utility.GetAllPublicationCodes();
            return View(SipList);
        }

            
        // POST: /SpecialIssues/Create

        [HttpPost]
        public ActionResult Add(FormCollection fc)
        {

            string ErrorMessage = string.Empty;
            if (!string.IsNullOrEmpty(fc["pubcode"]) && !string.IsNullOrEmpty(fc["year"]) && !string.IsNullOrEmpty(fc["desc"]) && !string.IsNullOrEmpty(fc["nsdate"]))
            {

                SpecialIssue si = new SpecialIssue();
                si.ShortDesc = fc["desc"];
                si.LongDesc = fc["desc"];
                si.NewsstandDate = DateTime.Parse(fc["nsdate"]);
                si.PubCodeFK = Scheduling.Database.Utility.GetNullableIntValueFromDbField(fc["pubcode"]);
                si.YearFk =Convert.ToInt32(fc["year"]);
                Scheduling.Database.Utility.CreateSpecialIssueEntry(si);
                
            }

            else
            {
                ViewBag.ErrorMessage = "Requires Values in All Selected Fields";
               

            }

                List<SpecialIssue> SipList = Scheduling.Database.Utility.GetAllSpecialIssues();
                ViewBag.YearList = Scheduling.Database.Utility.GetAllYears();
                ViewBag.PubcodeList = Scheduling.Database.Utility.GetAllPublicationCodes();

                return View("Index", SipList);
        }
        
        //
        // GET: /SpecialIssues/Edit/5
 
        public ActionResult Edit(int id)
        {
            return View();
        }
        
 
        public ActionResult Delete(int id)
        {
            string ComText = string.Format("delete from [dbo].specialissues where id={0}", id);
            Scheduling.Database.Utility.ExecuteNonQueryWrapper(ComText);
            return RedirectToAction("Index");
        }

        //
        // POST: /SpecialIssues/Delete/5

      
    }
}
