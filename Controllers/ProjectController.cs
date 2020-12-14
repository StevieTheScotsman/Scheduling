using Scheduling.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace ProductionSchedule.Controllers
{
    public class ProjectController : Controller
    {

        public ActionResult ManageSingleProject(int id = 0)
        {

            int i = id;
            if (!String.IsNullOrWhiteSpace(Request["id"]))
            {

                bool ValidQueryStr = Int32.TryParse(Request["id"], out i);
                if (ValidQueryStr) i = Convert.ToInt32(Request["id"]);

            }

            List<ProjectDisplay> ProdDisplayList = new List<ProjectDisplay>();

            if (i > 0)
            {
                ProdDisplayList = Scheduling.Database.Utility.GetAllProjects().Where(x => x.ID == i).ToList();
            }

            //model
            EditProjectWithMilestones CurrentProject = new EditProjectWithMilestones();

            if (ProdDisplayList.Count == 1)
            {

                ProjectDisplay p = ProdDisplayList.First();


                CurrentProject.ID = p.ID;
                CurrentProject.Name = p.Name;
                CurrentProject.Year =
                  Scheduling.Database.Utility.GetAllYears().Where(x => x.ID == p.YearFK).First().Value.ToString();
                CurrentProject.DateCreated = p.DateCreated.ToLongDateString();
                CurrentProject.CurrentVersion = p.CurrentVersion;
                CurrentProject.CurrentProjectStatus = p.CurrentProjectStatus;
                CurrentProject.IsLocked = p.IsLocked;

                //Get the milestones
                List<MilestoneValue> MilValList = Scheduling.Database.Utility.GetMilestoneValuesByProjectID(p.ID);
                CurrentProject.MileValueList = MilValList;

            }



            return View(CurrentProject);
        }

        [HttpPost]
        public ActionResult AjaxRemoveMilestone(EditSingleMilestone esm)
        {
            int CurrentFieldID = Convert.ToInt32(esm.MilestoneFieldID);
            int CurrentProjectID = Convert.ToInt32(esm.ProjectID);
            Scheduling.Database.Utility.DeleteSingleMilestoneValueEntry(CurrentFieldID, CurrentProjectID);
            string RetStr = "1";
            return Json(RetStr);
        }

        [HttpPost]
        public ActionResult AjaxAddMilestone(AddSingleMilestone asm)
        {
          //debug
          Scheduling.Database.Utility.CreateApplicationLoggingEntry("dep is" + asm.DependencyID);
          Scheduling.Database.Utility.CreateApplicationLoggingEntry("calc is" + asm.CalculationID);


            string CurrentMessage = string.Empty;
            int CurrentProject = Convert.ToInt32(asm.ProjectID);


            if (string.IsNullOrWhiteSpace(asm.MilestoneFieldID))
            {

                CurrentMessage += "Milestone Needs To Be Selected !";
                return Json(new { success = 0, message = CurrentMessage });
            }

            string CurrentMilestone = asm.MilestoneFieldID;

            //check parent is not the same as field
            if (!string.IsNullOrWhiteSpace(asm.MilestoneParentID))
            {

                string CurrentParent = asm.MilestoneParentID;
                if (CurrentParent == CurrentMilestone)
                {
                    CurrentMessage = "Error Milestone Cannot Be The Same As The Parent !";
                    return Json(new { success = 0, message = CurrentMessage });

                }


            }

            //ensure if we have a dependency or calculation we have both
            bool HasDependency = !string.IsNullOrWhiteSpace(asm.DependencyID);
            bool HasCalculation = !string.IsNullOrWhiteSpace(asm.CalculationID);

            if (HasDependency || HasCalculation)
            {
                bool HasBoth = HasDependency && HasCalculation;
                if (!HasBoth)
                {
                    CurrentMessage = "Dependency and Calculation are Both Required !";
                    return Json(new { success = 0, message = CurrentMessage });

                }

            }


            //Get max display 

            List<Scheduling.Models.MilestoneValue> MilestoneValueList = Scheduling.Database.Utility.GetMilestoneValuesByProjectID(CurrentProject);

            string MaxDisplayValStr = string.Empty;
            int? MaxDisVal = MilestoneValueList.Select(x => x.DisplaySortOrder).Max();
            if (MaxDisVal.HasValue)
            {
                MaxDisplayValStr = (MaxDisVal.Value + 5).ToString();
            }


            Scheduling.Database.Utility.CreateMilestoneValueEntryForExistingProject(CurrentProject, Convert.ToInt32(CurrentMilestone), asm.MilestoneParentID, asm.DependencyID, asm.CalculationID, MaxDisplayValStr);
            string CurrentFieldName = Scheduling.Database.Utility.GetAllMilestoneFields().Where(x => x.ID == Convert.ToInt32(CurrentMilestone)).First().Description;
            return Json(new { success = 1, message = "Success" });


        }

        public ActionResult RenameProject(FormCollection fc)
        {
            int CurrentProjectID = Convert.ToInt32(fc["ProjectID"]);
            string ProjectName = fc["ProjectName"];

            Scheduling.Database.Utility.RenameProject(CurrentProjectID, ProjectName);
            string BaseUrl = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/";
            
            string RetUrl = string.Format("{0}project/managesingleproject/{1}", BaseUrl,CurrentProjectID);
            return Redirect(RetUrl);
        }

        //used on new ui screen..same code different calling code..we now it is a magazine type as we can get here
        public ActionResult RevertToBaseline(FormCollection fc)
        {

            int CurrentProjectID = Convert.ToInt32(fc["ProjectID"]);
            int NsID = Convert.ToInt32(Scheduling.StringFunctions.Utility.GetAppSettingValue("NewsstandOnSaleDateMilestoneValue"));

            //get value of newstand field
            List<MilestoneValue> MvList = Scheduling.Database.Utility.GetMilestoneValuesByProjectID(CurrentProjectID);
            string CurrentDueDate = MvList.Where(x => x.MilestoneFieldFK == NsID).First().DueDate;


            //Recreate fields and populate due date.

            int FieldRes = Scheduling.Database.Utility.ReCreateMilestoneFieldsFromProfileTableOnProjectReset(CurrentProjectID, CurrentDueDate);

            //Recreate values now we have the nodes 

            int ValueRes = Scheduling.Database.Utility.ReCreateMilestoneValuesOnProjectReset(CurrentProjectID, CurrentDueDate);

            string BaseUrl = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/";
            string RetUrl = string.Format("{0}project/managesingleproject/{1}", BaseUrl, CurrentProjectID);
            return Redirect(RetUrl);

        }

    }


}
