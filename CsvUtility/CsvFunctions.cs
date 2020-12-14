using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Scheduling.Models;
using StringUtility = Scheduling.StringFunctions.Utility;

namespace Scheduling.Csv
{
    public class Utility
    {
                
        public static List<ProjectCreatedCSV> ConvertProjectDisplayToProjectCreatedCSV(List<ProjectDisplay> ProjDisplay)
        {
            List<ProjectCreatedCSV> RetList = new List<ProjectCreatedCSV>();

            //Create Header

            ProjectCreatedCSV CsvHeader = new ProjectCreatedCSV();
            CsvHeader.ProjectName =StringUtility.PrepareCsvField(Scheduling.StringFunctions.Utility.GetAppSettingValue("ProjectsCreatedCsvColOneHeader"));
            CsvHeader.CreationDate = StringUtility.PrepareCsvField(Scheduling.StringFunctions.Utility.GetAppSettingValue("ProjectsCreatedCsvColTwoHeader"));
            CsvHeader.TimeOfCreation = StringUtility.PrepareCsvField(Scheduling.StringFunctions.Utility.GetAppSettingValue("ProjectsCreatedCsvColThreeHeader"));
            CsvHeader.Year = StringUtility.PrepareCsvField(Scheduling.StringFunctions.Utility.GetAppSettingValue("ProjectsCreatedCsvColFourHeader"));
            CsvHeader.ProfileType = StringUtility.PrepareCsvField(Scheduling.StringFunctions.Utility.GetAppSettingValue("ProjectsCreatedCsvColFiveHeader"));
            RetList.Add(CsvHeader);


            foreach(ProjectDisplay pd in ProjDisplay)
            {
                ProjectCreatedCSV item = new ProjectCreatedCSV();

                item.ProjectName =StringUtility.PrepareCsvField(pd.Name);
                item.CreationDate =StringUtility.PrepareCsvField(pd.DateCreated.ToLongDateString());
                item.TimeOfCreation =StringUtility.PrepareCsvField(pd.DateCreated.ToShortTimeString());
                item.Year = StringUtility.PrepareCsvField(Database.Utility.GetAllYears().Where(x => x.ID == pd.YearFK).First().Value.ToString());
                item.ProfileType =StringUtility.PrepareCsvField(Scheduling.Database.Utility.GetProfileTypeNameFromProjectID(pd.ID));
                RetList.Add(item);

            }

            return RetList;
        }

        public static List<ProjectNewstandCSV> ConvertProjectNewstandToProjectNewstandCSV(List<ProjectNewstand> InputList)
        {
            List<ProjectNewstandCSV> RetList = new List<ProjectNewstandCSV>();

            //header Comment out for now
            ProjectNewstandCSV CsvHeader = new ProjectNewstandCSV();
            CsvHeader.NewstandDate = StringUtility.PrepareCsvField(Scheduling.StringFunctions.Utility.GetAppSettingValue("ProjectNewstandCsvColOneHeader"));
            CsvHeader.ProjectName = StringUtility.PrepareCsvField(Scheduling.StringFunctions.Utility.GetAppSettingValue("ProjectNewstandCsvColTwoHeader"));
            RetList.Add(CsvHeader);

            foreach (ProjectNewstand pn in InputList)
            {
                ProjectNewstandCSV pnc = new ProjectNewstandCSV();
                pnc.NewstandDate = StringUtility.PrepareCsvField(pn.NewstandDate);
                pnc.ProjectName = StringUtility.PrepareCsvField(pn.ProjectName);
                RetList.Add(pnc);

            }

            return RetList;

        }
        
    }
}