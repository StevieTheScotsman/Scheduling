using System.Collections.Generic;
using Scheduling.Models;

namespace Scheduling.Html
{
    public partial class Utility
    {
        public List<ProjectHistory>GenerateProjectHistoryFromProjectID(int i)
        {
            return Scheduling.Database.Utility.GetProjectHistoryFromProjectID(i);
            
        }
    }
}