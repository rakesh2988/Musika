using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Musika.Library.Utilities
{
    public static class Helper
    {
        public static DateTime? GetUTCDateTime(this DateTime? dt, string _TimeZone = "Eastern Standard Time")
        {
            try
            {
                DateTime specifiedTime = DateTime.SpecifyKind(dt.Value, DateTimeKind.Unspecified);
                TimeZoneInfo est = TimeZoneInfo.FindSystemTimeZoneById(_TimeZone);
                DateTime someDateTimeInUtc = TimeZoneInfo.ConvertTimeToUtc(specifiedTime, est);
                return someDateTimeInUtc;
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        public static void CreateDirectories(string _dir)
        {
            if (!Directory.Exists(_dir))
            {
                Directory.CreateDirectory(_dir);
            }
        }

        public static void DeleteDirectory(string _dir,bool Subdirectory)
        {
            if (Directory.Exists(_dir))
            {
                Directory.Delete(_dir, Subdirectory);
            }
        }

         public static void DeleteFiles(string _dir)
        {
            if (Directory.Exists(_dir))
            {
                Array.ForEach(Directory.GetFiles(_dir), File.Delete);
            }
        }

         public static List<string> GetFiles(string _dir)
         {
             List<string> _lstFiles = new List<string>();

             if (Directory.Exists(_dir))
             {
                 string[] filespath = Directory.GetFiles(_dir, "*.*",SearchOption.TopDirectoryOnly);
                 foreach (string _file in filespath)
                 {
                     _lstFiles.Add(_file.ToString());
                 }
             }
             return _lstFiles;
         }

       
        public static IEnumerable<T> GetValues<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }
       

        public static string GetTimeAgo(DateTime? dt)
        {
            try
            {
                if (dt.HasValue)
                {
                    TimeSpan span = DateTime.Now - Convert.ToDateTime(dt);
                    if (span.Days > 365)
                    {
                        int years = (span.Days / 365);
                        if (span.Days % 365 != 0)
                            years += 1;
                        return String.Format("{0} {1} ago",
                        years, years == 1 ? "year" : "years");
                    }
                    if (span.Days > 30)
                    {
                        int months = (span.Days / 30);
                        if (span.Days % 31 != 0)
                            months += 1;
                        return String.Format("{0} {1} ago",
                        months, months == 1 ? "month" : "months");
                    }
                    if (span.Days == 1)
                        return String.Format("{1}",
                                         "Yesterday");
                    else if (span.Days > 1)
                        return String.Format("{0} {1} ago",
                        span.Days, span.Days == 1 ? "day" : "days");
                    if (span.Hours > 0)
                        return String.Format("{0} {1} ago",
                        span.Hours, span.Hours == 1 ? "hour" : "hours");
                    if (span.Minutes > 0)
                        return String.Format("{0} {1} ago",
                        span.Minutes, span.Minutes == 1 ? "minute" : "minutes");
                    if (span.Seconds > 5)
                        return String.Format("{0} seconds ago", span.Seconds);
                    if (span.Seconds <= 5)
                        return "just now";
                    return string.Empty;
                }
                else
                {
                    return string.Empty;
                }
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public static string GetControlledCharacterString(string inputString, int requiredLength)
        {
            string str = "";
            if (inputString.Length <= requiredLength)
            {
                str = inputString;
            }
            else
            {
                str = inputString.Substring(0, requiredLength).Insert(requiredLength, "...");
            }
            return str;
        }
        public static DateTime? GetFromDateTime(string datetime)
        {
            DateTime _value;
            if (DateTime.TryParse(datetime, out _value))
            {
                DateTime _date = Convert.ToDateTime(datetime + " 00:00:00");
                return _date;
            }
            else
                return null;
        }

        public static DateTime? GetToDateTime(string datetime)
        {
            DateTime _value;
            if (DateTime.TryParse(datetime, out _value))
            {
                DateTime _date = Convert.ToDateTime(datetime + " 23:59:59");
                return _date;
            }
            else
                return null;
        }

        public static string CreateImageFromEncodedString(string encodedImage, string Path)
        {
            try
            {
                var base64String = Convert.ToString(encodedImage);
                // Convert Base64 String to byte[]
                var imageBytes = Convert.FromBase64String(base64String);
                var ms = new MemoryStream(imageBytes, 0, imageBytes.Length);
                // Convert byte[] to Image
                ms.Write(imageBytes, 0, imageBytes.Length);
                var image = new Bitmap(ms);
                var photoName = Guid.NewGuid().ToString() + ".png";
                Path = Path + photoName;
                image.Save(Path);
                return photoName;
            }
            catch (Exception ex)
            {
                return "Error";
            }
        }

        public static DateTime EndOfDay(DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day, 23, 59, 59, 999);
        }

        public static DateTime StartOfDay(DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, 0);
        }
        //public static string ExtractHtmlInnerText(string htmlText)
        //{
        //    //Match any Html tag (opening or closing tags) 
        //    // followed by any successive whitespaces
        //    //consider the Html text as a single line

        //    //Regex regex = new Regex("(<.*?>\\s*)+", RegexOptions.Singleline);
        //    Regex regex = new Regex(@"<[^>]+>|&nbsp;", RegexOptions.Singleline);

        //    // replace all html tags (and consequtive whitespaces) by spaces
        //    // trim the first and last space

        //    string resultText = regex.Replace(htmlText, "").Trim();

        //    // string str1 = System.Text.RegularExpressions.Regex.Replace(htmlText, "(<[a|A][^>]*>|)", "");
        //    return resultText;
        //}
        public static DateTime GetStartOfWeek(DateTime date)
        {
            int DaysToSubtract = (int)date.DayOfWeek;
            DateTime dt = date.Subtract(TimeSpan.FromDays(DaysToSubtract));
            return new DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0, 0);
        }
        public static DateTime FirstDayOfMonth(DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, 1);
        }
        public static DateTime LastDayOfMonth(DateTime dateTime)
        {
            DateTime firstDayOfTheMonth = new DateTime(dateTime.Year, dateTime.Month, 1);
            return firstDayOfTheMonth.AddMonths(1).AddDays(-1);
        }
        public static bool IsValidEmail(string emailAddress)
        {
            string patternStrict = @"^(([^<>()[\]\\.,;:\s@\""]+"
                  + @"(\.[^<>()[\]\\.,;:\s@\""]+)*)|(\"".+\""))@"
                  + @"((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}"
                  + @"\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+"
                  + @"[a-zA-Z]{2,}))$";
            Regex reStrict = new Regex(patternStrict);
            bool isStrictMatch = reStrict.IsMatch(emailAddress);
            return isStrictMatch;
        }

        public static bool IsValidPattern(string vToValidate,string vRegex)
        {
            string patternStrict = vRegex;
            Regex reStrict = new Regex(patternStrict);
            bool isStrictMatch = reStrict.IsMatch(vToValidate);
            return isStrictMatch;
        }

        public static Dictionary<string, bool> UserSetting()
        {
            var list = new Dictionary<string, bool>();
            list.Add("Notification", true);
            list.Add("SendEmail", true);
            return list;
        }
        public static string GetUserSettingString(string settingKey)
        {
            var retString = "";
            switch (settingKey)
            {
                case "Notification":
                    retString = "Notification";
                    break;
                case "SendEmail":
                    retString = "Send Email";
                    break;
                default:
                    break;
            }
            return retString;
        }

        public static string TitleCase(string value)
        {
            string titleString = ""; // destination string, this will be returned by function
            if (!String.IsNullOrEmpty(value))
            {
                string[] lowerCases = new string[13] { "of", "the", "in", "a", "an", "to", "and", "at", "from", "by", "on", "or", "is" }; // list of lower case words that should only be capitalised at start and end of title
                string[] specialCases = new string[7] { "UK", "USA", "IRS", "UCLA", "PHd", "UB40a", "MSc" }; // list of words that need capitalisation preserved at any point in title
                string[] words = value.ToLower().Split(' ');
                bool wordAdded = false; // flag to confirm whether this word appears in special case list
                int counter = 1;
                foreach (string s in words)
                {
                    // check if word appears in lower case list
                    foreach (string lcWord in lowerCases)
                    {
                        if (s.ToLower() == lcWord)
                        {
                            // if lower case word is the first or last word of the title then it still needs capital so skip this bit.
                            if (counter == 0 || counter == words.Length) { break; };
                            titleString += lcWord;
                            wordAdded = true;
                            break;
                        }
                    }

                    // check if word appears in special case list
                    foreach (string scWord in specialCases)
                    {
                        if (s.ToUpper() == scWord.ToUpper())
                        {
                            titleString += scWord;
                            wordAdded = true;
                            break;
                        }
                    }

                    if (!wordAdded)
                    { // word does not appear in special cases or lower cases, so capitalise first letter and add to destination string
                        titleString += char.ToUpper(s[0]) + s.Substring(1).ToLower();
                    }
                    wordAdded = false;

                    if (counter < words.Length)
                    {
                        titleString += " "; //dont forget to add spaces back in again!
                    }
                    counter++;
                }
            }
            return titleString;
        }
    }
}
