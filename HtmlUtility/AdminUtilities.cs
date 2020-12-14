using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using Scheduling.Models;

namespace Scheduling.Html
{
    public class AdminUtilities
    {
        

        public static string BuildYearDropdown()
        {
            StringBuilder YearSB = new StringBuilder();
            List<Year> YearList = Database.Utility.GetAllYears();
            foreach (Year y in YearList)
            {
                string CurrentStr = string.Format("<option value='{0}'>{1}</option>",y.ID,y.Value);
                YearSB.Append(CurrentStr);

            }
            return YearSB.ToString() ;

        }

        public static string BuildReportingSubitemDropdown()
        {
              StringBuilder SubitemSB = new StringBuilder();
              List<ReportingSubitem> SubList = Database.Utility.GetAllReportingSubItems();


              foreach (ReportingSubitem si in SubList)
              {
                  string CurrentSubitemStr = string.Format("<option value='{0}'>{1}</option>", si.ID, si.Description);
                  SubitemSB.Append(CurrentSubitemStr);

              }
              return SubitemSB.ToString();
        }

        

        public static string BuildProjectRangeDropdown()
        {

            StringBuilder ProjRangeSB = new StringBuilder();
            List<Timeline> TimelineList = Database.Utility.GetAllProjectRanges();
            foreach (Timeline t in TimelineList)
            {

                string CurrentStr = string.Format("<option value='{0}'>{1}</option>",t.ID,t.ShortDesc);
                ProjRangeSB.Append(CurrentStr);

            }

            return ProjRangeSB.ToString();
        }

        public static string BuildProductDropdown()
        {

            StringBuilder ProdSB = new StringBuilder("<option value=''>Choose A Product</option>");
            List<Product> ProdList =Database.Utility.GetAllProducts();

            foreach (Product p in ProdList)
            {
                string CurrentStr = string.Format("<option value='{0}'>{1}</option>", p.ID, p.ProductID);
                ProdSB.Append(CurrentStr);
            }
            return ProdSB.ToString();
        }

        public static string BuildPublicationDropdown()
        {
            StringBuilder PubCodeSB = new StringBuilder("<option value=''>Choose A Publication Code</option>");
            List<PublicationCode> PubCodeList=Database.Utility.GetAllPublicationCodes();
            foreach (PublicationCode pc in PubCodeList)
            {
                string CurrentStr = string.Format("<option value='{0}'>{1} </option>", pc.ID, pc.ShortDesc);
                PubCodeSB.Append(CurrentStr);

            }

            return PubCodeSB.ToString();
        }


        

        public static string BuildMilestoneTreeSettingsProfileDropdownOptionsOnly()

        {
            string InitialOption = "<option value=''>Choose A Profile</option>";
            StringBuilder MstSB = new StringBuilder(InitialOption);
            
            List<MilestoneTreeSettingsProfile> ProfList = Scheduling.Database.Utility.GetAllMilestoneTreeSettingsProfiles();
            foreach (MilestoneTreeSettingsProfile prof in ProfList)
            {
                if(Scheduling.Database.Utility.MilestoneProfileHasSettings(prof.ID))
                {
                  MstSB.AppendFormat("<option value='{0}'>{1}</option>", prof.ID, prof.Description);
                }
            }

            
            return MstSB.ToString();

        }

        //This version is has select included

        public static string BuildMilestoneTreeSettingsProfileDropdownForAdminConfiguration()
        {
           
            
             StringBuilder MstSB = new StringBuilder();
             string AttrName="MilestoneTreeSettingsProfile";
             string TopOption="Choose Milestone Tree Settings Profile";
             MstSB.AppendFormat("<select name='{0}' class='select-{1}' id='{2}'><option value=''>{3}</option>",AttrName,AttrName,AttrName,TopOption);

            List<MilestoneTreeSettingsProfile> ProfList = Scheduling.Database.Utility.GetAllMilestoneTreeSettingsProfiles();
            foreach (MilestoneTreeSettingsProfile prof in ProfList)
            {

               MstSB.AppendFormat("<option value='{0}'>{1}</option>",prof.ID,prof.Description);
            }

            MstSB.Append("</select>");

            return MstSB.ToString();

        }

        //TODO Does this really need a profileid
        public static string BuildMultipleMilestoneTreeSettingAssociations()
        {
            string RetStr =string.Empty;
            string MultStr=string.Empty;

            int NumMilestones = Convert.ToInt32(ConfigurationManager.AppSettings["InitialNumOfShownMilestones"]);

           // string TotalStrStart = string.Format("<form><input type='hidden' name='hfProfile' id='hfProfile' value='{0}'/><input type='hidden' value='' name='hfUpdStr' id='hfUpdStr'/>", ProfileID);
            //string TotalStrEnd = "</form>";

            for (int i = 0; i < NumMilestones; i++)
            {
                RetStr += "<div class='mult-tree-settings'>";
                RetStr+=BuildSingleMilestoneTreeSettingAssociation();
                RetStr += "</div>";
            }

           // return string.Concat(TotalStrStart, RetStr, TotalStrEnd);
            return RetStr;

        }

        public static string BuildSingleMilestoneTreeSettingAssociation()
        {
            //current dropdown
            string SelCurrentNameAttr="CurrentMilestoneField";
            string SelCurrentTopOption = "Choose Current Task";
            string CurrentFieldDropdown = GetMilestoneFieldList(SelCurrentNameAttr, SelCurrentTopOption);

            //parent dropdown
            string SelParentNameAttr = "ParentMilestoneField";
            string SelParentTopOption = "Choose Parent Task";
            string ParentFieldDropdown = GetMilestoneFieldList(SelParentNameAttr, SelParentTopOption);

            //Dependant dropdown

            string SelDependantNameAttr = "DependantMilestoneField";
            string SelDependantTopOption = "Choose Dependant Task";
            string DependantFieldDropdown = GetMilestoneFieldList(SelDependantNameAttr, SelDependantTopOption);

            //dep calc dropdown
            string SelDependantCalculationTopOption = "Choose Dependant Calculation";
            string SelDependantClassAttr="DependantCalculationField";
            string SelDepCalculationDropdown = GetSingleCalculationFieldList(SelDependantClassAttr,SelDependantCalculationTopOption);

            //firing order dropdown
            string FiringHeader = "Choose Calc Firing Order";
            string CalcFirAttr = "CalcFiringOrderField";
            string CalcFiringOrderDropdown = GetFiringOrderList(CalcFirAttr, FiringHeader);

            //range calc dropdown
            string SelRangeCalculatioClassAttr="RangeCalculationField";
            string SelRangeCalculationTopOption = "Choose Due Date Range Calculation";
            string SelRangeCalculationDropdown = GetSingleCalculationFieldList(SelRangeCalculatioClassAttr, SelRangeCalculationTopOption);

            return string.Concat(CurrentFieldDropdown, ParentFieldDropdown, DependantFieldDropdown, SelDepCalculationDropdown,CalcFiringOrderDropdown,SelRangeCalculationDropdown);
        }

        public static string GetFiringOrderList(string input,string TopOption)
        {
            
            StringBuilder fsb = new StringBuilder();
            fsb.AppendFormat("<select name='{0}' class='select-{1}'><option value=''>{0}</option>",input,input, TopOption);

            List<int> CurrentValues = Scheduling.Database.Utility.CreateFiringOrderList();
            foreach (int i in CurrentValues)
            {

                fsb.AppendFormat("<option value='{0}'>{1}</option>",i,i);
            }

            fsb.Append("</select>");

            return fsb.ToString();

        }

        public static string  GetMilestoneFieldList(string SelNameAttr,string TopOption)

        {
            StringBuilder sb = new StringBuilder();
            List<MilestoneField> mfl = Scheduling.Database.Utility.GetAllParentMilestoneFields();
            if (mfl.Count > 0)
            {
                sb.AppendFormat("<select class='select-{0}' name='{1}'><option value=''>{2}</option>", SelNameAttr, SelNameAttr,TopOption);
                foreach (MilestoneField mf in mfl)
                {
                    sb.AppendFormat("<option value='{0}'>{1}</option>", mf.ID, mf.Description);

                }

                sb.Append("</select>");

            }

            return sb.ToString();
            

        }


        public static string GetSingleCalculationFieldList(string input,string TopOption)
        {
            StringBuilder calcSB=new StringBuilder();
           List<Calculation> CalcList= Scheduling.Database.Utility.GetAllCalculationFields();
            if(CalcList.Count > 0)
            {

                 calcSB.AppendFormat("<select class='select-{0}' name='{1}'><option value=''>{2}</option>", input, input,TopOption);
                 foreach(Calculation c in CalcList)
                 {
                     calcSB.AppendFormat("<option value='{0}'>{1}</option>",c.ID, c.ShortDesc);

                 }

                calcSB.Append("</select>");

            }

            return calcSB.ToString();


        }
       

    }
}