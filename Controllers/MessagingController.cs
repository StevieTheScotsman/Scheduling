using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Scheduling.Models;

namespace Scheduling.Controllers
{
    public class MessagingController : Controller
    {

        public ActionResult ListMessagingSettings()
        {
            List<MessagingSetting> MsList = Scheduling.Database.Utility.GetAllMessagingSettings();
            List<MessagingSettingDisplay> MsDisList = Scheduling.CastingFunctions.Utility.ConvertMessageSettingToMessageDisplaySetting(MsList);
            return View(MsDisList);
        }

        public ActionResult ManageMessagingEvents()
        {

            List<Scheduling.Models.MessagingEvent> EventList = Scheduling.Database.Utility.GetAllMessagingEvents().OrderBy(x=>x.Method).ToList();
            return View(EventList);
        }


        public ActionResult ManageMessagingSettingsRemoveSingleEntry(FormCollection fc)
        {

            int CurrentID = Convert.ToInt32(fc["id"]);
            Scheduling.Database.Utility.RemoveSingleMessagingSettingEntry(CurrentID);
            return RedirectToAction("ManageMessagingSettings");

        }

        public ActionResult ManageMessagingEventsAddEntry(FormCollection fc)
        {
            bool HasShortDesc = !string.IsNullOrWhiteSpace(fc["ShortDesc"]);
            bool HasMethod = !string.IsNullOrWhiteSpace(fc["method"]);

            if (HasShortDesc && HasMethod)
            {

                Scheduling.Database.Utility.CreateMessagingEventEntry(fc["method"], fc["ShortDesc"], fc["LongDesc"]);

            }

            else
            {

                ModelState.AddModelError("method", "Short Desc and Method Are Required");

            }

            List<Scheduling.Models.MessagingEvent> EventList = Scheduling.Database.Utility.GetAllMessagingEvents().ToList();
            return View("ManageMessagingEvents", EventList);


        }

        
        public ActionResult ManageMessagingEventsRemoveEntry(FormCollection fc)
        {
           int CurrentID = Convert.ToInt32(fc["id"]);
           Scheduling.Database.Utility.RemoveMessagingEventAlongWithSettings(CurrentID);

            return RedirectToAction("ManageMessagingEvents");


        }

        public ActionResult ProcessManageMessagingSettings(FormCollection fc)
        {

            //Get Event and Action Current no multiple selection on control
            int CurrentEvent = Convert.ToInt32(fc["event"]);
            int CurrentAction = Convert.ToInt32(fc["action"]);

            bool HaveUserEntry = !string.IsNullOrWhiteSpace(fc["users"]);
            bool HaveRoleEntry = !string.IsNullOrWhiteSpace(fc["roles"]);
            bool HaveGroupEntry = !string.IsNullOrWhiteSpace(fc["groups"]);
            bool HaveDeptEntry = !string.IsNullOrWhiteSpace(fc["depts"]);

            //since we are going back to the view we need to generate the view data.
            if (!HaveUserEntry && !HaveRoleEntry && !HaveGroupEntry && !HaveDeptEntry)
            {
                ModelState.AddModelError("users", "At Least One Selection Is Required");


                List<MessagingSetting> MsList = Scheduling.Database.Utility.GetAllMessagingSettings();
                List<MessagingSettingDisplay> MsDisList = Scheduling.CastingFunctions.Utility.ConvertMessageSettingToMessageDisplaySetting(MsList);

                List<MessagingAction> ActionList = Scheduling.Database.Utility.GetAllMessagingActions();
                List<MessagingEvent> EventList = Scheduling.Database.Utility.GetAllMessagingEvents();

                List<User> UserList = Scheduling.Database.Utility.GetAllUsers().OrderBy(x => x.UserName).ToList();
                List<Group> GroupList = Scheduling.Database.Utility.GetAllGroups().OrderBy(x => x.Description).ToList();
                List<Role> RoleList = Scheduling.Database.Utility.GetAllRoles().OrderBy(x => x.ShortDesc).ToList();
                List<Department> DepList = Scheduling.Database.Utility.GetAllDepartments().OrderBy(x => x.Description).ToList();


                ViewBag.UserList = UserList;
                ViewBag.GroupList = GroupList;
                ViewBag.RoleList = RoleList;
                ViewBag.DepList = DepList;
                ViewBag.EventList = EventList;
                ViewBag.ActionList = ActionList;
                ViewBag.DepList = DepList;

                return View("ManageMessagingSettings", MsDisList);

            }

            else
            {

                //delete current settings 
                Scheduling.Database.Utility.RemoveMessagingSettingsEntriesBasedOnEventAndAction(CurrentEvent, CurrentAction);

                //User Logic


                if (HaveUserEntry)
                {
                    string CurrentUsers = fc["users"];
                    List<string> UserList = Scheduling.StringFunctions.Utility.GetStringListFromStringWithPossibleCommaSeperator(CurrentUsers);
                    Scheduling.Database.Utility.CreateEventActionUserMessagingSettingEntry(CurrentEvent, CurrentAction, UserList);

                }

                //Role Logic


                if (HaveRoleEntry)
                {
                    string CurrentRoles = fc["roles"];
                    List<string> RoleList = Scheduling.StringFunctions.Utility.GetStringListFromStringWithPossibleCommaSeperator(CurrentRoles);
                    Scheduling.Database.Utility.CreateEventActionRoleMessagingSettingEntry(CurrentEvent, CurrentAction, RoleList);
                }

                //group logic 


                if (HaveGroupEntry)
                {
                    string CurrentGroups = fc["groups"];
                    List<string> GroupList = Scheduling.StringFunctions.Utility.GetStringListFromStringWithPossibleCommaSeperator(CurrentGroups);
                    Scheduling.Database.Utility.CreateEventActionGroupMessagingSettingEntry(CurrentEvent, CurrentAction, GroupList);

                }


                //dept logic

                if (HaveDeptEntry)
                {
                    string CurrentDepts = fc["depts"];
                    List<string> DepList = Scheduling.StringFunctions.Utility.GetStringListFromStringWithPossibleCommaSeperator(CurrentDepts);
                    Scheduling.Database.Utility.CreateEventActionDeptMessagingSettingEntry(CurrentEvent, CurrentAction, DepList);

                }


                return RedirectToAction("ManageMessagingSettings", "Messaging");


            }

        }

        public ActionResult ManageMessagingSettings()
        {
            List<MessagingSetting> MsList = Scheduling.Database.Utility.GetAllMessagingSettings().OrderBy(x=>x.EventFK).ToList();
            List<MessagingSettingDisplay> MsDisList = Scheduling.CastingFunctions.Utility.ConvertMessageSettingToMessageDisplaySetting(MsList);

            List<MessagingAction> ActionList = Scheduling.Database.Utility.GetAllMessagingActions();
            List<MessagingEvent> EventList = Scheduling.Database.Utility.GetAllMessagingEvents();

            List<User> UserList = Scheduling.Database.Utility.GetAllUsers().OrderBy(x => x.UserName).ToList();
            List<Group> GroupList = Scheduling.Database.Utility.GetAllGroups().OrderBy(x => x.Description).ToList();
            List<Role> RoleList = Scheduling.Database.Utility.GetAllRoles().OrderBy(x => x.ShortDesc).ToList();
            List<Department> DepList = Scheduling.Database.Utility.GetAllDepartments().OrderBy(x => x.Description).ToList();


            ViewBag.UserList = UserList;
            ViewBag.GroupList = GroupList;
            ViewBag.RoleList = RoleList;
            ViewBag.DepList = DepList;
            ViewBag.EventList = EventList;
            ViewBag.ActionList = ActionList;
            ViewBag.DepList = DepList;
            return View(MsDisList);

        }



    }
}
