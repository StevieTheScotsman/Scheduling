using System.Collections.Generic;

namespace Scheduling.Event
{
    public class Utility
    {

        public static bool CanEventBeDeleted(int EventID)
        {

            string NonDelEventStr = Scheduling.StringFunctions.Utility.GetAppSettingValue("NonDeletableEventIDS");
            bool CanDelete = true;

            if (!string.IsNullOrWhiteSpace(NonDelEventStr))
            {

                List<string> NonDeletableEvents = Scheduling.StringFunctions.Utility.GetStringListFromStringWithPossibleCommaSeperator(NonDelEventStr);

                foreach (string item in NonDeletableEvents)
                {

                    if (item ==EventID.ToString())
                    {

                        CanDelete = false;
                    }


                }
            }

            return CanDelete;

        }
    }
}