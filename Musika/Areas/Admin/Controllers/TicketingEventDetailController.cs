using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Musika.Areas.Admin.Controllers
{
    public class TicketingEventDetailController : Controller
    {
        // GET: Admin/TicketingEventDetail
        public ActionResult Index(Int32 EventID)
        {
            ViewBag.eventid = EventID;
            DataSet data;
            string count = "";
            data = new Musika.Repository.SPRepository.SpRepository().GetTempEventByEventId(EventID);
            if (data.Tables[0].Rows.Count > 0)
            {
                count = data.Tables[0].Rows[0][0].ToString();

            }
            else
            {
                count = data.Tables[0].Rows[0][0].ToString();

            }
            ViewBag.isapproval = count;
            return View();
        }
    }
}