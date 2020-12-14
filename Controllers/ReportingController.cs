using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;
using Scheduling.Models;
using Scheduling.ActionFilter;
using System.Linq;
using System.Text.RegularExpressions;
using System.Configuration;

namespace Scheduling.Controllers
{
    [CleanUpReportingDirectoryActionFilter]
    public class ReportingController : Controller
    {
        //PDF The Attribute will cause the view to render as a pdf.

        [EO.Pdf.Mvc.RenderAsPDF]
        public ActionResult ProjectsNewsStandPDF()
        {
            List<ProjectNewstand> PnList = Scheduling.Database.Utility.GetProjectNewstandDates();
            return View(PnList);

        }

        public ActionResult AddReportingPresentation()
        {
            ViewBag.ProjectRangeOptions = Scheduling.Html.AdminUtilities.BuildProjectRangeDropdown();
            ViewBag.PubCodeDropdownOptions = Scheduling.Html.AdminUtilities.BuildPublicationDropdown();
            ViewBag.ReportingSubItemOptins = Scheduling.Html.AdminUtilities.BuildReportingSubitemDropdown();

            return View();
        }

        public ActionResult Presentation()
        {
            List<MainSubItemSort> MssList = Scheduling.Database.Utility.GetAllMilestoneFieldMainSubItemsForReportSorting().ToList();
            List<PublicationCode> PubCodes = Scheduling.Database.Utility.GetAllPublicationCodes().OrderBy(x => x.LongDesc).ToList();

            ViewBag.PubCodes = PubCodes;
            return View(MssList);
        }

        public ActionResult DeleteSubSortWithTimelineAndPubcode(FormCollection fc)
        {
            if (!string.IsNullOrWhiteSpace(fc["hfTimeline"]) && !string.IsNullOrWhiteSpace(fc["hfPubcode"]))
            {
                string CurrentPubCode = fc["hfPubcode"];
                string CurrentTimeline = fc["hfTimeline"];
                string DelText = string.Format("delete from dbo.MilestoneFieldMainSubItemsReportSorting where pubcodefk ={0} and projectrangefk={1}", CurrentPubCode, CurrentTimeline);
                Scheduling.Database.Utility.ExecuteNonQueryWrapper(DelText);
            }

            return RedirectToAction("Presentation");
        }


        public ActionResult DeleteSubSortWithPubcodeOnly(FormCollection fc)
        {
            if (!string.IsNullOrWhiteSpace(fc["hfPubcode"]))
            {
                string CurrentPubCode = fc["hfPubcode"];
                string DelText = string.Format("delete from dbo.MilestoneFieldMainSubItemsReportSorting where pubcodefk ={0} and projectrangefk is null", CurrentPubCode);
                Scheduling.Database.Utility.ExecuteNonQueryWrapper(DelText);
            }

            return RedirectToAction("Presentation");

        }

        public ActionResult ProcessAddPresentationSubitems(FormCollection fc)

        {
            string SubItemsDelimitedStr = string.Empty;
            string PubcodeStr = string.Empty;
            string ProjectRangeStr = string.Empty;
            string InsText = string.Empty;

            if (!string.IsNullOrWhiteSpace(fc["hf-report-setup"]))
            {
                SubItemsDelimitedStr = fc["hf-report-setup"];
            }

            if (!string.IsNullOrWhiteSpace(fc["pubcode"]))
            {
                PubcodeStr = fc["pubcode"];
            }

            if (!string.IsNullOrWhiteSpace(fc["project-range"]))
            {
                ProjectRangeStr = fc["project-range"];
            }

            if (!string.IsNullOrWhiteSpace(SubItemsDelimitedStr) && !string.IsNullOrWhiteSpace(PubcodeStr))
            {

                string DelComText = string.Format("delete from dbo.MilestoneFieldMainSubItemsReportSorting where pubcodefk ={0}", PubcodeStr);
                string FilterStr = string.Empty;

                if (!string.IsNullOrWhiteSpace(ProjectRangeStr))
                {
                    FilterStr = string.Format("and projectrangefk = {0}", ProjectRangeStr);


                }

                else
                {
                    //whacks general one
                    FilterStr = "and projectrangefk is null";
                }

                DelComText = string.Concat(DelComText, " ", FilterStr);

                //On the client side we currently require 2 rows so in essence the else statement should not execute

                string ChoppedStr = SubItemsDelimitedStr.Substring(0, SubItemsDelimitedStr.Length - 2);

                List<string> DoublePipeList = new List<string>();

                if (ChoppedStr.Contains("||"))
                {

                    string[] RowArray = ChoppedStr.Split(new string[] { "||" }, StringSplitOptions.None);

                    DoublePipeList = RowArray.ToList<string>();
                }

                else

                {
                    DoublePipeList.Add(ChoppedStr);

                }

                string TotalExcStr = string.Empty;

                foreach (string s in DoublePipeList)
                {
                    string[] PipeArray = s.Split('|');

                    string CurrentInsStr = string.Empty;

                    string SubItemFK = PipeArray[0];
                    string SortOrder = PipeArray[1];

                    if (string.IsNullOrWhiteSpace(ProjectRangeStr))
                    {

                        CurrentInsStr = string.Format("insert into dbo.MilestoneFieldMainSubItemsReportSorting (PubCodeFK,MilestoneFieldMainSubItemFK,SortOrder) values({0},{1},{2});", PubcodeStr, SubItemFK, SortOrder);

                    }

                    else
                    {
                        CurrentInsStr = string.Format("insert into dbo.MilestoneFieldMainSubItemsReportSorting (PubCodeFK,MilestoneFieldMainSubItemFK,SortOrder,ProjectRangeFK) values({0},{1},{2},{3});", PubcodeStr, SubItemFK, SortOrder, ProjectRangeStr);

                    }

                    TotalExcStr = TotalExcStr + CurrentInsStr;

                }

                Scheduling.Database.Utility.ExecuteNonQueryWrapper(DelComText);
                Scheduling.Database.Utility.ExecuteNonQueryWrapper(TotalExcStr);

            }

            return RedirectToAction("Presentation");
        }


        public ActionResult DataIntegrity()
        {
            List<DupProjectEntry> DupEntryList = Scheduling.Database.Utility.GetAllDuplicateProjectEntries();
            return View(DupEntryList);

        }

        [EO.Pdf.Mvc.RenderAsPDF]
        public ActionResult ProjectsCreatedPDF()
        {
            int CreatedStatus = (int)Scheduling.Enums.ProjectStatus.Created;
            List<ProjectDisplay> PdList = Scheduling.Database.Utility.GetAllProjects().Where(x => x.CurrentProjectStatus == CreatedStatus).OrderByDescending(x => x.DateCreated).ToList();

            return View(PdList);
        }


        public ActionResult ExcelStyleReportsCSV(FormCollection fc)
        {
            List<ProjectDisplay> PdList = Scheduling.Database.Utility.GetAllProjects();
            return View(PdList);

        }

        //REGULAR

        public ActionResult ProjectsCreated()
        {
            int CreatedStatus = (int)Scheduling.Enums.ProjectStatus.Created;
            List<ProjectDisplay> PdList = Scheduling.Database.Utility.GetAllProjects().Where(x => x.CurrentProjectStatus == CreatedStatus).OrderByDescending(x => x.DateCreated).ToList();

            return View(PdList);
        }

        public ActionResult ProjectsNewsStand()
        {
            List<ProjectNewstand> PnList = Scheduling.Database.Utility.GetProjectNewstandDates();
            return View(PnList);

        }


        //Rolled in to version 4
        //ExcelStyleReportsFormatRevised.cshmtl (page)
        //Added by --- Tom W.
        [EO.Pdf.Mvc.RenderAsPDF]
        public ActionResult ExcelStyleReportsViewFormatRevised()
        {
            /*User selects fit and landscape */
            /* Best Yet with fit option ratio to width height should be square root of 2 */
            EO.Pdf.HtmlToPdf.Options.PageSize = new System.Drawing.SizeF(17f, 11f);
            EO.Pdf.HtmlToPdf.Options.OutputArea = new System.Drawing.RectangleF(0.1f, 0.1f, 16.8f, 10.8f);

            List<ProjectDisplay> PdList = (List<ProjectDisplay>)Session["ProjectDisplayResultSet"];
            int ReportingYearID = PdList.Select(x => x.YearFK).Min();
            int ReportingYearVal = Scheduling.Database.Utility.GetYearByID(ReportingYearID);
            ViewBag.ReportingYearVal = ReportingYearVal;
            return View(PdList);
        }



        [EO.Pdf.Mvc.RenderAsPDF]
        public ActionResult ExcelStyleReportsPDFReplicateView()
        {
            /*http://www.essentialobjects.com/doc/4/htmltopdf/page_size.aspx*/

            /*User selects fit and landscape */

            /* Best Yet with fit option ratio to width height should be square root of 2 */
            EO.Pdf.HtmlToPdf.Options.PageSize = new System.Drawing.SizeF(17f, 11f);
            EO.Pdf.HtmlToPdf.Options.OutputArea = new System.Drawing.RectangleF(0.1f, 0.1f, 16.8f, 10.8f);

            //removed ref helene 4/16/2014
            // EO.Pdf.HtmlToPdf.Options.HeaderHtmlFormat ="<div style='overflow:hidden' class='header-container'><div style='float:right'>Page {page_number} of &nbsp;{total_pages}</div></div>";
            //On excel style reports view session is set and ordered by SortByPubCodeDescThenYearThenTimeline method in display sorting .Currently Long Desc.
            List<ProjectDisplay> PdList = (List<ProjectDisplay>)Session["ProjectDisplayResultSet"];

            //Get Min Year Should be passed to partial view that does the header
            int ReportingYearID = PdList.Select(x => x.YearFK).Min();
            int ReportingYearVal = Scheduling.Database.Utility.GetYearByID(ReportingYearID);
            ViewBag.ReportingYearVal = ReportingYearVal;
            return View(PdList);

        }
        //set to 7 days
        //disable for now 2/3/2015
        //[OutputCache(Duration = 60 * 60 * 168, Location = OutputCacheLocation.Server, VaryByCustom = "cachetimestamp")]
        public ActionResult ExcelStyleReportsFilterByTimelineRange(FormCollection fc)
        {
            List<ProjectDisplay> PdList = new List<ProjectDisplay>();
            List<int> CurrentPubCodeList = new List<int>();
            string PersistedStart = string.Empty;
            string PersistedStop = string.Empty;

            string StartStr = fc["timelineRangeStart"];
            string StopStr = fc["timelineRangeEnd"];
            string PubCodeStr = fc["pubcode"];

            if (!string.IsNullOrWhiteSpace(StartStr) && !string.IsNullOrWhiteSpace(StopStr))
            {
                Regex RegTest = new Regex("^\\d{4}(-\\d{2})$");

                Match StartMatch = RegTest.Match(StartStr);
                Match StopMatch = RegTest.Match(StopStr);

                if (StartMatch.Success && StopMatch.Success)
                {
                    PdList = Scheduling.Database.Utility.GetDistinctProjectsBasedOnYearAndTimelineMonthRangeOnly(StartStr, StopStr, PubCodeStr);
                    PersistedStart = StartStr;
                    PersistedStop = StopStr;
                }

            }

            ViewBag.PersistedStart = PersistedStart;
            ViewBag.PersistedStop = PersistedStop;

            if (!string.IsNullOrWhiteSpace(PubCodeStr))
            {
                //multiple values
                if (PubCodeStr.Contains(','))
                {
                    string[] sa = PubCodeStr.Split(',');
                    foreach (string s in sa)
                    {
                        if (!string.IsNullOrWhiteSpace(s))
                        {
                            int i = Convert.ToInt32(s);
                            CurrentPubCodeList.Add(i);
                        }
                    }

                    List<int> DistinctList = CurrentPubCodeList;
                    PdList = PdList.Where(x => DistinctList.Contains(x.PubCodeFK.Value)).ToList();

                }
                //one value only
                else
                {

                    PdList = PdList.Where(x => x.PubCodeFK == Scheduling.StringFunctions.Utility.ConvertStringNullValueToNullableInt(PubCodeStr)).ToList();
                    CurrentPubCodeList.Add(Convert.ToInt32(PubCodeStr));
                }




            }

            ViewBag.CurrentPubCodeList = CurrentPubCodeList;
            //filtering complete old method 
            PdList = Scheduling.Sorting.DisplaySorting.SortByPubCodeDescThenYearThenTimeline(PdList);

            return View("ExcelStyleReports", PdList);

        }

        [EO.Pdf.Mvc.RenderAsPDF]
        public ActionResult ExcelStyleReportsPDFNewsStandReportForCirc()
        {
            EO.Pdf.HtmlToPdf.Options.PageSize = new System.Drawing.SizeF(17f, 11f);

            EO.Pdf.HtmlToPdf.Options.OutputArea = new System.Drawing.RectangleF(0.25f, 0.25f, 16.5f, 10.5f);

            List<ProjectDisplay> PdList = (List<ProjectDisplay>)Session["ProjectDisplayResultSet"];

            int ReportYearMin = PdList.Select(y => y.YearFK).Distinct().Min();
            int YearVal = Scheduling.Database.Utility.GetAllYears().Where(x => x.ID == ReportYearMin).First().Value;
            ViewBag.YearVal = YearVal;
            return View(PdList);

        }

        public ActionResult GenerateReportsFiltered(FormCollection fc)
        {
            Scheduling.Models.ReportProjectDisplayViewModel ViewModel= new ReportProjectDisplayViewModel();
            ViewModel.PubCodeSummary = string.Empty;
            List<ReportProjectDisplayViewModelSection> DisplaySectionList = new List<ReportProjectDisplayViewModelSection>();

            List<ProjectDisplay> PdList = new List<ProjectDisplay>();
            List<int> CurrentPubCodeList = new List<int>();

            ViewBag.InitialLoad = "Y";

            //top dropdown 
            if (fc.Count > 0 && !string.IsNullOrEmpty(fc["pubcode"]) && !string.IsNullOrEmpty(fc["year"]))
            {

                int CurrentPubcode = Convert.ToInt32(fc["pubcode"]);
                ViewBag.CurrentPubCode = CurrentPubcode;

                int CurrentYear = Convert.ToInt32(fc["year"]);
                ViewBag.CurrentYearCode = CurrentYear;

                PdList = Scheduling.Database.Utility.GetFilteredProjects(CurrentPubcode, CurrentYear);

                ViewBag.InitialLoad = "N";

            }
            //bottom dropdown
            if (fc.Count > 0 && !string.IsNullOrEmpty(fc["timelineRangeStart"]) && !string.IsNullOrEmpty(fc["timelineRangeEnd"]))
            {
                string PubCodeStr = fc["pubcode-timeline"];
                string StartStr = fc["timelineRangeStart"];
                string StopStr = fc["timelineRangeEnd"];

                ViewBag.InitialLoad = "N";

                Regex RegTest = new Regex("^\\d{4}(-\\d{2})$");

                Match StartMatch = RegTest.Match(StartStr);
                Match StopMatch = RegTest.Match(StopStr);
                //get all by date range
                if (StartMatch.Success && StopMatch.Success)
                {
                    PdList = Scheduling.Database.Utility.GetDistinctProjectsBasedOnYearAndTimelineMonthRangeOnly(StartStr, StopStr, PubCodeStr);
                    ViewBag.PersistedStart = StartStr;
                    ViewBag.PersistedStop = StopStr;
                }

                //start pubcode
                if (!string.IsNullOrWhiteSpace(PubCodeStr))
                {
                    //multiple values
                    if (PubCodeStr.Contains(','))
                    {
                        string[] sa = PubCodeStr.Split(',');
                        foreach (string s in sa)
                        {
                            if (!string.IsNullOrWhiteSpace(s))
                            {
                                int i = Convert.ToInt32(s);
                                CurrentPubCodeList.Add(i);
                            }
                        }

                        List<int> DistinctList = CurrentPubCodeList;
                        PdList = PdList.Where(x => DistinctList.Contains(x.PubCodeFK.Value)).ToList();

                    }
                    //one value only
                    else
                    {

                        PdList = PdList.Where(x => x.PubCodeFK == Scheduling.StringFunctions.Utility.ConvertStringNullValueToNullableInt(PubCodeStr)).ToList();
                        CurrentPubCodeList.Add(Convert.ToInt32(PubCodeStr));
                    }




                }
                //stop pubcode

            }

            if(PdList.Count > 0)
            {
                PdList = Scheduling.Sorting.DisplaySorting.SortByPubCodeDescThenYearThenTimeline(PdList);
                ViewModel.PubCodeSummary = Database.Utility.GeneratePubCodeSummaryReportHeader(PdList.Select(x => x.PubCodeFK).Distinct().ToList());
                DisplaySectionList= Database.Utility.GenerateReportProjectDisplayViewModelSectionListing(PdList);
                
            }
            
            //put into session on view for pdf generation
            ViewModel.ProjectDisplayList = PdList;
            ViewModel.DisplaySections = DisplaySectionList;

            ViewBag.CurrentPubCodeList = CurrentPubCodeList;

            return View(ViewModel);
        }

        public ActionResult GenerateReports(FormCollection fc)
        {
                       

            List<ProjectDisplay> PdList = new List<ProjectDisplay>();
            ViewBag.InitialLoad = "Y";

            if (fc.Count > 0 && !string.IsNullOrEmpty(fc["pubcode"]) && !string.IsNullOrEmpty(fc["year"]))
            {
                         
                int CurrentPubcode = Convert.ToInt32(fc["pubcode"]);
                ViewBag.CurrentPubCode = CurrentPubcode;
                               
                int CurrentYear = Convert.ToInt32(fc["year"]);
                ViewBag.CurrentYearCode = CurrentYear;
                
                PdList = Scheduling.Database.Utility.GetFilteredProjects(CurrentPubcode, CurrentYear);               

                ViewBag.InitialLoad = "N";

            }

           

                //stop
                return View(PdList);
           
        }


        public ActionResult ExcelStyleReports(FormCollection fc)
        {
            List<ProjectDisplay> PdList = Scheduling.Database.Utility.GetAllProjects();

            int? CurrentPubCode, CurrentYearCode, CurrentTimelineCode = null;

            if (!string.IsNullOrWhiteSpace(fc["pubcode"]))
            {
                CurrentPubCode = Convert.ToInt32(fc["pubcode"]);
            }

            //if empty get the pubcode in the webconfig to cut down on load times
            else
            {
                CurrentPubCode = Convert.ToInt32(ConfigurationManager.AppSettings["DefaultPubCode"]);
            }

            PdList = PdList.Where(x => CurrentPubCode == x.PubCodeFK).ToList();
            ViewBag.CurrentPubCode = CurrentPubCode;



            if (!string.IsNullOrWhiteSpace(fc["year"]))
            {
                CurrentYearCode = Convert.ToInt32(fc["year"]);
                PdList = PdList.Where(x => CurrentYearCode == x.YearFK).ToList();
                ViewBag.CurrentYearCode = CurrentYearCode;
            }

            if (!string.IsNullOrWhiteSpace(fc["timeline"]))
            {
                CurrentTimelineCode = Convert.ToInt32(fc["timeline"]);
                PdList = PdList.Where(x => CurrentTimelineCode == x.ProjectRangeFK).ToList();
                ViewBag.CurrentTimelineCode = CurrentTimelineCode;

            }

            //Try ordering here steve c 8/8/14
            PdList = Scheduling.Sorting.DisplaySorting.SortProjectsBasedOnReportingRequirements(PdList);
            return View(PdList);

        }


        //CSV

        public ActionResult ProjectsCreatedCSV()
        {
            string CsvExportDir = Scheduling.StringFunctions.Utility.GetAppSettingValue("CsvExportDirectory");
            string ExpDir = HttpContext.Request.PhysicalApplicationPath + "/" + CsvExportDir;

            string Prefix = Scheduling.StringFunctions.Utility.GetAppSettingValue("ProjectsCreatedCSVFilename");
            string Suffix = DateTime.Now.ToShortDateString().Replace("/", ".");
            string DailyFile = string.Format("{0}-{1}.csv", Prefix, Suffix);

            string CurrentFile = Path.Combine(ExpDir, DailyFile);

            if (!Directory.Exists(ExpDir))
            {
                Directory.CreateDirectory(ExpDir);
            }

            bool FileCreated = false;


            if (System.IO.File.Exists(CurrentFile))
            {

                System.IO.File.Delete(CurrentFile);

            }


            System.IO.File.Create(CurrentFile).Close();
            FileCreated = Scheduling.FileAccess.Utility.GrantFullAccess(CurrentFile);


            //we have the file in the system with permissions for everyone .Now Populate it.
            if (FileCreated)
            {

                List<ProjectDisplay> PdList = Scheduling.Database.Utility.GetProjectsWithStatusOfCreated();
                List<ProjectCreatedCSV> PcList = Scheduling.Csv.Utility.ConvertProjectDisplayToProjectCreatedCSV(PdList);
                IEnumerable<ProjectCreatedCSV> EnumList = PcList as IEnumerable<ProjectCreatedCSV>;


                FileHelpers.FileHelperEngine engine = new FileHelpers.FileHelperEngine(typeof(ProjectCreatedCSV));
                engine.WriteFile(CurrentFile, EnumList);


            }

            return new FilePathResult(CurrentFile, "text/csv") { FileDownloadName = DailyFile };
        }


        //http://www.codeproject.com/Articles/442515/Uploading-and-returning-files-in-ASP-NET-MVC

        public ActionResult ProjectsNewsStandCSV()
        {

            string CsvExportDir = Scheduling.StringFunctions.Utility.GetAppSettingValue("CsvExportDirectory");
            string ExpDir = HttpContext.Request.PhysicalApplicationPath + "/" + CsvExportDir;

            string Prefix = Scheduling.StringFunctions.Utility.GetAppSettingValue("ProjectNewsStandCSVFilename");
            string Suffix = DateTime.Now.ToShortDateString().Replace("/", ".");
            string DailyFile = string.Format("{0}-{1}.csv", Prefix, Suffix);

            string CurrentFile = Path.Combine(ExpDir, DailyFile);

            if (!Directory.Exists(ExpDir))
            {
                Directory.CreateDirectory(ExpDir);
            }

            bool FileCreated = false;


            if (System.IO.File.Exists(CurrentFile))
            {

                System.IO.File.Delete(CurrentFile);

            }


            System.IO.File.Create(CurrentFile).Close();
            FileCreated = Scheduling.FileAccess.Utility.GrantFullAccess(CurrentFile);


            //we have the file in the system with permissions for everyone .Now Populate it.
            if (FileCreated)
            {

                List<ProjectNewstand> PnList = Scheduling.Database.Utility.GetProjectNewstandDates();
                List<ProjectNewstandCSV> PncList = Scheduling.Csv.Utility.ConvertProjectNewstandToProjectNewstandCSV(PnList);
                IEnumerable<ProjectNewstandCSV> EnumList = PncList as IEnumerable<ProjectNewstandCSV>;


                FileHelpers.FileHelperEngine engine = new FileHelpers.FileHelperEngine(typeof(ProjectNewstandCSV));
                engine.WriteFile(CurrentFile, EnumList);


            }

            return new FilePathResult(CurrentFile, "text/csv") { FileDownloadName = DailyFile };

        }

    }
}
