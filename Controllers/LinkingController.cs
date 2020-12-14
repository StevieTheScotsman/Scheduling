using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Scheduling.Database;
using Scheduling.Models;

namespace Scheduling.Controllers
{
    public class LinkingController : Controller
    {
        public ActionResult ListLinkSettings()
        {
            List<ProjectLinkSetting> SettingList = Utility.GetAllProjectLinkSettings();
            List<ProjectLinkSettingDisplay> DisList = Scheduling.CastingFunctions.Utility.ConvertProjectLinkSettingToDisplay(SettingList);

            return View(DisList);

        }

        public ActionResult ManageLinkSettings()
        {
            List<ProjectLinkSetting> SettingList = Utility.GetAllProjectLinkSettings();
            List<ProjectLinkSettingDisplay> DisList = Scheduling.CastingFunctions.Utility.ConvertProjectLinkSettingToDisplay(SettingList);

            return View(DisList);

        }

        public ActionResult DeleteSingleProjectLinkEntry(int id)
        {
            Scheduling.Database.Utility.DeleteSingleProjectLinkAndKeepValues(id);
            return RedirectToAction("ManageLinkedProjects");

        }

        public ActionResult DeleteSingleProjectLinkEntryAndRemoveSecondaryValues(int id)
        {
            Scheduling.Database.Utility.DeleteSingleProjectLinkAndResetValues(id);            
            return RedirectToAction("ManageLinkedProjects");

        }




        public ActionResult ProcessAddLinkSetting(ProjectLinkSetting pls)
        {
            if (pls.PrimaryProfileTypeID == pls.SecondaryProfileTypeID)
            {
                ModelState.AddModelError("SecondaryProfileTypeID","Primary(Source) and Target(Secondary) profiles need to be DIFFERENT");

            }

            
            if(ModelState.IsValid)
            {

                Scheduling.Database.Utility.CreateNewLinkSetting(pls);
                return RedirectToAction("ManageLinkSettings");
            }

            else
            {

                List<PublicationCode> PubCodeList = Scheduling.Database.Utility.GetAllPublicationCodes(); ViewBag.PubCodeList = PubCodeList;
                List<MilestoneField> ParentMilestoneList = Scheduling.Database.Utility.GetAllParentMilestoneFields(); ViewBag.ParentMilestoneList = ParentMilestoneList;
                List<ProjectProfileType> ProjectProfileTypeList = Scheduling.Database.Utility.GetAllProjectProfileTypes(); ViewBag.ProjectProfileTypeList = ProjectProfileTypeList;
                List<Timeline> Timelines = Scheduling.Database.Utility.GetAllProjectRanges(); ViewBag.Timelines = Timelines;
                List<Calculation> Calculations = Scheduling.Database.Utility.GetAllCalculationFields(); ViewBag.Calculations = Calculations;
                return View("AddLinkSetting");
            }

            
            
        }

        public ActionResult AddLinkSetting()
        {
            
            List<PublicationCode> PubCodeList = Scheduling.Database.Utility.GetAllPublicationCodes();  ViewBag.PubCodeList = PubCodeList;
            List<MilestoneField>  ParentMilestoneList = Scheduling.Database.Utility.GetAllParentMilestoneFields(); ViewBag.ParentMilestoneList = ParentMilestoneList;
            List<ProjectProfileType> ProjectProfileTypeList = Scheduling.Database.Utility.GetAllProjectProfileTypes(); ViewBag.ProjectProfileTypeList = ProjectProfileTypeList;
            List<Timeline> Timelines = Scheduling.Database.Utility.GetAllProjectRanges(); ViewBag.Timelines = Timelines;
            List<Calculation> Calculations = Scheduling.Database.Utility.GetAllCalculationFields(); ViewBag.Calculations = Calculations;
            return View();
        }

        public ActionResult DeleteSingleLinkSetting(FormCollection fc)
        {
            int CurrentSetting = Convert.ToInt32(fc["id"]);
            Scheduling.Database.Utility.DeleteSingleLinkSetting(CurrentSetting);
            return RedirectToAction("ManageLinkSettings");
        }

        public ActionResult ManageSingleProjectLinking(FormCollection fc)
        {

            int CurrentProject = Convert.ToInt32(fc["id"]);
            ViewBag.CurrentProject = CurrentProject;
            List<ProjectLinkSetting> Settinglist = Scheduling.Database.Utility.GetProjectLinkSettingsByProjectID(CurrentProject);
            return View(Settinglist);

        }

        public ActionResult   ManageLinkedProjects()
        {
           List<ProjectLink> ProjList=Scheduling.Database.Utility.GetAllProjectLinks();
           List<ProjectLinkDisplay> RetList=Scheduling.CastingFunctions.Utility.ConvertProjectLinkToDisplay(ProjList);
           return View(RetList);
        }

      
        //Verify not sure if this is still used.
        public ActionResult ProcessManageSingleProjectLinking(FormCollection fc)
        {

            if(Scheduling.StringFunctions.Utility.GetAppSettingValue("LinkAndCalculate").ToLower()=="false")
            {
                Scheduling.Linking.Utility.ManageSingleProjectLinkingAddAndCreateInitialTopLevelEntry(fc);
            }

            else 

            {
                Scheduling.Linking.Utility.ManageSingleProjectLinkingAddAndCreateAllEntries(fc);
            }

            int RetID = Convert.ToInt32(fc["current-project"]);
            return RedirectToAction("ManageSingleProjectAfterDependencyAjaxCall", "Home", new { id = RetID, message = string.Empty });

        }
     
    }
}
