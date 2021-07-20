using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace accpagibigph3srv_sftp_monitoring
{
    class Utilities
    {
        public static string DbaseConStr = "";

        public static string TimeStamp()
        {
            return DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss tt") + " ";
        }

        public static void InitLogFolder()
        {
            if (!System.IO.Directory.Exists("Logs"))
                System.IO.Directory.CreateDirectory("Logs");
            if (!System.IO.Directory.Exists(@"Logs\" + DateTime.Now.ToString("MMddyyyy")))
                System.IO.Directory.CreateDirectory(@"Logs\" + DateTime.Now.ToString("MMddyyyy"));
        }

        public static void SaveToErrorLog(string strData)
        {
            InitLogFolder();
            try
            {
                System.IO.StreamWriter sw = new System.IO.StreamWriter(@"Logs\" + DateTime.Now.ToString("MMddyyyy") + @"\Error.txt", true);
                sw.WriteLine(strData);
                sw.Dispose();
                sw.Close();
            }
            catch (Exception ex)
            {
            }
        }

        public static void SaveToSystemLog(string strData)
        {
            InitLogFolder();
            try
            {
                System.IO.StreamWriter sw = new System.IO.StreamWriter(@"Logs\" + DateTime.Now.ToString("MMddyyyy") + @"\System.txt", true);
                sw.WriteLine(strData);
                sw.Dispose();
                sw.Close();
            }
            catch (Exception ex)
            {
            }
        }
    }
}
