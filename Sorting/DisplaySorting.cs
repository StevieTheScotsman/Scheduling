using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Scheduling.Models;

// steve c 8/11 update to use projectrange sort order field.
namespace Scheduling.Sorting
{
    public class DisplaySorting
    {

        public static List<ProjectDisplay> SortByPubCodeDescThenYearThenTimeline(List<ProjectDisplay> InputList)
        {
            List<ProjectDisplay> OutputList = new List<ProjectDisplay>();
            List<int?> DistinctPubIDForModel = InputList.Select(x => x.PubCodeFK).Distinct().ToList();
            List<Scheduling.Models.PublicationCode> PubCodeList = Scheduling.Database.Utility.GetAllPublicationCodes().Where(x => DistinctPubIDForModel.Contains(x.ID)).OrderBy(x => x.LongDesc).ToList();

            foreach (PublicationCode pc in PubCodeList)
            {
                List<ProjectDisplay> Temp = InputList.Where(x => x.PubCodeFK == pc.ID).OrderBy(x => x.YearFK).ThenBy(x => x.ProjectRangeSortOrder).ToList();
                OutputList.AddRange(Temp);

            }

            return OutputList;

        }
        [Obsolete]
        public static List<string> GetOrderedListBasedOnComparisonList(List<string> ComparisonList, List<string> InputList)
        {
            IEnumerable<string> EnumDescList = InputList.AsEnumerable<string>();
            IEnumerable<string> ResultantEnum = ComparisonList.Intersect(EnumDescList);

            List<string> RetList = ResultantEnum.ToList();
            return RetList;


        }


        public static List<ProjectDisplay> SortProjectsBasedOnReportingRequirements(List<ProjectDisplay> InputList)
        {
           
           List<ProjectDisplay> RetList= new List<ProjectDisplay>();
           
           //Use display order field in project range table..that populates projectrangesortorder property.
           //ensures jan-feb shows before march.

          RetList=InputList.OrderBy(x => x.PubCodeFK).ThenBy(x => x.YearFK).ThenBy(x => x.ProjectRangeSortOrder).ToList();

           return RetList;

        }



        public static List<string> GetSortedDisplaySequenceFromDbBasedOnPubCodeAndTimeline(ProjectDisplay pd)
        {
            List<string> RetList = new List<string>();
            List<MainSubItemSort> SisList = new List<MainSubItemSort>();

            int? CurrentPubCode = pd.PubCodeFK;
            int? CurrentTimeline = pd.ProjectRangeFK;

            if (CurrentTimeline.HasValue && CurrentPubCode.HasValue)
            {

                string ComText = string.Format("select count(*) from dbo.MilestoneFieldMainSubItemsReportSorting where pubcodefk={0} and projectrangefk={1}", CurrentPubCode, CurrentTimeline);
                int PubCodeRangeCount = Convert.ToInt32(Scheduling.Database.Utility.ExecuteScalarWrapper(ComText));
                if (PubCodeRangeCount > 0)
                {
                    SisList = Scheduling.Database.Utility.GetAllMilestoneFieldMainSubItemsForReportSorting().Where(x => x.PubCodeFK == CurrentPubCode).Where(x => x.ProjectRangeFK == CurrentTimeline).OrderBy(x => x.SortOrder).ToList();

                }

                else
                {
                    string ComPubText = string.Format("select count(*) from dbo.MilestoneFieldMainSubItemsReportSorting where pubcodefk={0} and projectrangefk is null", CurrentPubCode);
                    int PubCodeCount = Convert.ToInt32(Scheduling.Database.Utility.ExecuteScalarWrapper(ComPubText));
                    if (PubCodeCount > 0)
                    {
                        SisList = Scheduling.Database.Utility.GetAllMilestoneFieldMainSubItemsForReportSorting().Where(x => x.PubCodeFK == CurrentPubCode).Where(x=>!x.ProjectRangeFK.HasValue).OrderBy(x => x.SortOrder).ToList();
                    }
                }

            }


            else
            {

                string ComText = string.Format("select count(*) from dbo.MilestoneFieldMainSubItemsReportSorting where pubcodefk={0} and projectrangefk is null", CurrentPubCode);
                int PubCodeCount = Convert.ToInt32(Scheduling.Database.Utility.ExecuteScalarWrapper(ComText));
                SisList = Scheduling.Database.Utility.GetAllMilestoneFieldMainSubItemsForReportSorting().Where(x => x.PubCodeFK == CurrentPubCode).OrderBy(x => x.SortOrder).ToList();
            }

            //if not in table use default values
            // early edit,depts (a/e),ad only pages,ad index,late edit Now pulling from db 7/30/2014 

            List<int> RetIntList = new List<int>();

            if (SisList.Count > 0)
            {

                RetIntList = SisList.Select(x => x.MilestoneFieldSubItemFK).ToList();

            }

                
            else
            {
                List<MainSubItemSort> DefaultList = Scheduling.Database.Utility.GetAllMilestoneFieldMainSubItemsForReportSorting().Where(x => !x.ProjectRangeFK.HasValue).Where(y => !y.PubCodeFK.HasValue).OrderBy(z => z.SortOrder).ToList();
                
                foreach(MainSubItemSort item in DefaultList)
                {
                    RetIntList.Add(item.MilestoneFieldSubItemFK);
                }

            }


            foreach (int i in RetIntList)
            {
                string s = Scheduling.Database.Utility.GetAllMainSubItems().Where(x => x.ID == i).Select(x => x.Description).First();
                RetList.Add(s);
            }

            return RetList;

        }
        //Replaced by GetSortedDisplaySequenceFromDbBasedOnPubCodeAndTimeline 7/30/14

        [Obsolete]
        public static List<string> GetSortedDisplaySequenceBasedOnPubID(int? PubID, List<string> InputList)
        {
            List<string> ComparisonList = new List<string>();
        
            
            //dsc
            //if(PubID==11)
            //{
            //    ComparisonList.Add("Early Edit/Features");
            //    ComparisonList.Add("Early Depts./Ed. Brief");
            //    ComparisonList.Add("Ad Only Pages");
            //    ComparisonList.Add("Late Depts. (A/E)");

            //    return GetOrderedListBasedOnComparisonList(ComparisonList, InputList);
            //}
            //grw
            //if(PubID==12)
            //{

            //    ComparisonList.Add("Early Edit");
            //    ComparisonList.Add("Depts. (A/E)");
            //    ComparisonList.Add("Ad Only Pages");
            //    ComparisonList.Add("Ad Index");
            //    ComparisonList.Add("Late Edit");

            //    return GetOrderedListBasedOnComparisonList(ComparisonList, InputList);

            //}
            //mod
            //if(PubID==13)
            //{
            //    ComparisonList.Add("Group 1");
            //    ComparisonList.Add("Group 2");
            //    ComparisonList.Add("Ad Only Pages");
            //    ComparisonList.Add("Group 3");

            //    return GetOrderedListBasedOnComparisonList(ComparisonList, InputList);


            //}
            //mrr
            //if(PubID==14)
            //{
            //    ComparisonList.Add("Early Edit");
            //    ComparisonList.Add("Depts. (A/E)");
            //    ComparisonList.Add("Ad Only Pages");
            //    ComparisonList.Add("Ad Index");
            //    ComparisonList.Add("Late Edit/CNI Ad");

            //    return GetOrderedListBasedOnComparisonList(ComparisonList, InputList);

            //  }

            //trn
            //if(PubID==16)
            //{
            //    ComparisonList.Add("Early Edit");
            //    ComparisonList.Add("Depts. (A/E)");
            //    ComparisonList.Add("Ad Only Pages");
            //    ComparisonList.Add("Ad Index");
            //    ComparisonList.Add("Late Edit/CNI Ad");

            //    return GetOrderedListBasedOnComparisonList(ComparisonList, InputList);

            //}

            ////loc
            //if(PubID==17)
            //{

            //    ComparisonList.Add("Early Edit");
            //    ComparisonList.Add("Ad Only Pages");
            //    ComparisonList.Add("Late Depts. (A/E)");

            //    return GetOrderedListBasedOnComparisonList(ComparisonList, InputList);

            //}
            //sac contest cars
            //if(PubID==18)
            //{
            //    ComparisonList.Add("Early Edit");
            //    ComparisonList.Add("Mid Edit");
            //    ComparisonList.Add("Late Edit");


            //    return GetOrderedListBasedOnComparisonList(ComparisonList, InputList);

            //  }

            //hobby show
            //if (PubID == 19)
            //{
            //    ComparisonList.Add("Group 1");
            //    ComparisonList.Add("Group 2");
            //    ComparisonList.Add("Ad Only Pages");
            //    ComparisonList.Add("Group 3");

            //    return GetOrderedListBasedOnComparisonList(ComparisonList, InputList);

            //}


            //mrp model railroad planning
            //if(PubID==20)
            //{

            //    ComparisonList.Add("Early Edit");
            //    ComparisonList.Add("Ad Only Pages");
            //    ComparisonList.Add("Ad Index");
            //    ComparisonList.Add("Late Edit (A/E)");

            //    return GetOrderedListBasedOnComparisonList(ComparisonList, InputList);

            //}
            //gmr
            //if (PubID == 21)
            //{
            //    ComparisonList.Add("Early Edit");
            //    ComparisonList.Add("Depts. (A/E)");
            //    ComparisonList.Add("Ad Only Pages");
            //    ComparisonList.Add("Ad Index");
            //    ComparisonList.Add("Late Edit");

            //    return GetOrderedListBasedOnComparisonList(ComparisonList, InputList);

            //}
            //wrk
            //if(PubID==22)
            //{
            //    ComparisonList.Add("Batch A");
            //    ComparisonList.Add("Batch B");
            //    ComparisonList.Add("Batch C");
            //    //add ref helene
            //    ComparisonList.Add("Ad Only Pages");
            //    ComparisonList.Add("Ad Index");
            //    ComparisonList.Add("Late Edit");

            //    return GetOrderedListBasedOnComparisonList(ComparisonList, InputList);

            //}
            //fsm
            //if(PubID==2)
            //{

            //    ComparisonList.Add("Early Edit");
            //    ComparisonList.Add("Depts. (A/E)");
            //    ComparisonList.Add("Ad Only Pages");
            //    ComparisonList.Add("Ad Index");
            //    ComparisonList.Add("Late Edit/CNI Ad");

            //    return GetOrderedListBasedOnComparisonList(ComparisonList, InputList);
            //}

            //  }

            return InputList;


        }
    }
}