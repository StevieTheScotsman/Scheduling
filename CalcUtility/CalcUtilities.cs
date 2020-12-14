using System;
using System.Collections.Generic;
using System.Linq;
using Scheduling.Models;
using System.Text;

namespace Scheduling.Calc
{
    public class CalcUtilities
    {

        public static void RecalculateSingleMagazineEntry(EditSingleMilestoneWithDueDate esm)
        {
            DateTime dt = Convert.ToDateTime(esm.DueDate);
            int CurrentID = Convert.ToInt32(esm.ProjectID);

            MilestoneFieldNodeDisplay mfnd = new MilestoneFieldNodeDisplay();

            mfnd.Day = dt.Day;
            mfnd.Month = dt.Month;
            mfnd.Year = dt.Year;
            mfnd.MilestoneField =Convert.ToInt32(esm.MilestoneFieldFK);

            //need to update current node
            Scheduling.Database.Utility.UpdateSelectedMilestoneValueDueDate(esm);

            EditProjectMilestoneFieldNodeDisplay epmfnd = new EditProjectMilestoneFieldNodeDisplay();
            epmfnd.ProjectID = Convert.ToInt32(esm.ProjectID);
            epmfnd.MilestoneFieldID =Convert.ToInt32(esm.MilestoneFieldFK);
            epmfnd.Day = dt.Day;
            epmfnd.Month = dt.Month;
            epmfnd.Year = dt.Year;

            List<EditProjectMilestoneFieldNodeDisplay> UpdateList = Scheduling.Database.Utility.GetMilestoneFieldNodesBasedOnStartingNodeForEditProject(epmfnd);

            string ExcStr = string.Empty;
            foreach (EditProjectMilestoneFieldNodeDisplay item in UpdateList)
            {

                string CurrentDueDate = string.Format("{0}/{1}/{2}", item.Month, item.Day, item.Year);
                string CurrentStr = string.Format("update dbo.milestonevalue set DueDate='{0}' where projectpk={1} and milestonefieldfk={2};", CurrentDueDate, epmfnd.ProjectID, item.MilestoneFieldID);
                ExcStr += CurrentStr;


            }

            Scheduling.Database.Utility.ExecuteNonQueryWrapper(ExcStr);
            //string BaseNodeStr=Scheduling.Database.Utility.GetMilestoneDescFromID(epmfnd.MilestoneFieldID);
            //string Comments = string.Format("Recalculation....Updating Dependant Nodes For {0} using date of {1}", BaseNodeStr, esm.DueDate);
            //Scheduling.Database.Utility.CreateProjectHistoryEntry(epmfnd.ProjectID, Comments);

        }

        public static string RecalculateAllMagazineProjectsAfterLiveHolidayChange()
        {
                        
            StringBuilder sb = new StringBuilder();

            List<EditSingleMilestoneWithDueDate> EditList = Scheduling.Database.Utility.GetAllMagazineProjectStartingDueDatesForRecalculation();

            foreach (EditSingleMilestoneWithDueDate single in EditList)
            {
                
                RecalculateSingleMagazineEntry(single);
                Scheduling.Database.Utility.CreateProjectHistoryEntry(Convert.ToInt32(single.ProjectID), "Recalculating all milestone values using RecalculateAllMagazineProjectsAfterLiveHolidayChange function");
                sb.AppendFormat("Recalculating Project with ID of {0}",single.ProjectID);
                sb.AppendLine();

            }

           

            return sb.ToString();


        }
    
        public static void CalculateSecondaryTopLevelDateForLinkedProject(MilestoneValue Primary, MilestoneValue Secondary,int CalcID)
        {

            List<DateTime> HolidayDtList = Scheduling.Database.Utility.GetAllHolidays();
            Calculation c = Scheduling.Database.Utility.GetAllCalculationFields().AsEnumerable().Where(x => x.ID ==CalcID).First();

            int CurrentDiff = Convert.ToInt32(c.Diff);
            bool IncWeekends = Convert.ToBoolean(c.IncWeekends);
            bool IncHolidays = Convert.ToBoolean(c.IncHolidays);

            //start 
            string CurrentDtStr = Primary.DueDate;
            DateTime CurrentDt = Convert.ToDateTime(CurrentDtStr);

            DateTime NewDtValue = CurrentDt;

            //basic calc

            if (!IncHolidays && !IncWeekends)
            {
                NewDtValue = CurrentDt.AddDays(CurrentDiff);

            }

            else
            {

                if (CurrentDiff < 0)
                {
                    int OriginalDiff = CurrentDiff;
                    int ExtDays = GetExtendedDaysForNegativeDiff(CurrentDt, CurrentDiff, IncHolidays, IncWeekends, HolidayDtList, 0, OriginalDiff);
                    int FinalDays = (ExtDays * -1) + CurrentDiff;

                    NewDtValue = CurrentDt.AddDays(FinalDays);

                }

                if (CurrentDiff > 0)
                {
                    int OriginalDiff = CurrentDiff;
                    int ExtDays = GetExtendedDaysForPositiveDiff(CurrentDt, CurrentDiff, IncHolidays, IncWeekends, HolidayDtList, 0, OriginalDiff);
                    int FinalDays = ExtDays + CurrentDiff;
                    NewDtValue = CurrentDt.AddDays(FinalDays);

                }



            }

            UpdateNewDependantDateForEditProject(Secondary.ProjectPK,Secondary.MilestoneFieldFK, NewDtValue);


            //stop


        }

        public static void CalculateNewDependantDateForEditProject(MilestoneValue mv, int ProjectID, MilestoneFieldNodeDisplay mfnd)
        {

            //holidays 

            List<DateTime> HolidayDtList = Scheduling.Database.Utility.GetAllHolidays();


            //we are passing in a start node and current value..

            Calculation c = Scheduling.Database.Utility.GetAllCalculationFields().AsEnumerable().Where(x => x.ID == mv.CalculationFK).First();

            int CurrentDiff = Convert.ToInt32(c.Diff);
            bool IncWeekends = Convert.ToBoolean(c.IncWeekends);
            bool IncHolidays = Convert.ToBoolean(c.IncHolidays);


            DateTime CurrentDt = new DateTime(mfnd.Year, mfnd.Month, mfnd.Day);
            DateTime NewDtValue = CurrentDt;

            //basic calc

            if (!IncHolidays && !IncWeekends)
            {
                NewDtValue = CurrentDt.AddDays(CurrentDiff);
                
            }

            else
            {

                if (CurrentDiff < 0)
                {
                    int OriginalDiff = CurrentDiff;
                    int ExtDays = GetExtendedDaysForNegativeDiff(CurrentDt, CurrentDiff, IncHolidays, IncWeekends, HolidayDtList, 0, OriginalDiff);
                    int FinalDays = (ExtDays * -1) + CurrentDiff;

                    NewDtValue = CurrentDt.AddDays(FinalDays);

                }

                if (CurrentDiff > 0)
                {
                    int OriginalDiff = CurrentDiff;
                    int ExtDays = GetExtendedDaysForPositiveDiff(CurrentDt, CurrentDiff, IncHolidays, IncWeekends, HolidayDtList, 0, OriginalDiff);
                    int FinalDays = ExtDays + CurrentDiff;
                    NewDtValue = CurrentDt.AddDays(FinalDays);

                }



            }

            UpdateNewDependantDateForEditProject(ProjectID, mv.MilestoneFieldFK, NewDtValue);


        }

        public static void UpdateNewDependantDateForEditProject(int ProjectID, int MilestoneFieldID, DateTime dt)
        {
            string CurrentUser=Scheduling.Security.Utility.GetCurrentLoggedInUser();
            string UpdateStr = string.Format("update dbo.MilestoneValue set DueDate='{0}',modifiedby='{1}',lastmodified=getdate() where ProjectPK={2} and MilestoneFieldFK={3}", dt.ToShortDateString(), CurrentUser, ProjectID, MilestoneFieldID);
            Scheduling.Database.Utility.ExecuteNonQueryWrapper(UpdateStr);
            //create project note entry
            string CurrentFieldName=Scheduling.Database.Utility.GetMilestoneDescFromID(MilestoneFieldID);
            ProjectDisplay p =Scheduling.Database.Utility.GetAllProjects().Where(x=>x.ID==ProjectID).First();            
            string Comments=string.Format("Updated Field {0} with new Due Date of {1}",CurrentFieldName,dt.ToShortDateString());
            Scheduling.Database.Utility.CreateProjectHistoryEntry(p.CurrentProjectStatus, Comments, ProjectID, p.CurrentVersion);

        }



        public static int GetExtendedDaysForPositiveDiff(DateTime CurrentDt, int CurrentDiff, bool IncHolidays, bool IncWeekends, List<DateTime> HolidayDtList, int PassedInAddition, int OriginalDiff)
        {
            int LastCalculatedAdditions = PassedInAddition;

            int CurrentCalculatedAdditions = 0;

            for (int i = 1; i <= CurrentDiff; i++)
            {
                int DaysToAdd = i;
                bool DayHasBeenAdded = false;
                DateTime DateToCheck = CurrentDt.AddDays(DaysToAdd);
                if (IncWeekends)
                {
                    DayOfWeek dow = DateToCheck.DayOfWeek;
                    if (dow == DayOfWeek.Saturday || dow == DayOfWeek.Sunday)
                    {
                        CurrentCalculatedAdditions++;
                        DayHasBeenAdded = true;
                    }

                }

                if (IncHolidays && !DayHasBeenAdded)
                {
                    foreach (DateTime dt in HolidayDtList)
                    {
                        if (dt.Date.Equals(DateToCheck.Date))
                        {
                            CurrentCalculatedAdditions++;

                        }

                    }

                }

            }


            if (LastCalculatedAdditions == CurrentCalculatedAdditions)
            {

                return LastCalculatedAdditions;


            }

            else
            {

                CurrentDiff = OriginalDiff + CurrentCalculatedAdditions;
                return GetExtendedDaysForPositiveDiff(CurrentDt, CurrentDiff, IncHolidays, IncWeekends, HolidayDtList, CurrentCalculatedAdditions, OriginalDiff);


            }



        }

        public static int GetExtendedDaysForNegativeDiff(DateTime CurrentDt, int CurrentDiff, bool IncHolidays, bool IncWeekends, List<DateTime> HolidayDtList, int PassedInAddition, int OriginalDiff)
        {
            int LastCalculatedAdditions = PassedInAddition;

            int CurrentCalculatedAdditions = 0;

            for (int i = -1; i >= CurrentDiff; i--)
            {
                int DaysToAdd = i;
                bool DayHasBeenAdded = false;
                DateTime DateToCheck = CurrentDt.AddDays(DaysToAdd);
                if (IncWeekends)
                {
                    DayOfWeek dow = DateToCheck.DayOfWeek;
                    if (dow == DayOfWeek.Saturday || dow == DayOfWeek.Sunday)
                    {
                        CurrentCalculatedAdditions++;
                        DayHasBeenAdded = true;
                    }

                }

                if (IncHolidays && !DayHasBeenAdded)
                {
                    foreach (DateTime dt in HolidayDtList)
                    {
                        if (dt.Date.Equals(DateToCheck.Date))
                        {
                            CurrentCalculatedAdditions++;

                        }

                    }

                }

            }


            if (LastCalculatedAdditions == CurrentCalculatedAdditions)
            {

                return LastCalculatedAdditions;


            }

            else
            {

                CurrentDiff = OriginalDiff + (-1 * CurrentCalculatedAdditions);
                return GetExtendedDaysForNegativeDiff(CurrentDt, CurrentDiff, IncHolidays, IncWeekends, HolidayDtList, CurrentCalculatedAdditions, OriginalDiff);


            }

        }

        public static MilestoneFieldNodeDisplay CreateMilestoneFieldDisplayObjectFromDueDate(DateTime NewDtValue, int CurrentTimeline, int CurrentMilestoneField, int CurrentMilestoneProfile)
        {

            MilestoneFieldNodeDisplay NewNodeDisplayItem = new MilestoneFieldNodeDisplay();
            NewNodeDisplayItem.Day = NewDtValue.Day;
            NewNodeDisplayItem.Month = NewDtValue.Month;
            NewNodeDisplayItem.Year = NewDtValue.Year;
            NewNodeDisplayItem.Timeline = CurrentTimeline;
            NewNodeDisplayItem.MilestoneField = CurrentMilestoneField;
            NewNodeDisplayItem.MilestoneProfile = CurrentMilestoneProfile;

            return NewNodeDisplayItem;
        }

        public static EditProjectMilestoneFieldNodeDisplay CreateEditProjectMilestoneFieldNodeDisplayFromDueDate(DateTime NewDtValue,int CurrentMilestoneField, int CurrentProject)
        {

            EditProjectMilestoneFieldNodeDisplay NewNodeDisplayItem = new EditProjectMilestoneFieldNodeDisplay();
            NewNodeDisplayItem.Day = NewDtValue.Day;
            NewNodeDisplayItem.Month = NewDtValue.Month;
            NewNodeDisplayItem.Year = NewDtValue.Year;
            NewNodeDisplayItem.ProjectID = CurrentProject;
            NewNodeDisplayItem.MilestoneFieldID = CurrentMilestoneField;
            return NewNodeDisplayItem;



        }


        public static List<EditProjectMilestoneFieldNodeDisplay> GetMilestoneDisplayListForEditProject(EditProjectMilestoneFieldNodeDisplay StartNode, List<NodeCalculationProcess> CalcProcessList)
        {

            List<EditProjectMilestoneFieldNodeDisplay> NodeDisplayList = new List<EditProjectMilestoneFieldNodeDisplay>();

            List<DateTime> HolidayDtList = Scheduling.Database.Utility.GetAllHolidays();

            NodeDisplayList.Add(StartNode);
                        
            //start 
            foreach (NodeCalculationProcess ncp in CalcProcessList)
            {
                int CurrentMilestoneField = ncp.MilestoneFieldID;

                int CurrentDependency = ncp.DependantUponID;
                int CurrentCalculation = ncp.CalculationID;

                int DepCount = NodeDisplayList.Where(x => x.MilestoneFieldID == CurrentDependency).Count();

                if (DepCount == 0) break;
                EditProjectMilestoneFieldNodeDisplay DisplayItem = NodeDisplayList.AsEnumerable().Where(x => x.MilestoneFieldID == CurrentDependency).First();

                Calculation c = Scheduling.Database.Utility.GetAllCalculationFields().AsEnumerable().Where(x => x.ID == CurrentCalculation).First();


                int CurrentDiff = Convert.ToInt32(c.Diff);
                bool IncWeekends = Convert.ToBoolean(c.IncWeekends);
                bool IncHolidays = Convert.ToBoolean(c.IncHolidays);

                DateTime CurrentDt = new DateTime(DisplayItem.Year, DisplayItem.Month, DisplayItem.Day);
                DateTime NewDtValue = CurrentDt;

                if (!IncHolidays && !IncWeekends)
                {
                    NewDtValue = CurrentDt.AddDays(CurrentDiff);
                    EditProjectMilestoneFieldNodeDisplay NewNodeDisplayItem = CreateEditProjectMilestoneFieldNodeDisplayFromDueDate(NewDtValue, CurrentMilestoneField, StartNode.ProjectID);

                    NodeDisplayList.Add(NewNodeDisplayItem);

                }


                else
                {

                    //todo could be refactored get working first
                    //Calc Diff < 0 start
                    if (CurrentDiff < 0)
                    {
                        int OriginalDiff = CurrentDiff;
                        int ExtDays = GetExtendedDaysForNegativeDiff(CurrentDt, CurrentDiff, IncHolidays, IncWeekends, HolidayDtList, 0, OriginalDiff);
                        int FinalDays = (ExtDays * -1) + CurrentDiff;

                        NewDtValue = CurrentDt.AddDays(FinalDays);

                        EditProjectMilestoneFieldNodeDisplay NewNodeDisplayItem = CreateEditProjectMilestoneFieldNodeDisplayFromDueDate(NewDtValue, CurrentMilestoneField, StartNode.ProjectID);
                        NodeDisplayList.Add(NewNodeDisplayItem);

                    }


                    //Calc Diff greater than zero start
                    if (CurrentDiff > 0)
                    {
                        int OriginalDiff = CurrentDiff;
                        int ExtDays = GetExtendedDaysForPositiveDiff(CurrentDt, CurrentDiff, IncHolidays, IncWeekends, HolidayDtList, 0, OriginalDiff);
                        int FinalDays = ExtDays + CurrentDiff;
                        NewDtValue = CurrentDt.AddDays(FinalDays);

                        EditProjectMilestoneFieldNodeDisplay NewNodeDisplayItem = CreateEditProjectMilestoneFieldNodeDisplayFromDueDate(NewDtValue, CurrentMilestoneField, StartNode.ProjectID);
                        NodeDisplayList.Add(NewNodeDisplayItem);

                    }


                }

            }


            //stop

            return NodeDisplayList;

        }



        //old version
        public static List<EditProjectMilestoneFieldNodeDisplay> GetMilestoneDisplayListForEditProjectOLD(EditProjectMilestoneFieldNodeDisplay StartNode, List<NodeCalculationProcess> CalcProcessList)
        {

            List<EditProjectMilestoneFieldNodeDisplay> NodeDisplayList = new List<EditProjectMilestoneFieldNodeDisplay>();

            List<DateTime> HolidayDtList = Scheduling.Database.Utility.GetAllHolidays();

            NodeDisplayList.Add(StartNode);

            //start 
            foreach (NodeCalculationProcess ncp in CalcProcessList)
            {
                int CurrentMilestoneField = ncp.MilestoneFieldID;

                int CurrentDependency = ncp.DependantUponID;
                int CurrentCalculation = ncp.CalculationID;

                EditProjectMilestoneFieldNodeDisplay DisplayItem = NodeDisplayList.AsEnumerable().Where(x => x.MilestoneFieldID == CurrentDependency).First();

                Calculation c = Scheduling.Database.Utility.GetAllCalculationFields().AsEnumerable().Where(x => x.ID == CurrentCalculation).First();


                int CurrentDiff = Convert.ToInt32(c.Diff);
                bool IncWeekends = Convert.ToBoolean(c.IncWeekends);
                bool IncHolidays = Convert.ToBoolean(c.IncHolidays);

                DateTime CurrentDt = new DateTime(DisplayItem.Year, DisplayItem.Month, DisplayItem.Day);
                DateTime NewDtValue = CurrentDt;

                if (!IncHolidays && !IncWeekends)
                {
                    NewDtValue = CurrentDt.AddDays(CurrentDiff);
                    EditProjectMilestoneFieldNodeDisplay NewNodeDisplayItem = CreateEditProjectMilestoneFieldNodeDisplayFromDueDate(NewDtValue, CurrentMilestoneField, StartNode.ProjectID);

                    NodeDisplayList.Add(NewNodeDisplayItem);

                }


                else
                {

                    //todo could be refactored get working first
                    //Calc Diff < 0 start
                    if (CurrentDiff < 0)
                    {
                        int OriginalDiff = CurrentDiff;
                        int ExtDays = GetExtendedDaysForNegativeDiff(CurrentDt, CurrentDiff, IncHolidays, IncWeekends, HolidayDtList, 0, OriginalDiff);
                        int FinalDays = (ExtDays * -1) + CurrentDiff;

                        NewDtValue = CurrentDt.AddDays(FinalDays);

                        EditProjectMilestoneFieldNodeDisplay NewNodeDisplayItem = CreateEditProjectMilestoneFieldNodeDisplayFromDueDate(NewDtValue, CurrentMilestoneField, StartNode.ProjectID);
                        NodeDisplayList.Add(NewNodeDisplayItem);

                    }


                    //Calc Diff greater than zero start
                    if (CurrentDiff > 0)
                    {
                        int OriginalDiff = CurrentDiff;
                        int ExtDays = GetExtendedDaysForPositiveDiff(CurrentDt, CurrentDiff, IncHolidays, IncWeekends, HolidayDtList, 0, OriginalDiff);
                        int FinalDays = ExtDays + CurrentDiff;
                        NewDtValue = CurrentDt.AddDays(FinalDays);

                        EditProjectMilestoneFieldNodeDisplay NewNodeDisplayItem = CreateEditProjectMilestoneFieldNodeDisplayFromDueDate(NewDtValue, CurrentMilestoneField, StartNode.ProjectID);
                        NodeDisplayList.Add(NewNodeDisplayItem);

                    }


                }

            }


            //stop

            return NodeDisplayList;

        }

        //takes a starting node and produces a list of objects for the ajax call this is done for the base profile.
        public static List<MilestoneFieldNodeDisplay> GetMilestoneDisplayList(MilestoneFieldNodeDisplay StartNode, List<NodeCalculationProcess> CalcProcessList)
        {
            int CurrentTimeline = StartNode.Timeline;
            int CurrentMilestoneProfile = StartNode.MilestoneProfile;

            List<MilestoneFieldNodeDisplay> NodeDisplayList = new List<MilestoneFieldNodeDisplay>();

            List<DateTime> HolidayDtList = Scheduling.Database.Utility.GetAllHolidays();

            NodeDisplayList.Add(StartNode);

            foreach (NodeCalculationProcess ncp in CalcProcessList)
            {

                int CurrentMilestoneField = ncp.MilestoneFieldID;

                int CurrentDependency = ncp.DependantUponID;
                int CurrentCalculation = ncp.CalculationID;

                MilestoneFieldNodeDisplay DisplayItem = NodeDisplayList.AsEnumerable().Where(x => x.MilestoneField == CurrentDependency).First();

                Calculation c = Scheduling.Database.Utility.GetAllCalculationFields().AsEnumerable().Where(x => x.ID == CurrentCalculation).First();

                int CurrentDiff = Convert.ToInt32(c.Diff);
                bool IncWeekends = Convert.ToBoolean(c.IncWeekends);
                bool IncHolidays = Convert.ToBoolean(c.IncHolidays);

                DateTime CurrentDt = new DateTime(DisplayItem.Year, DisplayItem.Month, DisplayItem.Day);
                DateTime NewDtValue = CurrentDt;

                //if it is a base calculation just go ahead
                if (!IncHolidays && !IncWeekends)
                {
                    NewDtValue = CurrentDt.AddDays(CurrentDiff);
                    MilestoneFieldNodeDisplay NewNodeDisplayItem = CreateMilestoneFieldDisplayObjectFromDueDate(NewDtValue, CurrentTimeline, CurrentMilestoneField, CurrentMilestoneProfile);

                    NodeDisplayList.Add(NewNodeDisplayItem);

                }

                //more checks required use seperate logic flow.
                else
                {

                    //todo could be refactored get working first
                    //Calc Diff < 0 start
                    if (CurrentDiff < 0)
                    {
                        int OriginalDiff = CurrentDiff;
                        int ExtDays = GetExtendedDaysForNegativeDiff(CurrentDt, CurrentDiff, IncHolidays, IncWeekends, HolidayDtList, 0, OriginalDiff);
                        int FinalDays = (ExtDays * -1) + CurrentDiff;

                        NewDtValue = CurrentDt.AddDays(FinalDays);

                        MilestoneFieldNodeDisplay NewNodeDisplayItem = CreateMilestoneFieldDisplayObjectFromDueDate(NewDtValue, CurrentTimeline, CurrentMilestoneField, CurrentMilestoneProfile);
                        NodeDisplayList.Add(NewNodeDisplayItem);

                    }


                    //Calc Diff greater than zero start
                    if (CurrentDiff > 0)
                    {
                        int OriginalDiff = CurrentDiff;
                        int ExtDays = GetExtendedDaysForPositiveDiff(CurrentDt, CurrentDiff, IncHolidays, IncWeekends, HolidayDtList, 0, OriginalDiff);
                        int FinalDays = ExtDays + CurrentDiff;
                        NewDtValue = CurrentDt.AddDays(FinalDays);

                        MilestoneFieldNodeDisplay NewNodeDisplayItem = CreateMilestoneFieldDisplayObjectFromDueDate(NewDtValue, CurrentTimeline, CurrentMilestoneField, CurrentMilestoneProfile);
                        NodeDisplayList.Add(NewNodeDisplayItem);

                    }

                }

            }

            return NodeDisplayList;
        }


    }



}