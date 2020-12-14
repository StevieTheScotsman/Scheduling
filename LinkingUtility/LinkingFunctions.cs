using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Scheduling.Models;

namespace Scheduling.Linking
{
    public class Utility
    {

        public static void ManageSingleProjectLinkingAddAndCreateInitialTopLevelEntry(FormCollection fc)
        {


            if (!string.IsNullOrWhiteSpace(fc["chosen-linkable-project"]))
            {
                int PrimaryProject = Convert.ToInt32(fc["chosen-linkable-project"]);
                int SecondaryProject = Convert.ToInt32(fc["current-project"]);
                int LinkSettingID = Convert.ToInt32(fc["chosen-setting-link"]);

                Scheduling.Database.Utility.CreateProjectLinkEntry(PrimaryProject, SecondaryProject,LinkSettingID);
                Scheduling.Linking.Utility.GenerateTopLevelNodeOnlyForLinkedProject(PrimaryProject, SecondaryProject);
            }

        }


        public static void ManageSingleProjectLinkingAddAndCreateAllEntries(FormCollection fc)
        {


            if (!string.IsNullOrWhiteSpace(fc["chosen-linkable-project"]))
            {
                int PrimaryProject = Convert.ToInt32(fc["chosen-linkable-project"]);
                int SecondaryProject = Convert.ToInt32(fc["current-project"]);
                int LinkSettingID = Convert.ToInt32(fc["chosen-setting-link"]);

                Scheduling.Database.Utility.CreateProjectLinkEntry(PrimaryProject, SecondaryProject,LinkSettingID);                                
                GenerateAllSecondaryFieldValuesForLinkedProjects(PrimaryProject, SecondaryProject);

            }

        }
        
        public static void GenerateTopLevelNodeOnlyForLinkedProject(int PrimaryProjectID, int SecondaryProjectID)
        {
            
            int PrimaryProfileTypeID = Scheduling.Database.Utility.GetProfileTypeIDFromProjectID(PrimaryProjectID);
            int SecondaryProfileTypeID = Scheduling.Database.Utility.GetProfileTypeIDFromProjectID(SecondaryProjectID);

            Scheduling.Models.ProjectLinkSetting ps = Scheduling.Database.Utility.GetAllProjectLinkSettings().Where(x => x.PrimaryProfileTypeID == PrimaryProfileTypeID && x.SecondaryProfileTypeID == SecondaryProfileTypeID).ToList().First();
            int PrimaryMilestoneID = ps.PrimaryMilestoneID;
            int SecondaryMilestoneID = ps.SecondaryMilestoneID;
            int CalcID = ps.CalculationID;

            MilestoneValue PrimaryMilestoneRecord = Scheduling.Database.Utility.GetMilestoneValuesByProjectID(PrimaryProjectID).Where(x => x.MilestoneFieldFK == PrimaryMilestoneID).ToList().First();
            MilestoneValue SecondaryMilestoneRecord = Scheduling.Database.Utility.GetMilestoneValuesByProjectID(SecondaryProjectID).Where(x => x.MilestoneFieldFK == SecondaryMilestoneID).ToList().First();

            //Remove existing entries and then update only top one record
            string ComText=string.Format("update dbo.milestonevalue set duedate = null where projectpk={0}",SecondaryProjectID);
            Scheduling.Database.Utility.ExecuteNonQueryWrapper(ComText);

            
            Scheduling.Calc.CalcUtilities.CalculateSecondaryTopLevelDateForLinkedProject(PrimaryMilestoneRecord, SecondaryMilestoneRecord, CalcID);

        }

        public static void GenerateAllSecondaryFieldValuesForLinkedProjects(int PrimaryProjectID, int SecondaryProjectID)
        {

            int PrimaryProfileTypeID = Scheduling.Database.Utility.GetProfileTypeIDFromProjectID(PrimaryProjectID);
            int SecondaryProfileTypeID = Scheduling.Database.Utility.GetProfileTypeIDFromProjectID(SecondaryProjectID);

            Scheduling.Models.ProjectLinkSetting ps = Scheduling.Database.Utility.GetAllProjectLinkSettings().Where(x => x.PrimaryProfileTypeID == PrimaryProfileTypeID && x.SecondaryProfileTypeID == SecondaryProfileTypeID).ToList().First();
            int PrimaryMilestoneID = ps.PrimaryMilestoneID;
            int SecondaryMilestoneID = ps.SecondaryMilestoneID;
            int CalcID = ps.CalculationID;

            MilestoneValue PrimaryMilestoneRecord = Scheduling.Database.Utility.GetMilestoneValuesByProjectID(PrimaryProjectID).Where(x => x.MilestoneFieldFK == PrimaryMilestoneID).ToList().First();
            MilestoneValue SecondaryMilestoneRecord = Scheduling.Database.Utility.GetMilestoneValuesByProjectID(SecondaryProjectID).Where(x => x.MilestoneFieldFK == SecondaryMilestoneID).ToList().First();

            //Update the new entry and use this new entry as a baseline
            Scheduling.Calc.CalcUtilities.CalculateSecondaryTopLevelDateForLinkedProject(PrimaryMilestoneRecord, SecondaryMilestoneRecord, CalcID);
                       
            string UpdatedDueDate=Scheduling.Database.Utility.GetMilestoneValuesByProjectID(SecondaryProjectID).Where(x=>x.MilestoneFieldFK==SecondaryMilestoneID).First().DueDate;
            DateTime dt = Convert.ToDateTime(UpdatedDueDate);

            EditProjectMilestoneFieldNodeDisplay mfnd = new EditProjectMilestoneFieldNodeDisplay();
            mfnd.ProjectID=SecondaryProjectID;
            mfnd.MilestoneFieldID=SecondaryMilestoneID;
            mfnd.Day=dt.Day;
            mfnd.Month=dt.Month;
            mfnd.Year=dt.Year;


            List <EditProjectMilestoneFieldNodeDisplay> UpdateList =Scheduling.Database.Utility.GetMilestoneFieldNodesBasedOnStartingNodeForEditProject(mfnd);

            string ExcStr = string.Empty;
            foreach (EditProjectMilestoneFieldNodeDisplay item in UpdateList)
            {

                string CurrentDueDate = string.Format("{0}/{1}/{2}", item.Month, item.Day, item.Year);
                string CurrentStr = string.Format("update dbo.milestonevalue set DueDate='{0}' where projectpk={1} and milestonefieldfk={2};", CurrentDueDate,mfnd.ProjectID, item.MilestoneFieldID);
                ExcStr += CurrentStr;


            }


            Scheduling.Database.Utility.ExecuteNonQueryWrapper(ExcStr);

            string Comments = string.Format("Ran Link Calculation Process");
            Scheduling.Database.Utility.CreateProjectHistoryEntry(mfnd.ProjectID,Comments);
            //build mfnd start
            //MilestoneFieldNodeDisplay mfnd= new MilestoneFieldNodeDisplay();
            //DateTime dt=Convert.ToDateTime(SecondaryMilestoneRecord.DueDate);
            //mfnd.Day = dt.Day;
            //mfnd.Month = dt.Month;
            //mfnd.Year = dt.Year;
            //mfnd.MilestoneField = SecondaryMilestoneID;
            ////stop

            //List<MilestoneValue> DepList = Scheduling.Database.Utility.GetMilestoneValueCalculationsByProjectID(SecondaryProjectID).Where(x => x.DependantUpon == SecondaryMilestoneID).ToList();

            //Scheduling.Calc.CalcUtilities.CalculateRemainingDependancyNodesforEditProject(DepList, mfnd, SecondaryProjectID);
        }

    }
}