using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Musika.Common
{
    public class ErrorLog
    {
        public static void WriteErrorLog(Exception ex)
        {
            string webPageName = Path.GetFileName(HttpContext.Current.Request.Path);
            string errorLogFilename = "ErrorLog_" + DateTime.Now.ToString("dd-MM-yyyy") + ".txt";
            string folderPath = HttpContext.Current.Server.MapPath("~/ErrorLogFiles");
            bool exists = System.IO.Directory.Exists(folderPath);

            if (!exists)
                System.IO.Directory.CreateDirectory(folderPath);
            string filePath = HttpContext.Current.Server.MapPath("~/ErrorLogFiles/" + errorLogFilename);
            if (File.Exists(filePath))
            {
                using (StreamWriter stwriter = new StreamWriter(filePath, true))
                {
                    stwriter.WriteLine("-------------------Error Log Start-----------as on " + DateTime.Now.ToString("hh:mm tt"));
                    stwriter.WriteLine("WebPage Name :" + webPageName);
                    stwriter.WriteLine("Message:" + ex.ToString());
                    stwriter.WriteLine("-------------------End----------------------------");
                }
            }
            else
            {
                StreamWriter stwriter = File.CreateText(filePath);
                
                stwriter.WriteLine("-------------------Error Log Start-----------as on " + DateTime.Now.ToString("hh:mm tt"));
                stwriter.WriteLine("WebPage Name :" + webPageName);
                stwriter.WriteLine("Message: " + ex.ToString());
                stwriter.WriteLine("-------------------End----------------------------");
                stwriter.Close();
            }
        }
    }
}