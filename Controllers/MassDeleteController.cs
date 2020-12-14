using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Scheduling.Models;

namespace ProductionSchedule.Controllers
{
    public class MassDeleteController : Controller
    {
        //
        // GET: /Deletion/

        public ActionResult LoadProjectsFromBaseline(string id)
        {
            int i =Convert.ToInt32(id);
            List<ProjectDisplay> RetList = Scheduling.Database.Utility.GetAllProjects().Where(x => x.MilestoneTreeSettingsProfileFK == i).ToList();
            ViewBag.ID = i;
            return View(RetList);
        }

        [HttpPost]
        public ActionResult DeleteProjects(FormCollection fc)
        {

            string Projects = fc["ProjectsToDelete"];

            List<int> ProjectsToDelete = new List<int>();

            if(Projects.Contains(','))
            {
                string[] StrArray = Projects.Split(',');

                foreach(string s in StrArray)
                {
                    ProjectsToDelete.Add(Convert.ToInt32(s));

                }
            }

            else
            {
                ProjectsToDelete.Add(Convert.ToInt32(Projects));

            }

            foreach(int i in ProjectsToDelete)
            {
                Scheduling.Database.Utility.DeleteProjectAndAllProjectInformationByID(i);

            }

            return RedirectToAction("ManageMilestoneTreeSettingProfiles", "Home");
        }

    }
}
