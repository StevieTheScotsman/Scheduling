using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Scheduling.Models;
using System.Linq.Expressions;


namespace Scheduling.Database
{

    //equality start
    //http://msdn.microsoft.com/en-us/library/vstudio/bb336390(v=vs.100).aspx
    // Custom comparer for the Projectclass 
    public class ProjectComparer : IEqualityComparer<ProjectDisplay>
    {
        // Projects are equal if their ids are equal
        public bool Equals(ProjectDisplay x, ProjectDisplay y)
        {
            return x.ID == y.ID;

        }


        public int GetHashCode(ProjectDisplay pd)
        {
            return pd.ID.GetHashCode();
        }

    }


    //stop

    public class Utility
    {

        //generate reports view model
        public static ReportProjectDisplayViewModel GetReportProjectDisplayViewModel(FormCollection fc)
        {


            return new ReportProjectDisplayViewModel();
        }

        //helper function

        public static string GetBaselineDescByID(int i)
        {
            Scheduling.Models.MilestoneTreeSettingsProfileDisplay mpd = new MilestoneTreeSettingsProfileDisplay();
            mpd.ID = i;
            string CommandText = string.Format("select [description] from dbo.MilestoneTreeSettingsProfile where id={0}", i);
            mpd.Description = ExecuteScalarWrapper(CommandText).ToString();
            return mpd.Description;

        }
        public static Dictionary<string, string> GetAssignedBaselines()
        {
            Dictionary<string, string> RetList = new Dictionary<string, string>();

            string ComText = string.Format("select id,description from dbo.MilestoneTreeSettingsProfile where id in(select distinct milestonetreesettingsprofilefk from dbo.project) order by description");

            SqlCommand com = new SqlCommand(ComText, GetConnection());
            SqlDataReader sdr = null;

            try
            {
                com.Connection.Open();
                sdr = com.ExecuteReader();

                while (sdr.Read())
                {

                    if (sdr.HasRows)
                    {
                        RetList.Add(sdr["id"].ToString(), sdr["description"].ToString());

                    }


                }

            }

            catch (Exception e)
            {
                Scheduling.Database.Utility.CreateApplicationErrorLoggingEntry(e.Message);

            }

            finally
            {

                if (!sdr.IsClosed) sdr.Close();
                if (com.Connection.State == ConnectionState.Open) com.Connection.Close();
                com.Dispose();

            }

            return RetList;
        }


        public static Dictionary<string, string> GetUnassignedBaselines()
        {
            Dictionary<string, string> RetList = new Dictionary<string, string>();

            string ComText = string.Format("select id,description from dbo.MilestoneTreeSettingsProfile where id not in(select distinct milestonetreesettingsprofilefk from dbo.project) order by description");

            SqlCommand com = new SqlCommand(ComText, GetConnection());
            SqlDataReader sdr = null;

            try
            {
                com.Connection.Open();
                sdr = com.ExecuteReader();

                while (sdr.Read())
                {

                    if (sdr.HasRows)
                    {
                        RetList.Add(sdr["id"].ToString(), sdr["description"].ToString());

                    }


                }

            }

            catch (Exception e)
            {

                Scheduling.Database.Utility.CreateApplicationErrorLoggingEntry(e.Message);
            }

            finally
            {

                if (!sdr.IsClosed) sdr.Close();
                if (com.Connection.State == ConnectionState.Open) com.Connection.Close();
                com.Dispose();

            }

            return RetList;
        }
        public static int? GetNullableIntValueFromDbField(object o)
        {
            if (o is DBNull) return null;
            return Convert.ToInt32(o);
        }


        public static DateTime? GetNullableDateTimeValueFromDbField(object o)
        {
            if (o is DBNull) return null;
            return Convert.ToDateTime(o);
        }

        public static string GetNullableStringValueFromDbField(object o)
        {

            if (o is DBNull) return null;
            return string.Empty;
        }

        public static string GetStringValueForPossibleDBNullField(object o)
        {
            if (o is DBNull)
            {
                return string.Empty;
            }

            return o.ToString();


        }

        public static bool MilestoneProfileHasSettings(int ProfileID)
        {

            string ComText = string.Format("select count(*) from dbo.MilestoneTreeSettings where MilestoneTreeSettingsProfileFK={0}", ProfileID);
            return Convert.ToInt32(ExecuteScalarWrapper(ComText)) > 0;

        }

        public static void CloneExistingMilestoneProfile(int SourceID, int SourceProjectType, string TargetDescription)
        {

            string PrevStr = "select isnull(max(ID),0) from dbo.MilestoneTreeSettingsProfile";
            int PrevVal = Convert.ToInt32(ExecuteScalarWrapper(PrevStr));

            //bring over report footer
            string InsertFooterNote = string.Empty;
            string CurrentFooterNote = Scheduling.Database.Utility.GetAllMilestoneTreeSettingsProfiles().Where(x => x.ID == SourceID).First().ReportFooterNote;

            if (!string.IsNullOrWhiteSpace(CurrentFooterNote))
            {
                InsertFooterNote = CurrentFooterNote;
            }

            string InsStr = string.Format("insert into dbo.MilestoneTreeSettingsProfile (Description,ProfileTypeFK,ReportFooterNote) values('{0}',{1},'{2}');", TargetDescription, SourceProjectType, InsertFooterNote);

            ExecuteNonQueryWrapper(InsStr);

            string NextStr = "select isnull(max(ID),0) from dbo.MilestoneTreeSettingsProfile";
            int NextVal = Convert.ToInt32(ExecuteScalarWrapper(NextStr));

            if (NextVal > PrevVal)
            {
                string ExcStr = string.Format("insert into dbo.MilestoneTreeSettings (MilestoneFieldFK,MilestoneFieldParentFK,DependantUpon,CalculationID,CalcFiringOrder,RangeCalculationID,MilestoneTreeSettingsProfileFK,DisplayOrder) select MilestoneFieldFK,MilestoneFieldParentFK,DependantUpon,CalculationID,CalcFiringOrder,RangeCalculationID,{0},DisplayOrder from dbo.MilestoneTreeSettings where MilestoneTreeSettingsProfileFK={1}", NextVal, SourceID);
                ExecuteNonQueryWrapper(ExcStr);
            }


        }

        public static void CreateNewLinkSetting(ProjectLinkSetting pls)
        {

            SqlCommand com = new SqlCommand();
            com.CommandText = @"insert into dbo.[ProjectLinkSettings] (Name,PrimaryProfileTypeID,PrimaryMilestoneID,SecondaryProfileTypeID,SecondaryMilestoneID,CalculationID,PubCodeID,TimelineID,EntryDate,CreatedBy)" +
                " values (@Name,@PrimaryProfileTypeID,@PrimaryMilestoneID,@SecondaryProfileTypeID,@SecondaryMilestoneID,@CalculationID,@PubCodeID,@TimelineID,getdate(),@CreatedBy)";

            com.Parameters.AddWithValue("@Name", pls.Name);
            com.Parameters.AddWithValue("@PrimaryProfileTypeID", pls.PrimaryProfileTypeID);
            com.Parameters.AddWithValue("@PrimaryMilestoneID", pls.PrimaryMilestoneID);
            com.Parameters.AddWithValue("@SecondaryProfileTypeID", pls.SecondaryProfileTypeID);
            com.Parameters.AddWithValue("@SecondaryMilestoneID", pls.SecondaryMilestoneID);
            com.Parameters.AddWithValue("@CalculationID", pls.CalculationID);

            //pubcode

            if (pls.PubCode.HasValue)
            {
                com.Parameters.AddWithValue("@PubCodeID", pls.PubCode.Value);

            }

            else
            {
                com.Parameters.AddWithValue("@PubCodeID", DBNull.Value);
            }

            //timeline

            if (pls.Timeline.HasValue)
            {
                com.Parameters.AddWithValue("@TimelineID", pls.Timeline.Value);

            }

            else
            {
                com.Parameters.AddWithValue("@TimelineID", DBNull.Value);
            }



            com.Parameters.AddWithValue("@CreatedBy", Scheduling.Security.Utility.GetCurrentLoggedInUser());

            ExecuteSecureCommandNonQueryWrapper(com);
            string LogStr = string.Format("Creating Project Link named {0}", pls.Name);
            CreateActivityLogEntry(LogStr);

        }

        public static void CreateNewMilestoneProfile(string Desc, int ProjectType)
        {

            SqlCommand com = new SqlCommand();
            com.CommandText = @"insert into dbo.[MilestoneTreeSettingsProfile](Description,ProfileTypeFK) values(@Desc,@ProjectType)";
            com.Parameters.AddWithValue("@Desc", Desc);
            com.Parameters.AddWithValue("@ProjectType", ProjectType);

            ExecuteSecureCommandNonQueryWrapper(com);

            string LogStr = string.Format("Adding Milestone Tree Setting Profile {0}", Desc);
            CreateActivityLogEntry(LogStr);

        }


        public static void CreateNewUser(string Email, string Username, int Role)
        {
            SqlCommand com = new SqlCommand();
            com.CommandText = @"insert into dbo.[User](Email,UserName,RoleFK) values(@Email,@Username,@Role)";
            com.Parameters.AddWithValue("@Email", Email);
            com.Parameters.AddWithValue("@Username", Username);
            com.Parameters.AddWithValue("@Role", Role);

            ExecuteSecureCommandNonQueryWrapper(com);

            string LogStr = string.Format("Adding User {0}", Username);
            CreateActivityLogEntry(LogStr);

        }
        public static string GetTotalProjectCount()
        {

            string ComText = "select count(distinct ProjectPK) from dbo.MilestoneValue";
            return ExecuteScalarWrapper(ComText).ToString();

        }

        public static string GetAppVersionNumber()
        {

            return Scheduling.StringFunctions.Utility.GetAppSettingValue("AppVersionNumber");
        }

        //stringify for ease
        public static string GetAvailableLinkedProjectsByProjectLinkSettingID(ProjectLinkSetting PlsInput)
        {
            ProjectLinkSetting pls = GetAllProjectLinkSettings().Where(x => x.ID == PlsInput.ID).First();
            string ComText = string.Format("select * from dbo.Project where MilestoneTreeSettingsProfileFK in (select ID from MilestoneTreeSettingsProfile where ProfileTypeFK = {0}) and id in (select projectpk from dbo.MilestoneValue where MilestoneFieldFK={1} and duedate is not null)", pls.PrimaryProfileTypeID, pls.PrimaryMilestoneID);
            StringBuilder sb = new StringBuilder();
            SqlCommand com = new SqlCommand(ComText, GetConnection());
            SqlDataReader sdr = null;
            List<ProjectDisplay> ProjList = new List<ProjectDisplay>();
            List<ProjectLinkViewModel> DisplayList = new List<ProjectLinkViewModel>();
            try
            {
                com.Connection.Open();
                sdr = com.ExecuteReader();

                while (sdr.Read())
                {

                    if (sdr.HasRows)
                    {
                        ProjectDisplay pd = new ProjectDisplay();
                        pd.ID = Convert.ToInt32(sdr["id"]);
                        pd.Name = sdr["Name"].ToString();
                        pd.YearFK = Convert.ToInt32(sdr["yearfk"]);
                        pd.PubCodeFK = GetNullableIntValueFromDbField(sdr["pubcodefk"]);
                        pd.ProjectRangeFK = GetNullableIntValueFromDbField(sdr["projectrangefk"]);
                        ProjList.Add(pd);

                    }


                }

            }



            finally
            {

                if (!sdr.IsClosed) sdr.Close();
                if (com.Connection.State == ConnectionState.Open) com.Connection.Close();
                com.Dispose();

            }


            if (pls.PubCode.HasValue)
            {
                ProjList = ProjList.Where(x => x.PubCodeFK == pls.PubCode.Value).ToList();

            }

            if (pls.Timeline.HasValue)
            {
                ProjList = ProjList.Where(x => x.ProjectRangeFK == pls.Timeline.Value).ToList();
            }

            if (ProjList.Count > 0)
            {
                DisplayList = Scheduling.CastingFunctions.Utility.ConvertProjectToProjectLinkViewModelForProjectLinkDropdown(ProjList);
                foreach (ProjectLinkViewModel plvm in DisplayList)
                {

                    sb.AppendFormat("{0}|{1}|{2}||", plvm.ID, plvm.Name, plvm.Year);
                }


            }
            string RetStr = sb.ToString();
            if (!string.IsNullOrEmpty(RetStr))
            {
                RetStr = RetStr.Substring(0, RetStr.Length - 2);
            }
            return RetStr;
        }



        public static List<MilestoneField> GetAvailableMajorProjectMilestonesForDropdown(int ProjectID)
        {
            //grab major milestones not being used in the project
            string ComText = string.Format("select * from dbo.milestonefield  where id not in(select  milestonefieldfk from milestonevalue where projectpk={0}) and isCreatedByUser is null", ProjectID);

            SqlCommand com = new SqlCommand(ComText, GetConnection());
            SqlDataReader sdr = null;
            List<MilestoneField> MfList = new List<MilestoneField>();

            //start

            try
            {
                com.Connection.Open();
                sdr = com.ExecuteReader();

                while (sdr.Read())
                {

                    if (sdr.HasRows)
                    {
                        MilestoneField mf = new MilestoneField();
                        mf.Description = sdr["Description"].ToString();
                        mf.ID = Convert.ToInt32(sdr["ID"]);
                        MfList.Add(mf);
                    }


                }


            }


            finally
            {
                if (!sdr.IsClosed) sdr.Close();
                if (com.Connection.State == ConnectionState.Open) com.Connection.Close();
                com.Dispose();


            }


            return MfList;

            //stop

        }

        //Activity Logging Start



        public static void CreateActivityLogEntry(string Message, int ProjectFK)
        {
            string CurrentUsername = Scheduling.Security.Utility.GetCurrentLoggedInUser();
            string ExcStr = string.Format("insert into dbo.Activity (Message,ModifiedBy,ProjectFK,DateModified) values ('{0}','{1}',{2},getdate())", Message, CurrentUsername, ProjectFK);
            Scheduling.Database.Utility.ExecuteNonQueryWrapper(ExcStr);
            TruncateActivityLog();

        }

        public static void CreateActivityLogEntry(string Message)
        {
            string CurrentUsername = Scheduling.Security.Utility.GetCurrentLoggedInUser();
            string ExcStr = string.Format("insert into dbo.Activity (Message,ModifiedBy,DateModified) values ('{0}','{1}',getdate())", Message, CurrentUsername);
            Scheduling.Database.Utility.ExecuteNonQueryWrapper(ExcStr);
            TruncateActivityLog();

        }

        public static void TruncateApplicationErrorLog()
        {
            string Duration = ConfigurationManager.AppSettings["ApplicationErrorLogDuration"];
            string DelStr = string.Format("delete from dbo.ApplicationErrorLogging where DateModified < (getdate()-{0})", Duration);
            Scheduling.Database.Utility.ExecuteNonQueryWrapper(DelStr);

        }

        public static void TruncateApplicationLog()
        {
            string Duration = ConfigurationManager.AppSettings["ApplicationLogDuration"];
            string DelStr = string.Format("delete from dbo.ApplicationLogging where DateModified < (getdate()-{0})", Duration);
            Scheduling.Database.Utility.ExecuteNonQueryWrapper(DelStr);

        }

        public static void TruncateActivityLog()
        {
            string Duration = ConfigurationManager.AppSettings["ActivityLogDuration"];
            string DelStr = string.Format("delete from dbo.Activity where DateModified < (getdate()-{0})", Duration);
            Scheduling.Database.Utility.ExecuteNonQueryWrapper(DelStr);
        }

        public static void CreateProjectChangeRequest(string Comment, string ProjectID)
        {

            SqlCommand com = new SqlCommand();
            string CurrentUser = Scheduling.Security.Utility.GetCurrentLoggedInUser();
            com.CommandText = @"insert into dbo.[ProjectChangeRequest](ProjectFK,RequestorComment,CreatedBy,DateRequested,RequestStatus) values(@ProjectFK,@RequestorComment,@CreatedBy,getdate(),1)";
            com.Parameters.AddWithValue("@ProjectFK", ProjectID);
            com.Parameters.AddWithValue("@RequestorComment", Comment.Trim());
            com.Parameters.AddWithValue("@CreatedBy", CurrentUser);

            ExecuteSecureCommandNonQueryWrapper(com);


            //update project status 
        }

        //SB2 1.12.2015 Remove Single Quote From Application Logging Messages.
        public static void CreateApplicationLoggingEntry(string Message)
        {
            Message = Message.Replace("'", string.Empty);
            string ExcStr = string.Format("insert into dbo.ApplicationLogging (Message,DateModified) values ('{0}',getdate())", Message);
            Scheduling.Database.Utility.ExecuteNonQueryWrapper(ExcStr);
            TruncateApplicationLog();
        }

        public static void CreateApplicationErrorLoggingEntry(string Message)
        {
            Message = Message.Replace("'", string.Empty);
            string ExcStr = string.Format("insert into dbo.ApplicationErrorLogging (Message,DateModified) values ('{0}',getdate())", Message);
            Scheduling.Database.Utility.ExecuteNonQueryWrapper(ExcStr);
            TruncateApplicationErrorLog();
        }

        //Activity Logging Stop

        //ProjectHistory todo consider refactor to one function

        public static void CreateProjectHistoryEntry(int ProjectStatus, string Comments, int ProjectID, int CurrentVersion)
        {
            string CurrentUsername = Scheduling.Security.Utility.GetCurrentLoggedInUser();

            string ExcStr = string.Format("insert into dbo.ProjectHistory (CurrentProjectStatus,Comments,ProjectFK,Username,CurrentProjectVersion,EntryDate) values({0},'{1}',{2},'{3}',{4},getdate())", ProjectStatus, Comments, ProjectID, CurrentUsername, CurrentVersion);
            Scheduling.Database.Utility.ExecuteNonQueryWrapper(ExcStr);

        }

        public static void CreateProjectHistoryEntry(int ProjectID, string Comments)
        {
            ProjectDisplay pd = Scheduling.Database.Utility.GetAllProjects().Where(x => x.ID == ProjectID).ToList().First();

            int ProjectStatus = pd.CurrentProjectStatus;
            int CurrentVersion = pd.CurrentVersion;
            CreateProjectHistoryEntry(ProjectStatus, Comments, ProjectID, CurrentVersion);
        }

        public static void CreateProjectHistoryEntryForStatusUpdate(int NewStatusID, int ProjectID)
        {

            ProjectDisplay pd = Scheduling.Database.Utility.GetCurrentProjectByProjectID(ProjectID);
            string CurrentUser = Scheduling.Security.Utility.GetCurrentLoggedInUser();
            string NewStatus = GetAllProjectStatuses().Where(x => x.ID == NewStatusID).First().Description;
            string Comments = string.Format("Project Status Updated To {0} by {1}", NewStatus, CurrentUser);
            Scheduling.Database.Utility.CreateProjectHistoryEntry(pd.CurrentProjectStatus, Comments, pd.ID, pd.CurrentVersion);

        }
        //only one secondary to primary link allowed hence deletion 
        public static void CreateProjectLinkEntry(int PrimaryID, int SecondaryID, int LinkSettingID)
        {

            string ComText = string.Format("delete from dbo.ProjectLinks where SecondaryProjectID={0};insert into dbo.ProjectLinks (PrimaryProjectID,SecondaryProjectID,LinkSettingID) values ({1},{2},{3});", SecondaryID, PrimaryID, SecondaryID, LinkSettingID);
            ExecuteNonQueryWrapper(ComText);
            string PrimaryName = GetAllProjects().Where(x => x.ID == PrimaryID).First().Name;
            ProjectDisplay cpd = GetAllProjects().Where(x => x.ID == SecondaryID).First();
            string Comments = string.Format("Linking with Primary Project {0}", PrimaryName);
            CreateProjectHistoryEntry(SecondaryID, Comments);

        }

        public static void CreateProjectNoteEntry(ProjectNote pn)
        {
            string CurrentUsername = Scheduling.Security.Utility.GetCurrentLoggedInUser();

            SqlCommand com = new SqlCommand();
            com.CommandText = @"insert into dbo.[ProjectNote](ProjectNoteLabelFK,NoteValue,ProjectFK,EntryDate,Username) values(@NoteLabel,@NoteValue,@ProjectFK,getdate(),@Username)";
            com.Parameters.AddWithValue("@NoteLabel", pn.NoteLabelID);
            com.Parameters.AddWithValue("@NoteValue", pn.NoteValue.Trim());
            com.Parameters.AddWithValue("@ProjectFK", Convert.ToInt32(pn.ProjectFK));
            com.Parameters.AddWithValue("@Username", CurrentUsername);

            ExecuteSecureCommandNonQueryWrapper(com);

            string Desc = Scheduling.Database.Utility.GetAllProjects().Where(x => x.ID == pn.ProjectFK).First().Name;

            string LogStr = string.Format("Adding Project Note To {0}", Desc);
            CreateActivityLogEntry(LogStr);

        }

        [HttpPost]
        public static void CreatePublicationSubItemReportingEntry(int PubID, List<int> InputList)
        {
            //whack then recreate ..For now don't deal with edge cases like discover that can be timeline specific.Going forward these should not be needed.
            StringBuilder sb = new StringBuilder();
            string ComText = string.Format("delete from dbo.MilestoneFieldMainSubItemsReportSorting where PubCodeFK={0}", PubID);
            ExecuteNonQueryWrapper(ComText);

            foreach (int i in InputList)
            {
                int SortOrder = InputList.IndexOf(i) + 1;
                sb.AppendFormat("insert into [dbo].[MilestoneFieldMainSubItemsReportSorting] (PubCodeFK,ProjectRangeFk,MilestoneFieldMainSubItemFK,SortOrder) values({0},null,{1},{2});", PubID, i, SortOrder);
            }

            string InputStr = sb.ToString();
            string CurrentPubName = GetPubCodeShortDescFromID(PubID);
            string Comments = string.Format("Updating Reporting Sub Item SortOrder For {0}", CurrentPubName);
            ExecuteNonQueryWrapper(InputStr);
            CreateActivityLogEntry(Comments);

        }

        public static void CreatePublicationEntry(PublicationCode pc)
        {

            SqlCommand com = new SqlCommand();
            com.CommandText = @"insert into dbo.[PublicationCode](ShortDesc,LongDesc,IsActive,PrinterFK,ProfitCenter,ReportDesc,ParentPub,IsAnnual,ShowInNewsstandReport,HasCustomOffset) values(@ShortDesc,@LongDesc,@IsActive,@PrinterFK,@ProfitCenter,@ReportDesc,@ParentPub,@IsAnnual,@ShowInNewsstandReport,@HasCustomOffset)";

            com.Parameters.AddWithValue("@ShortDesc", pc.ShortDesc);
            com.Parameters.AddWithValue("@LongDesc", pc.LongDesc);
            com.Parameters.AddWithValue("@IsActive", pc.IsActive);

            //printer is nullable

            if (pc.PrinterFK.HasValue)
            {
                com.Parameters.AddWithValue("@PrinterFK", pc.PrinterFK);

            }

            else
            {
                com.Parameters.AddWithValue("@PrinterFK", DBNull.Value);

            }


            com.Parameters.AddWithValue("@ProfitCenter", pc.ProfitCenter);
            com.Parameters.AddWithValue("@ReportDesc", pc.ReportDesc);

            //parent pub is nullable

            if (pc.ParentPub.HasValue)
            {
                com.Parameters.AddWithValue("@ParentPub", pc.ParentPub);
            }

            else
            {
                com.Parameters.AddWithValue("@ParentPub", DBNull.Value);
            }


            com.Parameters.AddWithValue("@IsAnnual", pc.IsAnnual);
            com.Parameters.AddWithValue("@ShowInNewsstandReport", pc.ShowInNewsStandReport);
            com.Parameters.AddWithValue("@HasCustomOffset", pc.HasCustomOffset);

            ExecuteSecureCommandNonQueryWrapper(com);

            string LogStr = string.Format("Adding Publication {0}", pc.ShortDesc);
            CreateActivityLogEntry(LogStr);

        }

        //delete all activity invoked only by super admin
        public static void DeleteAllActivity()
        {
            string ComText = "delete from dbo.Activity where 1=1";
            ExecuteNonQueryWrapper(ComText);
            CreateActivityLogEntry("Deleting All Activity");

        }

        public static void DeleteProjectHistoryEntriesAfterInitialCreation(int ProjectID)
        {
            int CreationStatusID = (int)Scheduling.Enums.ProjectStatus.Created;

            string ComText = string.Format("delete from dbo.ProjectHistory where projectfk ={0} and CurrentProjectStatus <> {1}", ProjectID, CreationStatusID);
            ExecuteNonQueryWrapper(ComText);

        }

        public static void DeleteSingleGroupEntry(int GroupID)
        {

            string ComText = string.Format("delete from dbo.[group] where id = {0}", GroupID);
            ExecuteNonQueryWrapper(ComText);

        }

        public static void DeleteSingleMilestoneValueEntry(int FieldID, int ProjectID)
        {
            string ComText = string.Format("delete from dbo.MilestoneValue where MilestoneFieldFK={0} and ProjectPK={1}", FieldID, ProjectID);
            ExecuteNonQueryWrapper(ComText);

        }

        //Called from manage projects
        public static void DeleteSingleProjectLinkEntryFromManagedKeepingValues(int CurrentSecondaryProject)
        {
            ProjectLink AssociatedLink = Scheduling.Database.Utility.GetAllProjectLinks().Where(x => x.SecondaryProjectID == CurrentSecondaryProject).First();
            DeleteSingleProjectLinkAndKeepValues(AssociatedLink.ID);
        }

        public static void DeleteSingleProjectLinkEntry(int id, int PrimaryProjectID, int SecondaryProjectID)
        {
            string ComText = string.Format("delete from dbo.projectlinks where id ={0}", id);
            ExecuteNonQueryWrapper(ComText);
            string PrimaryProjectName = GetAllProjects().Where(x => x.ID == PrimaryProjectID).First().Name;
            string Comments = string.Format("Removing Project Link to Primary Project {0}", PrimaryProjectName);
            CreateProjectHistoryEntry(SecondaryProjectID, Comments);

        }

        //Verify code fails on code analysis.Validate code when project linking module is written
        public static void DeleteSingleProjectLinkAndKeepValues(int id)
        {
            ProjectLink ProjLink = Scheduling.Database.Utility.GetAllProjectLinks().Where(x => x.ID == id).First();
            string ComText = string.Format("delete from dbo.projectlinks where id={0}", id);
            Scheduling.Database.Utility.ExecuteNonQueryWrapper(ComText);
            Scheduling.Database.Utility.CreateProjectHistoryEntry(ProjLink.SecondaryProjectID, "Removing Entry From Project Link Table");
        }

        public static void DeleteSingleProjectLinkAndResetValues(int id)
        {
            ProjectLink ProjLink = Scheduling.Database.Utility.GetAllProjectLinks().Where(x => x.ID == id).First();
            string ComText = string.Format("update dbo.milestonevalue set duedate = null where projectpk={0};delete from dbo.projectlinks where id={1}", ProjLink.SecondaryProjectID, id);
            Scheduling.Database.Utility.ExecuteNonQueryWrapper(ComText);
            Scheduling.Database.Utility.CreateProjectHistoryEntry(ProjLink.SecondaryProjectID, string.Format("Resetting all due dates and removing entry from project link table"));

        }


        //6/2 Now link settings can only be deleted when NOT assigned to projects

        public static void DeleteSingleLinkSetting(int id)
        {
            //Get Projects where this link is used
            List<int> ProjectIntList = GetAllProjectLinks().Where(x => x.LinkSettingID == id).Select(x => x.SecondaryProjectID).ToList();
            string ComTextA = string.Empty;
            //for each project set all values in milestone value due date to null and remove entry from the link table.
            foreach (int pid in ProjectIntList)
            {
                ComTextA += string.Format("update dbo.milestonevalue set DueDate=null where ProjectPk={0};", pid);

            }

            string ComTextB = string.Format("delete from dbo.projectlinks where linksettingid={0}", id);
            string ComTextC = string.Format("delete from dbo.projectlinksettings where id={0}", id);
            string ComText = string.Format("{0}{1};{2};", ComTextA, ComTextB, ComTextC);

            ExecuteNonQueryWrapper(ComText);

        }
        public static void DeleteSingleNoteEntry(int id, int ProjectID)
        {

            ProjectDisplay pd = GetAllProjects().Where(x => x.ID == ProjectID).First();
            string ProjectName = pd.Name;

            string ProjectNoteLabel = GetProjectNotesFromProjectID(ProjectID).Where(x => x.ID == id).First().NoteLabelID.ToString();

            string ComText = string.Format("delete from dbo.ProjectNote where id={0}", id);
            ExecuteNonQueryWrapper(ComText);

            string Comments = string.Format("Deleting Note With Label {0}", ProjectNoteLabel);
            Scheduling.Database.Utility.CreateProjectHistoryEntry(pd.CurrentProjectStatus, Comments, pd.ID, pd.CurrentVersion);

        }

        public static void DeleteSingleRequestEntry(int id, int ProjectID)
        {

            ProjectDisplay pd = GetAllProjects().Where(x => x.ID == ProjectID).First();
            string ProjectName = pd.Name;

            ChangeRequest cr = GetAllChangeRequests().Where(x => x.ID == id).First();
            string ReqComment = cr.RequestorComment;
            string Requestor = cr.CreatedBy;

            string ComText = string.Format("delete from dbo.ProjectChangeRequest where id={0}", id);
            ExecuteNonQueryWrapper(ComText);

            string Comments = string.Format("Deleting Request With Comment  {0} Created By {1} ", ReqComment, Requestor);
            Scheduling.Database.Utility.CreateProjectHistoryEntry(pd.CurrentProjectStatus, Comments, pd.ID, pd.CurrentVersion);

        }

        public static void EditPublicationEntry(PublicationCode pc)
        {
            //nullable printer
            string CurrentPrinter = "null";
            if (pc.PrinterFK.HasValue) CurrentPrinter = pc.PrinterFK.Value.ToString();

            //nullable parentpub
            string CurrentParentPub = "null";
            if (pc.ParentPub.HasValue) CurrentParentPub = pc.ParentPub.Value.ToString();

            string ComText = string.Format("update dbo.publicationcode set ShortDesc='{0}',LongDesc='{1}',IsActive={2},PrinterFK={3},ProfitCenter={4},ReportDesc='{5}',ParentPub={6},IsAnnual={7},ShowInNewsStandReport={8},HasCustomOffset={9} where id={10}", pc.ShortDesc, pc.LongDesc, pc.IsActive, CurrentPrinter, pc.ProfitCenter, pc.ReportDesc, CurrentParentPub, pc.IsAnnual, pc.ShowInNewsStandReport, pc.HasCustomOffset, pc.ID);
            ExecuteNonQueryWrapper(ComText);
            string Message = string.Format("Updating Publication information for pubcode {0}", pc.ShortDesc);
            Scheduling.Database.Utility.CreateActivityLogEntry(Message);

        }

        public static void EditSingleChangeRequestEntry(int id, int ProjectID, int Status)
        {
            string ComText = string.Format("update dbo.ProjectChangeRequest set RequestStatus = {0} where ID ={1}", Status, id);
            ExecuteNonQueryWrapper(ComText);
            ChangeRequest cr = GetAllChangeRequests().Where(x => x.ID == id).First();
            string CrComment = cr.RequestorComment;
            string CrRequestor = cr.CreatedBy;
            string UpdatedStatus = GetAllChangeRequestStatuses().Where(x => x.ID == Status).First().Description;
            string Comments = string.Format("Change Request ..{0} updated to status of {1}", CrComment, UpdatedStatus);
            CreateProjectHistoryEntry(ProjectID, Comments);

        }

        public static void UpdateReviewedProjects(string input)
        {
            List<string> ProjList = Scheduling.StringFunctions.Utility.GetStringListFromStringWithPossibleCommaSeperator(input);
            int ProjectStatus = Convert.ToInt32(Scheduling.StringFunctions.Utility.GetAppSettingValue("ProjectStatusApprovedOnSaleID"));

            foreach (string s in ProjList)
            {
                int ProjectID = Convert.ToInt32(s);
                string ComText = string.Format("update dbo.Project set CurrentProjectStatus ={0} where ID={1}", ProjectStatus, s);
                ExecuteNonQueryWrapper(ComText);

                CreateProjectHistoryEntryForStatusUpdate(ProjectStatus, ProjectID);

            }


        }

        public static void UpdateProjectsToScheduleApproved(string input)
        {

            List<string> ProjList = Scheduling.StringFunctions.Utility.GetStringListFromStringWithPossibleCommaSeperator(input);
            int ProjectStatus = Convert.ToInt32(Scheduling.StringFunctions.Utility.GetAppSettingValue("ProjectStatusScheduleCreatedID"));

            foreach (string s in ProjList)
            {
                int ProjectID = Convert.ToInt32(s);
                string ComText = string.Format("update dbo.Project set CurrentProjectStatus ={0} where ID={1}", ProjectStatus, s);
                ExecuteNonQueryWrapper(ComText);

                CreateProjectHistoryEntryForStatusUpdate(ProjectStatus, ProjectID);

            }

        }

        public static void UpdateSelectedMilestoneValueDueDate(EditSingleMilestoneWithDueDate esm)
        {
            string OldDueDate = Scheduling.Database.Utility.GetMilestoneValuesByProjectID(Convert.ToInt32(esm.ProjectID)).Where(x => x.MilestoneFieldFK == Convert.ToInt32(esm.MilestoneFieldFK)).First().DueDate;
            string ComText = string.Format("update dbo.MilestoneValue set DueDate='{0}',lastmodified=getdate() where ProjectPK={1} and MilestoneFieldFK={2};", esm.DueDate, esm.ProjectID, esm.MilestoneFieldFK);
            ExecuteNonQueryWrapper(ComText);

            //get project name
            ProjectDisplay pd = Scheduling.Database.Utility.GetAllProjects().Where(x => x.ID == Convert.ToInt32(esm.ProjectID)).First();
            string MileField = Scheduling.Database.Utility.GetAllMilestoneFields().Where(x => x.ID == Convert.ToInt32(esm.MilestoneFieldFK)).First().Description;

            string Message = string.Format("Update Selected  Milestone Field {0}. DueDate was {1}.Changed to {2} ", MileField, OldDueDate, esm.DueDate);
            Scheduling.Database.Utility.CreateProjectHistoryEntry(pd.CurrentProjectStatus, Message, Convert.ToInt32(esm.ProjectID), pd.CurrentVersion);

        }


        public static void UpdateProjectStatus(int ProjectID, int ProjectStatus)
        {
            string ComText = string.Format("update dbo.Project set CurrentProjectStatus={0} where ID={1}", ProjectStatus, ProjectID);
            ExecuteNonQueryWrapper(ComText);
            string UpdatedStatus = GetAllProjectStatuses().Where(x => x.ID == ProjectStatus).First().Description;
            string Comments = string.Format("Update Project Status to {0}", UpdatedStatus);
            CreateProjectHistoryEntry(ProjectID, Comments);

        }


        public static void EditSingleProjectMilestoneValueEntry(EditSingleProjectMilestoneValueWithDueDate esp)
        {

            string ProjectName = Scheduling.Database.Utility.GetAllProjects().Where(x => x.ID == Convert.ToInt32(esp.ProjectID)).First().Name;


            MilestoneValue mv = Scheduling.Database.Utility.GetMilestoneValuesByProjectID(Convert.ToInt32(esp.ProjectID))
                .Where(x => x.ID == Convert.ToInt32(esp.MilestoneValueID))
                .First();

            string MilestoneFieldName =
            Scheduling.Database.Utility.GetAllMilestoneFields().Where(x => x.ID == mv.ID).First().Description;

            string ComText = string.Format("update dbo.MilestoneValue set DueDate='{0}',lastmodified=getdate() where ProjectPK={1} and ID={2};", esp.DueDate, esp.ProjectID, esp.MilestoneValueID);
            ExecuteNonQueryWrapper(ComText);

            string Message = string.Format("Single Project Editing for Milestone Field {0}. DueDate Changed From {1} to {2} ", MilestoneFieldName, mv.DueDate, esp.DueDate);
            Scheduling.Database.Utility.CreateProjectHistoryEntry(Convert.ToInt32(esp.ProjectID), Message);
            Scheduling.Database.Utility.CreateApplicationLoggingEntry(Message);

        }

        public static void EditSingleMilestoneValueEntry(EditSingleMilestoneWithDueDate esm)
        {
            string AppMessage = string.Format("Attempting to update project {0} with mile field pk of {1}", esm.ProjectID,
              esm.MilestoneFieldFK);
            Scheduling.Database.Utility.CreateApplicationLoggingEntry(AppMessage);
            string OldDueDate = Scheduling.Database.Utility.GetMilestoneValuesByProjectID(Convert.ToInt32(esm.ProjectID)).Where(x => x.MilestoneFieldFK == Convert.ToInt32(esm.MilestoneFieldFK)).First().DueDate;
            string ComText = string.Format("update dbo.MilestoneValue set DueDate='{0}',lastmodified=getdate() where ProjectPK={1} and MilestoneFieldFK={2};", esm.DueDate, esm.ProjectID, esm.MilestoneFieldFK);
            ExecuteNonQueryWrapper(ComText);

            //get project name
            ProjectDisplay pd = Scheduling.Database.Utility.GetAllProjects().Where(x => x.ID == Convert.ToInt32(esm.ProjectID)).First();
            string MileField = Scheduling.Database.Utility.GetAllMilestoneFields().Where(x => x.ID == Convert.ToInt32(esm.MilestoneFieldFK)).First().Description;

            string Message = string.Format("Manual Editing of Milestone Field {0}. DueDate was {1}.Changed to {2}.Dependencies on this field removed ", MileField, OldDueDate, esm.DueDate);
            Scheduling.Database.Utility.CreateProjectHistoryEntry(pd.CurrentProjectStatus, Message, Convert.ToInt32(esm.ProjectID), pd.CurrentVersion);

        }
        //new note functionality
        public static void EditSingleNoteField(EditSingleNoteField esn)
        {
            string CurrentProjectID = esn.ProjectID;
            string CurrentNoteLabelID = esn.NoteLabelID;
            string CurrentNoteValue = string.Empty;
            if (!string.IsNullOrWhiteSpace(esn.NoteValue))
            {
                CurrentNoteValue = esn.NoteValue;
            }

            string ComText = string.Format("delete from dbo.ProjectNote where ProjectFk={0} and ProjectNoteLabelFk={1};", Convert.ToInt32(CurrentProjectID), Convert.ToInt32(CurrentNoteLabelID));
            ExecuteNonQueryWrapper(ComText);

            SqlCommand com = new SqlCommand();
            com.CommandText = @"insert into dbo.[ProjectNote] (ProjectNoteLabelFK,NoteValue,ProjectFk,EntryDate,Username) values(@ProjectNoteLabelFK,@NoteValue,@ProjectFk,getdate(),@Username)";

            com.Parameters.AddWithValue("@ProjectNoteLabelFk", Convert.ToInt32(CurrentNoteLabelID));
            com.Parameters.AddWithValue("@NoteValue", CurrentNoteValue);
            com.Parameters.AddWithValue("@ProjectFK", Convert.ToInt32(CurrentProjectID));
            com.Parameters.AddWithValue("@Username", Scheduling.Security.Utility.GetCurrentLoggedInUser());

            ExecuteSecureCommandNonQueryWrapper(com);

            string NoteKeyField = Scheduling.Database.Utility.GetAllProjectNoteLabels().Where(x => x.ID == Convert.ToInt32(CurrentNoteLabelID)).First().ShortDesc;
            string NoteComment = string.Format("Note field update Key is {0} Value is {1}", NoteKeyField, CurrentNoteValue);
            CreateProjectHistoryEntry(Convert.ToInt32(CurrentProjectID), NoteComment);

        }

        public static void EditSingleMilestoneValueEntryAndRemoveDependencies(EditSingleMilestoneWithDueDate esm)
        {
            string OldDueDate = Scheduling.Database.Utility.GetMilestoneValuesByProjectID(Convert.ToInt32(esm.ProjectID)).Where(x => x.MilestoneFieldFK == Convert.ToInt32(esm.MilestoneFieldFK)).First().DueDate;
            string ComText = string.Format("update dbo.MilestoneValue set DueDate='{0}' where ProjectPK={1} and MilestoneFieldFK={2};", esm.DueDate, esm.ProjectID, esm.MilestoneFieldFK);
            ComText += string.Format("update dbo.MilestoneValue set DependantUpon = null,calculationFK=null,lastmodified=getdate() where ProjectPK={0} and DependantUpon = {1};", esm.ProjectID, esm.MilestoneFieldFK);
            ExecuteNonQueryWrapper(ComText);

            //get project name
            ProjectDisplay pd = Scheduling.Database.Utility.GetAllProjects().Where(x => x.ID == Convert.ToInt32(esm.ProjectID)).First();
            string MileField = Scheduling.Database.Utility.GetAllMilestoneFields().Where(x => x.ID == Convert.ToInt32(esm.MilestoneFieldFK)).First().Description;

            string Message = string.Format("Manual Editing of Milestone Field {0}. DueDate was {1}.Changed to {2}.Dependencies on this field removed ", MileField, OldDueDate, esm.DueDate);
            Scheduling.Database.Utility.CreateProjectHistoryEntry(pd.CurrentProjectStatus, Message, Convert.ToInt32(esm.ProjectID), pd.CurrentVersion);
        }


        //delete record if not being used by a project todo return message
        public static void DeleteSingleMilestoneTreeProfileSetting(int i)
        {

            string MileName = GetAllMilestoneTreeSettingsProfiles().Where(x => x.ID == i).First().Description;
            string CountStr = string.Format("select count(*) from dbo.Project where MilestoneTreeSettingsProfileFK={0}", i);
            int c = Convert.ToInt32(ExecuteScalarWrapper(CountStr));
            if (c == 0)
            {

                string DelStr = string.Format("delete from dbo.MilestoneTreeSettingsProfile where id={0}", i);
                ExecuteNonQueryWrapper(DelStr);

                string CascadeDelStr = string.Format("delete from dbo.MilestoneTreeSettings where MilestoneTreeSettingsProfileFK={0}", i);
                ExecuteNonQueryWrapper(CascadeDelStr);

                string LogStr = string.Format("deleting Milestone Tree Setting Profile named {0} and relevant entries in Milestone Tree Settings Table ", MileName);
                CreateActivityLogEntry(LogStr);
            }

            else
            {
                string FailDelStr = string.Format("Cannot delete {0}..Assigned to one or more projects", MileName);
                CreateActivityLogEntry(FailDelStr);

            }



        }

        //delete single project
        public static void DeleteProjectAndAllProjectInformationByID(int ProjectID)
        {
            string CurrentProject = Scheduling.Database.Utility.GetAllProjects().Where(x => x.ID == ProjectID).First().Name;
            string ExcStr = string.Format("delete from dbo.ProjectHistory where ProjectFk={0};delete from dbo.MilestoneValue where ProjectPk={1};delete from dbo.Project where id={2};delete from dbo.ProjectLinks where PrimaryProjectID={3} or SecondaryProjectID={6};delete from dbo.ProjectNote where ProjectFK={4};delete from dbo.Activity where ProjectFk={5};", ProjectID, ProjectID, ProjectID, ProjectID, ProjectID, ProjectID, ProjectID);
            ExecuteNonQueryWrapper(ExcStr);

            //also whack change requests
            string ReqStr = string.Format("delete from dbo.ProjectChangeRequest where projectfk={0}", ProjectID);
            ExecuteNonQueryWrapper(ReqStr);

            string ActStr = string.Format("Deleting Project {0}", CurrentProject);
            CreateActivityLogEntry(ActStr);

        }

        //Get values with a calculation and a dependency revisit not currently revisit todo
        public static List<MilestoneValue> GetMilestoneValueCalculationsByProjectID(int ProjectPK)
        {
            List<MilestoneValue> MilValList = new List<MilestoneValue>();
            string ComText = string.Format("select * from dbo.MilestoneValue where ProjectPK={0} and dependantupon is not null and calculationFK is not null", ProjectPK);

            SqlCommand com = new SqlCommand(ComText, GetConnection());
            SqlDataReader sdr = null;

            try
            {
                com.Connection.Open();
                sdr = com.ExecuteReader();

                while (sdr.Read())
                {

                    if (sdr.HasRows)
                    {
                        MilestoneValue mv = new MilestoneValue();
                        mv.ID = Convert.ToInt32(sdr["ID"]);
                        mv.MilestoneFieldFK = Convert.ToInt32(sdr["MilestoneFieldFK"]);
                        mv.CalculationFK = GetNullableIntValueFromDbField(sdr["CalculationFK"]);
                        mv.RangeCalculationFK = GetNullableIntValueFromDbField(sdr["RangeCalculationFK"]);
                        mv.DependantUpon = GetNullableIntValueFromDbField(sdr["DependantUpon"]);
                        mv.ProjectPK = Convert.ToInt32(sdr["ProjectPK"]);
                        mv.DueDate = Scheduling.StringFunctions.Utility.GenerateDatePickerFieldFromDatabaseDateField(sdr["DueDate"]);
                        mv.CalcFiringOrder = GetNullableIntValueFromDbField(sdr["CalcFiringOrder"]);
                        MilValList.Add(mv);
                    }


                }


            }



            finally
            {
                if (!sdr.IsClosed) sdr.Close();
                if (com.Connection.State == ConnectionState.Open) com.Connection.Close();
                com.Dispose();

            }

            return MilValList;

        }

        //Footer Note is on the baseline for the project
        public static string GetReportFooterNoteByProjectID(int i)
        {
            string RetStr = string.Empty;

            int? CurrentProfileID = GetAllProjects().Where(x => x.ID == i).First().MilestoneTreeSettingsProfileFK;
            if (CurrentProfileID.HasValue)
            {
                MilestoneTreeSettingsProfile mtsp = Scheduling.Database.Utility.GetAllMilestoneTreeSettingsProfiles().Where(x => x.ID == CurrentProfileID.Value).First();
                RetStr = mtsp.ReportFooterNote;

            }

            return RetStr;
        }

        public static List<MilestoneTreeSetting> GetMilestoneTreeSettingsByProfileID(int ProfileID)
        {

            List<MilestoneTreeSetting> MtsList = new List<MilestoneTreeSetting>();
            string ComText = string.Format("select * from MilestoneTreeSettings where MilestoneTreeSettingsProfileFK={0}", ProfileID);
            SqlCommand com = new SqlCommand(ComText, GetConnection());
            SqlDataReader sdr = null;

            try
            {
                com.Connection.Open();
                sdr = com.ExecuteReader();

                while (sdr.Read())
                {

                    if (sdr.HasRows)
                    {
                        MilestoneTreeSetting mts = new MilestoneTreeSetting();
                        mts.ID = Convert.ToInt32(sdr["ID"]);
                        mts.MilestoneField = Convert.ToInt32(sdr["MilestoneFieldFK"]);
                        mts.MilestoneParentField = GetNullableIntValueFromDbField(sdr["MilestoneFieldParentFK"]);
                        mts.MilestoneFieldDependantUpon = GetNullableIntValueFromDbField(sdr["DependantUpon"]);
                        mts.CalculationID = GetNullableIntValueFromDbField(sdr["CalculationID"]);
                        mts.CalcFiringOrder = GetNullableIntValueFromDbField(sdr["CalcFiringOrder"]);
                        mts.RangeCalculationID = GetNullableIntValueFromDbField(sdr["RangeCalculationID"]);
                        mts.MilestoneTreeSettingsProfileID = Convert.ToInt32(sdr["MilestoneTreeSettingsProfileFK"]);
                        mts.DisplayOrder = GetNullableIntValueFromDbField(sdr["DisplayOrder"]);
                        mts.MilestoneParentField = GetNullableIntValueFromDbField(sdr["MilestoneFieldParentFK"]);

                        MtsList.Add(mts);
                    }


                }


            }


            finally
            {
                if (!sdr.IsClosed) sdr.Close();
                if (com.Connection.State == ConnectionState.Open) com.Connection.Close();
                com.Dispose();

            }

            return MtsList;

        }

        public static bool DoesSingleProjectMilestoneHaveDependencies(EditSingleProjectMilestoneValueWithDueDate esp)
        {

            int CurrentProject = Convert.ToInt32(esp.ProjectID);
            int CurrentMilestoneValueID = Convert.ToInt32(esp.MilestoneValueID);

            int DepCount = GetMilestoneValuesByProjectID(CurrentProject).Where(x => x.ID == CurrentMilestoneValueID).Count();
            return DepCount == 1;
        }

        public static bool DoesMilestoneHaveDependencies(EditSingleMilestone esm)
        {

            int CurrentProject = Convert.ToInt32(esm.ProjectID);
            int CurrentMilestone = Convert.ToInt32(esm.MilestoneFieldID);

            int DepCount = GetMilestoneValuesByProjectID(CurrentProject).Where(x => x.DependantUpon == CurrentMilestone).Count();
            return DepCount > 0;
        }


        public static List<MilestoneValue> GetMilestoneValuesByProjectIDOrderedByDueDateDesc(int ProjectPK)
        {
            List<MilestoneValue> MilValList = new List<MilestoneValue>();
            string ComText = string.Format("select * from dbo.MilestoneValue where ProjectPk={0} order by duedate desc", ProjectPK);
            SqlCommand com = new SqlCommand(ComText, GetConnection());
            SqlDataReader sdr = null;


            try
            {
                com.Connection.Open();
                sdr = com.ExecuteReader();

                while (sdr.Read())
                {

                    if (sdr.HasRows)
                    {
                        MilestoneValue mv = new MilestoneValue();
                        mv.ID = Convert.ToInt32(sdr["ID"]);
                        mv.MilestoneFieldFK = Convert.ToInt32(sdr["MilestoneFieldFK"]);
                        mv.CalculationFK = GetNullableIntValueFromDbField(sdr["CalculationFK"]);
                        mv.RangeCalculationFK = GetNullableIntValueFromDbField(sdr["RangeCalculationFK"]);
                        mv.DependantUpon = GetNullableIntValueFromDbField(sdr["DependantUpon"]);
                        mv.ProjectPK = Convert.ToInt32(sdr["ProjectPK"]);
                        mv.DueDate = Scheduling.StringFunctions.Utility.GenerateDatePickerFieldFromDatabaseDateField(sdr["DueDate"]);
                        mv.CalcFiringOrder = GetNullableIntValueFromDbField(sdr["CalcFiringOrder"]);
                        mv.DisplaySortOrder = GetNullableIntValueFromDbField(sdr["DisplaySortOrder"]);
                        mv.ParentID = GetNullableIntValueFromDbField(sdr["ParentID"]);
                        MilValList.Add(mv);
                    }


                }


            }


            finally
            {
                if (!sdr.IsClosed) sdr.Close();
                if (com.Connection.State == ConnectionState.Open) com.Connection.Close();
                com.Dispose();

            }

            return MilValList;

        }

        public static List<MilestoneValue> GetMilestoneValuesByProjectID(int ProjectPK)
        {
            List<MilestoneValue> MilValList = new List<MilestoneValue>();
            string ComText = string.Format("select * from dbo.MilestoneValue where ProjectPk={0}", ProjectPK);
            SqlCommand com = new SqlCommand(ComText, GetConnection());
            SqlDataReader sdr = null;


            try
            {
                com.Connection.Open();
                sdr = com.ExecuteReader();

                while (sdr.Read())
                {

                    if (sdr.HasRows)
                    {
                        MilestoneValue mv = new MilestoneValue();
                        mv.ID = Convert.ToInt32(sdr["ID"]);
                        mv.MilestoneFieldFK = Convert.ToInt32(sdr["MilestoneFieldFK"]);
                        mv.CalculationFK = GetNullableIntValueFromDbField(sdr["CalculationFK"]);
                        mv.RangeCalculationFK = GetNullableIntValueFromDbField(sdr["RangeCalculationFK"]);
                        mv.DependantUpon = GetNullableIntValueFromDbField(sdr["DependantUpon"]);
                        mv.ProjectPK = Convert.ToInt32(sdr["ProjectPK"]);
                        mv.DueDate = Scheduling.StringFunctions.Utility.GenerateDatePickerFieldFromDatabaseDateField(sdr["DueDate"]);
                        mv.CalcFiringOrder = GetNullableIntValueFromDbField(sdr["CalcFiringOrder"]);
                        mv.DisplaySortOrder = GetNullableIntValueFromDbField(sdr["DisplaySortOrder"]);
                        mv.ParentID = GetNullableIntValueFromDbField(sdr["ParentID"]);
                        MilValList.Add(mv);
                    }


                }


            }


            finally
            {
                if (!sdr.IsClosed) sdr.Close();
                if (com.Connection.State == ConnectionState.Open) com.Connection.Close();
                com.Dispose();

            }

            return MilValList;

        }

        public static DateTime? GetNullableDateTimeForListedProject(int ProjectPK)
        {

            int NewsConstant = Convert.ToInt32(ConfigurationManager.AppSettings["NewsstandOnSaleDateMilestoneValue"]);
            string ComText = string.Format("select DueDate from dbo.MilestoneValue where ProjectPk={0} and MilestoneFieldFK={1}", ProjectPK, NewsConstant);
            object o = ExecuteScalarWrapper(ComText);
            if (o is DBNull)
            {
                return null;

            }

            else
            {
                return Convert.ToDateTime(o);
            }

        }
        //Get NewstandDate for listed Project
        public static string GetNewstandDateForListedProject(int ProjectPK)
        {
            int NewsConstant = Convert.ToInt32(ConfigurationManager.AppSettings["NewsstandOnSaleDateMilestoneValue"]);
            string ComText = string.Format("select DueDate from dbo.MilestoneValue where ProjectPk={0} and MilestoneFieldFK={1}", ProjectPK, NewsConstant);
            object o = ExecuteScalarWrapper(ComText);
            if (o is DBNull)
            {
                return "N/A";

            }

            else
            {
                return Convert.ToDateTime(o).ToLongDateString();
            }
        }



        public static void CreateMilestoneValueEntryForExistingProject(int ProjectID, int MilestoneField, string OptionalParent, string OptionalDependency, string OptionalCalculation, string DisplayOrder)
        {

            string NullStr = "null";

            string CurrentParent = NullStr;
            string CurrentDependency = NullStr;
            string CurrentCalculation = NullStr;

            if (!string.IsNullOrWhiteSpace(OptionalParent)) CurrentParent = OptionalParent.ToString();
            if (!string.IsNullOrWhiteSpace(OptionalDependency)) CurrentDependency = OptionalDependency.ToString();
            if (!string.IsNullOrWhiteSpace(OptionalCalculation)) CurrentCalculation = OptionalCalculation.ToString();

            string CountStr = string.Format("select count(*) from dbo.milestonevalue where projectpk={0} and CalculationFK is not null", ProjectID);
            int CountRet = Convert.ToInt32(ExecuteScalarWrapper(CountStr));

            int CalcFiringOrder = 0;
            if (CountRet > 0)
            {
                string MaxCalcOrderStr = string.Format("select max(CalcFiringOrder) from dbo.milestonevalue where projectpk={0}", ProjectID);
                CalcFiringOrder = Convert.ToInt32(ExecuteScalarWrapper(MaxCalcOrderStr));

            }

            string CurrentUser = Scheduling.Security.Utility.GetCurrentLoggedInUser();

            CalcFiringOrder = CalcFiringOrder + 5;
            string InsStr = "insert into dbo.MilestoneValue (ParentID,MilestoneFieldFk,CalculationFK,DependantUpon,ProjectPK,Comments,ModifiedBy,LastModified,DisplaySortOrder,CalcFiringOrder) values(#ParentID#,#MilestoneFieldFk#,#CalculationFK#,#DependantUpon#,#ProjectPK#,'#Comments#','#ModifiedBy#',getdate(),#DisplaySortOrder#,#CalcFiringOrder#);";


            InsStr = InsStr.Replace("#ParentID#", CurrentParent);
            InsStr = InsStr.Replace("#MilestoneFieldFk#", MilestoneField.ToString());
            InsStr = InsStr.Replace("#CalculationFK#", CurrentCalculation);
            InsStr = InsStr.Replace("#DependantUpon#", CurrentDependency);
            InsStr = InsStr.Replace("#ProjectPK#", ProjectID.ToString());
            InsStr = InsStr.Replace("#Comments#", "Added after project creation");
            InsStr = InsStr.Replace("#ModifiedBy#", CurrentUser);
            InsStr = InsStr.Replace("#DisplaySortOrder#", DisplayOrder);
            InsStr = InsStr.Replace("#CalcFiringOrder#", CalcFiringOrder.ToString());

            ExecuteNonQueryWrapper(InsStr);

            //create project history entry
            ProjectDisplay pd = GetAllProjects().Where(x => x.ID == ProjectID).First();
            string ProjectName = pd.Name;
            string Desc = Scheduling.Database.Utility.GetMilestoneDescFromID(MilestoneField);
            string Comments = string.Format("Add Major Milestone {0} to the Project ", Desc);
            CreateProjectHistoryEntry(pd.CurrentProjectStatus, Comments, pd.ID, pd.CurrentVersion);
        }

        public static int ReCreateMilestoneValuesOnProjectReset(int ProjectID, string StartingDueDate)
        {
            EditProjectMilestoneFieldNodeDisplay epmfnd = new EditProjectMilestoneFieldNodeDisplay();
            epmfnd.ProjectID = ProjectID;

            DateTime CurrentDateTime = Convert.ToDateTime(StartingDueDate);

            string NsMilestoneVal = ConfigurationManager.AppSettings["NewsstandOnSaleDateMilestoneValue"];
            epmfnd.MilestoneFieldID = Convert.ToInt32(NsMilestoneVal);
            epmfnd.Day = CurrentDateTime.Day;
            epmfnd.Month = CurrentDateTime.Month;
            epmfnd.Year = CurrentDateTime.Year;

            List<EditProjectMilestoneFieldNodeDisplay> UpdateList = Scheduling.Database.Utility.GetMilestoneFieldNodesBasedOnStartingNodeForEditProject(epmfnd);

            string ExcStr = string.Empty;
            foreach (EditProjectMilestoneFieldNodeDisplay item in UpdateList)
            {

                string CurrentDueDate = string.Format("{0}/{1}/{2}", item.Month, item.Day, item.Year);
                string CurrentStr = string.Format("update dbo.milestonevalue set DueDate='{0}' where projectpk={1} and milestonefieldfk={2};", CurrentDueDate, epmfnd.ProjectID, item.MilestoneFieldID);
                ExcStr += CurrentStr;


            }

            ExecuteNonQueryWrapper(ExcStr);

            return 0;
        }


        public static int ReCreateMilestoneFieldsFromProfileTableOnProjectReset(int ProjectID, string StartingDueDate)
        {
            string NsMilestoneVal = ConfigurationManager.AppSettings["NewsstandOnSaleDateMilestoneValue"];

            ProjectDisplay p = Scheduling.Database.Utility.GetAllProjects().Where(x => x.ID == ProjectID).First();
            int NsFieldID = Convert.ToInt32(Scheduling.StringFunctions.Utility.GetAppSettingValue("NewsstandOnSaleDateMilestoneValue"));
            int CurrentMilestoneProfile = p.MilestoneTreeSettingsProfileFK.Value;

            string ComText = string.Format("select * from dbo.MilestoneTreeSettings where MilestoneTreeSettingsProfileFK={0} order by CalcFiringOrder", CurrentMilestoneProfile);
            SqlCommand com = new SqlCommand(ComText, GetConnection());
            SqlDataReader sdr = null;
            List<MilestoneTreeSetting> MtsList = new List<MilestoneTreeSetting>();
            try
            {
                com.Connection.Open();
                sdr = com.ExecuteReader();

                while (sdr.Read())
                {

                    if (sdr.HasRows)
                    {
                        MilestoneTreeSetting mts = new MilestoneTreeSetting();
                        mts.MilestoneField = Convert.ToInt32(sdr["MilestoneFieldFK"]);
                        mts.MilestoneParentField = GetNullableIntValueFromDbField(sdr["MilestoneFieldParentFK"]);
                        mts.MilestoneFieldDependantUpon = GetNullableIntValueFromDbField(sdr["DependantUpon"]);
                        mts.CalculationID = GetNullableIntValueFromDbField(sdr["CalculationID"]);
                        mts.CalcFiringOrder = GetNullableIntValueFromDbField(sdr["CalcFiringOrder"]);
                        mts.RangeCalculationID = GetNullableIntValueFromDbField(sdr["RangeCalculationID"]);
                        mts.MilestoneTreeSettingsProfileID = CurrentMilestoneProfile;
                        mts.DisplayOrder = Convert.ToInt32(sdr["DisplayOrder"]);
                        MtsList.Add(mts);

                    }


                }


            }



            finally
            {
                if (!sdr.IsClosed) sdr.Close();
                if (com.Connection.State == ConnectionState.Open) com.Connection.Close();
                com.Dispose();
            }


            if (MtsList.Count > 0)
            {
                string ExcStr = string.Empty;

                //whack existing entries then rebuild
                string DelStr = string.Format("delete from dbo.milestonevalue where projectpk = {0}", ProjectID);
                ExecuteNonQueryWrapper(DelStr);

                foreach (MilestoneTreeSetting mts in MtsList)
                {
                    string CurrentStr = "insert into dbo.milestonevalue (ParentID,MilestoneFieldFK,CalculationFK,RangeCalculationFK,DependantUpon,ProjectPK,Comments,DueDate,ModifiedBy,LastModified,DisplaySortOrder,CalcFiringOrder)";

                    string ParentID = Scheduling.StringFunctions.Utility.ConvertNullableIntToInsertString(mts.MilestoneParentField);
                    string MilestoneFieldFK = mts.MilestoneField.ToString();
                    string CalculationFK = Scheduling.StringFunctions.Utility.ConvertNullableIntToInsertString(mts.CalculationID);
                    string RangeCalculationFK = Scheduling.StringFunctions.Utility.ConvertNullableIntToInsertString(mts.RangeCalculationID);
                    string DependantUpon = Scheduling.StringFunctions.Utility.ConvertNullableIntToInsertString(mts.MilestoneFieldDependantUpon);
                    string ProjectPK = ProjectID.ToString();
                    string Comments = "ReCreating From Profile";
                    //todo consider revising

                    string DisplaySortOrder = Scheduling.StringFunctions.Utility.ConvertNullableIntToInsertString(mts.DisplayOrder);
                    string CalcFiringOrder = Scheduling.StringFunctions.Utility.ConvertNullableIntToInsertString(mts.CalcFiringOrder);

                    string DueDate = "null";
                    if (mts.MilestoneField == Convert.ToInt32(ConfigurationManager.AppSettings["NewsstandOnSaleDateMilestoneValue"]) && StartingDueDate.ToLower() != "null")
                    {
                        DueDate = string.Format("'{0}'", StartingDueDate);
                    }

                    //todo consider using range date calculation here..For now let them use it in the edit page.
                    CurrentStr += "values (#ParentID#,#MilestoneFieldFK#,#CalculationFK#,#RangeCalculationFK#,#DependantUpon#,#ProjectPK#,'#Comments#',#DueDate#,'#ModifiedBy#',getdate(),#DisplaySortOrder#,#CalcFiringOrder#);";

                    CurrentStr = CurrentStr.Replace("#ParentID#", ParentID);
                    CurrentStr = CurrentStr.Replace("#MilestoneFieldFK#", MilestoneFieldFK);
                    CurrentStr = CurrentStr.Replace("#CalculationFK#", CalculationFK);
                    CurrentStr = CurrentStr.Replace("#RangeCalculationFK#", RangeCalculationFK);
                    CurrentStr = CurrentStr.Replace("#DependantUpon#", DependantUpon);
                    CurrentStr = CurrentStr.Replace("#ProjectPK#", ProjectPK);
                    CurrentStr = CurrentStr.Replace("#Comments#", Comments);
                    CurrentStr = CurrentStr.Replace("#DueDate#", DueDate);
                    CurrentStr = CurrentStr.Replace("#ModifiedBy#", Scheduling.Security.Utility.GetCurrentLoggedInUser());
                    CurrentStr = CurrentStr.Replace("#DisplaySortOrder#", DisplaySortOrder);
                    CurrentStr = CurrentStr.Replace("#CalcFiringOrder#", CalcFiringOrder);
                    ExcStr += CurrentStr;
                }
                ExecuteNonQueryWrapper(ExcStr);

            }
            //second part is creating insert statement..
            CreateProjectHistoryEntry(1, "Rebuilding Project Milestone Fields Based on Profile", ProjectID, 1);
            return 1;

        }



        public static int CreateMilestonesValuesFromProfileTable(SingleProjectWithNewstand spwn, int CurrentProjectEntry)
        {
            string NsMilestoneVal = ConfigurationManager.AppSettings["NewsstandOnSaleDateMilestoneValue"];

            int CurrentMilestoneProfile = spwn.Profile;

            string ComText = string.Format("select * from dbo.MilestoneTreeSettings where MilestoneTreeSettingsProfileFK={0} order by CalcFiringOrder", CurrentMilestoneProfile);
            SqlCommand com = new SqlCommand(ComText, GetConnection());
            SqlDataReader sdr = null;
            List<MilestoneTreeSetting> MtsList = new List<MilestoneTreeSetting>();
            try
            {
                com.Connection.Open();
                sdr = com.ExecuteReader();

                while (sdr.Read())
                {

                    if (sdr.HasRows)
                    {
                        MilestoneTreeSetting mts = new MilestoneTreeSetting();
                        mts.MilestoneField = Convert.ToInt32(sdr["MilestoneFieldFK"]);
                        mts.MilestoneParentField = GetNullableIntValueFromDbField(sdr["MilestoneFieldParentFK"]);
                        mts.MilestoneFieldDependantUpon = GetNullableIntValueFromDbField(sdr["DependantUpon"]);
                        mts.CalculationID = GetNullableIntValueFromDbField(sdr["CalculationID"]);
                        mts.CalcFiringOrder = GetNullableIntValueFromDbField(sdr["CalcFiringOrder"]);
                        mts.RangeCalculationID = GetNullableIntValueFromDbField(sdr["RangeCalculationID"]);
                        mts.MilestoneTreeSettingsProfileID = CurrentMilestoneProfile;
                        mts.DisplayOrder = Convert.ToInt32(sdr["DisplayOrder"]);
                        MtsList.Add(mts);

                    }


                }


            }



            finally
            {
                if (!sdr.IsClosed) sdr.Close();
                if (com.Connection.State == ConnectionState.Open) com.Connection.Close();
                com.Dispose();
            }


            if (MtsList.Count > 0)
            {
                string ExcStr = string.Empty;
                foreach (MilestoneTreeSetting mts in MtsList)
                {
                    string CurrentStr = "insert into dbo.milestonevalue (ParentID,MilestoneFieldFK,CalculationFK,RangeCalculationFK,DependantUpon,ProjectPK,Comments,DueDate,ModifiedBy,LastModified,DisplaySortOrder,CalcFiringOrder)";

                    string ParentID = Scheduling.StringFunctions.Utility.ConvertNullableIntToInsertString(mts.MilestoneParentField);
                    string MilestoneFieldFK = mts.MilestoneField.ToString();
                    string CalculationFK = Scheduling.StringFunctions.Utility.ConvertNullableIntToInsertString(mts.CalculationID);
                    string RangeCalculationFK = Scheduling.StringFunctions.Utility.ConvertNullableIntToInsertString(mts.RangeCalculationID);
                    string DependantUpon = Scheduling.StringFunctions.Utility.ConvertNullableIntToInsertString(mts.MilestoneFieldDependantUpon);
                    string ProjectPK = CurrentProjectEntry.ToString();
                    string Comments = "Created From Profile";
                    //todo consider revising

                    string DisplaySortOrder = Scheduling.StringFunctions.Utility.ConvertNullableIntToInsertString(mts.DisplayOrder);
                    string CalcFiringOrder = Scheduling.StringFunctions.Utility.ConvertNullableIntToInsertString(mts.CalcFiringOrder);

                    string DueDate = "null";
                    if (mts.MilestoneField == Convert.ToInt32(ConfigurationManager.AppSettings["NewsstandOnSaleDateMilestoneValue"]) && spwn.NewsStandDate.ToLower() != "null")
                    {
                        DueDate = string.Format("'{0}'", spwn.NewsStandDate);
                    }

                    //todo consider using range date calculation here..For now let them use it in the edit page.
                    CurrentStr += "values (#ParentID#,#MilestoneFieldFK#,#CalculationFK#,#RangeCalculationFK#,#DependantUpon#,#ProjectPK#,'#Comments#',#DueDate#,'#ModifiedBy#',getdate(),#DisplaySortOrder#,#CalcFiringOrder#);";

                    CurrentStr = CurrentStr.Replace("#ParentID#", ParentID);
                    CurrentStr = CurrentStr.Replace("#MilestoneFieldFK#", MilestoneFieldFK);
                    CurrentStr = CurrentStr.Replace("#CalculationFK#", CalculationFK);
                    CurrentStr = CurrentStr.Replace("#RangeCalculationFK#", RangeCalculationFK);
                    CurrentStr = CurrentStr.Replace("#DependantUpon#", DependantUpon);
                    CurrentStr = CurrentStr.Replace("#ProjectPK#", ProjectPK);
                    CurrentStr = CurrentStr.Replace("#Comments#", Comments);
                    CurrentStr = CurrentStr.Replace("#DueDate#", DueDate);
                    CurrentStr = CurrentStr.Replace("#ModifiedBy#", Scheduling.Security.Utility.GetCurrentLoggedInUser());
                    CurrentStr = CurrentStr.Replace("#DisplaySortOrder#", DisplaySortOrder);
                    CurrentStr = CurrentStr.Replace("#CalcFiringOrder#", CalcFiringOrder);
                    ExcStr += CurrentStr;
                }
                ExecuteNonQueryWrapper(ExcStr);

            }
            //second part is creating insert statement..
            CreateProjectHistoryEntry(1, "Initial Project Creation", CurrentProjectEntry, 1);
            return 1;

        }

        public static int CreateSingleProjectAndCreateMilestonesFromProfileTable(SingleProjectWithNewstand spwn)
        {
            string CurrentVersion = "1";
            string CurrentUser = Scheduling.Security.Utility.GetCurrentLoggedInUser();

            string PrevStr = "select isnull(max(ID),0) from dbo.Project";
            int PrevVal = Convert.ToInt32(ExecuteScalarWrapper(PrevStr));

            string Name = string.Empty;

            if (string.IsNullOrWhiteSpace(spwn.Product))
            {
                spwn.Product = "null";
            }

            //for now have name be in format Year / Timeline / Pubcode
            Name = Scheduling.StringFunctions.Utility.GenerateDefaultProjectName(spwn);

            string InsStr = "insert into dbo.Project (PubCodeFK,ProductFK,YearFK,MilestoneTreeSettingsProfileFK,ProjectRangeFK,CurrentVersion,CurrentProjectVersionWorkflowState,CreatedBy,DateCreated,CurrentProjectStatus,CurrentProjectStatusWorkflowState,Comments,Name,IsLocked)";
            InsStr += "values (#PubCodeFK#,#ProductFK#,#YearFK#,#MilestoneTreeSettingsProfileFK#,#ProjectRangeFK#,#CurrentVersion#,1,'#CreatedBy#',getdate(),1,1,'Project Created','#Name#',0)";

            InsStr = InsStr.Replace("#PubCodeFK#", spwn.PubCode);
            InsStr = InsStr.Replace("#ProductFK#", spwn.Product);
            InsStr = InsStr.Replace("#YearFK#", spwn.Year.ToString());
            InsStr = InsStr.Replace("#MilestoneTreeSettingsProfileFK#", spwn.Profile.ToString());
            InsStr = InsStr.Replace("#ProjectRangeFK#", spwn.Timeline);
            InsStr = InsStr.Replace("#CurrentVersion#", CurrentVersion);
            InsStr = InsStr.Replace("#CreatedBy#", CurrentUser);
            InsStr = InsStr.Replace("#Name#", Name);


            ExecuteNonQueryWrapper(InsStr);

            string NextStr = "select isnull(max(ID),0) from dbo.Project";
            int NextVal = Convert.ToInt32(ExecuteScalarWrapper(NextStr));

            if (NextVal > PrevVal)
            {
                int CurrentProfileType = GetProfileTypeIDFromProjectID(NextVal);
                if (CurrentProfileType == (int)Scheduling.Enums.BaselineType.Magazine)
                {
                    CreateKPCJobNumberForMagazineProject(NextVal);

                }

                //if project is a monthly magazine create note for next issue ads 
                int Count = Scheduling.Database.Utility.GetProjectsWherePubCodeHasNoParentForMagazineTypeProfile().Where(x => x.ID == NextVal).Count();
                if (Count == 1)
                {
                    Scheduling.Database.Utility.CreateNextIssueAdsAndCrossPromoAdsForMagazineProject(NextVal);

                }
                //Get milestone value DB Value


                return CreateMilestonesValuesFromProfileTable(spwn, NextVal);

            }

            else
            {
                return 0;

            }


        }

        public static void CreateReportFooterNoteForBaseline(int ProfileID, string TextAreaInput)
        {
            SqlCommand com = new SqlCommand();
            com.CommandText = string.Format("update dbo.[MilestoneTreeSettingsProfile] set ReportFooterNote=@ReportFooterNote where id={0}", ProfileID);
            com.Parameters.AddWithValue("@ReportFooterNote", TextAreaInput);
            ExecuteSecureCommandNonQueryWrapper(com);
        }

        public static void CreateMilestoneProfileSubItem(int ProfileID, string FieldDesc, string CalcOrder, int Parent, string Dependancy, string Calculation)
        {
            SqlCommand com = new SqlCommand();
            com.CommandText = @"insert into dbo.[MilestoneField](Description,IsCreatedByUser) values(@FieldDesc,1)";
            com.Parameters.AddWithValue("@FieldDesc", FieldDesc);

            //create the milestone field
            ExecuteSecureCommandNonQueryWrapper(com);

            //now get the max value of the miletone field 
            string ComText = "select max(ID) from MilestoneField";
            int CurrentMaxVal = Convert.ToInt32(Scheduling.Database.Utility.ExecuteScalarWrapper(ComText));

            //we now need to create the milestone tree settings entry for this profile.

            string NullStr = "null";

            //
            string MilestoneFieldFK = CurrentMaxVal.ToString();
            string MilestoneFieldParentFK = Parent.ToString();


            //dep

            string DependantUpon = NullStr;
            if (!string.IsNullOrWhiteSpace(Dependancy)) DependantUpon = Dependancy;


            //calc

            string CalculationID = NullStr;
            if (!string.IsNullOrWhiteSpace(Calculation)) CalculationID = Calculation;

            //calc order

            string CalcFiringOrder = NullStr;
            if (!string.IsNullOrWhiteSpace(CalcOrder)) CalcFiringOrder = CalcOrder;


            string InsCom = @"insert into dbo.[MilestoneTreeSettings](MilestoneFieldFk,MilestoneFieldParentFK,DependantUpon,CalculationID,CalcFiringOrder,MilestoneTreeSettingsProfileFK,DisplayOrder) values(#MilestoneFieldFk#,#MilestoneFieldParentFK#,#DependantUpon#,#CalculationID#,#CalcFiringOrder#,#MilestoneTreeSettingsProfileFK#,1)";

            InsCom = InsCom.Replace("#MilestoneFieldFk#", MilestoneFieldFK);
            InsCom = InsCom.Replace("#MilestoneFieldParentFK#", MilestoneFieldParentFK);
            InsCom = InsCom.Replace("#DependantUpon#", DependantUpon);
            InsCom = InsCom.Replace("#CalculationID#", CalculationID);
            InsCom = InsCom.Replace("#CalcFiringOrder#", CalcFiringOrder);
            InsCom = InsCom.Replace("#MilestoneTreeSettingsProfileFK#", ProfileID.ToString());

            ExecuteNonQueryWrapper(InsCom);

        }

        public static List<ProjectLinkSetting> GetAllProjectLinkSettings()
        {

            string ComText = "select * from dbo.ProjectLinkSettings";
            SqlCommand com = new SqlCommand(ComText, GetConnection());
            SqlDataReader sdr = null;
            List<ProjectLinkSetting> SettingList = new List<ProjectLinkSetting>();


            try
            {
                com.Connection.Open();
                sdr = com.ExecuteReader();

                while (sdr.Read())
                {

                    if (sdr.HasRows)
                    {
                        ProjectLinkSetting p = new ProjectLinkSetting();
                        p.ID = Convert.ToInt32(sdr["ID"]);
                        p.Name = sdr["Name"].ToString();
                        p.PrimaryProfileTypeID = Convert.ToInt32(sdr["PrimaryProfileTypeID"]);
                        p.PrimaryMilestoneID = Convert.ToInt32(sdr["PrimaryMilestoneID"]);
                        p.SecondaryProfileTypeID = Convert.ToInt32(sdr["SecondaryProfileTypeID"]);
                        p.SecondaryMilestoneID = Convert.ToInt32(sdr["SecondaryMilestoneID"]);
                        p.CalculationID = Convert.ToInt32(sdr["CalculationID"]);
                        p.PubCode = Scheduling.Database.Utility.GetNullableIntValueFromDbField(sdr["PubCodeID"]);
                        p.Timeline = Scheduling.Database.Utility.GetNullableIntValueFromDbField(sdr["TimelineID"]);
                        SettingList.Add(p);
                    }


                }


            }


            finally
            {

                if (!sdr.IsClosed) sdr.Close();
                if (com.Connection.State == ConnectionState.Open) com.Connection.Close();
                com.Dispose();

            }

            return SettingList;
        }

        public static List<ProjectLink> GetAllProjectLinks()
        {
            string ComText = "select * from dbo.ProjectLinks";
            SqlCommand com = new SqlCommand(ComText, GetConnection());
            SqlDataReader sdr = null;
            List<ProjectLink> ProjList = new List<ProjectLink>();
            try
            {
                com.Connection.Open();
                sdr = com.ExecuteReader();

                while (sdr.Read())
                {

                    if (sdr.HasRows)
                    {
                        ProjectLink p = new ProjectLink();
                        p.ID = Convert.ToInt32(sdr["ID"]);
                        p.PrimaryProjectID = Convert.ToInt32(sdr["PrimaryProjectID"]);
                        p.SecondaryProjectID = Convert.ToInt32(sdr["SecondaryProjectID"]);
                        p.LinkSettingID = Convert.ToInt32(sdr["LinkSettingID"]);
                        ProjList.Add(p);
                    }


                }


            }



            finally
            {
                if (!sdr.IsClosed) sdr.Close();
                if (com.Connection.State == ConnectionState.Open) com.Connection.Close();
                com.Dispose();


            }

            return ProjList;

        }

        //return projects based on magazine profile that use main pub codes.used for automatic note generation for these projects
        public static List<ProjectDisplay> GetProjectsWherePubCodeHasNoParentForMagazineTypeProfile()
        {
            int MagazineProfileTypeID = Convert.ToInt32(Scheduling.StringFunctions.Utility.GetAppSettingValue("MagazineProjectType"));
            string ComText = string.Format("select * from dbo.project where pubcodefk is not null and pubcodefk in(select ID from dbo.PublicationCode where parentpub is null) and milestonetreesettingsprofilefk in (select id from milestonetreesettingsprofile where profiletypefk={0})", MagazineProfileTypeID);
            SqlCommand com = new SqlCommand(ComText, GetConnection());
            SqlDataReader sdr = null;
            List<ProjectDisplay> ProjList = new List<ProjectDisplay>();
            try
            {
                com.Connection.Open();
                sdr = com.ExecuteReader();

                while (sdr.Read())
                {

                    if (sdr.HasRows)
                    {
                        ProjectDisplay p = new ProjectDisplay();
                        p.ID = Convert.ToInt32(sdr["ID"]);
                        p.PubCodeFK = GetNullableIntValueFromDbField(sdr["PubCodeFK"]);
                        p.ProductFK = GetNullableIntValueFromDbField(sdr["ProductFK"]);
                        p.YearFK = Convert.ToInt32(sdr["YearFK"]);
                        p.MilestoneTreeSettingsProfileFK = GetNullableIntValueFromDbField(sdr["MilestoneTreeSettingsProfileFK"]);
                        p.CurrentVersion = Convert.ToInt32(sdr["CurrentVersion"]);
                        p.Comments = sdr["Comments"].ToString();
                        p.DateCreated = Convert.ToDateTime(sdr["DateCreated"]);
                        p.CreatedBy = sdr["CreatedBy"].ToString();
                        p.Name = sdr["Name"].ToString();
                        p.ProjectRangeFK = GetNullableIntValueFromDbField(sdr["ProjectRangeFK"]);
                        p.CurrentProjectStatus = Convert.ToInt32(sdr["CurrentProjectStatus"]);
                        p.CurrentVersion = Convert.ToInt32(sdr["CurrentVersion"]);
                        p.IsLocked = GetNullableIntValueFromDbField(sdr["IsLocked"]);
                        ProjList.Add(p);
                    }


                }


            }

            finally
            {
                if (!sdr.IsClosed) sdr.Close();
                if (com.Connection.State == ConnectionState.Open) com.Connection.Close();
                com.Dispose();


            }

            return ProjList;


        }

        public static int? GetProjectRangeDisplayOrderFromProjectRangeID(int? i)
        {
            int? SortOrder = null;
            if (i.HasValue)
            {
                string ComText = string.Format("select DisplayOrder from dbo.ProjectRange where id={0}", i);
                SortOrder = Convert.ToInt32(ExecuteScalarWrapper(ComText));
            }

            return SortOrder;
        }

        public static List<ProjectDisplay> GetFilteredProjects(int pubcode, int year)
        {

            string ComText = string.Format("select * from dbo.Project where yearfk ={0} and pubcodefk ={1}", year, pubcode);
            SqlCommand com = new SqlCommand(ComText, GetConnection());
            SqlDataReader sdr = null;
            List<ProjectDisplay> ProjList = new List<ProjectDisplay>();
            try
            {
                com.Connection.Open();
                sdr = com.ExecuteReader();

                while (sdr.Read())
                {

                    if (sdr.HasRows)
                    {
                        ProjectDisplay p = new ProjectDisplay();
                        p.ID = Convert.ToInt32(sdr["ID"]);
                        p.PubCodeFK = GetNullableIntValueFromDbField(sdr["PubCodeFK"]);
                        p.ProductFK = GetNullableIntValueFromDbField(sdr["ProductFK"]);
                        p.YearFK = Convert.ToInt32(sdr["YearFK"]);
                        p.MilestoneTreeSettingsProfileFK = GetNullableIntValueFromDbField(sdr["MilestoneTreeSettingsProfileFK"]);
                        p.CurrentVersion = Convert.ToInt32(sdr["CurrentVersion"]);
                        p.Comments = sdr["Comments"].ToString();
                        p.DateCreated = Convert.ToDateTime(sdr["DateCreated"]);
                        p.CreatedBy = sdr["CreatedBy"].ToString();
                        p.Name = sdr["Name"].ToString();
                        p.ProjectRangeFK = GetNullableIntValueFromDbField(sdr["ProjectRangeFK"]);
                        p.CurrentProjectStatus = Convert.ToInt32(sdr["CurrentProjectStatus"]);
                        p.CurrentVersion = Convert.ToInt32(sdr["CurrentVersion"]);
                        p.IsLocked = GetNullableIntValueFromDbField(sdr["IsLocked"]);
                        p.ProjectRangeSortOrder = GetProjectRangeDisplayOrderFromProjectRangeID(p.ProjectRangeFK);
                        ProjList.Add(p);
                    }


                }


            }

            catch (Exception e)
            {

                Scheduling.Database.Utility.CreateApplicationErrorLoggingEntry(e.Message);
            }

            finally
            {
                if (!sdr.IsClosed) sdr.Close();
                if (com.Connection.State == ConnectionState.Open) com.Connection.Close();
                com.Dispose();


            }

            return ProjList;




        }

        public static List<ProjectDisplay> GetAllProjects()
        {
            string ComText = "select * from dbo.Project";
            SqlCommand com = new SqlCommand(ComText, GetConnection());
            SqlDataReader sdr = null;
            List<ProjectDisplay> ProjList = new List<ProjectDisplay>();
            try
            {
                com.Connection.Open();
                sdr = com.ExecuteReader();

                while (sdr.Read())
                {

                    if (sdr.HasRows)
                    {
                        ProjectDisplay p = new ProjectDisplay();
                        p.ID = Convert.ToInt32(sdr["ID"]);
                        p.PubCodeFK = GetNullableIntValueFromDbField(sdr["PubCodeFK"]);
                        p.ProductFK = GetNullableIntValueFromDbField(sdr["ProductFK"]);
                        p.YearFK = Convert.ToInt32(sdr["YearFK"]);
                        p.MilestoneTreeSettingsProfileFK = GetNullableIntValueFromDbField(sdr["MilestoneTreeSettingsProfileFK"]);
                        p.CurrentVersion = Convert.ToInt32(sdr["CurrentVersion"]);
                        p.Comments = sdr["Comments"].ToString();
                        p.DateCreated = Convert.ToDateTime(sdr["DateCreated"]);
                        p.CreatedBy = sdr["CreatedBy"].ToString();
                        p.Name = sdr["Name"].ToString();
                        p.ProjectRangeFK = GetNullableIntValueFromDbField(sdr["ProjectRangeFK"]);
                        p.CurrentProjectStatus = Convert.ToInt32(sdr["CurrentProjectStatus"]);
                        p.CurrentVersion = Convert.ToInt32(sdr["CurrentVersion"]);
                        p.IsLocked = GetNullableIntValueFromDbField(sdr["IsLocked"]);
                        p.ProjectRangeSortOrder = GetProjectRangeDisplayOrderFromProjectRangeID(p.ProjectRangeFK);
                        ProjList.Add(p);
                    }


                }


            }

            finally
            {
                if (!sdr.IsClosed) sdr.Close();
                if (com.Connection.State == ConnectionState.Open) com.Connection.Close();
                com.Dispose();


            }

            return ProjList;

        }
        public static int CreateMilestonesForProjectEntryAndReturnMaxID(List<NodeSaveProcess> NspList, int ProjectFK)
        {
            string InsertStr = string.Empty;
            string DueDateEntry = "null";
            string EarlyDueDateEntry = "null";

            foreach (NodeSaveProcess nsp in NspList)
            {
                //There could be funky entries that are injected on the add process.Currently do not deal with them here until after the insert.
                //generate a new earlyduedate if necessary

                if (!string.IsNullOrWhiteSpace(nsp.MilestoneDueDate))
                {
                    if (nsp.MilestoneDueDate.Contains("/"))
                    {

                        string[] DueDateArray = nsp.MilestoneDueDate.Split('/');

                        if (DueDateArray.Length == 3)
                        {
                            int CurrentMonth = Convert.ToInt32(DueDateArray[0]);
                            int CurrentDay = Convert.ToInt32(DueDateArray[1]);
                            int CurrentYear = Convert.ToInt32(DueDateArray[2]);
                            DateTime CurrentDueDate = new DateTime(CurrentYear, CurrentMonth, CurrentDay);
                            DueDateEntry = string.Format("'{0}'", CurrentDueDate.ToShortDateString());


                            //we have a due date,lets see if we have a range calculation


                            if (nsp.MilestoneRangeCalcField > 0)
                            {

                                Calculation c = Database.Utility.GetAllCalculationFields().AsEnumerable().Where(x => x.ID == nsp.MilestoneRangeCalcField).First();

                                bool IncHolidays = Convert.ToBoolean(c.IncHolidays);
                                bool IncWeekends = Convert.ToBoolean(c.IncWeekends);
                                int CurrentDiff = Convert.ToInt32(c.Diff);


                                //only process if negative diff
                                if (CurrentDiff < 0)
                                {
                                    //if basic calc go ahead
                                    if (!IncHolidays && !IncWeekends)
                                    {

                                        DateTime EarlyDueDateIncExc = CurrentDueDate.AddDays(CurrentDiff);
                                        EarlyDueDateEntry = string.Format("'{0}'", EarlyDueDateIncExc.ToShortDateString());

                                    }

                                    //factor in holidays and weekends...
                                    else
                                    {
                                        List<DateTime> HolidayList = Database.Utility.GetAllHolidays();
                                        int OriginalDiff = CurrentDiff;
                                        int ExtDays = Calc.CalcUtilities.GetExtendedDaysForNegativeDiff(CurrentDueDate, CurrentDiff, IncHolidays, IncWeekends, HolidayList, 0, OriginalDiff);
                                        int FinalDays = ExtDays + CurrentDiff;

                                        DateTime EarlyDueDate = CurrentDueDate.AddDays(CurrentDiff);
                                        EarlyDueDateEntry = string.Format("'{0}'", EarlyDueDate.ToShortDateString());


                                    }

                                }



                            }
                        }


                    }

                }



                string CurrentStr = "insert into dbo.MilestoneValue (ParentID,MilestoneFieldFK,CalculationFk,RangeCalculationFK,DependantUpon,ProjectPK,DueDate,EarlyDueDate)";
                CurrentStr += "values (#ParentID#,#MilestoneFieldFK#,#CalculationFK#,#RangeCalculationFK#,#DependantUpon#,#ProjectPK#,#DueDate#,#EarlyDueDate#);";
                //parent
                string ParentIDVal = "null"; if (nsp.MilestoneParentField > 0) { ParentIDVal = nsp.MilestoneParentField.ToString(); }
                CurrentStr = CurrentStr.Replace("#ParentID#", ParentIDVal);

                //milestonefield
                CurrentStr = CurrentStr.Replace("#MilestoneFieldFK#", nsp.MilestoneField.ToString());

                //calculationFK

                string CalculationFKVal = "null"; if (nsp.MilestoneDepCalcField > 0) { CalculationFKVal = nsp.MilestoneDepCalcField.ToString(); }
                CurrentStr = CurrentStr.Replace("#CalculationFK#", CalculationFKVal);

                //range calc fk
                string RangeCalculationFKVal = "null"; if (nsp.MilestoneRangeCalcField > 0) { RangeCalculationFKVal = nsp.MilestoneRangeCalcField.ToString(); }
                CurrentStr = CurrentStr.Replace("#RangeCalculationFK#", RangeCalculationFKVal);

                //dep upon
                string DependantUponVal = "null"; if (nsp.MilestoneDepField > 0) { DependantUponVal = nsp.MilestoneDepField.ToString(); }
                CurrentStr = CurrentStr.Replace("#DependantUpon#", DependantUponVal);

                //project pk
                CurrentStr = CurrentStr.Replace("#ProjectPK#", ProjectFK.ToString());

                //duedate dealt with previously
                CurrentStr = CurrentStr.Replace("#DueDate#", DueDateEntry);

                //earlyduedate dealt with previously
                CurrentStr = CurrentStr.Replace("#EarlyDueDate#", EarlyDueDateEntry);

                InsertStr += CurrentStr;
            }

            string ExcStr = InsertStr + "select max(ID) from dbo.MilestoneValue;";
            int RetInt = Convert.ToInt32(Database.Utility.ExecuteScalarWrapper(ExcStr));

            return RetInt;
        }

        public static int CreateSingleProjectEntryAndReturnMaxID(List<NodeSaveProcess> NspList)
        {
            //name will be pulled from session
            HttpContext.Current.Session["CurrentUser"] = "Steve Curran";

            //todo figure out if we want to really delete records at the moment

            int CurrentProjectMilestoneProfileSetting = NspList.AsEnumerable().First().ProjectMilestoneTreeSettingsProfile;
            int CurrentProjectProductSetting = NspList.AsEnumerable().First().ProjectProduct;
            int CurrentProjectPubcodeSetting = NspList.AsEnumerable().First().ProjectPubCode;
            int CurrentProjectYearSetting = NspList.AsEnumerable().First().ProjectYear;
            int CurrentProjectRangeSetting = NspList.AsEnumerable().First().ProjectRangeField;

            string PubCodeFK = "null"; if (CurrentProjectPubcodeSetting != 0) { PubCodeFK = CurrentProjectPubcodeSetting.ToString(); };
            string ProductFK = "null"; if (CurrentProjectProductSetting != 0) { ProductFK = CurrentProjectProductSetting.ToString(); };
            string ProjectRangeFK = "null"; if (CurrentProjectRangeSetting != 0) { ProjectRangeFK = CurrentProjectRangeSetting.ToString(); };


            string ProjectInsertStr = "insert into dbo.Project (PubCodeFK,ProductFk,YearFK,MilestoneTreeSettingsProfileFK,ProjectRangeFK,CurrentVersion,CurrentProjectVersionWorkflowState,CurrentProjectStatus,CurrentProjectStatusWorkflowState,CreatedBy,DateCreated,Comments,IsLocked)";
            ProjectInsertStr += "values(#PubCodeFK#,#ProductFk#,#YearFK#,#MilestoneTreeSettingsProfileFK#,#ProjectRangeFK#,#CurrentVersion#,#CurrentProjectVersionWorkflowState#,#CurrentProjectStatus#,#CurrentProjectStatusWorkflowState#,'#CreatedBy#',getdate(),'#Comments#',0);select max(ID) from dbo.Project";

            //replace pubcode
            ProjectInsertStr = ProjectInsertStr.Replace("#PubCodeFK#", PubCodeFK);

            //replace product
            ProjectInsertStr = ProjectInsertStr.Replace("#ProductFk#", ProductFK);

            //Year

            ProjectInsertStr = ProjectInsertStr.Replace("#YearFK#", CurrentProjectYearSetting.ToString());

            //MilestoneTreeSettingsProfile
            ProjectInsertStr = ProjectInsertStr.Replace("#MilestoneTreeSettingsProfileFK#", CurrentProjectMilestoneProfileSetting.ToString());

            //Project Range/Timeline
            ProjectInsertStr = ProjectInsertStr.Replace("#ProjectRangeFK#", ProjectRangeFK);

            //Current Version
            ProjectInsertStr = ProjectInsertStr.Replace("#CurrentVersion#", "1");

            //current version workflow state of 1 for created.
            ProjectInsertStr = ProjectInsertStr.Replace("#CurrentProjectVersionWorkflowState#", "1");

            //current status
            ProjectInsertStr = ProjectInsertStr.Replace("#CurrentProjectStatus#", Scheduling.Enums.ProjectStatus.Created.ToString());

            //current Project Status Workflow Status for created ..

            ProjectInsertStr = ProjectInsertStr.Replace("#CurrentProjectStatusWorkflowState#", "1");

            //created by
            ProjectInsertStr = ProjectInsertStr.Replace("#CreatedBy#", HttpContext.Current.Session["CurrentUser"].ToString());

            //Comments
            ProjectInsertStr = ProjectInsertStr.Replace("#Comments#", "Initial Creation");

            int RetInt = Convert.ToInt32(Database.Utility.ExecuteScalarWrapper(ProjectInsertStr));
            return RetInt;

        }

        public static List<ProjectProfileType> GetAllProjectProfileTypes()
        {

            List<ProjectProfileType> ProjTypeList = new List<ProjectProfileType>();

            string ComText = ("select * from dbo.ProjectProfileType");

            SqlCommand com = new SqlCommand(ComText, GetConnection());
            SqlDataReader sdr = null;

            try
            {
                com.Connection.Open();
                sdr = com.ExecuteReader();

                while (sdr.Read())
                {

                    if (sdr.HasRows)
                    {
                        ProjectProfileType ppt = new ProjectProfileType();
                        ppt.ID = Convert.ToInt32(sdr["ID"]);
                        ppt.Description = sdr["Description"].ToString();
                        ProjTypeList.Add(ppt);
                    }


                }


            }



            finally
            {
                if (!sdr.IsClosed) sdr.Close();
                if (com.Connection.State == ConnectionState.Open) com.Connection.Close();
                com.Dispose();


            }

            return ProjTypeList;

        }

        public static List<ProjectProfileType> GetAllExistingProjectProfileTypes()
        {

            List<ProjectProfileType> ProfileTypeList = new List<ProjectProfileType>();

            string ComText = "select * from dbo.projectprofiletype where id in";
            ComText = ComText + "(select profiletypefk from milestonetreesettingsprofile where id in(select distinct milestonetreesettingsprofilefk from dbo.project))";

            SqlCommand com = new SqlCommand(ComText, GetConnection());
            SqlDataReader sdr = null;

            //start 
            try
            {
                com.Connection.Open();
                sdr = com.ExecuteReader();

                while (sdr.Read())
                {

                    if (sdr.HasRows)
                    {
                        ProjectProfileType ppt = new ProjectProfileType();
                        ppt.ID = Convert.ToInt32(sdr["ID"]);
                        ppt.Description = sdr["Description"].ToString();
                        ProfileTypeList.Add(ppt);
                    }


                }


            }


            finally
            {

                if (!sdr.IsClosed) sdr.Close();
                if (com.Connection.State == ConnectionState.Open) com.Connection.Close();
                com.Dispose();


            }
            //stop

            return ProfileTypeList;

        }

        public static List<Timeline> GetAllExistingProjectRanges()
        {
            List<int?> DistinctList = GetAllProjects().Where(x => x.ProjectRangeFK != null).Select(x => x.ProjectRangeFK).Distinct().ToList();
            List<Timeline> RetList = GetAllProjectRanges().Where(x => DistinctList.Contains(x.ID)).OrderBy(x => x.ID).ToList();
            return RetList;
        }

        public static List<ProjectStatus> GetAllExistingProjectStatuses()
        {

            List<int> DistinctList = GetAllProjects().Select(x => x.CurrentProjectStatus).Distinct().ToList();
            List<ProjectStatus> RetList = GetAllProjectStatuses().Where(x => DistinctList.Contains(x.ID)).OrderBy(x => x.ID).ToList();
            return RetList;
        }

        public static List<Year> GetAllExistingProjectYears()
        {

            List<int> DistinctList = GetAllProjects().Select(x => x.YearFK).Distinct().ToList();
            List<Year> RetList = GetAllYears().Where(x => DistinctList.Contains(x.ID)).OrderBy(x => x.ID).ToList();
            return RetList;
        }



        public static List<ProjectStatus> GetAllProjectStatuses()
        {

            List<ProjectStatus> ProjStatusList = new List<ProjectStatus>();

            string ComText = ("select * from dbo.ProjectStatus");

            SqlCommand com = new SqlCommand(ComText, GetConnection());
            SqlDataReader sdr = null;


            try
            {
                com.Connection.Open();
                sdr = com.ExecuteReader();

                while (sdr.Read())
                {

                    if (sdr.HasRows)
                    {
                        ProjectStatus ps = new ProjectStatus();
                        ps.ID = Convert.ToInt32(sdr["ID"]);
                        ps.Description = sdr["Description"].ToString();
                        ProjStatusList.Add(ps);

                    }


                }


            }


            finally
            {
                if (!sdr.IsClosed) sdr.Close();
                if (com.Connection.State == ConnectionState.Open) com.Connection.Close();
                com.Dispose();


            }

            return ProjStatusList;


        }


        public static List<DateTime> GetAllHolidays()
        {
            List<DateTime> DateTimeHolidayList = new List<DateTime>();
            string ComText = ("select h.digit as DayVal,h.MonthFK as MonthVal,y.Value as YearVal from dbo.Holiday h inner join dbo.Year y on y.ID=h.yearFK");

            SqlCommand com = new SqlCommand(ComText, GetConnection());
            SqlDataReader sdr = null;


            try
            {
                com.Connection.Open();
                sdr = com.ExecuteReader();

                while (sdr.Read())
                {

                    if (sdr.HasRows)
                    {
                        int CurrentDay = Convert.ToInt32(sdr["DayVal"]);
                        int CurrentMonth = Convert.ToInt32(sdr["MonthVal"]);
                        int CurrentYear = Convert.ToInt32(sdr["YearVal"]);
                        DateTime CurrentHoliday = new DateTime(CurrentYear, CurrentMonth, CurrentDay);
                        DateTimeHolidayList.Add(CurrentHoliday);
                    }


                }


            }


            finally
            {
                if (!sdr.IsClosed) sdr.Close();
                if (com.Connection.State == ConnectionState.Open) com.Connection.Close();
                com.Dispose();


            }

            return DateTimeHolidayList;

        }

        //edit project start

        public static List<EditProjectMilestoneFieldNodeDisplay> GetMilestoneFieldNodesBasedOnStartingNodeForEditProject(EditProjectMilestoneFieldNodeDisplay mfnd)
        {
            string FirOrderComText = string.Format("select min(CalcFiringOrder) from dbo.milestonevalue where projectpk ={0} and dependantupon={1}", mfnd.ProjectID, mfnd.MilestoneFieldID);
            int StartingCalcFiringOrder = Convert.ToInt32(ExecuteScalarWrapper(FirOrderComText));
            List<EditProjectMilestoneFieldNodeDisplay> MfnDisplayList = new List<EditProjectMilestoneFieldNodeDisplay>();

            List<NodeCalculationProcess> NcpList = new List<NodeCalculationProcess>();
            string ComText = string.Format("select MilestoneFieldFK,DependantUpon,CalculationFK,CalcFiringOrder from dbo.MilestoneValue where ProjectPk={0} and DependantUpon is not null and CalculationFK  is not null and CalcFiringOrder >={1}  order by CalcFiringOrder", mfnd.ProjectID, StartingCalcFiringOrder);

            SqlCommand com = new SqlCommand(ComText, GetConnection());
            SqlDataReader sdr = null;

            try
            {
                com.Connection.Open();
                sdr = com.ExecuteReader();

                while (sdr.Read())
                {

                    if (sdr.HasRows)
                    {

                        NodeCalculationProcess ncp = new NodeCalculationProcess();
                        ncp.MilestoneFieldID = Convert.ToInt32(sdr["MilestoneFieldFK"]);
                        ncp.CalculationID = Convert.ToInt32(sdr["CalculationFK"]);
                        ncp.DependantUponID = Convert.ToInt32(sdr["DependantUpon"]);
                        NcpList.Add(ncp);
                    }


                }



            }



            finally
            {

                if (!sdr.IsClosed) sdr.Close();
                if (com.Connection.State == ConnectionState.Open) com.Connection.Close();
                com.Dispose();



            }


            List<EditProjectMilestoneFieldNodeDisplay> MfnRetDisplayList = Scheduling.Calc.CalcUtilities.GetMilestoneDisplayListForEditProject(mfnd, NcpList);
            return MfnRetDisplayList;



        }



        //This should return a list that can be handled by the ajax call.
        public static List<MilestoneFieldNodeDisplay> GetMilestoneFieldNodesBasedOnStartingNode(MilestoneFieldNodeDisplay StartNode)
        {

            List<MilestoneFieldNodeDisplay> MfnDisplayList = new List<MilestoneFieldNodeDisplay>();

            List<NodeCalculationProcess> NcpList = new List<NodeCalculationProcess>();
            string ComText = string.Format("select MilestoneFieldFK,DependantUpon,CalculationID,CalcFiringOrder from dbo.MilestoneTreeSettings where MilestoneTreeSettingsProfileFK={0} and DependantUpon is not null and CalculationID is not null and CalcFiringOrder is not null  order by CalcFiringOrder", StartNode.MilestoneProfile);

            SqlCommand com = new SqlCommand(ComText, GetConnection());
            SqlDataReader sdr = null;

            try
            {
                com.Connection.Open();
                sdr = com.ExecuteReader();

                while (sdr.Read())
                {

                    if (sdr.HasRows)
                    {

                        NodeCalculationProcess ncp = new NodeCalculationProcess();
                        ncp.MilestoneFieldID = Convert.ToInt32(sdr["MilestoneFieldFK"]);
                        ncp.CalculationID = Convert.ToInt32(sdr["CalculationID"]);
                        ncp.DependantUponID = Convert.ToInt32(sdr["DependantUpon"]);
                        NcpList.Add(ncp);
                    }


                }



            }



            finally
            {

                if (!sdr.IsClosed) sdr.Close();
                if (com.Connection.State == ConnectionState.Open) com.Connection.Close();
                com.Dispose();



            }


            List<MilestoneFieldNodeDisplay> MfnRetDisplayList = Scheduling.Calc.CalcUtilities.GetMilestoneDisplayList(StartNode, NcpList);
            return MfnRetDisplayList;
        }

        public static List<Role> GetAllRoles()
        {

            List<Role> RoleList = new List<Role>();
            string ComText = "select * from dbo.Role";

            SqlCommand com = new SqlCommand(ComText, GetConnection());
            SqlDataReader sdr = null;

            try
            {

                com.Connection.Open();
                sdr = com.ExecuteReader();

                while (sdr.Read())
                {

                    if (sdr.HasRows)
                    {

                        Role r = new Role();
                        r.ID = Convert.ToInt32(sdr["ID"]);
                        r.ShortDesc = sdr["ShortDesc"].ToString();
                        r.LongDesc = sdr["LongDesc"].ToString();
                        RoleList.Add(r);
                    }


                }

            }


            finally
            {
                if (!sdr.IsClosed) sdr.Close();
                if (com.Connection.State == ConnectionState.Open) com.Connection.Close();
                com.Dispose();


            }

            return RoleList;

        }


        public static List<MainSubItem> GetAllMainSubItems()
        {
            List<MainSubItem> MainSubItemList = new List<MainSubItem>();

            string ComText = "select * from MilestoneFieldMainSubItems";

            SqlCommand com = new SqlCommand(ComText, GetConnection());
            SqlDataReader sdr = null;

            try
            {
                com.Connection.Open();
                sdr = com.ExecuteReader();

                while (sdr.Read())
                {
                    if (sdr.HasRows)
                    {

                        MainSubItem si = new MainSubItem();
                        si.ID = Convert.ToInt32(sdr["ID"]);
                        si.Description = sdr["Description"].ToString();
                        MainSubItemList.Add(si);

                    }

                }


            }

            catch
            {


            }

            finally
            {

                if (!sdr.IsClosed) sdr.Close();
                if (com.Connection.State == ConnectionState.Open) com.Connection.Close();
                com.Dispose();


            }

            return MainSubItemList;



        }
        public static List<MainSubItemSort> GetAllMilestoneFieldMainSubItemsForReportSorting()
        {

            List<MainSubItemSort> SISList = new List<MainSubItemSort>();

            string ComText = "select * from [dbo].[MilestoneFieldMainSubItemsReportSorting]";

            SqlCommand com = new SqlCommand(ComText, GetConnection());
            SqlDataReader sdr = null;

            try
            {
                com.Connection.Open();
                sdr = com.ExecuteReader();

                while (sdr.Read())
                {
                    if (sdr.HasRows)
                    {

                        MainSubItemSort sis = new MainSubItemSort();
                        sis.ID = Convert.ToInt32(sdr["ID"]);
                        sis.PubCodeFK = Scheduling.Database.Utility.GetNullableIntValueFromDbField(sdr["PubCodeFK"]);
                        sis.ProjectRangeFK = Scheduling.Database.Utility.GetNullableIntValueFromDbField(sdr["ProjectRangeFK"]);
                        sis.MilestoneFieldSubItemFK = Convert.ToInt32(sdr["MilestoneFieldMainSubItemFk"]);
                        sis.SortOrder = Convert.ToInt32(sdr["SortOrder"]);
                        SISList.Add(sis);

                    }

                }


            }

            catch
            {


            }

            finally
            {

                if (!sdr.IsClosed) sdr.Close();
                if (com.Connection.State == ConnectionState.Open) com.Connection.Close();
                com.Dispose();


            }

            return SISList;


        }

        public static List<SpecialIssue> GetAllSpecialIssues()
        {
            List<SpecialIssue> SIPList = new List<SpecialIssue>();

            string ComText = "select * from [dbo].[SpecialIssues]";

            SqlCommand com = new SqlCommand(ComText, GetConnection());
            SqlDataReader sdr = null;

            try
            {
                com.Connection.Open();
                sdr = com.ExecuteReader();

                while (sdr.Read())
                {
                    if (sdr.HasRows)
                    {

                        SpecialIssue sip = new SpecialIssue();
                        sip.ID = Convert.ToInt32(sdr["ID"]);
                        sip.PubCodeFK = Scheduling.Database.Utility.GetNullableIntValueFromDbField(sdr["PubCodeFK"]);
                        sip.YearFk = Convert.ToInt32(sdr["YearFK"]);
                        sip.ShortDesc = sdr["ShortDesc"].ToString();
                        sip.LongDesc = Scheduling.Database.Utility.GetNullableStringValueFromDbField(sdr["LongDesc"]);
                        sip.NewsstandDate = Scheduling.Database.Utility.GetNullableDateTimeValueFromDbField(sdr["NewsstandDate"]);
                        SIPList.Add(sip);

                    }

                }


            }

            catch
            {


            }

            finally
            {

                if (!sdr.IsClosed) sdr.Close();
                if (com.Connection.State == ConnectionState.Open) com.Connection.Close();
                com.Dispose();


            }

            return SIPList;
        }

        public static List<MainSubItem> GetAllMainSubItemsForReporting()
        {
            List<MainSubItem> SubItemList = new List<MainSubItem>();
            string ComText = "select * from [dbo].MilestoneFieldMainSubItems";

            SqlCommand com = new SqlCommand(ComText, GetConnection());
            SqlDataReader sdr = null;

            try
            {
                com.Connection.Open();
                sdr = com.ExecuteReader();

                while (sdr.Read())
                {
                    if (sdr.HasRows)
                    {
                        MainSubItem msi = new MainSubItem();
                        msi.ID = Convert.ToInt32(sdr["id"]);
                        msi.Description = sdr["Description"].ToString();
                        SubItemList.Add(msi);
                    }

                }

            }



            finally
            {

                if (!sdr.IsClosed) sdr.Close();
                if (com.Connection.State == ConnectionState.Open) com.Connection.Close();
                com.Dispose();

            }

            return SubItemList;

        }

        //Called by master page when app loads..catch error
        public static List<User> GetAllUsers()
        {
            List<User> UserList = new List<User>();
            string ComText = "select * from [dbo].[User]";

            SqlCommand com = new SqlCommand(ComText, GetConnection());
            SqlDataReader sdr = null;

            try
            {
                com.Connection.Open();
                sdr = com.ExecuteReader();

                while (sdr.Read())
                {
                    if (sdr.HasRows)
                    {

                        User u = new User();
                        u.ID = Convert.ToInt32(sdr["ID"]);
                        u.Email = sdr["Email"].ToString();
                        u.UserName = sdr["UserName"].ToString();
                        u.RoleFK = Convert.ToInt32(sdr["RoleFK"]);
                        UserList.Add(u);

                    }

                }



            }



            finally
            {

                if (!sdr.IsClosed) sdr.Close();
                if (com.Connection.State == ConnectionState.Open) com.Connection.Close();
                com.Dispose();

            }

            return UserList;

        }


        /*User To Groups */

        public static bool IsUserInGroup(int User, int Group)
        {

            string ComText = string.Format("select count(*) from dbo.UserToGroups where UserID={0} and GroupID ={1}", User, Group);
            int Count = Convert.ToInt32(ExecuteScalarWrapper(ComText));
            return Count == 1;

        }

        public static bool IsEmailActionViable(int EventID, int ActionID)
        {
            string ComText = string.Format("select count(*) from dbo.MessagingSettings where EventFK={0} and ActionFK={1}", EventID, ActionID);
            int Count = Convert.ToInt32(ExecuteScalarWrapper(ComText));
            return Count > 0;
        }

        public static bool IsMagazineProfileTypeBasedOnSettingsProfile(int ProfileID)
        {

            string MagazineProfileTypeID = ConfigurationManager.AppSettings["MagazineProjectType"];
            string ComText = string.Format("select count(*) from MilestoneTreeSettingsProfile where ID={0} and ProfileTypeFK={1}", ProfileID, MagazineProfileTypeID);
            int Count = Convert.ToInt32(ExecuteScalarWrapper(ComText));
            return Count == 1;


        }

        public static bool IsLinkSettingAvailableForDeletion(int LinkID)
        {
            return Scheduling.Database.Utility.GetAllProjectLinks().Where(x => x.LinkSettingID == LinkID).Count() == 0;
        }

        public static bool IsProjectLinkAvailableForDeletion(int SecProjectID)
        {
            return Scheduling.Database.Utility.GetAllProjectLinks().Where(x => x.SecondaryProjectID == SecProjectID).Count() == 1;

        }
        public static bool IsGroupAvailableForDeletion(int GroupID)
        {
            string ComText = string.Format("select (select count(distinct groupid) from dbo.UserToGroups where groupid={0}) + (select count(distinct groupfk) from dbo.MessagingSettings where groupfk={1}) + (select count(distinct groupfk) from dbo.Depttogroupstopubcode where groupfk ={2})", GroupID, GroupID, GroupID);
            int Count = Convert.ToInt32(ExecuteScalarWrapper(ComText));
            return Count == 0;

        }

        public static List<User> GetUniqueEmailRecipientsBasedOnNoPubCode(int EventID, int ActionID)
        {

            StringBuilder sb = new StringBuilder("select * from dbo.[User] where Id in(");
            sb.AppendFormat(" select u.ID as u from dbo.[User] u where RoleFK in (select distinct rolefk from [dbo].[MessagingSettings] where rolefk is not null and EventFK={0} and ActionFK ={1}) ", EventID, ActionID);
            sb.Append(" union ");
            sb.AppendFormat("SELECT [UserFK] as u FROM [dbo].[MessagingSettings] where userFK is not null and EventFK={0} and ActionFK ={1}", EventID, ActionID);

            sb.Append(" union ");
            sb.AppendFormat(" select userid as u from dbo.UserToGroups where groupid in (select distinct groupfk from [dbo].[MessagingSettings] where groupfk is not null and EventFK={0} and ActionFK={1}))", EventID, ActionID);

            return GetUniqueEmailRecipients(sb.ToString());

        }

        public static List<string> GetUniqueSubItemDescriptionForReportingFromProjectID(int ProjectID)
        {
            string ComText = string.Format("select distinct [description] from dbo.MilestoneField where id in(select distinct milestonefieldfk from dbo.milestonevalue where parentid is not null and projectpk={0})", ProjectID);

            List<string> RetList = new List<string>();
            SqlDataReader sdr = null;
            SqlCommand com = new SqlCommand(ComText, GetConnection());

            try
            {

                com.Connection.Open();
                sdr = com.ExecuteReader();

                while (sdr.Read())
                {

                    if (sdr.HasRows)
                    {
                        string cs = sdr[0].ToString();
                        RetList.Add(cs);

                    }


                }

            }



            finally
            {
                if (!sdr.IsClosed) sdr.Close();
                if (com.Connection.State == ConnectionState.Open) com.Connection.Close();
                com.Dispose();


            }

            return RetList;

        }

        public static List<User> GetUniqueEmailRecipients(string ComText)
        {

            List<User> EmailUserList = new List<User>();

            SqlCommand com = new SqlCommand(ComText, GetConnection());
            SqlDataReader sdr = null;

            try
            {

                com.Connection.Open();
                sdr = com.ExecuteReader();

                while (sdr.Read())
                {

                    if (sdr.HasRows)
                    {

                        User u = new User();
                        u.ID = Convert.ToInt32(sdr["ID"]);
                        u.Email = sdr["Email"].ToString();
                        u.UserName = sdr["Username"].ToString();
                        EmailUserList.Add(u);

                    }


                }

            }



            finally
            {
                if (!sdr.IsClosed) sdr.Close();
                if (com.Connection.State == ConnectionState.Open) com.Connection.Close();
                com.Dispose();


            }

            return EmailUserList;


        }

        public static List<User> GetUniqueEmailRecipientsBasedOnPubCode(int EventID, int ActionID, int PubCodeID)
        {

            StringBuilder sb = new StringBuilder("select * from dbo.[User] where Id in(");
            sb.AppendFormat(" select u.ID as u from dbo.[User] u where RoleFK in (select distinct rolefk from [dbo].[MessagingSettings] where rolefk is not null and EventFK={0} and ActionFK ={1}) ", EventID, ActionID);
            sb.Append(" union ");
            sb.AppendFormat("SELECT [UserFK] as u FROM [dbo].[MessagingSettings] where userFK is not null and EventFK={0} and ActionFK ={1}", EventID, ActionID);

            //start factor in assigning multiple groups to a dept .the logic will look at the department 
            sb.Append(" union ");
            sb.AppendFormat("select userid as u from dbo.UserToGroups where groupid  in (select distinct groupfk from [dbo].[depttogroupstopubcode] where deptfk in(select distinct deptfk from dbo.messagingsettings where deptfk is not null and EventFK={0} and ActionFK={1}) and pubcodefk={2})", EventID, ActionID, PubCodeID);
            //stop 


            sb.Append(" union ");
            sb.AppendFormat(" select userid as u from dbo.UserToGroups where groupid in (select distinct groupfk from [dbo].[MessagingSettings] where groupfk is not null and EventFK={0} and ActionFK={1}))", EventID, ActionID);

            string ComText = sb.ToString();

            return GetUniqueEmailRecipients(ComText);


        }

        public static List<ReportingSubitem> GetAllReportingSubItems()
        {

            List<ReportingSubitem> SubItemList = new List<ReportingSubitem>();
            string ComText = "select * from dbo.MilestoneFieldMainSubItems";
            SqlCommand com = new SqlCommand(ComText, GetConnection());
            SqlDataReader sdr = null;

            try
            {

                com.Connection.Open();
                sdr = com.ExecuteReader();

                while (sdr.Read())
                {

                    if (sdr.HasRows)
                    {
                        ReportingSubitem si = new ReportingSubitem();
                        si.ID = Convert.ToInt32(sdr["ID"]);
                        si.Description = sdr["Description"].ToString();

                        SubItemList.Add(si);

                    }


                }

            }


            finally
            {
                if (!sdr.IsClosed) sdr.Close();
                if (com.Connection.State == ConnectionState.Open) com.Connection.Close();
                com.Dispose();

            }

            return SubItemList;

        }

        public static List<UserToGroups> GetAllUserToGroups()
        {
            List<UserToGroups> UserGroupList = new List<UserToGroups>();
            string ComText = "select * from UserToGroups";
            SqlCommand com = new SqlCommand(ComText, GetConnection());
            SqlDataReader sdr = null;

            try
            {

                com.Connection.Open();
                sdr = com.ExecuteReader();

                while (sdr.Read())
                {

                    if (sdr.HasRows)
                    {

                        UserToGroups utg = new UserToGroups();
                        utg.ID = Convert.ToInt32(sdr["ID"]);
                        utg.UserID = sdr["UserID"].ToString();
                        utg.GroupID = sdr["GroupID"].ToString();
                        UserGroupList.Add(utg);

                    }


                }

            }


            finally
            {
                if (!sdr.IsClosed) sdr.Close();
                if (com.Connection.State == ConnectionState.Open) com.Connection.Close();
                com.Dispose();


            }

            return UserGroupList;

        }

        public static string GetAliasByPubCodeAndMilestoneField(int? PubCode, int MileField, string CurrentValue)
        {
            string RetStr = CurrentValue;
            if (PubCode.HasValue)
            {

                int Count = Scheduling.Database.Utility.GetAllFieldAliases().Where(x => x.PubCodeFK == PubCode && x.FieldFK == MileField).Count();
                if (Count == 1)
                {
                    RetStr = Scheduling.Database.Utility.GetAllFieldAliases().Where(x => x.PubCodeFK == PubCode && x.FieldFK == MileField).First().AliasValue;
                }

            }

            return RetStr;
        }

        public static List<Year> GetAllYears()
        {

            List<Year> YearList = new List<Year>();
            string ComText = "select * from Year";

            SqlCommand com = new SqlCommand(ComText, GetConnection());
            SqlDataReader sdr = null;

            try
            {

                com.Connection.Open();
                sdr = com.ExecuteReader();

                while (sdr.Read())
                {

                    if (sdr.HasRows)
                    {

                        Year y = new Year();
                        y.ID = Convert.ToInt32(sdr["ID"]);
                        y.Value = Convert.ToInt32(sdr["Value"]);
                        y.IsActive = Convert.ToInt32(sdr["IsActive"]);
                        YearList.Add(y);

                    }


                }

            }

            finally
            {
                if (!sdr.IsClosed) sdr.Close();
                if (com.Connection.State == ConnectionState.Open) com.Connection.Close();
                com.Dispose();


            }

            return YearList;


        }


        //This can be called from mass update tool or on project creation
        public static void CreateNextIssueAdsAndCrossPromoAdsForMagazineProject(int ProjectID)
        {
            string PubVal = string.Empty;
            int ProjectNoteLabelID = (int)Scheduling.Enums.MagazineNoteLabel.NextIssueAndCrossPromo;
            ProjectDisplay pd = Scheduling.Database.Utility.GetAllProjects().Where(x => x.ID == ProjectID).First();

            //Get PubCodeStr
            if (pd.PubCodeFK.HasValue)
            {
                PubVal = Scheduling.Database.Utility.GetAllPublicationCodes().Where(x => x.ID == pd.PubCodeFK.Value).First().ShortDesc;


                string NoteVal = string.Format("CIR-ADH-{0}NEXT", PubVal);

                string ComText = string.Format("delete from dbo.[ProjectNote] where ProjectNoteLabelFK={0} and ProjectFk={1}", ProjectNoteLabelID, ProjectID);
                Scheduling.Database.Utility.ExecuteNonQueryWrapper(ComText);


                ProjectNote pn = new ProjectNote();
                pn.ProjectFK = ProjectID;
                pn.NoteLabelID = ProjectNoteLabelID;
                pn.NoteValue = NoteVal;
                CreateProjectNoteEntry(pn);
            }

        }

        //This can be called from mass update tool or on project creation
        public static void CreateKPCJobNumberForMagazineProject(int ProjectID)
        {
            int ProjectNoteLabelID = (int)Scheduling.Enums.MagazineNoteLabel.KPCJobNumber;

            string PubVal = string.Empty;

            ProjectDisplay pd = Scheduling.Database.Utility.GetAllProjects().Where(x => x.ID == ProjectID).First();

            //Get PubCodeStr
            if (pd.PubCodeFK.HasValue)
            {
                PubVal = Scheduling.Database.Utility.GetAllPublicationCodes().Where(x => x.ID == pd.PubCodeFK.Value).First().ShortDesc;
            }

            //GetYearStr
            int CurrentYear = pd.YearFK;
            string YearStr = Scheduling.Database.Utility.GetAllYears().Where(x => x.ID == pd.YearFK).First().Value.ToString();
            YearStr = YearStr.Substring(2);

            //Month Str
            int? CurrentTimeline = pd.ProjectRangeFK;
            string MonthStr = Scheduling.Database.Utility.GetAllProjectRanges().Where(x => x.ID == pd.ProjectRangeFK.Value).First().ShortDesc.ToUpper();

            string NoteVal = string.Format("MAG-{0}-{1}{2}", PubVal, MonthStr, YearStr);

            string ComText = string.Format("delete from dbo.[ProjectNote] where ProjectNoteLabelFK={0} and ProjectFk={1}", ProjectNoteLabelID, ProjectID);
            Scheduling.Database.Utility.ExecuteNonQueryWrapper(ComText);

            ProjectNote pn = new ProjectNote();
            pn.ProjectFK = ProjectID;
            pn.NoteLabelID = ProjectNoteLabelID;
            pn.NoteValue = NoteVal;
            CreateProjectNoteEntry(pn);


        }

        public static List<Timeline> GetAllProjectRanges()
        {

            List<Timeline> ProjRangeList = new List<Timeline>();

            string ComText = "select * from dbo.ProjectRange";

            SqlCommand com = new SqlCommand(ComText, GetConnection());
            SqlDataReader sdr = null;



            try
            {

                com.Connection.Open();
                sdr = com.ExecuteReader();

                while (sdr.Read())
                {

                    if (sdr.HasRows)
                    {

                        Timeline t = new Timeline();
                        t.ID = Convert.ToInt32(sdr["ID"]);
                        t.ShortDesc = sdr["ShortDesc"].ToString();
                        t.LongDesc = sdr["LongDesc"].ToString();
                        ProjRangeList.Add(t);
                    }


                }

            }


            finally
            {
                if (!sdr.IsClosed) sdr.Close();
                if (com.Connection.State == ConnectionState.Open) com.Connection.Close();
                com.Dispose();


            }

            return ProjRangeList;

        }

        public static List<PublicationCode> GetAllProjectPublicationCodes()
        {
            List<ProjectDisplay> PdList = GetAllProjects().Where(x => x.PubCodeFK != null).ToList();
            List<int?> PbList = PdList.Select(x => x.PubCodeFK).Distinct().ToList();
            List<PublicationCode> RetList = GetAllPublicationCodes().Where(x => PbList.Contains(x.ID)).OrderBy(x => x.ShortDesc).ToList();

            return RetList;


        }

        public static List<Printer> GetAllPrinters()
        {
            List<Printer> PrinterList = new List<Printer>();

            string ComText = "select * from dbo.Printer";

            SqlCommand com = new SqlCommand(ComText, GetConnection());
            SqlDataReader sdr = null;

            try
            {

                com.Connection.Open();
                sdr = com.ExecuteReader();

                while (sdr.Read())
                {

                    if (sdr.HasRows)
                    {

                        Printer p = new Printer();
                        p.ID = Convert.ToInt32(sdr["ID"]);
                        p.Company = sdr["Company"].ToString();
                        p.Address = sdr["Address"].ToString();
                        PrinterList.Add(p);

                    }


                }


            }

            catch
            {

            }

            finally
            {

                if (!sdr.IsClosed) sdr.Close();
                if (com.Connection.State == ConnectionState.Open) com.Connection.Close();
                com.Dispose();

            }

            return PrinterList;
        }

        public static List<PublicationCode> GetAllPublicationCodes(bool ShowNonActive)
        {
            string ComText = "select * from dbo.PublicationCode where IsActive =1";

            if (ShowNonActive)
            {
                ComText = "select * from dbo.PublicationCode";

            }

            List<PublicationCode> PubCodeList = new List<PublicationCode>();


            SqlCommand com = new SqlCommand(ComText, GetConnection());
            SqlDataReader sdr = null;

            try
            {

                com.Connection.Open();
                sdr = com.ExecuteReader();

                while (sdr.Read())
                {

                    if (sdr.HasRows)
                    {

                        PublicationCode p = new PublicationCode();
                        p.ID = Convert.ToInt32(sdr["ID"]);
                        p.ShortDesc = sdr["ShortDesc"].ToString();
                        p.LongDesc = sdr["LongDesc"].ToString();
                        p.ReportDesc = Scheduling.Database.Utility.GetStringValueForPossibleDBNullField(sdr["ReportDesc"]);
                        p.PrinterFK = Scheduling.Database.Utility.GetNullableIntValueFromDbField(sdr["PrinterFK"]);
                        p.ProfitCenter = Scheduling.Database.Utility.GetNullableIntValueFromDbField(sdr["ProfitCenter"]);
                        p.ParentPub = Scheduling.Database.Utility.GetNullableIntValueFromDbField(sdr["ParentPub"]);
                        p.IsAnnual = Convert.ToInt32(sdr["IsAnnual"]);
                        p.IsActive = Scheduling.Database.Utility.GetNullableIntValueFromDbField(sdr["IsActive"]);
                        p.ShowInNewsStandReport = (Convert.ToInt32(sdr["ShowInNewsStandReport"]));
                        p.HasCustomOffset = (Convert.ToInt32(sdr["HasCustomOffset"]));
                        PubCodeList.Add(p);

                    }


                }

            }

            finally
            {
                if (!sdr.IsClosed) sdr.Close();
                if (com.Connection.State == ConnectionState.Open) com.Connection.Close();
                com.Dispose();


            }

            return PubCodeList.OrderBy(x => x.ShortDesc).ToList();

        }

        public static List<PublicationCode> GetAllPublicationCodes()
        {

            List<PublicationCode> PubCodeList = new List<PublicationCode>();
            string ComText = "select * from dbo.PublicationCode where IsActive =1";

            SqlCommand com = new SqlCommand(ComText, GetConnection());
            SqlDataReader sdr = null;

            try
            {

                com.Connection.Open();
                sdr = com.ExecuteReader();

                while (sdr.Read())
                {

                    if (sdr.HasRows)
                    {

                        PublicationCode p = new PublicationCode();
                        p.ID = Convert.ToInt32(sdr["ID"]);
                        p.ShortDesc = sdr["ShortDesc"].ToString();
                        p.LongDesc = sdr["LongDesc"].ToString();
                        p.ReportDesc = Scheduling.Database.Utility.GetStringValueForPossibleDBNullField(sdr["ReportDesc"]);
                        p.PrinterFK = Scheduling.Database.Utility.GetNullableIntValueFromDbField(sdr["PrinterFK"]);
                        p.ProfitCenter = Scheduling.Database.Utility.GetNullableIntValueFromDbField(sdr["ProfitCenter"]);
                        p.ParentPub = Scheduling.Database.Utility.GetNullableIntValueFromDbField(sdr["ParentPub"]);
                        p.IsAnnual = Convert.ToInt32(sdr["IsAnnual"]);
                        p.IsActive = Scheduling.Database.Utility.GetNullableIntValueFromDbField(sdr["IsActive"]);
                        p.ShowInNewsStandReport = (Convert.ToInt32(sdr["ShowInNewsStandReport"]));
                        p.HasCustomOffset = (Convert.ToInt32(sdr["HasCustomOffset"]));
                        PubCodeList.Add(p);

                    }


                }

            }

            finally
            {
                if (!sdr.IsClosed) sdr.Close();
                if (com.Connection.State == ConnectionState.Open) com.Connection.Close();
                com.Dispose();


            }

            return PubCodeList.OrderBy(x => x.ShortDesc).ToList();

        }

        public static List<Product> GetAllProducts()
        {

            List<Product> ProdList = new List<Product>();
            string ComText = "select * from dbo.Product";

            SqlCommand com = new SqlCommand(ComText, GetConnection());
            SqlDataReader sdr = null;

            try
            {

                com.Connection.Open();
                sdr = com.ExecuteReader();

                while (sdr.Read())
                {

                    if (sdr.HasRows)
                    {

                        Product p = new Product();
                        p.ID = Convert.ToInt32(sdr["ID"]);
                        p.ProductID = sdr["ProductID"].ToString();
                        p.Description = sdr["Description"].ToString();
                        ProdList.Add(p);
                    }

                }


            }


            finally
            {
                if (!sdr.IsClosed) sdr.Close();
                if (com.Connection.State == ConnectionState.Open) com.Connection.Close();
                com.Dispose();


            }

            return ProdList;

        }

        public static void RemoveAddedMilestoneValueProjectEntries(int ProjectID, int ProfileID)
        {
            string ComText = string.Format("delete from dbo.MilestoneValue where projectpk={0} and MilestoneFieldFK not in (select distinct milestonefieldfk from dbo.MilestoneTreeSettings where MilestoneTreeSettingsProfileFK={1})", ProjectID, ProfileID);
            ExecuteNonQueryWrapper(ComText);

        }

        public static void RemoveMessagingSettingsEntriesBasedOnEventAndAction(int EventID, int ActionID)
        {
            string ComText = string.Format("delete from dbo.MessagingSettings where EventFK = {0} and ActionFK = {1}", EventID, ActionID);
            ExecuteNonQueryWrapper(ComText);

        }

        public static void RemoveMessagingEventAlongWithSettings(int EventID)
        {


            string ComText = string.Format("delete from dbo.MessagingSettings where EventFK={0};delete from MessagingEvent where ID={1}", EventID, EventID);
            ExecuteNonQueryWrapper(ComText);


        }

        public static void RemoveSingleMessagingSettingEntry(int ID)
        {
            string ComText = string.Format("delete from dbo.MessagingSettings where id={0}", ID);
            ExecuteNonQueryWrapper(ComText);
        }

        public static void RemoveSingleGroupAssociationEntry(int ID)
        {
            string ComText = string.Format("delete from dbo.DeptToGroupsToPubCode where id={0}", ID);
            ExecuteNonQueryWrapper(ComText);

        }

        public static void CreateEventActionUserMessagingSettingEntry(int EventID, int ActionID, List<string> InputUserList)
        {

            foreach (string s in InputUserList)
            {

                string ComText = string.Format("insert into dbo.MessagingSettings (EventFK,ActionFK,UserFK) values ({0},{1},{2})", EventID, ActionID, s);
                ExecuteNonQueryWrapper(ComText);
            }


        }


        public static void CreateEventActionRoleMessagingSettingEntry(int EventID, int ActionID, List<string> InputRoleList)
        {

            foreach (string s in InputRoleList)
            {

                string ComText = string.Format("insert into dbo.MessagingSettings (EventFK,ActionFK,RoleFK) values ({0},{1},{2})", EventID, ActionID, s);
                ExecuteNonQueryWrapper(ComText);
            }


        }

        public static void CreateEventActionGroupMessagingSettingEntry(int EventID, int ActionID, List<string> InputGroupList)
        {


            foreach (string s in InputGroupList)
            {
                string ComText = string.Format("insert into dbo.MessagingSettings (EventFK,ActionFK,GroupFK) values ({0},{1},{2} )", EventID, ActionID, s);
                ExecuteNonQueryWrapper(ComText);
            }


        }


        public static void CreateEventActionDeptMessagingSettingEntry(int EventID, int ActionID, List<string> InputDeptList)
        {

            foreach (string s in InputDeptList)
            {
                string ComText = string.Format("insert into dbo.MessagingSettings (EventFK,ActionFK,DeptFK) values ({0},{1},{2} )", EventID, ActionID, s);
                ExecuteNonQueryWrapper(ComText);
            }


        }

        public static void ReintroduceDependencies(int ProjectID, int ProfileID)
        {
            //for those that have setting just run an update statement;
            List<MilestoneTreeSetting> MtsList =
              Scheduling.Database.Utility.GetMilestoneTreeSettingsByProfileID(ProfileID)
                .Where(x => x.CalculationID.HasValue).OrderBy(x => x.CalcFiringOrder).ToList();

            foreach (MilestoneTreeSetting mts in MtsList)
            {

                string ComText =
                  string.Format(
                    "update [dbo].milestonevalue set calculationfk ={0},dependantupon = {1} where projectpk={2} and milestonefieldfk={3}",
                    mts.CalculationID, mts.MilestoneFieldDependantUpon, ProjectID, mts.MilestoneField);

                ExecuteNonQueryWrapper(ComText);

            }
        }

        public static void ReAddDeletedMilestoneValueProjectEntries(int ProjectID, int ProfileID)
        {
            string ComText = string.Format("select count(*)  from dbo.MilestoneTreeSettings where MilestoneTreeSettingsProfileFK={0} and MilestoneFieldFK not in(select MilestoneFieldFK from dbo.MilestoneValue where ProjectPK={1})", ProfileID, ProjectID);

            int Count = Convert.ToInt32(ExecuteScalarWrapper(ComText));

            if (Count > 0)
            {
                string SelStr = string.Format("select *  from dbo.MilestoneTreeSettings where MilestoneTreeSettingsProfileFK={0} and MilestoneFieldFK not in(select MilestoneFieldFK from dbo.MilestoneValue where ProjectPK={1})", ProfileID, ProjectID);
                SqlCommand com = new SqlCommand(SelStr, GetConnection());
                SqlDataReader sdr = null;
                List<MilestoneTreeSetting> MtsList = new List<MilestoneTreeSetting>();
                try
                {

                    com.Connection.Open();
                    sdr = com.ExecuteReader();

                    while (sdr.Read())
                    {

                        if (sdr.HasRows)
                        {
                            MilestoneTreeSetting mts = new MilestoneTreeSetting();
                            mts.ID = Convert.ToInt32(sdr["ID"]);
                            mts.MilestoneField = Convert.ToInt32(sdr["MilestoneFieldFK"]);
                            mts.MilestoneParentField = GetNullableIntValueFromDbField(sdr["MilestoneFieldParentFK"]);
                            mts.MilestoneFieldDependantUpon = GetNullableIntValueFromDbField(sdr["DependantUpon"]);
                            mts.CalculationID = GetNullableIntValueFromDbField(sdr["CalculationID"]);
                            mts.CalcFiringOrder = GetNullableIntValueFromDbField(sdr["CalcFiringOrder"]);
                            mts.RangeCalculationID = GetNullableIntValueFromDbField(sdr["RangeCalculationID"]);
                            mts.MilestoneTreeSettingsProfileID = Convert.ToInt32(sdr["MilestoneTreeSettingsProfileFK"]);
                            mts.DisplayOrder = GetNullableIntValueFromDbField(sdr["DisplayOrder"]);
                            mts.MilestoneParentField = GetNullableIntValueFromDbField(sdr["MilestoneFieldParentFK"]);

                            MtsList.Add(mts);

                        }

                    }


                }




                finally
                {

                    if (!sdr.IsClosed) sdr.Close();
                    if (com.Connection.State == ConnectionState.Open) com.Connection.Close();
                    com.Dispose();


                }


                //we have a list of milestone setting that need to be added to the milestone value table

                string ExcStr = string.Empty;

                foreach (MilestoneTreeSetting mts in MtsList)
                {

                    //Parent ID
                    string ParentID = "null"; if (mts.MilestoneParentField.HasValue) ParentID = mts.MilestoneParentField.Value.ToString();

                    //CalcID
                    string CalcID = "null"; if (mts.CalculationID.HasValue) CalcID = mts.CalculationID.Value.ToString();

                    //Dependant Upon 
                    string DepID = "null"; if (mts.MilestoneFieldDependantUpon.HasValue) DepID = mts.MilestoneFieldDependantUpon.Value.ToString();

                    //calc firing order 
                    string CalcFirOrder = "null"; if (mts.CalcFiringOrder.HasValue) CalcFirOrder = mts.CalcFiringOrder.Value.ToString();


                    string CurrentExcStr = "insert into dbo.MilestoneValue (ParentID,MilestoneFieldFK,CalculationFK,DependantUpon,ProjectPK,CalcFiringOrder,DisplaySortOrder,LastModified) values (#ParentID#,#MilestoneFieldFK#,#CalculationFK#,#DependantUpon#,#ProjectPK#,#CalcFiringOrder#,#DisplaySortOrder#,getdate());";
                    CurrentExcStr = CurrentExcStr.Replace("#ParentID#", ParentID);
                    CurrentExcStr = CurrentExcStr.Replace("#MilestoneFieldFK#", mts.MilestoneField.ToString());
                    CurrentExcStr = CurrentExcStr.Replace("#CalculationFK#", CalcID);
                    CurrentExcStr = CurrentExcStr.Replace("#DependantUpon#", DepID);
                    CurrentExcStr = CurrentExcStr.Replace("#ProjectPK#", ProjectID.ToString());
                    CurrentExcStr = CurrentExcStr.Replace("#CalcFiringOrder#", CalcFirOrder);
                    CurrentExcStr = CurrentExcStr.Replace("#DisplaySortOrder#", mts.DisplayOrder.ToString());

                    ExcStr += CurrentExcStr;

                }

                ExecuteNonQueryWrapper(ExcStr);
            }

        }

        public static void RenameProject(int ID, string Name = "----")
        {
            SqlCommand com = new SqlCommand();
            com.CommandText = @"update dbo.[Project] set [Name]=@Name where ID=@ID";
            com.Parameters.AddWithValue("@Name", Name);
            com.Parameters.AddWithValue("@ID", ID);

            ExecuteSecureCommandNonQueryWrapper(com);
        }

        public static List<int> RetrieveRowIDList(string ComText)
        {
            SqlCommand com = new SqlCommand(ComText, GetConnection());
            SqlDataReader sdr = null;
            List<int> RetIntArray = new List<int>();
            try
            {

                com.Connection.Open();
                sdr = com.ExecuteReader();

                while (sdr.Read())
                {

                    if (sdr.HasRows)
                    {

                        int CurrentValue = Convert.ToInt32(sdr[0]);
                        RetIntArray.Add(CurrentValue);
                    }

                }


            }



            finally
            {
                if (!sdr.IsClosed) sdr.Close();
                if (com.Connection.State == ConnectionState.Open) com.Connection.Close();
                com.Dispose();


            }

            return RetIntArray;

        }

        public static string GenerateDbNullEntryForValue(int? i)
        {
            if (i == null)
                return "null";
            else return i.ToString();

        }

        public static MilestoneTreeSetting GetMilestoneTreeSettingRowByID(int i)
        {
            //string ComText = string.Format("select isnull(MilestoneFieldFK,-1) as MilestoneFieldFK,isnull(MilestoneFieldParentFK,-1),isnull(* from MilestoneTreeSettings where ID={0}", i);
            string ComText = string.Format("select isnull(MilestoneFieldFK,-1) as MilestoneFieldFK,isnull(MilestoneFieldParentFK,-1) as MilestoneFieldParentFK,isnull(DependantUpon,-1) as DependantUpon,isnull(CalculationID,-1) as CalculationID,isnull(CalcFiringOrder,-1) as CalcFiringOrder,isnull(RangeCalculationID,-1) as RangeCalculationID,MilestoneTreeSettingsProfileFK from MilestoneTreeSettings where ID={0}", i);
            SqlCommand com = new SqlCommand(ComText, GetConnection());

            SqlDataReader sdr = null;

            MilestoneTreeSetting mts = new MilestoneTreeSetting();

            try
            {
                com.Connection.Open();
                sdr = com.ExecuteReader();

                while (sdr.Read())
                {

                    if (sdr.HasRows)
                    {

                        mts.MilestoneField = Convert.ToInt32(sdr["MilestoneFieldFK"]);

                        //Parent Value

                        mts.MilestoneParentField = Convert.ToInt32(sdr["MilestoneFieldParentFK"]);
                        if (mts.MilestoneParentField == -1) mts.MilestoneParentField = null;


                        //Dependency Value
                        mts.MilestoneFieldDependantUpon = Convert.ToInt32(sdr["DependantUpon"]);
                        if (mts.MilestoneFieldDependantUpon == -1) mts.MilestoneFieldDependantUpon = null;

                        //Calculation ID
                        mts.CalculationID = Convert.ToInt32(sdr["CalculationID"]);
                        if (mts.CalculationID == -1) mts.CalculationID = null;

                        //CalcFiringOrder
                        mts.CalcFiringOrder = Convert.ToInt32(sdr["CalcFiringOrder"]);
                        if (mts.CalcFiringOrder == -1) mts.CalcFiringOrder = null;

                        //RangeCalculation
                        mts.RangeCalculationID = Convert.ToInt32(sdr["RangeCalculationID"]);
                        if (mts.RangeCalculationID == -1) mts.RangeCalculationID = null;

                        //Profile
                        mts.MilestoneTreeSettingsProfileID = Convert.ToInt32(sdr["MilestoneTreeSettingsProfileFK"]);
                    }

                }

            }


            finally
            {
                if (!sdr.IsClosed) sdr.Close();
                if (com.Connection.State == ConnectionState.Open) com.Connection.Close();
                com.Dispose();
            }


            return mts;
        }

        //Admin Build insert call for CreateMilestoneSetting
        public static void ProcessCreateMilestoneSettings(List<MilestoneTreeSetting> MtsList)
        {
            string InsertStr = string.Empty;
            string ExecuteStr = string.Empty;

            MilestoneTreeSetting MtsFirst = MtsList.AsEnumerable().First();
            int CurrentProfile = MtsFirst.MilestoneTreeSettingsProfileID;

            string DeleteStr = string.Format("delete from dbo.MilestoneTreeSettings where MilestoneTreeSettingsProfileFK={0};", CurrentProfile);

            //Build individual insert strings



            foreach (MilestoneTreeSetting MtsItem in MtsList)
            {


                InsertStr = string.Format("insert into dbo.MilestoneTreeSettings values({0},{1},{2},{3},{4},{5},{6});",
                MtsItem.MilestoneField,
                GenerateDbNullEntryForValue(MtsItem.MilestoneParentField),
                GenerateDbNullEntryForValue(MtsItem.MilestoneFieldDependantUpon),
                GenerateDbNullEntryForValue(MtsItem.CalculationID),
                GenerateDbNullEntryForValue(MtsItem.CalcFiringOrder),
                GenerateDbNullEntryForValue(MtsItem.RangeCalculationID),
                MtsItem.MilestoneTreeSettingsProfileID);

                ExecuteStr = ExecuteStr + InsertStr;

            }

            string FinalStr = string.Concat(DeleteStr, ExecuteStr);
            ExecuteNonQueryWrapper(FinalStr);
        }

        //Get AllMagazineProjectsStartingDueDates For Recalculation

        public static List<EditSingleMilestoneWithDueDate> GetAllMagazineProjectStartingDueDatesForRecalculation()
        {
            List<EditSingleMilestoneWithDueDate> EditProjList = new List<EditSingleMilestoneWithDueDate>();
            int MagazineProfileType = Convert.ToInt32(Scheduling.StringFunctions.Utility.GetAppSettingValue("MagazineProjectType"));
            int NSFieldID = Convert.ToInt32(Scheduling.StringFunctions.Utility.GetAppSettingValue("NewsstandOnSaleDateMilestoneValue"));
            string ComText = string.Format("select projectpk,duedate from dbo.milestonevalue where milestonefieldfk={0} and DueDate is not null and ProjectPK in(select ID from dbo.Project where MilestoneTreeSettingsProfileFK in(select ID from MilestoneTreeSettingsProfile where ProfileTypeFK={1}))", NSFieldID, MagazineProfileType);

            SqlCommand com = new SqlCommand(ComText, GetConnection());
            SqlDataReader sdr = null;

            try
            {
                com.Connection.Open();
                sdr = com.ExecuteReader();

                while (sdr.Read())
                {

                    if (sdr.HasRows)
                    {

                        EditSingleMilestoneWithDueDate emwd = new EditSingleMilestoneWithDueDate();
                        emwd.DueDate = sdr["DueDate"].ToString();
                        emwd.MilestoneFieldFK = NSFieldID.ToString();
                        emwd.ProjectID = sdr["ProjectPk"].ToString();
                        EditProjList.Add(emwd);

                    }

                }

            }




            finally
            {

                if (!sdr.IsClosed) sdr.Close();
                if (com.Connection.State == ConnectionState.Open) com.Connection.Close();
                com.Dispose();

            }

            return EditProjList;

        }


        //------------------------------------Admin Read Calls Region--------------------------------------------- 


        public static List<MilestoneTreeSettingsProfile> GetAllUnAssignedMilestoneTreeSettingsProfiles()
        {
            List<MilestoneTreeSettingsProfile> MtsProfList = new List<MilestoneTreeSettingsProfile>();
            string ComText = "select * from dbo.MilestoneTreeSettingsProfile where ID not in(select distinct(MilestoneTreeSettingsProfileFK) from dbo.Project)";

            SqlCommand com = new SqlCommand(ComText, GetConnection());
            SqlDataReader sdr = null;


            try
            {

                com.Connection.Open();
                sdr = com.ExecuteReader();

                while (sdr.Read())
                {

                    if (sdr.HasRows)
                    {

                        MilestoneTreeSettingsProfile p = new MilestoneTreeSettingsProfile();
                        p.Description = sdr["Description"].ToString();
                        p.ID = Convert.ToInt32(sdr["ID"]);
                        p.ProjectType = Convert.ToInt32(sdr["ProfileTypeFK"]);
                        MtsProfList.Add(p);
                    }

                }


            }



            finally
            {
                if (!sdr.IsClosed) sdr.Close();
                if (com.Connection.State == ConnectionState.Open) com.Connection.Close();
                com.Dispose();


            }

            return MtsProfList;

        }

        public static List<int> GetDistinctAssignedMilestoneTreeSettings()
        {
            List<int> RetList = new List<int>();
            string ComText = "select distinct MilestoneTreeSettingsProfileFK FROM [dbo].[Project]";

            SqlCommand com = new SqlCommand(ComText, GetConnection());
            SqlDataReader sdr = null;

            //start 

            try
            {

                com.Connection.Open();
                sdr = com.ExecuteReader();

                while (sdr.Read())
                {

                    if (sdr.HasRows)
                    {
                        int ID = Convert.ToInt32(sdr[0]);
                        RetList.Add(ID);
                    }

                }


            }


            finally
            {
                if (!sdr.IsClosed) sdr.Close();
                if (com.Connection.State == ConnectionState.Open) com.Connection.Close();
                com.Dispose();


            }

            return RetList;

        }


        public static List<int> GetDistinctUnAssignedMilestoneTreeSettings()
        {
            List<int> RetList = new List<int>();
            string ComText = "SELECT distinct id from MilestoneTreeSettingsProfile where id not in(select distinct MilestoneTreeSettingsProfileFK FROM [dbo].[Project])";

            SqlCommand com = new SqlCommand(ComText, GetConnection());
            SqlDataReader sdr = null;

            //start 

            try
            {

                com.Connection.Open();
                sdr = com.ExecuteReader();

                while (sdr.Read())
                {

                    if (sdr.HasRows)
                    {
                        int ID = Convert.ToInt32(sdr[0]);
                        RetList.Add(ID);
                    }

                }


            }


            finally
            {
                if (!sdr.IsClosed) sdr.Close();
                if (com.Connection.State == ConnectionState.Open) com.Connection.Close();
                com.Dispose();


            }

            return RetList;

        }




        public static List<int> GetDistinctMilestoneTreeSettingsForProjectsBasedOnYear(int i)
        {
            List<int> RetList = new List<int>();
            string ComText = string.Format("SELECT distinct MilestoneTreeSettingsProfileFK FROM [dbo].[Project] where yearfk ={0}", i);

            SqlCommand com = new SqlCommand(ComText, GetConnection());
            SqlDataReader sdr = null;

            //start 

            try
            {

                com.Connection.Open();
                sdr = com.ExecuteReader();

                while (sdr.Read())
                {

                    if (sdr.HasRows)
                    {
                        int ID = Convert.ToInt32(sdr[0]);
                        RetList.Add(ID);
                    }

                }


            }


            finally
            {
                if (!sdr.IsClosed) sdr.Close();
                if (com.Connection.State == ConnectionState.Open) com.Connection.Close();
                com.Dispose();


            }

            return RetList;

        }


        public static List<int> GetDistinctMilestoneTreeSettingsForProjectsBasedOnPub(int i)
        {
            List<int> RetList = new List<int>();
            string ComText = string.Format("SELECT distinct MilestoneTreeSettingsProfileFK FROM [dbo].[Project] where PubCodeFK ={0}", i);

            SqlCommand com = new SqlCommand(ComText, GetConnection());
            SqlDataReader sdr = null;

            //start 

            try
            {

                com.Connection.Open();
                sdr = com.ExecuteReader();

                while (sdr.Read())
                {

                    if (sdr.HasRows)
                    {
                        int ID = Convert.ToInt32(sdr[0]);
                        RetList.Add(ID);
                    }

                }


            }


            finally
            {
                if (!sdr.IsClosed) sdr.Close();
                if (com.Connection.State == ConnectionState.Open) com.Connection.Close();
                com.Dispose();


            }

            return RetList;

        }



        public static List<MilestoneTreeSettingsProfile> GetAllMilestoneTreeSettingsProfiles()
        {

            List<MilestoneTreeSettingsProfile> MtsProfList = new List<MilestoneTreeSettingsProfile>();
            string ComText = "select * from dbo.MilestoneTreeSettingsProfile order by Description";

            SqlCommand com = new SqlCommand(ComText, GetConnection());
            SqlDataReader sdr = null;


            try
            {

                com.Connection.Open();
                sdr = com.ExecuteReader();

                while (sdr.Read())
                {

                    if (sdr.HasRows)
                    {

                        MilestoneTreeSettingsProfile p = new MilestoneTreeSettingsProfile();
                        p.Description = sdr["Description"].ToString();
                        p.ID = Convert.ToInt32(sdr["ID"]);
                        p.ProjectType = Convert.ToInt32(sdr["ProfileTypeFK"]);
                        p.ReportFooterNote = GetStringValueForPossibleDBNullField(sdr["ReportFooterNote"]);
                        MtsProfList.Add(p);
                    }

                }


            }


            finally
            {
                if (!sdr.IsClosed) sdr.Close();
                if (com.Connection.State == ConnectionState.Open) com.Connection.Close();
                com.Dispose();


            }

            return MtsProfList;

        }

        public static List<int> CreateFiringOrderList()
        {
            List<int> FirOrderList = new List<int>();
            string ComText = "select count(ID) from dbo.MilestoneField";
            int NumRecords = Convert.ToInt32(ExecuteScalarWrapper(ComText));
            for (int i = 1; i <= NumRecords; i++)
            {

                FirOrderList.Add(i);
            }

            return FirOrderList;
        }


        public static void CreateGroupAssociationEntry(int DeptID, int GroupID, int PubCodeID)
        {
            string ComText = string.Format("insert into dbo.DeptToGroupsToPubCode(DeptFk,GroupFK,PubCodeFk) values ({0},{1},{2})", DeptID, GroupID, PubCodeID);
            ExecuteNonQueryWrapper(ComText);
        }

        public static void CreateGroupEntry(string input)
        {

            SqlCommand com = new SqlCommand();
            string CurrentUser = Scheduling.Security.Utility.GetCurrentLoggedInUser();
            com.CommandText = @"insert into [dbo].[group](Description) values(@Description)";
            com.Parameters.AddWithValue("@Description", input);
            ExecuteSecureCommandNonQueryWrapper(com);
            string ActStr = string.Format("Created Group {0}", input);
            CreateActivityLogEntry(ActStr);

        }

        public static void CreateMajorMilestoneEntry(string input)
        {
            SqlCommand com = new SqlCommand();
            string CurrentUser = Scheduling.Security.Utility.GetCurrentLoggedInUser();
            com.CommandText = @"insert into dbo.MilestoneField(Description) values(@Description)";
            com.Parameters.AddWithValue("@Description", input);
            ExecuteSecureCommandNonQueryWrapper(com);

        }

        public static void CreateMessagingEventEntry(string Method, string ShortDesc, string LongDesc)
        {

            string LongDescEntry = "null";
            if (!string.IsNullOrWhiteSpace(LongDesc))
            {
                LongDescEntry = LongDesc;
            }

            string ComText = "insert into dbo.MessagingEvent(ShortDesc,Method,LongDesc) values (@ShortDesc,@Method,@LongDesc)";
            SqlCommand com = new SqlCommand();
            com.CommandText = ComText;
            com.Parameters.AddWithValue("@ShortDesc", ShortDesc);
            com.Parameters.AddWithValue("@Method", Method);
            com.Parameters.AddWithValue("@LongDesc", LongDesc);
            ExecuteSecureCommandNonQueryWrapper(com);


        }

        public static void CreateHolidayEntry(DateTime dt)
        {
            int CurrentYear = dt.Year;
            int CurrentMonth = dt.Month;
            int CurrentDay = dt.Day;

            int YearCount = GetAllYears().Where(x => x.Value == dt.Year).Count();

            if (YearCount == 1)
            {
                int YearFk = GetAllYears().Where(x => x.Value == dt.Year).First().ID;
                string ExcStr = string.Format("insert into dbo.Holiday (MonthFK,YearFK,digit) values ( {0},{1},{2});", dt.Month, YearFk, dt.Day);
                ExecuteNonQueryWrapper(ExcStr);
                CreateActivityLogEntry(string.Format("Creating Holiday Entry for {0}", dt.ToLongDateString()));
            }







        }

        public static void CreateSpecialIssueEntry(SpecialIssue si)
        {
            SqlCommand com = new SqlCommand();
            com.CommandText = @"insert into dbo.[SpecialIssues] (YearFk,PubCodeFK,ShortDesc,LongDesc,NewsstandDate) values(@YearFk,@PubCodeFK,@ShortDesc,@LongDesc,@NewsstandDate)";
            com.Parameters.AddWithValue("@YearFK", si.YearFk);
            com.Parameters.AddWithValue("@PubCodeFK", si.PubCodeFK.Value);
            com.Parameters.AddWithValue("@ShortDesc", si.ShortDesc);
            com.Parameters.AddWithValue("@LongDesc", si.LongDesc);
            com.Parameters.AddWithValue("@NewsstandDate", si.NewsstandDate);

            ExecuteSecureCommandNonQueryWrapper(com);

            string LogStr = string.Format("Adding Special Issue {0}", si.ShortDesc);
            CreateActivityLogEntry(LogStr);
        }

        public static List<Log> GetAllApplicationErrorLogs()
        {
            List<Log> LogList = new List<Log>();

            string ComText = "select * from dbo.ApplicationErrorLogging order by DateModified desc";
            SqlCommand com = new SqlCommand(ComText, GetConnection());
            SqlDataReader sdr = null;



            try
            {

                com.Connection.Open();
                sdr = com.ExecuteReader();

                while (sdr.Read())
                {

                    if (sdr.HasRows)
                    {

                        Log l = new Log();
                        l.ID = Convert.ToInt32(sdr["ID"]);
                        l.Message = sdr["Message"].ToString();
                        l.DateModified = Convert.ToDateTime(sdr["DateModified"]);

                        LogList.Add(l);
                    }
                }


            }

            catch (Exception e)
            {


                Scheduling.Database.Utility.CreateApplicationErrorLoggingEntry(e.Message);

            }

            return LogList;


        }

        public static List<Log> GetAllApplicationLogs()
        {

            List<Log> LogList = new List<Log>();

            string ComText = "select * from dbo.ApplicationLogging order by DateModified desc";
            SqlCommand com = new SqlCommand(ComText, GetConnection());
            SqlDataReader sdr = null;



            try
            {

                com.Connection.Open();
                sdr = com.ExecuteReader();

                while (sdr.Read())
                {

                    if (sdr.HasRows)
                    {

                        Log l = new Log();
                        l.ID = Convert.ToInt32(sdr["ID"]);
                        l.Message = sdr["Message"].ToString();
                        l.DateModified = Convert.ToDateTime(sdr["DateModified"]);

                        LogList.Add(l);
                    }
                }


            }

            catch (Exception e)
            {

                Scheduling.Database.Utility.CreateApplicationErrorLoggingEntry(e.Message);


            }

            return LogList;

        }

        public List<MessagingAction> GetAllActions()
        {
            string ComText = "select * from dbo.MessagingAction order by ShortDesc";
            List<MessagingAction> MessActionList = new List<MessagingAction>();
            SqlCommand com = new SqlCommand(ComText, GetConnection());
            SqlDataReader sdr = null;


            try
            {

                com.Connection.Open();
                sdr = com.ExecuteReader();

                while (sdr.Read())
                {

                    if (sdr.HasRows)
                    {

                        MessagingAction a = new MessagingAction();
                        a.ID = Convert.ToInt32(sdr["ID"]);
                        a.ShortDesc = sdr["ShortDesc"].ToString();
                        a.LongDesc = GetNullableStringValueFromDbField(sdr["LongDesc"]);
                        MessActionList.Add(a);
                    }
                }


            }



            finally
            {

                if (!sdr.IsClosed) sdr.Close();
                if (com.Connection.State == ConnectionState.Open) com.Connection.Close();
                com.Dispose();

            }

            return MessActionList;

        }

        //Get Activities
        public static List<Activity> GetAllActivities()
        {
            string ComText = "select * from dbo.Activity order by DateModified desc";
            List<Activity> ActList = new List<Activity>();
            SqlCommand com = new SqlCommand(ComText, GetConnection());
            SqlDataReader sdr = null;

            try
            {

                com.Connection.Open();
                sdr = com.ExecuteReader();

                while (sdr.Read())
                {

                    if (sdr.HasRows)
                    {

                        Activity a = new Activity();
                        a.ID = Convert.ToInt32(sdr["ID"]);
                        a.Message = sdr["Message"].ToString();
                        a.DateModified = Convert.ToDateTime(sdr["DateModified"].ToString());
                        a.ModifiedBy = sdr["ModifiedBy"].ToString();
                        a.MilestoneValueFk = GetNullableIntValueFromDbField(sdr["MilestoneValueFK"]);
                        a.ProjectFK = GetNullableIntValueFromDbField(sdr["ProjectFK"]);
                        ActList.Add(a);
                    }
                }


            }


            finally
            {
                if (!sdr.IsClosed) sdr.Close();
                if (com.Connection.State == ConnectionState.Open) com.Connection.Close();
                com.Dispose();
            }

            return ActList;

        }


        public static List<ChangeRequestStatus> GetAllChangeRequestStatuses()
        {

            List<ChangeRequestStatus> CrStatusList = new List<ChangeRequestStatus>();
            string ComText = "select * from dbo.ChangeRequestStatus";

            SqlCommand com = new SqlCommand(ComText, GetConnection());
            SqlDataReader sdr = null;

            try
            {
                com.Connection.Open();
                sdr = com.ExecuteReader();

                while (sdr.Read())
                {

                    if (sdr.HasRows)
                    {

                        ChangeRequestStatus s = new ChangeRequestStatus();
                        s.ID = Convert.ToInt32(sdr["ID"]);
                        s.Description = sdr["Description"].ToString();
                        CrStatusList.Add(s);
                    }
                }


            }


            finally
            {

                if (!sdr.IsClosed) sdr.Close();
                if (com.Connection.State == ConnectionState.Open) com.Connection.Close();
                com.Dispose();



            }

            return CrStatusList;

        }


        public static List<ProjectDisplay> GetAllAvailableLinkableProjectsByProjectID(int ProjectID)
        {
            ProjectDisplay pd = Scheduling.Database.Utility.GetAllProjects().Where(x => x.ID == ProjectID).First();
            List<ProjectDisplay> ProjList = new List<ProjectDisplay>();

            int CurrentPubCode = (int)pd.PubCodeFK;
            int CurrentYear = pd.YearFK;


            int CurrentProfileID = (int)pd.MilestoneTreeSettingsProfileFK;
            int CurrentProfileType = Scheduling.Database.Utility.GetAllMilestoneTreeSettingsProfiles().Where(x => x.ID == CurrentProfileID).First().ProjectType;


            string ComText = string.Format("select * from dbo.Project where pubcodefk={0} and yearfk={1} and milestonetreesettingsprofilefk in ( select ID from milestonetreesettingsprofile where profiletypefk=(select primaryprofiletypeid from projectlinksettings where secondaryprofiletypeid ={2}))", CurrentPubCode, CurrentYear, CurrentProfileType);

            SqlCommand com = new SqlCommand(ComText, GetConnection());
            SqlDataReader sdr = null;


            try
            {

                com.Connection.Open();
                sdr = com.ExecuteReader();

                while (sdr.Read())
                {

                    if (sdr.HasRows)
                    {

                        ProjectDisplay p = new ProjectDisplay();
                        p.ID = Convert.ToInt32(sdr["ID"]);
                        p.PubCodeFK = GetNullableIntValueFromDbField(sdr["PubCodeFK"]);
                        p.ProductFK = GetNullableIntValueFromDbField(sdr["ProductFK"]);
                        p.YearFK = Convert.ToInt32(sdr["YearFK"]);
                        p.MilestoneTreeSettingsProfileFK = GetNullableIntValueFromDbField(sdr["MilestoneTreeSettingsProfileFK"]);
                        p.CurrentVersion = Convert.ToInt32(sdr["CurrentVersion"]);
                        p.Comments = sdr["Comments"].ToString();
                        p.DateCreated = Convert.ToDateTime(sdr["DateCreated"]);
                        p.CreatedBy = sdr["CreatedBy"].ToString();
                        p.Name = sdr["Name"].ToString();
                        p.ProjectRangeFK = GetNullableIntValueFromDbField(sdr["ProjectRangeFK"]);
                        p.CurrentProjectStatus = Convert.ToInt32(sdr["CurrentProjectStatus"]);
                        p.CurrentVersion = Convert.ToInt32(sdr["CurrentVersion"]);
                        ProjList.Add(p);

                    }
                }

            }


            finally
            {

                if (!sdr.IsClosed) sdr.Close();
                if (com.Connection.State == ConnectionState.Open) com.Connection.Close();
                com.Dispose();


            }
            return ProjList;

        }

        public static List<ChangeRequest> GetAllChangeRequests()
        {

            List<ChangeRequest> CrList = new List<ChangeRequest>();
            string ComText = "select * from dbo.ProjectChangeRequest";

            SqlCommand com = new SqlCommand(ComText, GetConnection());
            SqlDataReader sdr = null;

            try
            {
                com.Connection.Open();
                sdr = com.ExecuteReader();

                while (sdr.Read())
                {

                    if (sdr.HasRows)
                    {

                        ChangeRequest c = new ChangeRequest();
                        c.ID = Convert.ToInt32(sdr["ID"]);
                        c.RequestorComment = sdr["RequestorComment"].ToString();
                        c.CreatedBy = sdr["CreatedBy"].ToString();
                        c.DateRequested = Convert.ToDateTime(sdr["DateRequested"]);
                        c.DateReviewed = GetNullableDateTimeValueFromDbField(sdr["DateReviewed"]);
                        c.RequestStatus = Convert.ToInt32(sdr["RequestStatus"]);
                        c.ProjectFK = Convert.ToInt32(sdr["ProjectFK"]);
                        c.RequestStatus = Convert.ToInt32(sdr["RequestStatus"]);
                        CrList.Add(c);
                    }
                }


            }


            finally
            {

                if (!sdr.IsClosed) sdr.Close();
                if (com.Connection.State == ConnectionState.Open) com.Connection.Close();
                com.Dispose();



            }

            return CrList;

        }

        //Get Calculation Field List
        public static List<Calculation> GetAllCalculationFields()
        {

            List<Calculation> CalcList = new List<Calculation>();
            string ComText = "select * from dbo.Calculation order by ShortDesc";

            SqlCommand com = new SqlCommand(ComText, GetConnection());
            SqlDataReader sdr = null;

            try
            {

                com.Connection.Open();
                sdr = com.ExecuteReader();

                while (sdr.Read())
                {

                    if (sdr.HasRows)
                    {

                        Calculation c = new Calculation();
                        c.ID = Convert.ToInt32(sdr["ID"]);
                        c.ShortDesc = sdr["ShortDesc"].ToString();
                        c.LongDesc = sdr["LongDesc"].ToString() ?? "N/A";
                        c.Diff = sdr["Diff"].ToString();
                        c.IncWeekends = Convert.ToInt32(sdr["IncWeekends"]);
                        c.IncHolidays = Convert.ToInt32(sdr["IncHolidays"]);
                        CalcList.Add(c);
                    }
                }


            }



            finally
            {
                if (!sdr.IsClosed) sdr.Close();
                if (com.Connection.State == ConnectionState.Open) com.Connection.Close();
                com.Dispose();


            }

            return CalcList;

        }


        public static List<Department> GetAllDepartments()
        {

            List<Department> DepList = new List<Department>();

            string ComText = "select * from dbo.Department";
            SqlCommand com = new SqlCommand(ComText, GetConnection());
            SqlDataReader sdr = null;

            try
            {
                com.Connection.Open();
                sdr = com.ExecuteReader();

                while (sdr.Read())
                {

                    if (sdr.HasRows)
                    {

                        Department d = new Department();
                        d.ID = Convert.ToInt32(sdr["ID"]);
                        d.Description = sdr["Description"].ToString();
                        DepList.Add(d);
                    }
                }


            }


            finally
            {
                if (!sdr.IsClosed) sdr.Close();
                if (com.Connection.State == ConnectionState.Open) com.Connection.Close();
                com.Dispose();


            }


            return DepList;


        }

        //Group 2 dept 2 pubcode

        public static List<DeptToGroupsToPubCode> GetAllGroupsToDeptToPubCode()
        {

            string ComText = "select * from dbo.DeptToGroupsToPubCode";

            List<DeptToGroupsToPubCode> ItemList = new List<DeptToGroupsToPubCode>();

            SqlCommand com = new SqlCommand(ComText, GetConnection());
            SqlDataReader sdr = null;

            try
            {
                com.Connection.Open();
                sdr = com.ExecuteReader();

                while (sdr.Read())
                {

                    if (sdr.HasRows)
                    {

                        DeptToGroupsToPubCode item = new DeptToGroupsToPubCode();
                        item.ID = Convert.ToInt32(sdr["ID"]);
                        item.DeptID = Convert.ToInt32(sdr["DeptFK"]);
                        item.GroupID = Convert.ToInt32(sdr["GroupFK"]);
                        item.PubCodeID = Convert.ToInt32(sdr["PubCodeFK"]);
                        ItemList.Add(item);
                    }
                }


            }


            finally
            {
                if (!sdr.IsClosed) sdr.Close();
                if (com.Connection.State == ConnectionState.Open) com.Connection.Close();
                com.Dispose();


            }

            return ItemList;

        }


        /*Get All Groups */


        public static List<Group> GetAllGroups()
        {
            List<Group> GroupList = new List<Group>();

            string ComText = "select * from [dbo].[Group]";
            SqlCommand com = new SqlCommand(ComText, GetConnection());
            SqlDataReader sdr = null;

            try
            {
                com.Connection.Open();
                sdr = com.ExecuteReader();

                while (sdr.Read())
                {

                    if (sdr.HasRows)
                    {

                        Group g = new Group();
                        g.ID = Convert.ToInt32(sdr["ID"]);
                        g.Description = sdr["Description"].ToString();
                        GroupList.Add(g);
                    }
                }


            }


            finally
            {
                if (!sdr.IsClosed) sdr.Close();
                if (com.Connection.State == ConnectionState.Open) com.Connection.Close();
                com.Dispose();


            }


            return GroupList;


        }


        /*Get All Messaging Actions */

        public static List<MessagingAction> GetAllMessagingActions()
        {
            List<MessagingAction> ActionList = new List<MessagingAction>();

            string ComText = "select * from dbo.MessagingAction";
            SqlCommand com = new SqlCommand(ComText, GetConnection());
            SqlDataReader sdr = null;

            try
            {
                com.Connection.Open();
                sdr = com.ExecuteReader();

                while (sdr.Read())
                {

                    if (sdr.HasRows)
                    {

                        MessagingAction ma = new MessagingAction();
                        ma.ID = Convert.ToInt32(sdr["ID"]);
                        ma.ShortDesc = sdr["ShortDesc"].ToString();
                        ma.LongDesc = sdr["LongDesc"].ToString();
                        ActionList.Add(ma);
                    }
                }


            }




            finally
            {
                if (!sdr.IsClosed) sdr.Close();
                if (com.Connection.State == ConnectionState.Open) com.Connection.Close();
                com.Dispose();


            }


            return ActionList;

        }


        /*Get All Messaging Events*/


        public static List<MessagingEvent> GetAllMessagingEvents()
        {
            List<MessagingEvent> EventList = new List<MessagingEvent>();

            string ComText = "select * from dbo.MessagingEvent";
            SqlCommand com = new SqlCommand(ComText, GetConnection());
            SqlDataReader sdr = null;


            try
            {
                com.Connection.Open();
                sdr = com.ExecuteReader();

                while (sdr.Read())
                {

                    if (sdr.HasRows)
                    {

                        MessagingEvent me = new MessagingEvent();
                        me.ID = Convert.ToInt32(sdr["ID"]);
                        me.ShortDesc = sdr["ShortDesc"].ToString();
                        me.LongDesc = sdr["LongDesc"].ToString();
                        me.Method = sdr["Method"].ToString();
                        EventList.Add(me);
                    }
                }


            }


            finally
            {
                if (!sdr.IsClosed) sdr.Close();
                if (com.Connection.State == ConnectionState.Open) com.Connection.Close();
                com.Dispose();


            }


            return EventList;

        }


        /* Get All Messaging Settings */

        public static List<MessagingSetting> GetAllMessagingSettings()
        {

            List<MessagingSetting> MsList = new List<MessagingSetting>();
            string ComText = "select * from dbo.MessagingSettings";
            SqlCommand com = new SqlCommand(ComText, GetConnection());
            SqlDataReader sdr = null;
            try
            {
                com.Connection.Open();
                sdr = com.ExecuteReader();

                while (sdr.Read())
                {

                    if (sdr.HasRows)
                    {

                        MessagingSetting ms = new MessagingSetting();
                        ms.ID = Convert.ToInt32(sdr["ID"]);
                        ms.EventFK = Convert.ToInt32(sdr["EventFK"]);
                        ms.ActionFk = Convert.ToInt32(sdr["ActionFK"]);
                        ms.RoleFk = GetNullableIntValueFromDbField(sdr["RoleFK"]);
                        ms.UserFK = GetNullableIntValueFromDbField(sdr["UserFK"]);
                        ms.GroupFK = GetNullableIntValueFromDbField(sdr["GroupFK"]);
                        ms.DeptFK = GetNullableIntValueFromDbField(sdr["DeptFK"]);
                        MsList.Add(ms);
                    }
                }


            }



            finally
            {
                if (!sdr.IsClosed) sdr.Close();
                if (com.Connection.State == ConnectionState.Open) com.Connection.Close();
                com.Dispose();


            }


            return MsList;




        }

        /*Get All Miletone Fields*/



        public static List<MilestoneField> GetAllMilestoneFields()
        {
            List<MilestoneField> MfList = new List<MilestoneField>();
            string ComText = "select * from dbo.MilestoneField  order by Description";
            SqlCommand com = new SqlCommand(ComText, GetConnection());
            SqlDataReader sdr = null;
            try
            {
                com.Connection.Open();
                sdr = com.ExecuteReader();

                while (sdr.Read())
                {

                    if (sdr.HasRows)
                    {

                        MilestoneField mf = new MilestoneField();
                        mf.ID = Convert.ToInt32(sdr["ID"]);
                        mf.Description = sdr["Description"].ToString();
                        mf.IsCreatedByUser = GetNullableIntValueFromDbField(sdr["IsCreatedByUser"]);
                        MfList.Add(mf);
                    }
                }


            }


            finally
            {
                if (!sdr.IsClosed) sdr.Close();
                if (com.Connection.State == ConnectionState.Open) com.Connection.Close();
                com.Dispose();


            }


            return MfList;

        }


        /*Get Parent Milestone Field List*/


        public static List<MilestoneField> GetAllParentMilestoneFields()
        {
            List<MilestoneField> MfList = new List<MilestoneField>();
            string ComText = "select * from dbo.MilestoneField where IsCreatedByUser is null or IsCreatedByUser =0 order by Description";
            SqlCommand com = new SqlCommand(ComText, GetConnection());
            SqlDataReader sdr = null;
            try
            {
                com.Connection.Open();
                sdr = com.ExecuteReader();

                while (sdr.Read())
                {

                    if (sdr.HasRows)
                    {

                        MilestoneField mf = new MilestoneField();
                        mf.ID = Convert.ToInt32(sdr["ID"]);
                        mf.Description = sdr["Description"].ToString();
                        MfList.Add(mf);
                    }
                }


            }


            finally
            {
                if (!sdr.IsClosed) sdr.Close();
                if (com.Connection.State == ConnectionState.Open) com.Connection.Close();
                com.Dispose();


            }


            return MfList;




        }

        //-----------------------------------------------------------------------------------Project Creation Region
        public static string GetTimelineFromID(int i)
        {
            string CommandText = string.Format("select LongDesc from dbo.ProjectRange where ID={0}", i);
            string rs = ExecuteScalarWrapper(CommandText).ToString();
            return rs;
        }

        //for now return the shortdesc
        public static string GetPubCodeShortDescFromID(int i)
        {
            string CommandText = string.Format("select ShortDesc from dbo.PublicationCode where ID={0}", i);
            string rs = ExecuteScalarWrapper(CommandText).ToString();
            return rs;
        }

        public static string GetPubCodeReportDescFromID(int i)
        {
            string CommandText = string.Format("select ReportDesc from dbo.PublicationCode where ID={0}", i);
            string rs = ExecuteScalarWrapper(CommandText).ToString();
            return rs;

        }

        public static string GetProductDescFromID(int i)
        {

            string CommandText = string.Format("select Description from dbo.Product where ID={0}", i);
            string rs = ExecuteScalarWrapper(CommandText).ToString();
            return rs;
        }

        public static string GetProfileDescFromID(int i)
        {

            string CommandText = string.Format("select Description from dbo.MilestoneTreeSettingsProfile where ID={0}", i);
            string rs = ExecuteScalarWrapper(CommandText).ToString();
            return rs;

        }


        public static int GetSecondaryMilestoneFieldIDLinkSettingByProjectID(int ProjectID)
        {

            int SecProfType = GetProfileTypeIDFromProjectID(ProjectID);
            int SecMilestoneID = GetAllProjectLinkSettings().Where(x => x.SecondaryProfileTypeID == SecProfType).ToList().First().SecondaryMilestoneID;
            return SecMilestoneID;

        }

        public static int GetProfileTypeIDFromProjectID(int ProjectID)
        {

            int CurrentProfileID = (int)GetAllProjects().Where(x => x.ID == ProjectID).First().MilestoneTreeSettingsProfileFK;
            int ProfileTypeID = GetAllMilestoneTreeSettingsProfiles().Where(x => x.ID == CurrentProfileID).First().ProjectType;
            return ProfileTypeID;
        }

        public static string GetProfileTypeNameFromProjectID(int ProjectID)
        {
            int CurrentProfileID = (int)GetAllProjects().Where(x => x.ID == ProjectID).First().MilestoneTreeSettingsProfileFK;
            int ProfileType = GetAllMilestoneTreeSettingsProfiles().Where(x => x.ID == CurrentProfileID).First().ProjectType;
            string ProfileTypeName = GetAllProjectProfileTypes().Where(x => x.ID == ProfileType).First().Description;
            return ProfileTypeName;
        }

        public static int GetMiletoneFieldIDSettingByProjectID(int ProjectID)
        {
            int CurrentProfileID = (int)GetAllProjects().Where(x => x.ID == ProjectID).First().MilestoneTreeSettingsProfileFK;
            int ProfileType = GetAllMilestoneTreeSettingsProfiles().Where(x => x.ID == CurrentProfileID).First().ProjectType;
            int ProfileTypeID = GetAllProjectProfileTypes().Where(x => x.ID == ProfileType).First().ID;

            int MilestoneFieldID = GetAllProjectLinkSettings().Where(x => x.SecondaryProfileTypeID == ProfileTypeID).First().PrimaryMilestoneID;
            return MilestoneFieldID;

        }

        public static int GetPrimaryProjectTypeProfileIDBasedOnPrimaryProjectID(int ProjectID)
        {

            int CurrentProfileID = (int)GetAllProjects().Where(x => x.ID == ProjectID).First().MilestoneTreeSettingsProfileFK;
            int ProfileType = GetAllMilestoneTreeSettingsProfiles().Where(x => x.ID == CurrentProfileID).First().ProjectType;
            return ProfileType;
        }

        public static List<ProjectDisplay> GetReviewableMagazineTypeProjects()
        {
            int MagazineTypeID = Convert.ToInt32(ConfigurationManager.AppSettings["MagazineProjectType"]);
            int OnSaleDateStatusCode = Convert.ToInt32(ConfigurationManager.AppSettings["OnSaleDateInReviewStatusCode"]);

            string ComText = string.Format("select * from dbo.project where CurrentProjectStatus={0} and MilestoneTreeSettingsProfileFK in (select ID from MilestoneTreeSettingsProfile where ProfileTypeFK={1})", OnSaleDateStatusCode, MagazineTypeID);
            SqlCommand com = new SqlCommand(ComText, GetConnection());
            SqlDataReader sdr = null;
            List<ProjectDisplay> ProjList = new List<ProjectDisplay>();
            try
            {
                com.Connection.Open();
                sdr = com.ExecuteReader();

                while (sdr.Read())
                {

                    if (sdr.HasRows)
                    {
                        ProjectDisplay p = new ProjectDisplay();
                        p.ID = Convert.ToInt32(sdr["ID"]);
                        p.PubCodeFK = GetNullableIntValueFromDbField(sdr["PubCodeFK"]);
                        p.ProductFK = GetNullableIntValueFromDbField(sdr["ProductFK"]);
                        p.YearFK = Convert.ToInt32(sdr["YearFK"]);
                        p.MilestoneTreeSettingsProfileFK = GetNullableIntValueFromDbField(sdr["MilestoneTreeSettingsProfileFK"]);
                        p.CurrentVersion = Convert.ToInt32(sdr["CurrentVersion"]);
                        p.Comments = sdr["Comments"].ToString();
                        p.DateCreated = Convert.ToDateTime(sdr["DateCreated"]);
                        p.CreatedBy = sdr["CreatedBy"].ToString();
                        p.Name = sdr["Name"].ToString();
                        p.ProjectRangeFK = GetNullableIntValueFromDbField(sdr["ProjectRangeFK"]);
                        p.CurrentProjectStatus = Convert.ToInt32(sdr["CurrentProjectStatus"]);
                        p.CurrentVersion = Convert.ToInt32(sdr["CurrentVersion"]);
                        ProjList.Add(p);
                    }


                }


            }



            finally
            {
                if (!sdr.IsClosed) sdr.Close();
                if (com.Connection.State == ConnectionState.Open) com.Connection.Close();
                com.Dispose();


            }

            return ProjList;

        }

        public static bool ArePendingChangeRequestsAvailableByProjectID(int ProjectID)
        {

            bool RetBool = false;
            int CreatedStatus = (int)Scheduling.Enums.ChangeRequestStatus.Created;
            RetBool = Scheduling.Database.Utility.GetAllChangeRequests().Where(x => x.ProjectFK == ProjectID && x.RequestStatus == CreatedStatus).Count() > 0;
            return RetBool;


        }

        public static List<ProjectLinkSetting> GetProjectLinkSettingsByProjectID(int ProjectID)
        {
            ProjectDisplay pd = Scheduling.Database.Utility.GetAllProjects().Where(x => x.ID == ProjectID).First();
            int CurrentProfileID = (int)pd.MilestoneTreeSettingsProfileFK;
            int CurrentProfileType = Scheduling.Database.Utility.GetAllMilestoneTreeSettingsProfiles().Where(x => x.ID == CurrentProfileID).First().ProjectType;

            List<ProjectLinkSetting> ProjLinkList = Scheduling.Database.Utility.GetAllProjectLinkSettings().Where(x => x.SecondaryProfileTypeID == CurrentProfileType).ToList();
            return ProjLinkList;
        }

        public static bool AreLinkableProjectsAvailableByProjectID(int ProjectID)
        {
            bool RetBool = false;

            List<ProjectLinkSetting> ProjLinkList = Scheduling.Database.Utility.GetProjectLinkSettingsByProjectID(ProjectID);

            if (ProjLinkList.Count > 0)
            {
                List<int> SettingMilestoneFieldList = ProjLinkList.Select(x => x.SecondaryMilestoneID).Distinct().ToList();
                int Count = GetMilestoneValuesByProjectID(ProjectID).Where(p => p.ParentID == null).Where(x => SettingMilestoneFieldList.Contains(x.MilestoneFieldFK)).Count();
                if (Count > 0) RetBool = true;
            }

            return RetBool;

        }



        public static string GetProjectNoteLabelBasedOnID(int i)
        {
            List<ProjectNoteLabel> LabelList = GetAllProjectNoteLabels();
            string RetStr = LabelList.Where(x => x.ID == i).First().ShortDesc;
            return RetStr;
        }

        public static List<ProjectNoteLabel> GetAllProjectNoteLabels()
        {

            List<ProjectNoteLabel> LabelList = new List<ProjectNoteLabel>();

            string ComText = string.Format("select * from dbo.ProjectNoteLabel where IsActive=1");
            SqlCommand com = new SqlCommand(ComText, GetConnection());
            SqlDataReader sdr = null;

            try
            {
                com.Connection.Open();
                sdr = com.ExecuteReader();

                while (sdr.Read())
                {

                    if (sdr.HasRows)
                    {
                        ProjectNoteLabel pnl = new ProjectNoteLabel();
                        pnl.ID = Convert.ToInt32(sdr["ID"]);
                        pnl.ShortDesc = sdr["ShortDesc"].ToString();
                        pnl.LongDesc = sdr["LongDesc"].ToString();
                        pnl.IsActive = Convert.ToBoolean(sdr["IsActive"]);
                        LabelList.Add(pnl);
                    }


                }


            }

            catch
            {


            }


            finally
            {
                if (!sdr.IsClosed) sdr.Close();
                if (com.Connection.State == ConnectionState.Open) com.Connection.Close();
                com.Dispose();


            }

            return LabelList;

        }

        public static string GetProjectStatusDescByProjectStatusID(int StatusID)
        {

            return GetAllProjectStatuses().Where(x => x.ID == StatusID).First().Description;
        }

        public static List<ChangeRequest> GetProjectChangeRequestsFromProjectID(int ProjectEntry)
        {

            string ComText = string.Format("select * from dbo.ProjectChangeRequest where ProjectFk={0}", ProjectEntry);
            SqlCommand com = new SqlCommand(ComText, GetConnection());
            SqlDataReader sdr = null;
            List<ChangeRequest> CrList = new List<ChangeRequest>();
            try
            {
                com.Connection.Open();
                sdr = com.ExecuteReader();

                while (sdr.Read())
                {

                    if (sdr.HasRows)
                    {
                        ChangeRequest cr = new ChangeRequest();
                        cr.ID = Convert.ToInt32(sdr["ID"]);
                        cr.RequestorComment = sdr["RequestorComment"].ToString();
                        cr.DateRequested = Convert.ToDateTime(sdr["DateRequested"]);
                        cr.CreatedBy = sdr["CreatedBy"].ToString();
                        cr.ProjectFK = Convert.ToInt32(sdr["ProjectFK"]);
                        cr.RequestStatus = Convert.ToInt32(sdr["RequestStatus"]);
                        CrList.Add(cr);
                    }


                }


            }


            finally
            {
                if (!sdr.IsClosed) sdr.Close();
                if (com.Connection.State == ConnectionState.Open) com.Connection.Close();
                com.Dispose();


            }

            return CrList;

        }

        public static ProjectDisplay GetCurrentProjectByProjectID(int ProjectID)
        {
            return GetAllProjects().Where(x => x.ID == ProjectID).First();

        }

        public static List<ProjectDisplay> GetProjectsWithOnSaleDateApprovedStatus()
        {
            int ApprovedStatusID = Convert.ToInt32(Scheduling.StringFunctions.Utility.GetAppSettingValue("ProjectStatusApprovedOnSaleID"));
            return Scheduling.Database.Utility.GetAllProjects().Where(x => x.CurrentProjectStatus == ApprovedStatusID).ToList();

        }

        public static List<ProjectDisplay> GetProjectsWithStatusOfCreated()
        {
            int CreatedStatus = (int)Scheduling.Enums.ProjectStatus.Created;
            List<ProjectDisplay> PdList = Scheduling.Database.Utility.GetAllProjects().Where(x => x.CurrentProjectStatus == CreatedStatus).OrderByDescending(x => x.DateCreated).ToList();
            return PdList;
        }

        public static List<ProjectDisplay> GetProjectsBasedOnBaselineType(int i)
        {

            string ComText = string.Format("select * from dbo.project where milestonetreesettingsprofilefk in (select id from milestonetreesettingsprofile where profiletypefk={0})", i);

            SqlCommand com = new SqlCommand(ComText, GetConnection());
            SqlDataReader sdr = null;
            List<ProjectDisplay> ProjList = new List<ProjectDisplay>();
            try
            {
                com.Connection.Open();
                sdr = com.ExecuteReader();

                while (sdr.Read())
                {

                    if (sdr.HasRows)
                    {
                        ProjectDisplay p = new ProjectDisplay();
                        p.ID = Convert.ToInt32(sdr["ID"]);
                        p.PubCodeFK = GetNullableIntValueFromDbField(sdr["PubCodeFK"]);
                        p.ProductFK = GetNullableIntValueFromDbField(sdr["ProductFK"]);
                        p.YearFK = Convert.ToInt32(sdr["YearFK"]);
                        p.MilestoneTreeSettingsProfileFK = GetNullableIntValueFromDbField(sdr["MilestoneTreeSettingsProfileFK"]);
                        p.CurrentVersion = Convert.ToInt32(sdr["CurrentVersion"]);
                        p.Comments = sdr["Comments"].ToString();
                        p.DateCreated = Convert.ToDateTime(sdr["DateCreated"]);
                        p.CreatedBy = sdr["CreatedBy"].ToString();
                        p.Name = sdr["Name"].ToString();
                        p.ProjectRangeFK = GetNullableIntValueFromDbField(sdr["ProjectRangeFK"]);
                        p.CurrentProjectStatus = Convert.ToInt32(sdr["CurrentProjectStatus"]);
                        p.CurrentVersion = Convert.ToInt32(sdr["CurrentVersion"]);
                        p.IsLocked = GetNullableIntValueFromDbField(sdr["IsLocked"]);
                        ProjList.Add(p);
                    }


                }


            }

            finally
            {
                if (!sdr.IsClosed) sdr.Close();
                if (com.Connection.State == ConnectionState.Open) com.Connection.Close();
                com.Dispose();


            }

            return ProjList;

        }

        public static List<ReportProjectDisplayViewModelSection> GenerateReportProjectDisplayViewModelSectionListing(List<ProjectDisplay> PdList)
        {
            List<ReportProjectDisplayViewModelSection> RetList = new List<ReportProjectDisplayViewModelSection>();
          

            //get newsstand id 
            int NewsstandID = Convert.ToInt32(Scheduling.StringFunctions.Utility.GetAppSettingValue("NewsstandOnSaleDateMilestoneValue"));

            foreach (ProjectDisplay pd in PdList)
            {
                ReportProjectDisplayViewModelSection sec = new ReportProjectDisplayViewModelSection();
                sec.SectionHeader = new ProjectDisplaySectionHeader();
                sec.TopRecords = new List<MilestoneDisplayRecord>();
                sec.SubItemRows = new List<SubItemRow>();

                //MilestoneValueWithoutParents
                List<Scheduling.Models.MilestoneValue> MilestoneValueWithoutParents = Scheduling.Database.Utility.GetMilestoneValuesByProjectID(pd.ID).Where(p => p.ParentID == null).OrderByDescending(x => x.DisplaySortOrder).ToList();

                foreach (Scheduling.Models.MilestoneValue TopItem in MilestoneValueWithoutParents)
                {
                    string TopValue = Scheduling.Database.Utility.GetAllMilestoneFields().Find(x => x.ID == TopItem.MilestoneFieldFK).Description;
                    if (TopItem.MilestoneFieldFK == NewsstandID)
                    {
                        TopValue = Scheduling.Database.Utility.GetAliasByPubCodeAndMilestoneField(pd.PubCodeFK, TopItem.MilestoneFieldFK, TopValue);
                    }

                    MilestoneDisplayRecord mdr = new MilestoneDisplayRecord();
                    mdr.Description = TopValue;
                    mdr.CssClass = string.Format("top-item top-item{0}", TopItem.MilestoneFieldFK);
                    mdr.Value = string.Empty;

                    if (!string.IsNullOrWhiteSpace(TopItem.DueDate))
                    {
                        string[] sa = TopItem.DueDate.Split('/');

                        if (TopItem.MilestoneFieldFK == Convert.ToInt32(Scheduling.StringFunctions.Utility.GetAppSettingValue("NewsstandOnSaleDateMilestoneValue")))
                        {
                            mdr.Value = string.Format("{0}/{1}/{2}", sa[0], sa[1], sa[2]);
                        }

                        else
                        {
                            mdr.Value = string.Format("{0}/{1}", sa[0], sa[1]);
                        }

                    }

                    sec.TopRecords.Add(mdr);

                }

                //end


                //milestone values with parents start

                //sub items
                List<string> DescList = Scheduling.Html.ReportingUtilities.GetDistinctSubItemDescriptions(pd.ID);

                //sort if necessary

                string SortSubItems = System.Configuration.ConfigurationManager.AppSettings["SortSubItemsForReporting"];

                //start sub item logic
                if (SortSubItems.ToUpper() == "TRUE")
                {
                    DescList = Scheduling.Sorting.DisplaySorting.GetSortedDisplaySequenceFromDbBasedOnPubCodeAndTimeline(pd);

                }
                //end

                foreach (string DescItem in DescList)
                {

                    SubItemRow sir = new SubItemRow();
                    sir.Description = DescItem;

                    sir.SubItemDates = new List<string>();

                    foreach (Scheduling.Models.MilestoneValue mv in MilestoneValueWithoutParents)

                    {

                        string insert = Scheduling.Html.ReportingUtilities.GetDateBasedOnSubItemDescAndParentIDForSpecificProject(DescItem, pd.ID, mv.MilestoneFieldFK);
                        sir.SubItemDates.Add(insert);
                        
                    }

                    sec.SubItemRows.Add(sir);

                }

                
                //end

                sec.SectionHeader.NotesAreAvailable = false;

                List<Scheduling.Models.ProjectNote> CurrentNotes = Scheduling.Database.Utility.GetProjectNotesFromProjectID(pd.ID);
                if (CurrentNotes.Count > 0)
                {
                    sec.SectionHeader.NotesAreAvailable = true;

                    if (CurrentNotes.Where(x => x.NoteLabelID == (int)Scheduling.Enums.MagazineNoteLabel.KPCJobNumber).Count() > 0)
                    {
                        sec.SectionHeader.KPCProjectNumber = CurrentNotes.Where(x => x.NoteLabelID == (int)Scheduling.Enums.MagazineNoteLabel.KPCJobNumber).First().NoteValue;

                    }

                    if (CurrentNotes.Where(x => x.NoteLabelID == (int)Scheduling.Enums.MagazineNoteLabel.QuadJobNumber).Count() > 0)
                    {
                        sec.SectionHeader.QuadJobNumber = CurrentNotes.Where(x => x.NoteLabelID == (int)Scheduling.Enums.MagazineNoteLabel.QuadJobNumber).First().NoteValue;

                    }

                    if (CurrentNotes.Where(x => x.NoteLabelID == (int)Scheduling.Enums.MagazineNoteLabel.NextIssueAndCrossPromo).Count() > 0)
                    {
                        sec.SectionHeader.NextIssueCrossPromoNumber = CurrentNotes.Where(x => x.NoteLabelID == (int)Scheduling.Enums.MagazineNoteLabel.NextIssueAndCrossPromo).First().NoteValue;

                    }



                }
                sec.SectionHeader.YearMonthHeader = Scheduling.Html.ReportingUtilities.GenerateMonthYearHeaderForSingleProject(pd);
                sec.SectionHeader.RevisedHeader = DateTime.Now.ToShortDateString();
                sec.SectionHeader.PubcodeHeader = Scheduling.Database.Utility.GetAllPublicationCodes().Where(x => x.ID == pd.PubCodeFK).First().ShortDesc;


                RetList.Add(sec);





            }
            return RetList;
        }

        public static string GeneratePubCodeSummaryReportHeader(List<int?> UniquePubCodes)
        {
            string PubHeader = string.Empty;
            foreach (int? i in UniquePubCodes)
            {
                if (i.HasValue)
                {
                    PubHeader += string.Format("...{0}", Scheduling.Database.Utility.GetPubCodeShortDescFromID(i.Value));

                }
            }

            return PubHeader;
        }


        public static List<ProjectDisplay> GetDistinctProjectsBasedOnYearAndTimelineMonthRangeOnly(string StartDate, string EndDate, string PubCodeStr)
        {
            List<ProjectDisplay> RetList = new List<ProjectDisplay>();

            string[] StartArray = StartDate.Split('-');
            string[] StopArray = EndDate.Split('-');

            //returns 0 if not month enum at the moment.
            int StartMonth = Scheduling.Mapping.Utility.GetMonthlyTimelineIDFromMonthlyDatepicker(StartArray[1]);
            int StopMonth = Scheduling.Mapping.Utility.GetMonthlyTimelineIDFromMonthlyDatepicker(StopArray[1]);


            int PassedInStartYear = Convert.ToInt32(StartArray[0]);
            int PassedInStopYear = Convert.ToInt32(StopArray[0]);


            bool ValidStartYear = Scheduling.Database.Utility.GetAllYears().Where(x => x.Value == Convert.ToInt32(StartArray[0])).Count() > 0;
            bool ValidStopYear = Scheduling.Database.Utility.GetAllYears().Where(x => x.Value == Convert.ToInt32(StopArray[0])).Count() > 0;

            if (ValidStartYear && ValidStopYear && StartMonth != 0 && StopMonth != 0)
            {

                string ComText = string.Empty;
                int StartYear = Scheduling.Database.Utility.GetAllYears().Where(x => x.Value == Convert.ToInt32(StartArray[0])).First().ID;
                int StopYear = Scheduling.Database.Utility.GetAllYears().Where(x => x.Value == Convert.ToInt32(StopArray[0])).First().ID;

                //3 scenarios..same year,two different ,more than 2 different

                int diff = StopYear - StartYear;

                if (diff == 0)
                {
                    RetList = GetAllProjects().Where(x => x.YearFK == StartYear).Where(x => x.ProjectRangeSortOrder >= StartMonth).Where(x => x.ProjectRangeSortOrder <= StopMonth).Distinct().ToList();

                }

                if (diff == 1)
                {

                    IEnumerable<ProjectDisplay> FirstYear = GetAllProjects().Where(x => x.ProjectRangeSortOrder >= StartMonth && x.YearFK == StartYear);
                    IEnumerable<ProjectDisplay> SecondYear = GetAllProjects().Where(x => x.ProjectRangeSortOrder <= StopMonth && x.YearFK == StopYear);
                    RetList = FirstYear.Union(SecondYear).ToList();
                }


                //linq behaves funky use raw sql.
                if (diff > 1)
                {

                    IEnumerable<ProjectDisplay> FirstYear = new List<ProjectDisplay>();
                    IEnumerable<ProjectDisplay> LastYear = new List<ProjectDisplay>();

                    if (!PubCodeStr.Contains(',') && !string.IsNullOrEmpty(PubCodeStr))
                    {
                        int PubCodeVal = Convert.ToInt32(PubCodeStr);

                        FirstYear = GetAllProjects().Where(x => x.ProjectRangeSortOrder >= StartMonth && x.YearFK == StartYear && x.PubCodeFK.Value == PubCodeVal);
                        LastYear = GetAllProjects().Where(x => x.ProjectRangeSortOrder <= StopMonth && x.YearFK == StopYear && x.PubCodeFK.Value == PubCodeVal);

                    }


                    else
                    {
                        FirstYear = GetAllProjects().Where(x => x.ProjectRangeSortOrder >= StartMonth && x.YearFK == StartYear);
                        LastYear = GetAllProjects().Where(x => x.ProjectRangeSortOrder <= StopMonth && x.YearFK == StopYear);

                    }


                    List<ProjectDisplay> ProjList = new List<ProjectDisplay>();


                    string SqlInStr = string.Empty;
                    for (int i = StartYear + 1; i == StopYear - 1; i++)
                    {
                        SqlInStr = i.ToString() + ",";

                    }

                    SqlInStr = SqlInStr.TrimEnd(',');

                    string SqlComText = string.Format("select * from dbo.project where yearfk in ({0})", SqlInStr);

                    SqlCommand com = new SqlCommand(SqlComText, GetConnection());
                    SqlDataReader sdr = null;
                    //start

                    try
                    {
                        com.Connection.Open();
                        sdr = com.ExecuteReader();

                        while (sdr.Read())
                        {

                            if (sdr.HasRows)
                            {
                                ProjectDisplay p = new ProjectDisplay();
                                p.ID = Convert.ToInt32(sdr["ID"]);
                                p.PubCodeFK = GetNullableIntValueFromDbField(sdr["PubCodeFK"]);
                                p.ProductFK = GetNullableIntValueFromDbField(sdr["ProductFK"]);
                                p.YearFK = Convert.ToInt32(sdr["YearFK"]);
                                p.MilestoneTreeSettingsProfileFK = GetNullableIntValueFromDbField(sdr["MilestoneTreeSettingsProfileFK"]);
                                p.CurrentVersion = Convert.ToInt32(sdr["CurrentVersion"]);
                                p.Comments = sdr["Comments"].ToString();
                                p.DateCreated = Convert.ToDateTime(sdr["DateCreated"]);
                                p.CreatedBy = sdr["CreatedBy"].ToString();
                                p.Name = sdr["Name"].ToString();
                                p.ProjectRangeFK = GetNullableIntValueFromDbField(sdr["ProjectRangeFK"]);
                                p.CurrentProjectStatus = Convert.ToInt32(sdr["CurrentProjectStatus"]);
                                p.CurrentVersion = Convert.ToInt32(sdr["CurrentVersion"]);
                                p.IsLocked = GetNullableIntValueFromDbField(sdr["IsLocked"]);
                                p.ProjectRangeSortOrder = GetProjectRangeDisplayOrderFromProjectRangeID(p.ProjectRangeFK);
                                ProjList.Add(p);
                            }


                        }


                    }

                    catch (Exception e)
                    {
                        Scheduling.Database.Utility.CreateApplicationErrorLoggingEntry(e.Message);
                    }

                    finally
                    {

                        if (!sdr.IsClosed) sdr.Close();
                        if (com.Connection.State == ConnectionState.Open) com.Connection.Close();
                        com.Dispose();

                    }

                    if (!PubCodeStr.Contains(',') && !string.IsNullOrEmpty(PubCodeStr))
                    {

                        ProjList = ProjList.Where(x => x.PubCodeFK == Convert.ToInt32(PubCodeStr)).ToList();
                    }

                    //stop
                    RetList = FirstYear.Concat(LastYear).Concat(ProjList).ToList();

                }
            }

            else
            {

                int MinStartYearValue = Scheduling.Database.Utility.GetAllYears().Select(x => x.Value).Min();
                int MaxStopYearValue = Scheduling.Database.Utility.GetAllYears().Select(x => x.Value).Max();

                int MinStartYearID = Scheduling.Database.Utility.GetAllYears().Select(x => x.ID).Min();
                int MaxStopYearID = Scheduling.Database.Utility.GetAllYears().Select(x => x.ID).Max();

                if (!ValidStartYear && !ValidStopYear)
                {
                    //if the start is less and the stop is more narrow it down.

                    if ((PassedInStartYear < MinStartYearValue) && (PassedInStopYear > MaxStopYearValue))
                    {

                        RetList = Scheduling.Database.Utility.GetAllProjects();
                    }


                }
                //This means a user can put a date way in the past
                if (!ValidStartYear && ValidStopYear)
                {

                    if (PassedInStartYear < MinStartYearValue)
                    {
                        string StartString = Scheduling.StringFunctions.Utility.GetAllProjectsMinimumStartDate();
                        return GetDistinctProjectsBasedOnYearAndTimelineMonthRangeOnly(StartString, EndDate, PubCodeStr);
                    }

                }
                //This allows user to put a date way in the future.
                if (ValidStartYear && !ValidStopYear)
                {
                    string LowestMonthStr = string.Empty;

                    if (PassedInStopYear > MaxStopYearValue)
                    {
                        string StartString = Scheduling.StringFunctions.Utility.GetAllProjectsMinimumStartDate();

                        List<ProjectDisplay> ExcludedList = GetDistinctProjectsBasedOnYearAndTimelineMonthRangeOnly(StartString, StartDate, PubCodeStr);

                        //We need to whack the last item and remove this set from the results.
                        if (ExcludedList.Count() > 1)
                        {
                            ExcludedList = ExcludedList.Take(ExcludedList.Count() - 1).ToList();
                        }

                        else
                        {
                            ExcludedList = new List<ProjectDisplay>();
                        }

                        //we need the second arg so we are comparing id's.The default comparer only relies on object references.
                        if (ExcludedList.Count() > 0)
                        {

                            IEnumerable<ProjectDisplay> TotalListEnum = Scheduling.Database.Utility.GetAllProjects().AsEnumerable<ProjectDisplay>();
                            IEnumerable<ProjectDisplay> ExcludedListEnum = ExcludedList.AsEnumerable<ProjectDisplay>();
                            RetList = TotalListEnum.Except(ExcludedListEnum, new ProjectComparer()).ToList();

                        }

                        else
                        {
                            RetList = Scheduling.Database.Utility.GetAllProjects();

                        }


                    }

                }


            }

            return RetList;

        }

        //3/12/14 Add ID property 
        public static List<ProjectNewstand> GetProjectNewstandDates()
        {

            int NewstandID = Convert.ToInt32(Scheduling.StringFunctions.Utility.GetAppSettingValue("NewsstandOnSaleDateMilestoneValue"));
            List<ProjectDisplay> PdList = Scheduling.Database.Utility.GetAllProjects();
            List<ProjectNewstand> PnList = new List<ProjectNewstand>();
            foreach (ProjectDisplay pd in PdList)
            {

                int MileCount = Scheduling.Database.Utility.GetMilestoneValuesByProjectID(pd.ID).Where(x => x.MilestoneFieldFK == NewstandID).Count();
                if (MileCount > 0)
                {
                    Scheduling.Models.MilestoneValue mv = Scheduling.Database.Utility.GetMilestoneValuesByProjectID(pd.ID).Where(x => x.MilestoneFieldFK == NewstandID).First();
                    string DueDate = Convert.ToDateTime(mv.DueDate).ToLongDateString();
                    string ProjectName = pd.Name;
                    ProjectNewstand pn = new ProjectNewstand();
                    pn.NewstandDate = DueDate;
                    pn.ProjectName = ProjectName;
                    pn.ProjectID = pd.ID;
                    PnList.Add(pn);
                }

            }

            return PnList;

        }

        public static List<int> GetProjectNoteKeyValues(int ProjectID)
        {
            List<int> RetList = new List<int>();
            int CurrentProfileID = GetProfileTypeIDFromProjectID(ProjectID);
            string ComText = string.Format("select ProjectNoteLabelFK from [dbo].[ProjectProfileTypeToNoteLabel] where ProfileTypeFk={0}", CurrentProfileID);

            SqlCommand com = new SqlCommand(ComText, GetConnection());
            SqlDataReader sdr = null;

            try
            {
                com.Connection.Open();
                sdr = com.ExecuteReader();

                while (sdr.Read())
                {

                    if (sdr.HasRows)
                    {
                        RetList.Add(Convert.ToInt32(sdr[0]));
                    }


                }



            }

            catch
            {


            }

            finally
            {
                if (!sdr.IsClosed) sdr.Close();
                if (com.Connection.State == ConnectionState.Open) com.Connection.Close();
                com.Dispose();


            }

            return RetList;

        }

        public static int GetLastProjectEntryIDInProjectHistory()
        {

            string ComText = "select id from dbo.project where id in(select top 1 projectfk from dbo.ProjectHistory order by entrydate desc)";
            int LastModifiedProjectID = Convert.ToInt32(ExecuteScalarWrapper(ComText));
            return LastModifiedProjectID;
        }

        public static int? GetPreviousProjectIDForManageProjectNav(int id)
        {

            ProjectDisplay pd = GetAllProjects().Where(x => x.ID == id).First();
            int CurrentYearFK = pd.YearFK;
            int? CurrentTimeline = pd.ProjectRangeFK;
            int? CurrentPubCode = pd.PubCodeFK;

            if (CurrentTimeline.HasValue && CurrentPubCode.HasValue)
            {
                List<ProjectDisplay> ProjectsWithSameYearAndPubCode = GetAllProjects().Where(x => x.YearFK == CurrentYearFK).Where(x => x.PubCodeFK == CurrentPubCode).ToList();
                int? MinForYear = ProjectsWithSameYearAndPubCode.Select(x => x.ProjectRangeFK).Distinct().Min();

                if (CurrentTimeline == MinForYear)
                {
                    return null;
                }

                else
                {

                    List<int?> OrderedTimeline = ProjectsWithSameYearAndPubCode.Where(x => x.ProjectRangeFK.HasValue).Select(x => x.ProjectRangeFK).OrderBy(x => x.Value).ToList();

                    int IndexOfCurrentTimeline = OrderedTimeline.IndexOf(CurrentTimeline);
                    int IndexForPrevious = IndexOfCurrentTimeline - 1;
                    int? DesiredTimeline = OrderedTimeline[IndexForPrevious];

                    int? RetValue = ProjectsWithSameYearAndPubCode.Where(x => x.ProjectRangeFK == DesiredTimeline).First().ID;
                    return RetValue;

                }
            }

            else
            {
                return null;

            }
        }

        public static int? GetNextProjectIDForManageProjectNav(int id)
        {
            ProjectDisplay pd = GetAllProjects().Where(x => x.ID == id).First();
            int CurrentYearFK = pd.YearFK;
            int? CurrentTimeline = pd.ProjectRangeFK;
            int? CurrentPubCode = pd.PubCodeFK;

            if (CurrentTimeline.HasValue && CurrentPubCode.HasValue)
            {
                List<ProjectDisplay> ProjectsWithSameYearAndPubCode = GetAllProjects().Where(x => x.YearFK == CurrentYearFK).Where(x => x.PubCodeFK == CurrentPubCode).ToList();
                int? MaxForYear = ProjectsWithSameYearAndPubCode.Select(x => x.ProjectRangeFK).Distinct().Max();

                if (CurrentTimeline == MaxForYear)
                {
                    return null;
                }

                else
                {
                    List<int?> OrderedTimeline = ProjectsWithSameYearAndPubCode.Where(x => x.ProjectRangeFK.HasValue).Select(x => x.ProjectRangeFK).OrderBy(x => x.Value).ToList();
                    int IndexOfCurrentTimeline = OrderedTimeline.IndexOf(CurrentTimeline);
                    int IndexForNext = IndexOfCurrentTimeline + 1;

                    int? DesiredTimeline = OrderedTimeline[IndexForNext];

                    int? RetValue = ProjectsWithSameYearAndPubCode.Where(x => x.ProjectRangeFK == DesiredTimeline).First().ID;
                    return RetValue;


                }
            }


            return null;

        }

        public static List<ProjectNote> GetProjectNotesFromProjectID(int ProjectEntry)
        {

            string ComText = string.Format("select * from dbo.ProjectNote where ProjectFk={0} order by EntryDate desc", ProjectEntry);
            SqlCommand com = new SqlCommand(ComText, GetConnection());
            SqlDataReader sdr = null;
            List<ProjectNote> NoteList = new List<ProjectNote>();
            try
            {
                com.Connection.Open();
                sdr = com.ExecuteReader();

                while (sdr.Read())
                {

                    if (sdr.HasRows)
                    {
                        ProjectNote pn = new ProjectNote();
                        pn.ID = Convert.ToInt32(sdr["ID"]);
                        pn.NoteLabelID = Convert.ToInt32(sdr["ProjectNoteLabelFK"]);
                        pn.ProjectFK = Convert.ToInt32(sdr["ProjectFK"]);
                        pn.EntryDate = Convert.ToDateTime(sdr["EntryDate"]).ToString();
                        pn.Username = sdr["Username"].ToString();
                        pn.NoteValue = sdr["NoteValue"].ToString();
                        NoteList.Add(pn);
                    }


                }


            }


            finally
            {
                if (!sdr.IsClosed) sdr.Close();
                if (com.Connection.State == ConnectionState.Open) com.Connection.Close();
                com.Dispose();


            }


            return NoteList;

        }


        public static List<ProjectHistory> GetProjectHistoryFromProjectID(int ProjectEntry)
        {

            string ComText = string.Format("select * from dbo.ProjectHistory where ProjectFk={0} order by EntryDate desc", ProjectEntry);
            SqlCommand com = new SqlCommand(ComText, GetConnection());
            SqlDataReader sdr = null;
            List<ProjectHistory> HisList = new List<ProjectHistory>();
            try
            {
                com.Connection.Open();
                sdr = com.ExecuteReader();

                while (sdr.Read())
                {

                    if (sdr.HasRows)
                    {
                        ProjectHistory ph = new ProjectHistory();
                        ph.EntryDate = Convert.ToDateTime(sdr["EntryDate"]).ToString();
                        ph.Comments = sdr["Comments"].ToString();
                        ph.Username = sdr["Username"].ToString();
                        HisList.Add(ph);
                    }


                }


            }



            finally
            {
                if (!sdr.IsClosed) sdr.Close();
                if (com.Connection.State == ConnectionState.Open) com.Connection.Close();
                com.Dispose();


            }


            return HisList;
        }

        public static int GetYearByID(int i)
        {
            string CommandText = string.Format("select Value from dbo.Year where ID={0}", i);
            int rs = Convert.ToInt32(ExecuteScalarWrapper(CommandText).ToString());
            return rs;
        }

        public static string GetMilestoneDescFromID(int i)
        {
            string CommandText = string.Format("select Description from dbo.MilestoneField where ID={0}", i);
            string rs = ExecuteScalarWrapper(CommandText).ToString();
            return rs;

        }

        public static string GetProjectSpecificDescWithParent(int ProjectID, int FieldID)
        {

            int? ParentVal = GetMilestoneValuesByProjectID(ProjectID).Where(x => x.MilestoneFieldFK == FieldID).First().ParentID;
            string RetStr = GetMilestoneDescFromID(FieldID);
            if (ParentVal.HasValue)
            {
                string ParentStr = GetMilestoneDescFromID((int)ParentVal);
                RetStr += string.Format(" ({0})", ParentStr);
            }

            return RetStr;
        }

        public static string GetTimelineShortDescFromID(int i)
        {
            string CommandText = string.Format("select ShortDesc from dbo.ProjectRange where ID={0}", i);
            string rs = ExecuteScalarWrapper(CommandText).ToString();
            return rs;

        }


        //-------------------------------------------------------General Functions-------------------------------------------------

        public static SqlConnection GetConnection()
        {

            string CurrentConStr = ConfigurationManager.ConnectionStrings["ProdScheduleDB"].ConnectionString;

            return new SqlConnection(CurrentConStr);

        }



        public static void ExecuteNonQueryWrapper(string CommandText)
        {
            SqlCommand com = new SqlCommand(CommandText, GetConnection());

            try
            {
                com.Connection.Open();
                com.ExecuteNonQuery();
            }


            finally
            {
                if (com.Connection.State == ConnectionState.Open) com.Connection.Close();
                com.Dispose();


            }

        }

        public static void ExecuteSecureCommandNonQueryWrapper(SqlCommand com)
        {

            com.Connection = GetConnection();

            try
            {
                com.Connection.Open();
                com.ExecuteNonQuery();
            }




            finally
            {
                if (com.Connection.State == ConnectionState.Open) com.Connection.Close();
                com.Dispose();
            }

        }


        public static object ExecuteScalarWrapper(string CommandText)
        {
            object o = null;
            SqlCommand com = new SqlCommand(CommandText, GetConnection());

            try
            {
                com.Connection.Open();
                o = com.ExecuteScalar();
            }


            finally
            {

                if (com.Connection.State == ConnectionState.Open) com.Connection.Close();
                com.Dispose();

            }

            return o;

        }

        public static bool VerifyNoExistingGroupAssociationEntry(int DeptID, int GroupID, int PubID)
        {
            int Count = GetAllGroupsToDeptToPubCode().Where(x => x.DeptID == DeptID && x.GroupID == GroupID && x.PubCodeID == PubID).ToList().Count;
            return Count == 0;
        }

        public static List<DupProjectEntry> GetAllDuplicateProjectEntries()
        {
            List<DupProjectEntry> DupEntries = new List<DupProjectEntry>();
            List<ProjectDisplay> CurrentProjectList = GetAllProjects().ToList();

            string ComText = "SELECT PubCodeFK,ProjectRangeFK,yearFk,COUNT(*) as count FROM [dbo].[Project] group by yearfk,PubCodeFK,ProjectRangeFK having COUNT(*) > 1";

            SqlCommand com = new SqlCommand(ComText, GetConnection());
            SqlDataReader sdr = null;




            try
            {
                com.Connection.Open();
                sdr = com.ExecuteReader();

                while (sdr.Read())
                {

                    if (sdr.HasRows)
                    {
                        DupProjectEntry de = new DupProjectEntry();
                        de.Count = Convert.ToInt32(sdr["count"]);
                        de.PubCodeFK = Convert.ToInt32(sdr["PubCodeFK"]);
                        de.ProjectRangeFK = Convert.ToInt32(sdr["ProjectRangeFK"]);
                        de.YearFK = Convert.ToInt32(sdr["YearFK"]);
                        DupEntries.Add(de);

                    }


                }


            }

            catch { }


            finally
            {
                if (!sdr.IsClosed) sdr.Close();
                if (com.Connection.State == ConnectionState.Open) com.Connection.Close();
                com.Dispose();


            }



            return DupEntries;
        }

        public static string VerifySingleMilestoneTreeProfileForNonMagazineType(int ProfileID)
        {
            string Message = string.Empty;

            List<MilestoneTreeSetting> SettingList = GetMilestoneTreeSettingsByProfileID(ProfileID).Where(x => x.CalcFiringOrder != null && x.MilestoneParentField == null).OrderBy(x => x.CalcFiringOrder).ToList();


            List<int> RunningList = new List<int>();

            foreach (MilestoneTreeSetting mts in SettingList)
            {
                string CurrentName = GetMilestoneDescFromID(mts.MilestoneField);

                //ignore first entry.

                if (RunningList.Count > 0)
                {

                    int? CurrentDependency = mts.MilestoneFieldDependantUpon;
                    if (!CurrentDependency.HasValue)
                    {
                        return string.Format("{0} field has no dependency", CurrentName);
                    }


                    int CurrentDependant = (int)mts.MilestoneFieldDependantUpon;
                    int Count = RunningList.Where(x => x == CurrentDependency).Count();
                    if (Count == 0)
                    {
                        string DepStr = GetMilestoneDescFromID((int)mts.MilestoneFieldDependantUpon);
                        string RetStr = string.Format("</br>The milestone field {0} has a dependency on {1 } that has NOT yet been calculated</br/>", CurrentName, DepStr);
                        Message += RetStr;
                    }

                }

                RunningList.Add(mts.MilestoneField);

            }

            return Message;

        }

        //Verify that main items(those without a parent) have a sequential calculation firing order.
        public static string VerifySingleMilestoneTreeProfileForMagazineType(int ProfileID)
        {
            string Message = string.Empty;

            bool IsMagProfileType = IsMagazineProfileTypeBasedOnSettingsProfile(ProfileID);
            // 12.11.14 Include Sub Items As Well as parent items ...
            List<MilestoneTreeSetting> SettingList = GetMilestoneTreeSettingsByProfileID(ProfileID).Where(x => x.CalcFiringOrder != null).OrderBy(x => x.CalcFiringOrder).ToList();

            if (SettingList.Count == 0) return "No Suitable Entries for This Baseline Profile";
            //verify that newsstand date is the first entry
            int NewsStandID = Convert.ToInt32(ConfigurationManager.AppSettings["NewsstandOnSaleDateMilestoneValue"]);

            bool ValidFirstEntry = false;

            int? FirstDependency = SettingList.First().MilestoneFieldDependantUpon;

            if (FirstDependency.HasValue)
            {
                if (SettingList.First().MilestoneFieldDependantUpon == NewsStandID) ValidFirstEntry = true;

            }

            if (!ValidFirstEntry) return "Newstand Needs To Be the First Calculation Dependency";


            List<int> RunningList = new List<int>();

            RunningList.Add(NewsStandID);

            int Counter = 0;


            foreach (MilestoneTreeSetting mts in SettingList)
            {
                string CurrentName = GetMilestoneDescFromID(mts.MilestoneField);
                Counter++;
                //ensure we have a dependency
                int? CurrentDependency = mts.MilestoneFieldDependantUpon;
                if (!CurrentDependency.HasValue)
                {
                    return string.Format("{0} field has no dependency", CurrentName);
                }



                int CurrentDependant = (int)mts.MilestoneFieldDependantUpon;
                int Count = RunningList.Where(x => x == CurrentDependency).Count();
                if (Count == 0)
                {
                    string DepStr = GetMilestoneDescFromID((int)mts.MilestoneFieldDependantUpon);
                    string RetStr = string.Format("</br>The milestone field {0} has a dependency on {1 } that has NOT yet been calculated. </br/> <strong>Check Calc Firing Order at Position {2}</strong>", CurrentName, DepStr, mts.CalcFiringOrder);
                    Message += RetStr;
                }

                RunningList.Add(mts.MilestoneField);


            }
            return Message;

        }



        public static List<FieldAlias> GetAllFieldAliases()
        {
            List<FieldAlias> AliasList = new List<FieldAlias>();
            string ComText = "select * from dbo.MilestoneFieldAlias";
            SqlCommand com = new SqlCommand(ComText, GetConnection());
            SqlDataReader sdr = null;

            try
            {
                com.Connection.Open();
                sdr = com.ExecuteReader();

                while (sdr.Read())
                {

                    if (sdr.HasRows)
                    {
                        FieldAlias fa = new FieldAlias();
                        fa.ID = Convert.ToInt32(sdr["ID"]);
                        fa.AliasValue = sdr["AliasValue"].ToString();
                        fa.FieldFK = Convert.ToInt32(sdr["FieldFK"]);
                        fa.PubCodeFK = Convert.ToInt32(sdr["PubCodeFk"]);
                        AliasList.Add(fa);
                    }


                }


            }

            catch
            {


            }

            finally
            {

                if (!sdr.IsClosed) sdr.Close();
                if (com.Connection.State == ConnectionState.Open) com.Connection.Close();
                com.Dispose();


            }

            return AliasList;

        }


    }
}