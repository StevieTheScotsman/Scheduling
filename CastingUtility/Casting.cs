using System.Collections.Generic;
using System.Linq;
using Scheduling.Models;

namespace Scheduling.CastingFunctions
{
    public class Utility
    {


        public static List<MilestoneTreeSettingsProfileDisplay> ConvertToMilestoneTreeSettingsProfileDisplay(List<MilestoneTreeSettingsProfile> InputList)
        {
            List<MilestoneTreeSettingsProfileDisplay> MtsDisplayList = new List<MilestoneTreeSettingsProfileDisplay>();

            if (InputList.Count > 0)
            {
                foreach (MilestoneTreeSettingsProfile mts in InputList)
                {
                    MilestoneTreeSettingsProfileDisplay pd = new MilestoneTreeSettingsProfileDisplay();
                    pd.ID = mts.ID;
                    pd.Description = mts.Description;
                    pd.ProjectProfileTypeName = Scheduling.Database.Utility.GetAllProjectProfileTypes().Where(x => x.ID == mts.ProjectType).First().Description;
                    MtsDisplayList.Add(pd);

                }
            }

            return MtsDisplayList;
        }



        public static List<ProjectLinkViewModel> ConvertProjectToProjectLinkViewModelForProjectLinkDropdown(List<ProjectDisplay> InputList)
        {
            List<ProjectLinkViewModel> RetList = new List<ProjectLinkViewModel>();
            if (InputList.Count > 0)
            {
                foreach (ProjectDisplay pd in InputList)
                {

                    ProjectLinkViewModel vm = new ProjectLinkViewModel();
                    vm.ID = pd.ID;
                    vm.Name = pd.Name;
                    vm.Year = Scheduling.Database.Utility.GetAllYears().Where(x => x.ID == pd.YearFK).First().Value.ToString();
                    RetList.Add(vm);
                }

            }
            return RetList;

        }



        public static List<ProjectNewstandCSV> ConvertProjectNewstandToProjectNewstandCSV(List<ProjectNewstand> InputList)
        {
            List<ProjectNewstandCSV> RetList = new List<ProjectNewstandCSV>();

            //header Comment out for now
            ProjectNewstandCSV CsvHeader = new ProjectNewstandCSV();
            CsvHeader.NewstandDate = Scheduling.StringFunctions.Utility.PrepareCsvField(Scheduling.StringFunctions.Utility.GetAppSettingValue("ProjectNewstandCsvColOneHeader"));
            CsvHeader.ProjectName = Scheduling.StringFunctions.Utility.PrepareCsvField(Scheduling.StringFunctions.Utility.GetAppSettingValue("ProjectNewstandCsvColTwoHeader"));
            RetList.Add(CsvHeader);

            foreach (ProjectNewstand pn in InputList)
            {
                ProjectNewstandCSV pnc = new ProjectNewstandCSV();
                pnc.NewstandDate = Scheduling.StringFunctions.Utility.PrepareCsvField(pn.NewstandDate);
                pnc.ProjectName = Scheduling.StringFunctions.Utility.PrepareCsvField(pn.ProjectName);
                RetList.Add(pnc);

            }

            return RetList;

        }

        public static List<DeptToGroupsToPubCodeDisplay> ConvertDeptToGroupsToPubCodeToDisplay(List<DeptToGroupsToPubCode> InputList)
        {

            List<DeptToGroupsToPubCodeDisplay> RetList = new List<DeptToGroupsToPubCodeDisplay>();

            List<Department> DeptList = Scheduling.Database.Utility.GetAllDepartments();
            List<Group> GroupList = Scheduling.Database.Utility.GetAllGroups();
            List<PublicationCode> PubCodeList = Scheduling.Database.Utility.GetAllPublicationCodes();

            foreach (DeptToGroupsToPubCode item in InputList)
            {
                DeptToGroupsToPubCodeDisplay d = new DeptToGroupsToPubCodeDisplay();
                d.ID = item.ID;
                d.GroupName = GroupList.Where(g => g.ID == item.GroupID).First().Description;
                d.DeptName = DeptList.Where(p => p.ID == item.DeptID).First().Description;
                d.PubCodeName = PubCodeList.Where(u => u.ID == item.PubCodeID).First().ShortDesc;
                RetList.Add(d);
            }
            return RetList;
        }

        public static List<ProjectLinkDisplay> ConvertProjectLinkToDisplay(List<ProjectLink> InputList)
        {

            List<ProjectLinkDisplay> RetList = new List<ProjectLinkDisplay>();

            foreach (ProjectLink p in InputList)
            {

                ProjectLinkDisplay pd = new ProjectLinkDisplay();
                //Project Names

                pd.PrimaryProject = Scheduling.Database.Utility.GetAllProjects().Where(x => x.ID == p.PrimaryProjectID).First().Name;
                pd.SecondaryProject = Scheduling.Database.Utility.GetAllProjects().Where(x => x.ID == p.SecondaryProjectID).First().Name;

                //Primary Profile Name for Projects
                int CurrentPrimaryProfileID = (int)Scheduling.Database.Utility.GetAllProjects().Where(x => x.ID == p.PrimaryProjectID).First().MilestoneTreeSettingsProfileFK;
                int PrimProfileType = Scheduling.Database.Utility.GetAllMilestoneTreeSettingsProfiles().Where(x => x.ID == CurrentPrimaryProfileID).First().ProjectType;

                pd.PrimaryProjectProfileType = Scheduling.Database.Utility.GetAllProjectProfileTypes().Where(x => x.ID == PrimProfileType).First().Description;

                //secondary Profile Name

                int CurrentSecondaryProfileID = (int)Scheduling.Database.Utility.GetAllProjects().Where(x => x.ID == p.SecondaryProjectID).First().MilestoneTreeSettingsProfileFK;
                int SecProfileType = Scheduling.Database.Utility.GetAllMilestoneTreeSettingsProfiles().Where(x => x.ID == CurrentSecondaryProfileID).First().ProjectType;

                pd.SecondaryProjectProfileType = Scheduling.Database.Utility.GetAllProjectProfileTypes().Where(x => x.ID == SecProfileType).First().Description;
                pd.ID = p.ID;
                pd.PrimaryProjectID = p.PrimaryProjectID;
                pd.SecondaryProjectID = p.SecondaryProjectID;
                RetList.Add(pd);
            }

            return RetList;
        }

        public static List<ProjectLinkSettingDisplay> ConvertProjectLinkSettingToDisplay(List<ProjectLinkSetting> InputList)
        {

            List<ProjectLinkSettingDisplay> DisList = new List<ProjectLinkSettingDisplay>();

            foreach (ProjectLinkSetting item in InputList)
            {

                ProjectLinkSettingDisplay psd = new ProjectLinkSettingDisplay();
                psd.ID = item.ID;
                psd.PrimaryMilestone = Scheduling.Database.Utility.GetAllMilestoneFields().Where(x => x.ID == item.PrimaryMilestoneID).First().Description;
                psd.PrimaryProfileType = Scheduling.Database.Utility.GetAllProjectProfileTypes().Where(x => x.ID == item.PrimaryProfileTypeID).First().Description;
                psd.SecondaryMilestone = Scheduling.Database.Utility.GetAllMilestoneFields().Where(x => x.ID == item.SecondaryMilestoneID).First().Description;
                psd.SecondaryProfileType = Scheduling.Database.Utility.GetAllProjectProfileTypes().Where(x => x.ID == item.SecondaryProfileTypeID).First().Description;
                psd.Calculation = Scheduling.Database.Utility.GetAllCalculationFields().Where(x => x.ID == item.CalculationID).First().ShortDesc;
                psd.Name = item.Name;

                string PubCodeStr = "N/A";
                if (item.PubCode.HasValue) { PubCodeStr = Scheduling.Database.Utility.GetAllPublicationCodes().Where(x => x.ID == item.PubCode).First().ShortDesc; }
                psd.PubCode = PubCodeStr;

                string TimelineStr = "N/A";
                if (item.Timeline.HasValue) { TimelineStr = Scheduling.Database.Utility.GetAllProjectRanges().Where(x => x.ID == item.Timeline).First().ShortDesc; }
                psd.Timeline = TimelineStr;

                DisList.Add(psd);
            }
            return DisList;

        }

        public static List<MessagingSettingDisplay> ConvertMessageSettingToMessageDisplaySetting(List<MessagingSetting> MsList)
        {
            List<MessagingSettingDisplay> DisList = new List<MessagingSettingDisplay>();

            List<MessagingEvent> EventList = Scheduling.Database.Utility.GetAllMessagingEvents();
            List<MessagingAction> ActionList = Scheduling.Database.Utility.GetAllMessagingActions();

            List<User> UserList = Scheduling.Database.Utility.GetAllUsers();
            List<Role> RoleList = Scheduling.Database.Utility.GetAllRoles();
            List<Group> GroupList = Scheduling.Database.Utility.GetAllGroups();
            List<Department> DeptList = Scheduling.Database.Utility.GetAllDepartments();

            foreach (MessagingSetting ms in MsList)
            {
                string CurrentEventName = EventList.Where(x => x.ID == ms.EventFK).First().ShortDesc;
                string CurrentActionName = ActionList.Where(y => y.ID == ms.ActionFk).First().ShortDesc;

                string CurrentUser = "N/A";
                if (ms.UserFK.HasValue)
                {
                    CurrentUser = UserList.Where(x => x.ID == ms.UserFK).First().UserName;
                }

                string CurrentRole = "N/A";
                if (ms.RoleFk.HasValue)
                {
                    CurrentRole = RoleList.Where(y => y.ID == ms.RoleFk).First().ShortDesc;

                }

                string CurrentGroup = "N/A";
                if (ms.GroupFK.HasValue)
                {
                    CurrentGroup = GroupList.Where(x => x.ID == ms.GroupFK).First().Description;
                }

                string CurrentDept = "N/A";
                if (ms.DeptFK.HasValue)
                {
                    CurrentDept = DeptList.Where(y => y.ID == ms.DeptFK).First().Description;

                }

                //create object
                MessagingSettingDisplay msd = new MessagingSettingDisplay();
                msd.EventName = CurrentEventName;
                msd.ActionName = CurrentActionName;
                msd.ID = ms.ID;
                msd.RoleName = CurrentRole;
                msd.UserName = CurrentUser;
                msd.GroupName = CurrentGroup;
                msd.DeptName = CurrentDept;

                DisList.Add(msd);

            }

            return DisList;
        }
    }
}