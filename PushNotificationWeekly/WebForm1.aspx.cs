using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Musika;
using Musika.Models;
using Musika.Library;
using Musika.Controllers.API;

namespace PushNotificationWeekly
{
    public partial class WebForm1 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                try
                {
                    Button1_Click(null, null);
                }
                catch (Exception ee)
                {

                }
            }
        }

        public bool GetMonday(DateTime time)
        {
            if (time.DayOfWeek == DayOfWeek.Monday)
            {
                return true;
            }
            else
            {
                return false;
            }            
        }

        public DateTime SetDateTime(DateTime dt)
        {
            DateTime dt1 = Convert.ToDateTime("01/01/1900");
            if (dt.DayOfWeek == DayOfWeek.Monday)
            {
                DateTime dateNow = dt;
                var DailyTime = "18:00:00";
                var timeParts = DailyTime.Split(new char[1] { ':' });
                var date = new DateTime(dateNow.Year, dateNow.Month, dateNow.Day, int.Parse(timeParts[0]), int.Parse(timeParts[1]), int.Parse(timeParts[2]));
                return date;
            }            
            else
            {
                return dt1;
            }
        }

        protected void Timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                APIV2Controller myController = new APIV2Controller("test");
                DateTime temp = SetDateTime(DateTime.Now);
                if (SetDateTime(DateTime.Now) != Convert.ToDateTime("01/01/1900"))
                {
                    myController.SendNotificationNew();
                    //Timer1.Interval = 86400000 * 7;
                }
            }
            catch (Exception ex)
            {

            }
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            Timer1_Tick(null,null);
        }
    }
}