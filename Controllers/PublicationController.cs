using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Scheduling.Models;

namespace ProductionSchedule.Controllers
{
    public class PublicationController : Controller
    {

        public ActionResult AddPublication()
        {
            return View();
        }

        [HttpPost]
        public ActionResult EditPublication(FormCollection fc)
        {
            int PubID = Convert.ToInt32(fc["id"]);
            PublicationCode pc = Scheduling.Database.Utility.GetAllPublicationCodes(true).Where(x => x.ID == PubID).First();
            return View(pc);
        }

        public ActionResult ManagePublicationCodes()
        {
            List<PublicationCode> PcList = Scheduling.Database.Utility.GetAllPublicationCodes(true);
            return View(PcList);

        }

        [HttpPost]
        public ActionResult ManagePublicationSubItemReport(FormCollection fc)
        {
            int PubID = Convert.ToInt32(fc["id"]);
            ViewBag.PubID = PubID;
            return View();

        }

        [HttpPost]
        public ActionResult ProcessManagePublicationSubItemReport(FormCollection fc)
        {
            List<int> InputList = new List<int>();
            int NumItems = Convert.ToInt32(fc["NumItems"]);
            int PubID = Convert.ToInt32(fc["id"]);
            for (int i = 1; i <= NumItems; i++)
            {
                string ElementToCheck = string.Format("dropdown_{0}", i);
                string ElementVal = fc[ElementToCheck];
                if (!string.IsNullOrWhiteSpace(ElementVal))
                {
                    int CurrentInt = Convert.ToInt32(ElementVal);
                    InputList.Add(CurrentInt);
                }
            }

            Scheduling.Database.Utility.CreatePublicationSubItemReportingEntry(PubID, InputList);

            return RedirectToAction("Presentation", "Reporting");
        }

        [HttpPost]
        public ActionResult DeleteSinglePublication(FormCollection fc)
        {
            int PubID = Convert.ToInt32(fc["id"]);
            string CurrentPubLongDesc = Scheduling.Database.Utility.GetAllPublicationCodes(true).Where(i => i.ID == PubID).Select(s => s.LongDesc).First();

            string ComText = string.Format("delete from dbo.PublicationCode where id ={0};delete from dbo.MilestoneFieldMainSubItemsReportSorting where pubcodefk={1}", PubID,PubID);
            Scheduling.Database.Utility.ExecuteNonQueryWrapper(ComText);

            string Message = string.Format("deleting publication code {0} and report sorting configuration", CurrentPubLongDesc);

            Scheduling.Database.Utility.CreateActivityLogEntry(Message);
            return RedirectToAction("ManagePublicationCodes");
        }


        public ActionResult ProcessEditPublication(FormCollection fc)
        {
            PublicationCode pc = new PublicationCode();
            pc.ID = Convert.ToInt32(fc["id"]);
            //required fields

            if (string.IsNullOrWhiteSpace(fc["ShortDesc"])) ModelState.AddModelError("ShortDesc", "Short Desc Required");
            if (string.IsNullOrWhiteSpace(fc["LongDesc"])) ModelState.AddModelError("LongDesc", "Long Desc Required");
            if (string.IsNullOrWhiteSpace(fc["ReportDesc"])) ModelState.AddModelError("ReportDesc", "Report Desc Required");


            if (string.IsNullOrWhiteSpace(fc["ProfitCenter"]))
            {
                ModelState.AddModelError("ProfitCenter", "Profit Center Required");

            }

            else
            {
                int ProfitCenter = 0;
                string CurrentProfitCenter = fc["ProfitCenter"];
                bool ValidProfitCenter = Int32.TryParse(CurrentProfitCenter, out ProfitCenter);
                if (!ValidProfitCenter) ModelState.AddModelError("Profit Center", "Number Required For Profit Center");
                if (ValidProfitCenter) pc.ProfitCenter = Convert.ToInt32(fc["ProfitCenter"]);
            }

            if (ModelState.IsValid)
            {


                pc.ShortDesc = fc["ShortDesc"];
                pc.LongDesc = fc["LongDesc"];
                pc.ReportDesc = fc["ReportDesc"];

                //get checkboxes
                int IsActive = 0; if (fc["IsActive"].ToLower().Contains("true")) IsActive = 1;
                int IsAnnual = 0; if (fc["IsAnnual"].ToLower().Contains("true")) IsAnnual = 1;
                int HasCustomOffset = 0; if (fc["HasCustomOffset"].ToLower().Contains("true")) HasCustomOffset = 1;
                int ShowInNewsStand = 0; if (fc["ShowInNewsStand"].ToLower().Contains("true")) ShowInNewsStand = 1;

                pc.IsActive = IsActive;
                pc.IsAnnual = IsAnnual;
                pc.HasCustomOffset = HasCustomOffset;
                pc.ShowInNewsStandReport = ShowInNewsStand;

                //printer fk

                pc.PrinterFK = null;

                if (!string.IsNullOrWhiteSpace(fc["Printer"]))
                {
                    pc.PrinterFK = Convert.ToInt32(fc["Printer"]);

                }

                //parent pub
                pc.ParentPub = null;
                if (!string.IsNullOrWhiteSpace(fc["ParentPub"]))
                {
                    pc.ParentPub = Convert.ToInt32(fc["ParentPub"]);

                }

                Scheduling.Database.Utility.EditPublicationEntry(pc);

                //redirect to view publication
                return RedirectToAction("ManagePublicationCodes", "Publication");
            }

            else
            {
                return View("EditPublication");

            }


        }

        //I used add model error as model binding is a pain with checkbox values stored as ints in the system.
        public ActionResult ProcessAddPublication(FormCollection fc)
        {

            PublicationCode pc = new PublicationCode();

            //required fields

            if (string.IsNullOrWhiteSpace(fc["ShortDesc"])) ModelState.AddModelError("ShortDesc", "Short Desc Required");
            if (string.IsNullOrWhiteSpace(fc["LongDesc"])) ModelState.AddModelError("LongDesc", "Long Desc Required");
            if (string.IsNullOrWhiteSpace(fc["ReportDesc"])) ModelState.AddModelError("ReportDesc", "Report Desc Required");


            if (string.IsNullOrWhiteSpace(fc["ProfitCenter"]))
            {
                ModelState.AddModelError("ProfitCenter", "Profit Center Required");

            }

            else
            {
                int ProfitCenter = 0;
                string CurrentProfitCenter = fc["ProfitCenter"];
                bool ValidProfitCenter = Int32.TryParse(CurrentProfitCenter, out ProfitCenter);
                if (!ValidProfitCenter) ModelState.AddModelError("Profit Center", "Number Required For Profit Center");
                if (ValidProfitCenter) pc.ProfitCenter = Convert.ToInt32(fc["ProfitCenter"]);
            }

            if (ModelState.IsValid)
            {


                pc.ShortDesc = fc["ShortDesc"];
                pc.LongDesc = fc["LongDesc"];
                pc.ReportDesc = fc["ReportDesc"];

                //get checkboxes
                int IsActive = 0; if (fc["IsActive"].ToLower().Contains("true")) IsActive = 1;
                int IsAnnual = 0; if (fc["IsAnnual"].ToLower().Contains("true")) IsAnnual = 1;
                int HasCustomOffset = 0; if (fc["HasCustomOffset"].ToLower().Contains("true")) HasCustomOffset = 1;
                int ShowInNewsStand = 0; if (fc["ShowInNewsStand"].ToLower().Contains("true")) ShowInNewsStand = 1;

                pc.IsActive = IsActive;
                pc.IsAnnual = IsAnnual;
                pc.HasCustomOffset = HasCustomOffset;
                pc.ShowInNewsStandReport = ShowInNewsStand;

                //printer fk

                pc.PrinterFK = null;

                if (!string.IsNullOrWhiteSpace(fc["Printer"]))
                {
                    pc.PrinterFK = Convert.ToInt32(fc["Printer"]);

                }

                //parent pub
                pc.ParentPub = null;
                if (!string.IsNullOrWhiteSpace(fc["ParentPub"]))
                {
                    pc.ParentPub = Convert.ToInt32(fc["ParentPub"]);

                }

                Scheduling.Database.Utility.CreatePublicationEntry(pc);

                //redirect to view publication
                return RedirectToAction("ManagePublicationCodes", "Publication");
            }

            else
            {
                return View("AddPublication");

            }

        }
    }
}
