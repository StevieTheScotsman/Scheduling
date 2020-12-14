using Scheduling.ActionFilter;
using Scheduling.Models;
using System;
using System.Collections.Generic;
using System.Json;
using System.Linq;
using System.Web.Mvc;

namespace Scheduling.Controllers
{
    public class AjaxController : Controller
    {

        //Mass Update of Baseline Based on Profile ID
        [HttpPost]

        public ActionResult RevertAllProjectsBasedOnBaselineID(int ID)
        {

            string RetStr = "0";

            try
            {
                List<ProjectDisplay> ProjList = Scheduling.Database.Utility.GetAllProjects().Where(x => x.MilestoneTreeSettingsProfileFK == ID).ToList();

                foreach (ProjectDisplay pd in ProjList)
                {
                    int NsID = Convert.ToInt32(Scheduling.StringFunctions.Utility.GetAppSettingValue("NewsstandOnSaleDateMilestoneValue"));

                    // get value of newstand field
                    List<MilestoneValue> MvList = Scheduling.Database.Utility.GetMilestoneValuesByProjectID(pd.ID);
                    string CurrentDueDate = MvList.Where(x => x.MilestoneFieldFK == NsID).First().DueDate;

                    int FieldRes = Scheduling.Database.Utility.ReCreateMilestoneFieldsFromProfileTableOnProjectReset(pd.ID, CurrentDueDate);

                    //Recreate values now we have the nodes

                    int ValueRes = Scheduling.Database.Utility.ReCreateMilestoneValuesOnProjectReset(pd.ID, CurrentDueDate);

                    string Message = string.Format("ReCreating MilestoneValues For Project {0} with id of {1}", pd.Name, pd.ID);
                    Scheduling.Database.Utility.CreateApplicationLoggingEntry(Message);
                    RetStr = "1";
                }

            }


            catch (Exception e)
            {

                Scheduling.Database.Utility.CreateApplicationErrorLoggingEntry(e.Message);

            }

            return Json(RetStr);

        }

        [HttpPost]
        public ActionResult GetAvailableLinkedProjectsBySecondaryProjectID(int ProjectLinkSettingID)
        {
            ProjectLinkSetting pls = Scheduling.Database.Utility.GetAllProjectLinkSettings().Where(x => x.ID == ProjectLinkSettingID).First();

            string RetString = Scheduling.Database.Utility.GetAvailableLinkedProjectsByProjectLinkSettingID(pls);

            return Json(RetString);

        }

        //Returns Profile Type Selection On CreateMilestoneSettings

        [HttpPost]
        public ActionResult GetProjectType(string input)
        {
            int i = Scheduling.Database.Utility.GetAllMilestoneTreeSettingsProfiles().Where(x => x.ID == Convert.ToInt32(input)).First().ProjectType;
            string s = Scheduling.Database.Utility.GetAllProjectProfileTypes().Where(x => x.ID == i).First().Description;
            return Json(new { id = i.ToString(), name = s });
        }


        [HttpPost]
        public ActionResult HasDependantFields(EditSingleMilestone esm)
        {
            string RetStr = "0";
            bool RetBool = Scheduling.Database.Utility.DoesMilestoneHaveDependencies(esm);
            if (RetBool) RetStr = "1";
            return Json(RetStr);
        }

        
        [HttpPost]
        public ActionResult AjaxRemoveProjectMilestone(EditSingleMilestone esm)
        {
            int CurrentFieldID = Convert.ToInt32(esm.MilestoneFieldID);
            int CurrentProjectID = Convert.ToInt32(esm.ProjectID);
            Scheduling.Database.Utility.DeleteSingleMilestoneValueEntry(CurrentFieldID, CurrentProjectID);
            string RetStr = "1";
            return Json(RetStr);
        }


        
        //We will walk the tree from the given node stopping when there is no dependencies.
        [HttpPost]
        public ActionResult AjaxUpdateKeepingDependants(EditSingleMilestoneWithDueDate esm)
        {
            string RetStr = "1";
            DateTime dt = Convert.ToDateTime(esm.DueDate);
            int CurrentID = Convert.ToInt32(esm.ProjectID);

            MilestoneFieldNodeDisplay mfnd = new MilestoneFieldNodeDisplay();

            mfnd.Day = dt.Day;
            mfnd.Month = dt.Month;
            mfnd.Year = dt.Year;
            mfnd.MilestoneField = Convert.ToInt32(esm.MilestoneFieldFK);

            //need to update current node
            Scheduling.Database.Utility.UpdateSelectedMilestoneValueDueDate(esm);

            EditProjectMilestoneFieldNodeDisplay epmfnd = new EditProjectMilestoneFieldNodeDisplay();
            epmfnd.ProjectID = Convert.ToInt32(esm.ProjectID);
            epmfnd.MilestoneFieldID = Convert.ToInt32(esm.MilestoneFieldFK);
            epmfnd.Day = dt.Day;
            epmfnd.Month = dt.Month;
            epmfnd.Year = dt.Year;

            List<EditProjectMilestoneFieldNodeDisplay> UpdateList = Scheduling.Database.Utility.GetMilestoneFieldNodesBasedOnStartingNodeForEditProject(epmfnd);

            //debug
            string UpdStr = string.Format("Update List has {0} Entries", UpdateList.Count);
            Scheduling.Database.Utility.CreateApplicationLoggingEntry(UpdStr);

            string ExcStr = string.Empty;
            foreach (EditProjectMilestoneFieldNodeDisplay item in UpdateList)
            {

                string CurrentDueDate = string.Format("{0}/{1}/{2}", item.Month, item.Day, item.Year);
                string CurrentStr = string.Format("update dbo.milestonevalue set DueDate='{0}' where projectpk={1} and milestonefieldfk={2};", CurrentDueDate, epmfnd.ProjectID, item.MilestoneFieldID);
                ExcStr += CurrentStr;


            }

            //debug
            UpdStr = string.Format("update str is {0}", ExcStr);
            Scheduling.Database.Utility.CreateApplicationLoggingEntry(UpdStr);

            Scheduling.Database.Utility.ExecuteNonQueryWrapper(ExcStr);
            string BaseNodeStr = Scheduling.Database.Utility.GetMilestoneDescFromID(epmfnd.MilestoneFieldID);
            string Comments = string.Format("Updating Dependant Nodes For {0} using date of {1}", BaseNodeStr, esm.DueDate);
            Scheduling.Database.Utility.CreateProjectHistoryEntry(epmfnd.ProjectID, Comments);

            return Json(RetStr);

        }


        //Create or update note field For Now Just Return 1.
        
        [HttpPost]
        public ActionResult AjaxUpdateNoteField(EditSingleNoteField esn)
        {
            Scheduling.Database.Utility.EditSingleNoteField(esn);
            return Json("1");
        }

        //Json Result Ignored at the moment ..returns check flag client side
        
        [HttpPost]
        public ActionResult AjaxUpdateBreakingDependants(EditSingleMilestoneWithDueDate esm)
        {
            Scheduling.Database.Utility.EditSingleMilestoneValueEntryAndRemoveDependencies(esm);
            return Json("Due Date Updated and Dependancies Removed");

        }
        
        [HttpPost]
        public ActionResult AjaxNoDependencySimpleUpdate(EditSingleMilestoneWithDueDate esm)
        {
            Scheduling.Database.Utility.EditSingleMilestoneValueEntry(esm);
            return Json("Due Date Updated");
        }


        public ActionResult SingleProjectHasDependantFields(EditSingleProjectMilestoneValueWithDueDate esp)
        {
            string RetStr = "0";
            //use existing functionality
            bool RetBool = Scheduling.Database.Utility.DoesSingleProjectMilestoneHaveDependencies(esp);
            if (RetBool) RetStr = "1";
            return Json(RetStr);
        }


        [HttpPost]
        //for simplicity return comma separated string.
        public ActionResult GetDependantFields(EditSingleMilestone esm)
        {
            string RetStr = string.Empty;
            List<MilestoneValue> MfList = Scheduling.Database.Utility.GetMilestoneValuesByProjectID(Convert.ToInt32(esm.ProjectID)).Where(x => x.DependantUpon == Convert.ToInt32(esm.MilestoneFieldID)).ToList();
            foreach (MilestoneValue mv in MfList)
            {
                RetStr += mv.MilestoneFieldFK.ToString() + ",";
            }

            RetStr = RetStr.Substring(0, RetStr.Length - 1);
            return Json(RetStr);

        }

        
        [OnNewsStandDateMultipleApprovalActionFilter]
        [HttpPost]
        public ActionResult UpdateReviewedProjects(string input)
        {
            string RetStr = "1";
            Scheduling.Database.Utility.UpdateReviewedProjects(input);
            return Json(RetStr);

        }
        
        [HttpPost]
        public ActionResult ProcessManageProjectsWithMultipleOnSaleDateApprovedStatus(string input)
        {

            string RetStr = "1";
            Scheduling.Database.Utility.UpdateProjectsToScheduleApproved(input);
            return Json(RetStr);

        }

        
        [HttpPost]
        public ActionResult UpdateEditProfileJson(FormCollection collection)
        {
            dynamic values = JsonValue.Parse(collection["values"]);

            string ExcStr = string.Empty;

            string CurrentProfileID = values[0].profileID;
            //Previously the sub items were getting whacked.
            string DelStr = string.Format("delete from MilestoneTreeSettings where MilestoneTreeSettingsProfileFK={0} and MilestoneFieldParentFK is null;", CurrentProfileID);

            for (int i = 0; i < values.Count; i++)
            {
                string InsStr = "insert into MilestoneTreeSettings (MilestoneFieldFK,MilestoneFieldParentFK,DependantUpon,CalculationID,CalcFiringOrder,RangeCalculationID,MilestoneTreeSettingsProfileFK,DisplayOrder) values (";
                InsStr += "#MilestoneFieldFK#,null,#DependantUpon#,#CalculationID#,#CalcFiringOrder#,null,#MilestoneTreeSettingsProfileFK#,#DisplayOrder#);";

                //mf 
                string MileFieldVal = values[i].mileFieldVal;
                InsStr = InsStr.Replace("#MilestoneFieldFK#", MileFieldVal);

                //dep field
                string DepFieldVal = values[i].depFieldVal;
                InsStr = InsStr.Replace("#DependantUpon#", DepFieldVal);

                //calc field
                string CalcFieldVal = values[i].calcFieldVal;
                InsStr = InsStr.Replace("#CalculationID#", CalcFieldVal);

                //fir order field
                string FirOrderFieldVal = values[i].firOrderFieldVal;
                InsStr = InsStr.Replace("#CalcFiringOrder#", FirOrderFieldVal);

                //display order
                string DisplayFieldVal = values[i].displayOrderFieldVal;
                InsStr = InsStr.Replace("#DisplayOrder#", DisplayFieldVal);

                //profile id

                InsStr = InsStr.Replace("#MilestoneTreeSettingsProfileFK#", CurrentProfileID);

                ExcStr = ExcStr + InsStr;

            }

            string FinalStr = DelStr + ExcStr;

            Scheduling.Database.Utility.ExecuteNonQueryWrapper(FinalStr);
            return new EmptyResult();
        }


    }
}
