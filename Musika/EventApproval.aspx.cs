using Musika.Library.Utilities;
using Musika.Models;
using Musika.Repository.GRepository;
using Musika.Repository.Interface;
using Musika.Repository.UnitofWork;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Musika
{
    public partial class EventApproval : System.Web.UI.Page
    {
        private readonly IUnitOfWork _unitOfWork;

        public EventApproval()
        {
            _unitOfWork = new UnitOfWork();
           
        }
        protected void Page_Load(object sender, EventArgs e)
        {
           
            if (!this.IsPostBack)
            {
                Approve();


            }
        }
        public void Approve()
        {
            DataSet ds;
            DataSet ds1;
            Models.TicketingEventsNew _Events = null;
            int EventID = Convert.ToInt32(Request.QueryString["ID"]);
            if (EventID > 0)
            {
                ds = new Musika.Repository.SPRepository.SpRepository().ShiftDatetoOriginalTables(EventID);
                GenericRepository<TicketingEventsNew> _EventsRepo = new GenericRepository<TicketingEventsNew>(_unitOfWork);
                _Events = _EventsRepo.Repository.GetById(EventID);

                bool tourdata = new Musika.Repository.SPRepository.SpRepository().SpUpdateTourData(_Events.ArtistId, _Events.VenueName, _Events.StartDate, _Events.EventTitle, _Events.EventID);
               // if (ds.Tables[0].Rows.Count > 0)
              //  {
                    ds1 = new Musika.Repository.SPRepository.SpRepository().SpGetTicketingEventUsersToSendEmail(EventID);
                    if (ds1.Tables[0].Rows.Count > 0)
                    {
                        DataRow dr = ds1.Tables[0].Rows[0];
                        ltMessage.Text = "Event Approved successfully.";
                        string html = string.Empty;
                        string Email = dr["Email"].ToString();
                        html = "<p>Hi " + dr["UserName"].ToString() + "," + " </p>";
                        html += "<p>Your Event Changes Has been approved by Admin." + "</p>";
                        html += "<p><br>You can view your changes in your panel" + "<p>";
                        //  html += "<p><br>User Name : " + dr["UserName"].ToString() + "<p>";
                        html += "<p><br><br><strong>Thanks,<br><br>The " + WebConfigurationManager.AppSettings["AppName"] + " Team</strong></p>";
                        SendEmailHelper.SendMail(Email, "Event Changes Approved", html, "");
                    }
               // }
                else
                {
                    ltMessage.Text = "Invalid Activation code.";
                }
            }
        }
    }
}