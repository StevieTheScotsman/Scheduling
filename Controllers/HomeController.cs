using Scheduling.ActionFilter;
using Scheduling.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;

namespace Scheduling.Controllers
{

    public class HomeController : Controller
    {
        //called by listmanagedprojectsaddremovemilestones.cshtml
        
        public ActionResult AddMilestoneToManageSingleProject(FormCollection fc)
        {

            string CurrentMessage = string.Empty;
            int CurrentProject = Convert.ToInt32(fc["ProjectID"]);
            string DisplayOrder = fc["display"].ToString();

            if (string.IsNullOrWhiteSpace(fc["field"]))
            {

                CurrentMessage += "Milestone Needs To Be Selected !";
                return RedirectToAction("ManageSingleProjectAfterDependencyAjaxCall", new { id = CurrentProject, message = CurrentMessage });
            }

            string CurrentMilestone = fc["field"];

            //check parent is not the same as field
            if (!string.IsNullOrWhiteSpace(fc["parent"]))
            {

                string CurrentParent = fc["parent"];
                if (CurrentParent == CurrentMilestone)
                {
                    CurrentMessage = "Error Milestone Cannot Be The Same As The Parent !";
                    return RedirectToAction("ManageSingleProjectAfterDependencyAjaxCall", new { id = CurrentProject, message = CurrentMessage });

                }


            }

            //ensure if we have a dependency or calculation we have both
            bool HasDependency = !string.IsNullOrWhiteSpace(fc["dependency"]);
            bool HasCalculation = !string.IsNullOrWhiteSpace(fc["calculation"]);

            if (HasDependency || HasCalculation)
            {
                bool HasBoth = HasDependency && HasCalculation;
                if (!HasBoth)
                {
                    CurrentMessage = "Dependency and Calculation are Both Required !";
                    return RedirectToAction("ManageSingleProjectAfterDependencyAjaxCall", new { id = CurrentProject, message = CurrentMessage });

                }

            }


            //we are good save entry
            Scheduling.Database.Utility.CreateMilestoneValueEntryForExistingProject(CurrentProject, Convert.ToInt32(CurrentMilestone), fc["parent"], fc["dependency"], fc["calculation"], DisplayOrder);
            string CurrentFieldName = Scheduling.Database.Utility.GetAllMilestoneFields().Where(x => x.ID == Convert.ToInt32(CurrentMilestone)).First().Description;
            return RedirectToAction("ManageSingleProjectAfterDependencyAjaxCall", new { id = CurrentProject, message = string.Format("Success..Added {0}", CurrentFieldName) });


        }


        //advanced editing has attached view
        public ActionResult AdvancedManageSingleProject(FormCollection fc)
        {
            int i = Convert.ToInt32(fc[0]);
            //Project Ref
            ViewBag.MilValList = Scheduling.Database.Utility.GetMilestoneValuesByProjectID(i);

            //Mil Val Ref
            ViewBag.Project = Scheduling.Database.Utility.GetAllProjects().Where(x => x.ID == i).First();

            //View Bag For mfl
            ViewBag.MilestoneFieldList = Scheduling.Database.Utility.GetAllParentMilestoneFields();

            //calc list 
            ViewBag.CalcList = Scheduling.Database.Utility.GetAllCalculationFields();

            //firing order list
            ViewBag.FiringOrderList = Scheduling.Database.Utility.CreateFiringOrderList();

            //date range calculation
            ViewBag.DateRangeCalcList = Scheduling.Database.Utility.GetAllCalculationFields();

            //Newstand Value
            ViewBag.NewstandValue = Scheduling.StringFunctions.Utility.GetAppSettingValue("NewsstandOnSaleDateMilestoneValue");

            return View();

        }

        //has attached view
        public ActionResult AddSingleMilestoneTreeSettingProfile()
        {
            ViewBag.ProfileTypes = Scheduling.Database.Utility.GetAllProjectProfileTypes();
            return View();

        }

        

        //rather than use a model just grab the entry and go right to the view. used by manage holidays has attached view
        
        public ActionResult ProcessAddHoliday(FormCollection fc)
        {
            bool ValidEntry = false;
            string CurrentDate = fc["holiday"];
            if (!string.IsNullOrWhiteSpace(CurrentDate))
            {
                DateTime dt;
                bool ValidDate = DateTime.TryParse(CurrentDate, out dt);

                if (ValidDate)
                {
                    Scheduling.Database.Utility.CreateHolidayEntry(dt);
                    ValidEntry = true;
                }

            }

            if (!ValidEntry)
            {
                ModelState.AddModelError("Holiday", "Invalid Due Date Needs Format mm/dd/yyyy");
            }


            List<DateTime> AllHolidays = Scheduling.Database.Utility.GetAllHolidays().OrderByDescending(x => x.Year).ThenBy(y => y.Month).ThenBy(z => z.Day).ToList();

            List<Year> YearList = Scheduling.Database.Utility.GetAllYears().OrderByDescending(x => x.Value).ToList();
            List<int> DistinctYears = YearList.Select(x => x.ID).Distinct().ToList();

            ViewBag.AllHolidays = AllHolidays;
            ViewBag.DistinctYears = DistinctYears;

            return View("ManageHolidays");


        }

        //Has corresponding view
        
        public ActionResult ProcessAddMajorMilestoneField(FormCollection fc)
        {

            if (string.IsNullOrWhiteSpace(fc["field"]))
            {
                ModelState.AddModelError("milestone", "Required Field !");

            }

            else
            {
                Scheduling.Database.Utility.CreateMajorMilestoneEntry(fc["field"]);

            }

            List<MilestoneField> MfList = Scheduling.Database.Utility.GetAllMilestoneFields().Where(x => x.IsCreatedByUser == null).ToList();
            return View("ManageMajorMilestones", MfList);
        }


        //Used in AddSingleMilestoneTreeSettingProfile view
        
        public ActionResult ProcessAddSingleMilestoneTreeSettingProfile(MilestoneTreeSettingsProfile mtsp)
        {
            if (ModelState.IsValid)
            {

                Scheduling.Database.Utility.CreateNewMilestoneProfile(mtsp.Description, mtsp.ProjectType);
                return RedirectToAction("ListMilestoneTreeSettingProfiles");

            }

            else
            {
                ViewBag.ProfileTypes = Scheduling.Database.Utility.GetAllProjectProfileTypes();
                return View("AddSingleMilestoneTreeSettingProfile");
            }



        }

        //has corresponding view
        public ActionResult CreateMilestoneSettings()
        {
            //Num of initial miletones .Default is 8
            ViewBag.NumOfMilestoneSettings = Scheduling.StringFunctions.Utility.GetAppSettingValue("NumOfDefaultMilestones");

            //viewbag profile type

            ViewBag.MilProfileList = Scheduling.Database.Utility.GetAllUnAssignedMilestoneTreeSettingsProfiles();

            //View Bag For mfl
            ViewBag.MilestoneFieldList = Scheduling.Database.Utility.GetAllParentMilestoneFields();

            //calc list 
            ViewBag.CalcList = Scheduling.Database.Utility.GetAllCalculationFields();

            //firing order list
            ViewBag.FiringOrderList = Scheduling.Database.Utility.CreateFiringOrderList();

            //date range calculation
            ViewBag.DateRangeCalcList = Scheduling.Database.Utility.GetAllCalculationFields();

            //ns milestone value hidden field

            ViewBag.NewstandValue = Scheduling.StringFunctions.Utility.GetAppSettingValue("NewsstandOnSaleDateMilestoneValue");

            //mag Proj Type hidden field 
            ViewBag.MagProjTypeValue = Scheduling.StringFunctions.Utility.GetAppSettingValue("MagazineProjectType");

            //default milestones
            ViewBag.DefaultMilestoneValue = Scheduling.StringFunctions.Utility.GetAppSettingValue("NumOfDefaultMilestones");

            return View();
        }


        //We will disable the input validation and we will handle it by using the remove html method.
        //used in create change request view ..functionality currently not used by users.
        [ValidateInput(false)]
        [OnCreateChangeRequestActionFilter]
        
        public ActionResult ProcessChangeRequest(FormCollection fc)
        {
            string CurrentComment = fc["RequestorComment"];
            if (!string.IsNullOrEmpty(CurrentComment))
            {
                CurrentComment = Scheduling.StringFunctions.Utility.RemoveHtml(CurrentComment);
                string CurrentProjectID = fc["ProjectID"];
                Scheduling.Database.Utility.CreateProjectChangeRequest(CurrentComment, CurrentProjectID);
            }

            return RedirectToAction("ListChangeRequests");


        }

        
        //currently fat controller ..should really be thinned down..used in create milestone settings view

        public ActionResult ProcessCreateMilestoneSettings(FormCollection fc)
        {
            //ensure hidden field set to y..client val is ok..
            bool CanContinue = false;
            string HfProcessRes = fc["hf-can-submit"];
            int MilestoneProfile = Convert.ToInt32(fc["hf-milestone-profile"]);
            string FirstFieldValue = fc["hf-first-field-value"];


            if (HfProcessRes == "y" && MilestoneProfile > 0) CanContinue = true;
            if (CanContinue)
            {
                int NumRecords = Convert.ToInt32(fc["hf-num-records"]);


                string ExcStr = string.Empty;
                string DelStr = string.Format("delete from milestonetreesettings where milestonetreesettingsprofileFK={0};", MilestoneProfile);

                for (int i = 1; i <= NumRecords; i++)
                {
                    string MfVal = string.Empty;
                    string MfCol = string.Format("MilestoneField-{0}", i);

                    if (i == 1 && !string.IsNullOrEmpty(FirstFieldValue))
                    {
                        MfVal = FirstFieldValue;

                    }

                    else
                    {
                        MfVal = fc[MfCol];

                    }

                    //milestone field cant be null
                    string NullStr = "null";

                    //get milestone parent 
                    string ParCol = string.Format("ParentField-{0}", i);
                    string ParVal = NullStr;
                    if (!string.IsNullOrWhiteSpace(fc[ParCol])) ParVal = fc[ParCol];

                    //get dependancy 
                    string DepCol = string.Format("DepField-{0}", i);
                    string DepVal = NullStr;
                    if (!string.IsNullOrWhiteSpace(fc[DepCol])) DepVal = fc[DepCol];

                    //get calculation
                    string CalcCol = string.Format("CalcField-{0}", i);
                    string CalcVal = NullStr;
                    if (!string.IsNullOrWhiteSpace(fc[CalcCol])) CalcVal = fc[CalcCol];

                    //get calcfiring order
                    string FirOrderCol = string.Format("FirOrderField-{0}", i);
                    string FirOrderVal = NullStr;
                    if (!string.IsNullOrWhiteSpace(fc[FirOrderCol])) FirOrderVal = fc[FirOrderCol];

                    //get range calc
                    string RangeCalcCol = string.Format("RangeField-{0}", i);
                    string RangeCalcVal = NullStr;
                    if (!string.IsNullOrWhiteSpace(fc[RangeCalcCol])) RangeCalcVal = fc[RangeCalcCol];

                    string CurrentExcStr = "insert into milestonetreesettings (MilestoneFieldFK,MilestoneFieldParentFK,DependantUpon,CalculationID,CalcFiringOrder,RangeCalculationID,MilestoneTreeSettingsProfileFK) values(#MilestoneFieldFK#,#MilestoneFieldParentFK#,#DependantUpon#,#CalculationID#,#CalcFiringOrder#,#RangeCalculationID#,#MilestoneTreeSettingsProfile#);";
                    CurrentExcStr = CurrentExcStr.Replace("#MilestoneFieldFK#", MfVal);
                    CurrentExcStr = CurrentExcStr.Replace("#MilestoneFieldParentFK#", ParVal);
                    CurrentExcStr = CurrentExcStr.Replace("#DependantUpon#", DepVal);
                    CurrentExcStr = CurrentExcStr.Replace("#CalculationID#", CalcVal);
                    CurrentExcStr = CurrentExcStr.Replace("#CalcFiringOrder#", FirOrderVal);
                    CurrentExcStr = CurrentExcStr.Replace("#RangeCalculationID#", RangeCalcVal);
                    CurrentExcStr = CurrentExcStr.Replace("#MilestoneTreeSettingsProfile#", MilestoneProfile.ToString());

                    ExcStr += CurrentExcStr;
                }

                string FinalExcStr = string.Concat(DelStr, ExcStr);
                Scheduling.Database.Utility.ExecuteNonQueryWrapper(FinalExcStr);
                string CurrentProfile = Scheduling.Database.Utility.GetAllMilestoneTreeSettingsProfiles().Where(x => x.ID == MilestoneProfile).First().Description;
                Scheduling.Database.Utility.CreateActivityLogEntry(string.Format("Creating Milestone Set up for Profile.. {0}", CurrentProfile));
                return RedirectToAction("ListMilestoneTreeSettingProfiles");


            }

            else
            {
                return RedirectToAction("CreateMilestoneSettings");

            }

        }


        //used in manage milestone tree setting profiles view
        
        public ActionResult DeleteSingleMilestoneTreeProfile(FormCollection fc)
        {

            int ID = Convert.ToInt32(fc["id"]);
            Scheduling.Database.Utility.DeleteSingleMilestoneTreeProfileSetting(ID);
            return RedirectToAction("ManageMilestoneTreeSettingProfiles");

        }

        
        //link id and secondary project is one to one mapping called from project only deletes link does not affect current dates...used by manage single project in home views
        //keep for future linking module work
        public ActionResult DeleteSingleProjectLinkFromManagedKeepingValues(FormCollection fc)
        {
            int CurrentProject = Convert.ToInt32(fc["projectid"]);
            Scheduling.Database.Utility.DeleteSingleProjectLinkEntryFromManagedKeepingValues(CurrentProject);

            return RedirectToAction("ManageSingleProjectAfterDependencyAjaxCall", new { id = CurrentProject });
        }


        
        //used in manage single project notes
        public ActionResult DeleteSingleProjectNote(FormCollection fc)
        {
            int ID = Convert.ToInt32(fc["id"]);
            int CurrentProject = Convert.ToInt32(fc["ProjectFk"]);

            Scheduling.Database.Utility.DeleteSingleNoteEntry(ID, CurrentProject);

            ProjectDisplay pd = Scheduling.Database.Utility.GetAllProjects().Where(x => x.ID == CurrentProject).First();
            ViewBag.ProjectDisplay = pd;

            List<ProjectNote> NoteList = Scheduling.Database.Utility.GetProjectNotesFromProjectID(CurrentProject);
            return View("ManageSingleProjectNotes", NoteList);



        }



        //used in security partial view 
        public ActionResult DeleteAllActivity()
        {
            Scheduling.Database.Utility.DeleteAllActivity();
            return RedirectToAction("ListActivities");

        }

        //used in manage groups
        public ActionResult DeleteGroupEntry(FormCollection fc)
        {
            int GroupID = Convert.ToInt32(fc["id"]);
            Scheduling.Database.Utility.DeleteSingleGroupEntry(GroupID);
            return RedirectToAction("ManageGroups");

        }

        //has corresponding view
        public ActionResult ResetHistory(FormCollection fc)
        {
            int CurrentProject = Convert.ToInt32(fc["id"]);
            Scheduling.Database.Utility.DeleteProjectHistoryEntriesAfterInitialCreation(CurrentProject);

            return RedirectToAction("ResetProjects", "Home");
        }


        //used in remove change requests view
        public ActionResult ProcessRemoveChangeRequestsSingleItem(FormCollection fc)
        {

            int CurrentItem = Convert.ToInt32(fc["id"]);
            int CurrentProject = Convert.ToInt32(fc["ProjectID"]);
            Scheduling.Database.Utility.DeleteSingleRequestEntry(CurrentItem, CurrentProject);
            return RedirectToAction("RemoveChangeRequests");

        }

        //used in remove change requestes view
        public ActionResult RemoveChangeRequests()
        {

            List<ChangeRequest> CrList = Scheduling.Database.Utility.GetAllChangeRequests().OrderByDescending(x => x.ProjectFK).ThenByDescending(x => x.DateRequested).ToList();
            ViewBag.RequestStatusList = Scheduling.Database.Utility.GetAllChangeRequestStatuses();
            return View(CrList);

        }

        //has corresponding view

        
        public ActionResult ResetProjects(FormCollection fc)
        {
            if (!string.IsNullOrWhiteSpace(fc["id"]))
            {
                int ProjectID = Convert.ToInt32(fc["id"]);
                Scheduling.Database.Utility.DeleteProjectAndAllProjectInformationByID(ProjectID);

            }

            List<ProjectDisplay> ProdDisplayList = Scheduling.Database.Utility.GetAllProjects();
            List<ReadOnlyProjectWithMilestones> RopList = new List<ReadOnlyProjectWithMilestones>();

            if (!string.IsNullOrEmpty(fc["timeline"]))
            {
                int CurrentTimeline = Convert.ToInt32(fc["timeline"]);
                ProdDisplayList = ProdDisplayList.Where(x => x.ProjectRangeFK == CurrentTimeline).ToList();
                ViewBag.CurrentTimeline = CurrentTimeline;
            }

            if (!string.IsNullOrEmpty(fc["pubcode"]))
            {
                int CurrentPubcode = Convert.ToInt32(fc["pubcode"]);
                ProdDisplayList = ProdDisplayList.Where(y => y.PubCodeFK == CurrentPubcode).ToList();
                ViewBag.CurrentPubcode = CurrentPubcode;
            }

            if (!string.IsNullOrEmpty(fc["year"]))
            {
                int CurrentYear = Convert.ToInt32(fc["year"]);
                ProdDisplayList = ProdDisplayList.Where(z => z.YearFK == CurrentYear).ToList();
                ViewBag.CurrentYear = CurrentYear;
            }



            if (ProdDisplayList.Count > 0)
            {


                foreach (ProjectDisplay p in ProdDisplayList)
                {

                    ReadOnlyProjectWithMilestones Rop = new ReadOnlyProjectWithMilestones();
                    Rop.Comments = p.Comments;
                    Rop.DateCreated = p.DateCreated.ToShortDateString();
                    Rop.CreatedBy = p.CreatedBy;
                    Rop.ID = p.ID;
                    Rop.ProfileDesc = Scheduling.Database.Utility.GetAllMilestoneTreeSettingsProfiles().Where(x => x.ID == p.MilestoneTreeSettingsProfileFK).First().Description;

                    string CurrentProduct = "N/A";
                    if (p.ProductFK.HasValue) CurrentProduct = Scheduling.Database.Utility.GetAllProducts().Where(x => x.ID == p.ProductFK).First().Description;

                    //assign Product
                    Rop.ProductDesc = CurrentProduct;

                    //timeline
                    string CurrentTimeline = "N/A";
                    if (p.ProjectRangeFK.HasValue)
                    {
                        CurrentTimeline = Scheduling.Database.Utility.GetAllProjectRanges().Where(x => x.ID == p.ProjectRangeFK).First().ShortDesc;
                    }

                    Rop.Timeline = CurrentTimeline;

                    //assign Pubcode

                    Rop.PubCodeDesc = p.PubCodeFK.HasValue ? Scheduling.Database.Utility.GetAllPublicationCodes().Where(x => x.ID == p.PubCodeFK).First().ShortDesc : "N/A";

                    //assign Year
                    Rop.Year = Scheduling.Database.Utility.GetAllYears().Where(x => x.ID == p.YearFK).First().Value.ToString();

                    //assign Name 
                    Rop.Name = p.Name;
                    Rop.NewstandDate = Scheduling.Database.Utility.GetNewstandDateForListedProject(p.ID);

                    //assign status
                    Rop.CurrentProjectStatus = Scheduling.Database.Utility.GetAllProjectStatuses().Where(x => x.ID == p.CurrentProjectStatus).First().Description;

                    RopList.Add(Rop);
                }

            }
            return View(RopList);


        }


        //main route..General landing page
        public ActionResult Index()
        {

            return View();

        }

        //has corresponding view
        public ActionResult ManageProjectsWithOnSaleDateApprovedStatus()
        {
            List<ProjectDisplay> ProjList = Scheduling.Database.Utility.GetProjectsWithOnSaleDateApprovedStatus();
            ViewBag.MilestoneFieldID = ConfigurationManager.AppSettings["NewsstandOnSaleDateMilestoneValue"];
            return View(ProjList);
        }

        //has corresponding view
        
        public ActionResult ProcessManageProjectsWithOnSaleDateApprovedStatus(FormCollection fc)
        {
            int CurrentProject = Convert.ToInt32(fc["ProjectID"]);
            int ApprovedStatusID = Convert.ToInt32(Scheduling.StringFunctions.Utility.GetAppSettingValue("ProjectStatusScheduleCreatedID"));
            Scheduling.Database.Utility.UpdateProjectStatus(CurrentProject, ApprovedStatusID);
            return RedirectToAction("ManageProjectsWithOnSaleDateApprovedStatus");
        }


        //used in manage change requests
        
        public ActionResult ProcessManageChangeRequestSingleItem(FormCollection fc)
        {
            int CurrentItemID = Convert.ToInt32(fc["id"]);
            int CurrentProjectID = Convert.ToInt32(fc["ProjectFK"]);

            string RadioControl = string.Format("RequestStatus-{0}", CurrentItemID);
            int CurrentStatus = Convert.ToInt32(fc[RadioControl]);

            Scheduling.Database.Utility.EditSingleChangeRequestEntry(CurrentItemID, CurrentProjectID, CurrentStatus);
            return RedirectToAction("ManageChangeRequests");

        }

        //has corresponding view

        public ActionResult CloneSingleMilestoneTreeProfile(FormCollection fc)
        {
            int i = Convert.ToInt32(fc[0]);
            MilestoneTreeSettingsProfile ep = Scheduling.Database.Utility.GetAllMilestoneTreeSettingsProfiles().Where(x => x.ID == i).First();
            ViewBag.ExistingProfile = ep;
            return View();


        }

        
        //used in Clone Single Milestone Tree Profile 
        public ActionResult ProcessCloneSingleMilestoneTreeProfile(MilestoneTreeSettingsProfile mp, FormCollection fc)
        {
            int SourceID = Convert.ToInt32(fc["id"]);

            if (ModelState.IsValid)
            {
                int CurrentProjectType = Convert.ToInt32(fc["ProjectType"]);
                string TargetDescription = fc["Description"].ToString();
                //replace non space,number and letters with empty string.
                TargetDescription = System.Text.RegularExpressions.Regex.Replace(TargetDescription, @"[^a-zA-Z0-9\s]", string.Empty);
                Scheduling.Database.Utility.CloneExistingMilestoneProfile(SourceID, CurrentProjectType, TargetDescription);
                return RedirectToAction("ManageMilestoneTreeSettingProfiles");
            }


            else
            {

                MilestoneTreeSettingsProfile ep = Scheduling.Database.Utility.GetAllMilestoneTreeSettingsProfiles().Where(x => x.ID == SourceID).First();
                ViewBag.ExistingProfile = ep;
                return View("CloneSingleMilestoneTreeProfile");

            }



        }

        //has corresponding view
        public ActionResult CreateMultipleStepOne(string ErrorMessage)
        {
            ViewBag.ProductDropdownOptions = Scheduling.Html.AdminUtilities.BuildProductDropdown();
            ViewBag.PubCodeDropdownOptions = Scheduling.Html.AdminUtilities.BuildPublicationDropdown();
            ViewBag.ProjectRangeOptions = Scheduling.Html.AdminUtilities.BuildProjectRangeDropdown();
            ViewBag.YearDropdownOptions = Scheduling.Html.AdminUtilities.BuildYearDropdown();
            ViewBag.MilestoneTreeSettingsProfileOptions = Scheduling.Html.AdminUtilities.BuildMilestoneTreeSettingsProfileDropdownOptionsOnly();

            //called from step 2 if any errors
            ViewBag.ErrorMessage = string.Empty;
            if (!string.IsNullOrWhiteSpace(ErrorMessage))
            {
                ViewBag.ErrorMessage = ErrorMessage;

            }

            return View();

        }


        //used in Create Multiple Step One view
        
        public ActionResult ProcessMultipleProjectsStepOne(FormCollection fc)
        {
            if (string.IsNullOrWhiteSpace(fc["year"]))
            {

                return RedirectToAction("CreateMultipleStepOne", new { ErrorMessage = "Year is Required" });
            }

            if (string.IsNullOrWhiteSpace(fc["profile"]))
            {
                return RedirectToAction("CreateMultipleStepOne", new { ErrorMessage = "Profile is Required" });

            }

            if (string.IsNullOrWhiteSpace(fc["timeline"]))
            {
                return RedirectToAction("CreateMultipleStepOne", new { ErrorMessage = "Timeline is Required" });

            }

            if (string.IsNullOrWhiteSpace(fc["pubcode"]))
            {
                return RedirectToAction("CreateMultipleStepOne", new { ErrorMessage = "Pubcode is Required" });

            }

            //Get Year
            string CurrentYear = fc["year"];

            //Pubcode

            string CurrentPubCode = string.Empty;
            if (!string.IsNullOrWhiteSpace(fc["pubcode"]))
            {
                CurrentPubCode = fc["pubcode"];

            }

            //Product

            string CurrentProduct = string.Empty;
            if (!string.IsNullOrWhiteSpace(fc["product"]))
            {
                CurrentProduct = fc["product"];

            }

            //profile
            string CurrentProfile = fc["profile"];

            int ProfileID = 0;
            bool HasProfileType = Int32.TryParse(CurrentProfile, out ProfileID);
            bool CanVerify = false;

            string VerifyStr = Scheduling.StringFunctions.Utility.GetAppSettingValue("ValidateBaselineOnProjectCreation").ToString().ToLower();
            if (string.Equals("true", VerifyStr)) CanVerify = true;

            if (HasProfileType && CanVerify)
            {
                VerifyStr = string.Empty;
                int CurrentProfileID = Convert.ToInt32(CurrentProfile);

                int ProfileTypeID = Scheduling.Database.Utility.GetAllMilestoneTreeSettingsProfiles().Where(x => x.ID == CurrentProfileID).First().ProjectType;
                if (ProfileTypeID == (int)Scheduling.Enums.BaselineType.Magazine)
                {

                    VerifyStr = Scheduling.Database.Utility.VerifySingleMilestoneTreeProfileForMagazineType(CurrentProfileID);

                }

                else
                {

                    VerifyStr = Scheduling.Database.Utility.VerifySingleMilestoneTreeProfileForNonMagazineType(CurrentProfileID);

                }
            }


            if (!string.IsNullOrWhiteSpace(VerifyStr))
            {
                return RedirectToAction("CreateMultipleStepOne", new { ErrorMessage = "This Baseline has Invalid Entries" });

            }

            //Get Timelines


            string CurrentStrTimeline = fc["timeline"].ToString();



            MultipleProjectStepTwo mpst = new MultipleProjectStepTwo();
            mpst.Year = CurrentYear;
            mpst.Timelines = CurrentStrTimeline;
            mpst.Profile = CurrentProfile;
            mpst.Product = CurrentProduct;
            mpst.Pubcode = CurrentPubCode;

            return RedirectToAction("CreateMultipleStepTwo", mpst);

        }

        //has corresponding view
        public ActionResult CreateMultipleStepTwo(MultipleProjectStepTwo mpst)
        {
            List<string> StrList = new List<string>();
            if (mpst.Timelines.Contains(','))
            {
                string[] StrArray = mpst.Timelines.Split(',');
                foreach (string s in StrArray)
                {
                    StrList.Add(s);
                }
            }

            else
            {
                StrList.Add(mpst.Timelines);

            }

            ViewBag.StrList = StrList;
            return View(mpst);

        }


        //Show Projects with value of ScheduleID and above..has corresponding view
        public ActionResult CreateChangeRequest(FormCollection fc)
        {
            List<ProjectDisplay> ProdDisplayList = Scheduling.Database.Utility.GetAllProjects();

            int ScheduleApprovedID = Convert.ToInt32(Scheduling.StringFunctions.Utility.GetAppSettingValue("ProjectStatusScheduleCreatedID"));
            ProdDisplayList = ProdDisplayList.Where(x => x.CurrentProjectStatus >= ScheduleApprovedID && x.IsLocked != 1).ToList();


            if (!string.IsNullOrEmpty(fc["timeline"]))
            {
                int CurrentTimeline = Convert.ToInt32(fc["timeline"]);
                ProdDisplayList = ProdDisplayList.Where(x => x.ProjectRangeFK == CurrentTimeline).ToList();
                ViewBag.CurrentTimeline = CurrentTimeline;
            }

            if (!string.IsNullOrEmpty(fc["pubcode"]))
            {
                int CurrentPubcode = Convert.ToInt32(fc["pubcode"]);
                ProdDisplayList = ProdDisplayList.Where(y => y.PubCodeFK == CurrentPubcode).ToList();
                ViewBag.CurrentPubcode = CurrentPubcode;
            }

            if (!string.IsNullOrEmpty(fc["year"]))
            {
                int CurrentYear = Convert.ToInt32(fc["year"]);
                ProdDisplayList = ProdDisplayList.Where(z => z.YearFK == CurrentYear).ToList();
                ViewBag.CurrentYear = CurrentYear;
            }

            List<EditProjectWithMilestones> EditList = new List<EditProjectWithMilestones>();

            if (ProdDisplayList.Count > 0)
            {

                foreach (ProjectDisplay p in ProdDisplayList)
                {
                    EditProjectWithMilestones epwm = new EditProjectWithMilestones();
                    epwm.ID = p.ID;
                    epwm.Name = p.Name;
                    epwm.Year = Scheduling.Database.Utility.GetAllYears().Where(x => x.ID == p.YearFK).First().Value.ToString();
                    epwm.DateCreated = p.DateCreated.ToLongDateString();
                    epwm.CurrentVersion = p.CurrentVersion;
                    epwm.CurrentProjectStatus = p.CurrentProjectStatus;

                    //Get the milestones
                    List<MilestoneValue> MilValList = Scheduling.Database.Utility.GetMilestoneValuesByProjectID(p.ID);
                    epwm.MileValueList = MilValList;
                    //Gen Strongly Typed View
                    EditList.Add(epwm);
                }

            }

            ViewBag.StatusList = Scheduling.Database.Utility.GetAllProjectStatuses();
            return View(EditList);


        }

        
        //used in Create Multiple Step Two
        public ActionResult ProcessMultipleProjectsStepTwo(FormCollection fc)
        {
            int counter = Convert.ToInt32(fc["counter"]);
            for (int i = 1; i <= counter; i++)
            {
                SingleProjectWithNewstand spwn = new SingleProjectWithNewstand();
                string TimelineField = string.Format("timeline-{0}", i);
                string DueDateField = string.Format("duedate-{0}", i);
                spwn.NewsStandDate = fc[DueDateField];
                spwn.Timeline = fc[TimelineField];
                spwn.Product = fc["product"];
                spwn.Profile = Convert.ToInt32(fc["profile"]);
                spwn.PubCode = fc["pubcode"];
                spwn.Year = Convert.ToInt32(fc["year"]);

                int IntRes = Database.Utility.CreateSingleProjectAndCreateMilestonesFromProfileTable(spwn);
            }
            //On Creation ensure the pubcode dropdown is initialised
            System.Web.HttpContext.Current.Session["CurrentPubCode"] = fc["pubcode"];
            return RedirectToAction("ListManagedProjects");


        }

        /*Test only remove me*/
        

        public ActionResult ListActivities()
        {
            List<Activity> ActList = Scheduling.Database.Utility.GetAllActivities();
            List<ActivityDisplay> ActDispList = new List<ActivityDisplay>();

            List<MilestoneField> MfList = Scheduling.Database.Utility.GetAllParentMilestoneFields();
            List<ProjectDisplay> ProjList = Scheduling.Database.Utility.GetAllProjects();

            foreach (Activity a in ActList)
            {

                ActivityDisplay ad = new ActivityDisplay();
                ad.ID = a.ID;
                ad.Message = a.Message;

                //create milestone name 
                ad.MilestoneFieldName = "N/A";
                if (a.MilestoneValueFk.HasValue) ad.MilestoneFieldName = MfList.Where(x => x.ID == a.MilestoneValueFk).First().Description;

                //create project name
                ad.ProjectFieldName = "N/A";
                if (a.ProjectFK.HasValue) ad.ProjectFieldName = ProjList.Where(y => y.ID == a.ProjectFK).First().Name;

                ad.DateModified = a.DateModified.ToString();
                ad.ModifiedBy = a.ModifiedBy;
                ActDispList.Add(ad);

            }

            return View(ActDispList);
        }

        /*List Milestone Tree Setting Profiles */

        //has corresponding view
        public ActionResult ListMilestoneTreeSettingProfiles()
        {

            List<MilestoneTreeSettingsProfile> MtsList = Scheduling.Database.Utility.GetAllMilestoneTreeSettingsProfiles();
            MtsList.OrderBy(x => x.ID);

            List<MilestoneTreeSettingsProfileDisplay> MtsDisplayList = new List<MilestoneTreeSettingsProfileDisplay>();

            foreach (MilestoneTreeSettingsProfile mts in MtsList)
            {
                MilestoneTreeSettingsProfileDisplay pd = new MilestoneTreeSettingsProfileDisplay();
                pd.ID = mts.ID;
                pd.Description = mts.Description;
                pd.ProjectProfileTypeName = Scheduling.Database.Utility.GetAllProjectProfileTypes().Where(x => x.ID == mts.ProjectType).First().Description;
                MtsDisplayList.Add(pd);


            }
            return View(MtsDisplayList);

        }

        //called from admin tab
        public ActionResult ListPublicationCodes()
        {
            List<PublicationCode> PcList = Scheduling.Database.Utility.GetAllPublicationCodes();
            return View(PcList);
        }

        //has corresponding view
        public ActionResult ListProjects(FormCollection fc)
        {
            List<ProjectDisplay> ProdDisplayList = Scheduling.Database.Utility.GetAllProjects();
            List<ReadOnlyProjectWithMilestones> RopList = new List<ReadOnlyProjectWithMilestones>();

            if (!string.IsNullOrEmpty(fc["project-type"]))
            {

                int CurrentProjectType = Convert.ToInt32(fc["project-type"]);
                ProdDisplayList = Scheduling.Database.Utility.GetProjectsBasedOnBaselineType(CurrentProjectType);
                ViewBag.CurrentProjectType = CurrentProjectType;
            }


            if (!string.IsNullOrEmpty(fc["timeline"]))
            {
                int CurrentTimeline = Convert.ToInt32(fc["timeline"]);
                ProdDisplayList = ProdDisplayList.Where(x => x.ProjectRangeFK == CurrentTimeline).ToList();
                ViewBag.CurrentTimeline = CurrentTimeline;
            }



            if (!string.IsNullOrEmpty(fc["pubcode"]))
            {
                int CurrentPubCode = Convert.ToInt32(fc["pubcode"]);
                ProdDisplayList = ProdDisplayList.Where(y => y.PubCodeFK == CurrentPubCode).ToList();
                ViewBag.CurrentPubcode = CurrentPubCode;
            }

            if (!string.IsNullOrEmpty(fc["year"]))
            {
                int CurrentYear = Convert.ToInt32(fc["year"]);
                ProdDisplayList = ProdDisplayList.Where(z => z.YearFK == CurrentYear).ToList();
                ViewBag.CurrentYear = CurrentYear;
            }


            if (!string.IsNullOrEmpty(fc["status"]))
            {
                int CurrentStatus = Convert.ToInt32(fc["status"]);
                ProdDisplayList = ProdDisplayList.Where(p => p.CurrentProjectStatus == CurrentStatus).ToList();
                ViewBag.CurrentStatus = CurrentStatus;

            }


            if (ProdDisplayList.Count > 0)
            {
                //if not timeline ensure we sort by project range SortOrder steve c 8/11/14
                ProdDisplayList = ProdDisplayList.OrderBy(p => p.PubCodeFK).ThenBy(p => p.YearFK).ThenBy(p => p.ProjectRangeSortOrder).ToList();

                foreach (ProjectDisplay p in ProdDisplayList)
                {

                    ReadOnlyProjectWithMilestones Rop = new ReadOnlyProjectWithMilestones();
                    Rop.Comments = p.Comments;
                    Rop.DateCreated = p.DateCreated.ToShortDateString();
                    Rop.CreatedBy = p.CreatedBy;
                    Rop.ID = p.ID;
                    Rop.ProfileDesc = Scheduling.Database.Utility.GetAllMilestoneTreeSettingsProfiles().Where(x => x.ID == p.MilestoneTreeSettingsProfileFK).First().Description;

                    string CurrentProduct = "N/A";
                    if (p.ProductFK.HasValue) CurrentProduct = Scheduling.Database.Utility.GetAllProducts().Where(x => x.ID == p.ProductFK).First().Description;

                    //assign Product
                    Rop.ProductDesc = CurrentProduct;

                    //timeline
                    string CurrentTimeline = "N/A";
                    if (p.ProjectRangeFK.HasValue)
                    {
                        CurrentTimeline = Scheduling.Database.Utility.GetAllProjectRanges().Where(x => x.ID == p.ProjectRangeFK).First().ShortDesc;
                    }

                    Rop.Timeline = CurrentTimeline;

                    //assign Pubcode

                    Rop.PubCodeDesc = p.PubCodeFK.HasValue ? Scheduling.Database.Utility.GetAllPublicationCodes().Where(x => x.ID == p.PubCodeFK).First().ShortDesc : "N/A";

                    //assign Year
                    Rop.Year = Scheduling.Database.Utility.GetAllYears().Where(x => x.ID == p.YearFK).First().Value.ToString();

                    //assign Name 
                    Rop.Name = p.Name;
                    Rop.NewstandDate = Scheduling.Database.Utility.GetNewstandDateForListedProject(p.ID);

                    //assign status
                    Rop.CurrentProjectStatus = Scheduling.Database.Utility.GetAllProjectStatuses().Where(x => x.ID == p.CurrentProjectStatus).First().Description;

                    RopList.Add(Rop);
                }

            }
            return View(RopList);




        }

        //has corresponding view
        public ActionResult ListChangeRequests()
        {

            List<ChangeRequest> CrList = Scheduling.Database.Utility.GetAllChangeRequests().OrderByDescending(x => x.ProjectFK).ThenByDescending(x => x.DateRequested).ToList();
            return View(CrList);
        }

        //called from admin tab
        public ActionResult ListDepartments()
        {
            List<Department> DepList = Scheduling.Database.Utility.GetAllDepartments().OrderBy(x => x.Description).ToList();
            return View(DepList);

        }

        //called from admin tab
        public ActionResult ListFieldAliases()
        {

            List<FieldAlias> AliasList = Scheduling.Database.Utility.GetAllFieldAliases();
            return View(AliasList);
        }

        //called from admin tab
        public ActionResult ListGroups()
        {
            List<Group> GroupList = Scheduling.Database.Utility.GetAllGroups().OrderBy(x => x.Description).ToList();
            return View(GroupList);


        }

        //has correponding view
        public ActionResult ManageGroups()
        {

            List<Group> GroupList = Scheduling.Database.Utility.GetAllGroups().OrderBy(x => x.Description).ToList();
            return View(GroupList);


        }

        //has correponding view

        public ActionResult ManageHolidays()
        {
            List<DateTime> AllHolidays = Scheduling.Database.Utility.GetAllHolidays().OrderByDescending(x => x.Year).ThenBy(y => y.Month).ThenBy(z => z.Day).ToList();

            List<Year> YearList = Scheduling.Database.Utility.GetAllYears().OrderByDescending(x => x.Value).ToList();
            List<int> DistinctYears = YearList.Select(x => x.ID).Distinct().ToList();

            ViewBag.AllHolidays = AllHolidays;
            ViewBag.DistinctYears = DistinctYears;

            return View();
        }

        public ActionResult ManageHolidayLiveChange()
        {
            return View();


        }

        //has correponding view
        
        public ActionResult ProcessManageHolidayLiveChange()
        {

            string Result = Scheduling.Calc.CalcUtilities.RecalculateAllMagazineProjectsAfterLiveHolidayChange();
            return View("ManageHolidayLiveChange");
        }

        //has correponding view
        public ActionResult ListGroupAssociations()
        {

            List<DeptToGroupsToPubCode> CompositeList = new List<DeptToGroupsToPubCode>();
            CompositeList = Scheduling.Database.Utility.GetAllGroupsToDeptToPubCode();
            List<DeptToGroupsToPubCodeDisplay> CompositeDisplay = Scheduling.CastingFunctions.Utility.ConvertDeptToGroupsToPubCodeToDisplay(CompositeList);
            return View(CompositeDisplay);
        }

        //has correponding view
        public ActionResult ManageChangeRequests()
        {
            List<ChangeRequest> CrList = Scheduling.Database.Utility.GetAllChangeRequests().OrderByDescending(x => x.ProjectFK).ThenByDescending(x => x.DateRequested).ToList();
            ViewBag.RequestStatusList = Scheduling.Database.Utility.GetAllChangeRequestStatuses();
            return View(CrList);
        }

        //used in list change requests
        public ActionResult ProcessListChangeRequests(FormCollection fc)
        {

            List<ChangeRequest> CrList = Scheduling.Database.Utility.GetAllChangeRequests().OrderByDescending(x => x.ProjectFK).ThenByDescending(x => x.DateRequested).ToList();
            ViewBag.CurrentProject = string.Empty;
            ViewBag.FilterByStatus = false;


            string FilterPending = fc["FilterByPendingChangeRequests"];

            if (!string.IsNullOrEmpty(FilterPending))
            {
                int CreatedStatus = (int)Scheduling.Enums.ChangeRequestStatus.Created;
                List<int> DistinctProjects = CrList.Where(x => x.RequestStatus == CreatedStatus).Select(x => x.ProjectFK).Distinct().ToList();
                CrList = CrList.Where(x => DistinctProjects.Contains(x.ProjectFK)).ToList();
                ViewBag.FilterByStatus = true;


            }

            else
            {
                if (!string.IsNullOrEmpty(fc["project"]))
                {
                    int CurrentProject = Convert.ToInt32(fc["project"]);
                    CrList = CrList.Where(x => x.ProjectFK == CurrentProject).ToList();
                    ViewBag.CurrentProject = CurrentProject;

                }

            }

            ViewBag.RequestStatusList = Scheduling.Database.Utility.GetAllChangeRequestStatuses();
            return View("ListChangeRequests", CrList);


        }

        //used in managechangerequests

        public ActionResult ProcessManageChangeRequests(FormCollection fc)
        {
            List<ChangeRequest> CrList = Scheduling.Database.Utility.GetAllChangeRequests().OrderByDescending(x => x.ProjectFK).ThenByDescending(x => x.DateRequested).ToList();
            ViewBag.CurrentProject = string.Empty;
            ViewBag.FilterByStatus = false;


            string FilterPending = fc["FilterByPendingChangeRequests"];

            if (!string.IsNullOrEmpty(FilterPending))
            {
                int CreatedStatus = (int)Scheduling.Enums.ChangeRequestStatus.Created;
                List<int> DistinctProjects = CrList.Where(x => x.RequestStatus == CreatedStatus).Select(x => x.ProjectFK).Distinct().ToList();
                CrList = CrList.Where(x => DistinctProjects.Contains(x.ProjectFK)).ToList();
                ViewBag.FilterByStatus = true;


            }

            else
            {
                if (!string.IsNullOrEmpty(fc["project"]))
                {
                    int CurrentProject = Convert.ToInt32(fc["project"]);
                    CrList = CrList.Where(x => x.ProjectFK == CurrentProject).ToList();
                    ViewBag.CurrentProject = CurrentProject;

                }

            }

            ViewBag.RequestStatusList = Scheduling.Database.Utility.GetAllChangeRequestStatuses();
            return View("ManageChangeRequests", CrList);

        }


        //has corresponding view
        public ActionResult ManageGroupAssociations()
        {

            List<DeptToGroupsToPubCode> CompositeList = new List<DeptToGroupsToPubCode>();

            CompositeList = Scheduling.Database.Utility.GetAllGroupsToDeptToPubCode();
            List<DeptToGroupsToPubCodeDisplay> CompositeDisplay = Scheduling.CastingFunctions.Utility.ConvertDeptToGroupsToPubCodeToDisplay(CompositeList);

            //dep dropdown
            List<Department> DepList = Scheduling.Database.Utility.GetAllDepartments();
            ViewBag.DepList = DepList;

            //pubcode dropdown

            List<PublicationCode> PubList = Scheduling.Database.Utility.GetAllPublicationCodes();
            ViewBag.PubList = PubList;

            //group dropdown
            List<Group> GroupList = Scheduling.Database.Utility.GetAllGroups();
            ViewBag.GroupList = GroupList;


            return View(CompositeDisplay);

        }

        //used in manage group associations
        public ActionResult ManageGroupAssociationsAddEntry(FormCollection fc)
        {

            string CurrentPubCode = fc["pubcode"];
            string CurrentGroup = fc["group"];
            string CurrentDept = fc["dept"];

            if (!string.IsNullOrWhiteSpace(CurrentPubCode) && !string.IsNullOrWhiteSpace(CurrentGroup) && !string.IsNullOrWhiteSpace(CurrentDept))
            {
                int Dept = Convert.ToInt32(CurrentDept);
                int Group = Convert.ToInt32(CurrentGroup);
                int PubCode = Convert.ToInt32(CurrentPubCode);

                if (Scheduling.Database.Utility.VerifyNoExistingGroupAssociationEntry(Dept, Group, PubCode))
                {
                    Scheduling.Database.Utility.CreateGroupAssociationEntry(Dept, Group, PubCode);

                }

            }

            return RedirectToAction("ManageGroupAssociations");
        }

        //used in manage group associations
        public ActionResult ManageGroupAssociationsRemoveEntry(FormCollection fc)
        {
            int ID = Convert.ToInt32(fc["id"]);
            Scheduling.Database.Utility.RemoveSingleGroupAssociationEntry(ID);
            return RedirectToAction("ManageGroupAssociations");
        }

        //has corresponding view
        public ActionResult ListProfileTypes()
        {
            List<ProjectProfileType> ProfTypeList = Scheduling.Database.Utility.GetAllProjectProfileTypes();

            return View(ProfTypeList);

        }

        //has corresponding view
        public ActionResult ListRoles()
        {

            List<Role> RoleList = Scheduling.Database.Utility.GetAllRoles().OrderBy(x => x.ShortDesc).ToList();
            return View(RoleList);

        }

        //has corresponding view
        public ActionResult ListUsers()
        {
            List<User> UserList = Scheduling.Database.Utility.GetAllUsers();
            List<UserDisplay> UserDisplayList = new List<UserDisplay>();
            foreach (User u in UserList)
            {
                UserDisplay ud = new UserDisplay();
                ud.ID = u.ID;
                ud.UserName = u.UserName;
                ud.RoleDesc = Scheduling.Database.Utility.GetAllRoles().Where(x => x.ID == u.RoleFK).First().ShortDesc;
                ud.Email = u.Email;

                string GroupDesc = "--";
                int Count = Scheduling.Database.Utility.GetAllUserToGroups().Where(x => Convert.ToInt32(x.UserID) == u.ID).ToList().Count;
                if (Count > 0)
                {
                    List<string> GpList = Scheduling.Database.Utility.GetAllUserToGroups().Where(x => Convert.ToInt32(x.UserID) == u.ID).Select(x => x.GroupID).ToList();
                    GroupDesc = string.Empty;
                    foreach (string s in GpList)
                    {
                        GroupDesc += Scheduling.Database.Utility.GetAllGroups().Where(y => y.ID == Convert.ToInt32(s)).First().Description + ",";


                    }

                    GroupDesc = GroupDesc.TrimEnd(',');
                }

                ud.GroupDesc = GroupDesc;

                UserDisplayList.Add(ud);
            }

            return View(UserDisplayList);
        }


        //not currently being implemented leave for now
        public ActionResult ManageSingleProjectChangeRequests(FormCollection fc)
        {

            int CurrentProject = Convert.ToInt32(fc["id"]);
            ProjectDisplay pd = Scheduling.Database.Utility.GetAllProjects().Where(x => x.ID == CurrentProject).First();
            List<ChangeRequest> CrList = Scheduling.Database.Utility.GetProjectChangeRequestsFromProjectID(CurrentProject);
            ViewBag.ProjectDisplay = pd;
            ViewBag.RequestStatusList = Scheduling.Database.Utility.GetAllChangeRequestStatuses();


            return View(CrList);

        }

        //has corresponding view
        public ActionResult ProcessManageGroups(FormCollection fc)
        {
            if (!string.IsNullOrWhiteSpace(fc["group"]))
            {
                string input = fc["Group"].ToUpper();
                Scheduling.Database.Utility.CreateGroupEntry(input);

            }

            List<Group> GroupList = Scheduling.Database.Utility.GetAllGroups().OrderBy(x => x.Description).ToList();
            return View("ManageGroups", GroupList);

        }


        
        //has corresonponding view..functionality not being used at present
        public ActionResult ProcessSingleManageableReviewableProject(FormCollection fc)
        {
            string ts = fc["review-radio-group"];
            int RadioValueID = Convert.ToInt32(fc["review-radio-group"]);
            string CurrentComment = string.Empty;
            if (!string.IsNullOrWhiteSpace(fc["comment"])) CurrentComment = fc["comment"];

            if (RadioValueID > 0)
            {
                int ProjectID = Convert.ToInt32(fc["ProjectID"]);
                Scheduling.Database.Utility.UpdateProjectStatus(ProjectID, RadioValueID);

                int ApprovalID = Convert.ToInt32(Scheduling.StringFunctions.Utility.GetAppSettingValue("ProjectStatusApprovedOnSaleID"));
                int RejectionID = Convert.ToInt32(Scheduling.StringFunctions.Utility.GetAppSettingValue("ProjectStatusRejectedOnSaleID"));

                //we know we have a valid value if not approved it must be rejected.
                if (RadioValueID == ApprovalID)
                {
                    Scheduling.Email.Utility.SendNotificationForSingleProjectNewsStandDateApproval(ProjectID, CurrentComment);

                }

                if (RadioValueID == RejectionID)
                {
                    Scheduling.Email.Utility.SendNotificationForSingleProjectNewsStandDateRejection(ProjectID, CurrentComment);

                }

            }
            return RedirectToAction("ManageReviewableProjects");


        }

        //used in list managed projects summary partial shared view
        public ActionResult ManageSingleProjectNotes(FormCollection fc)
        {

            int CurrentProject = Convert.ToInt32(fc["id"]);
            ProjectDisplay pd = Scheduling.Database.Utility.GetAllProjects().Where(x => x.ID == CurrentProject).First();

            ViewBag.ProjectDisplay = pd;
            List<ProjectNote> NoteList = Scheduling.Database.Utility.GetProjectNotesFromProjectID(CurrentProject);

            return View(NoteList);
        }

        //used in home /manage single project screen
        public ActionResult ManageSingleProjectAfterDependencyAjaxCall(int id, string message)
        {
            string AddRemoveMessage = string.Empty;
            if (string.IsNullOrWhiteSpace(message))
            {
                AddRemoveMessage = "Success";

            }

            else
            {

                AddRemoveMessage = message;

            }
            int i = id;
            List<ProjectDisplay> ProdDisplayList = Scheduling.Database.Utility.GetAllProjects().Where(x => x.ID == i).ToList();


            List<EditProjectWithMilestones> EditList = new List<EditProjectWithMilestones>();

            if (ProdDisplayList.Count > 0)
            {

                foreach (ProjectDisplay p in ProdDisplayList)
                {
                    EditProjectWithMilestones epwm = new EditProjectWithMilestones();
                    epwm.ID = p.ID;
                    epwm.Name = p.Name;
                    epwm.Year = Scheduling.Database.Utility.GetAllYears().Where(x => x.ID == p.YearFK).First().Value.ToString();
                    epwm.DateCreated = p.DateCreated.ToLongDateString();
                    epwm.CurrentVersion = p.CurrentVersion;
                    epwm.CurrentProjectStatus = p.CurrentProjectStatus;

                    //Get the milestones
                    List<MilestoneValue> MilValList = Scheduling.Database.Utility.GetMilestoneValuesByProjectIDOrderedByDueDateDesc(p.ID);

                    epwm.MileValueList = MilValList;
                    //Gen Strongly Typed View
                    EditList.Add(epwm);
                }

            }

            ViewBag.StatusList = Scheduling.Database.Utility.GetAllProjectStatuses();
            ViewBag.AddRemoveMessage = AddRemoveMessage;
            return View("ManageSingleProject", EditList);

        }

        //has corresonponding view
        public ActionResult ManageSingleProject(FormCollection fc)
        {

            string IdVal = Request["id"].ToString();
            int i = Convert.ToInt32(IdVal);
            List<ProjectDisplay> ProdDisplayList = Scheduling.Database.Utility.GetAllProjects().Where(x => x.ID == i).ToList();


            List<EditProjectWithMilestones> EditList = new List<EditProjectWithMilestones>();

            if (ProdDisplayList.Count > 0)
            {

                foreach (ProjectDisplay p in ProdDisplayList)
                {
                    EditProjectWithMilestones epwm = new EditProjectWithMilestones();
                    epwm.ID = p.ID;
                    epwm.Name = p.Name;
                    epwm.Year = Scheduling.Database.Utility.GetAllYears().Where(x => x.ID == p.YearFK).First().Value.ToString();
                    epwm.DateCreated = p.DateCreated.ToLongDateString();
                    epwm.CurrentVersion = p.CurrentVersion;
                    epwm.CurrentProjectStatus = p.CurrentProjectStatus;
                    epwm.IsLocked = p.IsLocked;

                    //Get the milestones
                    List<MilestoneValue> MilValList = Scheduling.Database.Utility.GetMilestoneValuesByProjectIDOrderedByDueDateDesc(p.ID);
                    epwm.MileValueList = MilValList;
                    //Gen Strongly Typed View
                    EditList.Add(epwm);
                }

            }


            ViewBag.StatusList = Scheduling.Database.Utility.GetAllProjectStatuses();
            return View(EditList);

        }


        //has corresonponding view
        public ActionResult ListManagedProjects(FormCollection fc)
        {
            List<ProjectDisplay> ProdDisplayList = Scheduling.Database.Utility.GetAllProjects();
            List<ReadOnlyProjectWithMilestones> RopList = new List<ReadOnlyProjectWithMilestones>();
                        

            if (!string.IsNullOrEmpty(fc["project-type"]))
            {

                int CurrentProjectType = Convert.ToInt32(fc["project-type"]);
                ProdDisplayList = Scheduling.Database.Utility.GetProjectsBasedOnBaselineType(CurrentProjectType);
                ViewBag.CurrentProjectType = CurrentProjectType;
            }

            //timeline
            if (!string.IsNullOrEmpty(fc["timeline"]))
            {
                int CurrentTimeline = Convert.ToInt32(fc["timeline"]);
                ProdDisplayList = ProdDisplayList.Where(x => x.ProjectRangeFK == CurrentTimeline).ToList();
                ViewBag.CurrentTimeline = CurrentTimeline;
            }

            //on initial load

            if (string.IsNullOrWhiteSpace(fc["pubcode"]) && string.IsNullOrWhiteSpace(fc["year"]))
            {

                int LastModifiedProjectID = Scheduling.Database.Utility.GetLastProjectEntryIDInProjectHistory();
                ProjectDisplay pd = ProdDisplayList.Where(x => x.ID == LastModifiedProjectID).First();
                int PrepopulatedPubCode = pd.PubCodeFK.Value;
                int PrepopulatedYear = pd.YearFK;
                ProdDisplayList = ProdDisplayList.Where(x => x.YearFK == PrepopulatedYear).Where(y => y.PubCodeFK == PrepopulatedPubCode).ToList();
                ViewBag.CurrentPubcode = PrepopulatedPubCode;
                ViewBag.CurrentYear = PrepopulatedYear;
            }
            
            if(!string.IsNullOrWhiteSpace(fc["pubcode"]))
            {
                int CurrentPubcode=Convert.ToInt32(fc["pubcode"]);
                ProdDisplayList = ProdDisplayList.Where(x => x.PubCodeFK.Value == CurrentPubcode).ToList();
                ViewBag.CurrentPubcode = CurrentPubcode;
            }

            if (!string.IsNullOrWhiteSpace(fc["year"]))
            {
                int CurrentYear = Convert.ToInt32(fc["year"]);
                ProdDisplayList = ProdDisplayList.Where(x => x.YearFK == CurrentYear).ToList();
                ViewBag.CurrentYear = CurrentYear;
            }
  


            if (ProdDisplayList.Count > 0)
            {
                //sort by project range sort order to deal with funky timelines
                ProdDisplayList = ProdDisplayList.OrderBy(p => p.PubCodeFK).ThenBy(p => p.YearFK).ThenBy(p => p.ProjectRangeSortOrder).ToList();

                foreach (ProjectDisplay p in ProdDisplayList)
                {

                    ReadOnlyProjectWithMilestones Rop = new ReadOnlyProjectWithMilestones();
                    Rop.Comments = p.Comments;
                    Rop.DateCreated = p.DateCreated.ToShortDateString();
                    Rop.CreatedBy = p.CreatedBy;
                    Rop.ID = p.ID;
                    Rop.ProfileDesc = Scheduling.Database.Utility.GetAllMilestoneTreeSettingsProfiles().Where(x => x.ID == p.MilestoneTreeSettingsProfileFK).First().Description;

                    string CurrentProduct = "N/A";
                    if (p.ProductFK.HasValue) CurrentProduct = Scheduling.Database.Utility.GetAllProducts().Where(x => x.ID == p.ProductFK).First().Description;

                    //assign Product
                    Rop.ProductDesc = CurrentProduct;

                    //timeline
                    string CurrentTimeline = "N/A";
                    if (p.ProjectRangeFK.HasValue)
                    {
                        CurrentTimeline = Scheduling.Database.Utility.GetAllProjectRanges().Where(x => x.ID == p.ProjectRangeFK).First().ShortDesc;
                    }

                    Rop.Timeline = CurrentTimeline;

                    //assign Pubcode

                    Rop.PubCodeDesc = p.PubCodeFK.HasValue ? Scheduling.Database.Utility.GetAllPublicationCodes().Where(x => x.ID == p.PubCodeFK).First().ShortDesc : "N/A";

                    //assign Year
                    Rop.Year = Scheduling.Database.Utility.GetAllYears().Where(x => x.ID == p.YearFK).First().Value.ToString();

                    //assign Name 
                    Rop.Name = p.Name;
                    Rop.NewstandDate = Scheduling.Database.Utility.GetNewstandDateForListedProject(p.ID);

                    //assign status
                    Rop.CurrentProjectStatus = Scheduling.Database.Utility.GetAllProjectStatuses().Where(x => x.ID == p.CurrentProjectStatus).First().Description;

                    RopList.Add(Rop);
                }

            }
            return View(RopList);



        }

        //has corresonponding view
        public ActionResult ManageMajorMilestones()
        {
            List<MilestoneField> MfList = Scheduling.Database.Utility.GetAllMilestoneFields().Where(x => x.IsCreatedByUser == null).OrderBy(x => x.Description).ToList();

            return View(MfList);

        }

        
        [Obsolete]
        //Auto Generate Kpc Job Number Note...leave as legacy 
        public ActionResult ProcessAutoGenerateKpcJobNumbersAsNote()
        {

            List<ProjectDisplay> ProjList = Scheduling.Database.Utility.GetAllProjects();
            int MagazineProfileType = Convert.ToInt32(ConfigurationManager.AppSettings["MagazineProjectType"]);


            foreach (ProjectDisplay pd in ProjList)
            {

                int CurrentProfileType = Scheduling.Database.Utility.GetProfileTypeIDFromProjectID(pd.ID);
                if (CurrentProfileType == MagazineProfileType)
                {
                    Scheduling.Database.Utility.CreateKPCJobNumberForMagazineProject(pd.ID);
                }

            }

            return RedirectToAction("Index");
        }

        [Obsolete]
        public ActionResult AutoGenerateKPCJobNumbersAsNote()
        {

            return View();
        }

        
        [Obsolete]
        public ActionResult ProcessAutoGenerateKPCJobNumbersForNextIssueAdsAndCrossPromoAds()
        {
            List<ProjectDisplay> ProjList = Scheduling.Database.Utility.GetProjectsWherePubCodeHasNoParentForMagazineTypeProfile();

            foreach (ProjectDisplay pd in ProjList)
            {

                {
                    Scheduling.Database.Utility.CreateNextIssueAdsAndCrossPromoAdsForMagazineProject(pd.ID);
                }

            }


            return RedirectToAction("Index");
        }


        //has corresponding view
        public ActionResult ManageReviewableProjects()
        {

            List<ProjectDisplay> PdList = Scheduling.Database.Utility.GetReviewableMagazineTypeProjects();
            ViewBag.MilestoneFieldID = ConfigurationManager.AppSettings["NewsstandOnSaleDateMilestoneValue"];

            return View(PdList);
        }


        //If we pass in unassigned don't filter on year and pubcode since these attributes are project based
        public ActionResult ManageMilestoneTreeSettingProfiles(FormCollection fc)
        {
            List<MilestoneTreeSettingsProfileDisplay> MtsDisplayList = new List<MilestoneTreeSettingsProfileDisplay>();

           

            //initial load show unassigned option
           
                ViewBag.CurrentAssignedType = "U";

           
            //single unassigned id
            if(!string.IsNullOrWhiteSpace(fc["unassigned-selection"]) && fc["assigned-type-selection"].ToUpper()=="U")
            {
               
               MilestoneTreeSettingsProfileDisplay  mpd = new MilestoneTreeSettingsProfileDisplay();
               mpd.Description = Scheduling.Database.Utility.GetBaselineDescByID(Convert.ToInt32(fc["unassigned-selection"]));
               mpd.ID = Convert.ToInt32(fc["unassigned-selection"]);
               MtsDisplayList.Add(mpd);
               ViewBag.CurrentAssignedType = "U";
               ViewBag.ChosenUnassigned = mpd.ID.ToString();


            }

            if (!string.IsNullOrWhiteSpace(fc["assigned-selection"]) && fc["assigned-type-selection"].ToUpper() == "A")
            {

                MilestoneTreeSettingsProfileDisplay mpd = new MilestoneTreeSettingsProfileDisplay();
                mpd.Description = Scheduling.Database.Utility.GetBaselineDescByID(Convert.ToInt32(fc["assigned-selection"]));
                mpd.ID = Convert.ToInt32(fc["assigned-selection"]);
                MtsDisplayList.Add(mpd);
                ViewBag.CurrentAssignedType = "A";
                ViewBag.ChosenAssigned = mpd.ID.ToString();


            }


            return View(MtsDisplayList);

           

        }


        //new version of code proposed for Version 3.4.2
        public ActionResult EditProjectsCalculateRevertToBaselineVersion342(FormCollection fc)
        {
            int CurrentProjectID = Convert.ToInt32(fc["id"]);

            string CurrentDueDate = fc["hfDueDate"].ToString();

            //Recreate fields and populate due date.

            int FieldRes = Scheduling.Database.Utility.ReCreateMilestoneFieldsFromProfileTableOnProjectReset(CurrentProjectID, CurrentDueDate);

            //Recreate values now we have the nodes 

            int ValueRes = Scheduling.Database.Utility.ReCreateMilestoneValuesOnProjectReset(CurrentProjectID, CurrentDueDate);
            string RetUrl = string.Format("/project/managesingleproject/{0}", CurrentProjectID);
            return Redirect(RetUrl);


        }

        
        [Obsolete]
        //Version 3 of the run calc..Don't use method EditProjectsCalculateRevertToBaselineVersion342 is preferred 1.13.2015 
        public ActionResult EditProjectsCalculateRevertToBaseline(FormCollection fc)
        {
            //for now this is based on a profile
            MilestoneFieldNodeDisplay mfnd = new MilestoneFieldNodeDisplay();
            int CurrentID = Convert.ToInt32(fc["id"]);
            int NsFieldID = Convert.ToInt32(Scheduling.StringFunctions.Utility.GetAppSettingValue("NewsstandOnSaleDateMilestoneValue"));

            DateTime dt = Convert.ToDateTime(fc["hfDueDate"].ToString());

            mfnd.Day = dt.Day;
            mfnd.Month = dt.Month;
            mfnd.Year = dt.Year;

            //Get current Project
            ProjectDisplay p = Scheduling.Database.Utility.GetAllProjects().Where(x => x.ID == CurrentID).First();

            //Get Profile

            mfnd.MilestoneProfile = p.MilestoneTreeSettingsProfileFK.Value;

            //Get N/S Milestone Field ID
            mfnd.MilestoneField = Convert.ToInt32(Scheduling.StringFunctions.Utility.GetAppSettingValue("NewsstandOnSaleDateMilestoneValue"));

            //Clear out the existing Due Dates so any manual entries are whacked
            string ComText = string.Format("update dbo.milestonevalue set DueDate = null where MilestoneFieldFk not in({0}) and projectpk={1}", mfnd.MilestoneField, CurrentID);
            Scheduling.Database.Utility.ExecuteNonQueryWrapper(ComText);

            Scheduling.Database.Utility.RemoveAddedMilestoneValueProjectEntries(CurrentID, mfnd.MilestoneProfile);

            //There could be milestones that have been deleted rebuild value table

            Scheduling.Database.Utility.ReAddDeletedMilestoneValueProjectEntries(CurrentID, mfnd.MilestoneProfile);

            //Version 3.4.2 Reintroduce Dependencies where necessary
            Scheduling.Database.Utility.ReintroduceDependencies(CurrentID, mfnd.MilestoneProfile);

            List<MilestoneFieldNodeDisplay> NodeList = Scheduling.Database.Utility.GetMilestoneFieldNodesBasedOnStartingNode(mfnd);

            //Get List of Milestones that have calculations assigned
            string ExcStr = string.Empty;
            foreach (MilestoneFieldNodeDisplay item in NodeList)
            {
                string CurrentDueDate = string.Format("{0}/{1}/{2}", item.Month, item.Day, item.Year);
                string CurrentStr = string.Format("update dbo.milestonevalue set DueDate='{0}' where projectpk={1} and milestonefieldfk={2};", CurrentDueDate, CurrentID, item.MilestoneField);
                ExcStr += CurrentStr;
            }

            Scheduling.Database.Utility.ExecuteNonQueryWrapper(ExcStr);

            //start 
            string Comments = string.Format("Ran Profile Calculation Process with StartDate of {0} and ResettingDependencies Where Necessary", fc["hfDueDate"].ToString());
            Scheduling.Database.Utility.CreateProjectHistoryEntry(p.CurrentProjectStatus, Comments, p.ID, p.CurrentVersion);

            List<EditProjectWithMilestones> EditList = new List<EditProjectWithMilestones>();


            EditProjectWithMilestones epwm = new EditProjectWithMilestones();
            epwm.ID = p.ID;
            epwm.Name = p.Name;
            epwm.Year = Scheduling.Database.Utility.GetAllYears().Where(x => x.ID == p.YearFK).First().Value.ToString();
            epwm.DateCreated = p.DateCreated.ToLongDateString();
            epwm.CurrentVersion = p.CurrentVersion;
            epwm.CurrentProjectStatus = p.CurrentProjectStatus;

            //Get the milestones
            List<MilestoneValue> MilValList = Scheduling.Database.Utility.GetMilestoneValuesByProjectIDOrderedByDueDateDesc(p.ID);
            epwm.MileValueList = MilValList;
            //Gen Strongly Typed View
            EditList.Add(epwm);
            ViewBag.StatusList = Scheduling.Database.Utility.GetAllProjectStatuses();
            return View("ManageSingleProject", EditList);

            //stop


        }


        //version 1 deprecated
        [Obsolete]
        public ActionResult EditProjectsCalculate(FormCollection fc)
        {
            //for now this is based on a profile
            MilestoneFieldNodeDisplay mfnd = new MilestoneFieldNodeDisplay();
            int CurrentID = Convert.ToInt32(fc["id"]);
            int NsFieldID = Convert.ToInt32(Scheduling.StringFunctions.Utility.GetAppSettingValue("NewsstandOnSaleDateMilestoneValue"));

            //MilestoneValue mv = Scheduling.Database.Utility.GetMilestoneValuesByProjectID(CurrentID).Where(x => x.MilestoneFieldFK == NsFieldID).First();
            DateTime dt = Convert.ToDateTime(fc["hfDueDate"].ToString());

            mfnd.Day = dt.Day;
            mfnd.Month = dt.Month;
            mfnd.Year = dt.Year;

            //Get current Project
            ProjectDisplay p = Scheduling.Database.Utility.GetAllProjects().Where(x => x.ID == CurrentID).First();

            mfnd.MilestoneProfile = p.MilestoneTreeSettingsProfileFK.Value;
            //todo is this even needed
            //mfnd.Timeline = p.ProjectRangeFK.Value;
            mfnd.MilestoneField = Convert.ToInt32(Scheduling.StringFunctions.Utility.GetAppSettingValue("NewsstandOnSaleDateMilestoneValue"));

            List<MilestoneFieldNodeDisplay> NodeList = Scheduling.Database.Utility.GetMilestoneFieldNodesBasedOnStartingNode(mfnd);

            string ExcStr = string.Empty;
            foreach (MilestoneFieldNodeDisplay item in NodeList)
            {
                string CurrentDueDate = string.Format("{0}/{1}/{2}", item.Month, item.Day, item.Year);
                string CurrentStr = string.Format("update dbo.milestonevalue set DueDate='{0}' where projectpk={1} and milestonefieldfk={2};", CurrentDueDate, CurrentID, item.MilestoneField);
                ExcStr += CurrentStr;
            }

            Scheduling.Database.Utility.ExecuteNonQueryWrapper(ExcStr);
            string Comments = string.Format("Ran Profile Calculation Process with StartDate of {0}", fc["hfDueDate"].ToString());
            Scheduling.Database.Utility.CreateProjectHistoryEntry(p.CurrentProjectStatus, Comments, p.ID, p.CurrentVersion);
            return RedirectToAction("EditProjects");

        }


        //has corresponding view
        public ActionResult ProcessEditSingleMilestoneTreeProfileSubItems(FormCollection fc)
        {

            int CurrentProfile = Convert.ToInt32(fc["id"]);
            string CurrentCalculation = fc["Calculation"];
            string CurrentDependancy = fc["Dependancy"];

            string SubItemName = fc["ItemName"].Trim();
            if (string.IsNullOrWhiteSpace(SubItemName))
            {
                ModelState.AddModelError("ItemName", "Item Name Is Required.");

            }

            string Parent = fc["Parent"].Trim();
            if (string.IsNullOrWhiteSpace(Parent))
            {
                ModelState.AddModelError("Parent", "Parent Is required.");

            }

            //ensure that we have a calculation and dependancy combination

            if (!string.IsNullOrWhiteSpace(CurrentCalculation) || !string.IsNullOrWhiteSpace(CurrentDependancy))
            {
                if (string.IsNullOrWhiteSpace(CurrentCalculation) || string.IsNullOrWhiteSpace(CurrentDependancy))
                {
                    ModelState.AddModelError("Calculation", "Calculation Requires A Dependancy.");

                }

            }


            //If there is a max firing order grab it otherwise set it to 5.
            string CalcOrderEntry = string.Empty;

            if (!string.IsNullOrWhiteSpace(CurrentCalculation) && ModelState.IsValid)
            {

                List<MilestoneTreeSetting> MaxList = Scheduling.Database.Utility.GetMilestoneTreeSettingsByProfileID(CurrentProfile).Where(x => x.CalcFiringOrder != null).ToList();
                if (MaxList.Count > 0)
                {
                    int? m = MaxList.Max(x => x.CalcFiringOrder);

                    CalcOrderEntry = (m + 5).ToString();

                }

                else
                {
                    CalcOrderEntry = "5";

                }





            }

            string DepEntry = string.Empty;
            if (!string.IsNullOrWhiteSpace(CurrentDependancy))
            {
                DepEntry = CurrentDependancy;
            }

            string CalculationEntry = string.Empty;
            if (!string.IsNullOrWhiteSpace(CurrentCalculation))
            {
                CalculationEntry = CurrentCalculation;
            }

            if (ModelState.IsValid)
            {

                Scheduling.Database.Utility.CreateMilestoneProfileSubItem(CurrentProfile, SubItemName, CalcOrderEntry, Convert.ToInt32(Parent), DepEntry, CalculationEntry);


            }

            MilestoneTreeSettingsProfile mtsp = Scheduling.Database.Utility.GetAllMilestoneTreeSettingsProfiles().Where(x => x.ID == CurrentProfile).First();
            List<MilestoneTreeSetting> MtsList = Scheduling.Database.Utility.GetMilestoneTreeSettingsByProfileID(CurrentProfile).OrderBy(x => x.DisplayOrder).ToList();

            List<MilestoneTreeSetting> MtsHasParentList = MtsList.Where(x => x.MilestoneParentField != null).OrderBy(x => x.MilestoneParentField).ToList();

            //filter out the milestone fields that are used in this profile
            List<int> ids = MtsList.Select(x => x.MilestoneField).Distinct().ToList();
            IEnumerable<MilestoneField> ParentDropdownFieldEnum = Scheduling.Database.Utility.GetAllParentMilestoneFields().Where(x => ids.Contains(x.ID));
            List<MilestoneField> ParentDropdownFieldList = ParentDropdownFieldEnum.ToList();
            List<int?> ParentIds = MtsHasParentList.Select(x => x.MilestoneParentField).Distinct().ToList();
            ViewBag.mtsp = mtsp;
            ViewBag.MtsHasParentList = MtsHasParentList;
            ViewBag.ParentDropdownFieldList = ParentDropdownFieldList;
            ViewBag.CalcList = Scheduling.Database.Utility.GetAllCalculationFields();
            ViewBag.ParentIds = ParentIds;


            //start 
            //We are providing adding the parent desc to the subitem for the dependency dropdown
            List<MilestoneTreeSetting> DepList = MtsList;
            List<MilestoneField> DepMfList = new List<MilestoneField>();
            foreach (MilestoneTreeSetting mts in DepList)
            {
                MilestoneField cmf = new MilestoneField();
                cmf.ID = mts.MilestoneField;

                string Desc = Scheduling.Database.Utility.GetMilestoneDescFromID(mts.MilestoneField);
                if (mts.MilestoneParentField.HasValue)
                {
                    string ParentDesc = Scheduling.Database.Utility.GetMilestoneDescFromID((int)mts.MilestoneParentField);
                    Desc = string.Format(" {0} ( {1} )", Desc, ParentDesc);
                }

                cmf.Description = Desc;
                DepMfList.Add(cmf);

            }

            ViewBag.DepList = DepMfList;
            //stop

            return View("EditSingleMilestoneTreeProfileSubItems");


        }


        //used in edit projects screen
        public ActionResult ProcessEditSingleProject(FormCollection fc)
        {
            string CurrentName = fc["Name"];
            string CurrentStatus = fc["ProjectStatus"];
            string CurrentID = fc["ID"];
            if (!string.IsNullOrWhiteSpace(CurrentName) && !string.IsNullOrWhiteSpace(fc["ProjectStatus"]))
            {
                string ExcStr = string.Format("update dbo.project set Name='{0}',CurrentProjectStatus={1} where ID={2}", CurrentName, CurrentStatus, CurrentID);
                Scheduling.Database.Utility.ExecuteNonQueryWrapper(ExcStr);
                string HistoryStatus = Scheduling.Database.Utility.GetAllProjectStatuses().Where(x => x.ID == Convert.ToInt32(CurrentStatus)).First().Description;
                string Comments = string.Format("Running Update Statement..Project Name ={0},Status={1}", CurrentName, HistoryStatus);

                Scheduling.Database.Utility.CreateProjectHistoryEntry(Convert.ToInt32(CurrentStatus), Comments, Convert.ToInt32(CurrentID), Convert.ToInt32(CurrentStatus));
            }
            return RedirectToAction("EditProjects");

        }

        //has corrresponding view
        [OnProjectLockStatusChangeActionFilter]
        [OnProjectStatusChangeActionFilter]
        public ActionResult ProcessEditSingleProjectFromManageSingleProject(FormCollection fc)
        {
            string CurrentName = fc["Name"];
            string CurrentStatus = fc["ProjectStatus"];
            string CurrentID = fc["ID"];
            int CurrentLockStatus = Convert.ToInt32(fc["lock"]);
            int OriginalLockStatus = Convert.ToInt32(fc["OriginalLockStatus"]);
            int OriginalProjectStatus = Convert.ToInt32(fc["OriginalProjectStatus"]);
            if (!string.IsNullOrWhiteSpace(CurrentName) && !string.IsNullOrWhiteSpace(fc["ProjectStatus"]))
            {
                string ExcStr = string.Format("update dbo.project set Name='{0}',CurrentProjectStatus={1},IsLocked={3} where ID={2}", CurrentName, CurrentStatus, CurrentID, CurrentLockStatus);
                Scheduling.Database.Utility.ExecuteNonQueryWrapper(ExcStr);
                string HistoryStatus = Scheduling.Database.Utility.GetAllProjectStatuses().Where(x => x.ID == Convert.ToInt32(CurrentStatus)).First().Description;
                string Comments = string.Format("Running Update Statement..Project Name ={0},Status={1},LockStatus={2}", CurrentName, HistoryStatus, CurrentLockStatus);

                Scheduling.Database.Utility.CreateProjectHistoryEntry(Convert.ToInt32(CurrentStatus), Comments, Convert.ToInt32(CurrentID), Convert.ToInt32(CurrentStatus));
            }


            List<EditProjectWithMilestones> EditList = new List<EditProjectWithMilestones>();
            ProjectDisplay p = Scheduling.Database.Utility.GetAllProjects().Where(x => x.ID == Convert.ToInt32(CurrentID)).First();

            EditProjectWithMilestones epwm = new EditProjectWithMilestones();
            epwm.ID = p.ID;
            epwm.Name = p.Name;
            epwm.Year = Scheduling.Database.Utility.GetAllYears().Where(x => x.ID == p.YearFK).First().Value.ToString();
            epwm.DateCreated = p.DateCreated.ToLongDateString();
            epwm.CurrentVersion = p.CurrentVersion;
            epwm.CurrentProjectStatus = p.CurrentProjectStatus;
            epwm.IsLocked = p.IsLocked;

            //Get the milestones
            List<MilestoneValue> MilValList = Scheduling.Database.Utility.GetMilestoneValuesByProjectIDOrderedByDueDateDesc(p.ID);
            epwm.MileValueList = MilValList;

            //Gen Strongly Typed View
            EditList.Add(epwm);
            ViewBag.StatusList = Scheduling.Database.Utility.GetAllProjectStatuses();
            return View("ManageSingleProject", EditList);

        }

        public ActionResult EditProjects(FormCollection fc)
        {
            List<ProjectDisplay> ProdDisplayList = Scheduling.Database.Utility.GetAllProjects();

            if (!string.IsNullOrEmpty(fc["timeline"]))
            {
                int CurrentTimeline = Convert.ToInt32(fc["timeline"]);
                ProdDisplayList = ProdDisplayList.Where(x => x.ProjectRangeFK == CurrentTimeline).ToList();
                ViewBag.CurrentTimeline = CurrentTimeline;
            }

            if (!string.IsNullOrEmpty(fc["pubcode"]))
            {
                int CurrentPubcode = Convert.ToInt32(fc["pubcode"]);
                ProdDisplayList = ProdDisplayList.Where(y => y.PubCodeFK == CurrentPubcode).ToList();
                ViewBag.CurrentPubcode = CurrentPubcode;
            }

            if (!string.IsNullOrEmpty(fc["year"]))
            {
                int CurrentYear = Convert.ToInt32(fc["year"]);
                ProdDisplayList = ProdDisplayList.Where(z => z.YearFK == CurrentYear).ToList();
                ViewBag.CurrentYear = CurrentYear;
            }

            List<EditProjectWithMilestones> EditList = new List<EditProjectWithMilestones>();

            if (ProdDisplayList.Count > 0)
            {

                foreach (ProjectDisplay p in ProdDisplayList)
                {
                    EditProjectWithMilestones epwm = new EditProjectWithMilestones();
                    epwm.ID = p.ID;
                    epwm.Name = p.Name;
                    epwm.Year = Scheduling.Database.Utility.GetAllYears().Where(x => x.ID == p.YearFK).First().Value.ToString();
                    epwm.DateCreated = p.DateCreated.ToLongDateString();
                    epwm.CurrentVersion = p.CurrentVersion;
                    epwm.CurrentProjectStatus = p.CurrentProjectStatus;

                    //Get the milestones
                    List<MilestoneValue> MilValList = Scheduling.Database.Utility.GetMilestoneValuesByProjectID(p.ID);
                    epwm.MileValueList = MilValList;
                    //Gen Strongly Typed View
                    EditList.Add(epwm);
                }

            }

            ViewBag.StatusList = Scheduling.Database.Utility.GetAllProjectStatuses();
            return View(EditList);
        }

        public ActionResult EditUsers()
        {

            List<User> UserList = Scheduling.Database.Utility.GetAllUsers();
            ViewBag.Roles = Scheduling.Database.Utility.GetAllRoles();
            ViewBag.Groups = Scheduling.Database.Utility.GetAllGroups();
            return View(UserList);
        }

        public ActionResult EditSingleUser(FormCollection fc)
        {
            string CurrentRole = fc["role"];
            string CurrentID = fc["id"];
            string ComStr = string.Format("update dbo.[User] set RoleFk={0} where ID={1}", CurrentRole, CurrentID);
            Scheduling.Database.Utility.ExecuteNonQueryWrapper(ComStr);

            string CurrentGroups = fc["groups"];

            //allow ability to remove from group
            string GroupExcStr = string.Format("delete from UserToGroups where UserID={0};", CurrentID);
            Scheduling.Database.Utility.ExecuteNonQueryWrapper(GroupExcStr);

            if (!string.IsNullOrWhiteSpace(CurrentGroups))
            {


                if (CurrentGroups.Contains(','))
                {
                    string[] GroupSplStr = CurrentGroups.Split(',');
                    foreach (string s in GroupSplStr)
                    {

                        string CurrentExcStr = string.Format("insert into UserToGroups (UserID,GroupID) values({0},{1})", CurrentID, s);
                        Scheduling.Database.Utility.ExecuteNonQueryWrapper(CurrentExcStr);

                    }


                }


                else
                {
                    string CurrentExcStr = string.Format("insert into UserToGroups (UserID,GroupID) values({0},{1})", CurrentID, CurrentGroups);
                    Scheduling.Database.Utility.ExecuteNonQueryWrapper(CurrentExcStr);


                }
            }
            return RedirectToAction("EditUsers");

        }

        public ActionResult EditSingleMilestoneTreeProfileSubItems(FormCollection fc)
        {
            int CurrentProfile = Convert.ToInt32(fc["id"]);
            MilestoneTreeSettingsProfile mtsp = Scheduling.Database.Utility.GetAllMilestoneTreeSettingsProfiles().Where(x => x.ID == CurrentProfile).First();
            List<MilestoneTreeSetting> MtsList = Scheduling.Database.Utility.GetMilestoneTreeSettingsByProfileID(CurrentProfile).OrderBy(x => x.DisplayOrder).ToList();
            List<MilestoneTreeSetting> MtsHasParentList = MtsList.Where(x => x.MilestoneParentField != null).ToList();

            //filter out the milestone fields that are used in this profile
            List<int> ids = MtsList.Select(x => x.MilestoneField).Distinct().ToList();
            IEnumerable<MilestoneField> ParentDropdownFieldEnum = Scheduling.Database.Utility.GetAllParentMilestoneFields().Where(x => ids.Contains(x.ID));
            List<MilestoneField> ParentDropdownFieldList = ParentDropdownFieldEnum.ToList();

            ViewBag.mtsp = mtsp;
            ViewBag.MtsHasParentList = MtsHasParentList;
            ViewBag.ParentDropdownFieldList = ParentDropdownFieldList;
            ViewBag.CalcList = Scheduling.Database.Utility.GetAllCalculationFields();
            ViewBag.ErrorMessage = string.Empty;

            //We are providing adding the parent desc to the subitem for the dependency dropdown
            List<MilestoneTreeSetting> DepList = MtsList;
            List<MilestoneField> DepMfList = new List<MilestoneField>();
            foreach (MilestoneTreeSetting mts in DepList)
            {
                MilestoneField cmf = new MilestoneField();
                cmf.ID = mts.MilestoneField;

                string Desc = Scheduling.Database.Utility.GetMilestoneDescFromID(mts.MilestoneField);
                if (mts.MilestoneParentField.HasValue)
                {
                    string ParentDesc = Scheduling.Database.Utility.GetMilestoneDescFromID((int)mts.MilestoneParentField);
                    Desc = string.Format(" {0} ( {1} )", Desc, ParentDesc);
                }

                cmf.Description = Desc;
                DepMfList.Add(cmf);

            }

            //ParentIds
            List<int?> ParentIds = MtsHasParentList.Select(x => x.MilestoneParentField).Distinct().ToList();
            ViewBag.ParentIds = ParentIds;
            ViewBag.DepList = DepMfList;

            return View();

        }
        [ValidateInput(false)]
        public ActionResult ProcessEditSingleMilestoneReportFooter(FormCollection fc)
        {
            int ProfileID = Convert.ToInt32(fc["id"]);
            string InputTextArea = string.Empty;
            if (!string.IsNullOrWhiteSpace(fc["note"]))
            {
                //steve issue 8/22/14
                // InputTextArea = Scheduling.Html.SanitizeUtility.SanitizeFromWhitelist(fc["note"]);
                InputTextArea = fc["note"];
            }

            Scheduling.Database.Utility.CreateReportFooterNoteForBaseline(ProfileID, InputTextArea);
            string CurrentBaseline = Scheduling.Database.Utility.GetProfileDescFromID(ProfileID);
            string ActivityStr = string.Format("Update {0} Report Footer To {1}", CurrentBaseline, InputTextArea);
            Scheduling.Database.Utility.CreateActivityLogEntry(ActivityStr);
            return RedirectToAction("ManageMilestoneTreeSettingProfiles");
        }

        public ActionResult EditSingleMilestoneReportFooter(FormCollection fc)
        {
            int CurrentProfileID = Convert.ToInt32(fc["id"]);
            Scheduling.Models.MilestoneTreeSettingsProfile mtsp = Scheduling.Database.Utility.GetAllMilestoneTreeSettingsProfiles().Where(x => x.ID == CurrentProfileID).First();
            return View(mtsp);


        }

        public ActionResult EditSingleMilestoneTreeProfile(FormCollection fc)
        {
            int CurrentProfile = Convert.ToInt32(fc["id"]);
            List<MilestoneTreeSetting> MtsList = Scheduling.Database.Utility.GetMilestoneTreeSettingsByProfileID(CurrentProfile).OrderBy(x => x.DisplayOrder).ToList();
            MilestoneTreeSettingsProfile mtsp = Scheduling.Database.Utility.GetAllMilestoneTreeSettingsProfiles().Where(x => x.ID == CurrentProfile).First();

            List<MilestoneTreeSetting> MtsNoParentList = MtsList.Where(x => x.MilestoneParentField == null).ToList();

            //View Bag For mfl
            ViewBag.MilestoneFieldList = Scheduling.Database.Utility.GetAllParentMilestoneFields();

            //calc list 
            ViewBag.CalcList = Scheduling.Database.Utility.GetAllCalculationFields();

            //firing order list
            ViewBag.FiringOrderList = Scheduling.Database.Utility.CreateFiringOrderList();

            //date range calculation
            ViewBag.DateRangeCalcList = Scheduling.Database.Utility.GetAllCalculationFields();

            ViewBag.MilestoneTreeSettingsProfile = mtsp;
            return View(MtsNoParentList);

        }

        public ActionResult AddSingleUser()
        {
            ViewBag.Roles = Scheduling.Database.Utility.GetAllRoles();
            return View();
        }

        public ActionResult EditSingleMilestoneTreeSubItemFiringOrder(FormCollection fc)
        {

            int CurrentID = Convert.ToInt32(fc["ID"]);
            int CurrentProfile = Convert.ToInt32(fc["ProfileID"]);
            int CurrentFiringOrder = Convert.ToInt32(fc["CalcFiringOrder"]);

            //start update 

            string ComText = string.Empty;
            if (!string.IsNullOrWhiteSpace(fc["CalcID"]))
            {
                int CurrentCalcID = Convert.ToInt32(fc["CalcID"]);
                ComText = string.Format("update dbo.MilestoneTreeSettings set CalcFiringOrder={0},CalculationID={2}  where ID={1}", CurrentFiringOrder, CurrentID, CurrentCalcID);
            }

            else
            {
                ComText = string.Format("update dbo.MilestoneTreeSettings set CalcFiringOrder={0}  where ID={1}", CurrentFiringOrder, CurrentID);

            }

            Scheduling.Database.Utility.ExecuteNonQueryWrapper(ComText);

            //stop update


            //rebuild info and return View
            MilestoneTreeSettingsProfile mtsp = Scheduling.Database.Utility.GetAllMilestoneTreeSettingsProfiles().Where(x => x.ID == CurrentProfile).First();
            List<MilestoneTreeSetting> MtsList = Scheduling.Database.Utility.GetMilestoneTreeSettingsByProfileID(CurrentProfile).OrderBy(x => x.DisplayOrder).ToList();
            List<MilestoneTreeSetting> MtsHasParentList = MtsList.Where(x => x.MilestoneParentField != null).ToList();

            //filter out the milestone fields that are used in this profile
            List<int> ids = MtsList.Select(x => x.MilestoneField).Distinct().ToList();
            IEnumerable<MilestoneField> ParentDropdownFieldEnum = Scheduling.Database.Utility.GetAllParentMilestoneFields().Where(x => ids.Contains(x.ID));
            List<MilestoneField> ParentDropdownFieldList = ParentDropdownFieldEnum.ToList();

            ViewBag.mtsp = mtsp;
            ViewBag.MtsHasParentList = MtsHasParentList;
            ViewBag.ParentDropdownFieldList = ParentDropdownFieldList;
            ViewBag.CalcList = Scheduling.Database.Utility.GetAllCalculationFields();
            ViewBag.ErrorMessage = string.Empty;

            //dep fields 
            //We are providing adding the parent desc to the subitem for the dependency dropdown
            List<MilestoneTreeSetting> DepList = MtsList;
            List<MilestoneField> DepMfList = new List<MilestoneField>();
            foreach (MilestoneTreeSetting mts in DepList)
            {
                MilestoneField cmf = new MilestoneField();
                cmf.ID = mts.MilestoneField;

                string Desc = Scheduling.Database.Utility.GetMilestoneDescFromID(mts.MilestoneField);
                if (mts.MilestoneParentField.HasValue)
                {
                    string ParentDesc = Scheduling.Database.Utility.GetMilestoneDescFromID((int)mts.MilestoneParentField);
                    Desc = string.Format(" {0} ( {1} )", Desc, ParentDesc);
                }

                cmf.Description = Desc;
                DepMfList.Add(cmf);

            }
            ViewBag.DepList = DepMfList;
            //

            //ParentIds
            List<int?> ParentIds = MtsHasParentList.Select(x => x.MilestoneParentField).Distinct().ToList();
            ViewBag.ParentIds = ParentIds;



            return View("EditSingleMilestoneTreeProfileSubItems");
        }

        public ActionResult DeleteSingleMilestoneTreeSubItem(FormCollection fc)
        {
            int CurrentID = Convert.ToInt32(fc["ID"]);
            int CurrentProfile = Convert.ToInt32(fc["ProfileID"]);

            string ComText = string.Format("delete from dbo.MilestoneTreeSettings where ID={0}", CurrentID);
            Scheduling.Database.Utility.ExecuteNonQueryWrapper(ComText);

            //rebuild info and return View
            MilestoneTreeSettingsProfile mtsp = Scheduling.Database.Utility.GetAllMilestoneTreeSettingsProfiles().Where(x => x.ID == CurrentProfile).First();
            List<MilestoneTreeSetting> MtsList = Scheduling.Database.Utility.GetMilestoneTreeSettingsByProfileID(CurrentProfile).OrderBy(x => x.DisplayOrder).ToList();
            List<MilestoneTreeSetting> MtsHasParentList = MtsList.Where(x => x.MilestoneParentField != null).ToList();

            //filter out the milestone fields that are used in this profile
            List<int> ids = MtsList.Select(x => x.MilestoneField).Distinct().ToList();
            IEnumerable<MilestoneField> ParentDropdownFieldEnum = Scheduling.Database.Utility.GetAllParentMilestoneFields().Where(x => ids.Contains(x.ID));
            List<MilestoneField> ParentDropdownFieldList = ParentDropdownFieldEnum.ToList();

            ViewBag.mtsp = mtsp;
            ViewBag.MtsHasParentList = MtsHasParentList;
            ViewBag.ParentDropdownFieldList = ParentDropdownFieldList;
            ViewBag.CalcList = Scheduling.Database.Utility.GetAllCalculationFields();
            ViewBag.ErrorMessage = string.Empty;

            //start Create DepList
            List<MilestoneTreeSetting> DepList = MtsList;
            List<MilestoneField> DepMfList = new List<MilestoneField>();
            foreach (MilestoneTreeSetting mts in DepList)
            {
                MilestoneField cmf = new MilestoneField();
                cmf.ID = mts.MilestoneField;

                string Desc = Scheduling.Database.Utility.GetMilestoneDescFromID(mts.MilestoneField);
                if (mts.MilestoneParentField.HasValue)
                {
                    string ParentDesc = Scheduling.Database.Utility.GetMilestoneDescFromID((int)mts.MilestoneParentField);
                    Desc = string.Format(" {0} ( {1} )", Desc, ParentDesc);
                }

                cmf.Description = Desc;
                DepMfList.Add(cmf);

            }
            ViewBag.DepList = DepMfList;

            //stop

            //ParentIds
            List<int?> ParentIds = MtsHasParentList.Select(x => x.MilestoneParentField).Distinct().ToList();
            ViewBag.ParentIds = ParentIds;



            return View("EditSingleMilestoneTreeProfileSubItems");


        }

        public ActionResult DeleteSingleUser(FormCollection fc)
        {
            int CurrentID = Convert.ToInt32(fc["hfDeleteSingleUser"]);

            string DelUsername = Scheduling.Database.Utility.GetAllUsers().Where(x => x.ID == CurrentID).First().UserName;
            string LogStr = string.Format("Removing User {0}", DelUsername);

            string ComText = string.Format("delete from dbo.[User] where ID={0}", CurrentID);
            Scheduling.Database.Utility.ExecuteNonQueryWrapper(ComText);

            Scheduling.Database.Utility.CreateActivityLogEntry(LogStr);
            return RedirectToAction("EditUsers");

        }


        public ActionResult VerifySingleMilestoneTreeProfileForMagazineType(FormCollection fc)
        {
            int CurrentBaseline = Convert.ToInt32(fc["ID"]);
            string Message = Scheduling.Database.Utility.VerifySingleMilestoneTreeProfileForMagazineType(CurrentBaseline);
            ViewBag.CurrentProfile = Scheduling.Database.Utility.GetProfileDescFromID(CurrentBaseline);
            ViewBag.CurrentMessage = Message;
            return View();

        }

        public ActionResult VerifySingleMilestoneTreeProfileForNonMagazineType(FormCollection fc)
        {
            int CurrentBaseline = Convert.ToInt32(fc["ID"]);
            string Message = Scheduling.Database.Utility.VerifySingleMilestoneTreeProfileForNonMagazineType(CurrentBaseline);
            ViewBag.CurrentProfile = Scheduling.Database.Utility.GetProfileDescFromID(CurrentBaseline);
            ViewBag.CurrentMessage = Message;
            return View();

        }

        public ActionResult ViewAllProjectHistory(FormCollection fc = null)
        {
            List<int> DistinctProjects = Database.Utility.GetAllProjects().OrderByDescending(x => x.DateCreated).Select(x => x.ID).ToList();
            return View(DistinctProjects);

        }

        public ActionResult ProcessAddSingleUser(User u, FormCollection fc)
        {
            //Username has required attribute on user model..
            if (ModelState.IsValid)
            {
                string CurrentEmail = "N/A";
                if (!string.IsNullOrWhiteSpace(u.Email))
                {
                    CurrentEmail = u.Email;
                }

                //This will default to requestor
                int RoleFK = Convert.ToInt32(u.RoleFK);
                string CurrentUsername = u.UserName;

                Scheduling.Database.Utility.CreateNewUser(CurrentEmail, CurrentUsername, RoleFK);
                return RedirectToAction("EditUsers");


            }

            else
            {
                ViewBag.Roles = Scheduling.Database.Utility.GetAllRoles();
                return View("AddSingleUser");

            }


        }


    }







}
