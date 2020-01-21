using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using Musika.Common;
using Musika.Library.Utilities;
using Musika.Models;
using Musika.Repository.SPRepository;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Net.Mime;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Http;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Musika
{
    public partial class PaymentResponse : System.Web.UI.Page
    {
        MusikaEntities db = new MusikaEntities();
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {

                TicketingEventTicketsSummary _ticketsSummary = new TicketingEventTicketsSummary();
                TourDate tourDate = new TourDate();
                WriteLog.WriteLogFile("Error");
                string strSandbox = "";
                //Post back to either sandbox or live
                if (ConfigurationManager.AppSettings["UseSandbox"].ToString() == "true")
                    strSandbox = "https://www.sandbox.paypal.com/cgi-bin/webscr";
                else
                    strSandbox = "https://www.paypal.com/cgi-bin/webscr";
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(strSandbox);

                string strFormValues = Encoding.ASCII.GetString(Request.BinaryRead(Request.ContentLength));

                //Set values for the request back
                req.Method = "POST";
                req.ContentType = "application/x-www-form-urlencoded";
                byte[] param = Request.BinaryRead(HttpContext.Current.Request.ContentLength);
                string strRequest = Encoding.ASCII.GetString(param);
                strRequest = strFormValues + "&cmd=_notify-validate";
                req.ContentLength = strRequest.Length;

                //Send the request to PayPal and get the response
                StreamWriter streamOut = new StreamWriter(req.GetRequestStream(), System.Text.Encoding.ASCII);
                streamOut.Write(strRequest);
                streamOut.Close();
                StreamReader streamIn = new StreamReader(req.GetResponse().GetResponseStream());
                string strResponse = streamIn.ReadToEnd();
                streamIn.Close();
                // string response = GetPayPalResponse(true);
                NameValueCollection parsed = HttpUtility.ParseQueryString(strFormValues);
                string s = parsed["custom"].ToString();
                string[] values = s.Split(',');
                int paymentID = Convert.ToInt32(values[2].ToString());
                decimal couponDisc = Convert.ToDecimal(values[3].ToString());
                PaymentStatus _PaymentStatus = db.PaymentStatus.First(a => a.Id == paymentID);

                if (strResponse == "VERIFIED")
                {
                    //**********************************************


                    switch (parsed["payment_status"].ToString())
                    {
                        case "Completed": //If statement to check and verify the business email and that the script was triggered from the buy_now button.
                            if (_PaymentStatus.IsPaymentSucess != true)
                            {
                                PaymentProcess(parsed, strFormValues, _PaymentStatus, couponDisc);
                            }
                            break;
                        case "Pending":
                            // PaymentProcess(parsed, strFormValues, _PaymentStatus);
                            _PaymentStatus.IsPaymentSucess = false;

                            _PaymentStatus.OrderNum = "";
                            _PaymentStatus.PaymentResponse = strFormValues.ToString();

                            _PaymentStatus.TransactionId = parsed["txn_id"].ToString();
                            //_PaymentStatus.UserID = Convert.ToInt32(parsed["item_number"]);
                            db.Entry(_PaymentStatus).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            string htmlPending = "<p>Hi," + parsed["first_name"] + " " + parsed["last_name"] + "</p>";
                            htmlPending += "<p>Thanks for using " + System.Configuration.ConfigurationManager.AppSettings["AppName"] + "! </p>";
                            htmlPending += "<p>Your Payment is under Review.After successfull payment we will notify you.</p>";

                            htmlPending += "<p><br/><br/><strong>Thanks,<br/>The " + System.Configuration.ConfigurationManager.AppSettings["AppName"] + " Team</strong></p>";
                            string emailPending = parsed["payer_email"].ToString();

                            #region "Mail Functionality"
                            SendEmailHelper.SendMail(emailPending, "Musika Event Ticket Status", htmlPending, "");
                            #endregion
                            //Response.Redirect("Failure.aspx?msg="+ parsed["pending_reason"].ToString(), true);
                            break;

                        case "Failed":

                            _PaymentStatus.IsPaymentSucess = false;

                            _PaymentStatus.OrderNum = "";
                            _PaymentStatus.PaymentResponse = strFormValues.ToString();
                            _PaymentStatus.PaymentType = "Paypal";

                            _PaymentStatus.TransactionId = parsed["txn_id"].ToString();

                            db.Entry(_PaymentStatus).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            // Send Mail to User 
                            string html = "<p>Hi," + parsed["first_name"] + " " + parsed["last_name"] + "</p>";
                            html += "<p>Thanks for using " + System.Configuration.ConfigurationManager.AppSettings["AppName"] + "! </p>";
                            html += "<p>Your Payment has been Failed. </p>";

                            html += "<p><br/><br/><strong>Thanks,<br/>The " + System.Configuration.ConfigurationManager.AppSettings["AppName"] + " Team</strong></p>";
                            string email = parsed["payer_email"].ToString();

                            #region "Mail Functionality"
                            SendEmailHelper.SendMail(email, "Musika Event Ticket Status", html, "");
                            #endregion
                            // Response.Redirect("Failure.aspx?msg=" + parsed["pending_reason"].ToString(), true);
                            break;
                        case "Denied":

                            _PaymentStatus.IsPaymentSucess = false;

                            _PaymentStatus.OrderNum = "";
                            _PaymentStatus.PaymentResponse = strFormValues.ToString();
                            _PaymentStatus.PaymentType = "Paypal";

                            _PaymentStatus.TransactionId = parsed["txn_id"].ToString();

                            db.Entry(_PaymentStatus).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            // Send Mail to User 
                            string emailhtml = "<p>Hi," + parsed["first_name"] + " " + parsed["last_name"] + "</p>";
                            emailhtml += "<p>Thanks for using " + System.Configuration.ConfigurationManager.AppSettings["AppName"] + "! </p>";
                            emailhtml += "<p>Your Payment has been Denied. </p>";

                            emailhtml += "<p><br/><br/><strong>Thanks,<br/>The " + System.Configuration.ConfigurationManager.AppSettings["AppName"] + " Team</strong></p>";
                            string emailStatus = parsed["payer_email"].ToString();

                            #region "Mail Functionality"
                            SendEmailHelper.SendMail(emailStatus, "Musika Event Ticket Status", emailhtml, "");
                            #endregion
                            //Response.Redirect("Failure.aspx?msg=" + parsed["pending_reason"].ToString(), true);
                            break;
                    }
                    //***********************************************
                }
                else if (strResponse == "INVALID")
                {
                    //**********saving payment status in database*******************//

                    _PaymentStatus.IsPaymentSucess = false;

                    _PaymentStatus.OrderNum = "";
                    _PaymentStatus.PaymentResponse = strFormValues.ToString();

                    _PaymentStatus.TransactionId = "";
                    _PaymentStatus.CreatedDate = DateTime.Now;
                    db.Entry(_PaymentStatus).State = System.Data.Entity.EntityState.Modified;

                    db.SaveChanges();
                    WriteLog.WriteLogFile("IPNResponse" + strResponse);
                    lblmessage.Text = "INVALID";
                    // Send Mail to User 
                    string html = "<p>Hi," + parsed["first_name"] + " " + parsed["last_name"] + "</p>";
                    html += "<p>Thanks for using " + System.Configuration.ConfigurationManager.AppSettings["AppName"] + "! </p>";
                    html += "<p>Your Payment has not been Successfully done. </p>";

                    html += "<p><br/><br/><strong>Thanks,<br/>The " + System.Configuration.ConfigurationManager.AppSettings["AppName"] + " Team</strong></p>";
                    string email = parsed["payer_email"].ToString();

                    #region "Mail Functionality"
                    SendEmailHelper.SendMail(email, "Musika Event Ticket Status", html, "");
                    #endregion
                    //Response.Redirect("Failure.aspx?msg=" + parsed["pending_reason"].ToString(), true);
                }
                else
                {

                    _PaymentStatus.IsPaymentSucess = false;

                    _PaymentStatus.OrderNum = "";
                    _PaymentStatus.PaymentResponse = strFormValues.ToString();
                    _PaymentStatus.PaymentType = "Paypal";

                    _PaymentStatus.TransactionId = "";
                    _PaymentStatus.UserID = Convert.ToInt32(parsed["item_number"]);
                    db.Entry(_PaymentStatus).State = System.Data.Entity.EntityState.Modified;

                    db.SaveChanges();
                    WriteLog.WriteLogFile("IPNResponse" + strResponse);

                    string html = "<p>Hi," + parsed["first_name"] + " " + parsed["last_name"] + "</p>";
                    html += "<p>Thanks for using " + System.Configuration.ConfigurationManager.AppSettings["AppName"] + "! </p>";
                    html += "<p>Your Payment has not been Successfully done. </p>";

                    html += "<p><br/><br/><strong>Thanks,<br/>The " + System.Configuration.ConfigurationManager.AppSettings["AppName"] + " Team</strong></p>";
                    string email = parsed["payer_email"].ToString();

                    #region "Commented Mail Functionality"
                    SendEmailHelper.SendMail(email, "Musika Event Ticket Status", html, "");
                    #endregion
                    // Response.Redirect("Failure.aspx?msg=" + parsed["pending_reason"].ToString(), true);
                }
            }
            catch (Exception ex)
            {
                string msg = ex.Message + "\n" + ex.StackTrace;
                WriteLog.WriteLogFile("IPNResponse" + ex.Message);
                //Response.Redirect("Failure.aspx", true);
            }
        }
        public void PaymentProcess(NameValueCollection parsed, string strFormValues, PaymentStatus _PaymentStatus, decimal couponDisc)
        {
            TicketingEventTicketsSummary _ticketsSummary = new TicketingEventTicketsSummary();
            TourDate tourDate = new TourDate();
            TicketingEventsNew _ticketingEventsNew = new TicketingEventsNew();
            //WriteLog.WriteLogFile("IPNResponse" + strResponse);
            lblmessage.Text = "VERIFIED";
            string data = string.Empty;
            string personalData = string.Empty;
            TicketEventDetails ticketDetails = new TicketEventDetails();
            Coupons coupons = new Coupons();
            string img = string.Empty;
            string email = string.Empty; //"30533,Paid,129,134.0,29,email,Android,4"
            string s = parsed["custom"].ToString();//"34639,Paid,9087654321,shalludogra19@gmail.com,address,city,United States,Maryland,100(payment_Id),IOS,1"
            string[] values = s.Split(',');
            int qty = 0;
            string lang = "";
            string orderNo = Get8Digits();
            int _tourid = Convert.ToInt32(values[0]);
            tourDate = db.TourDate.Where(p => p.TourDateID == _tourid).Single();
            //_ticketsSummary = db.TicketingEventTicketsSummary.Where(p => p.EventID == tourDate.TicketingEventID).Single();
            int id = Convert.ToInt32(tourDate.TicketingEventID);
            string ticketPackage = parsed["item_name"].ToString();
            string ticketType = values[1].ToString();
            _ticketsSummary = db.TicketingEventTicketsSummary.Where(p => p.TicketCategory == ticketPackage && p.TicketType == ticketType && p.EventID == id).FirstOrDefault();
            _ticketingEventsNew = db.TicketingEventsNew.Where(p => p.EventID == id).Single();
            string eventimage = _ticketingEventsNew.EventImage;
            int couponId = Convert.ToInt32(values[4]);
            decimal discountSigleTicket = 0.0M;
            string coupondiscount = "";
            if (couponId > 0)
            {
                coupons = db.Coupons.Where(p => p.Id == couponId).Single();
                discountSigleTicket = Math.Ceiling(_ticketsSummary.Cost * coupons.Discount.Value) / 100;
                discountSigleTicket = Math.Round(discountSigleTicket, 2);
                coupondiscount = "(" + Math.Round(coupons.Discount.Value) + "% off)";

            }
            decimal actualPrice = Convert.ToDecimal(_ticketsSummary.Cost);
            decimal singleDiscountedPrice = _ticketsSummary.Cost - discountSigleTicket;
            decimal TicketPrice = actualPrice * Convert.ToInt32(values[7]);
            decimal DicountedPrice = (actualPrice * Convert.ToInt32(values[7])) - Convert.ToDecimal(values[3]);
            int acutalQuantity = Convert.ToInt32(values[7]);
          
            DataSet ds = new DataSet();
            ds = new SpRepository().SpGetSummary(Convert.ToInt32(id));
            string _servicefee = Convert.ToString(WebConfigurationManager.AppSettings["ServiceFee"]);
            string _tax = Convert.ToString(WebConfigurationManager.AppSettings["ItbisTax"]);
            if (ds.Tables[0].Rows.Count > 0)
            {
                //DataRow dr = ds.Tables[0].Rows[0];
                //_servicefee = string.IsNullOrEmpty(dr["ServiceFee"].ToString()) ? "15" : dr["ServiceFee"].ToString().Replace("%", "");
                //_tax = string.IsNullOrEmpty(dr["Tax"].ToString()) ? "18" : dr["Tax"].ToString().Replace("%", "");

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
            }

            decimal price = Convert.ToDecimal(parsed["payment_gross"]);

            decimal servicefee = ((Convert.ToDecimal(DicountedPrice) * Convert.ToDecimal(_servicefee.Replace("%", ""))) / 100);
            decimal servicefeeSingle = ((Convert.ToDecimal(singleDiscountedPrice) * Convert.ToDecimal(_servicefee.Replace("%", ""))) / 100);
            decimal Tax = ((Convert.ToDecimal(DicountedPrice) * Convert.ToDecimal(_tax.Replace("%", ""))) / 100);
            decimal TaxSingleTicket = ((Convert.ToDecimal((singleDiscountedPrice) * Convert.ToDecimal(_tax.Replace("%", ""))) / 100));
            decimal totalbeforeTax = (servicefee + (Convert.ToDecimal(DicountedPrice)));


            decimal total = (servicefee + Tax + (Convert.ToDecimal(DicountedPrice)));
            decimal totalSingleTicket = singleDiscountedPrice + servicefeeSingle + TaxSingleTicket;
            if (ticketType.ToLower() == "donation" && price > 0)
            {
                TicketPrice = price;
                total = price;
            }
            List<TicketQRCodeDetail> files = new List<TicketQRCodeDetail>();
            string currency = parsed["mc_currency"].ToString();
            string package = parsed["item_name"].ToString();
            if (currency == "DOP")
            {
                currency = "RD$";
            }
            else
            {
                currency = "$";
            }
            int quantity = Convert.ToInt16(values[7]);
            lang = values[8].ToString();
            try
            {
                bool res = false;
                for (int i = 0; i < Convert.ToInt32(values[7]); i++)
                {
                    //"30533,Paid,129,134.0,29,email,Android,4"
                    //tourDateId,ticketType,phonenumber,DOB,deviceId
                    qty = 1;
                    data = values[0].ToString() + "~" + parsed["item_number"] + "~" + values[6].ToString() + "~" + values[1].ToString() + "~" + parsed["mode"] + "~" + qty;
                    string country = parsed["address_country"].ToString();
                    // Generate QR Code
                    string dataTicket = Guid.NewGuid().ToString();
                    img = new Musika.Controllers.API.TicketingAPIController().GetZXingQRCode(dataTicket);
                    //personalData += txtName.Text + "~" + txtAddress.Text + "~" + country + "~" + txtCity.Text + "~" + txtPostalCode.Text + "~" + txtPhone.Text + "~" + state.ToString();
                    //test~fghfg~US~fghfgh~~9087654323~Belarus~shalludogra19@gmail.com
                    personalData = parsed["first_name"].ToString() + "~" + " " + "~" + " " + "~" + " " + "~" + " " + "~" + " " + "~" + " " + "~" + " ";
                    ticketDetails = new Musika.Controllers.API.TicketingAPIController().GenerateTicketNumber(data, dataTicket, personalData, img, orderNo);

                    // Save in Database
                    TicketingEventTicketConfirmation ticketConform = new TicketingEventTicketConfirmation();
                    ticketConform.EventID = Convert.ToInt32(ticketDetails.EventID);
                    ticketConform.UserID = Convert.ToInt32(parsed["item_number"]);
                    ticketConform.Dob = Convert.ToDateTime(DateTime.Now);
                    ticketConform.Gender = "";
                    ticketConform.Address = parsed["address_street"];
                    ticketConform.City = "";
                    ticketConform.State = "";
                    ticketConform.Country = "";
                    ticketConform.PostalCode = "";
                    ticketConform.Email = "";
                    ticketConform.PhoneNumber = "";
                    ticketConform.TicketNumber = ticketDetails.TicketNumber;
                    ticketConform.TicketType = ticketDetails.TicketType;
                    ticketConform.Mode = parsed["payment_type"].ToString() + "-" + parsed["item_name"].ToString();
                    ticketConform.TicketSerialNumber = ticketDetails.TicketSerialNumber;
                    ticketConform.ScannedTicket = img;
                    ticketConform.TourDateID = Convert.ToInt32(values[0].ToString());
                    ticketConform.Quantity = Convert.ToInt32(qty);
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
                        email = values[5].ToString();

                        string filePath = Server.MapPath("/Content/QRCodeImages/" + img);
                        files.Add(new TicketQRCodeDetail { EventQRCodeImage = filePath, QRCodeNumber = ticketnumber });
                    }
                    else
                    {

                    }
                }
                if (res == true)
                {
                    //  PaymentStatus _PaymentStatus = new PaymentStatus();

                    var couponData = db.Coupons.Where(x => x.Id == couponId).SingleOrDefault();
                    if (couponData != null)
                    {
                        couponData.Status = true;
                        db.Entry(couponData).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                    }

                    _PaymentStatus.IsPaymentSucess = true;
                    _PaymentStatus.Address = parsed["address_street"].ToString();
                    _PaymentStatus.OrderNum = orderNo.ToString();
                    _PaymentStatus.PaymentResponse = strFormValues.ToString();
                    // _PaymentStatus.PaymentType = "Paypal";
                    _PaymentStatus.Cost = price.ToString();
                    _PaymentStatus.TransactionId = parsed["txn_id"].ToString();
                    _PaymentStatus.PostalCode = parsed["address_zip"].ToString();
                    _PaymentStatus.Country = parsed["address_country"].ToString();
                    _PaymentStatus.State = parsed["address_state"].ToString();
                    _PaymentStatus.City = parsed["address_city"].ToString();

                    db.Entry(_PaymentStatus).State = System.Data.Entity.EntityState.Modified;
                    // db.PaymentStatus.Add(_PaymentStatus);

                    db.SaveChanges();
                    StringBuilder sbPDF = new StringBuilder();
                    string htmlbody = "";

                    if (lang.ToLower() == "en-en")
                    {
                        htmlbody = "<p>Hi," + parsed["first_name"] + " " + parsed["last_name"] + "</p>";
                        htmlbody += "<p>Thanks for using " + System.Configuration.ConfigurationManager.AppSettings["AppName"] + "! </p>";
                        htmlbody += "<p>Your Ticket has been confirmed for " + ticketDetails.EventTitle + " Event.</p>";
                        htmlbody += "<p>The QR Code Image is attached along with this mail.</p>";
                        htmlbody += @"<!doctype html><html lang='en'><head><title>Musika</title></head><body><table border='0' cellpadding='0' margin:'0px' cellspacing='0' width='100%' style='overflow-x:auto;'><tr></tr><tr><td width='260' valign='top'>
                                     <table border='3' cellpadding='20' cellspacing='0' 
                                      style='max-width: 100%; width: 100%; margin: 0 auto; margin-bottom: 40px; border: 3px solid #ddd;'>
                                     <tr><td><table style='width:100%'>
                                     <tr><td style='width: 25%'><h2 style='font-size: 30px; font-weight: 700;'>Musika</h2>
                                     <p style='font-size: 13px; font-weight: 400;margin:0;'>Av Lope de Vega 13 <br/>Local 801 <br/>Naco, Santo Domingo, Distrito Nacional <br/>www.Musikaapp.com <br/>Email: Info@musikaapp.com<br/>US:  888-730-5310<br/>RD: 829-954-8355 </p>
                                     </td><td style='width: 25%'></td><td style='width:25%'></td><td style='width:25%'><p style='font-size: 13px; font-weight: 400; margin-top: 50px;'>
                                     <b style='font-size: 16px;'> Shipping Address:</b><br/> " + parsed["first_name"] + " " + parsed["last_name"] + " <br/> " + _PaymentStatus.Address + "<br/> " + _PaymentStatus.City + " <br/> " + _PaymentStatus.Country + "<br/><br/><b style='font-size: 16px;'> Shipping:</b><br/> Standard </p>" +
                                     "</td></tr></table></td></tr><tr style='background-color: #ddd;'><td><table style='width: 100%;'><tr>" +
                                     "<td style='padding: 15px 20px;width:25%;'><h2 style='font-size: 15px; font-weight: 700; margin-top: 5px;'>#ORDER :" + orderNo + "</h2>" +
                                     "</td><td style='padding: 15px 20px; width:50%;'>" +
                                    "<h2 style='font-size: 23px; font-weight: 700; margin-top: 5px; text-align: center;'>PAYMENT INFORMATION </h2>" +
                                    "</td><td style='padding: 15px 20px; width:25%;'><p style='font-size: 14px; font-weight: 700; margin-top: 5px;'>Printable version </p>" +
                                    "</td></tr></table></td></tr> " +
                  " <tr>" +
                  " <td>" +
                  " <table width = '100%' cellpadding = '10' style = 'border: 2px solid #ddd;'>" +
                  " <tr> " +
                  " <td style = 'width: 30%; border-right: 2px solid #ddd;'><p style='font-size: 13px;margin:0;'><strong> Payment method: </strong> </p>" +
                  " <p style = 'font-size: 13px;margin:0;'> Card brand | Last 4 digits: 0123 <br/> Approval code: " +
                  " XXXXX <br/> Transaction Type: Purchase </p><strong> Billing Address:</strong> " +
                  "<p style = 'font-size: 13px;'> " + parsed["first_name"] + " " + parsed["last_name"] + " <br/> " + _PaymentStatus.Address + " <br/> " + _PaymentStatus.City + "<br/>" + _PaymentStatus.Country + "</p> </td><td style = 'width: 70%;'> <table width = '100%'><tr style='font-size: 13px;'> " +
                  "<td style = 'padding: 0px 3px; width: 27%;font-size: 13px;'>" +
                  " <p style='font-size: 13px;margin:0;'><strong style='font-size:16px;'>Ordered items</strong>" +
                  " </p></td> <td style = 'padding: 0px 3px;'></td> <td style = 'padding: 0px 3px; font-size: 13px;'></td>" +
                  " <td style = 'padding: 0px 3px; width: 30%;'> Subtotal article(s): " + currency + TicketPrice + " </td> </tr>" +
                   "<tr><td style = 'padding: 0px 3px;'><p style='font-size: 13px;margin:0;'># of tickets: " + acutalQuantity.ToString() + " </p></td>" +
                  " <td style = 'padding: 0px 3px; font-size: 13px;'></td> <td style = 'padding: 0px 3px;'></td> " +
                  " <td style = 'padding: 0px 3px; '> Shipping and handling of merchandise:  " + currency + servicefee + "   </td> " +
                  " </tr>" +
                  " <tr style = 'font-size: 13px;'> " +
                  " <td style = 'padding: 0px 3px;'><p style='font-size: 13px;margin:0;'>Type of ticket: " + package + '/' + ticketDetails.TicketType + "  </p></td> " +
                  " <td style = 'padding: 0px 3px; font-size: 13px;'></td> " +
                  " <td style = 'padding: 0px 3px; font-size: 13px;'></td> " +
                  " <td style = 'padding: 0px 3px; width: 40%; font-size: 13px;'> Total before ITBIS: " + currency + totalbeforeTax + " </td> " +
                  " </tr> " +
                  " <tr style = 'font-size: 13px;'> " +
                  " <td style = 'padding: 0px 3px;'><p style='font-size: 13px;margin:0;'>Ticket Price: " + currency + TicketPrice + "</p></td> " +
                  " <td style = 'padding: 0px 3px; font-size: 13px;'></td> " +
                  " <td style = 'padding: 0px 3px; font-size: 13px; color: #ff1010;'> </td> " +
                  " <td style = 'padding: 0px 3px;'><p style='font-size: 13px;margin:0;'> ITBIS estimated: " + currency + Tax + " </p></td> " +
                  " </tr> " +
                  " <tr style = 'font-size: 13px;'> " +
                  " <td style = 'padding: 0px 3px; '></td> " +
                  " <td style = 'padding: 0px 3px; '></td> " +
                  " <td style = 'padding: 0px 3px;'></td> " +
                  " <td style = 'padding: 0px 3px;'><p style='font-size: 13px;margin:0;'>  Coupon Discount: -" + currency + couponDisc +coupondiscount + "</p></td> " +
                  " </tr> " +
                  " <tr style = 'font-size: 13px;'> " +
                  " <td style = 'padding: 0px 3px; '></td> " +
                  " <td style = 'padding: 0px 3px; '></td> " +
                  "  <td style = 'padding: 0px 3px;'></td> " +
                  " <td style = 'padding: 0px 3px;'><p style='font-size: 13px;margin:0;'><strong> Total: " + currency + total + " </strong></p></td> " +
                  " </tr> " +
                  "  </table> " +
                  "  </td> " +
                  " </tr> " +
                  " </table> " +
                  "  </td> " +
                  " </tr> " +
                  "  <tr style = 'font-size: 14px;'> " +
                  " <td><strong> Refunds are not accepted after 30 days. See our Return Policies. " +
                  " Contact us to RD #: 829-954-8355 </strong></td> " +
                  " </tr>  </table>  </td>   </tr>   </table> " +
                  " </body></html> ";
                        htmlbody += "<p><br/><br/><strong>Thanks,<br/>The " + System.Configuration.ConfigurationManager.AppSettings["AppName"] + " Team</strong></p>";
                        //pdf code
                        for (int i = 0; i < acutalQuantity; i++)
                        {
                            //sbPDF.Append(@"<!doctype html><html lang='en'><head><title>Musika</title><style>table{border-collapse:collapse;}	  
                            //         </style></head><body>
                            //            <table border='0' cellpadding='0' margin:'0px' cellspacing='0' width='100%' style='overflow-x:auto;'><tr>
                            //            </tr><tr><td width='260' valign='top'>
                            //            <table border='3' cellpadding='20' cellspacing='0' 
                            //            style=' max-width: 100%; width: 100%; margin: 0 auto; margin-bottom: 40px; border: 3px solid #ddd;'><tr>
                            //            <td><table style='width:100%'>
                            //            <tr><td style='width: 25%'><h2 style='font-size: 30px; font-weight: 700;'>
                            //           <img style='width: 80%;' src='http://23.111.138.246/Content/Images/logo.png'/></h2>
                            //            <p style='font-size: 13px; font-weight: 400;'>Email: Info@musikaapp.com<br/>US:  888-730-5310<br/>RD: 829-954-8355 </p>
                            //            </td><td style='width: 25%'></td><td style='width:25%'></td><td style='width:25%'>
                            //            <p style='font-size: 13px;margin:0;'><strong>Payment method: </strong> </p>" +
                            //  " <p style='font-size: 13px; margin:0;margin-bottom:10px;'> Card brand | Last 4 digits: 0123 <br/> Approval Code: " +
                            //  " XXXXX <br/> Transaction Type: Purchase </p><strong style='font-size:13px;'> Billing Address:</strong> " +
                            //  " <p style='font-size: 13px;margin:0;'> " + parsed["first_name"] + " " + parsed["last_name"] + "<br/> " + _PaymentStatus.Address + " <br/> " + _PaymentStatus.City + "<br/>" + _PaymentStatus.Country + "</p></td></tr></table></td></tr><tr><td><table style='width: 100%;'>" +
                            //  " <tr><td><h2 style='font-size: 23px; text-align: center;'> Ticket Information </h2></td></tr></table></td></tr>" +
                            //  " <tr>" +
                            //  " <td style='border-color:#ddd;'>" +
                            //  " <table width='100%' cellpadding='10' style='width:100%;border:2px solid #ddd;' bordercolor='#ddd'><tbody style='width: 100%;border-color:#ddd;'>" +
                            //  " <tr style='font-size: 13px;width: 100%;border-color:#ddd;'> " +
                            //  " <td style='font-size: 13px;border-left: 0;border-top: 0;border-bottom: 0;width:33.33%;'><p style='font-size: 13px;margin:0;'><strong>" + _ticketingEventsNew.EventTitle + " </strong> </p>" +
                            //  " <p style='font-size: 13px; margin:0;margin-bottom:10px;'> " + _ticketingEventsNew.StartDate.Value.ToString("dddd, dd MMMM yyyy") + " - " + _ticketingEventsNew.EndDate.Value.ToString("dddd, dd MMMM yyyy") + " </p><strong style='font-size:13px;'> " + _ticketingEventsNew.VenueName + "</strong> " +
                            //  " <p style='font-size: 13px;margin:0;margin-bottom:10px;'> " + _ticketingEventsNew.Address1 + "<br/> " + _ticketingEventsNew.City + "," + _ticketingEventsNew.State + "," + _ticketingEventsNew.ZipCode + "</p>" +
                            //  " <p style='font-size: 13px; margin:0; margin-bottom:15px;'><strong style='font-size:13px'> Ordered items</strong></p>" +
                            //  " <p style='font-size: 13px; margin:0;'># of tickets: 1 </p>" +
                            //  " <p style='font-size: 13px; margin:0;'>Type of ticket: " + package + '/' + ticketType + "</p>" +
                            //  " <p style='font-size: 13px; margin:0;'> Ticket Price: " + currency + _ticketsSummary.Cost.ToString().Replace(",", ".") + "</p>" +
                            //  " <p style='font-size: 13px; margin:0; margin-bottom:5px;'><strong style='font-size:13px'> Service Charges: " + currency + servicefeeSingle.ToString().Replace(",", ".") + "</strong></p>" +
                            //  " <p style='font-size: 13px; margin:0; margin-bottom:5px;'><strong style='font-size:13px'> ITBIS estimated:" + currency + TaxSingleTicket.ToString().Replace(",", ".") + "</strong></p>" +
                            //  " <p style='font-size: 13px; margin:0;'>Subtotal article(s): " + currency + totalSingleTicket.ToString().Replace(",", ".") + "</p>" +
                            //  " <p style='font-size: 13px; margin:0;'>Coupon Discount: -" + currency + couponDisc.ToString().Replace(",", ".") + "</p> " +
                            //  " <p style='font-size: 13px; margin:0;'><strong>Total: " + currency + totalSingleTicket.ToString().Replace(",", ".") + " </strong></p>" +
                            //  " </td>" +
                            //  " <td style='border-left: 0;border-top: 0;border-bottom: 0;border-right:0;width:33.33%;'><table width='100%'><tr style='font-size: 13px;'>" +
                            //  " <td style='padding: 0px 3px;'><p style='font-size: 13px; margin:0;text-align: right;'><img style='width:100%;' src='http://23.111.138.246/Content/EventImages/dc11a9c3-c1e4-4340-b3ef-ce7f9a1a6370.jpg'/></p>" +
                            //  " <p style='font-size: 13px; margin:0;text-align: center;'><img style='width:40%;' src='" + files[i].EventQRCodeImage + "'/></p>" + files[i].QRCodeNumber + "</td></tr><tr>" +
                            //  " <td style='padding: 0px 3px;'>" +
                            //  " </td></tr><tr style='font-size: 13px;'><td style='padding: 0px 3px; width: 40%;'></td></tr><tr style='font-size: 13px;'>" +
                            //  " <td style='padding: 0px 3px;'> </td></tr>" +
                            //  " <tr style='font-size: 13px;'><td style='padding: 0px 3px;'></td></tr>" +
                            //  " <tr style='font-size: 13px;'><td style='padding: 0px 3px;'></td></tr>" +
                            //  " </table></td></tr> " +
                            //  " </tbody></table> " +
                            //  " </td> " +
                            //  " </tr> " +
                            //  " <tr style='font-size: 14px;'>" +
                            //  " <td align='center'><strong> Refunds are not accepted after 30 days.See our return policies.Contactenos al RD #: 829-954-8355 </strong><p>" +
                            //  " <a href='www.musikaapp.com'>www.Musikaapp.com</a></p>" +
                            //  " <p style='font-size: 15px; font-weight: 400;'> Av Lope de Vega 13 Local 801, Naco, Santo Domingo, RD </p></td>" +
                            //  " </tr></table></td></tr></table></body></html>");


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
                              " <p style='font-size: 13px;margin:0;'> " + parsed["first_name"] + " " + parsed["last_name"] + "<br/> " + _PaymentStatus.Address + " <br/> " + _PaymentStatus.City + "<br/>" + _PaymentStatus.Country + "</p>" +
                              "</td></tr></table></td></tr><tr><td style='padding: 10px;border-top: 1px solid #ddd;border-bottom: 1px solid #ddd;border-right: 0;border-left: 0;'><table style='width: 100%;'>" +
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
                              "  <table width='100%' align='right' style='margin-top:10px;'><tr><td>" +
                              "<p style='width:100%;text-align:right;font-weight:bold;'> " + files[i].QRCodeNumber + "</p></td></tr></table>" +
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




                            if ((i + 1) < acutalQuantity)
                            {
                                sbPDF.Append("<div style='page-break-before:always'> &nbsp;</div>");
                            }
                        }
                    }
                    else
                    {
                        htmlbody = "<p>Hola," + parsed["first_name"] + " " + parsed["last_name"] + "</p>";
                        htmlbody += "<p>Gracias por usar " + System.Configuration.ConfigurationManager.AppSettings["AppName"] + "! </p>";
                        htmlbody += "<p>Su boleto ha sido confirmado para " + ticketDetails.EventTitle + " evento.</p>";
                        htmlbody += "<p>La imagen del código QR se adjunta junto con este correo.</p>";
                        htmlbody += @"<!doctype html><html lang='en'><head><title>Musika</title></head><body><table border='0' cellpadding='0' margin:'0px' cellspacing='0' width='100%' style='overflow-x:auto;'><tr></tr><tr><td width='260' valign='top'>
<table border='3' cellpadding='20' cellspacing='0' style=' max-width: 100%; width: 100%; margin: 0 auto; margin-bottom: 40px; border: 3px solid #ddd;'>
<tr><td><table style='width:100%'>
  <tr><td style='width: 25%'><h2 style='font-size: 30px; font-weight: 700;'>Musika</h2>
<p style='font-size: 13px; font-weight: 400;margin:0;'>Av Lope de Vega 13 <br/>Local 801 <br/>Naco, Santo Domingo, Distrito Nacional <br/>www.Musikaapp.com <br/>Email: Info@musikaapp.com<br/>US:  888-730-5310<br/>RD: 829-954-8355 </p>
</td><td style='width: 25%'></td><td style='width:25%'></td><td style='width:25%'><p style='font-size: 13px; font-weight: 400; margin-top: 50px;'>
<b style='font-size: 16px;'> Direccion de Envio:</b><br/> " + parsed["first_name"] + " " + parsed["last_name"] + " <br/> " + _PaymentStatus.Address + "<br/> " + _PaymentStatus.City + " <br/> " + _PaymentStatus.Country + "<br/><br/><b style='font-size: 16px;'> Envio:</b><br/> Estandar </p>" +
    "</td></tr></table></td></tr><tr style='background-color: #ddd;'><td><table style='width: 100%;'><tr><td style='padding: 15px 20px;width:25%;'><h2 style='font-size: 15px; font-weight: 700; margin-top: 5px;'>#ORDEN :" + orderNo + "</h2></td><td style='padding: 15px 20px; width:50%;'>" +
    "<h2 style='font-size: 23px; font-weight: 700; margin-top: 5px; text-align: center;'>INFORMACION DE PAGO </h2></td><td style='padding: 15px 20px; width:25%;'><p style='font-size: 14px; font-weight: 700; margin-top: 5px;'>Versión imprimible </p></td></tr></table></td></tr> " +
                  " <tr>" +
                  " <td>" +
                  " <table width = '100%' cellpadding = '10' style = 'border: 2px solid #ddd;'>" +
                  " <tr> " +
                  " <td style = 'width: 30%; border-right: 2px solid #ddd;'><p style='font-size: 13px;margin:0;'><strong> Método de pago: </strong> </p>" +
                  " <p style = 'font-size: 13px;margin:0;'> Marca de la tarjeta | Últimos 4 digitos: 0123 <br/> Código de aprobación: " +
                  " XXXXX <br/> Tipo de transacción: Compra </p><strong> Dirección de facturación:</strong> " +
                  "<p style = 'font-size: 13px;'> " + parsed["first_name"] + " " + parsed["last_name"] + " <br/> " + _PaymentStatus.Address + " <br/> " + _PaymentStatus.City + "<br/>" + _PaymentStatus.Country + "</p> </td><td style = 'width: 70%;'> <table width = '100%'><tr style='font-size: 13px;'> " +
                  "<td style = 'padding: 0px 3px; width: 27%;font-size: 13px;'><p style='font-size: 13px;margin:0;'><strong style='font-size:16px;'>Articulos ordenados</strong></p></td> <td style = 'padding: 0px 3px;'></td> <td style = 'padding: 0px 3px; font-size: 13px;'></td>" +
                  "<td style = 'padding: 0px 3px; width: 30%;'> Subtotal artículo(s): " + currency + TicketPrice + "</td> </tr><tr> <td style = 'padding: 0px 3px;'><p style='font-size: 13px;margin:0;'># de Tickets: " + acutalQuantity.ToString() + " </p></td>" +
                  "<td style = 'padding: 0px 3px; font-size: 13px;'></td> <td style = 'padding: 0px 3px;'></td> " +
                  " <td style = 'padding: 0px 3px; '> Envio y manejo de mercancia:  " + currency + servicefee + "   </td> " +
                  " </tr><tr style = 'font-size: 13px;'> " +
                  " <td style = 'padding: 0px 3px;'><p style='font-size: 13px;margin:0;'>Tipo de boleto: " + package + '/' + ticketDetails.TicketType + "  </p></td> " +
                  " <td style = 'padding: 0px 3px; font-size: 13px;'></td> " +
                  " <td style = 'padding: 0px 3px; font-size: 13px;'></td> " +
                  "  <td style = 'padding: 0px 3px; width: 40%; font-size: 13px;'> Total antes de ITBIS: " + currency + totalbeforeTax + "</td> " +
                  "</tr> " +
                  " <tr style = 'font-size: 13px;'> " +
                  " <td style = 'padding: 0px 3px;'><p style='font-size: 13px;margin:0;'>Precio de boleta : " + currency + TicketPrice + "</p></td> " +
                  " <td style = 'padding: 0px 3px; font-size: 13px;'></td> " +
                  "  <td style = 'padding: 0px 3px; font-size: 13px; color: #ff1010;'> </td> " +
                  "  <td style = 'padding: 0px 3px;'><p style='font-size: 13px;margin:0;'> ITBIS estimado: " + currency + Tax + " </p></td> " +
                  "  </tr> " +
                  "  <tr style = 'font-size: 13px;'> " +
                  " <td style = 'padding: 0px 3px; '></td> " +
                  "  <td style = 'padding: 0px 3px; '></td> " +
                  "  <td style = 'padding: 0px 3px;'></td> " +
                  " <td style = 'padding: 0px 3px;'><p style='font-size: 13px;margin:0;'>  Descuento del cupón: -" + currency + couponDisc + " </p></td> " +
                  " </tr> " +
                   "  <tr style = 'font-size: 13px;'> " +
                  " <td style = 'padding: 0px 3px; '></td> " +
                  "  <td style = 'padding: 0px 3px; '></td> " +
                  "  <td style = 'padding: 0px 3px;'></td> " +
                  " <td style = 'padding: 0px 3px;'><p style='font-size: 13px;margin:0;'><strong> Total: " + currency + total + " </strong></p></td> " +
                  " </tr> " +
                  "  </table> " +
                  "  </td> " +
                  " </tr> " +
                  " </table> " +
                  "  </td> " +
                  " </tr> " +
                  "  <tr style = 'font-size: 14px;'> " +
                  " <td><strong> No se aceptan reembolsos después de 30 días.Consulte nuestras políticas de devolución.¿Dudas? " +
                  " Contactenos al RD #: 829-954-8355 </strong></td> " +
                  " </tr>  </table>  </td>   </tr>   </table> " +
                  " </body></html> ";
                        htmlbody += "<p><br/><br/><strong>Gracias,<br/>The " + System.Configuration.ConfigurationManager.AppSettings["AppName"] + " equipo</strong></p>";
                        //pdf code

                        for (int i = 0; i < acutalQuantity; i++)
                        {
                            //  sbPDF.Append(@"<!doctype html><html lang='en'><head><title>Musika</title><style>table{border-collapse:collapse;}	  
                            //           </style></head><body>
                            //              <table border='0' cellpadding='0' margin:'0px' cellspacing='0' width='100%' style='overflow-x:auto;'><tr>
                            //              </tr><tr><td width='260' valign='top'>
                            //              <table border='3' cellpadding='20' cellspacing='0' style=' max-width: 100%; width: 100%; margin: 0 auto; margin-bottom: 40px; border: 3px solid #ddd;'><tr>
                            //              <td><table style='width:100%'>
                            //              <tr><td style='width: 25%'><h2 style='font-size: 30px; font-weight: 700;'><img style='width: 80%;' src='http://23.111.138.246/Content/Images/logo.png'/></h2>
                            //              <p style='font-size: 13px; font-weight: 400;'>Email: Info@musikaapp.com<br/>US:  888-730-5310<br/>RD: 829-954-8355 </p>
                            //              </td><td style='width: 25%'></td><td style='width:25%'></td><td style='width:25%'>
                            //              <p style='font-size: 13px;margin:0;'><strong>Método de pago: </strong> </p>" +
                            //" <p style='font-size: 13px; margin:0;margin-bottom:10px;'>  Marca de la tarjeta | Últimos 4 digitos: 0123 <br/> Código de aprobación: " +
                            //" XXXXX <br/> Tipo de transacción: Compra  </p><strong style='font-size:13px;'> Dirección de facturación:</strong> " +
                            //" <p style='font-size: 13px;margin:0;'> " + parsed["first_name"] + " " + parsed["last_name"] + "<br/> " + _PaymentStatus.Address + " <br/> " + _PaymentStatus.City + "<br/>" + _PaymentStatus.Country + "</p></td></tr></table></td></tr><tr><td style='padding: 10px;'><table style='width: 100%;'>" +
                            //" <tr><td><h2 style='font-size: 23px; text-align: center;margin:0;'> Informacion de Boleta </h2></td></tr></table></td></tr>" +
                            //" <tr>" +
                            //" <td style='border-color:#ddd;'>" +
                            //" <table width='100%' cellpadding='10' style='width:100%;border:2px solid #ddd;' bordercolor='#ddd'><tbody style='width: 100%;border-color:#ddd;'>" +
                            //" <tr style='font-size: 13px;width: 100%;border-color:#ddd;'> " +
                            //" <td style='font-size: 13px;border-left: 0;border-top: 0;border-bottom: 0;width:33.33%;'><p style='font-size: 13px;margin:0;'><strong>" + _ticketingEventsNew.EventTitle + " </strong> </p>" +
                            //" <p style='font-size: 13px; margin:0;margin-bottom:10px;'> " + _ticketingEventsNew.StartDate.Value.ToString("dddd, dd MMMM yyyy") + " - " + _ticketingEventsNew.EndDate.Value.ToString("dddd, dd MMMM yyyy") + " </p><strong style='font-size:13px;'> " + _ticketingEventsNew.VenueName + "</strong> " +
                            //" <p style='font-size: 13px;margin:0;margin-bottom:10px;'> " + _ticketingEventsNew.Address1 + "<br/> " + _ticketingEventsNew.City + "," + _ticketingEventsNew.State + "," + _ticketingEventsNew.ZipCode + "</p>" +
                            //" <p style='font-size: 13px; margin:0; margin-bottom:15px;'><strong style='font-size:13px'> Articulos ordenados</strong></p>" +
                            //" <p style='font-size: 13px; margin:0;'># de Tickets: 1 </p>" +
                            //" <p style='font-size: 13px; margin:0;'>Tipo de boleto: " + package + '/' + ticketType + "</p>" +
                            //" <p style='font-size: 13px; margin:0;'> Precio de boleta: " + currency + _ticketsSummary.Cost.ToString().Replace(",", ".") + "</p>" +
                            //" <p style='font-size: 13px; margin:0; margin-bottom:5px;'><strong style='font-size:13px'> Cargos por servicios: " + currency + servicefeeSingle.ToString().Replace(",", ".") + "</strong></p>" +
                            //" <p style='font-size: 13px; margin:0; margin-bottom:5px;'><strong style='font-size:13px'> ITBIS estimado:" + currency + TaxSingleTicket.ToString().Replace(",", ".") + "</strong></p>" +
                            //" <p style='font-size: 13px; margin:0;'>Subtotal artículo(s):" + currency + totalSingleTicket.ToString().Replace(",", ".") + "</p>" +
                            //" <p style='font-size: 13px; margin:0;'>Descuento del cupón: -" + currency + couponDisc.ToString().Replace(",", ".") + "</p> " +
                            //" <p style='font-size: 13px; margin:0;'><strong>Total: " + currency + totalSingleTicket.ToString().Replace(",", ".") + " </strong></p>" +
                            //" </td>" +
                            //" <td style='border-left: 0;border-top: 0;border-bottom: 0;border-right:0;width:33.33%;'><table width='100%'><tr style='font-size: 13px;'>" +
                            //" <td style='padding: 0px 3px;'><p style='margin:0;'><img style='width:100%;' src='http://23.111.138.246/Content/EventImages/dc11a9c3-c1e4-4340-b3ef-ce7f9a1a6370.jpg'/></p>" +
                            //" <p style='font-size: 13px; margin:0;text-align: center;'><img style='width:70%;' src='" + files[i].EventQRCodeImage + "'/></p>" + files[i].QRCodeNumber + "</td></tr><tr>" +
                            //" <td style='padding: 0px 3px;'>" +
                            //" </td></tr><tr style='font-size: 13px;'><td style='padding: 0px 3px; width: 40%;'></td></tr><tr style='font-size: 13px;'>" +
                            //" <td style='padding: 0px 3px;'> </td></tr>" +
                            //" <tr style='font-size: 13px;'><td style='padding: 0px 3px;'></td></tr>" +
                            //" <tr style='font-size: 13px;'><td style='padding: 0px 3px;'></td></tr>" +
                            //" </table></td></tr> " +
                            //" </tbody></table> " +
                            //" </td> " +
                            //" </tr> " +
                            //" <tr style='font-size: 14px;'>" +
                            //" <td align='center'><strong> No se aceptan reembolsos después de 30 días.Consulte nuestras políticas de devolución.¿Dudas? Contactenos al RD #: 829-954-8355 </strong><p>" +
                            //" <a href='www.musikaapp.com'>www.Musikaapp.com</a></p>" +
                            //" <p style='font-size: 15px; font-weight: 400;'> Av Lope de Vega 13 Local 801, Naco, Santo Domingo, RD </p></td>" +
                            //" </tr></table></td></tr></table></body></html>");
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
                                       " <p style='font-size: 13px;margin:0;'> " + parsed["first_name"] + " " + parsed["last_name"] + "<br/> " + _PaymentStatus.Address + " <br/> " + _PaymentStatus.City + "<br/>" + _PaymentStatus.Country + "</p></td>" +
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
                                       " <p style='font-size: 13px; margin:0;'> Precio de boleta: " + currency + _ticketsSummary.Cost.ToString().Replace(",", ".") + "</p>" +
                                       " <p style='font-size: 13px; margin:0;'>Descuento del cupón: -" + currency + discountSigleTicket.ToString().Replace(",", ".") + coupondiscount + " </p> " +
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




                            if ((i + 1) < acutalQuantity)
                            {
                                sbPDF.Append("<div style='page-break-before:always'> &nbsp;</div>");
                            }
                        }

                    }

                    //sbPDF.Append("</table> </body></html>");
                    CommonCls CommonCls = new CommonCls();
                    MemoryStream file = new MemoryStream(CommonCls.PDFGenerate(sbPDF.ToString(), files.ToList()).ToArray());
                    // HTMLWorker htmlparser = new HTMLWorker(pdfDoc);
                    // MemoryStream file = new MemoryStream(PDFGenerate(sbPDF.ToString()).ToArray());

                    file.Seek(0, SeekOrigin.Begin);
                    Attachment data1 = new Attachment(file, "Musika.pdf", "application/pdf");
                    ContentDisposition disposition = data1.ContentDisposition;
                    #region "Mail Functionality"
                    #region "Send Mail Implementation"
                    // string filePath = Server.MapPath("/Content/QRCodeImages/" + img);
                    // SendEmailHelper.SendMail(email, "Musika Event Ticket Confirmation", htmlbody, filePath);

                    #endregion
                    if (files.Count > 0)
                    {
                        SendEmailHelper.SendMailWithAttachment(email, "Musika Event Ticket Confirmation", htmlbody, new List<string>(), data1);
                    }
                    else
                    {
                        // SendEmailHelper.SendMail(email, "Musika Event Ticket Confirmation", htmlbody, file);
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                Response.Redirect("Failure.aspx", true);
            }
            finally
            {
                ClientScript.RegisterStartupScript(this.GetType(), "Popup", "$('#MyPopup').modal('hide')", true);
            }
        }
        private MemoryStream PDFGenerate(string message)
        {

            using (MemoryStream stream = new System.IO.MemoryStream())
            {
                StringReader sr = new StringReader(message);
                Document pdfDoc = new Document(iTextSharp.text.PageSize.A4, 10f, 10f, 10f, 0f);
                PdfWriter writer = PdfWriter.GetInstance(pdfDoc, stream);
                pdfDoc.Open();
                XMLWorkerHelper.GetInstance().ParseXHtml(writer, pdfDoc, sr);
                pdfDoc.Close();
                //return System.IO.File(stream.ToArray(), "application/pdf", "Grid.pdf");
                return stream;
            }

        }
        public string Get8Digits()
        {
            var bytes = new byte[4];
            var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            uint random = BitConverter.ToUInt32(bytes, 0) % 100000000;
            return String.Format("{0:D8}", random);
        }
    }

}
