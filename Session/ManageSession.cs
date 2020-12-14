using System;

namespace Scheduling.Session
{
    public class Utility
    {

        public static int? GenerateYearValueUsingSession(string CurrentYearDropdownInput)
        {
             object c = System.Web.HttpContext.Current.Session["CurrentYear"];

             if (c == null)
             {
                 string CurrentSetting = Scheduling.StringFunctions.Utility.GetAppSettingValue("DefaultYear");

                 if(string.IsNullOrWhiteSpace(CurrentSetting))
                 {
                     int CurrentFullYear =System.DateTime.Now.Year;
                     int YearID = Scheduling.Database.Utility.GetAllYears().Find(x => x.Value == CurrentFullYear).ID;
                     CurrentSetting = YearID.ToString();
                 }
                 

                 System.Web.HttpContext.Current.Session["CurrentYear"] = CurrentSetting;

             }


             if (!string.IsNullOrWhiteSpace(CurrentYearDropdownInput))
             {
                 System.Web.HttpContext.Current.Session["CurrentYear"] = CurrentYearDropdownInput;

             }


             int? RetInt = null;
             object o = System.Web.HttpContext.Current.Session["CurrentYear"];

             if (o != null)
             {
                 RetInt = Convert.ToInt32(o);

             }


             return RetInt;
             


        }

        //in the case where we would need to get back to all pubcodes this code needs to be refactored.
        public static int? GeneratePubCodeValueUsingSession(string CurrentDropdownValueInput)
        {
            object c = System.Web.HttpContext.Current.Session["CurrentPubCode"];

            if (c == null)
            {
                System.Web.HttpContext.Current.Session["CurrentPubCode"] = Scheduling.StringFunctions.Utility.GetAppSettingValue("DefaultPubCode");
            }

            if (!string.IsNullOrWhiteSpace(CurrentDropdownValueInput))
            {
                System.Web.HttpContext.Current.Session["CurrentPubCode"] = CurrentDropdownValueInput;

            }

          

            

            int? RetInt = null;
            object o = System.Web.HttpContext.Current.Session["CurrentPubCode"];
           
            if(o !=null)
            {
                RetInt=Convert.ToInt32(o);

            }
          

            return RetInt;
        }


       
    }
}