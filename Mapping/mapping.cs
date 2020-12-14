using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Scheduling.Mapping
{
    public class Utility
    {

        public static int GetMonthlyTimelineIDFromMonthlyDatepicker(string input)
        {
            switch (input)
            {
                case "01": return (int)Scheduling.Enums.Timeline.JAN;
                case "02": return (int)Scheduling.Enums.Timeline.FEB;
                case "03": return (int)Scheduling.Enums.Timeline.MAR;
                case "04": return (int)Scheduling.Enums.Timeline.APR;
                case "05": return (int)Scheduling.Enums.Timeline.MAY;
                case "06": return (int)Scheduling.Enums.Timeline.JUN;
                case "07": return (int)Scheduling.Enums.Timeline.JUL;
                case "08": return (int)Scheduling.Enums.Timeline.AUG;
                case "09": return (int)Scheduling.Enums.Timeline.SEP;
                case "10": return (int)Scheduling.Enums.Timeline.OCT;
                case "11": return (int)Scheduling.Enums.Timeline.NOV;
                case "12": return (int)Scheduling.Enums.Timeline.DEC;
                default:  return 0;
            }
               

        }

        public static int GetYearIDFromMonthlyDatepicker(string input)
        {
            return 0;
        }
    }
}