using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Web;
using System;

namespace Scheduling.FileAccess
{
    public class Utility
    {

        public static bool GrantFullAccess(string fullPath)
        {
            DirectoryInfo dInfo = new DirectoryInfo(fullPath);
            DirectorySecurity dSecurity = dInfo.GetAccessControl();
            dSecurity.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.NoPropagateInherit, AccessControlType.Allow));
            dInfo.SetAccessControl(dSecurity);
            return true;
        }

        public static void UpdateCacheFile()
        {
            string path = Path.Combine(HttpContext.Current.Request.PhysicalApplicationPath, string.Format("{0}{1}{2}", "Caching", Path.DirectorySeparatorChar, "CacheTimestamp.txt"));
            string CacheStamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            bool FileCreated = false;

            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }

            System.IO.File.Create(path).Close();
            FileCreated = Scheduling.FileAccess.Utility.GrantFullAccess(path);

            if (FileCreated)
            {
                using (StreamWriter sw = new StreamWriter(path))
                {
                    sw.Write(CacheStamp);
                }

            }


            using (StreamReader sr = new StreamReader(path))
            {
                CacheStamp = sr.ReadToEnd().Trim();

            }
        }


        public static string ReadCacheFile()
        {
            string path = Path.Combine(HttpContext.Current.Request.PhysicalApplicationPath, string.Format("{0}{1}{2}", "Caching", Path.DirectorySeparatorChar, "CacheTimestamp.txt"));

            string CacheStamp = string.Empty;

            using (StreamReader sr = new StreamReader(path))
            {
                CacheStamp = sr.ReadToEnd().Trim();

            }

            return CacheStamp;

        }

        [Obsolete]
        public static void WriteToLogFile(string input)
        {
            string path = Path.Combine(HttpContext.Current.Request.PhysicalApplicationPath, string.Format("{0}{1}{2}", "Logging", Path.DirectorySeparatorChar, "logging.txt"));

            //for now just recreate todo ..whack if older than 30 days.

            bool FileCreated = false;

            if (System.IO.File.Exists(path))
            {

                System.IO.File.Delete(path);

            }

            System.IO.File.Create(path).Close();
            FileCreated = Scheduling.FileAccess.Utility.GrantFullAccess(path);

            if (FileCreated)
            {
                using (StreamWriter sw = new StreamWriter(path))
                {
                    sw.WriteLine(input);
                }

            }



        }
    }
}