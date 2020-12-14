using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using Scheduling.Models;
using System.Linq;

namespace Scheduling.Html
{
    public class ReportingUtilities
    {
        public static string GenerateMonthYearHeaderForSingleProject(ProjectDisplay pd)
        {
            string RetStr = string.Empty;
            int? CurrentMonth = pd.ProjectRangeFK;
            int? CurrentYear = pd.YearFK;

            if (CurrentMonth.HasValue && CurrentYear.HasValue)
            {
                Scheduling.Models.Timeline t = Scheduling.Database.Utility.GetAllProjectRanges().Where(x => x.ID == CurrentMonth.Value).First();
                string MonthStr = t.LongDesc;
                string YearStr = Scheduling.Database.Utility.GetAllYears().Where(x => x.ID == CurrentYear.Value).First().Value.ToString();
                RetStr = string.Format("{0} {1}",MonthStr,YearStr);
            }

            return RetStr;
        }



        public static int GetNumberOfMajorMilestonesForProject(int CurrentProject)
        {
            int Count= Scheduling.Database.Utility.GetMilestoneValuesByProjectID(CurrentProject).Where(p => p.ParentID == null).Count();

            return Count;
        }

        public static List<string> GetDistinctSubItemDescriptions(int ProjectID)
        {
            List<string> RetList =Scheduling.Database.Utility.GetUniqueSubItemDescriptionForReportingFromProjectID(ProjectID);
            return RetList;
        }

        public static string GetDateBasedOnSubItemDescAndParentIDForSpecificProject(string subitem, int ProjectID, int ParentID)
        {
            string DateStr = "-";
            string ComText = string.Format("select count(*) from dbo.MilestoneValue where ProjectPk ={0} and ParentID={1} and milestonefieldfk in(select id from dbo.milestonefield where [description]='{2}' and duedate is not null)", ProjectID, ParentID, subitem);
            int Count = Convert.ToInt32(Scheduling.Database.Utility.ExecuteScalarWrapper(ComText));
            if (Count == 1)
            {
                ComText = string.Format("select duedate from dbo.MilestoneValue where ProjectPk ={0} and ParentID={1} and milestonefieldfk in(select id from dbo.milestonefield where [description]='{2}')", ProjectID, ParentID, subitem);
                Object o =Scheduling.Database.Utility.ExecuteScalarWrapper(ComText);
                if(o !=null)
                {
                    DateTime dt = (DateTime)o;
                    DateStr = string.Format("{0}/{1}", dt.Month, dt.Day);
                }
                

            }

            return DateStr;

        }

        public static int GetTimelineIDBasedOnReportingMonthPicker(int SelectedMonth)
        {
            return 0;

        }

        public static int GetYearIDBasedOnReportingMonthPicker(int SelectedMonth)
        {

            return 0;
        }

    }
}