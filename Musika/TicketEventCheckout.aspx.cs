using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Musika.Library.Utilities;
using System.IO;
using Musika.Models;
using System.Net.Mail;
using System.Net.Mime;
using System.Data;
using System.Text;
//using System.Globalization;
using System.Threading;
using Musika.Repository.SPRepository;
using System.Net.Http;
using Newtonsoft.Json;
using System.Web.Configuration;
using Musika.Common;

namespace Musika
{
    public partial class TicketEventCheckout1 : System.Web.UI.Page
    {
        int userId; int tourDateId; string deviceId; string ticketType; decimal price = 0.0M; int qty; string package;
        string currency; public string username;
        decimal CouponDiscount = 0.0M;
        int CouponID;
        string language = "es-ES";
        MusikaEntities db = new MusikaEntities();
        Musika.Models.UsersModel user = new UsersModel();

        protected override void InitializeCulture()
        {

            if (Request.QueryString.AllKeys.Contains("lang"))
            //if (string.IsNullOrEmpty(Request.QueryString["lang"].ToString()))
            {
                language = Request.QueryString["lang"].ToString();

                if (language.ToLower() == "es")
                {
                    language = "es-ES";
                    Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(language);
                    Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(language);
                }
                else if (language.ToLower() == "en")
                {
                    language = "en-EN";
                    Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(language);
                    Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(language);
                }
                else
                {
                    //Set the Culture.
                    Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(language);
                    Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(language);
                }
            }
            else
            {
                //Set the Culture.
                Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(language);
                Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(language);
            }
            if (Request.QueryString.AllKeys.Contains("Disc"))
            {
                CouponDiscount = Convert.ToDecimal(Request.QueryString["Disc"].ToString(), System.Globalization.CultureInfo.InvariantCulture);
            }
            if (Request.QueryString.AllKeys.Contains("CId"))
            {
                CouponID = Convert.ToInt32(Request.QueryString["CId"].ToString());
            }

        }
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                try
                {

                    ClientScript.RegisterStartupScript(this.GetType(), "data", "bindData();", true);
                    userId = Convert.ToInt32(Request.QueryString["userId"].ToString());
                    tourDateId = Convert.ToInt32(Request.QueryString["tourDateId"].ToString());
                    deviceId = Convert.ToString(Request.QueryString["deviceId"].ToString());
                    ticketType = Convert.ToString(Request.QueryString["ticketType"].ToString());
                    if (Request.QueryString["Price"] != "")
                    {
                        price = Convert.ToDecimal(Request.QueryString["Price"], System.Globalization.CultureInfo.InvariantCulture);//Convert.ToDecimal(Request.QueryString["Price"].ToString());
                    }
                    else
                    {
                        price = 0;
                    }
                    qty = Convert.ToInt32(Request.QueryString["qty"].ToString());
                    package = Convert.ToString(Request.QueryString["package"].ToString());
                    currency = Convert.ToString(Request.QueryString["currency"].ToString());
                    var query = (from A in db.TourDate
                                 join B in db.Artists on A.ArtistID equals B.ArtistID into AB
                                 from B in AB.DefaultIfEmpty()
                                 join C in db.TicketingEventsNew on A.TicketingEventID equals C.EventID into AC
                                 from C in AC.DefaultIfEmpty()
                                 where A.TourDateID == tourDateId
                                 select new
                                 {
                                     artistname = B.ArtistName,
                                     eventName = A.EventName,
                                     EventStartDate = A.Tour_Utcdate,
                                     EventStartTime = C.StartTime
                                 }).ToList();
                    BindExpirationYear();

                    if (currency == "DOP")
                    {
                        currency = "RD$";
                    }
                    else
                    {
                        currency = "$";
                    }

                    lblPackage.Text = package;
                    if (query.Count > 0)
                    {
                        lblArtistName.Text = query[0].artistname;
                        lblEventName.Text = query[0].eventName;
                        lblStartDate.Text = Convert.ToDateTime(query[0].EventStartDate).ToLongDateString();
                        lblTime.Text = query[0].EventStartTime;
                    }

                    lblNumbers.Text = qty.ToString();
                    if (Request.QueryString["Price"] != "")
                    {
                        lblTotalPrice.Text = currency + " " + Request.QueryString["Price"].ToString();
                    }
                    else
                    {
                        lblTotalPrice.Text = currency + " " + price;
                    }
                    //txtPriceCurrency.Text = currency + " " + price.ToString();
                    creditDiv.Visible = true;
                    creditDiv1.Visible = true;
                    country.Visible = true;
                    paypalDiv.Visible = true;
                    divPay.Visible = false;
                    //paypalSpan.Attributes.Add("class", "active");

                    // btnAcceptCard.Visible = false;
                    if (ticketType.ToLower() == "free")
                    {
                        hdnTab.Value = "";
                        btnSubmit.Visible = true;
                        lblMessage2.Visible = false;
                        divPay.Visible = false;
                        navigation.Visible = false;
                        //  btnpayment.Enabled = false;
                        creditDiv.Visible = false;
                        billingAddress.Visible = true;
                        creditDiv1.Visible = false;
                        lblTotalPrice.Text = "Free";
                        country.Visible = true;
                    }
                    else if (ticketType.ToLower() == "donation" && price == 0)
                    {
                        hdnTab.Value = "";
                        btnSubmit.Visible = true;
                        lblMessage2.Visible = false;
                        divPay.Visible = false;
                        navigation.Visible = false;
                        billingAddress.Visible = true;
                        creditDiv.Visible = false;
                        creditDiv1.Visible = false;
                        country.Visible = true;
                        // btnpayment.Enabled = false;
                    }
                    else if (ticketType.ToLower() == "donation" && price > 0)
                    {
                        hdnTab.Value = "PayPal";
                        btnSubmit.Visible = false;
                        lblMessage2.Visible = true;
                        country.Visible = true;
                        navigation.Visible = true;

                    }
                    else
                    {
                        hdnTab.Value = "Card";
                        btnSubmit.Visible = false;
                        lblMessage2.Visible = true;

                        navigation.Visible = true;
                    }

                    user = new Musika.Controllers.API.TicketingAPIController().GetUserProfile(userId);
                    if (user != null)
                    {
                        string[] fullName = user.UserName.Split(' ');
                        string lastname = string.Empty;
                        txtName.Text = fullName[0].ToString();
                        username = user.UserName;
                        if (fullName.Length > 1)
                        {
                            foreach (var f in fullName.Skip(1))
                            {
                                lastname += f.ToString() + " ";

                            }
                            // txtLastName.Text = fullName[1].ToString();
                            txtLastName.Text = lastname.TrimEnd();
                        }
                        txtEmail.Text = user.Email;

                    }
                    PaymentStatus UserDetail = db.PaymentStatus.Where(p => p.UserID == userId).OrderByDescending(p => p.Id).FirstOrDefault();

                    if (UserDetail != null)
                    {
                        ClientScript.RegisterStartupScript(this.GetType(), "data", "GetStates();", true);
                        if (string.IsNullOrEmpty(user.Email))
                        {
                            txtEmail.Text = UserDetail.Email;
                        }
                        if (!string.IsNullOrEmpty(UserDetail.Country))
                        {
                            Session["country"] = UserDetail.Country;
                            Session["state"] = UserDetail.State;
                            ddlCountry.ClearSelection(); //making sure the previous selection has been cleared
                            ddlCountry.Items.FindByText(UserDetail.Country).Selected = true;
                            hdnSelectedState.Value = UserDetail.State;
                            hdnSelectedCountry.Value = UserDetail.Country;
                            ClientScript.RegisterStartupScript(this.GetType(), "data1", "GetChildren('" + ddlCountry.SelectedValue + "', 'States', $('.txtCustomerState'));", true);
                        }


                        // txtPhone.Text = UserDetail.PhoneNumber;
                        txtAddress.Text = UserDetail.Address;
                        txtApartment.Text = UserDetail.Apartment;
                        txtCity.Text = UserDetail.City;
                        //txtPostalCode.Text = UserDetail.PostalCode;

                    }
                    else
                    {

                    }
                }
                catch (Exception ex)
                {
                    // ErrorLog.WriteErrorLog(ex);
                }

            }
        }

        // Free Checkout
        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            try
            {
                //string country = hdncountry.Value.ToString();
                bool resposne;
                string body = string.Empty;
                string ticketDetail = string.Empty;
                ticketType = Convert.ToString(Request.QueryString["ticketType"].ToString());
                package = Convert.ToString(Request.QueryString["package"].ToString());
                if (ticketType.ToLower() == "free")
                {
                    body = "Ticket Generated Successfully";
                    ticketDetail = ticketType.ToUpper() + "-" + package.ToString();
                    resposne = GenerateFreeTicket(ticketDetail);
                    if (resposne)
                    {
                        ClientScript.RegisterStartupScript(this.GetType(), "Popup", "ShowMessageFree();", true);
                    }
                    else
                    {
                        ClientScript.RegisterStartupScript(this.GetType(), "Popup", "ShowMessageUnSuccessfulFree();", true);
                    }
                }
                else if (ticketType.ToLower() == "donation")
                {
                    body = "Ticket Generated Successfully";
                    ticketDetail = ticketType.ToUpper() + "-" + package.ToString();
                    resposne = GenerateFreeTicket(ticketDetail);
                    if (resposne)
                    {
                        ClientScript.RegisterStartupScript(this.GetType(), "Popup", "ShowMessageFree();", true);
                    }
                    else
                    {
                        ClientScript.RegisterStartupScript(this.GetType(), "Popup", "ShowMessageUnSuccessfulFree();", true);
                    }
                }
            }
            catch (Exception ex)
            {
                // ErrorLog.WriteErrorLog(ex);
            }

        }
        public void BindExpirationYear()
        {
            for (int i = 0; i <= 11; i++)
            {
                String year = (DateTime.Today.Year + i).ToString();
                System.Web.UI.WebControls.ListItem li = new System.Web.UI.WebControls.ListItem(year, year);
                ddlExpirationYear.Items.Add(li);
            }
            ddlExpirationYear.Items[0].Selected = true;
        }

        private bool GenerateFreeTicket(string Mode)
        {
            // Generate Ticket
            string country = "";
            string state = hdnSelectedState.Value.ToString();
            if (hdnSelectedCountry.Value.ToString() == "")
            {
                country = ddlCountry.SelectedItem.Text;
            }
            else
            {
                country = hdnSelectedCountry.Value.ToString();
            }
            string data = string.Empty;
            string personalData = string.Empty;
            string qrCode = string.Empty;

            string email = string.Empty;
            string mode = Mode;
            string img = string.Empty;
            int qty = 1;
            userId = Convert.ToInt32(Request.QueryString["userId"].ToString());
            string ticketNumber = String.Empty;
            TicketEventDetails ticketDetails = new TicketEventDetails();
            List<TicketQRCodeDetail> files = new List<TicketQRCodeDetail>();
            TicketingEventsNew _ticketingEventsNew = new TicketingEventsNew();
            TourDate tourDate = new TourDate();
            var user = db.Users.Where(p => p.UserID == userId).FirstOrDefault();
            username = user.UserName;
            personalData += username + "~" + txtAddress.Text.TrimEnd() + "~" + country + "~" + txtCity.Text.TrimEnd() + "~" + " " + "~" + "" + "~" + "";


            tourDateId = Convert.ToInt32(Request.QueryString["tourDateId"].ToString());
            deviceId = Convert.ToString(Request.QueryString["deviceId"].ToString());
            ticketType = Convert.ToString(Request.QueryString["ticketType"].ToString());
            price = Convert.ToDecimal(Request.QueryString["Price"].ToString());
            qty = Convert.ToInt32(Request.QueryString["qty"].ToString());
            currency = Convert.ToString(Request.QueryString["currency"].ToString());
            int quantity = 0;
            CommonCls CommonCls = new CommonCls();
            string orderNo = CommonCls.Get8Digits();
            tourDate = db.TourDate.Where(p => p.TourDateID == tourDateId).Single();
            int id = Convert.ToInt32(tourDate.TicketingEventID);
            _ticketingEventsNew = db.TicketingEventsNew.Where(p => p.EventID == id).Single();
            string eventimage = _ticketingEventsNew.EventImage;
            email = txtEmail.Text.TrimEnd();
            try
            {
                bool res = false;
                for (int i = 0; i < qty; i++)
                {
                    quantity = 1;
                    data = tourDateId + "~" + userId + "~" + deviceId + "~" + ticketType + "~" + mode + "~" + quantity;

                    // Generate QR Code
                    string dataTicket = Guid.NewGuid().ToString();
                    img = new Musika.Controllers.API.TicketingAPIController().GetZXingQRCode(dataTicket);

                    ticketDetails = new Musika.Controllers.API.TicketingAPIController().GenerateTicketNumber(data, dataTicket, personalData, img, "");

                    // Save Ticket Confirmation Details in Database
                    TicketingEventTicketConfirmation ticketConform = new TicketingEventTicketConfirmation();
                    ticketConform.EventID = Convert.ToInt32(ticketDetails.EventID);
                    ticketConform.UserID = Convert.ToInt32(Request.QueryString["userId"].ToString());
                    ticketConform.Dob = Convert.ToDateTime(DateTime.Now);
                    ticketConform.Gender = "";
                    ticketConform.Address = ticketDetails.Address;
                    ticketConform.City = ticketDetails.City;
                    ticketConform.State = state.ToString();
                    ticketConform.Country = ticketDetails.Country;
                    ticketConform.PostalCode = ticketDetails.PostalCode;
                    ticketConform.Email = txtEmail.Text;
                    ticketConform.PhoneNumber = ticketDetails.PhoneNumber;
                    ticketConform.TicketNumber = ticketDetails.TicketNumber;
                    ticketConform.TicketType = ticketDetails.TicketType;
                    ticketConform.Mode = ticketDetails.Mode;
                    ticketConform.TicketSerialNumber = ticketDetails.TicketSerialNumber;
                    ticketConform.ScannedTicket = img;
                    ticketConform.TourDateID = tourDateId;
                    ticketConform.Quantity = quantity;
                    ticketConform.OrderNum = orderNo;
                    res = new Musika.Repository.SPRepository.SpRepository().SpAddTicketingEventTicketConfirmation(ticketConform);

                    if (res == true)
                    {
                        string ticketnumber = "";
                        DataSet dataTickNo = new DataSet();
                        string tickno = Convert.ToString(ticketConform.TicketNumber);
                        dataTickNo = new Musika.Repository.SPRepository.SpRepository().GetMatchedTicketNumber(tickno);
                        if (dataTickNo.Tables[0].Rows.Count > 0)
                        {
                            DataRow dr = dataTickNo.Tables[0].Rows[0];
                            ticketnumber = dr["TicketSerialNumber"].ToString();
                        }

                        //******************************

                        string filePath = Server.MapPath("/Content/QRCodeImages/" + img);
                        files.Add(new TicketQRCodeDetail { EventQRCodeImage = filePath, QRCodeNumber = ticketnumber });
                    }

                    else
                    {
                        //ClientScript.RegisterStartupScript(this.GetType(), "Popup", "ShowUnPaidMessage();", true);
                        //Response.Write("<script language='javascript'>alert('T Not Successfull....');</script>");
                        ClientScript.RegisterStartupScript(this.GetType(), "Popup", "ShowMessageUnSuccessfulFree();", true);
                    }

                }
                StringBuilder sb = new StringBuilder();
                StringBuilder sbPDF = new StringBuilder();
                if (res == true)
                {

                    if (language.ToLower() == "en-en")
                    {
                        sb.Append("<p>Hi, " + username + "</p>");
                        sb.Append("<p>Thanks for using " + System.Configuration.ConfigurationManager.AppSettings["AppName"] + "! </p>");
                        sb.Append("<p>Your Ticket has been confirmed for " + ticketDetails.EventTitle + " Event.</p>");
                        sb.Append("<p>The QR Code Image is attached along with this mail.</p>");
                        sb.Append(@"<!doctype html><html lang='en'><head><title>Musika</title></head><body>
                        <table border='0' cellpadding='0' margin:'0px' cellspacing='0' width='100%' style='overflow-x:auto;'>
                        <tr></tr><tr><td width='260' valign='top'>
                        <table border='3' cellpadding='20' cellspacing='0' style=' max-width: 100%; width: 100%; margin: 0 auto; margin-bottom: 40px; border: 3px solid #ddd;'>
                        <tr><td><table style='width:100%'><tr><td style='width: 25%'><h2 style='font-size: 30px; font-weight: 700;'>Musika</h2>
                          <p style='font-size: 13px; font-weight: 400; margin:0;'>Av Lope de Vega 13 <br/>Local 801 <br/>Naco, Santo Domingo, Distrito Nacional <br/>www.Musikaapp.com <br/>Email: Info@musikaapp.com<br/>US:  888-730-5310<br/>RD: 829-954-8355 </p>
                         </td><td style='width: 25%'></td><td style='width:25%'></td><td style='width:25%'>
                         <p style='font-size: 13px; font-weight: 400; margin-top: 50px;'>
                         <b style='font-size: 16px;'> Shipping Address:</b>
                         <br/> " + username + " <br/> " + txtAddress.Text.TrimEnd() + "<br/> " + txtCity.Text.TrimEnd() + " <br/> " + country + "<br/><br/>" +
                          " <b style='font-size: 16px;'> Shipping:</b><br/> Standard </p></td></tr></table></td></tr>" +
                          " <tr style='background-color: #ddd;'><td><table style='width: 100%;'><tr>" +
                          " <td style='padding: 15px 20px;width:25%;'>" +
                          " <h2 style='font-size: 15px; font-weight: 700; margin-top: 5px;'>#ORDER :" + orderNo + " </h2>" +
                          " </td><td style='padding: 15px 20px; width:50%;'><h2 style='font-size: 23px; font-weight: 700; margin-top: 5px; text-align: center;'>PAYMENT INFORMATION </h2>" +
                          " </td><td style='padding: 15px 20px; width:25%;'><p style='font-size: 14px; font-weight: 700; margin-top: 5px;'>Printable version </p>" +
                          " </td></tr></table></td></tr><tr><td><table width='100%' cellpadding='10' style = 'border: 2px solid #ddd;'><tr> " +
                          " <td style = 'width: 30%; border-right: 2px solid #ddd;'><p style='font-size: 13px; margin:0;'><strong> Payment method: </strong> </p>" +
                          " <p style = 'font-size: 13px; margin:0;'> Transaction Type: "+ ticketType + " </p><strong> Billing Address:</strong> " +
                          " <p style='font-size: 13px; margin:0;'> " + username + "<br/> " + txtAddress.Text.TrimEnd() + " <br/> " + txtCity.Text.TrimEnd() + "<br/>" + country + "</p> " +
                          " </td><td style = 'width: 70%;'> <table width = '100%'><tr style='font-size: 13px;'> <td style = 'padding: 0px 3px; width: 27%;font-size: 13px;'>" +
                          " <p style='font-size: 13px; margin:0;'><strong style='font-size:16px;'>Ordered items</strong></p></td> <td style = 'padding: 0px 3px;'>" +
                          " </td> <td style = 'padding: 0px 3px; font-size: 13px;'></td><td style = 'padding: 0px 3px; width: 30%;'> Subtotal article(s): N/A</td> </tr><tr>" +
                          " <td style = 'padding: 0px 3px;'><p style='font-size: 13px; margin:0;'># of tickets: " + qty.ToString() + " </p></td>" +
                          " <td style = 'padding: 0px 3px; font-size: 13px;'></td> <td style = 'padding: 0px 3px;'></td> " +
                          " <td style = 'padding: 0px 3px;'><p style='font-size: 13px; margin:0;'> </p></td> " +
                          " </tr><tr style = 'font-size: 13px;'> " +
                          " <td style = 'padding: 0px 3px;'><p style='font-size: 13px; margin:0;'>Type of ticket: " + package + '/' + ticketType + "  </p></td> " +
                          " <td style = 'padding: 0px 3px; font-size: 13px;'></td> " +
                          " <td style = 'padding: 0px 3px; font-size: 13px;'></td> " +
                          " <td style = 'padding: 0px 3px;'> Shipping and handling of merchandise: N/A </td> " +
                          " </tr> " +
                          " <tr style = 'font-size: 13px;'> " +
                          " <td style = 'padding: 0px 3px;'><p style='font-size: 13px; margin:0;'>Ticket Price : " + ticketType.ToString() + "</p></td> " +
                          " <td style = 'padding: 0px 3px; font-size: 13px;'></td> " +
                          " <td style = 'padding: 0px 3px; font-size: 13px; color: #ff1010;'> </td> " +
                         " <td style = 'padding: 0px 3px; width: 40%; font-size: 13px;'> Total before ITBIS: N/A</td> " +
                          " </tr> " +
                          " <tr style = 'font-size: 13px;'> " +
                          " <td style = 'padding: 0px 3px; '></td> " +
                          " <td style = 'padding: 0px 3px; '></td> " +
                          " <td style = 'padding: 0px 3px;'></td> " +
                            " <td style = 'padding: 0px 3px;'><p style='font-size: 13px; margin:0;'> ITBIS estimated: N/A </p></td> " +

                          " </tr> " +
                          " <tr style = 'font-size: 13px;'> " +
                          " <td style = 'padding: 0px 3px; '></td> " +
                          " <td style = 'padding: 0px 3px; '></td> " +
                          " <td style = 'padding: 0px 3px;'></td> " +
                          " <td style = 'padding: 0px 3px;'><p style='font-size: 13px; margin:0;'> Total: N/A </p></td> " +
                          " </tr> " +
                          " </table> " +
                          " </td> " +
                          " </tr> " +
                          " </table> " +
                          " </td> " +
                          " </tr> " +
                          " <tr style = 'font-size: 14px;'> " +
                          " <td><strong> Refunds are not accepted after 30 days.See our return policies. " +
                          " Contact us RD #: 829-954-8355 </strong></td> " +
                          " </tr>  </table>  </td>   </tr>   </table> " +
                          " </body></html> ");
                        sb.Append("<p><br/><br/><strong>Thanks,<br/>The " + System.Configuration.ConfigurationManager.AppSettings["AppName"] + " Team</strong></p>");
                        for (int i = 0; i < qty; i++)
                        {

                            // StringBuilder sbPDF = new StringBuilder();
                            sbPDF.Append(@"<!doctype html><html lang='en'><head><title>Musika</title><style>table{border-collapse:collapse;}	  
	                                    </style></head><body>
                                        <table border='0' cellpadding='0' margin:'0px' cellspacing='0' width='100%' style='overflow-x:auto;'><tr>
                                        </tr><tr><td width='260' valign='top'>
                                        <table border='3' cellpadding='20' cellspacing='0' 
                                        style=' max-width: 100%; width: 100%; margin: 0 auto; margin-bottom: 40px; border: 1px solid #ddd;'><tr>
                                        <td border='0'><table style='width:100%'>
                                        <tr><td style='width: 25%'><h2 style='font-size: 30px; font-weight: 700;'>
                                       <img style='width: 80%;' src='http://23.111.138.246/Content/Images/logo.png'/></h2>
                                        <p style='font-size: 13px; font-weight: 400;'>Email: Info@musikaapp.com<br/>US:  888-730-5310<br/>RD: 829-954-8355 </p>
                                        </td><td style='width: 25%'></td><td style='width:25%'></td><td style='width:25%'>
                                        <p style='font-size: 13px;margin:0;'><strong>Payment method: </strong> </p>" +
                              " <p style='font-size: 13px; margin:0;margin-bottom:10px;'> Transaction Type: "+ ticketType + " </p>" +
                              " <strong style='font-size:13px;'> Billing Address:</strong> " +
                              " <p style='font-size: 13px;margin:0;'> " + username + "<br/> " + txtAddress.Text.TrimEnd() + " <br/> " + txtCity.Text.TrimEnd() + "<br/>" + country + "</p></td></tr></table></td></tr><tr><td style='padding: 10px;border-top: 1px solid #ddd;border-bottom: 1px solid #ddd;border-right: 0;border-left: 0;'><table style='width: 100%;'>" +
                              " <tr><td><h2 style='font-size: 18px; text-align: center;margin:0;'> Ticket Information </h2></td></tr></table></td></tr>" +
                              " <tr>" +
                              " <td style='border-color:#ddd;'>" +
                              " <table width='100%' cellpadding='10' style='width:100%;border:1px solid #ddd;' bordercolor='#ddd'><tbody style='width: 100%;border-color:#ddd;'>" +
                              " <tr style='font-size: 13px;width: 100%;border-color:#ddd;'> " +
                              " <td style='width:50%;font-size: 13px;border-left: 0;border-top: 0;border-bottom: 0;'><p style='font-size: 13px;margin:0;'><strong>" + _ticketingEventsNew.EventTitle + "</strong></p>" +
                              " <p style='font-size: 13px; margin:0;margin-bottom:10px;'> " + _ticketingEventsNew.StartDate.Value.ToString("dddd, dd MMMM yyyy") + " - " + _ticketingEventsNew.EndDate.Value.ToString("dddd, dd MMMM yyyy") + " </p>" +
                              " <p style='font-size:13px;'> " + _ticketingEventsNew.VenueName + "</p> " +
                              " <p style='font-size: 13px;margin:0;margin-bottom:10px;'> " + _ticketingEventsNew.Address1 + "<br/> " + _ticketingEventsNew.City + "," + _ticketingEventsNew.State + "," + _ticketingEventsNew.ZipCode + "</p>" +
                              " <p style='font-size: 13px; margin:0; margin-bottom:15px;'><strong style='font-size:13px'> Ordered items</strong></p>" +
                              " <p style='font-size: 13px; margin:0;'># of tickets: 1 </p>" +
                              " <p style='font-size: 13px; margin:0;'>Type of ticket: " + package + '/' + ticketType + "</p>" +
                              " <p style='font-size: 13px; margin:0;'>Ticket Price: " + ticketType + "</p>" +
                              " <p style='font-size: 13px; margin:0; margin-bottom:5px;'> Service Charges: N/A </p>" +
                               " <p style='font-size: 13px; margin:0; margin-bottom:5px;'>ITBIS estimated: N/A </p>" +
                              " <p style='font-size: 13px; margin:0;'>Subtotal article(s): N/A </p>" +

                              " <p style='font-size: 13px; margin:0;'>Total: N/A </p>" +
                              " </td>" +
                              " <td style='width:50%; border-left: 0;border-top: 0;border-bottom: 0;border-right:0;'><table width='100%'><tr style='font-size: 13px;'>" +
                              " <td style='padding: 0px 3px;text-align:right;'>" +
                              " <table width='100%' align='right'><tr><td><img style='width:100%;' src='" + Server.MapPath("~/Content/EventImages/" + eventimage) + "'/></td></tr></table>" +
                              " <table width='100%' align='right' style='margin-top:10px;'>" +
                              " <tr><td><img style='margin-top:10px;width:80%;' src='" + files[i].EventQRCodeImage + "'/></td>" +
                              " </tr></table>" +
                              "  <table width='100%' align='right' style='margin-top:10px;'><tr><td><p style='width:100%;text-align:right;font-weight:bold;'> " + files[i].QRCodeNumber + "</p></td></tr></table>" +
                              " </td></tr><tr>" +
                              " <td style='padding: 0px 3px;'>" +
                              " </td></tr><tr style='font-size: 13px;'><td style='padding: 0px 3px; width: 40%;'></td></tr><tr style='font-size: 13px;'>" +
                              " <td style='padding: 0px 3px;'> </td></tr>" +
                              " <tr style='font-size: 13px;'><td style='padding: 0px 3px;'></td></tr>" +
                              " <tr style='font-size: 13px;'><td style='padding: 0px 3px;'></td></tr>" +
                              " </table></td></tr> " +
                              " </tbody></table> " +
                              " </td> " +
                              " </tr> " +
                              " <tr style='font-size: 14px;'>" +
                              " <td align='center' style='border-top: 1px solid #ddd;border-bottom: 1px solid #ddd;border-right: 0;border-left: 0;'><strong> Refunds are not accepted after 30 days.See our return policies.Contactenos al RD #: 829-954-8355 </strong>" +
                              " </td>" +
                              " </tr><tr style='font-size: 14px;'>" +
                              " <td align='center' style='border: 0; padding: 0;' ><p style='margin-bottom:10px;'>" +
                              " <a href='www.musikaapp.com'>www.Musikaapp.com</a></p>" +
                              " <p style='margin-top:0px;font-size: 15px; font-weight: 400;'> Av Lope de Vega 13 Local 801, Naco, Santo Domingo, RD </p></td>" +
                              " </tr></table></td></tr></table></body></html>");

                            if ((i + 1) < qty)
                            {
                                sbPDF.Append("<div style='page-break-before:always'> &nbsp;</div>");
                            }
                        }
                    }

                    else
                    {
                        sb.Append("<p>Hola, " + username + "</p>");
                        sb.Append("<p>Gracias por usar " + System.Configuration.ConfigurationManager.AppSettings["AppName"] + "! </p>");
                        sb.Append("<p>Su boleto ha sido confirmado para " + ticketDetails.EventTitle + " evento.</p>");
                        sb.Append("<p>La imagen del código QR se adjunta junto con este correo.</p>");
                        sb.Append(@"<!doctype html><html lang='en'><head><title>Musika</title></head><body>
                          <table border='0' cellpadding='0' margin:'0px' cellspacing='0' width='100%' style='overflow-x:auto;'>
                          <tr></tr><tr><td width='260' valign='top'><table border='3' cellpadding='20' cellspacing='0' style=' max-width: 100%; width: 100%; margin: 0 auto; margin-bottom: 40px; border: 3px solid #ddd;'>
                          <tr><td><table style='width:100%'><tr>
                          <td style='width: 25%'><h2 style='font-size: 30px; font-weight: 700;'>Musika</h2>
                          <p style='font-size: 13px; font-weight: 400; margin:0;'>Av Lope de Vega 13 <br/>Local 801 <br/>Naco, Santo Domingo, Distrito Nacional <br/>www.Musikaapp.com <br/>Email: Info@musikaapp.com<br/>US:  888-730-5310<br/>RD: 829-954-8355 </p>
                          </td><td style='width: 25%'></td><td style='width:25%'></td><td style='width:25%'>
                          <p style='font-size: 13px; font-weight: 400; margin-top: 50px;'>
                          <b style='font-size: 16px;'> Direccion de Envio:</b>
                          <br/> " + username + " <br/> " + txtAddress.Text.TrimEnd() + "<br/> " + txtCity.Text.TrimEnd() + " <br/> " + country + "<br/><br/>" +
                          "<b style='font-size: 16px;'> Envio:</b><br/> Estandar </p></td></tr></table></td></tr><tr style='background-color: #ddd;'>" +
                          "<td><table style='width: 100%;'><tr><td style='padding: 15px 20px;width:25%;'>" +
                          "<h2 style='font-size: 15px; font-weight: 700; margin-top: 5px;'>#ORDEN :" + orderNo + " </h2></td>" +
                          "<td style='padding: 15px 20px; width:50%;'><h2 style='font-size: 23px; font-weight: 700; margin-top: 5px; text-align: center;'>INFORMACION DE PAGO </h2>" +
                          "</td><td style='padding: 15px 20px; width:25%;'><p style='font-size: 14px; font-weight: 700; margin-top: 5px;'>Versión imprimible </p></td></tr></table></td></tr> " +
                          " <tr>" +
                          " <td>" +
                          " <table width = '100%' cellpadding = '10' style = 'border: 2px solid #ddd;'>" +
                          " <tr> " +
                          " <td style='width: 30%; border-right: 2px solid #ddd;'><p style='font-size: 13px; margin:0;'><strong> Método de pago: </strong> </p>" +
                          " <p style='font-size: 13px; margin:0;'> Tipo de transacción: "+ ticketType + " </p><strong> Dirección de facturación:</strong> " +
                          " <p style='font-size: 13px; margin:0;'> " + username + "<br/> " + txtAddress.Text.TrimEnd() + " <br/> " + txtCity.Text.TrimEnd() + "<br/>" + country + "</p> </td>" +
                          " <td style='width: 70%;'> <table width = '100%'><tr style='font-size: 13px;'> <td style = 'padding: 0px 3px; width: 27%;font-size: 13px;'><p style='font-size: 13px; margin:0;'>" +
                          " <strong style='font-size:16px;'>Articulos ordenados</strong></p></td> <td style = 'padding: 0px 3px;'></td> <td style = 'padding: 0px 3px; font-size: 13px;'></td>" +
                          " <td style='padding: 0px 3px; width: 30%;'> Subtotal artículo(s): N/A </td> </tr><tr>" +
                          " <td style='padding: 0px 3px;'><p style='font-size: 13px; margin:0;'># de Tickets: " + qty.ToString() + " </p></td>" +
                          " <td style='padding: 0px 3px; font-size: 13px;'></td> <td style = 'padding: 0px 3px;'></td> " +
                          " <td style='padding: 0px 3px;'><p style='font-size: 13px; margin:0;'> </p></td> " +

                          " </tr><tr style='font-size: 13px;'> " +
                          " <td style='padding: 0px 3px;'><p style='font-size: 13px; margin:0;'>Tipo de boleto: " + package + '/' + ticketType + "  </p></td> " +
                          " <td style='padding: 0px 3px; font-size: 13px;'></td> " +
                          " <td style='padding: 0px 3px; font-size: 13px;'></td> " +
                           " <td style='padding: 0px 3px;'> Envio y manejo de mercancia: N/A </td> " +

                          " </tr> " +
                          " <tr style='font-size: 13px;'> " +
                          " <td style='padding: 0px 3px;'><p style='font-size: 13px; margin:0;'>Precio de boleta : " + ticketType.ToString() + "</p></td> " +
                          " <td style='padding: 0px 3px; font-size: 13px;'></td> " +
                          " <td style='padding: 0px 3px; font-size: 13px; color: #ff1010;'> </td> " +
                          " <td style='padding: 0px 3px; width: 40%; font-size: 13px;'> Total antes de ITBIS: N/A</td> " +
                          " </tr> " +
                          " <tr style='font-size: 13px;'> " +
                          " <td style='padding: 0px 3px; '></td> " +
                          " <td style='padding: 0px 3px; '></td> " +
                          " <td style='padding: 0px 3px;'></td> " +
                           " <td style='padding: 0px 3px;'><p style='font-size: 13px; margin:0;'> ITBIS estimado: N/A </p></td> " +
                          " </tr> " +
                          " <tr style='font-size: 13px;'> " +
                          " <td style='padding: 0px 3px; '></td> " +
                          " <td style='padding: 0px 3px; '></td> " +
                          " <td style='padding: 0px 3px;'></td> " +
                          " <td style='padding: 0px 3px;'><p style='font-size: 13px; margin:0;'> Total: N/A</p></td> " +
                          " </tr> " +
                          " </table> " +
                          " </td> " +
                          " </tr> " +
                          " </table> " +
                          " </td> " +
                          " </tr> " +
                          " <tr style='font-size: 14px;'> " +
                          " <td><strong> No se aceptan reembolsos después de 30 días.Consulte nuestras políticas de devolución.¿Dudas? " +
                          " Contactenos al RD #: 829-954-8355 </strong></td> " +
                          " </tr>  </table>  </td>   </tr>   </table> " +
                          " </body></html> ");
                        sb.Append("<p><br/><br/><strong>Gracias,<br/>el " + System.Configuration.ConfigurationManager.AppSettings["AppName"] + " equipo</strong></p>");
                        for (int i = 0; i < qty; i++)
                        {
                            sbPDF.Append(@"<!doctype html><html lang='en'><head><title>Musika</title><style>table{border-collapse:collapse;}	  
	                                    </style></head><body>
                                        <table border='0' cellpadding='0' margin:'0px' cellspacing='0' width='100%' style='overflow-x:auto;'><tr>
                                        </tr><tr><td width='260' valign='top'>
                                        <table border='3' cellpadding='20' cellspacing='0' style=' max-width: 100%; width: 100%; margin: 0 auto; margin-bottom: 40px; border: 1px solid #ddd;'><tr>
                                        <td border='0'><table style='width:100%'>
                                        <tr><td style='width: 25%'><h2 style='font-size: 30px; font-weight: 700;'><img style='width: 80%;' src='http://23.111.138.246/Content/Images/logo.png'/></h2>
                                        <p style='font-size: 13px; font-weight: 400;'>Email: Info@musikaapp.com<br/>US:  888-730-5310<br/>RD: 829-954-8355 </p>
                                        </td><td style='width: 25%'></td><td style='width:25%'></td><td style='width:25%'>
                                        <p style='font-size: 13px;margin:0;'><strong>Método de pago: </strong> </p>" +
                          " <p style='font-size: 13px; margin:0;margin-bottom:10px;'> Tipo de transacción: "+ticketType+"  </p><strong style='font-size:13px;'> Dirección de facturación:</strong> " +
                          " <p style='font-size: 13px;margin:0;'> " + username + "<br/> " + txtAddress.Text.TrimEnd() + " <br/> " + txtCity.Text.TrimEnd() + "<br/>" + country + "</p></td>" +
                          " </tr></table></td></tr><tr><td style='padding: 10px;border-top: 1px solid #ddd;border-bottom: 1px solid #ddd;border-right: 0;border-left: 0;'><table style='width: 100%;'>" +
                          " <tr><td><h2 style='font-size: 18px; text-align: center;margin:0;'> Informacion de Boleta </h2></td></tr></table></td></tr>" +
                          " <tr>" +
                          " <td style='border-color:#ddd;'>" +
                          " <table width='100%' cellpadding='10' style='width:100%;border:1px solid #ddd;' bordercolor='#ddd'><tbody style='width: 100%;border-color:#ddd;'>" +
                          " <tr style='font-size: 13px;width: 100%;border-color:#ddd;'> " +
                          " <td style='font-size: 13px;border-left: 0;border-top: 0;border-bottom: 0;width:50%;'><p style='font-size: 13px;margin:0;'><strong>" + _ticketingEventsNew.EventTitle + "</strong></p>" +
                          " <p style='font-size: 13px; margin:0;margin-bottom:10px;'> " + _ticketingEventsNew.StartDate.Value.ToString("dddd, dd MMMM yyyy") + " - " + _ticketingEventsNew.EndDate.Value.ToString("dddd, dd MMMM yyyy") + " </p>" +
                          " <p style='font-size:13px;'> " + _ticketingEventsNew.VenueName + "</p> " +
                          " <p style='font-size: 13px;margin:0;margin-bottom:10px;'> " + _ticketingEventsNew.Address1 + "<br/> " + _ticketingEventsNew.City + "," + _ticketingEventsNew.State + "," + _ticketingEventsNew.ZipCode + "</p>" +
                          " <p style='font-size: 13px; margin:0; margin-bottom:15px;'><strong style='font-size:13px'> Articulos ordenados</strong></p>" +
                          " <p style='font-size: 13px; margin:0;'># de Tickets: 1 </p>" +
                          " <p style='font-size: 13px; margin:0;'>Tipo de boleto: " + package + '/' + ticketType + "</p>" +
                          " <p style='font-size: 13px; margin:0;'>Precio de boleta: " + ticketType.ToString()+ "</p>" +
                         
                          " <p style='font-size: 13px; margin:0; margin-bottom:5px;'> Cargos por servicios: N/A </p>" +
                          " <p style='font-size: 13px; margin:0; margin-bottom:5px;'> ITBIS estimado: N/A </p>" +
                          " <p style='font-size: 13px; margin:0;'>Subtotal artículo(s): N/A </p>" +
                          " <p style='font-size: 13px; margin:0;'>Total: N/A</p>" +
                          " </td>" +
                          " <td style='width:50%; border-left: 0;border-top: 0;border-bottom: 0;border-right:0;'><table width='100%'><tr style='font-size: 13px;'>" +
                              " <td style='padding: 0px 3px;text-align:right;'>" +
                              " <table width='100%' align='right'><tr><td><img style='width:100%;' src='" + Server.MapPath("~/Content/EventImages/" + eventimage) + "'/></td></tr></table>" +
                              " <table width='100%' align='right' style='margin-top:10px;'>" +
                              " <tr><td><img style='margin-top:10px;width:80%;' src='" + files[i].EventQRCodeImage + "'/></td>" +
                              " </tr></table>" +
                              "  <table width='100%' align='right' style='margin-top:10px;'><tr><td><p style='width:100%;text-align:right;font-weight:bold;'> " + files[i].QRCodeNumber + "</p></td></tr></table>" +
                              " </td></tr><tr>" +
                              " <td style='padding: 0px 3px;'>" +
                              " </td></tr><tr style='font-size: 13px;'><td style='padding: 0px 3px; width: 40%;'></td></tr><tr style='font-size: 13px;'>" +
                              " <td style='padding: 0px 3px;'> </td></tr>" +
                              " <tr style='font-size: 13px;'><td style='padding: 0px 3px;'></td></tr>" +
                              " <tr style='font-size: 13px;'><td style='padding: 0px 3px;'></td></tr>" +
                              " </table></td></tr> " +
                              " </tbody></table> " +
                              " </td> " +
                              " </tr> " +
                              " <tr style='font-size: 14px;'>" +
                              " <td align='center' style='border-top: 1px solid #ddd;border-bottom: 1px solid #ddd;border-right: 0;border-left: 0;'><strong> No se aceptan reembolsos después de 30 días.Consulte nuestras políticas de devolución.¿Dudas? Contactenos al RD #: 829-954-8355 </strong>" +
                              " </td>" +
                              " </tr><tr style='font-size: 14px;'>" +
                              " <td align='center' style='border: 0; padding: 0;' ><p style='margin-bottom:10px;'>" +
                              " <a href='www.musikaapp.com'>www.Musikaapp.com</a></p>" +
                              " <p style='margin-top:0px;font-size: 15px; font-weight: 400;'> Av Lope de Vega 13 Local 801, Naco, Santo Domingo, RD </p></td>" +
                              " </tr></table></td></tr></table></body></html>");

                            if ((i + 1) < qty)
                            {
                                sbPDF.Append("<div style='page-break-before:always'> &nbsp;</div>");
                            }
                        }
                    }

                }

                MemoryStream file = new MemoryStream(CommonCls.PDFGenerate(sbPDF.ToString(), files.ToList()).ToArray());

                file.Seek(0, SeekOrigin.Begin);
                Attachment data1 = new Attachment(file, "Musika.pdf", "application/pdf");
                ContentDisposition disposition = data1.ContentDisposition;



                if (files.Count > 0)
                {
                    SendEmailHelper.SendMailWithAttachment(email, "Musika Event Ticket Confirmation", sb.ToString(), new List<string>(), data1);
                }
                else
                {
                    // SendEmailHelper.SendMail(email, "Musika Event Ticket Confirmation", htmlbody, file);
                }
                //else
                //{
                //    return false;
                //    //lblmessage.Text = "";
                //}

            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        // Popup OK button
        protected void btnAcceptPayment_Click(object sender, EventArgs e)
        {
            try
            {
                string country = "";
                string state = hdnSelectedState.Value.ToString();
                if (hdnSelectedCountry.Value.ToString() == "")
                {
                    country = ddlCountry.SelectedItem.Text;
                }
                else
                {
                    country = hdnSelectedCountry.Value.ToString();
                }
                string data = string.Empty;
                string personalData = string.Empty;
                string qrCode = string.Empty;

                string email = string.Empty;
                userId = Convert.ToInt32(Request.QueryString["userId"].ToString());
                var user = db.Users.Where(p => p.UserID == userId).FirstOrDefault();
                username = user.UserName;
                data = username;//txtName.Text.TrimEnd();// + txtCardNumber.Text;
                string img = string.Empty;
                int qty = 1;
                decimal price = 0.0M;
                string ticketNumber = String.Empty;
                TicketEventDetails ticketDetails = new TicketEventDetails();


                tourDateId = Convert.ToInt32(Request.QueryString["tourDateId"].ToString());
                deviceId = Convert.ToString(Request.QueryString["deviceId"].ToString());
                ticketType = Convert.ToString(Request.QueryString["ticketType"].ToString());
                price = Convert.ToDecimal(Request.QueryString["Price"], System.Globalization.CultureInfo.InvariantCulture);
                qty = Convert.ToInt32(Request.QueryString["qty"].ToString());
                package = Convert.ToString(Request.QueryString["package"].ToString());
                currency = Convert.ToString(Request.QueryString["currency"].ToString());
                currency = "USD";


                PaymentStatus _PaymentStatus = new PaymentStatus();
                //_PaymentStatus.Address = txtAddress.Text.TrimEnd();
                ///_PaymentStatus.City = txtCity.Text.TrimEnd();
                //_PaymentStatus.Country = country.ToString();
                _PaymentStatus.Email = txtEmail.Text.TrimEnd();
                //_PaymentStatus.IsPaymentSucess = false;
                //_PaymentStatus.Apartment = txtApartment.Text.TrimEnd();
                _PaymentStatus.OrderNum = "";
                _PaymentStatus.PaymentResponse = "";
                _PaymentStatus.PaymentType = "Paypal";
                _PaymentStatus.Quantity = Convert.ToInt32(qty.ToString());
                _PaymentStatus.TourDateID = tourDateId;
                //_PaymentStatus.PhoneNumber = txtPhone.Text;
                //_PaymentStatus.PostalCode = "";// txtPostalCode.Text;
                //_PaymentStatus.State = state;
                _PaymentStatus.TransactionId = "";
                _PaymentStatus.UserID = Convert.ToInt32(userId.ToString());
                db.PaymentStatus.Add(_PaymentStatus);

                db.SaveChanges();
                var payment_id = _PaymentStatus.Id;
                //personalData = txtPhone.Text + "," + txtEmail.Text + "," + txtAddress.Text.Replace("#", "~").Replace(",", " ") + "," + txtCity.Text + "," + country + "," + payment_id + "," + CouponDiscount.ToString().Replace(",", ".");
                personalData = payment_id + "," + CouponDiscount.ToString().Replace(",", ".") + "," + CouponID + "," + txtEmail.Text.TrimEnd();

                Response.Redirect("~/paypal.aspx" + "?userId=" + userId + "&tourDateId=" + tourDateId + "&ticketType=" + ticketType + "&Price=" + price.ToString().Replace(",", ".") + "&qty=" + qty + "&currency=" + currency + "&package=" + package + "&lang=" + language + "&deviceId=" + deviceId + "&data=" + personalData.ToString());
            }
            catch (Exception ex)
            {
                // ErrorLog.WriteErrorLog(ex);
            }

        }

        protected void btnAcceptCard_Click(object sender, EventArgs e)
        {
            try
            {
                string country = "";
                btnAcceptCard.Enabled = false;
                string state = hdnSelectedState.Value.ToString();

                if (hdnSelectedCountry.Value.ToString() == "")
                {
                    country = ddlCountry.SelectedItem.Text;
                }
                else
                {
                    country = hdnSelectedCountry.Value.ToString();
                }
                string cardnumber = hdnCardNumber.Value.ToString();
                CommonCls CommonCls = new CommonCls();
                TicketingEventTicketsSummary _ticketsSummary = new TicketingEventTicketsSummary();
                TicketingEventsNew _ticketingEventsNew = new TicketingEventsNew();
                Coupons coupons = new Coupons();
                TourDate tourDate = new TourDate();
                string email = txtEmail.Text.TrimEnd();
                currency = Convert.ToString(Request.QueryString["currency"].ToString());
                price = Convert.ToDecimal(Request.QueryString["Price"], System.Globalization.CultureInfo.InvariantCulture);
                userId = Convert.ToInt32(Request.QueryString["userId"].ToString());
                tourDateId = Convert.ToInt32(Request.QueryString["tourDateId"].ToString());
                deviceId = Convert.ToString(Request.QueryString["deviceId"].ToString());
                ticketType = Convert.ToString(Request.QueryString["ticketType"].ToString());
                //price = Convert.ToDecimal(Request.QueryString["Price"].ToString());
                qty = Convert.ToInt32(Request.QueryString["qty"].ToString());
                package = Convert.ToString(Request.QueryString["package"].ToString());
                currency = Convert.ToString(Request.QueryString["currency"].ToString());
                tourDate = db.TourDate.Where(p => p.TourDateID == tourDateId).Single();
                int id = Convert.ToInt32(tourDate.TicketingEventID);
                _ticketsSummary = db.TicketingEventTicketsSummary.Where(p => p.TicketCategory == package && p.TicketType == ticketType && p.EventID == id).Single();
                _ticketingEventsNew = db.TicketingEventsNew.Where(p => p.EventID == id).Single();
                string eventimage = _ticketingEventsNew.EventImage;
                decimal discountSigleTicket = 0.0M;
                string coupondiscount = "";
                if (CouponID > 0)
                {
                    coupons = db.Coupons.Where(p => p.Id == CouponID).Single();
                    discountSigleTicket = Math.Ceiling(_ticketsSummary.Cost * coupons.Discount.Value) / 100;
                    discountSigleTicket = Math.Round(discountSigleTicket, 2);
                    coupondiscount = "(" + Math.Round(coupons.Discount.Value) + "% off)";

                }
                //else
                //{
                //    discountSigleTicket = _ticketsSummary.Cost;
                //}
                DataSet ds = new DataSet();
                ds = new SpRepository().SpGetSummary(Convert.ToInt32(id));
                string _servicefee = Convert.ToString(WebConfigurationManager.AppSettings["ServiceFee"]);
                string _tax = Convert.ToString(WebConfigurationManager.AppSettings["ItbisTax"]);
                decimal actualPrice = _ticketsSummary.Cost;
                decimal singleDiscountedPrice = _ticketsSummary.Cost - discountSigleTicket;
                decimal TicketPrice = (actualPrice * qty);
                actualPrice = (actualPrice * qty) - CouponDiscount;
                if (ds.Tables[0].Rows.Count > 0)
                {
                    DataRow dr = ds.Tables[0].Rows[0];
                    decimal serviceValue;
                    decimal.TryParse(dr["ServiceFee"].ToString().Replace("%", ""), out serviceValue);
                    if (serviceValue > 0)
                    {
                        _servicefee = serviceValue.ToString();
                    }


                    // _servicefee = string.IsNullOrEmpty(dr["ServiceFee"].ToString()) ? _servicefee : dr["ServiceFee"].ToString().Replace("%", "");
                    decimal taxValue;
                    decimal.TryParse(dr["Tax"].ToString().Replace("%", ""), out taxValue);
                    if (taxValue > 0)
                    {
                        _tax = taxValue.ToString();
                    }

                    //_tax = (string.IsNullOrEmpty(dr["Tax"].ToString()) || dr["Tax"].ToString()=="%") ? _tax : dr["Tax"].ToString().Replace("%", "");
                }

                decimal servicefee = ((Convert.ToDecimal(actualPrice) * Convert.ToDecimal(_servicefee.Replace("%", ""))) / 100);
                decimal servicefeeSingle = ((Convert.ToDecimal(singleDiscountedPrice) * Convert.ToDecimal(_servicefee.Replace("%", ""))) / 100);
                decimal Tax = ((Convert.ToDecimal(actualPrice) * Convert.ToDecimal(_tax.Replace("%", ""))) / 100);
                decimal TaxSingleTicket = ((Convert.ToDecimal((singleDiscountedPrice) * Convert.ToDecimal(_tax.Replace("%", ""))) / 100));
                decimal totalbeforeTax = (servicefee + (Convert.ToDecimal(actualPrice)));
                decimal total = (servicefee + Tax + (Convert.ToDecimal(actualPrice)));
                var user = db.Users.Where(p => p.UserID == userId).FirstOrDefault();
                username = user.UserName;
                decimal totalSingleTicket = singleDiscountedPrice + servicefeeSingle + TaxSingleTicket;
                if (ticketType.ToLower() == "donation" && price > 0)
                {
                    TicketPrice = price;
                    total = price;
                }
                string orderNo = CommonCls.Get8Digits();
                List<TicketQRCodeDetail> files = new List<TicketQRCodeDetail>();
                if (currency == "DOP")
                {
                    currency = "RD$";
                }
                else
                {
                    currency = "$";
                }
                int quantity = 0;
                string data = string.Empty;
                string personalData = string.Empty;
                TicketEventDetails ticketDetails = new TicketEventDetails();
                string img = string.Empty;
                personalData += username + "~" + txtAddress.Text.TrimEnd() + "~" + country + "~" + txtCity.Text.TrimEnd() + "~" + "" + "~" + "" + "~" + state.ToString();
                string mode = ticketType.ToUpper() + "-" + package.ToString();

                PaymentStatus _PaymentStatus = new PaymentStatus();

                _PaymentStatus.Address = txtAddress.Text.TrimEnd();
                _PaymentStatus.City = txtCity.Text.TrimEnd();
                _PaymentStatus.Country = country;
                _PaymentStatus.Email = txtEmail.Text.TrimEnd();
                //_PaymentStatus.IsPaymentSucess = true;
                _PaymentStatus.Cost = total.ToString();


                // _PaymentStatus.PaymentResponse = Azul_Response.ToString();
                _PaymentStatus.PaymentType = "AZUL";
                _PaymentStatus.Quantity = Convert.ToInt32(qty.ToString());
                _PaymentStatus.TourDateID = Convert.ToInt32(tourDateId.ToString());
                //_PaymentStatus.PhoneNumber = txtPhone.Text;
                _PaymentStatus.PostalCode = "";// txtPostalCode.Text;
                _PaymentStatus.State = state;
                _PaymentStatus.Apartment = txtApartment.Text.TrimEnd();
                //_PaymentStatus.TransactionId = response.AzulOrderId.ToString();
                _PaymentStatus.UserID = Convert.ToInt32(Request.QueryString["userId"].ToString());

                db.PaymentStatus.Add(_PaymentStatus);
                db.SaveChanges();
                string Azul_Response = AzulPaymentAPI(cardnumber, ddlExpirationYear.Text, ddlMonth.SelectedValue.ToString(), txtCVV.Text.TrimEnd(), Convert.ToDouble(price), Convert.ToDouble(Tax), orderNo);
                dynamic response = JsonConvert.DeserializeObject(Azul_Response);

                if (!Object.ReferenceEquals(null, response))
                {
                    if (response.ResponseCode == "ISO8583" && response.IsoCode == "00")
                    {
                        try
                        {
                            bool res = false;
                            for (int i = 0; i < qty; i++)
                            {
                                quantity = 1;
                                data = tourDateId + "~" + userId + "~" + deviceId + "~" + ticketType + "~" + mode + "~" + quantity;

                                // Generate QR Code
                                string dataTicket = Guid.NewGuid().ToString();
                                img = new Musika.Controllers.API.TicketingAPIController().GetZXingQRCode(dataTicket);

                                ticketDetails = new Musika.Controllers.API.TicketingAPIController().GenerateTicketNumber(data, dataTicket, personalData, img, orderNo);

                                // Save in Database
                                // Save Ticket Confirmation Details in Database
                                TicketingEventTicketConfirmation ticketConform = new TicketingEventTicketConfirmation();
                                ticketConform.EventID = Convert.ToInt32(ticketDetails.EventID);
                                ticketConform.UserID = Convert.ToInt32(Request.QueryString["userId"].ToString());
                                ticketConform.Dob = Convert.ToDateTime(DateTime.Now);
                                ticketConform.Gender = "";
                                ticketConform.Address = ticketDetails.Address;
                                ticketConform.City = ticketDetails.City;
                                ticketConform.State = ticketDetails.State;
                                ticketConform.Country = ticketDetails.Country;
                                ticketConform.PostalCode = ticketDetails.PostalCode;
                                ticketConform.Email = ticketDetails.Email;
                                ticketConform.PhoneNumber = ticketDetails.PhoneNumber;
                                ticketConform.TicketNumber = ticketDetails.TicketNumber;
                                ticketConform.TicketType = ticketDetails.TicketType;
                                ticketConform.Mode = ticketDetails.Mode;
                                ticketConform.TicketSerialNumber = ticketDetails.TicketSerialNumber;
                                ticketConform.ScannedTicket = img;
                                ticketConform.TourDateID = tourDateId;
                                ticketConform.Quantity = quantity;
                                ticketConform.OrderNum = orderNo;
                                res = new Musika.Repository.SPRepository.SpRepository().SpAddTicketingEventTicketConfirmation(ticketConform);

                                if (res == true)
                                {
                                    string ticketnumber = "";
                                    DataSet dataTickNo = new DataSet();
                                    string tickno = Convert.ToString(ticketConform.TicketNumber);
                                    dataTickNo = new Musika.Repository.SPRepository.SpRepository().GetMatchedTicketNumber(tickno);
                                    if (dataTickNo.Tables[0].Rows.Count > 0)
                                    {
                                        DataRow dr = dataTickNo.Tables[0].Rows[0];
                                        ticketnumber = dr["TicketSerialNumber"].ToString();
                                    }
                                    string filePath = Server.MapPath("/Content/QRCodeImages/" + img);
                                    files.Add(new TicketQRCodeDetail { EventQRCodeImage = filePath, QRCodeNumber = ticketnumber });

                                }
                                else
                                {
                                    ClientScript.RegisterStartupScript(this.GetType(), "Popup", "ShowUnPaidMessage();", true);
                                    Response.Write("<script language='javascript'>alert('Payment Not Successfull....');</script>");
                                }
                            }
                            StringBuilder sb = new StringBuilder();
                            StringBuilder sbPDF = new StringBuilder();
                            if (res == true)
                            {
                                //******************Expire Used Coupon********************
                                // Coupons coupons = new Coupons();
                                var couponData = db.Coupons.Where(x => x.Id == CouponID).SingleOrDefault();
                                if (couponData != null)
                                {
                                    couponData.Status = true;
                                    db.Entry(couponData).State = System.Data.Entity.EntityState.Modified;

                                    db.SaveChanges();
                                }
                                //**********saving payment status in database*******************//
                                // PaymentStatus _PaymentStatusUpdate = new PaymentStatus();


                                _PaymentStatus.IsPaymentSucess = true;

                                _PaymentStatus.PaymentResponse = Azul_Response.ToString();
                                _PaymentStatus.PaymentType = "AZUL";
                                _PaymentStatus.OrderNum = orderNo.ToString();
                                _PaymentStatus.TransactionId = response.AzulOrderId.ToString();

                                db.Entry(_PaymentStatus).State = System.Data.Entity.EntityState.Modified;
                                // db.PaymentStatus.Add(_PaymentStatus);

                                db.SaveChanges();

                                //**********saving payment status in database*******************//


                                if (language.ToLower() == "en-en")
                                {
                                    sb.Append("<p>Hi, " + username + "</p>");
                                    sb.Append("<p>Thanks for using " + System.Configuration.ConfigurationManager.AppSettings["AppName"] + "! </p>");
                                    sb.Append("<p>Your Ticket has been confirmed for " + ticketDetails.EventTitle + " Event.</p>");
                                    sb.Append("<p>The QR Code Image is attached along with this mail.</p>");
                                    sb.Append(@"<!doctype html><html lang='en'><head><title>Musika</title></head><body><table border='0' cellpadding='0' margin:'0px' cellspacing='0' width='100%' style='overflow-x:auto;'><tr></tr><tr><td width='260' valign='top'><table border='3' cellpadding='20' cellspacing='0' style=' max-width: 100%; width: 100%; margin: 0 auto; margin-bottom: 40px; border: 3px solid #ddd;'><tr><td><table style='width:100%'>
                                       <tr><td style='width: 25%'><h2 style='font-size: 30px; font-weight: 700;'>Musika</h2><p style='font-size: 13px; font-weight: 400; margin:0;'>Av Lope de Vega 13 <br/>Local 801 <br/>Naco, Santo Domingo, Distrito Nacional <br/>www.Musikaapp.com <br/>Email: Info@musikaapp.com<br/>US:  888-730-5310<br/>RD: 829-954-8355 </p></td><td style='width: 25%'></td><td style='width:25%'></td><td style='width:25%'><p style='font-size: 13px; font-weight: 400; margin-top: 50px;'><b style='font-size: 16px;'> Shipping Address:</b><br/> " + username + " <br/> " + txtAddress.Text.TrimEnd() + "<br/> " + txtCity.Text.TrimEnd() + " <br/> " + country + "<br/><br/><b style='font-size: 16px;'> Shipping:</b><br/> Standard </p></td></tr></table></td></tr><tr style='background-color: #ddd;'><td><table style='width: 100%;'><tr><td style='padding: 15px 20px;width:25%;'><h2 style='font-size: 15px; font-weight: 700; margin-top: 5px;'>#ORDER :" + orderNo + " </h2></td><td style='padding: 15px 20px; width:50%;'><h2 style='font-size: 23px; font-weight: 700; margin-top: 5px; text-align: center;'>PAYMENT INFORMATION </h2></td><td style='padding: 15px 20px; width:25%;'><p style='font-size: 14px; font-weight: 700; margin-top: 5px;'>Printable version </p></td></tr></table></td></tr> " +
                                      " <tr>" +
                                      " <td>" +
                                      " <table width = '100%' cellpadding='10' style='border: 2px solid #ddd;'>" +
                                      " <tr> " +
                                      " <td style='width: 30%; border-right: 2px solid #ddd;'><p style='font-size: 13px; margin:0;'><strong> Payment method: </strong> </p>" +
                                      " <p style='font-size: 13px; margin:0;'> Card brand | Last 4 digits: 0123 <br/> Approval code: " +
                                      " XXXXX <br/> Transaction Type: Purchase </p><strong> Billing Address:</strong> " +
                                      " <p style='font-size: 13px; margin:0;'> " + username + "<br/> " + txtAddress.Text.TrimEnd() + " <br/> " + txtCity.Text.TrimEnd() + "<br/>" + country + "</p> </td><td style='width: 70%;'> <table width='100%'><tr style='font-size: 13px;'> <td style='padding: 0px 3px; width: 27%;font-size: 13px;'><p style='font-size: 13px; margin:0;'><strong style='font-size:16px;'>Ordered items</strong></p></td> <td style='padding: 0px 3px;'></td> <td style='padding: 0px 3px; font-size: 13px;'></td><td style='padding: 0px 3px; width: 30%;'> Subtotal article(s): " + currency + TicketPrice.ToString().Replace(",", ".") + "</td> </tr><tr> <td style='padding: 0px 3px;'><p style='font-size: 13px; margin:0;'># of tickets: " + qty.ToString() + " </p></td>" +
                                      "<td style='padding: 0px 3px; font-size: 13px;'></td> <td style='padding: 0px 3px;'></td> " +
                                      " <td style='padding: 0px 3px;'><p style='font-size: 13px; margin:0;'> Coupon Discount: -" + currency + CouponDiscount.ToString().Replace(",", ".") + coupondiscount + " </p></td> " +
                                      " </tr><tr style='font-size: 13px;'> " +
                                      " <td style='padding: 0px 3px;'><p style='font-size: 13px; margin:0;'>Type of ticket: " + package + '/' + ticketType + "  </p></td> " +
                                      " <td style='padding: 0px 3px; font-size: 13px;'></td> " +
                                      " <td style='padding: 0px 3px; font-size: 13px;'></td> " +
                                       " <td style='padding: 0px 3px;'> Shipping and handling of merchandise: " + currency + servicefee.ToString().Replace(",", ".") + "   </td> " +

                                      " </tr> " +
                                      " <tr style='font-size: 13px;'> " +
                                      " <td style='padding: 0px 3px;'><p style='font-size: 13px; margin:0;'>Ticket Price : " + currency + TicketPrice.ToString().Replace(",", ".") + "</p></td> " +
                                      " <td style='padding: 0px 3px; font-size: 13px;'></td> " +
                                      " <td style='padding: 0px 3px; font-size: 13px; color: #ff1010;'> </td> " +
                                     " <td style='padding: 0px 3px; width: 40%; font-size: 13px;'> Total before ITBIS: " + currency + totalbeforeTax.ToString().Replace(",", ".") + "</td> " +
                                      " </tr> " +
                                      " <tr style='font-size: 13px;'> " +
                                      " <td style='padding: 0px 3px; '></td> " +
                                      " <td style='padding: 0px 3px; '></td> " +
                                      " <td style='padding: 0px 3px;'></td> " +
                                        " <td style='padding: 0px 3px;'><p style='font-size: 13px; margin:0;'> ITBIS estimated: " + currency + Tax.ToString().Replace(",", ".") + " </p></td> " +

                                      " </tr> " +
                                      " <tr style='font-size: 13px;'> " +
                                      " <td style='padding: 0px 3px; '></td> " +
                                      " <td style='padding: 0px 3px; '></td> " +
                                      " <td style='padding: 0px 3px;'></td> " +
                                      " <td style='padding: 0px 3px;'><p style='font-size: 13px; margin:0;'><strong> Total: " + currency + total.ToString().Replace(",", ".") + " </strong></p></td> " +
                                      " </tr> " +
                                      " </table> " +
                                      " </td> " +
                                      " </tr> " +
                                      " </table> " +
                                      " </td> " +
                                      " </tr> " +
                                      " <tr style='font-size: 14px;'> " +
                                      " <td><strong> Refunds are not accepted after 30 days.See our return policies. " +
                                      " Contact us RD #: 829-954-8355 </strong></td> " +
                                      " </tr>  </table>  </td>   </tr>   </table> " +
                                      " </body></html> ");
                                    sb.Append("<p><br/><br/><strong>Thanks,<br/>The " + System.Configuration.ConfigurationManager.AppSettings["AppName"] + " Team</strong></p>");
                                    for (int i=0; i < qty; i++)
                                    {

                                        // StringBuilder sbPDF = new StringBuilder();
                                        sbPDF.Append(@"<!doctype html><html lang='en'><head><title>Musika</title><style>table{border-collapse:collapse;}	  
	                                    </style></head><body>
                                        <table border='0' cellpadding='0' margin:'0px' cellspacing='0' width='100%' style='overflow-x:auto;'><tr>
                                        </tr><tr><td width='260' valign='top'>
                                        <table border='3' cellpadding='20' cellspacing='0' 
                                        style=' max-width: 100%; width: 100%; margin: 0 auto; margin-bottom: 40px; border: 1px solid #ddd;'><tr>
                                        <td border='0'><table style='width:100%'>
                                        <tr><td style='width: 25%'><h2 style='font-size: 30px; font-weight: 700;'>
                                       <img style='width: 80%;' src='http://23.111.138.246/Content/Images/logo.png'/></h2>
                                        <p style='font-size: 13px; font-weight: 400;'>Email: Info@musikaapp.com<br/>US:  888-730-5310<br/>RD: 829-954-8355 </p>
                                        </td><td style='width: 25%'></td><td style='width:25%'></td><td style='width:25%'>
                                        <p style='font-size: 13px;margin:0;'><strong>Payment method: </strong> </p>" +
                                          " <p style='font-size: 13px; margin:0;margin-bottom:10px;'> Card brand | Last 4 digits: 0123 <br/> Approval Code: " +
                                          " XXXXX <br/> Transaction Type: Purchase </p><strong style='font-size:13px;'> Billing Address:</strong> " +
                                          " <p style='font-size: 13px;margin:0;'> " + username + "<br/> " + txtAddress.Text.TrimEnd() + " <br/> " + txtCity.Text.TrimEnd() + "<br/>" + country + "</p></td></tr></table></td></tr><tr><td style='padding: 10px;border-top: 1px solid #ddd;border-bottom: 1px solid #ddd;border-right: 0;border-left: 0;'><table style='width: 100%;'>" +
                                          " <tr><td><h2 style='font-size: 18px; text-align: center;margin:0;'> Ticket Information </h2></td></tr></table></td></tr>" +
                                          " <tr>" +
                                          " <td style='border-color:#ddd;'>" +
                                          " <table width='100%' cellpadding='10' style='width:100%;border:1px solid #ddd;' bordercolor='#ddd'><tbody style='width: 100%;border-color:#ddd;'>" +
                                          " <tr style='font-size: 13px;width: 100%;border-color:#ddd;'> " +
                                          " <td style='width:50%;font-size: 13px;border-left: 0;border-top: 0;border-bottom: 0;'><p style='font-size: 13px;margin:0;'><strong>" + _ticketingEventsNew.EventTitle + "</strong></p>" +
                                          " <p style='font-size: 13px; margin:0;margin-bottom:10px;'> " + _ticketingEventsNew.StartDate.Value.ToString("dddd, dd MMMM yyyy") + " - " + _ticketingEventsNew.EndDate.Value.ToString("dddd, dd MMMM yyyy") + " </p>" +
                                          " <p style='font-size:13px;'> " + _ticketingEventsNew.VenueName + "</p> " +
                                          " <p style='font-size: 13px;margin:0;margin-bottom:10px;'> " + _ticketingEventsNew.Address1 + "<br/> " + _ticketingEventsNew.City + "," + _ticketingEventsNew.State + "," + _ticketingEventsNew.ZipCode + "</p>" +
                                          " <p style='font-size: 13px; margin:0; margin-bottom:15px;'><strong style='font-size:13px'> Ordered items</strong></p>" +
                                          " <p style='font-size: 13px; margin:0;'># of tickets: 1 </p>" +
                                          " <p style='font-size: 13px; margin:0;'>Type of ticket: " + package + '/' + ticketType + "</p>" +
                                          " <p style='font-size: 13px; margin:0;'> Ticket Price: " + currency + _ticketsSummary.Cost.ToString().Replace(",", ".") + "</p>" +
                                            " <p style='font-size: 13px; margin:0;'>Coupon Discount: -" + currency + discountSigleTicket.ToString().Replace(",", ".") + coupondiscount + "</p> " +
                                          " <p style='font-size: 13px; margin:0; margin-bottom:5px;'> Service Charges: " + currency + servicefeeSingle.ToString().Replace(",", ".") + "</p>" +
                                           " <p style='font-size: 13px; margin:0; margin-bottom:5px;'>ITBIS estimated:" + currency + TaxSingleTicket.ToString().Replace(",", ".") + "</p>" +
                                          " <p style='font-size: 13px; margin:0;'>Subtotal article(s): " + currency + totalSingleTicket.ToString().Replace(",", ".") + "</p>" +

                                          " <p style='font-size: 13px; margin:0;'><strong>Total: " + currency + totalSingleTicket.ToString().Replace(",", ".") + " </strong></p>" +
                                          " </td>" +
                                          " <td style='width:50%; border-left: 0;border-top: 0;border-bottom: 0;border-right:0;'><table width='100%'><tr style='font-size: 13px;'>" +
                                          " <td style='padding: 0px 3px;text-align:right;'>" +
                                          " <table width='100%' align='right'><tr><td><img style='width:100%;' src='" + Server.MapPath("~/Content/EventImages/" + eventimage) + "'/></td></tr></table>" +
                                          " <table width='100%' align='right' style='margin-top:10px;'>" +
                                          " <tr><td><img style='margin-top:10px;width:80%;' src='" + files[i].EventQRCodeImage + "'/></td>" +
                                          " </tr></table>" +
                                          "  <table width='100%' align='right' style='margin-top:10px;'><tr><td><p style='width:100%;text-align:right;font-weight:bold;'> " + files[i].QRCodeNumber + "</p></td></tr></table>" +
                                          " </td></tr><tr>" +
                                          " <td style='padding: 0px 3px;'>" +
                                          " </td></tr><tr style='font-size: 13px;'><td style='padding: 0px 3px; width: 40%;'></td></tr><tr style='font-size: 13px;'>" +
                                          " <td style='padding: 0px 3px;'> </td></tr>" +
                                          " <tr style='font-size: 13px;'><td style='padding: 0px 3px;'></td></tr>" +
                                          " <tr style='font-size: 13px;'><td style='padding: 0px 3px;'></td></tr>" +
                                          " </table></td></tr> " +
                                          " </tbody></table> " +
                                          " </td> " +
                                          " </tr> " +
                                          " <tr style='font-size: 14px;'>" +
                                          " <td align='center' style='border-top: 1px solid #ddd;border-bottom: 1px solid #ddd;border-right: 0;border-left: 0;'><strong> Refunds are not accepted after 30 days.See our return policies.Contactenos al RD #: 829-954-8355 </strong>" +
                                          " </td>" +
                                          " </tr><tr style='font-size: 14px;'>" +
                                          " <td align='center' style='border: 0; padding: 0;' ><p style='margin-bottom:10px;'>" +
                                          " <a href='www.musikaapp.com'>www.Musikaapp.com</a></p>" +
                                          " <p style='margin-top:0px;font-size: 15px; font-weight: 400;'> Av Lope de Vega 13 Local 801, Naco, Santo Domingo, RD </p></td>" +
                                          " </tr></table></td></tr></table></body></html>");

                                        if ((i + 1) < qty)
                                        {
                                            sbPDF.Append("<div style='page-break-before:always'> &nbsp;</div>");
                                        }
                                    }
                                }

                                else
                                {
                                    sb.Append("<p>Hola, " + username + "</p>");
                                    sb.Append("<p>Gracias por usar " + System.Configuration.ConfigurationManager.AppSettings["AppName"] + "! </p>");
                                    sb.Append("<p>Su boleto ha sido confirmado para " + ticketDetails.EventTitle + " evento.</p>");
                                    sb.Append("<p>La imagen del código QR se adjunta junto con este correo.</p>");
                                    sb.Append(@"<!doctype html><html lang='en'><head><title>Musika</title></head><body><table border='0' cellpadding='0' margin:'0px' cellspacing='0' width='100%' style='overflow-x:auto;'><tr></tr><tr><td width='260' valign='top'><table border='3' cellpadding='20' cellspacing='0' style=' max-width: 100%; width: 100%; margin: 0 auto; margin-bottom: 40px; border: 3px solid #ddd;'><tr><td><table style='width:100%'>
                                        <tr><td style='width: 25%'><h2 style='font-size: 30px; font-weight: 700;'>Musika</h2><p style='font-size: 13px; font-weight: 400; margin:0;'>Av Lope de Vega 13 <br/>Local 801 <br/>Naco, Santo Domingo, Distrito Nacional <br/>www.Musikaapp.com <br/>Email: Info@musikaapp.com<br/>US:  888-730-5310<br/>RD: 829-954-8355 </p></td><td style='width: 25%'></td><td style='width:25%'></td><td style='width:25%'><p style='font-size: 13px; font-weight: 400; margin-top: 50px;'><b style='font-size: 16px;'> Direccion de Envio:</b><br/> " + username + " <br/> " + txtAddress.Text.TrimEnd() + "<br/> " + txtCity.Text.TrimEnd() + " <br/> " + country + "<br/><br/><b style='font-size: 16px;'> Envio:</b><br/> Estandar </p></td></tr></table></td></tr><tr style='background-color: #ddd;'><td><table style='width: 100%;'><tr><td style='padding: 15px 20px;width:25%;'><h2 style='font-size: 15px; font-weight: 700; margin-top: 5px;'>#ORDEN :" + orderNo + " </h2></td><td style='padding: 15px 20px; width:50%;'><h2 style='font-size: 23px; font-weight: 700; margin-top: 5px; text-align: center;'>INFORMACION DE PAGO </h2></td><td style='padding: 15px 20px; width:25%;'><p style='font-size: 14px; font-weight: 700; margin-top: 5px;'>Versión imprimible </p></td></tr></table></td></tr> " +
                                      " <tr>" +
                                      " <td>" +
                                      " <table width='100%' cellpadding='10' style='border: 2px solid #ddd;'>" +
                                      " <tr> " +
                                      " <td style='width: 30%; border-right: 2px solid #ddd;'><p style='font-size: 13px; margin:0;'><strong> Método de pago: </strong> </p>" +
                                      " <p style='font-size: 13px; margin:0;'> Marca de la tarjeta | Últimos 4 digitos: 0123 <br/> Código de aprobación: " +
                                      " XXXXX <br/> Tipo de transacción: Compra </p><strong> Dirección de facturación:</strong> " +
                                      " <p style='font-size: 13px; margin:0;'> " + username + "<br/> " + txtAddress.Text.TrimEnd() + " <br/> " + txtCity.Text.TrimEnd() + "<br/>" + country + "</p> </td>" +
                                      " <td style='width: 70%;'> <table width='100%'><tr style='font-size: 13px;'> <td style='padding: 0px 3px; width: 27%;font-size: 13px;'><p style='font-size: 13px; margin:0;'>" +
                                      " <strong style='font-size:16px;'>Articulos ordenados</strong></p></td> <td style='padding: 0px 3px;'></td> <td style='padding: 0px 3px; font-size: 13px;'></td>" +
                                      " <td style='padding: 0px 3px; width: 30%;'> Subtotal artículo(s): " + currency + TicketPrice.ToString().Replace(",", ".") + "</td> </tr><tr>" +
                                      " <td style='padding: 0px 3px;'><p style='font-size: 13px; margin:0;'># de Tickets: " + qty.ToString() + " </p></td>" +
                                      " <td style='padding: 0px 3px; font-size: 13px;'></td> <td style='padding: 0px 3px;'></td> " +
                                      " <td style='padding: 0px 3px;'><p style='font-size: 13px; margin:0;'> Descuento del cupón: -" + currency + CouponDiscount.ToString().Replace(",", ".") + coupondiscount + " </p></td> " +

                                      " </tr><tr style='font-size: 13px;'> " +
                                      " <td style='padding: 0px 3px;'><p style='font-size: 13px; margin:0;'>Tipo de boleto: " + package + '/' + ticketType + "  </p></td> " +
                                      " <td style='padding: 0px 3px; font-size: 13px;'></td> " +
                                      " <td style='padding: 0px 3px; font-size: 13px;'></td> " +
                                       " <td style='padding: 0px 3px;'> Envio y manejo de mercancia:  " + currency + servicefee.ToString().Replace(",", ".") + "   </td> " +

                                      " </tr> " +
                                      " <tr style='font-size: 13px;'> " +
                                      " <td style='padding: 0px 3px;'><p style='font-size: 13px; margin:0;'>Precio de boleta : " + currency + TicketPrice.ToString().Replace(",", ".") + "</p></td> " +
                                      " <td style='padding: 0px 3px; font-size: 13px;'></td> " +
                                      " <td style='padding: 0px 3px; font-size: 13px; color: #ff1010;'> </td> " +
                                      " <td style='padding: 0px 3px; width: 40%; font-size: 13px;'> Total antes de ITBIS: " + currency + totalbeforeTax.ToString().Replace(",", ".") + "</td> " +
                                      " </tr> " +
                                      " <tr style='font-size: 13px;'> " +
                                      " <td style='padding: 0px 3px; '></td> " +
                                      " <td style='padding: 0px 3px; '></td> " +
                                      " <td style='padding: 0px 3px;'></td> " +
                                       " <td style='padding: 0px 3px;'><p style='font-size: 13px; margin:0;'> ITBIS estimado: " + currency + Tax.ToString().Replace(",", ".") + " </p></td> " +
                                      " </tr> " +
                                      " <tr style='font-size: 13px;'> " +
                                      " <td style='padding: 0px 3px; '></td> " +
                                      " <td style='padding: 0px 3px; '></td> " +
                                      " <td style='padding: 0px 3px;'></td> " +
                                      " <td style='padding: 0px 3px;'><p style='font-size: 13px; margin:0;'><strong> Total: " + currency + total.ToString().Replace(",", ".") + " </strong></p></td> " +
                                      " </tr> " +
                                      " </table> " +
                                      " </td> " +
                                      " </tr> " +
                                      " </table> " +
                                      " </td> " +
                                      " </tr> " +
                                      " <tr style='font-size: 14px;'> " +
                                      " <td><strong> No se aceptan reembolsos después de 30 días.Consulte nuestras políticas de devolución.¿Dudas? " +
                                      " Contactenos al RD #: 829-954-8355 </strong></td> " +
                                      " </tr>  </table>  </td>   </tr>   </table> " +
                                      " </body></html> ");
                                    sb.Append("<p><br/><br/><strong>Gracias,<br/>el " + System.Configuration.ConfigurationManager.AppSettings["AppName"] + " equipo</strong></p>");
                                    for (int i = 0; i < qty; i++)
                                    {
                                        sbPDF.Append(@"<!doctype html><html lang='en'><head><title>Musika</title><style>table{border-collapse:collapse;}	  
	                                    </style></head><body>
                                        <table border='0' cellpadding='0' margin:'0px' cellspacing='0' width='100%' style='overflow-x:auto;'><tr>
                                        </tr><tr><td width='260' valign='top'>
                                        <table border='3' cellpadding='20' cellspacing='0' style=' max-width: 100%; width: 100%; margin: 0 auto; margin-bottom: 40px; border: 1px solid #ddd;'><tr>
                                        <td border='0'><table style='width:100%'>
                                        <tr><td style='width: 25%'><h2 style='font-size: 30px; font-weight: 700;'><img style='width: 80%;' src='http://23.111.138.246/Content/Images/logo.png'/></h2>
                                        <p style='font-size: 13px; font-weight: 400;'>Email: Info@musikaapp.com<br/>US:  888-730-5310<br/>RD: 829-954-8355 </p>
                                        </td><td style='width: 25%'></td><td style='width:25%'></td><td style='width:25%'>
                                        <p style='font-size: 13px;margin:0;'><strong>Método de pago: </strong> </p>" +
                                      " <p style='font-size: 13px; margin:0;margin-bottom:10px;'>  Marca de la tarjeta | Últimos 4 digitos: 0123 <br/> Código de aprobación: " +
                                      " XXXXX <br/> Tipo de transacción: Compra  </p><strong style='font-size:13px;'> Dirección de facturación:</strong> " +
                                      " <p style='font-size: 13px;margin:0;'> " + username + "<br/> " + txtAddress.Text.TrimEnd() + " <br/> " + txtCity.Text.TrimEnd() + "<br/>" + country + "</p></td>" +
                                      " </tr></table></td></tr><tr><td style='padding: 10px;border-top: 1px solid #ddd;border-bottom: 1px solid #ddd;border-right: 0;border-left: 0;'><table style='width: 100%;'>" +
                                      " <tr><td><h2 style='font-size: 18px; text-align: center;margin:0;'> Informacion de Boleta </h2></td></tr></table></td></tr>" +
                                      " <tr>" +
                                      " <td style='border-color:#ddd;'>" +
                                      " <table width='100%' cellpadding='10' style='width:100%;border:1px solid #ddd;' bordercolor='#ddd'><tbody style='width: 100%;border-color:#ddd;'>" +
                                      " <tr style='font-size: 13px;width: 100%;border-color:#ddd;'> " +
                                      " <td style='font-size: 13px;border-left: 0;border-top: 0;border-bottom: 0;width:50%;'><p style='font-size: 13px;margin:0;'><strong>" + _ticketingEventsNew.EventTitle + "</strong></p>" +
                                      " <p style='font-size: 13px; margin:0;margin-bottom:10px;'> " + _ticketingEventsNew.StartDate.Value.ToString("dddd, dd MMMM yyyy") + " - " + _ticketingEventsNew.EndDate.Value.ToString("dddd, dd MMMM yyyy") + " </p>" +
                                      " <p style='font-size:13px;'> " + _ticketingEventsNew.VenueName + "</p> " +
                                      " <p style='font-size: 13px;margin:0;margin-bottom:10px;'> " + _ticketingEventsNew.Address1 + "<br/> " + _ticketingEventsNew.City + "," + _ticketingEventsNew.State + "," + _ticketingEventsNew.ZipCode + "</p>" +
                                      " <p style='font-size: 13px; margin:0; margin-bottom:15px;'><strong style='font-size:13px'> Articulos ordenados</strong></p>" +
                                      " <p style='font-size: 13px; margin:0;'># de Tickets: 1 </p>" +
                                      " <p style='font-size: 13px; margin:0;'>Tipo de boleto: " + package + '/' + ticketType + "</p>" +
                                      " <p style='font-size: 13px; margin:0;'>Precio de boleta: " + currency + _ticketsSummary.Cost.ToString().Replace(",", ".") + "</p>" +
                                      " <p style='font-size: 13px; margin:0;'>Descuento del cupón: -" + currency + discountSigleTicket.ToString().Replace(",", ".") + coupondiscount + "</p> " +
                                      " <p style='font-size: 13px; margin:0; margin-bottom:5px;'> Cargos por servicios: " + currency + servicefeeSingle.ToString().Replace(",", ".") + "</p>" +
                                      " <p style='font-size: 13px; margin:0; margin-bottom:5px;'> ITBIS estimado:" + currency + TaxSingleTicket.ToString().Replace(",", ".") + "</p>" +
                                      " <p style='font-size: 13px; margin:0;'>Subtotal artículo(s):" + currency + totalSingleTicket.ToString().Replace(",", ".") + "</p>" +
                                      " <p style='font-size: 13px; margin:0;'><strong>Total: " + currency + totalSingleTicket.ToString().Replace(",", ".") + " </strong></p>" +
                                      " </td>" +
                                      " <td style='width:50%; border-left: 0;border-top: 0;border-bottom: 0;border-right:0;'><table width='100%'><tr style='font-size: 13px;'>" +
                                          " <td style='padding: 0px 3px;text-align:right;'>" +
                                          " <table width='100%' align='right'><tr><td><img style='width:100%;' src='" + Server.MapPath("~/Content/EventImages/" + eventimage) + "'/></td></tr></table>" +
                                          " <table width='100%' align='right' style='margin-top:10px;'>" +
                                          " <tr><td><img style='margin-top:10px;width:80%;' src='" + files[i].EventQRCodeImage + "'/></td>" +
                                          " </tr></table>" +
                                          "  <table width='100%' align='right' style='margin-top:10px;'><tr><td><p style='width:100%;text-align:right;font-weight:bold;'> " + files[i].QRCodeNumber + "</p></td></tr></table>" +
                                          " </td></tr><tr>" +
                                          " <td style='padding: 0px 3px;'>" +
                                          " </td></tr><tr style='font-size: 13px;'><td style='padding: 0px 3px; width: 40%;'></td></tr><tr style='font-size: 13px;'>" +
                                          " <td style='padding: 0px 3px;'> </td></tr>" +
                                          " <tr style='font-size: 13px;'><td style='padding: 0px 3px;'></td></tr>" +
                                          " <tr style='font-size: 13px;'><td style='padding: 0px 3px;'></td></tr>" +
                                          " </table></td></tr> " +
                                          " </tbody></table> " +
                                          " </td> " +
                                          " </tr> " +
                                          " <tr style='font-size: 14px;'>" +
                                          " <td align='center' style='border-top: 1px solid #ddd;border-bottom: 1px solid #ddd;border-right: 0;border-left: 0;'><strong> No se aceptan reembolsos después de 30 días.Consulte nuestras políticas de devolución.¿Dudas? Contactenos al RD #: 829-954-8355 </strong>" +
                                          " </td>" +
                                          " </tr><tr style='font-size: 14px;'>" +
                                          " <td align='center' style='border: 0; padding: 0;' ><p style='margin-bottom:10px;'>" +
                                          " <a href='www.musikaapp.com'>www.Musikaapp.com</a></p>" +
                                          " <p style='margin-top:0px;font-size: 15px; font-weight: 400;'> Av Lope de Vega 13 Local 801, Naco, Santo Domingo, RD </p></td>" +
                                          " </tr></table></td></tr></table></body></html>");

                                        if ((i + 1) < qty)
                                        {
                                            sbPDF.Append("<div style='page-break-before:always'> &nbsp;</div>");
                                        }
                                    }
                                }

                            }

                            MemoryStream file = new MemoryStream(CommonCls.PDFGenerate(sbPDF.ToString(), files.ToList()).ToArray());

                            file.Seek(0, SeekOrigin.Begin);
                            Attachment data1 = new Attachment(file, "Musika.pdf", "application/pdf");
                            ContentDisposition disposition = data1.ContentDisposition;



                            if (files.Count > 0)
                            {
                                SendEmailHelper.SendMailWithAttachment(email, "Musika Event Ticket Confirmation", sb.ToString(), new List<string>(), data1);
                            }
                            else
                            {
                                // SendEmailHelper.SendMail(email, "Musika Event Ticket Confirmation", htmlbody, file);
                            }



                        }
                        catch (Exception ex)
                        {
                            // ErrorLog.WriteErrorLog(ex);
                            // errormsg.Text = "Something went wrong!!!";
                        }
                        ClientScript.RegisterStartupScript(this.GetType(), "Popup", "ShowMessagePaid();", true);
                        ClientScript.RegisterStartupScript(this.GetType(), "Popup", "$('#MyPopup').modal('hide')", true);

                        Response.Write("<script type='text/javascript'>function showAndroidToast(toast) { Android.showToast(toast); }</script>");
                    }
                    else if (response.ResponseCode == "ISO8583" && response.IsoCode == "99")
                    {
                        //**********saving payment status in database*******************//
                        //PaymentStatus _PaymentStatus = new PaymentStatus();
                        _PaymentStatus.IsPaymentSucess = false;
                        _PaymentStatus.OrderNum = "";
                        _PaymentStatus.PaymentResponse = Azul_Response.ToString();
                        _PaymentStatus.PaymentType = "AZUL";
                        if (response.AzulOrderId != null)
                        {
                            _PaymentStatus.TransactionId = response.AzulOrderId.ToString();
                        }
                        else
                        {
                            _PaymentStatus.TransactionId = "";
                        }
                        db.Entry(_PaymentStatus).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        ClientScript.RegisterStartupScript(this.GetType(), "Popup", "ShowUnPaidMessageAZUL('" + response.ErrorDescription + "');", true);

                    }
                    else if (response.ResponseCode == "ISO8583" && response.IsoCode == "51")
                    {
                        //**********saving payment status in database*******************//
                        //PaymentStatus _PaymentStatus = new PaymentStatus();
                        _PaymentStatus.IsPaymentSucess = false;
                        _PaymentStatus.OrderNum = "";
                        _PaymentStatus.PaymentResponse = Azul_Response.ToString();
                        _PaymentStatus.PaymentType = "AZUL";
                        if (response.AzulOrderId != null)
                        {
                            _PaymentStatus.TransactionId = response.AzulOrderId.ToString();
                        }
                        else
                        {
                            _PaymentStatus.TransactionId = "";
                        }
                        db.Entry(_PaymentStatus).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        ClientScript.RegisterStartupScript(this.GetType(), "Popup", "ShowUnPaidMessageAZUL('" + response.ErrorDescription + "');", true);

                    }
                    else
                    {
                        //**********saving payment status in database*******************//
                        //PaymentStatus _PaymentStatus = new PaymentStatus();
                        _PaymentStatus.IsPaymentSucess = false;
                        _PaymentStatus.OrderNum = "";
                        _PaymentStatus.PaymentResponse = Azul_Response.ToString();
                        _PaymentStatus.PaymentType = "AZUL";
                        if (response.AzulOrderId != null)
                        {
                            _PaymentStatus.TransactionId = response.AzulOrderId.ToString();
                        }
                        else
                        {
                            _PaymentStatus.TransactionId = "";
                        }
                        db.Entry(_PaymentStatus).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        string validation = response.ErrorDescription.Value;

                        if (validation.Contains("VALIDATION_ERROR:ExpirationPassed"))
                        {
                            ClientScript.RegisterStartupScript(this.GetType(), "Popup", "ShowUnPaidMessageAZUL('Error: Expiration Passed','exp');", true);

                        }
                        else if (validation.Contains("VALIDATION_ERROR:CardNumber"))
                        {
                            ClientScript.RegisterStartupScript(this.GetType(), "Popup", "ShowUnPaidMessageAZUL('Error: Invalid CardNumber','card');", true);

                        }
                        else
                        {
                            ClientScript.RegisterStartupScript(this.GetType(), "Popup", "ShowUnPaidMessageAZUL('Something went wrong');", true);
                        }
                    }
                }
                else
                {
                    ClientScript.RegisterStartupScript(this.GetType(), "Popup", "ShowUnPaidMessage();", true);
                }
                btnAcceptCard.Enabled = true;
            }
            catch (Exception ex)
            {
                //ErrorLog.WriteErrorLog(ex);
            }

        }
        public string AzulPaymentAPI(string cardNum, string expiration, string month, string CVC, double Amount, double ItbisTax, string orderNo)
        {
            string url = "http://23.111.138.246:8000/payment/request"; //live url
            //string url = "http://23.111.138.246:8001/payment/request"; //testing url
            try
            {
                var client = new HttpClient();
                var decimalUpdation = string.Format("{0:0.00}", Amount);
                var decimalTaxUpdation = string.Format("{0:0.00}", ItbisTax);
                decimalUpdation = decimalUpdation.Replace(".", "");
                decimalTaxUpdation = decimalTaxUpdation.Replace(".", "");
                var keyValues = new List<KeyValuePair<string, string>>();
                keyValues.Add(new KeyValuePair<string, string>("Store", "39361980014"));
                keyValues.Add(new KeyValuePair<string, string>("CardNumber", cardNum));
                keyValues.Add(new KeyValuePair<string, string>("Expiration", expiration + month));
                keyValues.Add(new KeyValuePair<string, string>("Amount", Convert.ToString(decimalUpdation).Replace(",", "")));
                keyValues.Add(new KeyValuePair<string, string>("OrderNumber", orderNo));
                keyValues.Add(new KeyValuePair<string, string>("CVC", CVC));
                keyValues.Add(new KeyValuePair<string, string>("ITBIS", Convert.ToString(decimalTaxUpdation).Replace(",", "")));

                using (var httpClient = new HttpClient())
                {
                    using (var content = new FormUrlEncodedContent(keyValues))
                    {
                        content.Headers.Clear();
                        content.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                        var response = httpClient.PostAsync(url, content);
                        var result = response.Result;
                        var res = result.Content.ReadAsStringAsync();
                        // lblresponse.Text = JsonConvert.SerializeObject(keyValues);
                        return res.Result.ToString();
                    }
                }

            }

            catch (Exception ex)
            {
                // ErrorLog.WriteErrorLog(ex);
            }
            return "";
        }

        protected void drpPaymentMethod_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Convert.ToInt32(drpPaymentMethod.SelectedValue) == 0)
            {
                hdnTab.Value = "PayPal";
                billingAddress.Visible = false;
                ClientScript.RegisterStartupScript(this.GetType(), "data", "bindData();", true);
                country.Visible = false;
                if (Session["country"] != null)
                {

                    ddlCountry.ClearSelection(); //making sure the previous selection has been cleared
                    ddlCountry.Items.FindByText(Session["country"].ToString()).Selected = true;
                    hdnSelectedState.Value = Session["state"].ToString();
                    ClientScript.RegisterStartupScript(this.GetType(), "data1", "GetChildren('" + ddlCountry.SelectedValue + "', 'States', $('.txtCustomerState'));", true);
                }
                btnAcceptCard.Visible = false;
                btnAcceptPayment.Visible = true;
                creditDiv.Visible = false;
                creditDiv1.Visible = false;
                paypalDiv.Visible = true;
                divPay.Visible = true;
                // paypalSpan.Attributes.Add("class", "active");
                // cardSpan.Attributes.Remove("class");
            }
            else
            {
                hdnTab.Value = "Card";
                country.Visible = true;
                billingAddress.Visible = true;
                ClientScript.RegisterStartupScript(this.GetType(), "data", "bindData();", true);
                if (Session["country"] != null)
                {

                    ddlCountry.ClearSelection(); //making sure the previous selection has been cleared
                    ddlCountry.Items.FindByText(Session["country"].ToString()).Selected = true;
                    hdnSelectedState.Value = Session["state"].ToString();
                    ClientScript.RegisterStartupScript(this.GetType(), "data1", "GetChildren('" + ddlCountry.SelectedValue + "', 'States', $('.txtCustomerState'));", true);
                }
                currency = Convert.ToString(Request.QueryString["currency"].ToString());
                price = Convert.ToDecimal(Request.QueryString["Price"].ToString());
                if (currency == "DOP")
                {
                    currency = "RD$";
                }
                else
                {
                    currency = "$";
                }
                btnAcceptCard.Visible = true;
                btnAcceptPayment.Visible = false;
                divPay.Visible = false;
                creditDiv.Visible = true;
                creditDiv1.Visible = true;
                //paypalDiv.Visible = true;

                // cardSpan.Attributes.Add("class", "active");
                // paypalSpan.Attributes.Remove("class");
            }
        }
    }

}