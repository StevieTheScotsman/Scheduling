using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Scheduling.Models;
using System.Text.RegularExpressions;

namespace Scheduling.StringFunctions
{
    public class Utility
    {
        

        public static string GetAppSettingValue(string s)
        {
            return ConfigurationManager.AppSettings[s].Trim();

        }
        //wrap string in double quotes.
        public static string PrepareCsvField(string Input)
        {
            return "\"" + Input.Trim() + "\"";

        }

        public static string GetAllProjectsMinimumStartDate()
        {
            //we need to get list with
            List<ProjectDisplay> ProjList = Scheduling.Database.Utility.GetAllProjects();
            int MinYearForProjects = ProjList.Select(x => x.YearFK).Min();


            int? MinMonthForProjects = ProjList.Where(x => x.YearFK == MinYearForProjects).Select(x => x.ProjectRangeFK).Min();

            string MinMonthStrForProjects = "01";

            if (MinMonthForProjects.HasValue)
            {

                if (MinMonthForProjects > 12) { MinMonthForProjects = 12; }

                if (MinMonthForProjects < 10)
                {
                    MinMonthStrForProjects = string.Format("0{0}", MinMonthForProjects);

                }

                else
                {
                    MinMonthStrForProjects = MinMonthForProjects.ToString();
                }


            }

            string MinYearStrForProjects = Scheduling.Database.Utility.GetAllYears().Where(x => x.ID == MinYearForProjects).First().Value.ToString();

            string StartString = string.Format("{0}-{1}", MinYearStrForProjects, MinMonthStrForProjects);


            return StartString;
        }

        public static string RemoveHtml(string input)
        {
            Regex reg = new Regex("<[^>]*>", RegexOptions.IgnoreCase);
            return reg.Replace(input, string.Empty);

        }

        public static string GenerateDefaultProjectName(SingleProjectWithNewstand spwn)
        {
            string ProductStr = string.Empty;
            if(!string.IsNullOrWhiteSpace(spwn.Product) && spwn.Product !="null")
            {
                ProductStr = spwn.Product;
            }

            string YearStr=Scheduling.Database.Utility.GetYearByID(Convert.ToInt32(spwn.Year)).ToString();
            string TimelineStr=Scheduling.Database.Utility.GetTimelineShortDescFromID(Convert.ToInt32(spwn.Timeline));
            string PubStr=Scheduling.Database.Utility.GetPubCodeShortDescFromID(Convert.ToInt32(spwn.PubCode));


            return string.Format("{0} {1} {2}", TimelineStr, PubStr, ProductStr);

        }

        public static string GenerateDatePickerFieldFromDatabaseDateField(object o)
        {
            //for now return empty string for null value
            string RetStr = string.Empty;
            if (!(o is DBNull))
            {
                DateTime CurrentDTVal = Convert.ToDateTime(o);
                RetStr = CurrentDTVal.ToShortDateString();
                
            }

            

            return RetStr;

        }

        public static List<string> GetStringListFromStringWithPossibleCommaSeperator(string s)
        {
            List<string> RetStrList = new List<string>();
            if (s.Contains(','))
            {
                string[] sa = s.Split(',');
                foreach (string item in sa)
                {
                    RetStrList.Add(item);

                }


            }

            else
            {

            RetStrList.Add(s);

            }


            return RetStrList;

        }

        

        public static string GetDateTimeStrFromDateInput(string s)
        {
            
            string RetStr = "null";

            if (!string.IsNullOrWhiteSpace(s))
            {
                s=s.Trim();

                if (s.Contains('/'))
                {

                    string[] sa =s.Split('/');
                    if (sa.Length == 3)
                    {
                        int CurrentMonth = Convert.ToInt32(sa[0]);
                        int CurrentDay = Convert.ToInt32(sa[1]);
                        int CurrentYear = Convert.ToInt32(sa[2]);
                        DateTime dt = new DateTime(CurrentYear, CurrentMonth, CurrentDay);
                        RetStr = dt.ToShortDateString();
                    }

                }


            }

            return RetStr;

        }

        public static List<MilestoneTreeSetting> GetMilestoneTreeSettingListFromUpdateStr(string s,string p)
        {
            List<MilestoneTreeSetting> MstList = new List<MilestoneTreeSetting>();
            if(s.Contains("||"))
            {

                string[] SplitDoubleDelim=s.Split(new string[] {"||" },StringSplitOptions.None);
                foreach (string item in SplitDoubleDelim)
                {
                    MilestoneTreeSetting mts = GetSingleObjectFromString(item, p);
                    MstList.Add(mts);

                }
            }

            else
            {
                MilestoneTreeSetting mts=GetSingleObjectFromString(s,p);
                MstList.Add(mts);

            }

            return MstList;
        }


        public static int? ConvertStringNullValueToNullableInt(string s)
        {
            if (s.ToUpper().Trim() == "NULL")
            {

                return null;
            }

            else
            {
                return Convert.ToInt32(s);

            }

        }

        public static string ConvertNullableIntToInsertString(int? i)
        {
            if (i.HasValue)
            {
                return i.ToString();
            }

            else
            {

                return "null";

            }


        }
        //return one object 
        public static MilestoneTreeSetting GetSingleObjectFromString(string s,string p)
        {
            string[] SplitSingleDelim = s.Split('|');

            string CurrentTaskValue = SplitSingleDelim[0];
            string CurrentParentTaskValue = SplitSingleDelim[1];
            string CurrentDepTaskValue = SplitSingleDelim[2];
            string CurrentDepCalcValue = SplitSingleDelim[3];
            string CurrentDepCalcFiringOrder = SplitSingleDelim[4];
            string CurrentDepRangeCalculation = SplitSingleDelim[5];
            string CurrentProfile = p;

            MilestoneTreeSetting mts = new MilestoneTreeSetting();

            //assign current task
            mts.MilestoneField = Convert.ToInt32(CurrentTaskValue);
            mts.MilestoneParentField = ConvertStringNullValueToNullableInt(CurrentParentTaskValue);
            mts.MilestoneFieldDependantUpon = ConvertStringNullValueToNullableInt(CurrentDepTaskValue);
            mts.CalculationID = ConvertStringNullValueToNullableInt(CurrentDepCalcValue);
            mts.CalcFiringOrder = ConvertStringNullValueToNullableInt(CurrentDepCalcFiringOrder);
            mts.RangeCalculationID = ConvertStringNullValueToNullableInt(CurrentDepRangeCalculation);
            mts.MilestoneTreeSettingsProfileID = Convert.ToInt32(CurrentProfile);

            return mts;
        }
    }
}