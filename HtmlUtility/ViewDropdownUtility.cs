using System.Collections.Generic;
using System.Text;
using Scheduling.Models;
using System.Linq;
using System;

namespace Scheduling.Html
{
    public class ViewDropdown
    {

        public static string BuildUnassignedBaselineDropdown(string CurrentChosenUnassigned)
        {
            StringBuilder sb = new StringBuilder();
            Dictionary<string, string> UnassignedDict = Database.Utility.GetUnassignedBaselines();

            foreach(KeyValuePair<string,string> entry in UnassignedDict)
            {
                if(entry.Key.ToString().ToUpper()==CurrentChosenUnassigned)
                {
                    sb.AppendFormat("<option selected='selected' value='{0}'>{1}</option>", entry.Key, entry.Value);
                }

                else
                { 
                sb.AppendFormat("<option  value='{0}'>{1}</option>",entry.Key,entry.Value);

                }
            }
            return sb.ToString();
        }

        public static string BuildAssignedBaselineDropdown(string CurrentChosenAssigned)
        {
            StringBuilder sb = new StringBuilder();
            Dictionary<string, string> AssignedDict = Database.Utility.GetAssignedBaselines();

            foreach (KeyValuePair<string, string> entry in AssignedDict)
            {
                if (entry.Key.ToString().ToUpper() == CurrentChosenAssigned)
                {
                    sb.AppendFormat("<option selected='selected' value='{0}'>{1}</option>", entry.Key, entry.Value);
                }

                else
                {
                    sb.AppendFormat("<option  value='{0}'>{1}</option>", entry.Key, entry.Value);

                }
            }
            return sb.ToString();
        }

        public static string BuildProjectsWithChangeRequestsDropdown(string CurrentProject)
        {

            StringBuilder sb = new StringBuilder();


            int Count = Scheduling.Database.Utility.GetAllChangeRequests().Count;

            if (Count > 0)
            {

                List<int> DistinctProjects = Scheduling.Database.Utility.GetAllChangeRequests().Select(x => x.ProjectFK).Distinct().ToList();

                foreach (int i in DistinctProjects)
                {
                    ProjectDisplay pd = Scheduling.Database.Utility.GetAllProjects().Where(x => x.ID == i).First();

                    if (!string.IsNullOrEmpty(CurrentProject))
                    {
                        int ProjectID = Convert.ToInt32(CurrentProject);
                        if (pd.ID == ProjectID)
                        {
                            sb.AppendFormat("<option selected='selected' value='{0}'>{1}</option>", pd.ID, pd.Name);
                        }

                        else
                        {
                            sb.AppendFormat("<option value='{0}'>{1}</option>", pd.ID, pd.Name);

                        }

                    }

                    else
                    {

                        sb.AppendFormat("<option value='{0}'>{1}</option>", pd.ID, pd.Name);


                    }

                }

            }

            return sb.ToString();



        }

        public static string BuildSubItemReportingDropdown()
        {
            StringBuilder sb = new StringBuilder();
            List<MainSubItem> SubItemList = Scheduling.Database.Utility.GetAllMainSubItemsForReporting();
            foreach (MainSubItem msi in SubItemList)
            {
                sb.AppendFormat("<option value='{0}'>{1}</option>", msi.ID, msi.Description);
            }

            return sb.ToString();

        }

        public static string BuildPreselectedParentPubForPublication(int? ParentPubID)
        {
            StringBuilder sb = new StringBuilder();
            List<PublicationCode> ParentPubs = Scheduling.Database.Utility.GetAllPublicationCodes().Where(p => !p.ParentPub.HasValue).ToList();

            foreach (PublicationCode p in ParentPubs)
            {
                if (ParentPubID.HasValue)
                {
                    if (p.ID == ParentPubID)
                    {
                        sb.AppendFormat("<option selected='selected' value='{0}'>{1}</option>", p.ID, p.ShortDesc);
                        continue;
                    }
                }


                sb.AppendFormat("<option value='{0}'>{1}</option>", p.ID, p.ShortDesc);


            }

            return sb.ToString();

        }

        public static string BuildPreselectedPrinterDropdownForPublication(int? PrinterID)
        {
            StringBuilder sb = new StringBuilder();
            List<Printer> PrinterList = Database.Utility.GetAllPrinters().OrderBy(p => p.Company).ThenBy(p => p.Address).ToList();

            foreach (Printer p in PrinterList)
            {
                if (PrinterID.HasValue)
                {
                    if (PrinterID.Value == p.ID)
                    {
                        sb.AppendFormat("<option selected='selected' value='{0}'>{1} ({2})</option>", p.ID, p.Company, p.Address);
                        continue;
                    }

                }


                sb.AppendFormat("<option value='{0}'>{1} ({2})</option>", p.ID, p.Company, p.Address);
            }

            return sb.ToString();
        }

       

        public static string BuildPreselectedBaselineTypeDropdown(int? BaselineTypeID)
        {

            StringBuilder sb = new StringBuilder();
            List<ProjectProfileType> ProfileTypeList = Database.Utility.GetAllProjectProfileTypes();
            foreach (ProjectProfileType ppt in ProfileTypeList)
            {

                if (BaselineTypeID.HasValue)
                {

                    if (BaselineTypeID == ppt.ID)
                    {
                        sb.AppendFormat("<option selected='selected' value='{0}'>{1}</option>", ppt.ID, ppt.Description);

                    }


                    else
                    {

                        sb.AppendFormat("<option  value='{0}'>{1}</option>", ppt.ID, ppt.Description);
                    }

                }

                else
                {
                    sb.AppendFormat("<option  value='{0}'>{1}</option>", ppt.ID, ppt.Description);

                }

            }

            return sb.ToString();

        }

        public static string BuildPreselectedYearDropdownForReporting(int? YearID)
        {
            StringBuilder sb = new StringBuilder();
            List<Year> YearList = YearList = Database.Utility.GetAllExistingProjectYears();
            if (YearList.Count > 0)
            {

                foreach (Year y in YearList)
                {
                    if (YearID.HasValue)
                    {
                        if (YearID == y.ID)
                        {
                            sb.AppendFormat("<option selected='selected' value='{0}'>{1}</option>", y.ID, y.Value);
                        }

                        else
                        {
                            sb.AppendFormat("<option value='{0}'>{1}</option>", y.ID, y.Value);

                        }

                    }

                    else
                    {
                        sb.AppendFormat("<option value='{0}'>{1}</option>", y.ID, y.Value);
                    }

                }
            }

            return sb.ToString();


        }

        public static string BuildPreselectedMultiplePublicationDropdown(List<int> input)
        {
            StringBuilder sb = new StringBuilder();
            if (input == null) return BuildPreselectedPublicationDropdown(null);

            else
            {

                List<PublicationCode> PubCodeList = Database.Utility.GetAllProjectPublicationCodes();

                foreach (PublicationCode pc in PubCodeList)
                {
                    int count = input.Where(x => x == pc.ID).Count();
                    if (count == 1)
                    {
                        sb.AppendFormat("<option selected ='selected' value ='{0}'>{1} </option>", pc.ID, pc.ShortDesc);
                    }


                    else
                    {
                        sb.AppendFormat("<option  value ='{0}'>{1} </option>", pc.ID, pc.ShortDesc);

                    }

                }

                return sb.ToString();
            }


        }

        public static string BuildPreselectedPublicationDropdownForAllPubs(int? i)
        {
            StringBuilder PubCodeSB = new StringBuilder();

            //if we have at least one project assigned to a pubcode only show this otherwise show them all.

            List<PublicationCode> PubCodeList =Database.Utility.GetAllPublicationCodes();

           

            foreach (PublicationCode pc in PubCodeList)
            {
                string CurrentStr;

                if (i.HasValue && pc.ID == i)
                {
                    CurrentStr = string.Format("<option selected='selected' value='{0}'>{1} </option>", pc.ID, pc.ShortDesc);

                }

                else
                {
                    CurrentStr = string.Format("<option value='{0}'>{1} </option>", pc.ID, pc.ShortDesc);

                }

                PubCodeSB.Append(CurrentStr);

            }

            return PubCodeSB.ToString();
        }


        public static string BuildPreselectedPublicationDropdown(int? i)
        {
            StringBuilder PubCodeSB = new StringBuilder();

            //if we have at least one project assigned to a pubcode only show this otherwise show them all.

            List<PublicationCode> PubCodeList = Database.Utility.GetAllProjectPublicationCodes();

            if (PubCodeList.Count == 0)
            {
                PubCodeList = Database.Utility.GetAllPublicationCodes();

            }


            foreach (PublicationCode pc in PubCodeList)
            {
                string CurrentStr;

                if (i.HasValue && pc.ID == i)
                {
                    CurrentStr = string.Format("<option selected='selected' value='{0}'>{1} </option>", pc.ID, pc.ShortDesc);

                }

                else
                {
                    CurrentStr = string.Format("<option value='{0}'>{1} </option>", pc.ID, pc.ShortDesc);

                }

                PubCodeSB.Append(CurrentStr);

            }

            return PubCodeSB.ToString();
        }


        public static string BuildPreselectedYearDropdown(int? i)
        {
            StringBuilder YearSB = new StringBuilder();

            List<Year> YearList = null;

            if (Scheduling.Database.Utility.GetAllProjects().Count > 0)
            {

                YearList = Database.Utility.GetAllExistingProjectYears();

            }


            else
            {

                YearList = Database.Utility.GetAllYears();

            }


            foreach (Year y in YearList)
            {
                string CurrentStr;
                if (i.HasValue && i == y.ID)
                {
                    CurrentStr = string.Format("<option selected='selected' value='{0}'>{1}</option>", y.ID, y.Value);
                }


                else
                {
                    CurrentStr = string.Format("<option value='{0}'>{1}</option>", y.ID, y.Value);

                }

                YearSB.Append(CurrentStr);

            }
            return YearSB.ToString();

        }


        public static string BuildPreselectedProjectTypeDropdown(int? i)
        {

            StringBuilder ProjTypeSB = new StringBuilder();

            List<ProjectProfileType> ProjProfileList = null;
            //take into account the fact that there may not be any distinct entries.
            if (Scheduling.Database.Utility.GetAllProjects().Count > 0)
            {
                ProjProfileList = Database.Utility.GetAllExistingProjectProfileTypes();

            }


            else
            {
                ProjProfileList = Database.Utility.GetAllProjectProfileTypes();

            }


            foreach (ProjectProfileType ppt in ProjProfileList)
            {
                string CurrentStr;
                if (i.HasValue && i == ppt.ID)
                {
                    CurrentStr = string.Format("<option selected='selected' value='{0}'>{1}</option>", ppt.ID, ppt.Description);
                }

                else
                {
                    CurrentStr = string.Format("<option value='{0}'>{1}</option>", ppt.ID, ppt.Description);

                }

                ProjTypeSB.Append(CurrentStr);

            }

            return ProjTypeSB.ToString();

        }

        public static string BuildPreselectedProjectStatusDropdown(int? i)
        {

            StringBuilder ProjStatusSB = new StringBuilder();

            List<ProjectStatus> ProjStatusList = null;
            //take into account the fact that there may not be any distinct entries.
            if (Scheduling.Database.Utility.GetAllProjects().Count > 0)
            {
                ProjStatusList = Database.Utility.GetAllExistingProjectStatuses();

            }


            else
            {
                ProjStatusList = Database.Utility.GetAllProjectStatuses();

            }


            foreach (ProjectStatus ps in ProjStatusList)
            {
                string CurrentStr;
                if (i.HasValue && i == ps.ID)
                {
                    CurrentStr = string.Format("<option selected='selected' value='{0}'>{1}</option>", ps.ID, ps.Description);
                }

                else
                {
                    CurrentStr = string.Format("<option value='{0}'>{1}</option>", ps.ID, ps.Description);

                }

                ProjStatusSB.Append(CurrentStr);

            }

            return ProjStatusSB.ToString();


        }

        public static string BuildPreselectedProjectRangeDropdown(int? i)
        {

            StringBuilder ProjRangeSB = new StringBuilder();
            List<Timeline> TimelineList = Database.Utility.GetAllExistingProjectRanges();
            foreach (Timeline t in TimelineList)
            {
                string CurrentStr;
                if (i.HasValue && i == t.ID)
                {
                    CurrentStr = string.Format("<option selected='selected' value='{0}'>{1}</option>", t.ID, t.ShortDesc);
                }

                else
                {
                    CurrentStr = string.Format("<option value='{0}'>{1}</option>", t.ID, t.ShortDesc);

                }

                ProjRangeSB.Append(CurrentStr);

            }

            return ProjRangeSB.ToString();
        }
    }
}