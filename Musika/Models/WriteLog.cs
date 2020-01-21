using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Musika.Models
{
    public class WriteLog
    {

        public static bool WriteLogFile(string strMessage)
        {
            try
            {
                //string path= HttpContext.Current.Server.MapPath("Log");
                // FileStream objFilestream = new FileStream(string.Format("{0}\\{1}", path, "Log.txt"), FileMode.Append, FileAccess.Write);
                // StreamWriter objStreamWriter = new StreamWriter((Stream)objFilestream);
                // objStreamWriter.WriteLine(strMessage);
                // objStreamWriter.Close();
                // objFilestream.Close();
                string path = AppDomain.CurrentDomain.BaseDirectory + "\\Log";
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                string filepath = AppDomain.CurrentDomain.BaseDirectory + "\\Log\\PaymentLog_" + DateTime.Now.Date.ToShortDateString().Replace('/', '_') + ".txt";
                if (!File.Exists(filepath))
                {
                    // Create a file to write to.   
                    using (StreamWriter sw = File.CreateText(filepath))
                    {
                        sw.WriteLine(strMessage);
                    }
                }
                else
                {
                    using (StreamWriter sw = File.AppendText(filepath))
                    {
                        sw.WriteLine(strMessage);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

    }
}