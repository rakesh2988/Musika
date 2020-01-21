using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;
using Musika.Library.Utilities;
using System.IO;
using Musika.Models;
using SendGrid;
using System.Net.Mail;
using System.Net.Mime;

namespace Musika
{
    public partial class TicketEventCheckout : System.Web.UI.Page
    {
        #region "Commented Code"
        //int userId;
        //int tourDateId;
        //string deviceId;
        //string ticketType;
        //string mode;
        //int qty;

        ////tourDateId = temp[0];
        ////    userid = temp[1];
        ////    deviceId = temp[2];
        ////    ticketType = temp[3];
        ////    mode = temp[4];

        //Musika.Models.UsersModel user = new UsersModel();
        //protected void Page_Load(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        userId = Convert.ToInt32(Request.QueryString["userId"].ToString());
        //        tourDateId = Convert.ToInt32(Request.QueryString["tourDateId"].ToString());
        //        deviceId = Convert.ToString(Request.QueryString["deviceId"].ToString());
        //        ticketType = Convert.ToString(Request.QueryString["ticketType"].ToString());
        //        //mode = Convert.ToString(Request.QueryString["mode"].ToString()) ?? "Cash";
        //        qty = Convert.ToInt32(Request.QueryString["qty"].ToString());
        //    }
        //    catch (Exception ee)
        //    { }
        //    if (!Page.IsPostBack)
        //    {
        //        // Get details of the User 
        //        user = new Musika.Controllers.API.TicketingAPIController().GetUserProfile(userId);
        //        if (user != null)
        //        {
        //            txtName.Text = user.UserName;
        //            txtEmail.Text = user.Email;
        //        }
        //    }
        //}
        #endregion

        int userId;
        int tourDateId;
        string deviceId;
        string ticketType;
        string mode;
        int qty;
        string couponCode;
        decimal cost;

        Musika.Models.UsersModel user = new UsersModel();

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                userId = Convert.ToInt32(Request.QueryString["userId"].ToString());
                tourDateId = Convert.ToInt32(Request.QueryString["tourDateId"].ToString());
                deviceId = Convert.ToString(Request.QueryString["deviceId"].ToString());
                ticketType = Convert.ToString(Request.QueryString["ticketType"].ToString());
                couponCode = Convert.ToString(Request.QueryString["CouponCode"].ToString());
                //mode = Convert.ToString(Request.QueryString["mode"].ToString()) ?? "Cash";
                qty = Convert.ToInt32(Request.QueryString["qty"].ToString());
                cost = new Musika.Repository.SPRepository.SpRepository().GetTicketCost(tourDateId, ticketType);
                if (ticketType.ToLower() == "free")
                {
                    btnSubmit.Visible = true;
                    lblMessage2.Visible = false;
                    divPay.Visible = false;
                }
                else
                {
                    btnSubmit.Visible = false;
                    lblMessage2.Visible = true;
                    divPay.Visible = true;
                }
            }
            catch (Exception)
            {
                //Response.Write(ee.Message + "\n" + ee.StackTrace);
            }

            if (!Page.IsPostBack)
            {
                btnpayment.Enabled = false;
                // Get details of the User 
                user = new Musika.Controllers.API.TicketingAPIController().GetUserProfile(userId);
                if (user != null)
                {
                    txtName.Text = user.UserName;
                    txtEmail.Text = user.Email;
                    txtTicketCost.Text = cost.ToString();
                    //txtTicketCost.Text = String.Format("{0:0.00}", cost).ToString();
                }
            }
        }

        // Make Payment by Card
        protected void btnpayment_Click(object sender, EventArgs e)
        {
            string data = string.Empty;
            string personalData = string.Empty;
            string qrCode = string.Empty;

            string email = string.Empty;
            string mode = "Card";

            data = txtName.Text + txtCardNumber.Text;
            string img = string.Empty;
            int qty = 1;

            #region "Popup Message"
            //string title = "Musika Alert";
            //string body = string.Empty;
            //body += "Before proceeding to make the payment check that the credit or debit card you will use in the purchase is highlighted. Only the holder of the card may withdraw the tickets corresponding to your order. You must present the credit or debit card with physical relief, identity document and printed confirmation (Indispensable)";
            //body += "WE DO NOT ACCEPT CARDS WITHOUT RELIEF.";
            //body += "TERMS AND CONDITIONS UEPA TICKETS";
            //body += "Musika is a registered trademark of Musika SRL, a leading provider of transaction processing, ticketing and bill payment services.";
            //body += "THE USER ACCEPTS VOLUNTARILY THE FOLLOWING TERMS AND CONDITIONS: ";

            //ClientScript.RegisterStartupScript(this.GetType(), "Popup", "ShowPopup2('" + title + "', '" + body + "');", true);

            #endregion


            string ticketNumber = String.Empty;
            TicketEventDetails ticketDetails = new TicketEventDetails();

            personalData += txtName.Text + "~" + txtAddress.Text + "~" + txtCountryState.Text + "~" + txtCity.Text + "~" + txtPostalCode.Text + "~" + txtPhone.Text;

            try
            {
                for (int i = 0; i < qty; i++)
                {
                    //userId,tourDateId,deviceId,ticketType,mode,qty
                    data = tourDateId + "~" + userId + "~" + deviceId + "~" + ticketType + "~" + mode + "~" + qty;

                    // Generate QR Code
                    string dataTicket = Guid.NewGuid().ToString();
                    img = new Musika.Controllers.API.TicketingAPIController().GetZXingQRCode(dataTicket);

                    ticketDetails = new Musika.Controllers.API.TicketingAPIController().GenerateTicketNumber(data, dataTicket, personalData, img);

                    // Save in Database
                    TicketingEventTicketConfirmation ticketConform = new TicketingEventTicketConfirmation();
                    ticketConform.EventID = Convert.ToInt32(ticketDetails.EventID);
                    ticketConform.UserID = Convert.ToInt32(Request.QueryString["userId"].ToString());
                    ticketConform.Dob = DateTime.Now;
                    ticketConform.Gender = ticketDetails.Gender;
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

                    bool res;
                    res = new Musika.Repository.SPRepository.SpRepository().SpAddTicketingEventTicketConfirmation(ticketConform);

                    if (res == true)
                    {
                        email = txtEmail.Text;

                        // Send Mail to User 
                        string html = "<p>Hi," + txtName.Text + "</p>";
                        html += "<p>Thanks for using " + System.Configuration.ConfigurationManager.AppSettings["AppName"] + "! </p>";
                        html += "<p>Your Payment is Successful. </p>";
                        html += "<p>The QR Code Image is attached along with the mail.</p>";

                        html += "<p>Event Name    : " + ticketDetails.EventTitle;
                        html += "<p>Venue Name    : " + ticketDetails.VenueName;
                        html += "<p>City          : " + ticketDetails.City;
                        html += "<p>State         : " + ticketDetails.State;
                        html += "<p>Ticket Number : " + ticketDetails.TicketNumber;

                        html += "<p><br><br><strong>Thanks,The " + System.Configuration.ConfigurationManager.AppSettings["AppName"] + " Team</strong></p>";


                        //EmailHelper.SendEmail(email, System.Configuration.ConfigurationManager.AppSettings["AppName"] + " : QR Code", html);

                        //#region "Send Mail Implementation"

                        //MailMessage mail = new MailMessage();
                        //SmtpClient SmtpServer = new SmtpClient("smtp.sendgrid.net");

                        //string mailFrom = System.Configuration.ConfigurationManager.AppSettings["ADMIN_EMAIL"].ToString();
                        ////mail.From = new MailAddress("mukeshsagar.eminence@gmail.com");
                        //mail.From = new MailAddress(mailFrom);
                        //mail.To.Add(email);
                        //mail.Subject = "Ticketing Payment Confirmation";

                        //mail.Body = Server.HtmlDecode(html);
                        //mail.Body = mail.Body.Replace("<p>", "");
                        //mail.Body = mail.Body.Replace("</p>", "\r\n");
                        //mail.Body = mail.Body.Replace("<br>", "\r");

                        //System.Net.Mail.Attachment attachment;

                        //string filePath = Server.MapPath("/Content/QRCodeImages/" + img);
                        //attachment = new System.Net.Mail.Attachment(filePath);
                        //mail.Attachments.Add(attachment);

                        //SmtpServer.Port = 587;      // 25;
                        //SmtpServer.Credentials = new System.Net.NetworkCredential(ConfigurationManager.AppSettings["ApiKey"], ConfigurationManager.AppSettings["ApiKeyPass"]);
                        //SmtpServer.EnableSsl = true;
                        ////SmtpServer.Send(mail);
                        //#endregion

                        //lblmessage.Text = "Payment Successfull";
                        Response.Write("<script language='javascript'>alert('Payment Successfull....');</script>");
                        //Response.Redirect("TicketEventCheckout.aspx");
                    }
                    else
                    {
                        Response.Write("<script language='javascript'>alert('Payment Not Successfull....');</script>");
                        //lblmessage.Text = "Payment Not Successfull";
                    }
                }

                #region "Commented Code"
                //MailMessage mailMsg = new MailMessage();

                //// To
                //mailMsg.To.Add(new MailAddress("shalludogra.eminence@gmail.com", "Shallu Dogra"));

                //// From
                //mailMsg.From = new MailAddress("mukeshsagar.eminence@gmail.com", "Mukesh Sagar");

                //// Subject and multipart/alternative Body
                //mailMsg.Subject = "subject";
                //string text = "text body";
                //string htmlMail = @"<p>html body</p>";
                //mailMsg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(text, null, MediaTypeNames.Text.Plain));
                //mailMsg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(htmlMail, null, MediaTypeNames.Text.Html));

                //// Init SmtpClient and send
                //SmtpClient smtpClient = new SmtpClient("smtp.sendgrid.net", Convert.ToInt32(587));
                //System.Net.NetworkCredential credentials = new System.Net.NetworkCredential("mukeshsagar.eminence@gmail.com", "admin@1234");
                //smtpClient.Credentials = credentials;

                //smtpClient.Send(mailMsg);
                #endregion

                //lblmessage.Text = "Thanks for your payment.";
                //lblmessage.ForeColor = System.Drawing.Color.Green;
            }
            catch (Exception)
            {
                //Response.Write("Message : " + ee.Message);
                //Response.Write(ee.StackTrace);
                Response.Redirect("TicketEventCheckout.aspx", true);
            }
        }

        // Convert Base 64 String to Image
        public System.Drawing.Image Base64ToImage(string base64String)
        {
            byte[] imageBytes = Convert.FromBase64String(base64String);
            MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length);
            ms.Write(imageBytes, 0, imageBytes.Length);
            System.Drawing.Image image = System.Drawing.Image.FromStream(ms, true);
            return image;
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            string title = "Musika Alert";
            string body = string.Empty;

            #region "Commented Code"
            //string body = "Antes de proceder a realizar el pago compruebe que la tarjeta de credito o debito que utilizara en la compra tenga relieve. Solo el titular de la tarjeta podra retirar los tickets correspondientes a su orden. Debera presentar la tarjeta de credito o debito con relieve fisicamente, documento de identidad y confirmacion impresa (Indispensable)" + Environment.NewLine;
            //body += "NO ACEPTAMOS TARJETAS SIN RELIEVE." + Environment.NewLine;
            //body += "TERMINOS Y CONDICIONES UEPA TICKETS" + Environment.NewLine;
            //body += "Musika es una marca registrada de Musika SRL, proveedor lider de servicios de procesamiento de transacciones, boleteria y pago de facturas." + Environment.NewLine;
            //body += "EL USUARIO ACEPTA VOLUNTARIAMENTE LOS SIGUIENTES TERMINOS Y CONDICIONES:" + Environment.NewLine;
            //body += "El usuario reconoce que Musika no es el Organizador ni el Responsable del Evento y por lo tanto desiste y renuncia irrevocablemente, desde ahora y para siempre y sin reservas de ningun tipo, a demandas, reclamaciones, o instancias que pudiere tener en contra de Musika, sus accionistas, directores, representantes o empleados, empresas subsidiarias o afiliadas, que tengan su fundamento o que esten relacionadas directa o indirectamente con el evento, de cualquier tipo de naturaleza. " + Environment.NewLine;
            //body += "Todas las ventas procesadas a traves de Musika son definitivas y registradas como venta final.No se aceptan devoluciones o cambios luego de procesado la venta." + Environment.NewLine;
            //body += "El usuario reconoce y acepta que cada boleto contiene una numeracion o codigo unico o individual que dara derecho a un solo acceso por boleto, por lo que el titular sera el unico responsable de salvaguardar la confidencialidad del codigo contenido en el mismo.El usuario acepta el riesgo de falsificacion, copia, manipulaciones o duplicidad en cuyo caso solo la primera persona en presentar el boleto o codigo tendra acceso al evento. Un boleto o codigo equivaldra siempre a una sola entrada, sujeto a las condiciones del evento." + Environment.NewLine;
            //body += "Para el retiro de boletas procesadas via www.MusikaApp.com o enlinea, el cliente debe presentar la tarjeta de credito o debito utilizada en su compra, y una identifiacion oficial.unicamente el propietario de la tarjeta puede retirar los boletos. No se aceptan tarjetas de credito y debito sin relieve. " + Environment.NewLine;
            //body += "Musika no asume responsabilidad alguna sobre la organizacion del evento, su postergacion o cancelacion, la cual es asumida directamente por el usuario, quien a su vez asume todo riesgo y peligro derivado del mismo.En caso de lluvia, fallas tecnicas, enfermedad o cualquier otra causa de fuerza mayor, el evento podria ser pospuesto y si sucediera dicha postergacion el ticket mantendra su validez hasta la fijacion de una nueva fecha, la cual sera aceptada por el usuario quien renuncia toda reclamacion por dichos cambios, reconociendo que la postergacion no dara derecho de reembolso. En el caso de que el organizador o responsable del evento anule o cancele el evento, sera responsabilidad exclusiva del mismo la devolucion total o parcial del pago recibido, segun proceda." + Environment.NewLine;
            //body += "En ningun caso seran reembolsados los eventuales gastos o cargos por servicios de boleteria de Musika, si algunos." + Environment.NewLine;
            //body += "El organizador se reserva el derecho de admision al evento, reservandose el derecho de permitir o no el ingreso de cualquier tipo de bebidas, comidas, camaras fotograficas, celulares, filmadoras y / o grabadoras.Esta prohibido ingresar todo tipo de armas de fuego y/ o piezas punzantes o que representen peligro, drogas, farmacos, metales, maderas, rollos de papel, fosforos o encendedores, material pirotecnico, punteros laser, envases de vidrio o plastico. " + Environment.NewLine;
            //body += "BAJO RESERVAS DE DERECHOS DE MUSIKA" + Environment.NewLine;
            //body += "Aceptar";

            //body.Replace("'", "");
            #endregion

            if (ticketType.ToLower() != "free")
            {
                try
                {

                    body += "Before proceeding to make the payment check that the credit or debit card you will use in the purchase is highlighted. Only the holder of the card may withdraw the tickets corresponding to your order. You must present the credit or debit card with physical relief, identity document and printed confirmation (Indispensable)";
                    body += "WE DO NOT ACCEPT CARDS WITHOUT RELIEF.";
                    body += "TERMS AND CONDITIONS UEPA TICKETS";
                    body += "Musika is a registered trademark of Musika SRL, a leading provider of transaction processing, ticketing and bill payment services.";
                    body += "THE USER ACCEPTS VOLUNTARILY THE FOLLOWING TERMS AND CONDITIONS: ";

                    //body 

                    ClientScript.RegisterStartupScript(this.GetType(), "Popup", "ShowPopup('" + title + "', '" + body + "');", true);
                    System.Threading.Thread.Sleep(5000);
                    //Page.ClientScript.RegisterStartupScript(this.GetType(), "Popup", "ShowPopup('" + title + "', '" + body + "');", true); 
                }
                catch (Exception)
                { }
                GeneratePaidTicket();
            }
            else
            {
                body = "Ticket Generated Successfully";
                GenerateFreeTicket();
                ClientScript.RegisterStartupScript(this.GetType(), "Popup", "ShowPopup('" + title + "', '" + body + "');", true);
            }
        }

        private void GeneratePaidTicket()
        {
            // Generate Ticket
            string data = string.Empty;
            string personalData = string.Empty;
            string qrCode = string.Empty;

            string email = string.Empty;
            string mode = "Card";

            data = txtName.Text + txtCardNumber.Text;
            string img = string.Empty;
            int qty = 1;

            string ticketNumber = String.Empty;
            TicketEventDetails ticketDetails = new TicketEventDetails();

            personalData += txtName.Text + "~" + txtAddress.Text + "~" + txtCountryState.Text + "~" + txtCity.Text + "~" + txtPostalCode.Text + "~" + txtPhone.Text;

            try
            {
                for (int i = 0; i < qty; i++)
                {
                    data = tourDateId + "~" + userId + "~" + deviceId + "~" + ticketType + "~" + mode + "~" + qty;

                    // Generate QR Code
                    string dataTicket = Guid.NewGuid().ToString();
                    img = new Musika.Controllers.API.TicketingAPIController().GetZXingQRCode(dataTicket);

                    ticketDetails = new Musika.Controllers.API.TicketingAPIController().GenerateTicketNumber(data, dataTicket, personalData, img);

                    // Save in Database
                    TicketingEventTicketConfirmation ticketConform = new TicketingEventTicketConfirmation();
                    ticketConform.EventID = Convert.ToInt32(ticketDetails.EventID);
                    ticketConform.UserID = Convert.ToInt32(Request.QueryString["userId"].ToString());
                    ticketConform.Dob = DateTime.Now;
                    ticketConform.Gender = ticketDetails.Gender;
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

                    bool res;
                    res = new Musika.Repository.SPRepository.SpRepository().SpAddTicketingEventTicketConfirmation(ticketConform);

                    if (res == true)
                    {
                        email = txtEmail.Text;

                        // Send Mail to User 
                        string html = "<p>Hi," + txtName.Text + "</p>";
                        html += "<p>Thanks for using " + System.Configuration.ConfigurationManager.AppSettings["AppName"] + "! </p>";
                        html += "<p>The QR Code Image is attached along with the mail.</p>";

                        html += "<p>Event Name    : " + ticketDetails.EventTitle;
                        html += "<p>Venue Name    : " + ticketDetails.VenueName;
                        html += "<p>City          : " + ticketDetails.City;
                        html += "<p>State         : " + ticketDetails.State;
                        html += "<p>Ticket Number : " + ticketDetails.TicketNumber;

                        html += "<p><br><br><strong>Thanks,The " + System.Configuration.ConfigurationManager.AppSettings["AppName"] + " Team</strong></p>";


                        //EmailHelper.SendEmail(email, System.Configuration.ConfigurationManager.AppSettings["AppName"] + " : QR Code", html);

                        #region "Send Mail Implementation"

                        MailMessage mail = new MailMessage();
                        SmtpClient SmtpServer = new SmtpClient("smtp.sendgrid.net");

                        string mailFrom = System.Configuration.ConfigurationManager.AppSettings["ADMIN_EMAIL"].ToString();
                        //mail.From = new MailAddress("mukeshsagar.eminence@gmail.com");
                        mail.From = new MailAddress(mailFrom);
                        mail.To.Add(email);
                        mail.Subject = "Ticketing Payment Confirmation";

                        mail.Body = Server.HtmlDecode(html);
                        mail.Body = mail.Body.Replace("<p>", "");
                        mail.Body = mail.Body.Replace("</p>", "\r\n");
                        mail.Body = mail.Body.Replace("<br>", "\r");

                        System.Net.Mail.Attachment attachment;

                        string filePath = Server.MapPath("/Content/QRCodeImages/" + img);
                        attachment = new System.Net.Mail.Attachment(filePath);
                        mail.Attachments.Add(attachment);

                        SmtpServer.Port = 587;      // 25;
                        SmtpServer.Credentials = new System.Net.NetworkCredential(ConfigurationManager.AppSettings["ApiKey"], ConfigurationManager.AppSettings["ApiKeyPass"]);
                        SmtpServer.EnableSsl = true;
                        //SmtpServer.Send(mail);
                        #endregion
                    }
                    else
                    {
                        lblmessage.Text = "";
                    }
                }
            }
            catch (Exception ee)
            {
                //Response.Write("Message : " + ee.Message);
                //Response.Write(ee.StackTrace);
            }
        }

        private void GenerateFreeTicket()
        {
            // Generate Ticket
            string data = string.Empty;
            string personalData = string.Empty;
            string qrCode = string.Empty;

            string email = string.Empty;
            string mode = "Card";
            string img = string.Empty;
            int qty = 1;

            string ticketNumber = String.Empty;
            TicketEventDetails ticketDetails = new TicketEventDetails();

            personalData += txtName.Text + "~" + txtAddress.Text + "~" + txtCountryState.Text + "~" + txtCity.Text + "~" + txtPostalCode.Text + "~" + txtPhone.Text;

            try
            {
                for (int i = 0; i < qty; i++)
                {
                    data = tourDateId + "~" + userId + "~" + deviceId + "~" + ticketType + "~" + mode + "~" + qty;

                    // Generate QR Code
                    string dataTicket = Guid.NewGuid().ToString();
                    img = new Musika.Controllers.API.TicketingAPIController().GetZXingQRCode(dataTicket);

                    ticketDetails = new Musika.Controllers.API.TicketingAPIController().GenerateTicketNumber(data, dataTicket, personalData, img);

                    // Save in Database
                    TicketingEventTicketConfirmation ticketConform = new TicketingEventTicketConfirmation();
                    ticketConform.EventID = Convert.ToInt32(ticketDetails.EventID);
                    ticketConform.UserID = Convert.ToInt32(Request.QueryString["userId"].ToString());
                    ticketConform.Dob = DateTime.Now;
                    ticketConform.Gender = ticketDetails.Gender;
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

                    bool res;
                    res = new Musika.Repository.SPRepository.SpRepository().SpAddTicketingEventTicketConfirmation(ticketConform);

                    if (res == true)
                    {
                        email = txtEmail.Text;

                        // Send Mail to User 
                        string html = "<p>Hi," + txtName.Text + "</p>";
                        html += "<p>Thanks for using " + System.Configuration.ConfigurationManager.AppSettings["AppName"] + "! </p>";
                        html += "<p>The QR Code Image is attached along with the mail.</p>";

                        html += "<p>Event Name    : " + ticketDetails.EventTitle;
                        html += "<p>Venue Name    : " + ticketDetails.VenueName;
                        html += "<p>City          : " + ticketDetails.City;
                        html += "<p>State         : " + ticketDetails.State;
                        html += "<p>Ticket Number : " + ticketDetails.TicketNumber;

                        html += "<p><br><br><strong>Thanks,The " + System.Configuration.ConfigurationManager.AppSettings["AppName"] + " Team</strong></p>";


                        //EmailHelper.SendEmail(email, System.Configuration.ConfigurationManager.AppSettings["AppName"] + " : QR Code", html);

                        #region "Send Mail Implementation"

                        MailMessage mail = new MailMessage();
                        SmtpClient SmtpServer = new SmtpClient("smtp.sendgrid.net");

                        string mailFrom = System.Configuration.ConfigurationManager.AppSettings["ADMIN_EMAIL"].ToString();
                        //mail.From = new MailAddress("mukeshsagar.eminence@gmail.com");
                        mail.From = new MailAddress(mailFrom);
                        mail.To.Add(email);
                        mail.Subject = "Ticketing Payment Confirmation";

                        mail.Body = Server.HtmlDecode(html);
                        mail.Body = mail.Body.Replace("<p>", "");
                        mail.Body = mail.Body.Replace("</p>", "\r\n");
                        mail.Body = mail.Body.Replace("<br>", "\r");

                        System.Net.Mail.Attachment attachment;

                        string filePath = Server.MapPath("/Content/QRCodeImages/" + img);
                        attachment = new System.Net.Mail.Attachment(filePath);
                        mail.Attachments.Add(attachment);

                        SmtpServer.Port = 587;      // 25;
                        SmtpServer.Credentials = new System.Net.NetworkCredential(ConfigurationManager.AppSettings["ApiKey"], ConfigurationManager.AppSettings["ApiKeyPass"]);
                        SmtpServer.EnableSsl = true;
                        //SmtpServer.Send(mail);
                        #endregion
                    }
                    else
                    {
                        lblmessage.Text = "";
                    }
                }
            }
            catch (Exception)
            {
                //Response.Write("Message : " + ee.Message);
                //Response.Write(ee.StackTrace);
            }
        }

        protected void chkAccept_CheckedChanged(object sender, EventArgs e)
        {
            if (chkAccept.Checked == true)
            {
                btnpayment.Enabled = true;
            }
            else
            {
                btnpayment.Enabled = false;
            }
        }

        protected void lnkButton_Click(object sender, EventArgs e)
        {
            string msg = @"Antes de proceder a realizar el pago compruebe que la tarjeta de credito o debito que utilizara en la compra";
            msg += "tenga relieve. Solo el titular de la tarjeta podra retirar los tickets correspondientes a su orden. Debera presentar ";
            msg += "la tarjeta de credito o debito con relieve fisicamente, documento de identidad y confirmacion impresa (Indispensable)";
            
            
            

            
//NO ACEPTAMOS TARJETAS SIN RELIEVE.

//Top of Form
//TERMINOS Y CONDICIONES UEPA TICKETS
//Musika es una marca registrada de Musika SRL, proveedor lider de servicios de procesamiento de transacciones, boleteria y pago de facturas.

//EL USUARIO ACEPTA VOLUNTARIAMENTE LOS SIGUIENTES TERMINOS Y CONDICIONES:

//            El usuario reconoce que Musika no es el Organizador ni el Responsable del Evento y por lo tanto desiste y renuncia irrevocablemente, desde ahora y para siempre y sin reservas de ningun tipo, a demandas, reclamaciones, o instancias que pudiere tener en contra de Musika, sus accionistas, directores, representantes o empleados, empresas subsidiarias o afiliadas, que tengan su fundamento o que estén relacionadas directa o indirectamente con el evento, de cualquier tipo de naturaleza. 

//Todas las ventas procesadas a traves de Musika son definitivas y registradas como venta final.No se aceptan devoluciones o cambios luego de procesado la venta.

//El usuario reconoce y acepta que cada boleto contiene una numeracion o codigo unico o individual que dara derecho a un solo acceso por boleto, por lo que el titular será el único responsable de salvaguardar la confidencialidad del codigo contenido en el mismo.El usuario acepta el riesgo de falsificacion, copia, manipulaciones o duplicidad en cuyo caso solo la primera persona en presentar el boleto o codigo tendra acceso al evento. Un boleto o codigo equivaldra siempre a una sola entrada, sujeto a las condiciones del evento.

//Para el retiro de boletas procesadas via www.MusikaApp.com o enlinea, el cliente debe presentar la tarjeta de credito o debito utilizada en su compra, y una identifiacion oficial.unicamente el propietario de la tarjeta puede retirar los boletos. No se aceptan tarjetas de credito y debito sin relieve. 

//Musika no asume responsabilidad alguna sobre la organizacion del evento, su postergacion o cancelacion, la cual es asumida directamente por el usuario, quien a su vez asume todo riesgo y peligro derivado del mismo.En caso de lluvia, fallas tecnicas, enfermedad o cualquier otra causa de fuerza mayor, el evento podria ser pospuesto y si sucediera dicha postergación el ticket mantendra su validez hasta la fijacion de una nueva fecha, la cual sera aceptada por el usuario quien renuncia toda reclamacion por dichos cambios, reconociendo que la postergacion no dara derecho de reembolso. En el caso de que el organizador o responsable del evento anule o cancele el evento, sera responsabilidad exclusiva del mismo la devolucion total o parcial del pago recibido, segun proceda.

//En ningun caso seran reembolsados los eventuales gastos o cargos por servicios de boletería de Musika, si algunos.

//El organizador se reserva el derecho de admision al evento, reservandose el derecho de permitir o no el ingreso de cualquier tipo de bebidas, comidas, camaras fotograficas, celulares, filmadoras y / o grabadoras.Esta prohibido ingresar todo tipo de armas de fuego y/ o piezas punzantes o que representen peligro, drogas, farmacos, metales, maderas, rollos de papel, fosforos o encendedores, material pirotecnico, punteros laser, envases de vidrio o plastico. 

//BAJO RESERVAS DE DERECHOS DE MUSIKA

//Aceptar"
                

        }
       
    }
}

