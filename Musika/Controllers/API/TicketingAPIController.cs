using ArtSeeker.Library.Common;
using Musika.Areas.Admin.AdminModels.View;
using Musika.Enums;
using Musika.Library.API;
using Musika.Library.CacheProvider;
using Musika.Library.Utilities;
using Musika.Models;
using Musika.Models.API.Input;
using Musika.Models.API.View;
using Musika.Repository.GRepository;
using Musika.Repository.Interface;
using Musika.Repository.SPRepository;
using Musika.Repository.UnitofWork;
using MvcPaging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Configuration;
using System.Web.Http;
using System.Web.Http.Cors;
using System.IO;
using System.Drawing;
using Musika.Library.Multipart;
using ThoughtWorks.QRCode.Codec;
using System.Linq.Dynamic;

using ZXing;
using ZXing.Common;
using System.Drawing.Imaging;
using System.Xml;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Data.SqlClient;
using Newtonsoft.Json;
using Microsoft.Ajax.Utilities;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;
using System.Reflection;

namespace Musika.Controllers.API
{
    [EnableCors(origins: "*", headers: "*", methods: "*", SupportsCredentials = true)]
    public class TicketingAPIController : ApiController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly HttpCache _Cache = new HttpCache();

        int _Imagethumbsize = 0;
        int _imageSize = 0;

        public TicketingAPIController()
        {
            _unitOfWork = new UnitOfWork();
            _Imagethumbsize = Convert.ToInt16(WebConfigurationManager.AppSettings["ImageThumbSize"].ToString());
            _imageSize = Convert.ToInt16(WebConfigurationManager.AppSettings["ImageSize"].ToString());
        }


        private class Results
        {
            public string ErrMessage { get; set; }
            public string Status { get; set; }
        }

        #region "Fine Code for Register New Ticketing User"
        [HttpPost]
        [Route("api/TicketingAPI/RegisterTicketingNewUser")]
        public HttpResponseMessage RegisterTicketingNewUser()           // Working Fine
        {
            string result = string.Empty;

            Models.TicketingUserModel model = new TicketingUserModel();

            Models.TicketingUsers ticketingUserEntity = new Models.TicketingUsers();
            GenericRepository<TicketingUsers> _userEntity = new GenericRepository<TicketingUsers>(_unitOfWork);
            _unitOfWork.StartTransaction();
            try
            {
                InputSignUp Input = new InputSignUp();
                var httpContext = (HttpContextWrapper)Request.Properties["MS_HttpContext"];

                Input.Password = AesCryptography.Encrypt(httpContext.Request.Form["Password"]);
                Input.Email = httpContext.Request.Form["Email"];
                Input.UserName = WebConfigurationManager.AppSettings["XMPPPrefix"].ToString() + httpContext.Request.Form["UserName"];

                Input.DeviceToken = httpContext.Request.Form["DeviceToken"];
                Input.DeviceType = httpContext.Request.Form["DeviceType"];
                Input.DeviceLat = Convert.ToDecimal(httpContext.Request.Form["DeviceLat"]);
                Input.DeviceLong = Convert.ToDecimal(httpContext.Request.Form["DeviceLong"]);
                Input.UserType = httpContext.Request.Form["UserType"];

                var context = new ValidationContext(Input, serviceProvider: null, items: null);
                var results = new List<ValidationResult>();
                var isValid = Validator.TryValidateObject(Input, context, results);

                if (!isValid)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, results[0].ErrorMessage));
                }

                // Check whether the POST operation is MultiPart?
                if (!Request.Content.IsMimeMultipartContent())
                {
                    throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                }

                string _UserName = WebConfigurationManager.AppSettings["XMPPPrefix"].ToString() + Input.UserName;
                Models.TicketingUsers entity = new Models.TicketingUsers();

                ///check USer Name 
                if (!Helper.IsValidPattern(_UserName, "^[a-zA-Z0-9. _-]{2,100}$"))
                {
                    Models.EventReponse tmpResponse = new EventReponse();

                    tmpResponse.ResponseId = 400;
                    tmpResponse.ReturnMessage = "Invalid User Name";

                    model.MessageResponse = tmpResponse;
                    return Request.CreateResponse(HttpStatusCode.NotAcceptable, model);
                }

                ///check email 
                bool Email = false;
                Email = Helper.IsValidEmail(Input.Email);
                if (Email == false)
                {
                    Models.EventReponse tmpResponse = new EventReponse();

                    tmpResponse.ResponseId = 400;
                    tmpResponse.ReturnMessage = "Invalid E-Mail ID";

                    model.MessageResponse = tmpResponse;
                    return Request.CreateResponse(HttpStatusCode.NotAcceptable, model);
                }


                //check if UserName already exists
                TicketingUsers _user = null;

                //check if Email already exists
                _user = _userEntity.Repository.Get(e => e.Email == Input.Email && e.RecordStatus != RecordStatus.Deleted.ToString());
                if (_user != null)
                {
                    Models.EventReponse tmpResponse = new EventReponse();

                    tmpResponse.ResponseId = 400;
                    tmpResponse.ReturnMessage = "Email Already Exits";

                    model.MessageResponse = tmpResponse;
                    return Request.CreateResponse(HttpStatusCode.NotAcceptable, model);
                }

                try
                {
                    entity.Password = Input.Password;
                    entity.Email = Input.Email;
                    entity.UserName = Input.UserName;

                    entity.DeviceToken = Input.DeviceToken;
                    entity.DeviceType = Input.DeviceType;
                    entity.DeviceLat = Input.DeviceLat;
                    entity.DeviceLong = Input.DeviceLong;

                    entity.CreatedDate = DateTime.Now;
                    entity.RecordStatus = RecordStatus.InActive.ToString();

                    entity.UserName = Input.UserName;
                    entity.UserType = Input.UserType;

                    entity.ThumbnailURL = WebConfigurationManager.AppSettings["SiteImgURL"] + "/default-user.png";
                    entity.ImageURL = ConfigurationManager.AppSettings["SiteImgURL"] + "/default-user.png";
                    _userEntity.Repository.Add(entity);

                    SetUserDevices(entity);

                    _unitOfWork.Commit();// End Transaction

                    TicketingUsers tmpUser = new TicketingUsers();
                    tmpUser = _userEntity.Repository.Get(e => e.UserID == entity.UserID);

                    model.UserID = tmpUser.UserID;
                    model.UserType = tmpUser.UserType;
                    model.UserName = tmpUser.UserName;
                    model.Email = tmpUser.Email;
                    model.Password = tmpUser.Password;
                    model.Addres = tmpUser.Addres;
                    model.City = tmpUser.City;
                    model.State = tmpUser.State;
                    model.PostalCode = tmpUser.PostalCode;
                    model.PhoneNumber = tmpUser.PhoneNumber;
                    model.FacebookID = tmpUser.FacebookID;
                    model.ThumbnailURL = tmpUser.ThumbnailURL;
                    model.ImageURL = tmpUser.ImageURL;
                    model.DeviceType = tmpUser.DeviceType;
                    model.DeviceToken = tmpUser.DeviceToken;
                    model.DeviceLat = tmpUser.DeviceLat;
                    model.DeviceLong = tmpUser.DeviceLong;
                    model.RecordStatus = tmpUser.RecordStatus;
                    model.ModifiedDate = tmpUser.ModifiedDate;
                    model.CreatedDate = tmpUser.CreatedDate;
                    model.SynFacebookID = tmpUser.SynFacebookID;
                    model.UserLanguage = tmpUser.UserLanguage;
                    model.Country = tmpUser.Country;

                    Models.EventReponse tmpResponse = new EventReponse();

                    tmpResponse.ResponseId = 200;
                    tmpResponse.ReturnMessage = "Success";

                    model.MessageResponse = tmpResponse;
                    return Request.CreateResponse(HttpStatusCode.OK, model);
                }
                catch (Exception ex)
                {
                    LogHelper.CreateLog3(ex, Request);
                    _unitOfWork.RollBack();//RollBack Transaction
                    return Request.CreateResponse(HttpStatusCode.BadRequest, model);
                }
            }
            catch (Exception ee)
            {
                LogHelper.CreateLog3(ee, Request);
                _unitOfWork.RollBack();//RollBack Transaction
                return Request.CreateResponse(HttpStatusCode.BadRequest, model);
            }
        }
        #endregion

        #region "Random Password Generation for Ticketing User as Staff"
        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        #endregion

        #region "Final Code for Ticketing User Registration"
        #region "Final Code for Ticketing User Registration"
        [HttpPost]
        [Route("api/TicketingAPI/RegisterTicketingUser")]
        public HttpResponseMessage RegisterTicketingUser([FromBody] TicketingUsers users)
        {
            string result = string.Empty;

            Models.TicketingUserModel model = new TicketingUserModel();

            Models.TicketingUsers ticketingUserEntity = new Models.TicketingUsers();
            GenericRepository<TicketingUsers> _userEntity = new GenericRepository<TicketingUsers>(_unitOfWork);
            //_unitOfWork.StartTransaction();
            try
            {
                InputSignUp Input = new InputSignUp();
                var httpContext = (HttpContextWrapper)Request.Properties["MS_HttpContext"];
                string randomPassword = string.Empty;
                //Input.Password = AesCryptography.Encrypt(users.Password);
                if (users.UserType == "Staff")
                {
                    randomPassword = RandomString(6);
                    Input.Password = AesCryptography.Encrypt(randomPassword);
                    Input.CreatedBy = users.CreatedBy;
                    Input.RecordStatus = "Active";
                }
                else
                {
                    Input.Password = AesCryptography.Encrypt(users.Password);
                    Input.CreatedBy = 0;
                    Input.RecordStatus = "InActive";
                }

                Input.Email = users.Email;

                Input.UserName = WebConfigurationManager.AppSettings["XMPPPrefix"].ToString() + users.UserName;

                Input.DeviceToken = users.DeviceToken;
                Input.DeviceType = users.DeviceType;
                Input.DeviceLat = Convert.ToDecimal(users.DeviceLat);
                Input.DeviceLong = Convert.ToDecimal(users.DeviceLong);
                Input.UserType = users.UserType;
                Input.Addres = users.Addres;
                Input.City = users.City;
                Input.State = users.State;
                Input.PostalCode = users.PostalCode;
                Input.PhoneNumber = users.PhoneNumber;
                Input.Country = users.Country;


                var context = new ValidationContext(Input, serviceProvider: null, items: null);
                var results = new List<ValidationResult>();
                var isValid = Validator.TryValidateObject(Input, context, results);

                if (!isValid)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, results[0].ErrorMessage));
                }


                string _UserName = WebConfigurationManager.AppSettings["XMPPPrefix"].ToString() + Input.UserName;
                Models.TicketingUsers entity = new Models.TicketingUsers();

                ///check USer Name 
                if (!Helper.IsValidPattern(_UserName, "^[a-zA-Z0-9. _-]{2,100}$"))
                {
                    Models.EventReponse tmpResponse = new EventReponse();

                    tmpResponse.ResponseId = 400;
                    tmpResponse.ReturnMessage = "Invalid User Name";

                    model.MessageResponse = tmpResponse;
                    return Request.CreateResponse(HttpStatusCode.NotAcceptable, model);
                }

                ///check email 
                bool Email = false;
                Email = Helper.IsValidEmail(Input.Email);
                if (Email == false)
                {
                    Models.EventReponse tmpResponse = new EventReponse();

                    tmpResponse.ResponseId = 400;
                    tmpResponse.ReturnMessage = "Invalid Email ID";

                    model.MessageResponse = tmpResponse;
                    return Request.CreateResponse(HttpStatusCode.NotAcceptable, model);
                }


                //check if UserName already exists
                TicketingUsers _user = null;

                // Check for User Name Duplicacy
                _user = _userEntity.Repository.Get(p => p.UserName == Input.UserName && p.RecordStatus != RecordStatus.Deleted.ToString() && p.UserType ==Input.UserType.ToString());
                if (_user != null)
                {
                    Models.EventReponse tmpResponse = new EventReponse();

                    tmpResponse.ResponseId = 400;
                    tmpResponse.ReturnMessage = "User Name Already Exists";

                    model.MessageResponse = tmpResponse;
                    return Request.CreateResponse(HttpStatusCode.OK, model);
                }

                //check if Email already exists
                _user = _userEntity.Repository.Get(e => e.Email == Input.Email && e.RecordStatus != RecordStatus.Deleted.ToString() && e.UserType == Input.UserType.ToString());
                if (_user != null)
                {
                    Models.EventReponse tmpResponse = new EventReponse();

                    tmpResponse.ResponseId = 400;
                    tmpResponse.ReturnMessage = "E-Mail Already Exists";

                    model.MessageResponse = tmpResponse;
                    return Request.CreateResponse(HttpStatusCode.OK, model);
                }

                try
                {
                    entity.Password = Input.Password;
                    entity.Email = Input.Email;
                    entity.UserName = Input.UserName;

                    entity.DeviceToken = Input.DeviceToken;
                    entity.DeviceType = Input.DeviceType;
                    entity.DeviceLat = Input.DeviceLat;
                    entity.DeviceLong = Input.DeviceLong;

                    entity.CreatedDate = DateTime.Now;
                    //entity.RecordStatus = RecordStatus.InActive.ToString();

                    entity.UserName = Input.UserName;
                    entity.UserType = Input.UserType;

                    entity.Addres = Input.Addres;
                    entity.City = Input.City;
                    entity.State = Input.State;
                    entity.Country = Input.Country;
                    entity.CreatedBy = Input.CreatedBy;
                    entity.PostalCode = Input.PostalCode;
                    entity.PhoneNumber = Input.PhoneNumber;

                    entity.ThumbnailURL = WebConfigurationManager.AppSettings["SiteImgURL"] + "/default-user.png";
                    entity.ImageURL = ConfigurationManager.AppSettings["SiteImgURL"] + "/default-user.png";

                    if (users.UserType == "Staff")
                    {
                        entity.RecordStatus = "Active";
                    }
                    else if (users.UserType == "Event Organizer")
                    {
                        entity.RecordStatus = "InActive";
                    }
                    else
                    {
                        entity.RecordStatus = "InActive";
                    }
                    _userEntity.Repository.Add(entity);

                    SetUserDevices(entity);

                    //_unitOfWork.Commit();// End Transaction

                    TicketingUsers tmpUser = new TicketingUsers();
                    tmpUser = _userEntity.Repository.Get(e => e.UserID == entity.UserID);

                    model.UserID = tmpUser.UserID;
                    model.UserType = tmpUser.UserType;
                    model.UserName = tmpUser.UserName;
                    model.Email = tmpUser.Email;
                    model.Password = tmpUser.Password;
                    model.Addres = tmpUser.Addres;
                    model.City = tmpUser.City;
                    model.State = tmpUser.State;
                    model.PostalCode = tmpUser.PostalCode;
                    model.PhoneNumber = tmpUser.PhoneNumber;
                    model.FacebookID = tmpUser.FacebookID;
                    model.ThumbnailURL = tmpUser.ThumbnailURL;
                    model.ImageURL = tmpUser.ImageURL;
                    model.DeviceType = tmpUser.DeviceType;
                    model.DeviceToken = tmpUser.DeviceToken;
                    model.DeviceLat = tmpUser.DeviceLat;
                    model.DeviceLong = tmpUser.DeviceLong;
                    model.RecordStatus = tmpUser.RecordStatus;
                    model.ModifiedDate = tmpUser.ModifiedDate;
                    model.CreatedDate = tmpUser.CreatedDate;
                    model.SynFacebookID = tmpUser.SynFacebookID;
                    model.UserLanguage = tmpUser.UserLanguage;
                    model.Country = tmpUser.Country;

                    // Send Mail to the New Registered User
                    #region "Mail Functionality"
                    var entitys = _userEntity.Repository.Get(p => p.Email == tmpUser.Email);
                    if (entitys != null)
                    {
                        if (users.UserType == "Staff")
                        {
                            string html = "<p>Hi," + "</p>";
                            html += "<p>Thanks for using " + WebConfigurationManager.AppSettings["AppName"] + "! </p>";
                            html += "<p>You have Successfully Registered in Musika application as Staff Member." + "</p>";
                            html += "<p><br>Your Login Credential is as follows :" + "<p>";
                            html += "<p><br>User Name : " + entitys.UserName + "<p>";
                            html += "<p><br>Password : " + randomPassword + "<p>";
                            html += "<p><br><br><strong>Thanks,The " + WebConfigurationManager.AppSettings["AppName"] + " Team</strong></p>";
                            SendEmailHelper.SendMail(model.Email, "New Staff Member Registration", html, "");
                        }
                        else if (users.UserType == "Event Organizer")
                        {
                            //*************************Activation process************************//

                            SendActivationEmail(tmpUser.UserID, users.Email, tmpUser.UserName);

                            //*********************End***************


                            #endregion
                        }

                    }
                    else
                    {

                    }
                    #endregion

                    Models.EventReponse tmpResponse = new EventReponse();

                    tmpResponse.ResponseId = 200;
                    tmpResponse.ReturnMessage = "Success";

                    model.MessageResponse = tmpResponse;
                    return Request.CreateResponse(HttpStatusCode.OK, model);
                }
                catch (Exception ex)
                {
                    LogHelper.CreateLog3(ex, Request);
                    //_unitOfWork.RollBack();//RollBack Transaction
                    return Request.CreateResponse(HttpStatusCode.BadRequest, model);
                }
            }
            catch (Exception ee)
            {
                LogHelper.CreateLog3(ee, Request);
                _unitOfWork.RollBack();//RollBack Transaction
                return Request.CreateResponse(HttpStatusCode.BadRequest, model);
            }
        }

        private void SendActivationEmail(int userId, string Email, string userName)
        {
            string constr = @"Data Source=23.111.138.246,2728; Initial Catalog=MusikaNew;App=Musika; User ID=sa; Password=sdsol99!;";
            string activationCode = Guid.NewGuid().ToString();
            using (SqlConnection con = new SqlConnection(constr))
            {
                using (SqlCommand cmd = new SqlCommand("INSERT INTO UserActivation VALUES(@UserId, @ActivationCode)"))
                {
                    using (SqlDataAdapter sda = new SqlDataAdapter())
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        cmd.Parameters.AddWithValue("@ActivationCode", activationCode);
                        cmd.Connection = con;
                        con.Open();
                        cmd.ExecuteNonQuery();
                        con.Close();
                    }
                }
            }

            string body = "Hello " + userName.Trim() + ",";
            body += "<br /><br />Please click the following link to activate your account";
            body += "<br /><a href = '" + "http://appserver.musikaapp.com/" + "Activation.aspx?ActivationCode=" + activationCode + "'>Click here to activate your account.</a>";
            body += "<br /><br />Thanks";

            SendEmailHelper.SendMail(Email, "Account Activation", body, "");
            // }
        }
        #endregion


        #region "Approve User for Login by Administrator"
        [HttpPost]
        [Route("api/TicketingAPI/ApproveTicketingUser")]
        public HttpResponseMessage ApproveTicketingUser([FromBody] TicketingUsers users)
        {
            string result = string.Empty;

            Models.TicketingUserModel model = new TicketingUserModel();

            Models.TicketingUsers ticketingUserEntity = new Models.TicketingUsers();
            GenericRepository<TicketingUsers> _userEntity = new GenericRepository<TicketingUsers>(_unitOfWork);
            //_unitOfWork.StartTransaction();
            try
            {
                InputSignUp Input = new InputSignUp();
                var httpContext = (HttpContextWrapper)Request.Properties["MS_HttpContext"];

                Input.Email = users.Email;

                Input.UserName = WebConfigurationManager.AppSettings["XMPPPrefix"].ToString() + users.UserName;

                TicketingUsers _user = null;

                //check if User already exists
                _user = _userEntity.Repository.Get(e => e.Email == Input.Email && e.RecordStatus == RecordStatus.InActive.ToString());
                if (_user != null)
                {
                    _user.RecordStatus = "Active";
                    _userEntity.Repository.Update(_user);

                    model.UserName = _user.UserName;
                    model.UserID = _user.UserID;
                    model.RecordStatus = _user.RecordStatus;
                    model.UserType = _user.UserType;
                    model.Email = _user.Email;
                    model.Password = _user.Password;
                    model.Addres = _user.Addres;
                    model.City = _user.City;
                    model.State = _user.State;
                    model.Country = _user.Country;
                    model.PostalCode = _user.PostalCode;
                    model.PhoneNumber = _user.PhoneNumber;

                    Models.EventReponse tmpResponse = new EventReponse();

                    tmpResponse.ResponseId = 200;
                    tmpResponse.ReturnMessage = "Status of User is Active";

                    model.MessageResponse = tmpResponse;

                    // Send Mail to User for Approval Confirmation

                    string html = "<p>Hi," + "</p>";
                    html += "<p>Thanks for using " + WebConfigurationManager.AppSettings["AppName"] + "! </p>";
                    html += "<p>You have Successfully Registered & Approved in Musika application as Event Organizer." + "</p>";
                    html += "<p><br><br><strong>Thanks,The " + WebConfigurationManager.AppSettings["AppName"] + " Team</strong></p>";

                    //EmailHelper.SendEmail(Input.Email, WebConfigurationManager.AppSettings["AppName"] + " : New Event Organizer", html);
                    //#region "Send Mail Implementation"

                    //MailMessage mail = new MailMessage();
                    //SmtpClient SmtpServer = new SmtpClient("smtp.sendgrid.net");

                    //string mailFrom = System.Configuration.ConfigurationManager.AppSettings["ADMIN_EMAIL"].ToString();
                    //mail.From = new MailAddress(mailFrom);
                    //mail.To.Add(model.Email);
                    //mail.Subject = "New Event Organizer Registration";

                    //mail.IsBodyHtml = true;
                    //mail.Body = html;

                    //SmtpServer.Port = 587;      // 25;
                    //SmtpServer.Credentials = new System.Net.NetworkCredential(ConfigurationManager.AppSettings["ApiKey"], ConfigurationManager.AppSettings["ApiKeyPass"]);
                    //SmtpServer.EnableSsl = true;
                    //SmtpServer.Send(mail);

                    //#endregion
                    SendEmailHelper.SendMail(model.Email, "New Event Organizer Registration", html, "");
                    return Request.CreateResponse(HttpStatusCode.OK, model);
                }
                else
                {
                    Models.EventReponse tmpResponse = new EventReponse();

                    tmpResponse.ResponseId = 400;
                    tmpResponse.ReturnMessage = "Failure";

                    model.MessageResponse = tmpResponse;
                    return Request.CreateResponse(HttpStatusCode.OK, model);
                }
            }
            catch (Exception)
            {
                Models.EventReponse tmpResponse = new EventReponse();

                tmpResponse.ResponseId = 400;
                tmpResponse.ReturnMessage = "Unexpected Error at Run Time";

                model.MessageResponse = tmpResponse;
                return Request.CreateResponse(HttpStatusCode.OK, model);
            }
        }
        #endregion

        #region "Final Code for Authenticate User by User Name or EmailID"
        // Purpose :
        // Procedure List:
        // Tables Used:
        [HttpPost]
        [Route("api/TicketingAPI/AuthenticateUser")]
        public HttpResponseMessage AuthenticateUser([FromBody] TicketingUsers users)
        {
            Models.TicketingUsers _Users = null;
            string result = string.Empty;
            Models.TicketingUserModel model = new TicketingUserModel();

            Models.TicketingUsers ticketingUserEntity = new Models.TicketingUsers();
            GenericRepository<TicketingUsers> _userEntity = new GenericRepository<TicketingUsers>(_unitOfWork);

            var httpContext = (HttpContextWrapper)Request.Properties["MS_HttpContext"];

            GenericRepository<TicketingUsers> _UsersRepo = new GenericRepository<TicketingUsers>(_unitOfWork);

            string tmpPwd = AesCryptography.Encrypt(users.Password);

            if (users.UserName.Contains("@"))
            {
                //_Users = _UsersRepo.Repository.Get(p => p.Email == users.UserName && p.Password == tmpPwd && p.RecordStatus == "Active");
                _Users = _UsersRepo.Repository.Get(p => p.Email == users.UserName && p.Password == tmpPwd && p.RecordStatus == "Active");
            }
            else if (!users.UserName.Contains("@"))
            {
                //_Users = _UsersRepo.Repository.Get(p => p.UserName == users.UserName && p.Password == tmpPwd && p.RecordStatus == "Active");
                _Users = _UsersRepo.Repository.Get(p => p.UserName == users.UserName && p.Password == tmpPwd && p.RecordStatus == "Active");
            }

            if (_Users == null)
            {
                //result = "Not An Authenticated User";
                Models.EventReponse tmpResponse = new EventReponse();

                if (_UsersRepo.Repository.Get(p => p.UserName == users.UserName) == null)
                {
                    tmpResponse.ReturnMessage = "Not An Authenticated User";
                }
                else if (_UsersRepo.Repository.Get(p => p.UserName == users.UserName && users.Password == users.Password) == null)
                {
                    tmpResponse.ReturnMessage = "Invalid User Name and Password Combination";
                }
                else if (_UsersRepo.Repository.Get(p => p.UserName == users.UserName && users.Password == users.Password && users.RecordStatus == "InActive") == null)
                {
                    tmpResponse.ReturnMessage = "User Must Activate the Account First through Email";
                }
                tmpResponse.ResponseId = 400;
                //tmpResponse.ReturnMessage = "Not An Authenticated User";

                model.MessageResponse = tmpResponse;
                return Request.CreateResponse(HttpStatusCode.NonAuthoritativeInformation, model);
            }
            else
            {
                //result = "Authenticated User";
                TicketingUsers tmpUser = new TicketingUsers();

                if (users.UserName.Contains("@"))
                {
                    //_Users = _UsersRepo.Repository.Get(p => p.Email == users.UserName && p.Password == tmpPwd && p.RecordStatus == "Active");
                    tmpUser = _userEntity.Repository.Get(p => p.Email == users.UserName);
                }
                else if (!users.UserName.Contains("@"))
                {
                    //_Users = _UsersRepo.Repository.Get(p => p.UserName == users.UserName && p.Password == tmpPwd && p.RecordStatus == "Active");
                    tmpUser = _userEntity.Repository.Get(p => p.UserName == users.UserName);
                }


                //if (!String.IsNullOrEmpty(users.UserName))
                //    tmpUser = _userEntity.Repository.Get(e => e.UserName == users.UserName);
                //else
                //    tmpUser = _userEntity.Repository.Get(e => e.Email == users.Email);

                model.UserID = tmpUser.UserID;
                model.UserType = tmpUser.UserType;
                model.UserName = tmpUser.UserName;
                model.Email = tmpUser.Email;

                model.RecordStatus = tmpUser.RecordStatus;


                Models.EventReponse tmpResponse = new EventReponse();

                tmpResponse.ResponseId = 200;
                tmpResponse.ReturnMessage = "Authenticated User";

                SetUserDevices(tmpUser);
                model.MessageResponse = tmpResponse;
                return Request.CreateResponse(HttpStatusCode.OK, model);
            }
        }
        #endregion

        #region "Final Code for Authenticate User by User Name or EmailID, Device Token and Device Type"
        [HttpPost]
        [Route("api/TicketingAPI/AuthenticateUserByMobile")]
        public HttpResponseMessage AuthenticateUserByMobile([FromBody] TicketingUsers users)
        {

            Models.TicketingUsers _Users = null;
            string result = string.Empty;
            Models.TicketingUserModel model = new TicketingUserModel();

            Models.TicketingUsers ticketingUserEntity = new Models.TicketingUsers();
            GenericRepository<TicketingUsers> _userEntity = new GenericRepository<TicketingUsers>(_unitOfWork);

            var httpContext = (HttpContextWrapper)Request.Properties["MS_HttpContext"];

            GenericRepository<TicketingUsers> _UsersRepo = new GenericRepository<TicketingUsers>(_unitOfWork);

            string tmpPwd = AesCryptography.Encrypt(users.Password);

            try
            {
                if (users.UserName.Contains("@"))
                {
                    _Users = _UsersRepo.Repository.Get(p => p.Email == users.UserName && p.Password == tmpPwd && p.RecordStatus == "Active");
                }
                else
                {
                    _Users = _UsersRepo.Repository.Get(p => p.UserName == users.UserName && p.Password == tmpPwd && p.RecordStatus == "Active");
                }
                if (_Users == null)
                {
                    //result = "Not An Authenticated User";
                    Models.EventReponse tmpResponse = new EventReponse();

                    tmpResponse.ResponseId = 400;
                    tmpResponse.ReturnMessage = "Email/Password do not match";

                    model.MessageResponse = tmpResponse;
                    return Request.CreateResponse(HttpStatusCode.NonAuthoritativeInformation, model);
                }
                else
                {
                    TicketingUsers tmpUser = new TicketingUsers();
                    if (!users.UserName.Contains("@"))
                    {
                        tmpUser = _userEntity.Repository.Get(e => e.UserName == users.UserName);
                    }
                    else
                    {
                        tmpUser = _userEntity.Repository.Get(e => e.Email == users.UserName);
                    }
                    model.UserID = tmpUser.UserID;
                    model.UserType = tmpUser.UserType;
                    model.UserName = tmpUser.UserName;
                    model.Email = tmpUser.Email;
                    model.Password = tmpUser.Password;
                    model.Addres = tmpUser.Addres;
                    model.City = tmpUser.City;
                    model.State = tmpUser.State;
                    model.PostalCode = tmpUser.PostalCode;
                    model.PhoneNumber = tmpUser.PhoneNumber;
                    model.FacebookID = tmpUser.FacebookID;
                    model.ThumbnailURL = tmpUser.ThumbnailURL;
                    model.ImageURL = tmpUser.ImageURL;
                    model.DeviceType = tmpUser.DeviceType;
                    model.DeviceToken = tmpUser.DeviceToken;
                    model.DeviceLat = tmpUser.DeviceLat;
                    model.DeviceLong = tmpUser.DeviceLong;
                    model.RecordStatus = tmpUser.RecordStatus;
                    model.ModifiedDate = tmpUser.ModifiedDate;
                    model.CreatedDate = tmpUser.CreatedDate;
                    model.SynFacebookID = tmpUser.SynFacebookID;
                    model.UserLanguage = tmpUser.UserLanguage;
                    model.Country = tmpUser.Country;

                    Models.EventReponse tmpResponse = new EventReponse();

                    tmpResponse.ResponseId = 200;
                    tmpResponse.ReturnMessage = "Authenticated User";

                    SetUserDevices(tmpUser);
                    model.MessageResponse = tmpResponse;
                    return Request.CreateResponse(HttpStatusCode.OK, model);
                }
            }
            catch (Exception ex)
            {
                Models.EventReponse tmpResponse = new EventReponse();

                tmpResponse.ResponseId = 400;
                tmpResponse.ReturnMessage = "Unexpected Data Values";

                model.MessageResponse = tmpResponse;
                return Request.CreateResponse(HttpStatusCode.NonAuthoritativeInformation, model);
            }
        }
        #endregion

        #region "Create / Update Ticketing Events"
        //[AllowAnonymous]
        [HttpPost]
        [Route("api/TicketingAPI/UpdateTicketingEvent")]
        public HttpResponseMessage UpdateTicketingEvent([FromBody] TicketingEventsNewModel events)
        {
            Models.EventReponse tmpResponse = new EventReponse();
            GenericRepository<TicketingUsers> _UsersRepo = new GenericRepository<TicketingUsers>(_unitOfWork);
            Models.TicketingEventTicketsSummary ticketingEventSummary = new Models.TicketingEventTicketsSummary();

            Models.TicketingEventsNew entity = new TicketingEventsNew();
            Models.TicketingEventNewStaff staffEntity = new TicketingEventNewStaff();
            string imageName = "";
            int eventCreatedUser = 0;
            bool Isedit = false;
            try
            {
                if (events != null)
                {
                    entity.EventTitle = events.EventTitle;
                    entity.EventLocation = events.EventLocation;
                    entity.VenueName = events.VenueName;
                    entity.Address1 = events.Address1;
                    entity.Address2 = events.Address2;
                    entity.City = events.City;
                    entity.State = events.State;
                    entity.ZipCode = events.ZipCode;
                    entity.StartDate = Convert.ToDateTime(events.StartDate);
                    entity.StartTime = events.StartTime;
                    entity.EndDate = Convert.ToDateTime(events.EndDate);
                    entity.EndTime = events.EndTime;
                    if (!String.IsNullOrEmpty(events.EventImage))
                    {
                        imageName = SaveImage(events.EventImage.Split(',')[1], "Event_" + events.EventID);
                        entity.EventImage = imageName;
                    }
                    else
                    {
                        entity.EventImage = events.EventImage;
                    }
                    entity.EventDescription = events.EventDescription;
                    entity.OrganizerName = events.OrganizerName;
                    entity.OrganizerDescription = events.OrganizerDescription;
                    entity.TicketType = events.TicketType;
                    entity.ListingPrivacy = events.ListingPrivacy;
                    entity.EventType = events.EventType;
                    entity.EventTopic = events.EventTopic;
                    entity.ShowTicketNumbers = 1;
                    entity.CreatedBy = events.CreatedBy;
                    entity.CreatedOn = Convert.ToDateTime(DateTime.Now);
                    entity.ISDELETED = false;
                    entity.IsApproved = events.IsApproved;
                    entity.NumberOfTickets = events.NumberOfTickets;
                    entity.ArtistId = Convert.ToInt32(events.ArtistId);
                    //entity.StaffId = Convert.ToInt32(events.StaffId);
                    entity.TicketUrl = "http://appserver.musikaapp.com/TicketEventCheckout.aspx";
                    Models.TicketingUsers _Users = null;
                    eventCreatedUser = Convert.ToInt32(events.CreatedBy);
                    _Users = _UsersRepo.Repository.GetById(eventCreatedUser);
                    string ctr;
                    Models.TicketingEventsNewModel entity1 = new Models.TicketingEventsNewModel();

                    if (String.IsNullOrEmpty(events.EventID.ToString()) || events.EventID == 0)
                    {
                        ctr = new Musika.Repository.SPRepository.SpRepository().SpAddTicketingEvent(entity).ToString();
                        if (ctr != "0")
                        {
                            entity1.EventID = Convert.ToInt32(ctr);
                            if (events.lstStaff.Count > 0)
                            {
                                for (int i = 0; i < events.lstStaff.Count; i++)
                                {
                                    new Musika.Repository.SPRepository.SpRepository().SpAddTicketingEventStaff(entity1.EventID, events.lstStaff[i].UserId);
                                }
                            }

                            // Send Mail to Administrator for Event Approval from Admin End

                            string html = "<p>Hi Administrator,</p>";
                            html += "<p>Thanks for using " + WebConfigurationManager.AppSettings["AppName"] + "! </p>";
                            html += "<p>The Following newly created Event by " + _Users.UserName.ToString() + " is pending for Approval: </p>";
                            html += "<p><br>Event Name : " + entity.EventTitle + "</p>";
                            html += "<p><br>Start Date : " + entity.StartDate + "</p>";
                            html += "<p><br>Created By   : " + _Users.UserName.ToString() + "</p>";
                            html += "<p><br><br><strong>Thanks,The " + WebConfigurationManager.AppSettings["AppName"] + " Team</strong></p>";
                            SendEmailHelper.SendMail(WebConfigurationManager.AppSettings["EventAdmin"], "New Event Registration", html, "");
                        }
                    }
                    else
                    {
                        Isedit = true;
                        // Avoid Update if the ticket is sold
                        entity.EventID = Convert.ToInt32(events.EventID);
                        // entity.IsApproved = true;
                        ctr = new Musika.Repository.SPRepository.SpRepository().SpUpdateTicketingEventTemp(entity).ToString();
                        entity1.EventID = Convert.ToInt32(events.EventID);
                    }

                    if (Convert.ToInt32(ctr) > 0)
                    {
                        entity1.EventTitle = events.EventTitle;
                        entity1.EventLocation = events.EventLocation;
                        entity1.VenueName = events.VenueName;
                        entity1.Address1 = events.Address1;
                        entity1.Address2 = events.Address2;
                        entity1.City = events.City;
                        entity1.State = events.State;
                        entity1.ZipCode = events.ZipCode;
                        entity1.StartDate = Convert.ToDateTime(events.StartDate);
                        entity1.StartTime = events.StartTime;
                        entity1.EndDate = Convert.ToDateTime(events.EndDate);
                        entity1.EndTime = events.EndTime;
                        entity1.EventImage = events.EventImage;
                        entity1.EventDescription = events.EventDescription;
                        entity1.OrganizerName = events.OrganizerName;
                        entity1.OrganizerDescription = events.OrganizerDescription;
                        entity1.TicketType = events.TicketType;
                        entity1.ListingPrivacy = events.ListingPrivacy;
                        entity1.EventType = events.EventType;
                        entity1.EventTopic = events.EventTopic;
                        entity1.ShowTicketNumbers = events.ShowTicketNumbers;
                        entity1.CreatedBy = events.CreatedBy;
                        entity1.CreatedOn = Convert.ToDateTime(events.CreatedOn);
                        entity1.Isdeleted = false;
                        entity1.IsApproved = events.IsApproved;
                        entity1.NumberOfTickets = events.NumberOfTickets;
                        entity1.ArtistId = Convert.ToInt32(events.ArtistId);

                        entity1.lstStaff = events.lstStaff;
                        if (!Isedit)
                        {
                            #region "Delete Existing Staff and add again"
                            //new Musika.Repository.SPRepository.SpRepository().SpDeleteTicketingStaff(entity1.EventID);
                            if (events.lstStaff.Count > 0)
                            {
                                for (int i = 0; i < events.lstStaff.Count; i++)
                                {
                                    new Musika.Repository.SPRepository.SpRepository().SpUpdateTicketingEventStaff(entity1.EventID, events.lstStaff[i].UserId);
                                }

                            }
                            #endregion

                            #region "Delete Existing Records for TicketingEventTicketSummary"
                            new Musika.Repository.SPRepository.SpRepository().SpDeleteTicketingEventTicketSummary(entity1.EventID);
                            #endregion

                            //if (events.Ticket != null && events.Ticket.lstTicketData.Count == 0)
                            //{
                            //    dynamic jsonSt = JsonConvert.DeserializeObject<dynamic>(events.Ticket.lstTicketData[0].PackageStartDate);
                            //    dynamic startdate = jsonSt["formatted"];
                            //    dynamic jsonEnd = JsonConvert.DeserializeObject<dynamic>(events.Ticket.lstTicketData[0].PackageEndDate);
                            //    dynamic enddatedate = jsonEnd["formatted"];
                            //    new Musika.Repository.SPRepository.SpRepository().SpUpdateTicketingEventTicketSummary(entity1.EventID, String.Empty, String.Empty, 0, 0, events.Ticket.CountryId, events.Ticket.Currency, events.Ticket.ServiceFee, events.Ticket.Tax, events.Ticket.RefundPolicy, startdate, enddatedate);
                            //}
                            //else
                            if (events.Ticket != null && events.Ticket.lstTicketData.Count > 0)
                            {
                                for (int i = 0; i < events.Ticket.lstTicketData.Count; i++)
                                {
                                    dynamic jsonSt = JsonConvert.DeserializeObject<dynamic>(events.Ticket.lstTicketData[i].PackageStartDate);
                                    string startdate = jsonSt["formatted"];
                                    dynamic jsonEnd = JsonConvert.DeserializeObject<dynamic>(events.Ticket.lstTicketData[i].PackageEndDate);
                                    string enddatedate = jsonEnd["formatted"];
                                    new Musika.Repository.SPRepository.SpRepository().SpUpdateTicketingEventTicketSummary(entity1.EventID, events.Ticket.lstTicketData[i].TicketCategory, events.Ticket.lstTicketData[i].TicketType, events.Ticket.lstTicketData[i].Price, events.Ticket.lstTicketData[i].Quantity, events.Ticket.CountryId, events.Ticket.Currency, events.Ticket.ServiceFee, events.Ticket.Tax, events.Ticket.RefundPolicy, startdate, enddatedate);
                                }

                            }
                        }

                        else
                        {
                            #region "Delete Existing Staff and add again"
                            //new Musika.Repository.SPRepository.SpRepository().SpDeleteTicketingStaff(entity1.EventID);
                            if (events.lstStaff.Count > 0)
                            {
                                for (int i = 0; i < events.lstStaff.Count; i++)
                                {
                                    new Musika.Repository.SPRepository.SpRepository().SpUpdateTicketingEventStaffTemp(entity1.EventID, events.lstStaff[i].UserId);
                                }

                            }
                            #endregion
                            Models.TicketingEventsNew _Events = null;
                            GenericRepository<TicketingEventsNew> _EventsRepo = new GenericRepository<TicketingEventsNew>(_unitOfWork);
                            _Events = _EventsRepo.Repository.GetById(entity1.EventID);
                            if (_Events != null)
                            {
                                if (_Events.EventID > 0) // if not admin
                                {
                                    _Events.IsApproved = false;
                                    _EventsRepo.Repository.Update(_Events);
                                }
                            }

                            #region "Delete Existing Records for TicketingEventTicketSummary"
                            new Musika.Repository.SPRepository.SpRepository().SpDeleteTicketingEventTicketSummaryTemp(entity1.EventID);
                            #endregion

                            //if (events.Ticket != null && events.Ticket.lstTicketData.Count == 0)
                            //{
                            //    dynamic jsonSt = JsonConvert.DeserializeObject<dynamic>(events.Ticket.lstTicketData[0].PackageStartDate);
                            //    dynamic startdate = jsonSt["formatted"];
                            //    dynamic jsonEnd = JsonConvert.DeserializeObject<dynamic>(events.Ticket.lstTicketData[0].PackageEndDate);
                            //    dynamic enddatedate = jsonEnd["formatted"];
                            //    new Musika.Repository.SPRepository.SpRepository().SpUpdateTicketingEventTicketSummaryTemp(entity1.EventID, String.Empty, String.Empty, 0, 0, events.Ticket.CountryId, events.Ticket.Currency,
                            //        events.Ticket.ServiceFee, events.Ticket.Tax, events.Ticket.RefundPolicy, startdate, enddatedate);
                            //}
                            //else 
                            if (events.Ticket != null && events.Ticket.lstTicketData.Count > 0)
                            {
                                for (int i = 0; i < events.Ticket.lstTicketData.Count; i++)
                                {
                                    dynamic jsonSt = JsonConvert.DeserializeObject<dynamic>(events.Ticket.lstTicketData[i].PackageStartDate);
                                    string startdate = jsonSt["formatted"];
                                    dynamic jsonEnd = JsonConvert.DeserializeObject<dynamic>(events.Ticket.lstTicketData[i].PackageEndDate);
                                    string enddatedate = jsonEnd["formatted"];
                                    new Musika.Repository.SPRepository.SpRepository().SpUpdateTicketingEventTicketSummaryTemp(entity1.EventID, events.Ticket.lstTicketData[i].TicketCategory, events.Ticket.lstTicketData[i].TicketType, events.Ticket.lstTicketData[i].Price, events.Ticket.lstTicketData[i].Quantity, events.Ticket.CountryId, events.Ticket.Currency, events.Ticket.ServiceFee, events.Ticket.Tax, events.Ticket.RefundPolicy, startdate, enddatedate);
                                }

                            }
                            string html = "Event Changes Approval of " + _Users.UserName.ToString() + "<br><br>" + "Event Title = " + events.EventTitle;
                            html += " <table border=" + 1 + " cellpadding=" + 0 + " cellspacing=" + 0 + " width = " + 400 + "><tr bgcolor='#4da6ff'><td><b> NAME </b></td><td><b>Old Value</b></td> <td> <b> New Value</b> </td></tr>";
                            Dictionary<string, string> lstdiff = new Dictionary<string, string>();
                            lstdiff = new Musika.Repository.SPRepository.SpRepository().SpGetEventTicketStaffDetails(entity1.EventID);
                            foreach (KeyValuePair<string, string> pair in lstdiff)
                            {
                                string[] value = pair.Value.Split('#');
                                if (pair.Key == "EventImage")
                                {
                                    //  String path = HttpContext.Current.Server.MapPath("~/Content/EventImages");
                                    String path = "http://appserver.musikaapp.com/Content/EventImages/"; //Path
                                    html += "<tr><td>" + pair.Key + "</td><td><img width='200px' height='200px' src='" + path + value[0].ToString() + "'/> </td><td><img width='200px' height='200px' src='" + path + value[1].ToString() + "'/></td> </tr>";
                                }
                                else if ((pair.Key != "IsApproved") && (pair.Key != "CreatedOn") && (pair.Key != "RefundPolicy"))
                                {
                                    html += "<tr><td>" + pair.Key + "</td><td> " + value[0].ToString() + "</td><td> " + value[1].ToString() + "</td> </tr>";
                                }

                            }
                            html += "</table>";
                            html += "<br /><a href = '" + "http://appserver.musikaapp.com/" + "EventApproval.aspx?ID=" + events.EventID + "'>Click here to Approve the changes.</a>";

                            SendEmailHelper.SendMail(WebConfigurationManager.AppSettings["EventAdmin"], "Event Changes Approval", html, "");
                        }
                        tmpResponse.ResponseId = 200;
                        tmpResponse.ReturnMessage = "Success";
                    }
                    else if (Convert.ToInt32(ctr) == -1)
                    {
                        tmpResponse.ResponseId = 200;
                        tmpResponse.ReturnMessage = "Tickets Already Sold";
                    }
                }
                else
                {
                    tmpResponse.ResponseId = 400;
                    tmpResponse.ReturnMessage = "Unexpected Error";
                }
            }
            catch (Exception ex)
            {
                tmpResponse.ResponseId = 400;
                //tmpResponse.ReturnMessage = "Unexpected Error";
                tmpResponse.ReturnMessage = ex.Message + "\n" + ex.StackTrace;
            }
            //return Request.CreateResponse(HttpStatusCode.OK, entity);
            return Request.CreateResponse(HttpStatusCode.OK, tmpResponse);
        }
        #endregion

        #region "Final Delete Ticketing Event by ID"
        [HttpGet]
        [Route("api/TicketingAPI/DeleteTicketingEventById")]
        public HttpResponseMessage DeleteTicketingEventById(string eventID)
        {
            string result = string.Empty;

            Models.TicketingEventsNew ticketingEventEntity = new Models.TicketingEventsNew();

            Models.TicketingEventsNewModel model = new TicketingEventsNewModel();
            GenericRepository<TicketingEventsNew> _ticketingEntity = new GenericRepository<TicketingEventsNew>(_unitOfWork);

            try
            {
                Models.TicketingEventsNew entity = new Models.TicketingEventsNew();
                int id = Convert.ToInt32(eventID);
                ticketingEventEntity = _ticketingEntity.Repository.Get(p => p.EventID == id);

                if (ticketingEventEntity != null)
                {
                    ticketingEventEntity.ISDELETED = true;
                }

                _ticketingEntity.Repository.Update(ticketingEventEntity);
                Models.EventReponse tmpResponse = new EventReponse();

                tmpResponse.ResponseId = 200;
                tmpResponse.ReturnMessage = "Success";

                model.EventID = entity.EventID;
                model.MessageResponse = tmpResponse;
                //return Request.CreateResponse(HttpStatusCode.OK, model);
                return Request.CreateResponse(HttpStatusCode.OK, tmpResponse);
            }
            catch (Exception ee)
            {
                LogHelper.CreateLog3(ee, Request);
                _unitOfWork.RollBack();//RollBack Transaction
                Models.EventReponse tmpResponse = new EventReponse();

                tmpResponse.ResponseId = 400;
                tmpResponse.ReturnMessage = "Error";

                return Request.CreateResponse(HttpStatusCode.OK, tmpResponse);
            }
        }
        #endregion


        #region "Final Delete Ticketing Event"
        [HttpPost]
        [Route("api/TicketingAPI/DeleteTicketingEvent")]
        public HttpResponseMessage DeleteTicketingEvent([FromBody] TicketingEventsNewModel events)
        {
            string result = string.Empty;

            Models.TicketingEventsNewModel ticketingEventEntity = new Models.TicketingEventsNewModel();
            GenericRepository<TicketingEventsNew> _ticketingEntity = new GenericRepository<TicketingEventsNew>(_unitOfWork);
            _unitOfWork.StartTransaction();

            try
            {
                Models.TicketingEventsNew entity = new Models.TicketingEventsNew();

                entity.Address1 = events.Address1;
                entity.Address2 = events.Address2;
                entity.City = events.City;
                entity.EndDate = events.EndDate;
                entity.EndTime = events.EndTime;
                entity.EventDescription = events.EventDescription;
                entity.EventID = events.EventID;
                entity.EventImage = events.EventImage;
                entity.EventLocation = events.EventLocation;
                entity.EventTitle = events.EventTitle;
                entity.EventTopic = events.EventTopic;
                entity.EventType = events.EventType;
                entity.ListingPrivacy = events.ListingPrivacy;
                entity.OrganizerDescription = events.OrganizerDescription;
                entity.OrganizerName = events.OrganizerName;
                entity.ShowTicketNumbers = events.ShowTicketNumbers;
                entity.StartDate = events.StartDate;
                entity.StartTime = events.StartTime;
                entity.State = events.State;
                entity.TicketType = events.TicketType;
                entity.VenueName = events.VenueName;
                entity.ZipCode = events.ZipCode;
                entity.CreatedBy = events.CreatedBy;
                entity.CreatedOn = events.CreatedOn;

                string tmp = (entity.ISDELETED == true ? "1" : "0");
                if (tmp.Equals("1"))
                {
                    entity.ISDELETED = true;
                }
                else
                {
                    entity.ISDELETED = false;
                }
                _ticketingEntity.Repository.Update(entity);
                Models.EventReponse tmpResponse = new EventReponse();

                tmpResponse.ResponseId = 200;
                tmpResponse.ReturnMessage = "Success";

                ticketingEventEntity.MessageResponse = tmpResponse;
                return Request.CreateResponse(HttpStatusCode.OK, ticketingEventEntity);
            }
            catch (Exception ee)
            {
                LogHelper.CreateLog3(ee, Request);
                _unitOfWork.RollBack();//RollBack Transaction
                Models.EventReponse tmpResponse = new EventReponse();

                tmpResponse.ResponseId = 400;
                tmpResponse.ReturnMessage = "Error";

                ticketingEventEntity.MessageResponse = tmpResponse;
                return Request.CreateResponse(HttpStatusCode.BadRequest, ticketingEventEntity);
            }
        }
        #endregion

        #region "Listing of Events"
        [HttpPost]
        [Route("api/TicketingAPI/GetTicketingEvents")]
        public HttpResponseMessage GetTicketingEvents()
        {
            string result = string.Empty;
            List<Musika.Models.TicketingEventsNew> lstTicketingEvents = new List<Musika.Models.TicketingEventsNew>();

            Models.TicketingEventsNewModel model = new TicketingEventsNewModel();
            #region "Old Code"
            GenericRepository<TicketingEventsNew> _ticketingEntity = new GenericRepository<TicketingEventsNew>(_unitOfWork);
            _unitOfWork.StartTransaction();

            try
            {
                lstTicketingEvents = _ticketingEntity.Repository.GetAll();

                Models.EventReponse tmpResponse = new EventReponse();

                tmpResponse.ResponseId = 200;
                tmpResponse.ReturnMessage = "Success";

                model.MessageResponse = tmpResponse;
                return Request.CreateResponse(HttpStatusCode.OK, lstTicketingEvents);
            }
            catch (Exception ee)
            {
                LogHelper.CreateLog3(ee, Request);
                _unitOfWork.RollBack();//RollBack Transaction

                Models.EventReponse tmpResponse = new EventReponse();

                tmpResponse.ResponseId = 400;
                tmpResponse.ReturnMessage = "Failure";

                model.MessageResponse = tmpResponse;
                return Request.CreateResponse(HttpStatusCode.OK, lstTicketingEvents);
            }
            #endregion
        }


        [HttpGet]
        [Route("api/TicketingAPI/GetTicketingEventsList")]
        public HttpResponseMessage GetTicketingEventsList()
        {
            string result = string.Empty;
            List<Musika.Models.TicketingEventsNew> lstTicketingEvents = new List<Musika.Models.TicketingEventsNew>();

            Models.TicketingEventsNewModel model = new TicketingEventsNewModel();
            GenericRepository<TicketingUsers> _TicketingUsersRepo = new GenericRepository<TicketingUsers>(_unitOfWork);
            GenericRepository<TicketingEventsNew> _TicketingEventsNewRepo = new GenericRepository<TicketingEventsNew>(_unitOfWork);

            GenericRepository<TicketingEventsNew> _ticketingEntity = new GenericRepository<TicketingEventsNew>(_unitOfWork);
            _unitOfWork.StartTransaction();

            try
            {
                var _list = (from A in _TicketingEventsNewRepo.Repository.GetAll()
                             join B in _TicketingUsersRepo.Repository.GetAll() on A.CreatedBy equals B.UserID
                             select new
                             {
                                 A.EventID,
                                 A.EventTitle,
                                 A.EventLocation,
                                 A.VenueName,
                                 A.Address1,
                                 A.Address2,
                                 A.City,
                                 A.State,
                                 A.ZipCode,
                                 A.StartDate,
                                 A.StartTime,
                                 A.EndDate,
                                 A.EndTime,
                                 A.EventImage,
                                 A.EventDescription,
                                 A.OrganizerName,
                                 A.OrganizerDescription,
                                 A.TicketType,
                                 A.ListingPrivacy,
                                 A.EventType,
                                 A.EventTopic,
                                 A.ShowTicketNumbers,
                                 A.CreatedBy,
                                 B.UserName,
                                 A.CreatedOn,
                                 A.IsApproved
                             }).AsQueryable();

                Models.EventReponse tmpResponse = new EventReponse();

                tmpResponse.ResponseId = 200;
                tmpResponse.ReturnMessage = "Success";

                model.MessageResponse = tmpResponse;
                return Request.CreateResponse(HttpStatusCode.OK, _list);
            }
            catch (Exception ee)
            {
                LogHelper.CreateLog3(ee, Request);
                _unitOfWork.RollBack();

                Models.EventReponse tmpResponse = new EventReponse();
                tmpResponse.ResponseId = 400;
                tmpResponse.ReturnMessage = "Failure";

                model.MessageResponse = tmpResponse;
                return Request.CreateResponse(HttpStatusCode.OK, tmpResponse);
            }
        }


        #region "Get Active Ticketing Events List"
        [HttpGet]
        [Route("api/TicketingAPI/GetActiveTicketingEventsList")]
        public HttpResponseMessage GetActiveTicketingEventsList()
        {
            string result = string.Empty;
            List<Musika.Models.TicketingEventsNew> lstTicketingEvents = new List<Musika.Models.TicketingEventsNew>();

            Models.TicketingEventsNewModel model = new TicketingEventsNewModel();

            GenericRepository<TicketingEventsNew> _ticketingEntity = new GenericRepository<TicketingEventsNew>(_unitOfWork);
            _unitOfWork.StartTransaction();

            try
            {
                lstTicketingEvents = _ticketingEntity.Repository.GetAll().Where(p => p.ISDELETED == true).ToList();

                Models.EventReponse tmpResponse = new EventReponse();

                tmpResponse.ResponseId = 200;
                tmpResponse.ReturnMessage = "Success";

                model.MessageResponse = tmpResponse;
                return Request.CreateResponse(HttpStatusCode.OK, lstTicketingEvents);
            }
            catch (Exception ee)
            {
                LogHelper.CreateLog3(ee, Request);
                _unitOfWork.RollBack();

                Models.EventReponse tmpResponse = new EventReponse();
                tmpResponse.ResponseId = 400;
                tmpResponse.ReturnMessage = "Failure";

                model.MessageResponse = tmpResponse;
                return Request.CreateResponse(HttpStatusCode.OK, tmpResponse);
            }
        }
        #endregion

        #region "Get Inactive  Ticketing Events List"
        [HttpGet]
        [Route("api/TicketingAPI/GetInActiveTicketingEventsList")]
        public HttpResponseMessage GetInActiveTicketingEventsList()
        {
            string result = string.Empty;
            List<Musika.Models.TicketingEventsNew> lstTicketingEvents = new List<Musika.Models.TicketingEventsNew>();

            Models.TicketingEventsNewModel model = new TicketingEventsNewModel();

            GenericRepository<TicketingEventsNew> _ticketingEntity = new GenericRepository<TicketingEventsNew>(_unitOfWork);
            _unitOfWork.StartTransaction();

            try
            {
                lstTicketingEvents = _ticketingEntity.Repository.GetAll().Where(p => p.ISDELETED == false).ToList();

                Models.EventReponse tmpResponse = new EventReponse();

                tmpResponse.ResponseId = 200;
                tmpResponse.ReturnMessage = "Success";

                model.MessageResponse = tmpResponse;
                return Request.CreateResponse(HttpStatusCode.OK, lstTicketingEvents);
            }
            catch (Exception ee)
            {
                LogHelper.CreateLog3(ee, Request);
                _unitOfWork.RollBack();

                Models.EventReponse tmpResponse = new EventReponse();
                tmpResponse.ResponseId = 400;
                tmpResponse.ReturnMessage = "Failure";

                model.MessageResponse = tmpResponse;
                return Request.CreateResponse(HttpStatusCode.OK, tmpResponse);
            }
        }
        #endregion
        #endregion


        #region "Get Ticketing User Events List"
        [AllowAnonymous]
        [Route("TicketingAPI/GetTicketingUserEvents")]
        [HttpGet]
        public HttpResponseMessage GetTicketingUserEvents(string sUserName, string sEmail, string sVenueName, int Pageindex, int Pagesize, string sortColumn, string sortOrder)
        {
            try
            {
                try
                {
                    if (sEmail.Equals("/"))
                    {
                        sEmail = string.Empty;
                    }
                }
                catch (Exception) { }
                GenericRepository<TicketingUsers> _UsersRepo = new GenericRepository<TicketingUsers>(_unitOfWork);
                GenericRepository<TicketingEventsNew> _TourDateRepo = new GenericRepository<TicketingEventsNew>(_unitOfWork);
                GenericRepository<Venue> _VenueRepo = new GenericRepository<Venue>(_unitOfWork);

                GenericRepository<TicketingEventsNew> _UserTourDateRepo = new GenericRepository<TicketingEventsNew>(_unitOfWork);
                GenericRepository<UserGoing> _UserGoingRepo = new GenericRepository<UserGoing>(_unitOfWork);

                var _list = (from A in _UsersRepo.Repository.GetAll()
                             join B in _TourDateRepo.Repository.GetAll() on A.UserID equals B.CreatedBy
                             //join C in _VenueRepo.Repository.GetAll() on B.VenueName equals C.VenueName
                             select new
                             {
                                 A.Email,
                                 A.CreatedDate,
                                 B.VenueName,
                                 B.StartDate,
                                 A.UserName,
                                 A.UserID,
                                 B.EventID,
                                 B.EventTitle,
                                 B.IsApproved
                             }
                             ).OrderByDescending(p => p.EventID).AsQueryable();

                //Filters
                if (!string.IsNullOrEmpty(sUserName))
                {
                    _list = _list.Where(p => p.UserName.IndexOf(sUserName.Trim(), StringComparison.OrdinalIgnoreCase) >= 0);
                }

                if (!string.IsNullOrEmpty(sEmail))
                {
                    _list = _list.Where(p => p.Email.IndexOf(sEmail.Trim(), StringComparison.OrdinalIgnoreCase) >= 0);
                }

                if (!string.IsNullOrEmpty(sVenueName))
                {
                    _list = _list.Where(p => p.VenueName.IndexOf(sVenueName.Trim(), StringComparison.OrdinalIgnoreCase) >= 0);
                }

                //Sorting
                //if (!string.IsNullOrEmpty(sortColumn))
                //{
                //    _list = _list.OrderBy("" + sortColumn + " " + sortOrder);
                //}

                //Pagination
                var _list2 = _list.ToPagedList(Pageindex - 1, Pagesize);

                Dictionary<string, object> d = new Dictionary<string, object>();

                if (_list.Count() > 0)
                {

                    d.Add("Items", _list2);
                    d.Add("PageCount", _list2.PageCount);
                    d.Add("PageNumber", _list2.PageNumber);
                    d.Add("PageSize", Pagesize);
                    return Request.CreateResponse(HttpStatusCode.OK, d);
                }
                else
                {
                    d.Add("Items", new List<string>());
                    d.Add("PageCount", 0);
                    d.Add("PageNumber", 0);
                    d.Add("PageSize", 0);
                    return Request.CreateResponse(HttpStatusCode.OK, d);
                }
            }
            catch (Exception ex)
            {
                //LogHelper.CreateLog(ex);
                return Request.CreateResponse(HttpStatusCode.OK, ex.Message);
            }
        }
        #endregion

        #region "Get Ticketing User Events List Search"
        [AllowAnonymous]
        [Route("TicketingAPI/GetTicketingUserEventsSearch")]
        [HttpGet]
        public HttpResponseMessage GetTicketingUserEventsSearch(string sUserName, string sEmail, string sVenueName, string sEventName, int Pageindex, int Pagesize, string sortColumn, string sortOrder)
        {
            try
            {
                try
                {
                    if (sEmail.Equals("/"))
                    {
                        sEmail = string.Empty;
                    }
                }
                catch (Exception) { }
                GenericRepository<TicketingUsers> _UsersRepo = new GenericRepository<TicketingUsers>(_unitOfWork);
                GenericRepository<TicketingEventsNew> _TourDateRepo = new GenericRepository<TicketingEventsNew>(_unitOfWork);
                GenericRepository<Venue> _VenueRepo = new GenericRepository<Venue>(_unitOfWork);

                GenericRepository<TicketingEventsNew> _UserTourDateRepo = new GenericRepository<TicketingEventsNew>(_unitOfWork);
                GenericRepository<UserGoing> _UserGoingRepo = new GenericRepository<UserGoing>(_unitOfWork);



                DataSet ds = new DataSet();
                ds = new Musika.Repository.SPRepository.SpRepository().GetTicketingEventsList();

                var _list = ds.Tables[0].AsEnumerable().Select(dataRow => new TicketingEventsList
                {
                    EventID = Convert.ToInt32(dataRow.Field<int>("EventID")),
                    EventTitle = Convert.ToString(dataRow.Field<string>("EventTitle")),
                    UserName = Convert.ToString(dataRow.Field<string>("UserName")),
                    StartDate = Convert.ToDateTime(dataRow.Field<DateTime>("StartDate")),
                    StartTime = Convert.ToString(dataRow.Field<string>("StartTime")),
                    EndDate = Convert.ToDateTime(dataRow.Field<DateTime>("EndDate")),
                    VenueName = Convert.ToString(dataRow.Field<string>("VenueName")),
                    VenueCity = Convert.ToString(dataRow.Field<string>("VenueCity")),
                    VenueState = Convert.ToString(dataRow.Field<string>("VenueState")),
                    Postal_Code = Convert.ToString(dataRow.Field<string>("Postal_Code")),
                    IsApproved = Convert.ToInt32(dataRow.Field<bool>("IsApproved")),
                    Email = Convert.ToString(dataRow.Field<string>("Email")),
                    ArtistName = (dataRow.Field<string>("ArtistName") != null) ? dataRow.Field<string>("ArtistName") : ""
                }).OrderByDescending(p => p.EventID).GroupBy(x => x.EventID, (key, group) => group.First()).ToList();



                #region "Commented Code"
                //Filters
                if (!string.IsNullOrEmpty(sUserName))
                {
                    _list = _list.Where(p => p.UserName.IndexOf(sUserName.Trim(), StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                    //_list = _list.Where(p => p.UserName.IndexOf(sUserName.Trim(), StringComparison.OrdinalIgnoreCase) >= 0);
                }

                if (!string.IsNullOrEmpty(sEmail))
                {
                    _list = _list.Where(p => p.Email.IndexOf(sEmail.Trim(), StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                }

                if (!string.IsNullOrEmpty(sVenueName))
                {
                    _list = _list.Where(p => p.VenueName.IndexOf(sVenueName.Trim(), StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                }

                if (!string.IsNullOrEmpty(sEventName))
                {
                    _list = _list.Where(p => p.EventTitle.IndexOf(sEventName.Trim(), StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                }

                //Sorting
                if (!string.IsNullOrEmpty(sortColumn))
                {
                    if (sortColumn == "EventName")
                    {
                        sortColumn = "EventTitle";
                    }
                    //_list = _list.OrderBy("" + sortColumn + " " + sortOrder);
                    _list = _list.OrderBy("" + sortColumn + " " + sortOrder).ToList();
                }
                #endregion

                //Pagination
                var _list2 = _list.GroupBy(x => x.EventID, (key, group) => group.First()).ToList().ToPagedList(Pageindex - 1, Pagesize);

                Dictionary<string, object> d = new Dictionary<string, object>();

                if (_list.Count() > 0)
                {
                    d.Add("Items", _list2);
                    d.Add("PageCount", _list2.PageCount);
                    d.Add("PageNumber", _list2.PageNumber);
                    d.Add("PageSize", Pagesize);
                    return Request.CreateResponse(HttpStatusCode.OK, d);
                }
                else
                {
                    d.Add("Items", new List<string>());
                    d.Add("PageCount", 0);
                    d.Add("PageNumber", 0);
                    d.Add("PageSize", 0);
                    return Request.CreateResponse(HttpStatusCode.OK, d);
                }
            }
            catch (Exception ex)
            {
                //LogHelper.CreateLog(ex);
                return Request.CreateResponse(HttpStatusCode.OK, ex.Message);
            }
        }
        #endregion


        #region "Final Code : Register Ticketing User"
        [HttpPost]
        [Route("api/TicketingAPI/RegisterTicketingUsers")]
        public HttpResponseMessage RegisterTicketingUsers()
        {
            string result = string.Empty;

            Models.TicketingUsers ticketingUserEntity = new Models.TicketingUsers();
            GenericRepository<TicketingUsers> _userEntity = new GenericRepository<TicketingUsers>(_unitOfWork);
            _unitOfWork.StartTransaction();
            try
            {
                InputSignUp Input = new InputSignUp();
                var httpContext = (HttpContextWrapper)Request.Properties["MS_HttpContext"];

                Input.Password = AesCryptography.Encrypt(httpContext.Request.Form["Password"]);
                Input.Email = httpContext.Request.Form["Email"];
                Input.UserName = WebConfigurationManager.AppSettings["XMPPPrefix"].ToString() + httpContext.Request.Form["UserName"];

                Input.DeviceToken = httpContext.Request.Form["DeviceToken"];
                Input.DeviceType = httpContext.Request.Form["DeviceType"];
                Input.DeviceLat = Convert.ToDecimal(httpContext.Request.Form["DeviceLat"]);
                Input.DeviceLong = Convert.ToDecimal(httpContext.Request.Form["DeviceLong"]);
                Input.UserType = httpContext.Request.Form["UserType"];

                var context = new ValidationContext(Input, serviceProvider: null, items: null);
                var results = new List<ValidationResult>();
                var isValid = Validator.TryValidateObject(Input, context, results);

                if (!isValid)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, results[0].ErrorMessage));
                }

                // Check whether the POST operation is MultiPart?
                if (!Request.Content.IsMimeMultipartContent())
                {
                    throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                }

                string _UserName = WebConfigurationManager.AppSettings["XMPPPrefix"].ToString() + Input.UserName;
                Models.TicketingUsers entity = new Models.TicketingUsers();

                ///check User Name 
                if (!Helper.IsValidPattern(_UserName, "^[a-zA-Z0-9. _-]{2,100}$"))
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.Reg_InvalUserName));
                }

                ///check email 
                bool Email = false;
                Email = Helper.IsValidEmail(Input.Email);
                if (Email == false)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.Invalidemail));
                }

                //check if UserName already exists
                TicketingUsers _user = null;

                //check if Email already exists
                _user = _userEntity.Repository.Get(e => e.Email == Input.Email && e.RecordStatus != RecordStatus.Deleted.ToString());
                if (_user != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.EmailAlreadyExists));
                }

                try
                {
                    entity.Password = Input.Password;
                    entity.Email = Input.Email;
                    entity.UserName = Input.UserName;

                    entity.DeviceToken = Input.DeviceToken;
                    entity.DeviceType = Input.DeviceType;
                    entity.DeviceLat = Input.DeviceLat;
                    entity.DeviceLong = Input.DeviceLong;

                    entity.CreatedDate = DateTime.Now;
                    entity.RecordStatus = RecordStatus.InActive.ToString();

                    entity.UserName = Input.UserName;
                    entity.UserType = Input.UserType;

                    entity.ThumbnailURL = WebConfigurationManager.AppSettings["SiteImgURL"] + "/default-user.png";
                    entity.ImageURL = ConfigurationManager.AppSettings["SiteImgURL"] + "/default-user.png";
                    _userEntity.Repository.Add(entity);

                    SetUserDevices(entity);

                    _unitOfWork.Commit();   // End Transaction

                    //return entity;
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, UserDetail(entity.UserID, true), "UserData"));
                }
                catch (Exception ex)
                {
                    LogHelper.CreateLog3(ex, Request);
                    _unitOfWork.RollBack();//RollBack Transaction
                    return null;
                }
            }
            catch (Exception ee)
            {
                LogHelper.CreateLog3(ee, Request);
                _unitOfWork.RollBack();//RollBack Transaction
                return null;
            }
        }
        #endregion


        #region "Final List of all TicketingUsers"
        [HttpGet]
        [Route("api/TicketingAPI/GetAllTicketingUsers")]
        public HttpResponseMessage GetAllTicketingUsers(string userType, string userName, string email, string deviceType, string recordStatus)
        {
            string result = string.Empty;
            List<Musika.Models.TicketingUsers> lstTicketingUsers = new List<Musika.Models.TicketingUsers>();

            TicketingUserModel model = new TicketingUserModel();

            GenericRepository<TicketingUsers> _ticketingUsersEntity = new GenericRepository<TicketingUsers>(_unitOfWork);
            _unitOfWork.StartTransaction();

            try
            {
                lstTicketingUsers = _ticketingUsersEntity.Repository.GetAll();

                //Filters
                if (!string.IsNullOrEmpty(userType))
                {
                    lstTicketingUsers = lstTicketingUsers.Where(p => p.UserType.IndexOf(userType.Trim()) >= 0).ToList();
                }

                if (!string.IsNullOrEmpty(userName))
                {
                    lstTicketingUsers = lstTicketingUsers.Where(p => p.UserType.IndexOf(userName.Trim()) >= 0).ToList();
                }

                if (!string.IsNullOrEmpty(email))
                {
                    lstTicketingUsers = lstTicketingUsers.Where(p => p.UserType.IndexOf(email.Trim()) >= 0).ToList();
                }

                if (!string.IsNullOrEmpty(deviceType))
                {
                    lstTicketingUsers = lstTicketingUsers.Where(p => p.UserType.IndexOf(deviceType.Trim()) >= 0).ToList();
                }

                if (!string.IsNullOrEmpty(recordStatus))
                {
                    lstTicketingUsers = lstTicketingUsers.Where(p => p.UserType.IndexOf(recordStatus.Trim()) >= 0).ToList();
                }
                Models.EventReponse tmpResponse = new EventReponse();

                tmpResponse.ResponseId = 200;
                tmpResponse.ReturnMessage = "Success";

                model.MessageResponse = tmpResponse;
                return Request.CreateResponse(HttpStatusCode.OK, lstTicketingUsers);
            }
            catch (Exception ee)
            {
                LogHelper.CreateLog3(ee, Request);
                _unitOfWork.RollBack();
                return Request.CreateResponse(HttpStatusCode.BadRequest, "");
            }
        }
        #endregion


        #region "Final : Ticketing User List"
        [AllowAnonymous]
        [Route("TicketingAPI/GetTicketingUsers")]
        [HttpGet]
        public HttpResponseMessage GetTicketingUsers(string Name2, string Email2, int Pageindex, int Pagesize, string sortColumn, string sortOrder)
        {
            try
            {
                GenericRepository<TicketingUsers> _UsersRepo = new GenericRepository<TicketingUsers>(_unitOfWork);
                GenericRepository<TicketingEventsNew> _TourDateRepo = new GenericRepository<TicketingEventsNew>(_unitOfWork);
                GenericRepository<UserArtists> _UserArtistsRepo = new GenericRepository<UserArtists>(_unitOfWork);
                GenericRepository<UserTourDate> _UserTourDateRepo = new GenericRepository<UserTourDate>(_unitOfWork);

                List<Models.API.View.ViewUser> _ViewUser = new List<Models.API.View.ViewUser>();

                DataSet ds = new DataSet();
                SpRepository _sp = new SpRepository();
                ds = _sp.SpGetTicketingUserList();

                var _list = General.DTtoList<Models.API.View.ViewTicketingUser>(ds.Tables[0]);

                //Filters
                if (!string.IsNullOrEmpty(Name2))
                {
                    _list = _list.Where(p => p.UserName.IndexOf(Name2.Trim(), StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                }

                if (!string.IsNullOrEmpty(Email2))
                {
                    _list = _list.Where(p => p.Email.IndexOf(Email2.Trim(), StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                }

                //Sorting
                if (!string.IsNullOrEmpty(sortColumn))
                {
                    _list = _list.OrderBy("" + sortColumn + " " + sortOrder).ToList();
                    //_list = _list.OrderBy(sortColumn).ToList();
                }

                //Pagination
                var _list2 = _list.ToPagedList(Pageindex - 1, Pagesize);

                Dictionary<string, object> d = new Dictionary<string, object>();

                if (_list2.Count() > 0)
                {
                    d.Add("Items", _list2);
                    d.Add("PageCount", _list2.PageCount);
                    d.Add("PageNumber", _list2.PageNumber);
                    d.Add("PageSize", Pagesize);

                    return Request.CreateResponse(HttpStatusCode.OK, d);
                }
                else
                {
                    d.Add("Items", new List<Models.API.View.ViewUser>());
                    d.Add("PageCount", 0);
                    d.Add("PageNumber", 0);
                    d.Add("PageSize", 0);
                    return Request.CreateResponse(HttpStatusCode.OK, d);
                }
            }
            catch (Exception ex)
            {
                //LogHelper.CreateLog(ex);
                return Request.CreateResponse(HttpStatusCode.OK, ex.Message);
            }
        }
        #endregion

        #region "Final List of Events ny User ID for Web"
        [HttpGet]
        [Route("api/TicketingAPI/GetAllTicketingEventsByUserID")]
        public HttpResponseMessage GetAllTicketingEventsByUserID(string userID)
        {
            DataSet ds = new DataSet();
            TicketingEventsNewModel model = new TicketingEventsNewModel();
            Models.EventReponse tmpresponse = new EventReponse();

            ds = new SpRepository().GetTicketsEventSummaryList(Convert.ToInt32(userID));
            if (ds.Tables[0].Rows.Count > 0)
            {
                var _list = ds.Tables[0].AsEnumerable().Select(
                           dataRow => new TicketingEventsNewModel
                           {
                               EventID = dataRow.Field<Int32>("EventID"),
                               EventTitle = dataRow.Field<string>("EventTitle"),
                               StartDate = Convert.ToDateTime(dataRow.Field<DateTime>("StartDate")),
                               ArtistName = dataRow.Field<string>("ArtistName"),
                               IsApproved = (dataRow.Field<Int32>("IsApproved") == 1 ? true : false),
                               TicketsSold = dataRow.Field<Int32>("Sold"),
                               TotalTickets = dataRow.Field<Int32>("Total")
                           }).OrderByDescending(p => p.StartDate).ToList();
                tmpresponse.ResponseId = 200;
                tmpresponse.ReturnMessage = "success";

                model.MessageResponse = tmpresponse;
                return Request.CreateResponse(HttpStatusCode.OK, _list);
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.OK, "");
            }
        }
        #endregion

        #region "Final List of Events ny User ID for Web"
        [HttpGet]
        [Route("api/TicketingAPI/SearchEventUserByNameTickNo")]
        public HttpResponseMessage SearchUserEventListNameTickNo(string EventID, string searchBy)
        {
            string result = string.Empty;
            EventReponse tmpResponse = new EventReponse();
            if (string.IsNullOrEmpty(searchBy))
            {
                searchBy = "";
            }
            TicketingEventTickets entityTickets = new TicketingEventTickets();

            TicketsListModelNew _listFinal = new TicketsListModelNew();
            Users user = new Users();
            GenericRepository<Users> _userEntity = new GenericRepository<Users>(_unitOfWork);

            user = _userEntity.Repository.Get(p => p.UserName.StartsWith(searchBy.ToString()));
            string searchByUsername = "";
            if (user != null)
            {

                //if (!searchBy.Any(c => char.IsDigit(c)))
                //{
                searchByUsername = searchBy;
                searchBy = "";
                // }
            }

            List<TicketingUserListEventNew> _lists = new List<TicketingUserListEventNew>();

            // Get Users List for An Event
            DataSet ds2 = new SpRepository().SpSearchAttendes(Convert.ToInt32(EventID.ToString()), searchByUsername);

            TicketingUserListEventNew model;
            if (ds2.Tables[0].Rows.Count > 0)
            {
                for (int i = 0; i < ds2.Tables[0].Rows.Count; i++)
                {
                    DataRow dr;
                    dr = ds2.Tables[0].Rows[i];
                    model = new TicketingUserListEventNew();
                    model.UserID = Convert.ToInt32(dr["UserID"].ToString());
                    model.UserName = Convert.ToString(dr["UserName"].ToString());
                    model.Email = Convert.ToString(dr["Email"].ToString());
                    model.Password = Convert.ToString(dr["Password"].ToString());
                    //model.TicketID = Convert.ToInt32(dr["TicketID"].ToString()).ToString();
                    model.EventID = Convert.ToString(dr["EventID"].ToString());
                    // model.TicketNumber = Convert.ToString(dr["TicketNumber"].ToString());
                    model.ImageUrl = Convert.ToString(dr["ImageUrl"].ToString());
                    model.lstTicket = GetSearchTicketsStatus(Convert.ToInt32(dr["UserID"].ToString()), Convert.ToInt32(dr["EventID"].ToString()), searchBy);
                    // model.Status = Convert.ToString(dr["Status"].ToString());

                    _lists.Add(model);
                }
            }
            else
            {
                _lists = null;
            }

            if (_lists != null)
            {
                if (user != null)
                {
                    _listFinal.lstUsers = _lists;
                }
                else
                {
                    _listFinal.lstUsers = _lists.Where(x => x.lstTicket.Count > 0).ToList();
                }
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, _listFinal));
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, null));
            }
        }
        #endregion

        #region "Final List of Events by User ID for Mobile"
        [HttpGet]
        [Route("api/TicketingAPI/GetTicketingEventsByUserID")]
        public HttpResponseMessage GetTicketingEventsByUserID(string userID)
        {
            string result = string.Empty;
            List<Musika.Models.TicketingEventsNew> lstTicketingEvents = new List<Musika.Models.TicketingEventsNew>();

            Models.TicketingEventsNewModel model = new TicketingEventsNewModel();

            GenericRepository<TicketingEventsNew> _ticketingEntity = new GenericRepository<TicketingEventsNew>(_unitOfWork);
            GenericRepository<TicketingUsers> _UserEntity = new GenericRepository<TicketingUsers>(_unitOfWork);
            GenericRepository<TicketingEventNewStaff> _StaffEvents = new GenericRepository<TicketingEventNewStaff>(_unitOfWork);

            TicketingUsers user = new TicketingUsers();
            _unitOfWork.StartTransaction();

            try
            {
                user = _UserEntity.Repository.GetAll().Where(p => p.UserID == Convert.ToUInt32(userID)).FirstOrDefault();
                if (user != null)
                {
                    if (user.UserType == "Event Organizer")
                    {
                        lstTicketingEvents = _ticketingEntity.Repository.GetAll().Where(p => p.CreatedBy == Convert.ToInt32(userID) && p.ISDELETED != true && p.IsApproved == true).ToList();
                    }
                    else if (user.UserType == "Staff")
                    {
                        int staffId;
                        List<TicketingEventNewStaff> lstEventStaff = new List<TicketingEventNewStaff>();
                        lstEventStaff = _StaffEvents.Repository.GetAll().Where(p => p.StaffId == Convert.ToInt32(userID)).ToList();
                        if (lstEventStaff.Count > 0)
                        {
                            for (int i = 0; i < lstEventStaff.Count; i++)
                            {
                                int evtId = Convert.ToInt32(lstEventStaff[i].EventId);
                                TicketingEventsNew evt = new TicketingEventsNew();
                                evt = _ticketingEntity.Repository.GetAll().Where(p => p.EventID == evtId).FirstOrDefault();

                                lstTicketingEvents.Add(evt);
                            }
                        }
                    }
                }

                List<Models.ViewTicketingEventListNew> lstEvents = new List<Models.ViewTicketingEventListNew>();
                if (lstTicketingEvents.Count > 0)
                {
                    for (int i = 0; i < lstTicketingEvents.Count; i++)
                    {
                        Models.ViewTicketingEventListNew temp = new Models.ViewTicketingEventListNew();
                        temp.EventID = lstTicketingEvents[i].EventID;
                        temp.StartDate = lstTicketingEvents[i].StartDate ?? DateTime.Now;
                        temp.StartTime = lstTicketingEvents[i].StartTime;
                        temp.EndDate = lstTicketingEvents[i].EndDate ?? DateTime.Now;
                        temp.EndTime = lstTicketingEvents[i].EndTime;
                        temp.VenueName = lstTicketingEvents[i].VenueName;
                        if (!String.IsNullOrEmpty(lstTicketingEvents[i].ArtistId.ToString()))
                        {
                            temp.ArtistName = new TicketingAPIController().GetArtistName(lstTicketingEvents[i].ArtistId);
                        }
                        else
                        {
                            temp.ArtistName = string.Empty;
                        }
                        if (!String.IsNullOrEmpty(lstTicketingEvents[i].EventImage))
                        {
                            temp.EventImage = lstTicketingEvents[i].EventImage;
                        }
                        else
                        {
                            temp.EventImage = string.Empty;
                        }
                        lstEvents.Add(temp);
                    }
                }
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, lstEvents.Where(p => p.StartDate >= DateTime.Now).OrderBy(p => p.StartDate)));
            }
            catch (Exception ee)
            {
                LogHelper.CreateLog3(ee, Request);
                _unitOfWork.RollBack();//RollBack Transaction
                return Request.CreateResponse(HttpStatusCode.BadRequest, "");
            }
        }

        public string GetArtistName(int? artistId)
        {
            int id;
            List<Musika.Models.Artists> lstTicketingEvents = new List<Musika.Models.Artists>();
            GenericRepository<Musika.Models.Artists> _artistsEntity = new GenericRepository<Musika.Models.Artists>(_unitOfWork);
            if (String.IsNullOrEmpty(artistId.ToString()) || (artistId == 0))
            {
                id = 0;
                return String.Empty;
            }
            else
            {
                id = Convert.ToInt32(artistId);
                return _artistsEntity.Repository.GetById(id).ArtistName;
            }
        }
        #endregion



        #region "Final Get Event Details by EventID"
        [AllowAnonymous]
        [Route("api/TicketingAPI/GetEventDetails")]
        [HttpGet]
        public HttpResponseMessage GetEventDetails(Int32 EventID)
        {
            DataSet ds = new DataSet();
            Models.EventReponse tmpResponse = new EventReponse();

            try
            {
                ds = new Musika.Repository.SPRepository.SpRepository().SpGetEventDetails(EventID);

                TicketingEventDetailsModel _list = ds.Tables[0].AsEnumerable().Select(
                    dataRow => new TicketingEventDetailsModel
                    {
                        EventID = dataRow.Field<int>("EventID"),
                        EventTitle = dataRow.Field<string>("EventTitle") == null ? "" : dataRow.Field<string>("EventTitle"),
                        EventLocation = dataRow.Field<string>("EventLocation") == null ? "" : dataRow.Field<string>("EventLocation"),
                        VenueName = dataRow.Field<string>("VenueName") == null ? "" : dataRow.Field<string>("VenueName"),
                        Address1 = dataRow.Field<string>("Address1") == null ? "" : dataRow.Field<string>("Address1"),
                        Address2 = dataRow.Field<string>("Address2") == null ? "" : dataRow.Field<string>("Address2"),
                        City = dataRow.Field<string>("City") == null ? "" : dataRow.Field<string>("City"),
                        State = dataRow.Field<string>("State") == null ? "" : dataRow.Field<string>("State"),
                        ZipCode = dataRow.Field<string>("ZipCode") == null ? "" : dataRow.Field<string>("ZipCode"),
                        StartDate = dataRow.Field<DateTime>("StartDate"),
                        StartTime = dataRow.Field<string>("StartTime") == null ? "" : dataRow.Field<string>("StartTime"),
                        EndDate = dataRow.Field<DateTime>("EndDate"),
                        EndTime = dataRow.Field<string>("EndTime") == null ? "" : dataRow.Field<string>("EndTime"),
                        EventImage = dataRow.Field<string>("EventImage") == null ? "" : dataRow.Field<string>("EventImage"),
                        EventDescription = dataRow.Field<string>("EventDescription") == null ? "" : dataRow.Field<string>("EventDescription"),
                        OrganizerName = dataRow.Field<string>("OrganizerName") == null ? "" : dataRow.Field<string>("OrganizerName"),
                        OrganizerDescription = dataRow.Field<string>("OrganizerDescription") == null ? "" : dataRow.Field<string>("OrganizerDescription"),
                        TicketType = dataRow.Field<string>("TicketType") == null ? "" : dataRow.Field<string>("TicketType"),
                        EventType = dataRow.Field<string>("EventType") == null ? "" : dataRow.Field<string>("EventType"),
                        ListingPrivacy = dataRow.Field<string>("ListingPrivacy") == null ? "" : dataRow.Field<string>("ListingPrivacy"),

                        ArtistId = dataRow.Field<Int32>("ArtistId"),
                        ArtistName = dataRow.Field<string>("ArtistName") == null ? "" : dataRow.Field<string>("ArtistName"),
                        VenueLat = dataRow.Field<decimal>("VenueLat"),
                        VenueLong = dataRow.Field<decimal>("VenueLong")
                    }
                 ).FirstOrDefault();



                if (_list != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, _list));
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, null));
                }
            }
            catch (Exception ee)
            {
                string strErrMgs = ee.Message + "\n" + ee.StackTrace;
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, null));
            }

        }
        #endregion

        #region "Final Change Password"
        [HttpPost]
        [Route("api/TicketingAPI/ChangePassword")]
        public HttpResponseMessage ChangePassword([FromBody] TicketingUsers users)
        {
            string result = string.Empty;

            GenericRepository<TicketingUsers> _UsersRepo = new GenericRepository<TicketingUsers>(_unitOfWork);

            TicketingUserModel objUser = new TicketingUserModel();
            objUser.UserName = users.UserName;
            objUser.Password = AesCryptography.Encrypt(users.Password);
            if (objUser.UserName != null)
            {
                string _username = users.UserName;

                var entity = _UsersRepo.Repository.Get(p => p.UserName == _username);
                if (entity != null)
                {
                    entity.UserName = objUser.UserName;
                    entity.Password = objUser.Password;
                    _UsersRepo.Repository.Update(entity);
                    Models.EventReponse tmpResponse = new EventReponse();

                    tmpResponse.ResponseId = 200;
                    tmpResponse.ReturnMessage = "Password Updated Successfully";

                    objUser.MessageResponse = tmpResponse;
                    return Request.CreateResponse(HttpStatusCode.OK, objUser);
                }
                else
                {
                    Models.EventReponse tmpResponse = new EventReponse();

                    tmpResponse.ResponseId = 400;
                    tmpResponse.ReturnMessage = "Failure";

                    objUser.MessageResponse = tmpResponse;
                    return Request.CreateResponse(HttpStatusCode.BadRequest, objUser);
                }
            }
            else
            {
                Models.EventReponse tmpResponse = new EventReponse();

                tmpResponse.ResponseId = 400;
                tmpResponse.ReturnMessage = "Failure";

                objUser.MessageResponse = tmpResponse;
                return Request.CreateResponse(HttpStatusCode.BadRequest, objUser);
            }
        }

        #endregion

        #region "Final Code - Retrieve Password"
        [HttpPost]
        [Route("api/TicketingAPI/RetreivePassword")]
        public HttpResponseMessage RetreivePassword([FromBody] TicketingUsers users)
        {
            TicketingUserModel objUser = new TicketingUserModel();
            objUser.Email = users.Email;
            if (objUser.Email != null)
            {
                GenericRepository<Users> _UsersRepo = new GenericRepository<Users>(_unitOfWork);

                string _email = users.Email;

                var entity = _UsersRepo.Repository.Get(p => p.Email == _email);
                if (entity != null)
                {
                    string password = AesCryptography.Decrypt(entity.Password);

                    string html = "<p>Hi " + entity.UserName + "</p>";
                    html += "<p>Thanks for using " + WebConfigurationManager.AppSettings["AppName"] + "! </p>";
                    html += "<p>Your password is : " + password + "</p>";
                    html += "<p><br>If you would like to change your password, go to the profile area of the app</p>";
                    html += "<p><br><br><strong>Thanks,The " + WebConfigurationManager.AppSettings["AppName"] + " Team</strong></p>";

                    //EmailHelper.SendEmail(entity.Email, WebConfigurationManager.AppSettings["AppName"] + " : Forget Password", html);
                    SendEmailHelper.SendMail(entity.Email, WebConfigurationManager.AppSettings["AppName"] + " : Forget Password", html, "");

                    //return JsonResponse.GetResponse(ResponseCode.Success, "Please check your email to get password.");
                    Models.EventReponse tmpResponse = new EventReponse();

                    tmpResponse.ResponseId = 200;
                    tmpResponse.ReturnMessage = "Success";

                    objUser.Email = users.Email;
                    objUser.Password = password;
                    objUser.MessageResponse = tmpResponse;
                    return Request.CreateResponse(HttpStatusCode.OK, objUser);
                }
                else
                {
                    Models.EventReponse tmpResponse = new EventReponse();

                    tmpResponse.ResponseId = 400;
                    tmpResponse.ReturnMessage = "User not found against this email.";

                    objUser.Email = users.Email;
                    objUser.MessageResponse = tmpResponse;
                    return Request.CreateResponse(HttpStatusCode.OK, objUser);
                }
            }
            else
            {
                Models.EventReponse tmpResponse = new EventReponse();

                tmpResponse.ResponseId = 400;
                tmpResponse.ReturnMessage = "Email cannot be empty.";

                objUser.Email = users.Email;
                objUser.MessageResponse = tmpResponse;
                return Request.CreateResponse(HttpStatusCode.OK, objUser);
            }
        }
        #endregion

        #region "Get User Ticketing User Details"
        private Models.API.View.ViewUser UserDetail(Int64 _UserID, bool _IsNewUser)
        {
            Models.API.View.ViewUser _viewUser = new Models.API.View.ViewUser();
            GenericRepository<TicketingUsers> _UsersRepo = new GenericRepository<TicketingUsers>(_unitOfWork);

            TicketingUsers entity;
            entity = _UsersRepo.Repository.Get(u => u.UserID == _UserID);

            if (entity != null)
            {
                _viewUser.UserID = entity.UserID;
                _viewUser.Email = entity.Email;
                _viewUser.UserName = entity.UserName;

                _viewUser.ImageURL = entity.ImageURL ?? string.Empty;
                _viewUser.ThumbnailURL = entity.ThumbnailURL ?? string.Empty;
                _viewUser.IsNewUser = _IsNewUser;
                _viewUser.RecordStatus = entity.RecordStatus;
            }
            else
            {
                return null;
            }
            return _viewUser;
        }
        #endregion

        #region "Register by Mobile"
        [HttpPost]
        [Route("api/TicketingAPI/RegisterByMobile")]
        public HttpResponseMessage RegisterByMobile()
        {
            string result = string.Empty;

            Models.TicketingUsers ticketingUserEntity = new Models.TicketingUsers();
            GenericRepository<TicketingUsers> _userEntity = new GenericRepository<TicketingUsers>(_unitOfWork);
            _unitOfWork.StartTransaction();
            try
            {

                InputSignUp Input = new InputSignUp();
                var httpContext = (HttpContextWrapper)Request.Properties["MS_HttpContext"];

                Input.Password = AesCryptography.Encrypt(httpContext.Request.Form["Password"]);
                Input.Email = httpContext.Request.Form["Email"];
                Input.DeviceToken = httpContext.Request.Form["DeviceToken"];

                // Check whether the POST operation is MultiPart?
                if (!Request.Content.IsMimeMultipartContent())
                {
                    throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                }

                Models.TicketingUsers entity = new Models.TicketingUsers();

                ///check email 
                bool Email = false;
                Email = Helper.IsValidEmail(Input.Email);
                if (Email == false)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.Invalidemail));
                }


                //check if Email already exists
                TicketingUsers _user = new TicketingUsers();
                _user = _userEntity.Repository.Get(e => e.Email == Input.Email && e.RecordStatus != RecordStatus.Deleted.ToString());
                if (_user != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.EmailAlreadyExists));
                }

                //_unitOfWork.StartTransaction();// Start Transaction

                try
                {
                    entity.Password = Input.Password;
                    entity.Email = Input.Email;
                    entity.UserName = Input.Email;

                    entity.DeviceToken = Input.DeviceToken;

                    entity.CreatedDate = DateTime.Now;
                    entity.RecordStatus = RecordStatus.InActive.ToString();
                    entity.UserType = "User";


                    entity.ThumbnailURL = WebConfigurationManager.AppSettings["SiteImgURL"] + "/default-user.png";
                    entity.ImageURL = ConfigurationManager.AppSettings["SiteImgURL"] + "/default-user.png";
                    _userEntity.Repository.Add(entity);

                    SetUserDevices(entity);

                    _unitOfWork.Commit();// End Transaction

                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, UserDetail(entity.UserID, true), "UserData"));
                }
                catch (Exception ex)
                {
                    LogHelper.CreateLog3(ex, Request);
                    _unitOfWork.RollBack();//RollBack Transaction
                    return null;
                }
            }
            catch (Exception ee)
            {
                LogHelper.CreateLog3(ee, Request);
                _unitOfWork.RollBack();//RollBack Transaction
                return null;
            }
        }
        #endregion

        #region "Register User"
        [HttpPost]
        [Route("api/TicketingAPI/Register")]
        public HttpResponseMessage Register()
        {
            var httpContext = (HttpContextWrapper)Request.Properties["MS_HttpContext"];

            TicketingUsers users = new TicketingUsers();

            users.UserType = httpContext.Request.Form["usertype"];
            users.UserName = httpContext.Request.Form["username"];
            users.Password = AesCryptography.Encrypt(httpContext.Request.Form["password"]);
            users.Email = httpContext.Request.Form["email"];
            users.Addres = httpContext.Request.Form["address"];
            users.Country = httpContext.Request.Form["country"];
            users.State = httpContext.Request.Form["state"];
            users.City = httpContext.Request.Form["city"];
            users.PostalCode = httpContext.Request.Form["postalcode"];
            users.PhoneNumber = httpContext.Request.Form["phonenumber"];
            users.RecordStatus = "InActive";

            Models.TicketingUsers ticketingUserEntity = new Models.TicketingUsers();
            GenericRepository<TicketingUsers> _userEntity = new GenericRepository<TicketingUsers>(_unitOfWork);

            _unitOfWork.StartTransaction();
            try
            {
                ticketingUserEntity.Password = users.Password;
                ticketingUserEntity.UserType = users.UserType;
                ticketingUserEntity.UserName = users.UserName;
                ticketingUserEntity.Email = users.Email;
                ticketingUserEntity.Addres = users.Addres;
                ticketingUserEntity.Country = users.Country;
                ticketingUserEntity.State = users.State;
                ticketingUserEntity.City = users.City;
                ticketingUserEntity.PostalCode = users.PostalCode;
                ticketingUserEntity.PhoneNumber = users.PhoneNumber;
                ticketingUserEntity.RecordStatus = users.RecordStatus;
                ticketingUserEntity.CreatedDate = DateTime.UtcNow;

                _userEntity.Repository.Add(ticketingUserEntity);

                _unitOfWork.Commit();// End Transaction

                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, TicketingUserDetails(ticketingUserEntity.UserID, true), "UserData"));
            }
            catch (Exception ex)
            {
                LogHelper.CreateLog3(ex, Request);
                _unitOfWork.RollBack();//RollBack Transaction
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Exception, ex.Message));
            }
        }
        #endregion

        #region "Ticketing User Details"
        private HttpResponseMessage TicketingUserDetails(Int64 _UserID, bool _IsNewUser)
        {
            TicketingUsers _viewUser = new TicketingUsers();
            GenericRepository<TicketingUsers> _UsersRepo = new GenericRepository<TicketingUsers>(_unitOfWork);

            TicketingUsers entity;
            entity = _UsersRepo.Repository.Get(u => u.UserID == _UserID);

            if (entity != null)
            {
                _viewUser.UserID = entity.UserID;
                _viewUser.UserType = entity.UserType;
                _viewUser.Email = entity.Email;
                _viewUser.UserName = entity.UserName;
                _viewUser.Password = entity.Password;
                _viewUser.Addres = entity.Addres;
                _viewUser.State = entity.State;
                _viewUser.Country = entity.Country;
                _viewUser.PostalCode = entity.PostalCode;
                _viewUser.PhoneNumber = entity.PhoneNumber;

                _viewUser.ImageURL = entity.ImageURL ?? string.Empty;
                _viewUser.ThumbnailURL = entity.ThumbnailURL ?? string.Empty;

                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, UserDetail(_viewUser.UserID, true), "UserData"));
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.UserNotFound));
            }
        }
        #endregion

        #region "Get InActive Users"
        [HttpGet]
        [Route("api/TicketingAPI/GetInActiveUsers")]
        public HttpResponseMessage GetInActiveUsers()
        {
            GenericRepository<TicketingUsers> _UsersRepo = new GenericRepository<TicketingUsers>(_unitOfWork);
            var _list = (from A1 in _UsersRepo.Repository.GetAll(p => p.RecordStatus == "InActive")
                         select new TicketingUsers
                         {
                             UserID = A1.UserID,
                             UserName = A1.UserName,
                             Addres = A1.Addres,
                             City = A1.City,
                             State = A1.State,
                             Country = A1.Country,
                             PostalCode = A1.PostalCode,
                             Email = A1.Email,
                             UserType = A1.UserType
                         }).ToList();

            _list = _list.OrderBy(p => p.UserID).ToList();
            return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, _list, "UsersList"));
        }
        #endregion

        #region "SignUp"
        [HttpPost]
        [Route("api/TicketBooking/Signup")]
        public HttpResponseMessage Signup()
        {
            try
            {
                InputSignUp Input = new InputSignUp();
                var httpContext = (HttpContextWrapper)Request.Properties["MS_HttpContext"];

                Input.UserName = httpContext.Request.QueryString[0].ToString();
                Input.Password = AesCryptography.Encrypt(httpContext.Request.QueryString[2].ToString());
                Input.Email = httpContext.Request.QueryString[1].ToString();

                var context = new ValidationContext(Input, serviceProvider: null, items: null);
                var results = new List<ValidationResult>();
                var isValid = Validator.TryValidateObject(Input, context, results);

                if (!isValid)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, results[0].ErrorMessage));
                }

                string _UserName = WebConfigurationManager.AppSettings["XMPPPrefix"].ToString() + Input.UserName;
                Models.TicketingUsers entity = new Models.TicketingUsers();
                GenericRepository<TicketingUsers> _userEntity = new GenericRepository<TicketingUsers>(_unitOfWork);

                ///check USer Name 
                if (!Helper.IsValidPattern(_UserName, "^[a-zA-Z0-9. _-]{2,100}$"))
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.Reg_InvalUserName));
                }

                //check if UserName already exists
                TicketingUsers _user = null;

                //check if Email already exists
                _user = _userEntity.Repository.Get(e => e.Email == Input.Email && e.RecordStatus != RecordStatus.Deleted.ToString());
                if (_user != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.EmailAlreadyExists));
                }

                _unitOfWork.StartTransaction();     // Start Transaction

                try
                {
                    entity.Password = Input.Password;
                    entity.Email = Input.Email;
                    entity.UserName = Input.UserName;

                    entity.CreatedDate = DateTime.Now;
                    entity.RecordStatus = RecordStatus.Active.ToString();
                    entity.RecordStatus = "Inactive";

                    _userEntity.Repository.Add(entity);

                    SetUserDevices(entity);

                    GenericRepository<UserSettings> _UserSettingsEntity = new GenericRepository<UserSettings>(_unitOfWork);
                    UserSettings _UserSettings = new UserSettings();

                    _UserSettings.UserID = entity.UserID;
                    _UserSettings.SettingKey = EUserSettings.Musika.ToString();
                    _UserSettings.SettingValue = true;
                    _UserSettings.NotificationCount = 0;

                    _UserSettingsEntity.Repository.Add(_UserSettings);

                    _unitOfWork.Commit();// End Transaction

                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, TicketingUserDetails(entity.UserID, true), "UserData"));
                }
                catch (Exception ex)
                {
                    LogHelper.CreateLog3(ex, Request);
                    _unitOfWork.RollBack();//RollBack Transaction
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Exception, ex.Message));
                }
            }
            catch (Exception ex)
            {
                LogHelper.CreateLog3(ex, Request);
                if (_unitOfWork.IsTransaction == true) _unitOfWork.RollBack();//RollBack Transaction
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Exception, ex.Message));
            }
        }
        #endregion

        #region "Set User Device"
        private void SetUserDevices(TicketingUsers user)
        {
            GenericRepository<UserDevices> _UserDevices = new GenericRepository<UserDevices>(_unitOfWork);
            if (!String.IsNullOrEmpty(user.DeviceType) && !String.IsNullOrEmpty(user.DeviceToken))
            {
                var existingTokens = _UserDevices.Repository.GetAll(x => x.DeviceToken == user.DeviceToken && x.DeviceType == user.DeviceType);

                if (existingTokens != null && existingTokens.Count() > 0)
                    existingTokens.ForEach(x => _UserDevices.Repository.DeletePermanent(x.UserDeviceId));


                _UserDevices.Repository.Add(new UserDevices()
                {
                    UserId = user.UserID,
                    DeviceToken = user.DeviceToken,
                    DeviceType = user.DeviceType,
                    CreatedDate = DateTime.Now
                });
            }
        }
        #endregion

        #region "Update Profile Picture"
        [HttpPost]
        [Route("api/TicketingAPI/UpdateUserPicture")]
        public HttpResponseMessage UpdateUserPicture()
        {
            try
            {
                var httpContext = (HttpContextWrapper)Request.Properties["MS_HttpContext"];

                if (httpContext.Request.Form["UserID"] == "")
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, "provide UserID"));
                }

                // Check whether the POST operation is MultiPart?
                if (!Request.Content.IsMimeMultipartContent())
                {
                    throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                }

                int _UserID = Convert.ToInt16(httpContext.Request.Form["UserID"]);

                Models.TicketingUsers entity = null;
                GenericRepository<TicketingUsers> _UsersRepo = new GenericRepository<TicketingUsers>(_unitOfWork);

                entity = _UsersRepo.Repository.Get(p => p.UserID == _UserID);

                if (entity != null)
                {
                    if (HttpContext.Current.Request.Files.Count > 0)
                    {
                        ImageResponse imgResponse = MultipartFiles.GetMultipartImage(HttpContext.Current.Request.Files, "Image", "profileimage", _Imagethumbsize, _Imagethumbsize, _imageSize, _imageSize, true, true, true, "profileimage");

                        if (imgResponse.IsSuccess == true)
                        {
                            entity.ThumbnailURL = imgResponse.ThumbnailURL;
                            entity.ImageURL = imgResponse.ImageURL;
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, imgResponse.ResponseMessage));
                        }
                        _UsersRepo.Repository.Update(entity);
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, "Image file is not provided", "UserData"));
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, UserDetail(entity.UserID, false), "UserData"));
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.UserNotFound));
                }
            }
            catch (Exception ex)
            {
                LogHelper.CreateLog3(ex, Request);
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, ex.Message));
            }
        }
        #endregion

        #region "Final Code Update User Profile"
        [AllowAnonymous]
        [HttpPost]
        [Route("api/TicketingAPI/UpdateUserProfile")]
        public HttpResponseMessage UpdateUserProfile()
        {
            Models.API.View.ViewUser tmpUsers = new Models.API.View.ViewUser();
            Models.EventReponse tmpResponse = new EventReponse();
            Models.TicketingUserModel model = new TicketingUserModel();

            Models.TicketingUsers _Users = null;
            GenericRepository<TicketingUsers> _UsersRepo = new GenericRepository<TicketingUsers>(_unitOfWork);

            var httpContext = (HttpContextWrapper)Request.Properties["MS_HttpContext"];

            TicketingUsers users = new TicketingUsers();

            users.UserID = Convert.ToInt32(httpContext.Request.Form["userid"]);
            users.Password = Convert.ToString(httpContext.Request.Form["password"]);
            users.ImageURL = Convert.ToString(httpContext.Request.Form["imageurl"]);

            string newPwd = Convert.ToString(httpContext.Request.Form["newpassword"]);
            newPwd = AesCryptography.Encrypt(newPwd.ToString());
            try
            {
                int userid = Numerics.GetInt(users.UserID);
                users.Password = AesCryptography.Encrypt(users.Password.ToString());
                _Users = _UsersRepo.Repository.Get(p => p.UserID == userid && p.Password == users.Password);
                if (_Users == null)
                {
                    tmpResponse.ResponseId = 400;
                    tmpResponse.ReturnMessage = "Old Password Do Not Match";

                    model.MessageResponse = tmpResponse;
                    return Request.CreateResponse(HttpStatusCode.OK, model);
                }
                else
                {
                    tmpUsers = UserDetail(userid, true);
                    if (tmpUsers != null)
                    {
                        _Users.UserID = Convert.ToInt32(tmpUsers.UserID);
                        _Users.UserName = tmpUsers.UserName;
                        _Users.Email = tmpUsers.Email;
                        _Users.Password = users.Password;
                        _Users.ThumbnailURL = tmpUsers.ThumbnailURL;
                        _Users.ImageURL = users.ImageURL;
                    }

                    if (_Users.Password.Equals(newPwd))
                    {
                        tmpResponse.ResponseId = 400;
                        tmpResponse.ReturnMessage = "Must Enter Different Password From Old Password";

                        model.MessageResponse = tmpResponse;
                        return Request.CreateResponse(HttpStatusCode.OK, model);
                    }
                    else
                    {
                        if (_Users != null)
                        {
                            _Users.UserName = (_Users.UserName != null) ? _Users.UserName.ToString() != "" ? _Users.UserName : _Users.UserName : _Users.UserName;
                            _Users.Password = (newPwd != null) ? newPwd.ToString() != "" ? newPwd.ToString() : newPwd : newPwd;
                            _Users.ImageURL = (_Users.ImageURL != null) ? _Users.ImageURL.ToString() != "" ? _Users.ImageURL : _Users.ImageURL : _Users.ImageURL;

                            _UsersRepo.Repository.Update(_Users);

                            _Users = _UsersRepo.Repository.GetById(_Users.UserID);

                            if (_Users != null)
                            {
                                model.UserID = _Users.UserID;
                                model.UserType = _Users.UserType;
                                model.UserName = _Users.UserName;
                                model.Email = _Users.Email;
                                model.Password = _Users.Password;
                                model.Addres = _Users.Addres;
                                model.City = _Users.City;
                                model.State = _Users.State;
                                model.Country = _Users.Country;
                                model.PostalCode = _Users.PostalCode;
                                model.PhoneNumber = _Users.PhoneNumber;
                                model.FacebookID = _Users.FacebookID;
                                model.ThumbnailURL = _Users.ThumbnailURL;
                                model.ImageURL = _Users.ImageURL;
                                model.DeviceType = _Users.DeviceType;
                                model.DeviceToken = _Users.DeviceToken;
                                model.DeviceLat = _Users.DeviceLat;
                                model.DeviceLong = _Users.DeviceLong;
                                model.RecordStatus = _Users.RecordStatus;
                                model.ModifiedDate = System.DateTime.Now;
                                model.CreatedDate = _Users.CreatedDate;
                                model.SynFacebookID = _Users.SynFacebookID;
                                model.UserLanguage = _Users.UserLanguage;
                            }

                            tmpResponse.ResponseId = 200;
                            tmpResponse.ReturnMessage = "User Details Updated Successfully";

                            model.MessageResponse = tmpResponse;
                            return Request.CreateResponse(HttpStatusCode.OK, model);
                        }
                        else
                        {
                            tmpResponse.ResponseId = 400;
                            tmpResponse.ReturnMessage = "User Details Not Updated";

                            model.MessageResponse = tmpResponse;
                            return Request.CreateResponse(HttpStatusCode.NotAcceptable, model);
                        }
                    }
                }
            }
            catch (Exception)
            {
                tmpResponse.ResponseId = 400;
                tmpResponse.ReturnMessage = "Unexpected Error";

                model.MessageResponse = tmpResponse;
                return Request.CreateResponse(HttpStatusCode.NotAcceptable, model);
            }
        }
        #endregion

        #region "Update User Status By User ID"
        [HttpPost]
        [Route("TicketingAPI/UpdateUserStatus")]
        public AdminResponse UpdateUserStatus(Int64 _UserId, string status)
        {
            AdminResponse _AdminResponse = new AdminResponse();
            try
            {
                Models.TicketingUsers _Users = null;
                GenericRepository<TicketingUsers> _UsersRepo = new GenericRepository<TicketingUsers>(_unitOfWork);

                int userid = Numerics.GetInt(_UserId);
                _Users = _UsersRepo.Repository.Get(p => p.UserID == userid);

                if (_Users != null)
                {
                    _Users.RecordStatus = status;
                    _UsersRepo.Repository.Update(_Users);

                    _AdminResponse.Status = true;
                    _AdminResponse.RetMessage = "User Status updated successfully.";
                    return _AdminResponse;
                }
                else
                {
                    _AdminResponse.Status = false;
                    _AdminResponse.RetMessage = "Invalid user";
                    return _AdminResponse;
                }
            }
            catch (Exception ex)
            {
                _AdminResponse.Status = false;
                _AdminResponse.RetMessage = ex.Message;
                return _AdminResponse;
            }
        }

        #endregion

        #region "Final Code Get User Profile By User ID"

        [HttpGet]
        [Route("api/TicketingAPI/GetUserProfileById")]
        public HttpResponseMessage GetUserProfileById(Int64 _UserID)
        {
            Models.TicketingUsers _Users = null;
            string result = string.Empty;
            Models.TicketingUserModel model = new TicketingUserModel();

            Models.TicketingUsers ticketingUserEntity = new Models.TicketingUsers();
            GenericRepository<TicketingUsers> _userEntity = new GenericRepository<TicketingUsers>(_unitOfWork);

            InputSignUp Input = new InputSignUp();
            var httpContext = (HttpContextWrapper)Request.Properties["MS_HttpContext"];

            try
            {
                Input.UserID = Convert.ToInt32(httpContext.Request.QueryString[0].ToString());
            }
            catch (Exception)
            {
                Input.UserID = -1;
            }

            TicketingAppUsers users = new TicketingAppUsers();
            users.UserID = Input.UserID;

            GenericRepository<TicketingUsers> _UsersRepo = new GenericRepository<TicketingUsers>(_unitOfWork);

            if (!String.IsNullOrEmpty(Input.UserID.ToString()))
            {
                _Users = _UsersRepo.Repository.Get(p => p.UserID == users.UserID);
            }
            else
            {
                Models.EventReponse tmpResponse = new EventReponse();
                tmpResponse.ResponseId = 400;
                tmpResponse.ReturnMessage = "Invalid User ID or User Not Exists";

                model.MessageResponse = tmpResponse;
                return Request.CreateResponse(HttpStatusCode.NonAuthoritativeInformation, model);
            }
            if (_Users == null)
            {
                //result = "No An Authenticated User";
                Models.EventReponse tmpResponse = new EventReponse();

                tmpResponse.ResponseId = 400;
                tmpResponse.ReturnMessage = "Invalid User ID or User Not Exists";

                model.MessageResponse = tmpResponse;
                return Request.CreateResponse(HttpStatusCode.NonAuthoritativeInformation, model);
            }
            else
            {
                //result = "Authenticated User";
                TicketingUsers tmpUser = new TicketingUsers();
                if (!String.IsNullOrEmpty(users.UserID.ToString()))
                    tmpUser = _userEntity.Repository.Get(e => e.UserID == users.UserID);
                else
                    tmpUser = null;

                model.UserID = tmpUser.UserID;
                model.UserType = tmpUser.UserType;
                model.UserName = tmpUser.UserName;
                model.Email = tmpUser.Email;
                model.Password = tmpUser.Password;
                model.Addres = tmpUser.Addres;
                model.City = tmpUser.City;
                model.State = tmpUser.State;
                model.PostalCode = tmpUser.PostalCode;
                model.PhoneNumber = tmpUser.PhoneNumber;
                model.FacebookID = tmpUser.FacebookID;
                model.ThumbnailURL = tmpUser.ThumbnailURL;
                model.ImageURL = tmpUser.ImageURL;
                model.DeviceType = tmpUser.DeviceType;
                model.DeviceToken = tmpUser.DeviceToken;
                model.DeviceLat = tmpUser.DeviceLat;
                model.DeviceLong = tmpUser.DeviceLong;
                model.RecordStatus = tmpUser.RecordStatus;
                model.ModifiedDate = tmpUser.ModifiedDate;
                model.CreatedDate = tmpUser.CreatedDate;
                model.SynFacebookID = tmpUser.SynFacebookID;
                model.UserLanguage = tmpUser.UserLanguage;
                model.Country = tmpUser.Country;

                Models.EventReponse tmpResponse = new EventReponse();

                tmpResponse.ResponseId = 200;
                tmpResponse.ReturnMessage = "Fetched User Details";

                model.MessageResponse = tmpResponse;
                return Request.CreateResponse(HttpStatusCode.OK, model);
            }
        }
        #endregion

        #region "Final Get User Profile"
        [HttpGet]
        [Route("api/TicketingAPI/GetUserProfile")]
        public UsersModel GetUserProfile(Int64 _UserID)
        {
            UsersModel _viewUser = new UsersModel();
            DataSet ds = new DataSet();
            ds = new SpRepository().GetUserProfileByUserId(_UserID);
            if (ds.Tables[0].Rows.Count > 0)
            {
                DataRow dr = ds.Tables[0].Rows[0];
                _viewUser.UserID = Convert.ToInt32(dr["UserID"].ToString());
                _viewUser.UserName = Convert.ToString(dr["UserName"].ToString());
                _viewUser.Password = Convert.ToString(dr["Password"].ToString()) ?? string.Empty;
                _viewUser.ImageURL = HttpUtility.UrlPathEncode(Convert.ToString(dr["ImageURL"].ToString()));
                _viewUser.ThumbnailURL = Convert.ToString(dr["ThumbnailURL"].ToString()) ?? string.Empty;
                _viewUser.Email = Convert.ToString(dr["Email"].ToString()) ?? string.Empty;
                return _viewUser;
            }
            else
            {
                return null;
            }
        }
        #endregion

        #region "Get User Detail for Web"
        [HttpGet]
        [Route("api/TicketingAPI/GetUserProfileForWeb")]
        public StaffMember GetUserProfileForWeb(Int64 _UserID)
        {
            StaffMember _viewUser = new StaffMember();
            GenericRepository<TicketingUsers> _UsersRepo = new GenericRepository<TicketingUsers>(_unitOfWork);

            TicketingUsers entity;
            entity = _UsersRepo.Repository.Get(u => u.UserID == _UserID);

            if (entity != null)
            {
                _viewUser.UserID = entity.UserID;
                _viewUser.Email = entity.Email;
                _viewUser.UserName = entity.UserName;
                _viewUser.Password = AesCryptography.Decrypt(entity.Password);
                _viewUser.Addres = entity.Addres;
                _viewUser.State = entity.State;
                _viewUser.City = entity.City;
                _viewUser.Country = entity.Country;
                _viewUser.PostalCode = entity.PostalCode;
                _viewUser.PhoneNumber = entity.PhoneNumber;
                _viewUser.UserType = entity.UserType;
                _viewUser.CreatedBy = entity.CreatedBy;
            }
            else
            {
                return null;
            }
            return _viewUser;
        }
        #endregion

        #region "Final : Get All Ticketing Events List"
        [HttpGet]
        [Route("api/TicketingAPI/GetAllTicketingEvents")]
        public HttpResponseMessage GetAllTicketingEvents()
        {
            GenericRepository<TicketingEventsNew> _UsersRepo = new GenericRepository<TicketingEventsNew>(_unitOfWork);
            var _list = (from A1 in _UsersRepo.Repository.GetAll(p => p.EventID > 0)
                         select new TicketingEventsNew
                         {
                             EventID = A1.EventID,
                             EventTitle = A1.EventTitle,
                             EventLocation = A1.EventLocation,
                             VenueName = A1.VenueName,
                             Address1 = A1.Address1,
                             Address2 = A1.Address2,
                             City = A1.City,
                             State = A1.State,
                             ZipCode = A1.ZipCode,
                             StartDate = Convert.ToDateTime(A1.StartDate),
                             StartTime = A1.StartTime,
                             EndDate = Convert.ToDateTime(A1.EndDate),
                             EndTime = A1.EndTime,
                             EventImage = A1.EventImage,
                             EventDescription = A1.EventDescription,
                             OrganizerName = A1.OrganizerName,
                             OrganizerDescription = A1.OrganizerDescription,
                             TicketType = A1.TicketType,
                             ListingPrivacy = A1.ListingPrivacy,
                             EventType = A1.EventType,
                             EventTopic = A1.EventTopic,
                             ShowTicketNumbers = A1.ShowTicketNumbers
                         }).ToList();

            _list = _list.OrderBy(p => p.EventID).ToList();
            if (_list.Count > 0)
            {
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, _list));
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, null));
            }
        }
        #endregion

        #region "Get Event By EventID"
        [HttpGet]
        [Route("api/TicketingAPI/GetAllTicketingEventsNewById")]
        public List<TicketingEventsNew> GetAllTicketingEventsNewById(string eventId)
        {
            int id = Convert.ToInt32(eventId);
            GenericRepository<TicketingEventsNew> _UsersRepo = new GenericRepository<TicketingEventsNew>(_unitOfWork);
            var _list = (from A1 in _UsersRepo.Repository.GetAll(p => p.EventID == id)
                         select new TicketingEventsNew
                         {
                             EventID = A1.EventID,
                             EventTitle = A1.EventTitle,
                             EventLocation = A1.EventLocation,
                             VenueName = A1.VenueName,
                             Address1 = A1.Address1,
                             Address2 = A1.Address2,
                             City = A1.City,
                             State = A1.State,
                             ZipCode = A1.ZipCode,
                             StartDate = Convert.ToDateTime(A1.StartDate),
                             StartTime = A1.StartTime,
                             EndDate = Convert.ToDateTime(A1.EndDate),
                             EndTime = A1.EndTime,
                             //EventImage = A1.EventImage,
                             EventImage = ImagesToBase64(A1.EventImage),
                             EventDescription = A1.EventDescription,
                             OrganizerName = A1.OrganizerName,
                             OrganizerDescription = A1.OrganizerDescription,
                             TicketType = A1.TicketType,
                             ListingPrivacy = A1.ListingPrivacy,
                             EventType = A1.EventType,
                             EventTopic = A1.EventTopic,
                             ShowTicketNumbers = A1.ShowTicketNumbers,
                             ArtistId = A1.ArtistId
                         }).ToList();

            _list = _list.OrderBy(p => p.EventID).ToList();
            return _list;
        }

        public string ImagesToBase64(string imageName)
        {
            string base64String = null;
            if (!String.IsNullOrEmpty(imageName))
            {
                String path = HttpContext.Current.Server.MapPath("~/Content/EventImages"); //Path
                string imgPath = Path.Combine(path, imageName);
                using (System.Drawing.Image image = System.Drawing.Image.FromFile(imgPath))
                {
                    using (MemoryStream m = new MemoryStream())
                    {
                        image.Save(m, image.RawFormat);
                        byte[] imageBytes = m.ToArray();
                        base64String = Convert.ToBase64String(imageBytes);
                        return base64String + "," + imageName;
                    }
                }
            }
            return "";
        }
        #endregion

        #region "Get Events by User ID -- Updated on 25-Oct-2018"
        // Musika.Models.TicketingEventsNewModel --> Model is updated  (TotalTickets, TicketsSold  --> new Field added)
        [HttpGet]
        [Route("api/TicketingAPI/GetAllTicketingByUserID")]
        public HttpResponseMessage GetAllTicketingByUserID(int userId)
        {
            GenericRepository<Artists> _ArtistRepo = new GenericRepository<Artists>(_unitOfWork);
            GenericRepository<TicketingUsers> _TicketingUsers = new GenericRepository<TicketingUsers>(_unitOfWork);
            GenericRepository<TicketingEventsNew> _UsersRepo = new GenericRepository<TicketingEventsNew>(_unitOfWork);

            var _list = (from A1 in _UsersRepo.Repository.GetAll(p => p.CreatedBy == userId)
                         join B in _ArtistRepo.Repository.GetAll() on A1.ArtistId equals B.ArtistID
                         join C in _TicketingUsers.Repository.GetAll() on A1.CreatedBy equals C.UserID
                         select new Musika.Models.TicketingEventsNewModel
                         {
                             EventID = A1.EventID,
                             EventTitle = A1.EventTitle,
                             EventLocation = A1.EventLocation,
                             VenueName = A1.VenueName,
                             Address1 = A1.Address1,
                             Address2 = A1.Address2,
                             City = A1.City,
                             State = A1.State,
                             ZipCode = A1.ZipCode,
                             StartDate = Convert.ToDateTime(A1.StartDate),
                             StartTime = A1.StartTime,
                             EndDate = Convert.ToDateTime(A1.EndDate),
                             EndTime = A1.EndTime,
                             EventImage = GetFullPath(A1.EventImage),
                             EventDescription = A1.EventDescription,
                             OrganizerName = A1.OrganizerName,
                             OrganizerDescription = A1.OrganizerDescription,
                             TicketType = A1.TicketType,
                             ListingPrivacy = A1.ListingPrivacy,
                             EventType = A1.EventType,
                             EventTopic = A1.EventTopic,
                             ShowTicketNumbers = A1.ShowTicketNumbers,
                             CreatedBy = A1.CreatedBy,
                             //CreatedByUser = C.UserName,
                             CreatedOn = A1.CreatedOn,
                             ArtistId = B.ArtistID,
                             ArtistName = B.ArtistName,
                             TotalTickets = 0,
                             TicketsSold = 0
                         }).OrderBy(p => p.EventID).ToList();

            List<TicketingEventsNewModel> lstFinal = new List<TicketingEventsNewModel>();
            if (_list.Count > 0)
            {
                for (int i = 0; i < _list.Count; i++)
                {
                    TicketingEventsNewModel model = new TicketingEventsNewModel();
                    model.EventID = _list[i].EventID;
                    model.EventTitle = _list[i].EventTitle;
                    model.EventLocation = _list[i].EventLocation;
                    model.VenueName = _list[i].VenueName;
                    model.Address1 = _list[i].Address1;
                    model.Address2 = _list[i].Address2;
                    model.City = _list[i].City;
                    model.State = _list[i].State;
                    model.ZipCode = _list[i].ZipCode;
                    model.StartDate = Convert.ToDateTime(_list[i].StartDate);
                    model.StartTime = _list[i].StartTime;
                    model.EndDate = Convert.ToDateTime(_list[i].EndDate);
                    model.EndTime = _list[i].EndTime;
                    model.EventImage = GetFullPath(_list[i].EventImage);
                    model.EventDescription = _list[i].EventDescription;
                    model.OrganizerName = _list[i].OrganizerName;
                    model.OrganizerDescription = _list[i].OrganizerDescription;
                    model.TicketType = _list[i].TicketType;
                    model.ListingPrivacy = _list[i].ListingPrivacy;
                    model.EventType = _list[i].EventType;
                    model.EventTopic = _list[i].EventTopic;
                    model.ShowTicketNumbers = _list[i].ShowTicketNumbers;
                    model.CreatedBy = _list[i].CreatedBy;
                    //CreatedByUser = C.UserName,
                    model.CreatedOn = _list[i].CreatedOn;
                    model.ArtistId = _list[i].ArtistId;
                    model.ArtistName = _list[i].ArtistName;

                    string temp = GetTotalTickets(model.EventID);
                    string[] words = temp.Split(',');
                    model.TotalTickets = Convert.ToInt32(words[0]);
                    model.TicketsSold = Convert.ToInt32(words[1]);

                    lstFinal.Add(model);
                }
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, lstFinal.GroupBy(x => x.EventID, (key, group) => group.First()).OrderBy(p => Convert.ToDateTime(p.StartDate)).ToList()));
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, null));
            }
        }

        public string GetTotalTickets(int eventID)
        {
            string res;
            DataSet ds = new DataSet();
            ds = new Musika.Repository.SPRepository.SpRepository().SpGetTicketingEventTicketSummary(eventID);
            res = ds.Tables[0].Rows[0][1].ToString() + "," + ds.Tables[0].Rows[0][0].ToString();
            return res;
        }

        private string GetFullPath(string imgName)
        {
            return @"http://appserver.musikaapp.com/Content/EventImages/" + imgName;
        }
        #endregion

        #region "Final : Get All Ticketing Events List"
        [HttpGet]
        [Route("api/TicketingAPI/GetAllTicketingEventsNew")]
        public List<TicketingEventsNew> GetAllTicketingEventsNew()
        {
            GenericRepository<TicketingEventsNew> _UsersRepo = new GenericRepository<TicketingEventsNew>(_unitOfWork);
            var _list = (from A1 in _UsersRepo.Repository.GetAll(p => p.EventID > 0)
                         select new TicketingEventsNew
                         {
                             EventID = A1.EventID,
                             EventTitle = A1.EventTitle,
                             EventLocation = A1.EventLocation,
                             VenueName = A1.VenueName,
                             Address1 = A1.Address1,
                             Address2 = A1.Address2,
                             City = A1.City,
                             State = A1.State,
                             ZipCode = A1.ZipCode,
                             StartDate = Convert.ToDateTime(A1.StartDate),
                             StartTime = A1.StartTime,
                             EndDate = Convert.ToDateTime(A1.EndDate),
                             EndTime = A1.EndTime,
                             EventImage = A1.EventImage,
                             EventDescription = A1.EventDescription,
                             OrganizerName = A1.OrganizerName,
                             OrganizerDescription = A1.OrganizerDescription,
                             TicketType = A1.TicketType,
                             ListingPrivacy = A1.ListingPrivacy,
                             EventType = A1.EventType,
                             EventTopic = A1.EventTopic,
                             ShowTicketNumbers = A1.ShowTicketNumbers
                         }).ToList();

            _list = _list.OrderBy(p => p.EventID).ToList();
            return _list;
        }
        #endregion

        #region "Validate Ticket Purchased by User"        
        [Route("api/TicketingAPI/ValidatePurchasedTicketByUser")]
        [HttpPost]
        public HttpResponseMessage ValidatePurchasedTicketByUser([FromBody] TixEventTixModel events)
        {
            string result = string.Empty;
            string finalResult = string.Empty;

            Models.EventReponse tmpResponse = new EventReponse();

            Models.EntityEventTicketUserDetails entityTickets = new Models.EntityEventTicketUserDetails();
            Models.ImportTicket entityBarcodeTickets = new Models.ImportTicket();

            GenericRepository<TicketingEventTicketConfirmation> _ticketingEventTicketsEntity = new GenericRepository<TicketingEventTicketConfirmation>(_unitOfWork);
            GenericRepository<TicketingUsers> _ticketingUsers = new GenericRepository<TicketingUsers>(_unitOfWork);
            GenericRepository<TourDate> _eventRepo = new GenericRepository<TourDate>(_unitOfWork);
            GenericRepository<TicketingEventsNew> _eventNewRepo = new GenericRepository<TicketingEventsNew>(_unitOfWork);

            GenericRepository<Users> _eventUserDetailsRepo = new GenericRepository<Users>(_unitOfWork);

            TicketingEventTickets tickets = new TicketingEventTickets();
            Users user = new Users();

            Models.TicketingUserModel muser = new TicketingUserModel();

            try
            {
                DataSet dataBarCode = new DataSet();
                dataBarCode = new SpRepository().GetmatchedBarCode(events.TicketNumber);
                if (dataBarCode.Tables[0].Rows.Count > 0)
                {

                    string _countsBarCode = new SpRepository().GetValidateBulkTicketByUser(events.TicketNumber, events.UserId);
                    tmpResponse.ResponseId = 200;
                    if (!String.IsNullOrEmpty(_countsBarCode))
                    {
                        // User Details
                        GenericRepository<ImportBulkTicket> _TicketingUserRepo = new GenericRepository<ImportBulkTicket>(_unitOfWork);

                        //List<ImportBulkTicket> lstUserTicketing = new List<ImportBulkTicket>();
                        ////int uid = Convert.ToInt32(_list[0].UserID);
                        //lstUserTicketing = _TicketingUserRepo.Repository.GetAll(p => p.BarCode == events.TicketNumber).ToList();
                        dataBarCode = new SpRepository().GetmatchedBarCode(events.TicketNumber);
                        DataRow dr = dataBarCode.Tables[0].Rows[0];

                        //entityBarcodeTickets.Name = dr["Name"].ToString();
                        //entityBarcodeTickets.order = dr["order"].ToString();
                        //entityBarcodeTickets.Price = dr["Price"].ToString();
                        //entityBarcodeTickets.Row = dr["Row"].ToString();
                        //entityBarcodeTickets.Seat = dr["Seat"].ToString();
                        //entityBarcodeTickets.Section = dr["Section"].ToString();

                        //entityBarcodeTickets.TicketStatus = Convert.ToBoolean(dr["TicketStatus"]);
                        //entityBarcodeTickets.BarCode = dr["BarCode"].ToString();
                        //entityBarcodeTickets.MessageResponse = tmpResponse;

                        entityTickets.UserName = dr["Name"].ToString();
                        entityTickets.Password = "";
                        entityTickets.FacebookID = "";
                        entityTickets.ThumbnailURL = "";
                        entityTickets.ImageURL = "";
                        entityTickets.DeviceType = "";

                        entityTickets.DeviceToken = "";
                        entityTickets.DeviceLat = 0;
                        entityTickets.DeviceLong = 0;
                        entityTickets.RecordStatus = "";
                        entityTickets.MessageResponse = tmpResponse;
                        if (_countsBarCode == "Success")
                        {
                            tmpResponse.ReturnMessage = _countsBarCode;
                            tmpResponse.ResponseId = 200;
                        }
                        else
                        {
                            tmpResponse.ReturnMessage = _countsBarCode;
                            tmpResponse.ResponseId = 400;
                        }
                        //tmpResponse.ReturnMessage = _counts;
                        //entityTickets.MessageResponse = tmpResponse;

                        //user = _eventUserDetailsRepo.Repository.GetAll().Where(p => p.UserID == _list[0].UserID).FirstOrDefault();
                        return Request.CreateResponse(HttpStatusCode.OK, entityTickets);
                    }
                    else
                    {

                    }
                }
                else
                {
                    DataSet data = new DataSet();

                    string ticketnumber = "";
                    string temp = "";
                    if (events.TicketNumber.ToString().Length > 10)
                    {
                        // data = new SpRepository().GetMatchedTicketNumber(events.TicketNumber);
                        temp = events.TicketNumber.ToString().ToUpper();
                        entityTickets.TicketNumber = Guid.Parse(events.TicketNumber);
                    }
                    else
                    {
                        data = new SpRepository().GetMatchedTicketSerailNumber(Convert.ToString(events.TicketNumber.ToString().ToUpper()));
                        if (data.Tables[0].Rows.Count > 0)
                        {
                            DataRow dr = data.Tables[0].Rows[0];
                            ticketnumber = dr["TicketNumber"].ToString();
                        }
                        entityTickets.TicketNumber = Guid.Parse(ticketnumber);
                    }

                    var _list = _ticketingEventTicketsEntity.Repository.GetAll().Where(p => p.TicketNumber == entityTickets.TicketNumber).ToList();
                    // }

                    int? tourDateId = _list[0].TourDateID;

                    int? ticketEventId;
                    ticketEventId = _eventRepo.Repository.GetById((int)tourDateId).TicketingEventID;

                    // Check for the Ticket / Event association
                    //int _counts = _eventNewRepo.Repository.GetAll().Where(p => p.EventID == ticketEventId && (p.CreatedBy != events.UserId || p.StaffId != events.UserId)).Count();

                    string _counts = new SpRepository().GetValidateTicketByUser(entityTickets.TicketNumber, events.UserId);
                    tmpResponse.ResponseId = 200;
                    if (!String.IsNullOrEmpty(_counts))
                    {


                        // User Details
                        GenericRepository<Users> _TicketingUserRepo = new GenericRepository<Users>(_unitOfWork);

                        List<Users> lstUserTicketing = new List<Users>();
                        int uid = Convert.ToInt32(_list[0].UserID);
                        lstUserTicketing = _TicketingUserRepo.Repository.GetAll(p => p.UserID == uid).ToList();

                        entityTickets.UserName = lstUserTicketing[0].UserName;
                        entityTickets.Email = lstUserTicketing[0].Email;
                        entityTickets.Password = lstUserTicketing[0].Password;
                        entityTickets.FacebookID = lstUserTicketing[0].FacebookID;
                        entityTickets.ThumbnailURL = lstUserTicketing[0].ThumbnailURL;
                        entityTickets.ImageURL = lstUserTicketing[0].ImageURL;
                        entityTickets.DeviceType = lstUserTicketing[0].DeviceType;
                        entityTickets.DeviceToken = lstUserTicketing[0].DeviceToken;
                        entityTickets.DeviceLat = lstUserTicketing[0].DeviceLat;
                        entityTickets.DeviceLong = lstUserTicketing[0].DeviceLong;
                        entityTickets.RecordStatus = lstUserTicketing[0].RecordStatus;
                        entityTickets.status = lstUserTicketing[0].RecordStatus;

                        //tmpResponse.ResponseId = 200;
                        //tmpResponse.ReturnMessage = "Success";

                        entityTickets.MessageResponse = tmpResponse;
                        if (_counts == "Success")
                        {
                            tmpResponse.ReturnMessage = _counts;
                            tmpResponse.ResponseId = 200;
                        }
                        else
                        {
                            tmpResponse.ReturnMessage = _counts;
                            tmpResponse.ResponseId = 400;
                        }
                        //tmpResponse.ReturnMessage = _counts;
                        //entityTickets.MessageResponse = tmpResponse;

                        //user = _eventUserDetailsRepo.Repository.GetAll().Where(p => p.UserID == _list[0].UserID).FirstOrDefault();
                        return Request.CreateResponse(HttpStatusCode.OK, entityTickets);
                    }
                    else
                    {

                    }
                }

                return Request.CreateResponse(HttpStatusCode.OK, entityTickets);
            }
            catch (Exception ex)
            {
                tmpResponse.ResponseId = 400;
                tmpResponse.ReturnMessage = "Failure";
                entityTickets.MessageResponse = tmpResponse;

                return Request.CreateResponse(HttpStatusCode.OK, entityTickets);
            }
        }
        #endregion

        #region "Validate Ticket Purchased by User"
        [HttpPost]
        [Route("api/TicketingAPI/ValidatePurchasedTicketByUserForWeb")]
        public HttpResponseMessage ValidatePurchasedTicketByUserForWeb([FromBody] TicketingEventTicketsModel events)
        {
            string result = string.Empty;
            Models.EventReponse tmpResponse = new EventReponse();

            Models.EntityEventTicketUserDetails entityTickets = new Models.EntityEventTicketUserDetails();

            GenericRepository<TicketingEventTickets> _ticketingEventTicketsEntity = new GenericRepository<TicketingEventTickets>(_unitOfWork);
            GenericRepository<TicketingUsers> _ticketingUsers = new GenericRepository<TicketingUsers>(_unitOfWork);

            TicketingEventTickets tickets = new TicketingEventTickets();

            try
            {
                entityTickets.TicketNumber = events.TicketNumber;

                // Get Ticketing User Details
                //var _list = _ticketingEventTicketsEntity.Repository.GetAll().Where(p => p.TicketNumber == events.TicketNumber).ToList();

                TicketingEventResultModel tempModel = new TicketingEventResultModel();
                //if (_list.Count == 0)
                {
                    //tempModel.EventID = _list[0].EventID;
                    //tempModel.TicketId = _list[0].TicketId;
                    //tempModel.TicketNumber = _list[0].TicketNumber;
                    //tempModel.UserId = _list[0].UserId;

                    tempModel.EventID = events.EventID;
                    tempModel.TicketId = events.TicketId;
                    tempModel.TicketNumber = events.TicketNumber;
                    tempModel.UserId = events.UserId;
                    tempModel.TicketType = events.TicketType;

                    // Update User Status in TicketingEventTickets

                    GenericRepository<TicketingUsers> _UsersRepo = new GenericRepository<TicketingUsers>(_unitOfWork);

                    //tickets.UserId = _list[0].UserId;
                    tickets.UserId = events.UserId;
                    tickets.Status = "Present";
                    tickets.CheckInDateTime = DateTime.Now;

                    new Musika.Repository.SPRepository.SpRepository().SpUpdateTicketingStatus(Convert.ToInt32(events.EventID.ToString()), events.TicketNumber.ToString(), events.TicketType, Convert.ToInt32(events.UserId.ToString()));



                    // Get details of the User Who Have purchased the Ticket
                    GenericRepository<TicketingUsers> _TicketingUserRepo = new GenericRepository<TicketingUsers>(_unitOfWork);

                    List<TicketingUsers> lstUserTicketing = new List<TicketingUsers>();
                    lstUserTicketing = _TicketingUserRepo.Repository.GetAll(p => p.UserID == tempModel.UserId).ToList();

                    entityTickets.UserType = lstUserTicketing[0].UserType;
                    entityTickets.UserName = lstUserTicketing[0].UserName;
                    entityTickets.Email = lstUserTicketing[0].Email;
                    entityTickets.Password = lstUserTicketing[0].Password;
                    entityTickets.Addres = lstUserTicketing[0].Addres;
                    entityTickets.City = lstUserTicketing[0].City;
                    entityTickets.State = lstUserTicketing[0].State;
                    entityTickets.PostalCode = lstUserTicketing[0].PostalCode;
                    entityTickets.PhoneNumber = lstUserTicketing[0].PhoneNumber;
                    entityTickets.FacebookID = lstUserTicketing[0].FacebookID;
                    entityTickets.ThumbnailURL = lstUserTicketing[0].ThumbnailURL;
                    entityTickets.ImageURL = lstUserTicketing[0].ImageURL;
                    entityTickets.DeviceType = lstUserTicketing[0].DeviceType;
                    entityTickets.DeviceToken = lstUserTicketing[0].DeviceToken;
                    entityTickets.DeviceLat = lstUserTicketing[0].DeviceLat;
                    entityTickets.DeviceLong = lstUserTicketing[0].DeviceLong;
                    entityTickets.RecordStatus = lstUserTicketing[0].RecordStatus;
                    entityTickets.Country = lstUserTicketing[0].Country;
                    entityTickets.status = lstUserTicketing[0].RecordStatus;


                    tmpResponse.ResponseId = 200;
                    tmpResponse.ReturnMessage = "Success";

                    entityTickets.MessageResponse = tmpResponse;
                }

                return Request.CreateResponse(HttpStatusCode.OK, entityTickets);
            }
            catch (Exception ee)
            {
                LogHelper.CreateLog3(ee, Request);
                _unitOfWork.RollBack();

                tmpResponse.ResponseId = 400;
                tmpResponse.ReturnMessage = "Unexpected Error";

                return Request.CreateResponse(HttpStatusCode.OK, entityTickets);
            }
        }
        #endregion

        #region "Get People Going"
        [HttpGet]
        [Route("api/TicketingAPI/GetPeopleGoing")]
        /* Test Url :: 23.111.138.246/api/v2/TicketBooking/GetPeopleGoing?TourID=12566&UserID=7749&Pageindex=1&Pagesize=10 */
        public HttpResponseMessage GetPeopleGoing(Int32 TourID, int UserID, int Pageindex, int Pagesize)
        {
            try
            {
                ViewPeopleGoing _ViewPeopleGoing = new ViewPeopleGoing();

                GenericRepository<TicketingEvents> _TourDateRepo = new GenericRepository<TicketingEvents>(_unitOfWork);
                GenericRepository<Venue> _VenueRepo = new GenericRepository<Venue>(_unitOfWork);
                GenericRepository<Artists> _ArtistsRepo = new GenericRepository<Artists>(_unitOfWork);
                GenericRepository<UserGoing> _UserGoingRepo = new GenericRepository<UserGoing>(_unitOfWork);
                GenericRepository<TicketingUsers> _UsersRepo = new GenericRepository<TicketingUsers>(_unitOfWork);

                GenericRepository<UserFriends> _UserFriendsRepo = new GenericRepository<UserFriends>(_unitOfWork);

                Models.Artists _Artist = null;
                Models.TicketingEvents _TourDate = null;

                _TourDate = _TourDateRepo.Repository.Get(p => p.TourDateID == TourID);

                if (_TourDate == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.EventNotDound, "PeopleGiong"));
                }

                _Artist = _ArtistsRepo.Repository.Get(p => p.ArtistID == _TourDate.ArtistID);

                _ViewPeopleGoing.TourID = _TourDate.TourDateID;
                _ViewPeopleGoing.ArtistID = _Artist.ArtistID;
                _ViewPeopleGoing.ArtistName = _Artist.ArtistName;
                _ViewPeopleGoing.ImageURL = _Artist.ImageURL ?? "";


                Models.Venue _Venue = null;
                _Venue = _VenueRepo.Repository.Get(p => p.VenueID == _TourDate.VenueID);

                _ViewPeopleGoing.VenueID = _Venue.VenueID;
                _ViewPeopleGoing.VenueName = _Venue.VenueName ?? "";
                _ViewPeopleGoing.VenuImageURL = _Venue.ImageURL ?? "";

                //who else is going
                _ViewPeopleGoing.Going = (from A1 in _UserFriendsRepo.Repository.GetAll(p => p.UserID == UserID && p.Matched_UserID != null)
                                          join A in _UserGoingRepo.Repository.GetAll(p => p.TourDateID == _TourDate.TourDateID && p.RecordStatus == EUserGoing.Going.ToString()) on A1.Matched_UserID equals A.UserID
                                          join B in _UsersRepo.Repository.GetAll() on A.UserID equals B.UserID
                                          select new ViewPeoples
                                          {
                                              ThumbnailURL = B.ThumbnailURL ?? "",
                                              UserID = A.UserID.Value,
                                              UserName = B.UserName.ToString(),
                                              Email = B.Email ?? "",
                                              CreatedDate = Convert.ToDateTime(A.CreatedDate).ToString("d")
                                          }).OrderByDescending(p => p.CreatedDate).ToPagedList(Pageindex - 1, Pagesize).ToList();

                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, _ViewPeopleGoing, "PeopleGiong"));
            }
            catch (Exception ex)
            {
                LogHelper.CreateLog3(ex, Request);
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, ex.Message, "PeopleGiong"));
            }
        }
        #endregion

        [HttpGet]
        [Route("api/TicketingAPI/ExportCSV")]
        public HttpResponseMessage ExportCSV(string eventId)
        {

            Models.TourDate _TourDate = null;

            //_unitOfWork.StartTransaction();
            GenericRepository<TourDate> _TourDateRepo = new GenericRepository<TourDate>(_unitOfWork);


            Int64 _EventID = Convert.ToInt64(eventId);
            _TourDate = _TourDateRepo.Repository.Get(p => p.TicketingEventID == _EventID);

            ExportCsv _ExportCsv = new ExportCsv();
            ViewEventDetail _ViewEventDetail = new ViewEventDetail();


            GenericRepository<Venue> _VenueRepo = new GenericRepository<Venue>(_unitOfWork);
            GenericRepository<Artists> _ArtistsRepo = new GenericRepository<Artists>(_unitOfWork);
            GenericRepository<UserTourDate> UserTourDateRepo = new GenericRepository<UserTourDate>(_unitOfWork);
            GenericRepository<UserFriends> _UserFriendsRepo = new GenericRepository<UserFriends>(_unitOfWork);
            GenericRepository<UserGoing> _UserGoingRepo = new GenericRepository<UserGoing>(_unitOfWork);
            GenericRepository<Users> _UsersRepo = new GenericRepository<Users>(_unitOfWork);
            GenericRepository<UserTourDate> _UserTourDateRepo = new GenericRepository<UserTourDate>(_unitOfWork);
            GenericRepository<TicketingUsers> _TicketingUserRepo = new GenericRepository<TicketingUsers>(_unitOfWork);

            GenericRepository<TicketingEventTickets> _TicketingRepo = new GenericRepository<TicketingEventTickets>(_unitOfWork);
            GenericRepository<TicketingEventTicketsSummary> _TicketingEventSummary = new GenericRepository<TicketingEventTicketsSummary>(_unitOfWork);
            GenericRepository<TicketingEventsNew> _TicketingEventNewRepo = new GenericRepository<TicketingEventsNew>(_unitOfWork);

            Models.UserTourDate _UserTourDate = null;
            Models.Artists _Artist = null;

            _Artist = _ArtistsRepo.Repository.Get(p => p.ArtistID == _TourDate.ArtistID);

            _ViewEventDetail.TourID = _TourDate.TourDateID;
            _ViewEventDetail.ArtistID = _Artist.ArtistID;
            _ExportCsv.ArtistName = _Artist.ArtistName;


            _ExportCsv.ImageURL = _Artist.ImageURL ?? "";
            _ExportCsv.BannerImage_URL = _Artist.ImageURL ?? "";

            _ViewEventDetail.OnTour = _Artist.OnTour;
            _ExportCsv.Gender = _Artist.Gender ?? "";
            _ExportCsv.Main_Genre = _Artist.Main_Genre ?? "";
            _ExportCsv.Decade = _Artist.Decade ?? "";
            _ExportCsv.Alternate_Names = _Artist.Alternate_Names ?? "";
            _ExportCsv.CreatedDate = _Artist.CreatedDate;
            _ExportCsv.ModifiedDate = _Artist.ModifiedDate;
            _ViewEventDetail.RecordStatus = _Artist.RecordStatus;
            _ViewEventDetail.Instagram_Tag = _Artist.Instagram_Tag != null && _Artist.Instagram_Tag != "" ? _Artist.Instagram_Tag : _Artist.ArtistName;
            try
            {
                _ExportCsv.Date_Local = _TourDate.Datetime_Local.ToString().Substring(0, 10).Trim();
                _ExportCsv.Time_Local = _TourDate.Datetime_Local.ToString().Substring(10).Trim();
            }
            catch (Exception) { }


            if (!String.IsNullOrEmpty(_TourDate.TicketURL))
            {
                _ExportCsv.TicketURL = _TourDate.TicketURL;
            }
            else
            {
                _ExportCsv.TicketURL = "http://appserver.musikaapp.com/TicketEventCheckout.aspx";
            }


            _ExportCsv.Event_Name = _TourDate.EventName;


            Models.UserGoing _UserGoing = null;

            Models.Venue _Venue = null;
            _Venue = _VenueRepo.Repository.Get(p => p.VenueID == _TourDate.VenueID);

            //who else is going
            var _lst = (from A1 in _UserFriendsRepo.Repository.GetAll(p => p.Matched_UserID != null)
                        join A in _UserGoingRepo.Repository.GetAll(p => p.TourDateID == _TourDate.TourDateID && p.RecordStatus == EUserGoing.Going.ToString()) on A1.Matched_UserID equals A.UserID
                        join B in _UsersRepo.Repository.GetAll() on A.UserID equals B.UserID
                        select new ExportCsv.ViewEventUsers
                        {
                            //ThumbnailURL = B.ThumbnailURL ?? "",
                            UserID = A.UserID.Value,
                            GoingUserName = B.UserName.ToString(),
                            Email = B.Email ?? "",
                            CreatedDate = Convert.ToDateTime(A.CreatedDate).ToString("d")
                        }).DistinctBy(y => y.UserID).OrderByDescending(p => p.CreatedDate).ToList();

            //who else is going
            _ExportCsv.UsersGoing = _lst.ToList();
            // _ViewEventDetail.UsersGoing = lstViewEventUsers;

            _ExportCsv.NoOfUserGoing = _lst.Count();


            // Added by Mukesh
            int? eventID;
            List<TourDate> lstSummary = new List<TourDate>();
            eventID = _TourDateRepo.Repository.GetAll().Where(t => t.TourDateID == _TourDate.TourDateID).FirstOrDefault().TicketingEventID;
            if (!String.IsNullOrEmpty(eventID.ToString()))
            {
                lstSummary = _TourDateRepo.Repository.GetAll().Where(p => p.TicketingEventID == eventID).ToList();
            }

            List<ExportCsv.Ticket> lstTicket = new List<ExportCsv.Ticket>();
            if (lstSummary.Count > 0)
            {
                for (int i = 0; i < lstSummary.Count; i++)
                {
                    ExportCsv.Ticket tickets = new ExportCsv.Ticket();
                    //tickets.EventId = lstSummary[i].TicketingEventID ?? default(int);

                    TicketingEventTicketsSummary summary = new TicketingEventTicketsSummary();
                    summary = _TicketingEventSummary.Repository.GetAll().Where(t => t.EventID == eventID).ToList().FirstOrDefault();

                    if (summary != null)
                    {
                        tickets.CountryId = summary.CountryId;
                        tickets.Currency = summary.Currency;
                        tickets.RefundPolicy = summary.RefundPolicy;
                        //tickets.EventId = summary.EventID;
                    }
                    else
                    {
                        tickets.CountryId = 1;
                        tickets.Currency = "$";
                        tickets.RefundPolicy = "Yes";
                        //tickets.EventId = tickets.EventId = eventID ?? default(int);
                    }
                    List<TicketingEventTicketsSummary> lstTicketData = new List<TicketingEventTicketsSummary>();
                    List<ExportCsv.TicketData> lstData = new List<ExportCsv.TicketData>();
                    int eventid = eventID ?? default(int);
                    DataSet ds1 = new DataSet();
                    ds1 = new SpRepository().GetTicketBalanceSummary(eventid);
                    if (ds1.Tables[0].Rows.Count > 0)
                    {
                        for (int j = 0; j < ds1.Tables[0].Rows.Count; j++)
                        {
                            ExportCsv.TicketData data = new ExportCsv.TicketData();
                            DataRow dr = ds1.Tables[0].Rows[j];

                            data.EventId = Convert.ToInt32(dr["EventId"].ToString());
                            data.TicketType = Convert.ToString(dr["TicketType"].ToString());
                            data.TicketCategory = Convert.ToString(dr["TicketCategory"].ToString());
                            data.Price = Convert.ToDecimal(dr["Price"].ToString());
                            data.Quantity = Convert.ToInt32(dr["Total"].ToString());
                            data.BalanceTickets = Convert.ToInt32(dr["Balance"].ToString());
                            data.PackageStartDate = Convert.ToString(dr["PackageStartDate"].ToString());
                            data.PackageEndDate = Convert.ToString(dr["PackageEndDate"].ToString());
                            lstData.Add(data);
                        }
                    }
                    tickets.lstTicketData = lstData;
                    lstTicket.Add(tickets);
                }
            }

            TicketingEventsNew ticketingNew = _TicketingEventNewRepo.Repository.GetAll().Where(p => p.EventID == eventID).FirstOrDefault();
            try
            {
                _ExportCsv.Date_Local = Convert.ToDateTime(ticketingNew.StartDate).ToString("d");
                _ExportCsv.Time_Local = Convert.ToDateTime(ticketingNew.StartTime).ToString("t");
            }
            catch (Exception)
            { }
            if (ticketingNew != null)
            {
                _ExportCsv.Event_Description = Regex.Replace(ticketingNew.EventDescription, "<.*?>", String.Empty).Replace("\n", "").Replace("\t", "");
                _ExportCsv.Event_Name = Regex.Replace(_TourDate.EventName, "<.*?>", String.Empty).Replace("\n", "").Replace("\t", "");
                ticketingNew.OrganizerName = _TicketingUserRepo.Repository.GetById((int)ticketingNew.CreatedBy).UserName;
                _ExportCsv.Organizer_Name = ticketingNew.OrganizerName;
                _ExportCsv.Organizer_Description = Regex.Replace(ticketingNew.OrganizerDescription, "<.*?>", String.Empty).Replace("\n", "").Replace("\t", "");
            }
            else
            {
                _ExportCsv.Organizer_Name = "Musika";
                _ExportCsv.Organizer_Description = "Musika Event Organizer";

            }
            string csv = string.Empty;
            _ExportCsv.lstTicket = lstTicket;

            return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, _ExportCsv, "CSV"));
        }
        public static string ToCsv<T>(string separator, IEnumerable<T> objectlist)
        {
            Type t = typeof(T);
            PropertyInfo[] fields = t.GetProperties();

            string header = String.Join(separator, fields.Select(f => f.Name).ToArray());

            StringBuilder csvdata = new StringBuilder();
            csvdata.AppendLine(header);

            foreach (var o in objectlist)
                csvdata.AppendLine(ToCsvFields(separator, fields, o));

            return csvdata.ToString();
        }
        public static string ToCsvFields(string separator, PropertyInfo[] fields, object o)
        {
            StringBuilder linie = new StringBuilder();

            foreach (var f in fields)
            {
                if (linie.Length > 0)
                    linie.Append(separator);

                var x = f.GetValue(o);

                if (x != null)
                    linie.Append(x.ToString());
            }

            return linie.ToString();
        }
        #region "Dashboard"
        [HttpGet]
        [Route("api/TicketingAPI/DashboardData")]
        public string DashboardData()
        {
            // Display the Number of Attendees
            Dashboard dashboard = new Dashboard();
            try
            {
                GenericRepository<TicketingUsers> _UsersRepo = new GenericRepository<TicketingUsers>(_unitOfWork);
                GenericRepository<Artists> _ArtistsRepo = new GenericRepository<Artists>(_unitOfWork);
                GenericRepository<TicketingEvents> _TourDateRepo = new GenericRepository<TicketingEvents>(_unitOfWork);
                GenericRepository<Venue> _VenueRepo = new GenericRepository<Venue>(_unitOfWork);
                GenericRepository<UserGoing> _UserGoingRepo = new GenericRepository<UserGoing>(_unitOfWork);
                GenericRepository<HotTour> _HotTourRepo = new GenericRepository<HotTour>(_unitOfWork);

                List<ViewArtistlst> _ViewArtistlst = new List<ViewArtistlst>();

                dashboard.guests = _UsersRepo.Repository.GetAll().Where(x => x.UserType == "User").Where(y => y.RecordStatus == "InActive").Count();

                return "Working";
            }
            catch (Exception ee)
            {
                return ee.Message;
            }
        }
        #endregion

        class Dashboard
        {
            public int guests { get; set; }         // Invitados
            public int Confirmed { get; set; }      // Confirmados
            public int LookingForward { get; set; } // En ESpera
            public int Cancelled { get; set; }      // Cancelados
        }


        #region "Image Conversion from base-64 to png"        
        public string ImageToBase64(string imageName)
        {
            string base64String = null;
            try
            {
                if (!String.IsNullOrEmpty(imageName))
                {
                    String path = HttpContext.Current.Server.MapPath("~/Content/EventImages"); //Path
                    string imgPath = Path.Combine(path, imageName);
                    using (System.Drawing.Image image = System.Drawing.Image.FromFile(imgPath))
                    {
                        using (MemoryStream m = new MemoryStream())
                        {
                            image.Save(m, image.RawFormat);
                            byte[] imageBytes = m.ToArray();
                            base64String = Convert.ToBase64String(imageBytes);
                            return base64String + "," + imageName;
                        }
                    }
                }
                return "";
            }
            catch (Exception)
            {
                return "";
            }
        }
        #endregion

        #region "Convert Image from png to base-64"
        public System.Drawing.Image Base64ToImage(string base64String)
        {
            byte[] imageBytes = Convert.FromBase64String(base64String);
            MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length);
            ms.Write(imageBytes, 0, imageBytes.Length);
            System.Drawing.Image image = System.Drawing.Image.FromStream(ms, true);
            return image;
        }
        #endregion

        #region "Final Code : ZXing Code"
        [HttpGet]
        [Route("api/TicketingAPI/GetZXingQRCode")]
        public string GetZXingQRCode(string content)
        {
            string alt = "QR Code";
            int height = 300;
            int width = 300;
            int margin = 0;
            HttpResponseMessage result = new HttpResponseMessage();
            string qrResult = GenerateZXingQRCode(content);

            string imageName = string.Empty;
            imageName = SaveQRCodeImage(qrResult, "QRCode_" + DateTime.Now.ToShortDateString());

            //return qrResult;
            return imageName;
        }

        public TicketEventDetails GenerateTicketNumber(string content, string dataTicket, string personalData, string img,string orderNo)
        {
            string ticketNumber = string.Empty;

            string tourDateId = string.Empty;
            int userid = 0;
            string deviceId = string.Empty;
            string ticketType = string.Empty;
            string mode = string.Empty;

            int qty;
            GenericRepository<Users> _UsersRepo = new GenericRepository<Users>(_unitOfWork);
            GenericRepository<TicketingEventsNew> _EventsRepo = new GenericRepository<TicketingEventsNew>(_unitOfWork);
            GenericRepository<TourDate> _TourDate = new GenericRepository<TourDate>(_unitOfWork);
            Users entity;
            //TicketingEventsNew eventEntity;

            string[] temp = content.Split('~');
            tourDateId = temp[0];
            userid = Convert.ToInt32(temp[1]);
            deviceId = temp[2];
            ticketType = temp[3];
            mode = temp[4];
            qty = Convert.ToInt32(temp[5]);

            //personalData += txtName.Text + "~" + txtAddress.Text + "~" + txtCountryState.Text + "~" + txtCity.Text + "~" + txtPostalCode.Text + "~" + txtPhone.Text; 

            string[] perData = personalData.Split('~');

            int uid = Convert.ToInt32(userid);

            entity = _UsersRepo.Repository.Get(u => u.UserID == uid);

            DataSet ds = new DataSet();
            ds = new SpRepository().SpGetEventDetailsByTourDateId(Convert.ToInt32(tourDateId));
            TicketEventDetails ticketEventDetails = new TicketEventDetails();
            TicketingEventTicketConfirmation ticketConfirm = new TicketingEventTicketConfirmation();


            if (ds.Tables[0].Rows.Count > 0)
            {
                DataRow dr = ds.Tables[0].Rows[0];
                ticketEventDetails.ArtistID = Convert.ToInt32(dr["ArtistID"].ToString());
                ticketEventDetails.TourDateID = Convert.ToInt32(dr["TourDateID"].ToString());
                ticketEventDetails.EventTitle = Convert.ToString(dr["EventTitle"].ToString());
                ticketEventDetails.VenueName = Convert.ToString(dr["VenueName"].ToString());
                ticketEventDetails.City = Convert.ToString(dr["City"].ToString());
                ticketEventDetails.State = Convert.ToString(dr["State"].ToString());
                ticketEventDetails.VenueLat = Convert.ToString(dr["VenueLat"].ToString());
                ticketEventDetails.VenueLong = Convert.ToString(dr["VenueLong"].ToString());
                ticketEventDetails.ArtistName = Convert.ToString(dr["ArtistName"].ToString());
                ticketEventDetails.Tour_Utcdate = Convert.ToString(dr["Tour_Utcdate"].ToString());
                if (!String.IsNullOrEmpty(dr["EventID"].ToString()))
                {
                    ticketEventDetails.EventID = Convert.ToInt32(dr["EventID"].ToString());
                    ticketConfirm.EventID = ticketEventDetails.EventID;
                }

                if (!String.IsNullOrEmpty(userid.ToString()))
                {
                    ticketConfirm.UserID = Convert.ToInt32(userid);
                }

                Users ticketUser = _UsersRepo.Repository.GetById(Convert.ToInt32(userid));

                if (ticketUser != null)
                {
                    ticketEventDetails.UserType = "User";
                    ticketEventDetails.UserName = ticketUser.UserName;
                    ticketEventDetails.Email = ticketUser.Email;
                    ticketEventDetails.DeviceType = ticketUser.DeviceType;
                    ticketEventDetails.TicketNumber = Guid.Parse(dataTicket);

                    ticketEventDetails.Address = perData[1];
                    ticketEventDetails.City = perData[3];
                    ticketEventDetails.State = perData[6];
                    ticketEventDetails.Country = perData[2];
                    ticketEventDetails.PostalCode = perData[4];
                    ticketEventDetails.Email = ticketUser.Email;
                    ticketEventDetails.PhoneNumber = perData[5];
                    ticketEventDetails.TicketType = ticketType;
                    ticketEventDetails.Mode = mode;
                    if (String.IsNullOrEmpty(ticketConfirm.EventID.ToString()))
                    {
                        //ticketEventDetails.TicketSerialNumber = "EVT-0000-" + RandomNumber(0, 9999);
                        // ticketEventDetails.TicketSerialNumber = "EVT-0000-" + GetSerialNumber(Convert.ToInt32(tourDateId));
                        ticketEventDetails.TicketSerialNumber = "";
                    }
                    else
                    {
                        //ticketEventDetails.TicketSerialNumber = "EVT-" + ticketConfirm.EventID + "-" + RandomNumber(0, 9999);
                        //ticketEventDetails.TicketSerialNumber = "EVT-" + ticketConfirm.EventID + "-" + GetSerialNumber(Convert.ToInt32(tourDateId));
                        ticketEventDetails.TicketSerialNumber = "";
                    }

                    //TicketingEventTicketConfirmation ticketGenerate = new TicketingEventTicketConfirmation();
                    //// add value to object
                    //ticketGenerate.EventID = ticketEventDetails.EventID;
                    //ticketGenerate.UserID = ticketEventDetails.UserID;

                    //if (perData[6].ToString().Contains("0001"))
                    //{
                    //    ticketGenerate.Dob = DateTime.Now;
                    //}
                    //else
                    //{
                    //    ticketGenerate.Dob = DateTime.Now;
                    //}

                    //ticketGenerate.Gender = ticketEventDetails.Gender;
                    //ticketGenerate.Address = ticketEventDetails.Address;
                    //ticketGenerate.City = ticketEventDetails.City;
                    //ticketGenerate.State = ticketEventDetails.State;
                    //ticketGenerate.Country = ticketEventDetails.Country;
                    //ticketGenerate.PostalCode = ticketEventDetails.PostalCode;
                    //ticketGenerate.Email = ticketEventDetails.Email;
                    //ticketGenerate.PhoneNumber = ticketEventDetails.PhoneNumber;
                    //ticketGenerate.TicketNumber = ticketEventDetails.TicketNumber;
                    //ticketGenerate.TicketType = ticketEventDetails.TicketType;
                    //ticketGenerate.Mode = ticketEventDetails.Mode;
                    //ticketGenerate.TicketSerialNumber = ticketEventDetails.TicketSerialNumber;
                    //ticketGenerate.ScannedTicket = img;
                    //ticketGenerate.OrderNum = orderNo;
                    //// ticketGenerate.TourDateID =Convert.ToInt32(tourDateId);
                    //ticketGenerate.Quantity = qty;

                    //new SpRepository().SpAddTicketingEventTicketConfirmation(ticketGenerate);
                }
            }
            return ticketEventDetails;
        }
        public string GetSerialNumber(int tourDateId)
        {
            string ticketNumber = string.Empty;
            List<TicketingEventTicketConfirmation> lst = new List<TicketingEventTicketConfirmation>();
            GenericRepository<TicketingEventTicketConfirmation> _TicketingEvtTicketRepo = new GenericRepository<TicketingEventTicketConfirmation>(_unitOfWork);
            lst = _TicketingEvtTicketRepo.Repository.GetAll().Where(p => p.TourDateID == tourDateId).ToList();
            if (lst.Count < 10)
            {
                ticketNumber = "000" + (lst.Count + 1).ToString();
            }
            else if (lst.Count < 100)
            {
                ticketNumber = "00" + (lst.Count + 1).ToString();
            }
            else if (lst.Count < 1000)
            {
                ticketNumber = "0" + (lst.Count + 1).ToString();
            }
            else
            {
                ticketNumber = (lst.Count + 1).ToString();
            }
            return ticketNumber;
        }

        public static int RandomNumber(int min, int max)
        {
            Random random = new Random();
            return random.Next(min, max);
        }

        public static string GenerateZXingQRCode(string url)
        {
            string alt = "QR code";
            int height = 500;
            int width = 500;
            int margin = 0;
            var qrWriter = new BarcodeWriter()
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new EncodingOptions() { Height = height, Width = width, Margin = margin }
            };

            using (var q = qrWriter.Write(url))
            {
                using (var ms = new MemoryStream())
                {
                    q.Save(ms, ImageFormat.Png);
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        public static string SaveQRCodeImage(string ImgStr, string ImgName)
        {

            String path = HttpContext.Current.Server.MapPath("~/Content/QRCodeImages"); //Path

            //Check if directory exist
            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path); //Create directory if it doesn't exist
            }
            var random = Guid.NewGuid();
            string imageName = random + ".jpg";

            //set the image path
            string imgPath = Path.Combine(path, imageName);

            byte[] imageBytes = Convert.FromBase64String(ImgStr);

            File.WriteAllBytes(imgPath, imageBytes);

            return imageName;
        }

        public static string SaveImage(string ImgStr, string ImgName)
        {
            String path = HttpContext.Current.Server.MapPath("~/Content/EventImages"); //Path

            //Check if directory exist
            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path); //Create directory if it doesn't exist
            }
            var random = Guid.NewGuid();
            string imageName = random + ".jpg";

            //set the image path
            string imgPath = Path.Combine(path, imageName);

            byte[] imageBytes = Convert.FromBase64String(ImgStr);

            File.WriteAllBytes(imgPath, imageBytes);

            return imageName;
        }

        #endregion

        #region "Generate QR Code"
        [HttpGet]
        [Route("api/TicketingAPI/GenerateQRCode")]
        public Image GenerateQRCode(string dataString)
        {

            QRCodeEncoder qrCodeEncoder = new QRCodeEncoder();
            String encoding = "AlphaNumeric";
            if (encoding == "Byte")
            {
                qrCodeEncoder.QRCodeEncodeMode = QRCodeEncoder.ENCODE_MODE.BYTE;
            }
            else if (encoding == "AlphaNumeric")
            {
                qrCodeEncoder.QRCodeEncodeMode = QRCodeEncoder.ENCODE_MODE.ALPHA_NUMERIC;
            }
            else if (encoding == "Numeric")
            {
                qrCodeEncoder.QRCodeEncodeMode = QRCodeEncoder.ENCODE_MODE.NUMERIC;
            }
            try
            {
                int scale = Convert.ToInt16(200);
                qrCodeEncoder.QRCodeScale = scale;
            }
            catch (Exception)
            {
                //MessageBox.Show("Invalid size!");
                //return;
            }
            try
            {
                int version = Convert.ToInt16(200);
                qrCodeEncoder.QRCodeVersion = version;
            }
            catch (Exception)
            {
                //MessageBox.Show("Invalid version !");
            }

            string errorCorrect = "M";
            if (errorCorrect == "L")
                qrCodeEncoder.QRCodeErrorCorrect = QRCodeEncoder.ERROR_CORRECTION.L;
            else if (errorCorrect == "M")
                qrCodeEncoder.QRCodeErrorCorrect = QRCodeEncoder.ERROR_CORRECTION.M;
            else if (errorCorrect == "Q")
                qrCodeEncoder.QRCodeErrorCorrect = QRCodeEncoder.ERROR_CORRECTION.Q;
            else if (errorCorrect == "H")
                qrCodeEncoder.QRCodeErrorCorrect = QRCodeEncoder.ERROR_CORRECTION.H;

            Image image;
            String data = dataString;
            image = qrCodeEncoder.Encode(data);
            return image;
        }
        #endregion

        #region "Get Details of QR Code"
        [HttpGet]

        public HttpResponseMessage TicketingUserQRDetails(string qrCode)
        {
            string result = "OK";
            try
            {

                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception)
            {
                result = "Failure";
                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
        }
        #endregion

        #region "Ticketing User Status Updation"
        [AllowAnonymous]
        [Route("TicketingAPI/ChangeTicketingUserStatus")]
        [HttpPost]
        public bool ChangeTicketingUserStatus(Int64 ID, Int16 InactivePeriod)
        {
            Models.TicketingUsers _Users = null;
            GenericRepository<TicketingUsers> _UsersRepo = new GenericRepository<TicketingUsers>(_unitOfWork);

            _Users = _UsersRepo.Repository.Get(p => p.UserID == ID);

            if (_Users != null)
            {
                if (_Users.UserID > 0) // if not admin
                {
                    _Users.RecordStatus = _Users.RecordStatus == RecordStatus.Active.ToString() ? RecordStatus.InActive.ToString() : RecordStatus.Active.ToString();

                    _UsersRepo.Repository.Update(_Users);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        #endregion

        #region "Artist List for Binding"
        [AllowAnonymous]
        [Route("api/TicketingAPI/GetArtistsList")]
        [HttpGet]
        public HttpResponseMessage GetArtistsList()
        {
            try
            {
                // var _ViewArtistlst = "";
                GenericRepository<Artists> _ArtistsRepo = new GenericRepository<Artists>(_unitOfWork);
                DataSet ds = new SpRepository().GetArtist();
                //List<UserTicketsNumber> lstTickets = new List<UserTicketsNumber>();
                if (ds.Tables[0].Rows.Count > 0)
                {
                    //var _ViewArtistlst=
                    var _ViewArtistlst = ds.Tables[0].AsEnumerable().Select(dataRow => new ArtistsModelList
                    {
                        ArtistId = dataRow.Field<int>("ArtistID"),
                        ArtistName = dataRow.Field<string>("ArtistName")
                    }).ToList();

                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, _ViewArtistlst));
                }

                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, null));
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, ex.Message);
            }
        }
        #endregion

        #region "Purchase Ticket by User"
        [HttpPost]
        [Route("api/TicketingAPI/PurchaseTicketByUser")]
        public HttpResponseMessage PurchaseTicketByUser([FromBody] TicketingEventTicketsModel events)
        {
            string result = string.Empty;
            Models.EventReponse tmpResponse = new EventReponse();

            Models.TicketingEventsNewModel ticketingEventEntity = new Models.TicketingEventsNewModel();
            GenericRepository<Artists> _artists = new GenericRepository<Artists>(_unitOfWork);
            GenericRepository<TicketingEventsNew> _ticketingEntity = new GenericRepository<TicketingEventsNew>(_unitOfWork);
            GenericRepository<TicketingEventTicketsModel> _ticketingEventTicketsEntity = new GenericRepository<TicketingEventTicketsModel>(_unitOfWork);

            Models.TicketingEventsNew entity = new Models.TicketingEventsNew();

            Models.TicketingEventTicketsModel entityTickets = new Models.TicketingEventTicketsModel();

            try
            {
                entityTickets.EventID = events.EventID;
                entityTickets.UserId = events.UserId;


                if (!String.IsNullOrEmpty(events.NumberOfTickets.ToString()))
                {
                    entityTickets.NumberOfTickets = events.NumberOfTickets;
                }
                else
                {
                    tmpResponse.ResponseId = 400;
                    tmpResponse.ReturnMessage = "Number Of Tickets Not Provided";

                    ticketingEventEntity.MessageResponse = tmpResponse;
                    return Request.CreateResponse(HttpStatusCode.OK, entity);
                }

                if (entityTickets.NumberOfTickets > 0)
                {
                    for (int i = 0; i < entityTickets.NumberOfTickets; i++)
                    {
                        GenericRepository<TicketingEventTickets> _tickets = new GenericRepository<TicketingEventTickets>(_unitOfWork);

                        List<TicketingEventTickets> lstModel = new List<TicketingEventTickets>();

                        lstModel = _tickets.Repository.GetAll().Where(p => p.EventID == entityTickets.EventID && p.UserId == null).ToList();
                        if (lstModel != null)
                        {
                            Musika.Models.TicketingEventTicketsModel temp = new Musika.Models.TicketingEventTicketsModel();
                            for (int j = 0; j < 1; j++)
                            {

                                temp.UserId = entityTickets.UserId;
                                temp.EventID = lstModel[0].EventID;
                                temp.TicketNumber = lstModel[0].TicketNumber;
                                new Musika.Repository.SPRepository.SpRepository().SpUpdateTicketingEventTickets(temp.EventID.GetValueOrDefault(), temp.TicketNumber.ToString(), temp.UserId);
                                //_ticketingEventTicketsEntity.Repository.Update(temp);
                            }
                        }
                    }
                }

                tmpResponse.ResponseId = 200;
                tmpResponse.ReturnMessage = "Success";

                ticketingEventEntity.MessageResponse = tmpResponse;
                return Request.CreateResponse(HttpStatusCode.OK, entity);
            }
            catch (Exception ee)
            {
                LogHelper.CreateLog3(ee, Request);
                _unitOfWork.RollBack();//RollBack Transaction

                tmpResponse.ResponseId = 400;
                tmpResponse.ReturnMessage = "Unexpected Error";

                ticketingEventEntity.MessageResponse = tmpResponse;
                return Request.CreateResponse(HttpStatusCode.OK, entity);
            }
        }
        #endregion
        private List<UserTicketsNumber> GetTicketsStatus(int userid, int eventid)
        {
            DataSet ds = new SpRepository().GetUsersTicketStatus(userid, eventid);
            List<UserTicketsNumber> lstTickets = new List<UserTicketsNumber>();
            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
            {
                UserTicketsNumber ticket = new UserTicketsNumber();
                ticket.Status = Convert.ToString(ds.Tables[0].Rows[i][0].ToString());
                ticket.TicketNumber = Convert.ToString(ds.Tables[0].Rows[i][1].ToString());
                lstTickets.Add(ticket);
            }
            return lstTickets;
        }
        private List<UserTicketsNumber> GetSearchTicketsStatus(int userid, int eventid, string SearchBy)
        {
            DataSet ds = new SpRepository().SpSearchAttendesTicketStatus(userid, eventid, SearchBy);
            List<UserTicketsNumber> lstTickets = new List<UserTicketsNumber>();
            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
            {
                UserTicketsNumber ticket = new UserTicketsNumber();
                ticket.Status = Convert.ToString(ds.Tables[0].Rows[i][0].ToString());
                ticket.TicketNumber = Convert.ToString(ds.Tables[0].Rows[i][1].ToString());
                lstTickets.Add(ticket);
            }
            return lstTickets;
        }

        [HttpPost]
        [Route("api/TicketingAPI/GetUsersListForAnEvent")]
        public HttpResponseMessage GetUsersListForAnEvent([FromBody] TicketingEventTicketsModel events)
        {
            string result = string.Empty;
            EventReponse tmpResponse = new EventReponse();

            TicketingEventTickets entityTickets = new TicketingEventTickets();

            List<TicketingUserListEventNew> _lists = new List<TicketingUserListEventNew>();

            // Get Users List for An Event
            DataSet ds2 = new SpRepository().SpGetUsersListForAnEvent(Convert.ToInt32(events.EventID.ToString()));

            TicketingUserListEventNew model;
            if (ds2.Tables[0].Rows.Count > 0)
            {
                for (int i = 0; i < ds2.Tables[0].Rows.Count; i++)
                {
                    DataRow dr;
                    dr = ds2.Tables[0].Rows[i];
                    model = new TicketingUserListEventNew();
                    model.UserID = Convert.ToInt32(dr["UserID"].ToString());
                    model.UserName = Convert.ToString(dr["UserName"].ToString());
                    model.Email = Convert.ToString(dr["Email"].ToString());
                    model.Password = Convert.ToString(dr["Password"].ToString());
                    //model.TicketID = Convert.ToInt32(dr["TicketID"].ToString()).ToString();
                    model.EventID = Convert.ToString(dr["EventID"].ToString());
                    // model.TicketNumber = Convert.ToString(dr["TicketNumber"].ToString());
                    model.ImageUrl = Convert.ToString(dr["ImageUrl"].ToString());
                    model.lstTicket = GetTicketsStatus(Convert.ToInt32(dr["UserID"].ToString()), Convert.ToInt32(dr["EventID"].ToString()));
                    // model.Status = Convert.ToString(dr["Status"].ToString());

                    _lists.Add(model);
                }
            }
            else
            {
                _lists = null;
            }

            TicketsListModelNew _listFinal = new TicketsListModelNew();
            List<TicketingUserListEventNew> lstTicketingUsers = new List<TicketingUserListEventNew>();

            if (_lists != null)
            {
                for (int i = 0; i < _lists.Count; i++)
                {
                    TicketingUserListEventNew temp = new TicketingUserListEventNew();
                    temp.EventID = events.EventID.ToString();

                    temp.UserID = Convert.ToInt32(_lists[i].UserID);
                    temp.UserName = Convert.ToString(_lists[i].UserName);
                    temp.Email = Convert.ToString(_lists[i].Email);
                    temp.Password = Convert.ToString(_lists[i].Password);
                    //temp.TicketNumber = Convert.ToString(_lists[i].TicketNumber);
                    temp.ImageUrl = Convert.ToString(_lists[i].ImageUrl);
                    //temp.Status = Convert.ToString(_lists[i].Status);
                    temp.lstTicket = GetTicketsStatus(Convert.ToInt32(_lists[i].UserID), Convert.ToInt32(events.EventID.ToString()));
                    lstTicketingUsers.Add(temp);
                }
            }

            UserTicketsSummary tempSummary = new UserTicketsSummary();

            DataSet ds = new SpRepository().SpGetTicketsSummaryForAnEvent(Convert.ToInt32(events.EventID.ToString()));
            if (ds.Tables[0].Rows.Count > 0)
            {
                DataRow dr = ds.Tables[0].Rows[0];
                tempSummary.TotalTickets = dr["TotalTickets"].ToString();
                tempSummary.TicketsScanned = dr["TicketsScanned"].ToString();
                tempSummary.TicketsSold = dr["TicketsSold"].ToString();
            }

            _listFinal.lstUsers = lstTicketingUsers;
            _listFinal.lstTicketSummary = tempSummary;

            if (_listFinal != null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, _listFinal));
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, null));
            }
        }



        class DashboardDataSummary
        {
            public List<string> events { get; set; }
            public List<int> counts { get; set; }
        }

        #region "DashboardChart"
        [HttpGet]
        [Route("api/TicketingAPI/GetTicketAnalyticsDetails")]
        public HttpResponseMessage GetTicketAnalyticsDetails()
        {
            DashboardDataSummary result = new DashboardDataSummary();
            try
            {
                List<string> lstEvents = new List<string>();
                List<int> lstCounts = new List<int>();

                GenericRepository<TicketingEventsNew> _UsersRepo = new GenericRepository<TicketingEventsNew>(_unitOfWork);
                List<TicketingEventsNew> lstData = _UsersRepo.Repository.GetAll();
                TicketingEventsNew data = new TicketingEventsNew();
                if (lstData.Count > 0)
                {
                    for (int i = 0; i < lstData.Count; i++)
                    {
                        data = lstData[i];
                        lstEvents.Add(data.EventTitle);
                        lstCounts.Add(Convert.ToInt32(data.NumberOfTickets));
                    }
                }
                result.events = lstEvents;
                result.counts = lstCounts;
                Models.EventReponse tmpResponse = new EventReponse();

                tmpResponse.ResponseId = 200;
                tmpResponse.ReturnMessage = "Success";
                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception)
            {
                Models.EventReponse tmpResponse = new EventReponse();

                tmpResponse.ResponseId = 400;
                tmpResponse.ReturnMessage = "Unexpected Error.";
                return Request.CreateResponse(HttpStatusCode.OK, tmpResponse);
            }
        }
        #endregion

        #region "Dashboard Summary by EventID"
        [HttpGet]
        [Route("api/TicketingAPI/GetEventDetailsByEventId")]
        public HttpResponseMessage GetEventDetailsByEventId(int eventID)
        {
            DataSet ds = new DataSet();
            Models.EventReponse tmpResponse = new EventReponse();
            Models.DashboardSummarybyEventId model = new DashboardSummarybyEventId();
            try
            {
                ds = new Musika.Repository.SPRepository.SpRepository().SpDashboradSummary(eventID);

                if (ds.Tables[0].Rows.Count > 0)
                {
                    DataRow dr = ds.Tables[0].Rows[0];
                    model.EventId = Convert.ToInt32(dr["EventId"].ToString());
                    model.StartDate = Convert.ToDateTime(dr["StartDate"].ToString());
                    model.StartTime = Convert.ToString(dr["StartTime"].ToString());
                    model.EndDate = Convert.ToDateTime(dr["EndDate"].ToString());
                    model.EndTime = Convert.ToString(dr["EndTime"].ToString());
                    if (!String.IsNullOrEmpty(dr["TotalTickets"].ToString()))
                    {
                        model.TotalTickets = Convert.ToInt32(dr["TotalTickets"].ToString());
                    }
                    else
                    {
                        model.TotalTickets = 0;
                    }
                    if (!String.IsNullOrEmpty(dr["TotalSales"].ToString()))
                    {
                        model.TotalSales = Convert.ToInt32(dr["TotalSales"].ToString());
                    }
                    else
                    {
                        model.TotalSales = 0;
                    }
                    if (!String.IsNullOrEmpty(dr["Attendees"].ToString()))
                    {
                        model.Attendees = Convert.ToInt32(dr["Attendees"].ToString());
                    }
                    else
                    {
                        model.Attendees = 0;
                    }
                }
                tmpResponse.ResponseId = 200;
                tmpResponse.ReturnMessage = "Success";
                model.MessageResponse = tmpResponse;
                return Request.CreateResponse(HttpStatusCode.OK, model);
            }
            catch (Exception)
            {
                tmpResponse.ResponseId = 400;
                tmpResponse.ReturnMessage = "Unexpected Error.";
                model.MessageResponse = tmpResponse;
                return Request.CreateResponse(HttpStatusCode.OK, model);
            }
        }
        #endregion

        #region "List Of Users Attending the Event"
        [HttpGet]
        [Route("api/TicketingAPI/GetListOfAttendingUsers")]
        public HttpResponseMessage GetListOfAttendingUsers(int eventId)
        {
            DataSet ds = new DataSet();
            Models.EventReponse tmpResponse = new EventReponse();
            List<TicketingEventAttendingUsers> models = new List<TicketingEventAttendingUsers>();

            try
            {
                ds = new Musika.Repository.SPRepository.SpRepository().SpUsersAttendingEvent(eventId);

                if (ds.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        TicketingEventAttendingUsers model = new TicketingEventAttendingUsers();
                        DataRow dr = ds.Tables[0].Rows[i];
                        model.Userid = Convert.ToInt32(dr["Userid"].ToString());
                        model.UserType = Convert.ToString(dr["UserType"].ToString());
                        model.UserName = Convert.ToString(dr["UserName"].ToString());
                        if (!String.IsNullOrEmpty(dr["Addres"].ToString()))
                        {
                            model.Addres = Convert.ToString(dr["Addres"].ToString());
                        }
                        else
                        {
                            model.Addres = string.Empty;
                        }
                        if (!String.IsNullOrEmpty(dr["City"].ToString()))
                        {
                            model.City = Convert.ToString(dr["City"].ToString());
                        }
                        else
                        {
                            model.City = string.Empty;
                        }
                        if (!String.IsNullOrEmpty(dr["State"].ToString()))
                        {
                            model.State = Convert.ToString(dr["State"].ToString());
                        }
                        else
                        {
                            model.State = string.Empty;
                        }
                        if (!String.IsNullOrEmpty(dr["Country"].ToString()))
                        {
                            model.Country = Convert.ToString(dr["Country"].ToString());
                        }
                        else
                        {
                            model.Country = string.Empty;
                        }
                        if (!String.IsNullOrEmpty(dr["PostalCode"].ToString()))
                        {
                            model.PostalCode = Convert.ToString(dr["PostalCode"].ToString());
                        }
                        else
                        {
                            model.PostalCode = string.Empty;
                        }
                        if (!String.IsNullOrEmpty(dr["PhoneNumber"].ToString()))
                        {
                            model.PhoneNumber = Convert.ToString(dr["PhoneNumber"].ToString());
                        }
                        else
                        {
                            model.PhoneNumber = string.Empty;
                        }
                        if (!String.IsNullOrEmpty(dr["Email"].ToString()))
                        {
                            model.Email = Convert.ToString(dr["Email"].ToString());
                        }
                        else
                        {
                            model.Email = string.Empty;
                        }
                        models.Add(model);
                    }
                }
                tmpResponse.ResponseId = 200;
                tmpResponse.ReturnMessage = "Success";
                return Request.CreateResponse(HttpStatusCode.OK, models);
            }
            catch (Exception)
            {
                tmpResponse.ResponseId = 400;
                tmpResponse.ReturnMessage = "Failure";
                return Request.CreateResponse(HttpStatusCode.OK, tmpResponse);
            }
        }
        #endregion

        #region "Get Country List"
        [HttpGet]
        [Route("api/TicketingAPI/GetCountriesList")]
        public HttpResponseMessage GetCountriesList()
        {
            //DataSet ds = new DataSet();
            Models.EventReponse tmpResponse = new EventReponse();
            List<CountryCodes> models = new List<CountryCodes>();
            GenericRepository<CountryCodes> _CountriesEntity = new GenericRepository<CountryCodes>(_unitOfWork);
            var _list = _CountriesEntity.Repository.GetAll().ToList();

            if (_list.Count > 0)
            {
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, _list));
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, null));
            }
        }
        #endregion

        #region "Get Ticket Details by UserId and EventId"
        [HttpGet]
        [Route("api/TicketingAPI/GetTicketDetailsByUserId")]
        public HttpResponseMessage GetTicketDetailsByUserId(int eventId, int userId)
        {
            Models.EventReponse tmpResponse = new EventReponse();


            List<UserTicketDetails> _list = new List<UserTicketDetails>();
            UserTicketDetails temp;

            if (!String.IsNullOrEmpty(eventId.ToString()))
            {
                DataSet ds = new DataSet();
                ds = new Musika.Repository.SPRepository.SpRepository().GetTicketingEventDetailsByUserId(userId);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        DataRow dr = ds.Tables[0].Rows[i];
                        temp = new UserTicketDetails();
                        temp.UserId = Convert.ToInt32(dr["UserId"].ToString());
                        temp.UserName = dr["UserName"].ToString();
                        temp.Email = dr["Email"].ToString();
                        temp.ImageURL = dr["ImageURL"].ToString();
                        temp.Status = dr["Status"].ToString();
                        temp.TicketType = dr["TicketType"].ToString();
                        temp.CheckInDateTime = dr["CheckInDateTime"].ToString();
                        temp.TicketNumber = dr["TicketNumber"].ToString();
                        temp.VenueName = dr["VenueName"].ToString();
                        temp.EventId = Convert.ToInt32(dr["EventId"].ToString());
                        _list.Add(temp);
                    }
                }

                if (_list.Count > 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, _list.OrderByDescending(p => Convert.ToDateTime((p.CheckInDateTime == null || p.CheckInDateTime == String.Empty) ? "01/01/1901 00:00:00 AM" : p.CheckInDateTime)).ToList()));
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, null));
                }
            }
            else
            {
                DataSet ds = new DataSet();
                ds = new Musika.Repository.SPRepository.SpRepository().GetTicketingEventDetailsByUserIdAndEventId(userId, eventId);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        DataRow dr = ds.Tables[0].Rows[i];
                        temp = new UserTicketDetails();
                        temp.UserId = Convert.ToInt32(dr["UserId"].ToString());
                        temp.UserName = dr["UserName"].ToString();
                        temp.Email = dr["Email"].ToString();
                        temp.ImageURL = dr["ImageURL"].ToString();
                        temp.Status = dr["Status"].ToString();
                        temp.TicketType = dr["TicketType"].ToString();
                        temp.CheckInDateTime = dr["CheckInDateTime"].ToString();
                        temp.TicketNumber = dr["TicketNumber"].ToString();
                        temp.VenueName = dr["VenueName"].ToString();
                        temp.EventId = Convert.ToInt32(dr["EventId"].ToString());
                        _list.Add(temp);
                    }
                }

                if (_list.Count > 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, _list));
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, null));
                }
            }
        }
        #endregion

        #region "Get Event for Editing"
        [AllowAnonymous]
        [HttpGet]
        [Route("api/TicketingAPI/GetTicketingEventsDetailById")]
        public TicketingEventsNewModel GetTicketingEventsDetailById(string eventId)
        {
            int id = Convert.ToInt32(eventId);
            EventReponse result = new EventReponse();
            TicketingEventsNewModel model = new TicketingEventsNewModel();

            GenericRepository<TicketingEventsNew> _UsersRepo = new GenericRepository<TicketingEventsNew>(_unitOfWork);

            GenericRepository<TicketingEventTicketsSummary> _SummaryRepo = new GenericRepository<TicketingEventTicketsSummary>(_unitOfWork);
            GenericRepository<TicketingEventNewStaff> _StaffRepo = new GenericRepository<TicketingEventNewStaff>(_unitOfWork);
            GenericRepository<TicketingUsers> _TicketingUsersRepo = new GenericRepository<TicketingUsers>(_unitOfWork);
            GenericRepository<TicketingEventTicketsSummary> _TicketDataRepo = new GenericRepository<TicketingEventTicketsSummary>(_unitOfWork);

            try
            {
                DataSet ds = new DataSet();
                ds = new SpRepository().GetTicketingEventsDetailAdmin(id);
                List<TicketingEventDetailsModel> _list = new List<TicketingEventDetailsModel>();
                if (ds.Tables[0].Rows.Count > 0)
                {
                    TicketingEventDetailsModel temp = new TicketingEventDetailsModel();
                    DataRow dr = ds.Tables[0].Rows[0];
                    temp.EventID = Convert.ToInt32(dr[0].ToString());
                    temp.EventTitle = Convert.ToString(dr[1].ToString());
                    temp.EventLocation = Convert.ToString(dr[2].ToString());
                    temp.VenueName = Convert.ToString(dr[3].ToString());
                    temp.Address1 = Convert.ToString(dr[4].ToString());
                    temp.Address2 = Convert.ToString(dr[5].ToString());
                    temp.City = Convert.ToString(dr[6].ToString());
                    temp.State = Convert.ToString(dr[7].ToString());
                    temp.ZipCode = Convert.ToString(dr[8].ToString());
                    temp.StartDate = Convert.ToDateTime(dr[9].ToString());
                    temp.StartTime = dr[10].ToString();
                    temp.EndDate = Convert.ToDateTime(dr[11].ToString());
                    temp.EndTime = Convert.ToString(dr[12].ToString());
                    temp.CreatedOn = Convert.ToDateTime(dr[13].ToString());
                    temp.OrganizerName = Convert.ToString(dr[14].ToString());
                    if (!string.IsNullOrEmpty(dr[15].ToString()))
                    {
                        temp.EventImage = Convert.ToString(dr[15].ToString());
                    }
                    temp.EventDescription = Convert.ToString(dr[16].ToString());

                    temp.OrganizerDescription = Convert.ToString(dr[18].ToString());
                    //temp.TicketType = Convert.ToString(dr[18].ToString());
                    temp.ListingPrivacy = Convert.ToString(dr[19].ToString());
                    temp.EventType = Convert.ToString(dr[21].ToString());
                    temp.EventTopic = Convert.ToString(dr[20].ToString());

                    temp.ArtistId = Convert.ToInt32(dr[23].ToString());
                    temp.ArtistName = Convert.ToString(dr[24].ToString());
                    //temp.TicketUrl = Convert.ToString(dr[23].ToString());
                    // temp.IsApproved = Convert.ToBoolean(dr[24].ToString());
                    _list.Add(temp);
                }



                if (_list.Count > 0)
                {
                    model.EventID = _list[0].EventID;
                    model.EventTitle = _list[0].EventTitle;
                    model.EventLocation = _list[0].EventLocation;
                    model.VenueName = _list[0].VenueName;
                    model.Address1 = _list[0].Address1;
                    model.Address2 = _list[0].Address2;
                    model.City = _list[0].City;
                    model.State = _list[0].State;
                    model.ZipCode = _list[0].ZipCode;
                    model.StartDate = Convert.ToDateTime(_list[0].StartDate);
                    model.StartTime = _list[0].StartTime;
                    model.EndDate = Convert.ToDateTime(_list[0].EndDate);
                    model.EndTime = _list[0].EndTime;
                    model.EventImage = _list[0].EventImage;
                    model.EventDescription = _list[0].EventDescription;
                    model.OrganizerName = _list[0].OrganizerName;
                    model.OrganizerDescription = _list[0].OrganizerDescription;
                    model.TicketType = _list[0].TicketType;
                    model.ListingPrivacy = _list[0].ListingPrivacy;
                    model.EventType = _list[0].EventType;
                    model.EventTopic = _list[0].EventTopic;
                    //model.ShowTicketNumbers = _list[0].ShowTicketNumbers;
                    model.OrganizerName = _list[0].OrganizerName.ToString();
                    model.CreatedOn = Convert.ToDateTime(_list[0].CreatedOn);
                    //model.NumberOfTickets = Convert.ToInt32(_list[0].NumberOfTickets);
                    model.ArtistId = Convert.ToInt32(_list[0].ArtistId);
                    model.ArtistName = _list[0].ArtistName;
                    //  model.IsApproved = _list[0].IsApproved;
                    List<Musika.Models.TicketingEventNewStaff> tmpTicketStaff = new List<TicketingEventNewStaff>();
                    List<Staff> tmpStaff = new List<Staff>();
                    tmpTicketStaff = _StaffRepo.Repository.GetAll().Where(p => p.EventId == model.EventID).ToList();
                    if (tmpTicketStaff.Count > 0)
                    {
                        List<Musika.Models.TicketingUsers> tmpTicketStaffUser = new List<TicketingUsers>();
                        //   tmpTicketStaffUser = _TicketingUsersRepo.Repository.GetAll().Where(p => p.UserID == tmpTicketStaff.).ToList();
                        for (int i = 0; i < tmpTicketStaff.Count; i++)
                        {
                            tmpTicketStaffUser = _TicketingUsersRepo.Repository.GetAll().Where(p => p.UserID == tmpTicketStaff[i].StaffId).ToList();
                            Staff data = new Staff();
                            data.UserId = tmpTicketStaffUser[0].UserID;// model.EventID;//
                            data.UserName = tmpTicketStaffUser[0].UserName;
                            tmpStaff.Add(data);
                        }
                    }

                    model.lstStaff = tmpStaff;

                    Ticket ticket = new Ticket();

                    List<Musika.Models.TicketingEventTicketsSummary> tmpSummary = new List<Musika.Models.TicketingEventTicketsSummary>();
                    tmpSummary = _SummaryRepo.Repository.GetAll().Where(p => p.EventID == model.EventID).ToList();
                    if (tmpSummary.Count > 0)
                    {
                        Ticket tt = new Ticket();
                        tt.EventId = Convert.ToInt32(tmpSummary[0].EventID);
                        tt.Currency = tmpSummary[0].Currency;
                        tt.CountryId = Convert.ToInt32(tmpSummary[0].CountryId);
                        tt.RefundPolicy = tmpSummary[0].RefundPolicy;

                        model.Ticket = tt;
                    }
                    else
                    {
                        Ticket tt = new Ticket();
                        tt.EventId = 0;
                        tt.Currency = String.Empty;
                        tt.CountryId = 0;
                        tt.RefundPolicy = String.Empty;

                        model.Ticket = tt;
                    }

                    List<Musika.Models.TicketingEventTicketsSummary> lstTicketData = new List<TicketingEventTicketsSummary>();
                    List<Musika.Models.TicketData> tmpTicketData = new List<TicketData>();
                    lstTicketData = _TicketDataRepo.Repository.GetAll().Where(p => p.EventID == model.EventID).ToList();
                    if (lstTicketData.Count > 0)
                    {
                        for (int i = 0; i < lstTicketData.Count; i++)
                        {
                            Musika.Models.TicketData data = new TicketData();
                            data.EventId = model.EventID;
                            data.Price = Convert.ToDecimal(lstTicketData[i].Cost);
                            data.Quantity = lstTicketData[i].Quantity;
                            data.TicketCategory = lstTicketData[i].TicketCategory;
                            data.TicketType = lstTicketData[i].TicketType;
                            data.PackageStartDate = String.Format("{0:yyyy-MM-dd}", lstTicketData[i].PackageStartDate);
                            data.PackageEndDate = String.Format("{0:yyyy-MM-dd}", lstTicketData[i].PackageEndDate);

                            tmpTicketData.Add(data);
                        }
                    }
                    else
                    {
                        Musika.Models.TicketData data = new TicketData();
                        data.EventId = 0;
                        data.Price = 0;
                        data.Quantity = 0;
                        data.TicketCategory = String.Empty;
                        data.TicketType = string.Empty;
                        tmpTicketData.Add(data);
                    }
                    model.Ticket.lstTicketData = tmpTicketData;
                }
            }
            catch (Exception ee)
            {
                result.ResponseId = 400;
                result.ReturnMessage = ee.Message + "\n" + ee.StackTrace;
                model.MessageResponse = result;
            }
            return model;
        }
        #endregion



        #region "Get Event for Editing"
        [HttpGet]
        [Route("api/TicketingAPI/GetTicketingEventsByIdForEdit")]
        public TicketingEventsNewModel GetTicketingEventsByIdForEdit(string eventId)
        {
            int id = Convert.ToInt32(eventId);
            EventReponse result = new EventReponse();
            TicketingEventsNewModel model = new TicketingEventsNewModel();

            GenericRepository<TicketingEventsNew> _UsersRepo = new GenericRepository<TicketingEventsNew>(_unitOfWork);

            GenericRepository<TicketingEventTicketsSummary> _SummaryRepo = new GenericRepository<TicketingEventTicketsSummary>(_unitOfWork);
            GenericRepository<TicketingEventNewStaff> _StaffRepo = new GenericRepository<TicketingEventNewStaff>(_unitOfWork);
            GenericRepository<TicketingUsers> _TicketingUsersRepo = new GenericRepository<TicketingUsers>(_unitOfWork);
            GenericRepository<TicketingEventTicketsSummary> _TicketDataRepo = new GenericRepository<TicketingEventTicketsSummary>(_unitOfWork);

            try
            {
                DataSet ds = new DataSet();
                ds = new SpRepository().GetTicketingEventsNewByEventID(id);
                List<TicketingEventsNew> _list = new List<TicketingEventsNew>();
                if (ds.Tables[0].Rows.Count > 0)
                {
                    TicketingEventsNew temp = new TicketingEventsNew();
                    DataRow dr = ds.Tables[0].Rows[0];
                    temp.EventID = Convert.ToInt32(dr[0].ToString());
                    temp.EventTitle = Convert.ToString(dr[1].ToString());
                    temp.EventLocation = Convert.ToString(dr[2].ToString());
                    temp.VenueName = Convert.ToString(dr[3].ToString());
                    temp.Address1 = Convert.ToString(dr[4].ToString());
                    temp.Address2 = Convert.ToString(dr[5].ToString());
                    temp.City = Convert.ToString(dr[6].ToString());
                    temp.State = Convert.ToString(dr[7].ToString());
                    temp.ZipCode = Convert.ToString(dr[8].ToString());
                    temp.StartDate = Convert.ToDateTime(dr[9].ToString());
                    temp.StartTime = dr[10].ToString();
                    temp.EndDate = Convert.ToDateTime(dr[11].ToString());
                    temp.EndTime = Convert.ToString(dr[12].ToString());
                    temp.CreatedBy = Convert.ToInt32(dr[13].ToString());
                    if (!string.IsNullOrEmpty(dr[14].ToString()))
                    {
                        temp.EventImage = ImageToBase64(Convert.ToString(dr[14].ToString()));
                    }
                    temp.EventDescription = Convert.ToString(dr[15].ToString());
                    temp.OrganizerName = Convert.ToString(dr[16].ToString());
                    temp.OrganizerDescription = Convert.ToString(dr[17].ToString());
                    temp.TicketType = Convert.ToString(dr[18].ToString());
                    temp.ListingPrivacy = Convert.ToString(dr[19].ToString());
                    temp.EventType = Convert.ToString(dr[20].ToString());
                    temp.EventTopic = Convert.ToString(dr[21].ToString());
                    temp.ShowTicketNumbers = Convert.ToInt32(dr[22].ToString());
                    temp.ArtistId = Convert.ToInt32(dr[23].ToString());
                    temp.TicketUrl = Convert.ToString(dr[24].ToString());
                    temp.IsApproved = Convert.ToBoolean(dr[25].ToString());
                    _list.Add(temp);
                }
                if (_list.Count > 0)
                {
                    model.EventID = _list[0].EventID;
                    model.EventTitle = _list[0].EventTitle;
                    model.EventLocation = _list[0].EventLocation;
                    model.VenueName = _list[0].VenueName;
                    model.Address1 = _list[0].Address1;
                    model.Address2 = _list[0].Address2;
                    model.City = _list[0].City;
                    model.State = _list[0].State;
                    model.ZipCode = _list[0].ZipCode;
                    model.StartDate = Convert.ToDateTime(_list[0].StartDate);
                    model.StartTime = _list[0].StartTime;
                    model.EndDate = Convert.ToDateTime(_list[0].EndDate);
                    model.EndTime = _list[0].EndTime;
                    model.EventImage = _list[0].EventImage;
                    model.EventDescription = _list[0].EventDescription;
                    model.OrganizerName = _list[0].OrganizerName;
                    model.OrganizerDescription = _list[0].OrganizerDescription;
                    model.TicketType = _list[0].TicketType;
                    model.ListingPrivacy = _list[0].ListingPrivacy;
                    model.EventType = _list[0].EventType;
                    model.EventTopic = _list[0].EventTopic;
                    model.ShowTicketNumbers = _list[0].ShowTicketNumbers;
                    model.CreatedBy = _list[0].CreatedBy;
                    model.CreatedOn = Convert.ToDateTime(_list[0].CreatedOn);
                    model.NumberOfTickets = Convert.ToInt32(_list[0].NumberOfTickets);
                    model.ArtistId = Convert.ToInt32(_list[0].ArtistId);
                    model.TicketUrl = _list[0].TicketUrl;
                    model.IsApproved = _list[0].IsApproved;
                    List<Musika.Models.TicketingEventNewStaff> tmpTicketStaff = new List<TicketingEventNewStaff>();
                    List<Staff> tmpStaff = new List<Staff>();
                    tmpTicketStaff = _StaffRepo.Repository.GetAll().Where(p => p.EventId == model.EventID).ToList();
                    if (tmpTicketStaff.Count > 0)
                    {
                        List<Musika.Models.TicketingUsers> tmpTicketStaffUser = new List<TicketingUsers>();
                        //   tmpTicketStaffUser = _TicketingUsersRepo.Repository.GetAll().Where(p => p.UserID == tmpTicketStaff.).ToList();
                        for (int i = 0; i < tmpTicketStaff.Count; i++)
                        {
                            tmpTicketStaffUser = _TicketingUsersRepo.Repository.GetAll().Where(p => p.UserID == tmpTicketStaff[i].StaffId).ToList();
                            Staff data = new Staff();
                            data.UserId = tmpTicketStaffUser[0].UserID;// model.EventID;//
                            data.UserName = tmpTicketStaffUser[0].UserName;
                            tmpStaff.Add(data);
                        }
                    }

                    model.lstStaff = tmpStaff;

                    Ticket ticket = new Ticket();

                    List<Musika.Models.TicketingEventTicketsSummary> tmpSummary = new List<Musika.Models.TicketingEventTicketsSummary>();
                    tmpSummary = _SummaryRepo.Repository.GetAll().Where(p => p.EventID == model.EventID).ToList();
                    if (tmpSummary.Count > 0)
                    {
                        Ticket tt = new Ticket();
                        tt.EventId = Convert.ToInt32(tmpSummary[0].EventID);
                        tt.Currency = tmpSummary[0].Currency;
                        tt.CountryId = Convert.ToInt32(tmpSummary[0].CountryId);
                        tt.RefundPolicy = tmpSummary[0].RefundPolicy;
                        tt.ServiceFee = tmpSummary[0].ServiceFee;
                        tt.Tax = tmpSummary[0].Tax;
                        model.Ticket = tt;
                    }
                    else
                    {
                        Ticket tt = new Ticket();
                        tt.EventId = 0;
                        tt.Currency = String.Empty;
                        tt.CountryId = 0;
                        tt.RefundPolicy = String.Empty;
                        tt.ServiceFee = "15";
                        tt.Tax = "18";
                        model.Ticket = tt;
                    }

                    List<Musika.Models.TicketingEventTicketsSummary> lstTicketData = new List<TicketingEventTicketsSummary>();
                    List<Musika.Models.TicketData> tmpTicketData = new List<TicketData>();
                    lstTicketData = _TicketDataRepo.Repository.GetAll().Where(p => p.EventID == model.EventID).ToList();
                    if (lstTicketData.Count > 0)
                    {
                        for (int i = 0; i < lstTicketData.Count; i++)
                        {
                            Musika.Models.TicketData data = new TicketData();
                            data.EventId = model.EventID;
                            data.Price = Convert.ToDecimal(lstTicketData[i].Cost);
                            data.Quantity = lstTicketData[i].Quantity;
                            data.TicketCategory = lstTicketData[i].TicketCategory;
                            data.TicketType = lstTicketData[i].TicketType;
                            data.PackageStartDate = String.Format("{0:yyyy-MM-dd}", lstTicketData[i].PackageStartDate);
                            data.PackageEndDate = String.Format("{0:yyyy-MM-dd}", lstTicketData[i].PackageEndDate);

                            tmpTicketData.Add(data);
                        }
                    }
                    else
                    {
                        Musika.Models.TicketData data = new TicketData();
                        data.EventId = 0;
                        data.Price = 0;
                        data.Quantity = 0;
                        data.TicketCategory = String.Empty;
                        data.TicketType = string.Empty;
                        tmpTicketData.Add(data);
                    }
                    model.Ticket.lstTicketData = tmpTicketData;
                }
            }
            catch (Exception ee)
            {
                result.ResponseId = 400;
                result.ReturnMessage = ee.Message + "\n" + ee.StackTrace;
                model.MessageResponse = result;
            }
            return model;
        }
        #endregion

        #region "Final Get Staff List for Binding"
        [HttpGet]
        [Route("api/TicketingAPI/GetStaffList")]
        public HttpResponseMessage GetStaffList(int userID)
        {
            GenericRepository<TicketingUsers> _TicketingUsers = new GenericRepository<TicketingUsers>(_unitOfWork);

            try
            {
                var _list = (from A1 in _TicketingUsers.Repository.GetAll(p => p.UserType == "Staff").Where(p => p.CreatedBy == userID && p.RecordStatus != "Deleted")
                             select new TicketingStaffList
                             {
                                 UserId = A1.UserID,
                                 UserName = A1.UserName,
                                 Email = A1.Email,
                                 PhoneNumber = A1.PhoneNumber,
                                 Country = A1.Country
                             }).OrderByDescending(s => s.UserId).ToList();

                if (_list.Count > 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, _list));
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, null));
                }
            }
            catch (Exception ee)
            {
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, ee.Message + "\n" + ee.StackTrace));
            }
        }
        #endregion

        #region "Get Number Of Males and Females for An Event"
        public class EventGenderMetric
        {
            public int EventID { get; set; }
            public int Males { get; set; }
            public int Females { get; set; }
        }

        [HttpGet]
        [Route("api/TicketingAPI/GetNumberOfMalesFemales")]
        public HttpResponseMessage GetNumberOfMalesFemales(int eventId)
        {
            Models.AttendeeSummary tmpResponse = new AttendeeSummary();
            DataSet ds = new DataSet();
            ds = new Musika.Repository.SPRepository.SpRepository().SpGetNumberOfMalesFemales(eventId);
            EventGenderMetric data = new EventGenderMetric();
            if (ds.Tables[0].Rows.Count > 0)
            {
                DataRow dr = ds.Tables[0].Rows[0];
                data.EventID = Convert.ToInt32(dr["EventID"].ToString());
                data.Males = Convert.ToInt32(dr["Males"].ToString());
                data.Females = Convert.ToInt32(dr["Females"].ToString());
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, data));
            }
            else
            {
                data.EventID = 0;
                data.Males = 0;
                data.Females = 0;
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, null));
            }
        }
        #endregion

        #region "Dashboard Summary Graphs ==> Not to be Changed"
        [HttpGet]
        [Route("api/TicketingAPI/GetTickeySummaryDetailsByEventId")]
        public HttpResponseMessage GetTickeySummaryDetailsByEventId(int eventId)
        {
            Models.EventReponse tmpResponse = new EventReponse();
            Musika.Models.DashboardSummary summary = new DashboardSummary();
            GenericRepository<TicketingEventsNew> _TicketingEvents = new GenericRepository<TicketingEventsNew>(_unitOfWork);

            int TotalTickets = 0;
            int TotalSales = 0;
            int Attendees = 0;

            var _list = (from A in _TicketingEvents.Repository.GetAll()
                         select new
                         {
                             A.EventID,
                             A.EventTitle,
                             A.StartDate,
                             A.StartTime,
                             A.EndDate,
                             A.EndTime
                         }
                         ).Where(p => p.EventID == eventId).ToList();

            if (_list.Count > 0)
            {
                summary.EventId = Convert.ToInt32(_list[0].EventID);
                summary.EventName = Convert.ToString(_list[0].EventTitle);
                summary.StartDate = Convert.ToDateTime(_list[0].StartDate);
                summary.StartTime = Convert.ToString(_list[0].StartTime);
                summary.EndDate = Convert.ToDateTime(_list[0].EndDate);
                summary.EndTime = Convert.ToString(_list[0].EndTime);

                List<EventAttendeeCounts> lst = new List<EventAttendeeCounts>();

                DataSet ds = new DataSet();
                ds = new SpRepository().SpGetTicketSummaryStats(eventId);

                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    DataRow dr = ds.Tables[0].Rows[i];
                    EventAttendeeCounts counts = new EventAttendeeCounts();
                    counts.Category = dr["Category"].ToString();
                    counts.TotalTickets = Convert.ToInt32(dr["Total"].ToString());
                    counts.Sold = Convert.ToInt32(dr["Sold"].ToString());
                    counts.Attendees = Convert.ToInt32(dr["Attended"].ToString());
                    counts.UnSold = Convert.ToInt32(dr["Unsold"].ToString());
                    TotalTickets += counts.TotalTickets;
                    TotalSales += counts.Sold;
                    Attendees += counts.Attendees;
                    lst.Add(counts);
                }
                summary.TotalSales = TotalSales;
                summary.TotalTickets = TotalTickets;
                summary.Attendees = Attendees;

                summary.lstCounts = lst;

                List<EventAttendeeCounts> lst2 = new List<EventAttendeeCounts>();
                ds = new SpRepository().SpGetTicketSummaryStatsByGender(eventId);

                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    DataRow dr = ds.Tables[0].Rows[i];
                    EventAttendeeCounts counts = new EventAttendeeCounts();
                    counts.Category = dr["Gender"].ToString();
                    counts.TotalTickets = Convert.ToInt32(dr["Total"].ToString());
                    counts.Sold = Convert.ToInt32(dr["Sold"].ToString());
                    counts.Attendees = Convert.ToInt32(dr["Attended"].ToString());
                    counts.UnSold = Convert.ToInt32(dr["Unsold"].ToString());
                    TotalTickets += counts.TotalTickets;
                    TotalSales += counts.Sold;
                    Attendees += counts.Attendees;
                    lst2.Add(counts);
                }
                summary.lstGenderAttendeeList = lst2;
            }

            if (_list.Count > 0)
            {
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, summary));
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, null));
            }
        }
        #endregion



        [Route("api/TicketingAPI/downloadEventCSVFile")]
        [HttpGet]
        public string downloadEventCSVFile(string Eventid)
        {
            string fileName = "";


            Models.TourDate _TourDate = null;

            //_unitOfWork.StartTransaction();
            GenericRepository<TourDate> _TourDateRepo = new GenericRepository<TourDate>(_unitOfWork);


            Int64 _EventID = Convert.ToInt64(Eventid);
            _TourDate = _TourDateRepo.Repository.Get(p => p.TicketingEventID == _EventID);

            ExportCsv _ExportCsv = new ExportCsv();
            Models.API.View.ViewEventDetail _ViewEventDetail = new Models.API.View.ViewEventDetail();


            GenericRepository<Venue> _VenueRepo = new GenericRepository<Venue>(_unitOfWork);
            GenericRepository<Artists> _ArtistsRepo = new GenericRepository<Artists>(_unitOfWork);
            GenericRepository<UserTourDate> UserTourDateRepo = new GenericRepository<UserTourDate>(_unitOfWork);
            GenericRepository<UserFriends> _UserFriendsRepo = new GenericRepository<UserFriends>(_unitOfWork);
            GenericRepository<UserGoing> _UserGoingRepo = new GenericRepository<UserGoing>(_unitOfWork);
            GenericRepository<Users> _UsersRepo = new GenericRepository<Users>(_unitOfWork);
            GenericRepository<UserTourDate> _UserTourDateRepo = new GenericRepository<UserTourDate>(_unitOfWork);
            GenericRepository<TicketingUsers> _TicketingUserRepo = new GenericRepository<TicketingUsers>(_unitOfWork);

            GenericRepository<TicketingEventTickets> _TicketingRepo = new GenericRepository<TicketingEventTickets>(_unitOfWork);
            GenericRepository<TicketingEventTicketsSummary> _TicketingEventSummary = new GenericRepository<TicketingEventTicketsSummary>(_unitOfWork);
            GenericRepository<TicketingEventsNew> _TicketingEventNewRepo = new GenericRepository<TicketingEventsNew>(_unitOfWork);

            Models.EventReponse tmpResponse = new EventReponse();
            Musika.Models.DashboardSummary DashboardSummary = new DashboardSummary();
            //GenericRepository<TicketingEventsNew> _TicketingEvents = new GenericRepository<TicketingEventsNew>(_unitOfWork);

            int TotalTickets = 0;
            int TotalSales = 0;
            int Attendees = 0;

            var _list = (from A in _TicketingEventNewRepo.Repository.GetAll()
                         select new
                         {
                             A.EventID,
                             A.EventTitle,
                             A.StartDate,
                             A.StartTime,
                             A.EndDate,
                             A.EndTime
                         }
                         ).Where(p => p.EventID == Convert.ToInt32(Eventid)).ToList();

            if (_list.Count > 0)
            {
                DashboardSummary.EventId = Convert.ToInt32(_list[0].EventID);
                DashboardSummary.EventName = Convert.ToString(_list[0].EventTitle);
                DashboardSummary.StartDate = Convert.ToDateTime(_list[0].StartDate);
                DashboardSummary.StartTime = Convert.ToString(_list[0].StartTime);
                DashboardSummary.EndDate = Convert.ToDateTime(_list[0].EndDate);
                DashboardSummary.EndTime = Convert.ToString(_list[0].EndTime);

                List<EventAttendeeCounts> lst = new List<EventAttendeeCounts>();

                DataSet ds = new DataSet();
                ds = new SpRepository().SpGetTicketSummaryStats(Convert.ToInt32(Eventid));

                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    DataRow dr = ds.Tables[0].Rows[i];
                    EventAttendeeCounts counts = new EventAttendeeCounts();
                    counts.Category = dr["Category"].ToString();
                    counts.TotalTickets = Convert.ToInt32(dr["Total"].ToString());
                    counts.Sold = Convert.ToInt32(dr["Sold"].ToString());
                    counts.Attendees = Convert.ToInt32(dr["Attended"].ToString());
                    counts.UnSold = Convert.ToInt32(dr["Unsold"].ToString());
                    TotalTickets += counts.TotalTickets;
                    TotalSales += counts.Sold;
                    Attendees += counts.Attendees;
                    lst.Add(counts);
                }
                DashboardSummary.TotalSales = TotalSales;
                DashboardSummary.TotalTickets = TotalTickets;
                DashboardSummary.Attendees = Attendees;

                DashboardSummary.lstCounts = lst;

                List<EventAttendeeCounts> lst2 = new List<EventAttendeeCounts>();
                ds = new SpRepository().SpGetTicketSummaryStatsByGender(Convert.ToInt32(Eventid));

                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    DataRow dr = ds.Tables[0].Rows[i];
                    EventAttendeeCounts counts = new EventAttendeeCounts();
                    counts.Category = dr["Gender"].ToString();
                    counts.TotalTickets = Convert.ToInt32(dr["Total"].ToString());
                    counts.Sold = Convert.ToInt32(dr["Sold"].ToString());
                    counts.Attendees = Convert.ToInt32(dr["Attended"].ToString());
                    counts.UnSold = Convert.ToInt32(dr["Unsold"].ToString());
                    TotalTickets += counts.TotalTickets;
                    TotalSales += counts.Sold;
                    Attendees += counts.Attendees;
                    lst2.Add(counts);
                }
                DashboardSummary.lstGenderAttendeeList = lst2;
            }


            Models.UserTourDate _UserTourDate = null;
            Models.Artists _Artist = null;

            _Artist = _ArtistsRepo.Repository.Get(p => p.ArtistID == _TourDate.ArtistID);

            _ViewEventDetail.TourID = _TourDate.TourDateID;
            _ViewEventDetail.ArtistID = _Artist.ArtistID;
            _ExportCsv.ArtistName = _Artist.ArtistName;


            _ExportCsv.ImageURL = _Artist.ImageURL ?? "";
            _ExportCsv.BannerImage_URL = _Artist.ImageURL ?? "";

            _ViewEventDetail.OnTour = _Artist.OnTour;
            _ExportCsv.Gender = _Artist.Gender ?? "";
            _ExportCsv.Main_Genre = _Artist.Main_Genre ?? "";
            _ExportCsv.Decade = _Artist.Decade ?? "";
            _ExportCsv.Alternate_Names = _Artist.Alternate_Names ?? "";
            _ExportCsv.CreatedDate = _Artist.CreatedDate;
            _ExportCsv.ModifiedDate = _Artist.ModifiedDate;
            _ViewEventDetail.RecordStatus = _Artist.RecordStatus;
            _ViewEventDetail.Instagram_Tag = _Artist.Instagram_Tag != null && _Artist.Instagram_Tag != "" ? _Artist.Instagram_Tag : _Artist.ArtistName;
            try
            {
                _ExportCsv.Date_Local = _TourDate.Datetime_Local.ToString().Substring(0, 10).Trim();
                _ExportCsv.Time_Local = _TourDate.Datetime_Local.ToString().Substring(10).Trim();
            }
            catch (Exception) { }


            if (!String.IsNullOrEmpty(_TourDate.TicketURL))
            {
                _ExportCsv.TicketURL = _TourDate.TicketURL;
            }
            else
            {
                _ExportCsv.TicketURL = "http://appserver.musikaapp.com/TicketEventCheckout.aspx";
            }


            _ExportCsv.Event_Name = _TourDate.EventName;


            Models.UserGoing _UserGoing = null;

            Models.Venue _Venue = null;
            _Venue = _VenueRepo.Repository.Get(p => p.VenueID == _TourDate.VenueID);


            //who else is going
            var _lst = (from A1 in _UserFriendsRepo.Repository.GetAll(p => p.Matched_UserID != null)
                        join A in _UserGoingRepo.Repository.GetAll(p => p.TourDateID == _TourDate.TourDateID && p.RecordStatus == EUserGoing.Going.ToString()) on A1.Matched_UserID equals A.UserID
                        join B in _UsersRepo.Repository.GetAll() on A.UserID equals B.UserID
                        select new ExportCsv.ViewEventUsers
                        {
                            //ThumbnailURL = B.ThumbnailURL ?? "",
                            UserID = A.UserID.Value,
                            GoingUserName = B.UserName.ToString(),
                            Email = B.Email ?? "",
                            CreatedDate = Convert.ToDateTime(A.CreatedDate).ToString("d")
                        }).DistinctBy(y => y.UserID).OrderByDescending(p => p.CreatedDate).ToList();

            //who else is going
            _ExportCsv.UsersGoing = _lst.ToList();
            // _ViewEventDetail.UsersGoing = lstViewEventUsers;

            _ExportCsv.NoOfUserGoing = _lst.Count();

            int? eventID;
            List<TourDate> lstSummary = new List<TourDate>();
            //eventID = _TourDateRepo.Repository.GetAll().Where(t => t.TourDateID == _TourDate.TourDateID).FirstOrDefault().TicketingEventID;
            eventID = Convert.ToInt32(Eventid);
            if (!String.IsNullOrEmpty(Eventid.ToString()))
            {
                lstSummary = _TourDateRepo.Repository.GetAll().Where(p => p.TicketingEventID == eventID).ToList();
            }

            List<ExportCsv.Ticket> lstTicket = new List<ExportCsv.Ticket>();
            if (lstSummary.Count > 0)
            {
                for (int i = 0; i < lstSummary.Count; i++)
                {
                    ExportCsv.Ticket tickets = new ExportCsv.Ticket();
                    //tickets.EventId = lstSummary[i].TicketingEventID ?? default(int);

                    TicketingEventTicketsSummary summary = new TicketingEventTicketsSummary();
                    summary = _TicketingEventSummary.Repository.GetAll().Where(t => t.EventID == eventID).ToList().FirstOrDefault();

                    if (summary != null)
                    {
                        tickets.CountryId = summary.CountryId;
                        tickets.Currency = summary.Currency;
                        tickets.RefundPolicy = summary.RefundPolicy;
                        //tickets.EventId = summary.EventID;
                    }
                    else
                    {
                        tickets.CountryId = 1;
                        tickets.Currency = "$";
                        tickets.RefundPolicy = "Yes";
                        //tickets.EventId = tickets.EventId = eventID ?? default(int);
                    }
                    List<TicketingEventTicketsSummary> lstTicketData = new List<TicketingEventTicketsSummary>();
                    List<ExportCsv.TicketData> lstData = new List<ExportCsv.TicketData>();
                    int eventid = eventID ?? default(int);
                    DataSet ds1 = new DataSet();
                    ds1 = new SpRepository().GetTicketBalanceSummary(eventid);
                    if (ds1.Tables[0].Rows.Count > 0)
                    {
                        for (int j = 0; j < ds1.Tables[0].Rows.Count; j++)
                        {
                            ExportCsv.TicketData data = new ExportCsv.TicketData();
                            DataRow dr = ds1.Tables[0].Rows[j];

                            data.EventId = Convert.ToInt32(dr["EventId"].ToString());
                            data.TicketType = Convert.ToString(dr["TicketType"].ToString());
                            data.TicketCategory = Convert.ToString(dr["TicketCategory"].ToString());
                            data.Price = Convert.ToDecimal(dr["Price"].ToString());
                            data.Quantity = Convert.ToInt32(dr["Total"].ToString());
                            data.BalanceTickets = Convert.ToInt32(dr["Balance"].ToString());
                            data.PackageStartDate = Convert.ToString(dr["PackageStartDate"].ToString());
                            data.PackageEndDate = Convert.ToString(dr["PackageEndDate"].ToString());
                            lstData.Add(data);

                        }
                    }



                    tickets.lstTicketData = lstData;
                    lstTicket.Add(tickets);
                }
            }

            TicketingEventsNew ticketingNew = _TicketingEventNewRepo.Repository.GetAll().Where(p => p.EventID == eventID).FirstOrDefault();


            try
            {
                _ExportCsv.Date_Local = Convert.ToDateTime(ticketingNew.StartDate).ToString("d");
                _ExportCsv.Time_Local = Convert.ToDateTime(ticketingNew.StartTime).ToString("t");
            }
            catch (Exception)
            { }



            if (ticketingNew != null)
            {
                _ExportCsv.Event_Description = Regex.Replace(ticketingNew.EventDescription, "<.*?>", String.Empty).Replace("\n", "").Replace("\t", "");
                _ExportCsv.Event_Name = Regex.Replace(_TourDate.EventName, "<.*?>", String.Empty).Replace("\n", "").Replace("\t", "");
                ticketingNew.OrganizerName = _TicketingUserRepo.Repository.GetById((int)ticketingNew.CreatedBy).UserName;
                _ExportCsv.Organizer_Name = ticketingNew.OrganizerName;
                _ExportCsv.Organizer_Description = Regex.Replace(ticketingNew.OrganizerDescription, "<.*?>", String.Empty).Replace("\n", "").Replace("\t", "");
            }
            else
            {
                _ExportCsv.Organizer_Name = "Musika";
                _ExportCsv.Organizer_Description = "Musika Event Organizer";

            }
            string csv = string.Empty;
            _ExportCsv.lstTicket = lstTicket;

            //===========================
            DataSet dsTicketBuyUser = new DataSet();
            List<ExportCsv.TicketBuyData> lstBuyData = new List<ExportCsv.TicketBuyData>();
            dsTicketBuyUser = new SpRepository().GetTicketBuyUser(Convert.ToInt32(Eventid));
            if (dsTicketBuyUser.Tables[0].Rows.Count > 0)
            {
                for (int j = 0; j < dsTicketBuyUser.Tables[0].Rows.Count; j++)
                {
                    ExportCsv.TicketBuyData data = new ExportCsv.TicketBuyData();
                    DataRow dr = dsTicketBuyUser.Tables[0].Rows[j];

                    //data.EventId = Convert.ToInt32(dr["EventId"].ToString());
                    data.UserName = Convert.ToString(dr["UserName"].ToString());
                    data.Email = Convert.ToString(dr["Email"].ToString());
                    data.Gender = Convert.ToString(dr["Gender"].ToString());
                    //data.Mode = Convert.ToString(dr["Mode"].ToString());
                    data.TicketType = Convert.ToString(dr["TicketType"].ToString());
                    data.Country = Convert.ToString(dr["Country"].ToString());
                    data.TicketCategory = Convert.ToString(dr["TicketCategory"].ToString());
                    lstBuyData.Add(data);

                }
            }
            #region Users export csv
            // GenericRepository<Users> _UsersRepo = new GenericRepository<Users>(_unitOfWork);
            // var list = _UsersRepo.Repository.GetAll().ToList();

            string sitePath = ConfigurationManager.AppSettings["SiteImgPath"] + "\\CSVFiles\\";
            if (!Directory.Exists(sitePath))
            {
                Directory.CreateDirectory(sitePath);
            }
            fileName = "Events_" + _ExportCsv.Event_Name + ".csv";
            string csvFilePath = sitePath + fileName;

            using (CsvFileWriter writer = new CsvFileWriter(csvFilePath))
            {
                ReadWriteCSVFile row = new ReadWriteCSVFile();

                writer.WriteLine("Event Detail");
                writer.WriteLine();

                row.Add(String.Format(" {0}", "Event_Name"));
                row.Add(String.Format(" {0}", "Event_Description"));
                row.Add(String.Format(" {0}", "ArtistName"));

                row.Add(String.Format(" {0}", "BannerImage_URL"));
                row.Add(String.Format(" {0}", "CreatedDate"));
                row.Add(String.Format(" {0}", "EventDate"));
                row.Add(String.Format(" {0}", "Decade"));
                row.Add(String.Format(" {0}", "ImageURL"));
                row.Add(String.Format(" {0}", "Main_Genre"));
                row.Add(String.Format(" {0}", "ModifiedDate"));

                row.Add(String.Format(" {0}", "NoOfUserGoing"));
                row.Add(String.Format(" {0}", "Organizer_Description"));
                row.Add(String.Format(" {0}", "Organizer_Name"));
                row.Add(String.Format(" {0}", "TicketURL"));
                row.Add(String.Format(" {0}", "About"));
                row.Add(String.Format(" {0}", "VenueCity"));
                row.Add(String.Format(" {0}", "VenueCountry"));
                row.Add(String.Format(" {0}", "VenueName"));
                row.Add(String.Format(" {0}", "VenueState"));

                row.Add(String.Format(" {0}", "Alternate_Names"));
                row.Add(String.Format(" {0}", "Time_Local"));

                writer.WriteRow(row);

                row = new ReadWriteCSVFile();
                row.Add(String.Format(" {0}", _ExportCsv.Event_Name));
                row.Add(String.Format(" {0}", _ExportCsv.Event_Description));
                row.Add(String.Format(" {0}", _ExportCsv.ArtistName));

                row.Add(String.Format(" {0}", _ExportCsv.BannerImage_URL));
                row.Add(String.Format(" {0}", _ExportCsv.CreatedDate));
                row.Add(String.Format(" {0}", _ExportCsv.Date_Local));
                row.Add(String.Format(" {0}", _ExportCsv.Decade));


                row.Add(String.Format(" {0}", _ExportCsv.ImageURL));
                row.Add(String.Format(" {0}", _ExportCsv.Main_Genre));
                row.Add(String.Format(" {0}", _ExportCsv.ModifiedDate));
                row.Add(String.Format(" {0}", _ExportCsv.NoOfUserGoing));
                row.Add(String.Format(" {0}", _ExportCsv.Organizer_Description));
                row.Add(String.Format(" {0}", _ExportCsv.Organizer_Name));
                row.Add(String.Format(" {0}", _ExportCsv.TicketURL));
                row.Add(String.Format(" {0}", _ExportCsv.About));
                row.Add(String.Format(" {0}", _ExportCsv.VenueCity));
                row.Add(String.Format(" {0}", _ExportCsv.VenueCountry));
                row.Add(String.Format(" {0}", _ExportCsv.VenueName));
                row.Add(String.Format(" {0}", _ExportCsv.VenueState));
                row.Add(String.Format(" {0}", _ExportCsv.Alternate_Names));
                row.Add(String.Format(" {0}", _ExportCsv.Time_Local));


                writer.WriteRow(row);
                if (_ExportCsv.lstTicket[0].lstTicketData.Count > 0)
                {
                    writer.WriteLine();
                    writer.WriteLine();
                    writer.WriteLine("Event Tickets Detail");
                    writer.WriteLine();

                    row = new ReadWriteCSVFile();
                    row.Add(String.Format(" {0}", "EventId"));
                    row.Add(String.Format(" {0}", "TicketCategory"));
                    row.Add(String.Format(" {0}", "TicketType"));
                    row.Add(String.Format(" {0}", "Price"));
                    row.Add(String.Format(" {0}", "Quantity"));
                    row.Add(String.Format(" {0}", "BalanceTickets"));
                    row.Add(String.Format(" {0}", "PackageStartDate"));
                    row.Add(String.Format(" {0}", "PackageEndDate"));
                    writer.WriteRow(row);
                    foreach (var rows in _ExportCsv.lstTicket[0].lstTicketData)
                    {
                        row = new ReadWriteCSVFile();
                        row.Add(String.Format(" {0}", rows.EventId));
                        row.Add(String.Format(" {0}", rows.TicketCategory));
                        row.Add(String.Format(" {0}", rows.TicketType));
                        row.Add(String.Format(" {0}", rows.Price));
                        row.Add(String.Format(" {0}", rows.Quantity));
                        row.Add(String.Format(" {0}", rows.BalanceTickets));
                        row.Add(String.Format(" {0}", rows.PackageStartDate));
                        row.Add(String.Format(" {0}", rows.PackageEndDate));
                        writer.WriteRow(row);
                    }
                }

                if (_ExportCsv.UsersGoing.Count > 0)
                {
                    writer.WriteLine();
                    writer.WriteLine();
                    writer.WriteLine("Going Users Detail");
                    writer.WriteLine();

                    row = new ReadWriteCSVFile();
                    row.Add(String.Format(" {0}", "UserID"));
                    row.Add(String.Format(" {0}", "GoingUserName"));
                    row.Add(String.Format(" {0}", "Email"));
                    row.Add(String.Format(" {0}", "CreatedDate"));
                    writer.WriteRow(row);
                    foreach (var rows in _ExportCsv.UsersGoing)
                    {
                        row = new ReadWriteCSVFile();
                        row.Add(String.Format(" {0}", rows.UserID));
                        row.Add(String.Format(" {0}", rows.GoingUserName));
                        row.Add(String.Format(" {0}", rows.Email));
                        row.Add(String.Format(" {0}", rows.CreatedDate));
                        writer.WriteRow(row);
                    }
                }


                if (DashboardSummary.lstCounts.Count > 0)
                {
                    writer.WriteLine();
                    writer.WriteLine();
                    writer.WriteLine("Sold/Unsold Ticket Detail");
                    writer.WriteLine();

                    row = new ReadWriteCSVFile();
                    row.Add(String.Format(" {0}", "Category"));
                    row.Add(String.Format(" {0}", "TotalTickets"));
                    row.Add(String.Format(" {0}", "Sold"));
                    row.Add(String.Format(" {0}", "Attendees"));
                    row.Add(String.Format(" {0}", "UnSold"));

                    writer.WriteRow(row);
                    foreach (var rows in DashboardSummary.lstCounts)
                    {
                        row = new ReadWriteCSVFile();
                        row.Add(String.Format(" {0}", rows.Category));
                        row.Add(String.Format(" {0}", rows.TotalTickets));
                        row.Add(String.Format(" {0}", rows.Sold));
                        row.Add(String.Format(" {0}", rows.Attendees));
                        row.Add(String.Format(" {0}", rows.UnSold));

                        writer.WriteRow(row);
                    }
                }

                if (lstBuyData.Count > 0)
                {
                    writer.WriteLine();
                    writer.WriteLine();
                    writer.WriteLine("User Ticket Buy Detail");
                    writer.WriteLine();

                    row = new ReadWriteCSVFile();
                    row.Add(String.Format(" {0}", "UserName"));
                    row.Add(String.Format(" {0}", "Gender"));
                    row.Add(String.Format(" {0}", "Country"));
                    row.Add(String.Format(" {0}", "Email"));
                    row.Add(String.Format(" {0}", "TicketCategory"));
                    row.Add(String.Format(" {0}", "TicketType"));

                    writer.WriteRow(row);
                    foreach (var rows in lstBuyData)
                    {
                        row = new ReadWriteCSVFile();
                        row.Add(String.Format(" {0}", rows.UserName));
                        row.Add(String.Format(" {0}", rows.Gender));
                        row.Add(String.Format(" {0}", rows.Country));
                        row.Add(String.Format(" {0}", rows.Email));
                        row.Add(String.Format(" {0}", rows.TicketCategory));
                        row.Add(String.Format(" {0}", rows.TicketType));
                        writer.WriteRow(row);
                    }
                }
            }
            #endregion
            // }
            return ConfigurationManager.AppSettings["WebPath"].ToString() + "/Content/Upload/CSVFiles/" + fileName;
        }


        #region "Dashboard Summary Graphs ==> Not to be Changed"
        [HttpGet]
        [Route("api/TicketingAPI/GetTicketSummaryDetailsByEventId")]
        public HttpResponseMessage GetTicketSummaryDetailsByEventId(int eventId)
        {
            Models.EventReponse tmpResponse = new EventReponse();
            Musika.Models.DashboardSummaryNew summary = new DashboardSummaryNew();
            GenericRepository<TicketingEventsNew> _TicketingEvents = new GenericRepository<TicketingEventsNew>(_unitOfWork);

            var _list = (from A in _TicketingEvents.Repository.GetAll()
                         select new
                         {
                             A.EventID,
                             A.EventTitle,
                             A.StartDate,
                             A.StartTime,
                             A.EndDate,
                             A.EndTime
                         }
                         ).Where(p => p.EventID == eventId).ToList();

            if (_list.Count > 0)
            {
                summary.EventId = Convert.ToInt32(_list[0].EventID);
                summary.EventName = Convert.ToString(_list[0].EventTitle);
                summary.StartDate = Convert.ToDateTime(_list[0].StartDate);
                summary.StartTime = Convert.ToString(_list[0].StartTime);
                summary.EndDate = Convert.ToDateTime(_list[0].EndDate);
                summary.EndTime = Convert.ToString(_list[0].EndTime);

                List<EventAttendeeCounts> lst2 = new List<EventAttendeeCounts>();

                DataSet ds = new DataSet();
                ds = new Musika.Repository.SPRepository.SpRepository().SpGetTicketSummaryStats(eventId);

                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    DataRow dr = ds.Tables[0].Rows[i];
                    EventAttendeeCounts counts = new EventAttendeeCounts();
                    counts.Category = dr["Category"].ToString();
                    counts.TotalTickets = Convert.ToInt32(dr["Total"].ToString());
                    counts.Sold = Convert.ToInt32(dr["Sold"].ToString());
                    counts.Attendees = Convert.ToInt32(dr["Attended"].ToString());
                    counts.UnSold = Convert.ToInt32(dr["Unsold"].ToString());
                    lst2.Add(counts);
                }
                summary.lstCounts = lst2;

                List<EventAttendeCountsByGender> lst = new List<EventAttendeCountsByGender>();

                DataSet ds1 = new DataSet();
                ds1 = new Musika.Repository.SPRepository.SpRepository().SpGetTicketSummaryStatsByGender(eventId);

                for (int i = 0; i < ds1.Tables[0].Rows.Count; i++)
                {
                    DataRow dr = ds1.Tables[0].Rows[i];
                    EventAttendeCountsByGender counts = new EventAttendeCountsByGender();
                    //counts.Category = dr[1].ToString();
                    counts.Category = dr["Category"].ToString();
                    counts.TotalTickets = Convert.ToInt32(dr["Total"].ToString());
                    counts.Sold = Convert.ToInt32(dr["Sold"].ToString());
                    counts.Attendees = Convert.ToInt32(dr["Attended"].ToString());
                    counts.UnSold = Convert.ToInt32(dr["Unsold"].ToString());
                    lst.Add(counts);
                }
                summary.lstGenderAttendeeList = lst;

                // Gender Based Attendee Counts
                List<EventAttendeCountsByGender> lstGenderCount = new List<EventAttendeCountsByGender>();

            }

            if (_list.Count > 0)
            {
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, summary));
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, null));
            }
        }
        #endregion

        #region "Get Ticket Summary by Age Group"
        [HttpGet]
        [Route("api/TicketingAPI/GetTicketSummaryByAgeGroupEventId")]
        public HttpResponseMessage GetTicketSummaryByAgeGroupEventId(int eventId)
        {
            Models.EventReponse tmpResponse = new EventReponse();
            Musika.Models.DashboardSummaryByAgeGroup summary = new DashboardSummaryByAgeGroup();
            GenericRepository<TicketingEventsNew> _TicketingEvents = new GenericRepository<TicketingEventsNew>(_unitOfWork);

            var _list = (from A in _TicketingEvents.Repository.GetAll()
                         select new
                         {
                             A.EventID,
                             A.EventTitle,
                             A.StartDate,
                             A.StartTime,
                             A.EndDate,
                             A.EndTime
                         }
                         ).Where(p => p.EventID == eventId).ToList();

            if (_list.Count > 0)
            {
                summary.EventId = Convert.ToInt32(_list[0].EventID);
                summary.EventName = Convert.ToString(_list[0].EventTitle);
                summary.StartDate = Convert.ToDateTime(_list[0].StartDate);
                summary.StartTime = Convert.ToString(_list[0].StartTime);
                summary.EndDate = Convert.ToDateTime(_list[0].EndDate);
                summary.EndTime = Convert.ToString(_list[0].EndTime);

                List<EventAttendeeCountsByAgeGroup> lst = new List<EventAttendeeCountsByAgeGroup>();

                DataSet ds = new DataSet();
                ds = new SpRepository().SpGetTicketSummaryByAgeGroupStats(eventId);

                int sold = 0;
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    DataRow dr = ds.Tables[0].Rows[i];
                    EventAttendeeCountsByAgeGroup counts = new EventAttendeeCountsByAgeGroup();
                    counts.EventID = summary.EventId;
                    counts.Age25 = Convert.ToInt32(dr["Under25"].ToString());
                    counts.Age50 = Convert.ToInt32(dr["Under2550"].ToString());
                    counts.Age75 = Convert.ToInt32(dr["Under5075"].ToString());
                    counts.Age100 = Convert.ToInt32(dr["Under75100"].ToString());
                    counts.Age125 = Convert.ToInt32(dr["above100"].ToString());

                    sold += counts.Age25 + counts.Age50 + counts.Age75 + counts.Age100 + counts.Age125;

                    summary.TotalSales = Convert.ToInt32(dr[2].ToString());
                    summary.TotalTickets = Convert.ToInt32(dr[1].ToString());
                    summary.Attendees = Convert.ToInt32(dr[3].ToString());

                    lst.Add(counts);
                }
                summary.lstCounts = lst;
            }

            if (_list.Count > 0)
            {
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, summary));
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, null));
            }
        }
        #endregion

        #region "Edit Staff Member"
        [HttpPost]
        [Route("api/TicketingAPI/UpdateStaffMember")]
        public HttpResponseMessage UpdateStaffMember([FromBody] TicketingAppUsers staff)
        {
            int id = Convert.ToInt32(staff.UserID);
            bool res;

            TicketingUsers user = new TicketingUsers();
            Models.EventReponse tmpResponse = new EventReponse();
            GenericRepository<TicketingUsers> _UsersRepo = new GenericRepository<TicketingUsers>(_unitOfWork);

            try
            {
                var _list = (from A1 in _UsersRepo.Repository.GetAll(p => p.UserID == staff.UserID)
                             select new TicketingUsers
                             {
                                 UserID = A1.UserID,
                                 UserName = A1.UserName,
                                 Email = A1.Email,
                                 Password = A1.Password,
                                 Addres = A1.Addres,
                                 City = A1.City,
                                 State = A1.State,
                                 Country = A1.Country,
                                 PostalCode = A1.PostalCode,
                                 PhoneNumber = A1.PhoneNumber
                             }).ToList();

                if (_list.Count > 0)
                {
                    res = new Musika.Repository.SPRepository.SpRepository().SpUpdateTicketingStaff(Convert.ToInt32(_list[0].UserID), staff.Email, AesCryptography.Encrypt(staff.Password), staff.Addres, staff.City, staff.State, staff.Country, staff.PostalCode, staff.PhoneNumber, staff.UserName);
                    if (res == true)
                    {
                        tmpResponse.ResponseId = 200;
                        tmpResponse.ReturnMessage = "Success";
                        return Request.CreateResponse(HttpStatusCode.OK, tmpResponse);
                    }
                    else
                    {
                        tmpResponse.ResponseId = 400;
                        tmpResponse.ReturnMessage = "Failure";
                        return Request.CreateResponse(HttpStatusCode.OK, tmpResponse);
                    }
                }
                return Request.CreateResponse(HttpStatusCode.OK, tmpResponse);
            }
            catch (Exception)
            {
                tmpResponse.ResponseId = 400;
                tmpResponse.ReturnMessage = "Failure";
                return Request.CreateResponse(HttpStatusCode.OK, tmpResponse);
            }
        }
        #endregion

        #region "Final Delete Staff Member"
        [HttpGet]
        [Route("api/TicketingAPI/DeleteStaffMember")]
        public HttpResponseMessage DeleteStaffMember(string userId)
        {
            string result = string.Empty;
            Models.EventReponse tmpResponse = new EventReponse();
            Models.TicketingUsers ticketingEventEntity = new Models.TicketingUsers();
            List<Models.TicketingEventsNew> ticketingEventNewEntity = new List<Models.TicketingEventsNew>();

            List<Models.TicketingUsers> model = new List<TicketingUsers>();
            GenericRepository<TicketingUsers> _ticketingEntity = new GenericRepository<TicketingUsers>(_unitOfWork);
            GenericRepository<TicketingEventsNew> _ticketingEventNew = new GenericRepository<TicketingEventsNew>(_unitOfWork);
            try
            {
                Models.TicketingEventsNew entity = new Models.TicketingEventsNew();
                int id = Convert.ToInt32(userId);
                ticketingEventEntity = _ticketingEntity.Repository.Get(p => p.UserID == id);

                int tt = Convert.ToInt32(userId) > 0 ? Convert.ToInt32(userId) : 0;

                ticketingEventNewEntity = _ticketingEventNew.Repository.GetAll().Where(p => p.StaffId == tt && p.ISDELETED == false && p.IsApproved == true).ToList();

                if (ticketingEventNewEntity.Count == 0)
                {
                    ticketingEventEntity.RecordStatus = "Deleted";
                    ticketingEventEntity.ModifiedDate = DateTime.Now;

                    _ticketingEntity.Repository.Update(ticketingEventEntity);
                    tmpResponse.ResponseId = 200;
                    tmpResponse.ReturnMessage = "Success";
                }
                else
                {
                    tmpResponse.ResponseId = 400;
                    tmpResponse.ReturnMessage = "Reference Exists";
                }
                return Request.CreateResponse(HttpStatusCode.OK, tmpResponse);
            }
            catch (Exception ee)
            {
                LogHelper.CreateLog3(ee, Request);
                _unitOfWork.RollBack();//RollBack Transaction                
                tmpResponse.ResponseId = 400;
                tmpResponse.ReturnMessage = "Error";
                return Request.CreateResponse(HttpStatusCode.OK, model);
            }
        }
        #endregion

        #region "Unused Code"
        [AllowAnonymous]
        [Route("TicketingAPI/ChangeTicketingEventStatus")]
        [HttpPost]
        public bool ChangeTicketingEventStatus(Int64 ID, string InactivePeriod)
        {
            Models.TicketingEventsNew _Events = null;
            Models.TicketingUsers _User = null;

            GenericRepository<TicketingEventsNew> _EventsRepo = new GenericRepository<TicketingEventsNew>(_unitOfWork);
            GenericRepository<TicketingUsers> _UsersRepo = new GenericRepository<TicketingUsers>(_unitOfWork);

            _Events = _EventsRepo.Repository.Get(p => p.EventID == ID);

            _User = _UsersRepo.Repository.Get(p => p.UserID == _Events.CreatedBy);

            if (_Events != null)
            {
                if (_Events.EventID > 0) // if not admin
                {
                    if (InactivePeriod.Equals("Approve"))
                    {
                        _Events.IsApproved = true;
                    }
                    else
                    {
                        _Events.IsApproved = false;
                    }
                    //_Events.RecordStatus = _Events.RecordStatus == RecordStatus.Active.ToString() ? RecordStatus.InActive.ToString() : RecordStatus.Active.ToString();

                    _EventsRepo.Repository.Update(_Events);

                    #region "Mail Functionality"
                    string html = "<p>Hi," + "</p>";
                    html += "<p>Thanks for using " + WebConfigurationManager.AppSettings["AppName"] + "! </p>";
                    html += "<p>Your Event status is changed to " + InactivePeriod + " from Administrator end." + "</p>";

                    html += "<p><br><br><strong>Thanks,<br>The " + WebConfigurationManager.AppSettings["AppName"] + " Team</strong></p>";

                    //EmailHelper.SendEmail(entity.Email, WebConfigurationManager.AppSettings["AppName"] + " : New Staff User", html);
                    //#region "Send Mail Implementation"

                    //MailMessage mail = new MailMessage();
                    //SmtpClient SmtpServer = new SmtpClient("smtp.sendgrid.net");

                    //string mailFrom = System.Configuration.ConfigurationManager.AppSettings["ADMIN_EMAIL"].ToString();
                    //mail.From = new MailAddress(mailFrom);


                    //mail.To.Add(_User.Email);
                    //mail.Subject = "Event Status";
                    //mail.Body = html;

                    //mail.IsBodyHtml = true;


                    //SmtpServer.Port = 587;      // 25;
                    //SmtpServer.Credentials = new System.Net.NetworkCredential(ConfigurationManager.AppSettings["ApiKey"], ConfigurationManager.AppSettings["ApiKeyPass"]);
                    //SmtpServer.EnableSsl = true;
                    ////SmtpServer.Send(mail);
                    //#endregion
                    #endregion
                    SendEmailHelper.SendMail(_User.Email, WebConfigurationManager.AppSettings["AppName"] + " : Event Status", html, "");

                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        #endregion

        #region "Get List Of Venues"
        [HttpGet]
        [Route("api/TicketingAPI/GetVenueList")]
        public HttpResponseMessage GetVenueList()
        {
            GenericRepository<Venue> _TicketingVenues = new GenericRepository<Venue>(_unitOfWork);
            try
            {
                var _list = (from A1 in _TicketingVenues.Repository.GetAll()
                             select new TicketingVenues
                             {
                                 VenueID = A1.VenueID,
                                 VenueName = A1.VenueName,
                                 Address = A1.Address,
                                 Extended_Address = A1.Extended_Address,
                                 VenueCity = A1.VenueCity,
                                 VenueState = A1.VenueState,
                                 Postal_Code = A1.Postal_Code
                             }).ToList();

                if (_list.Count > 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, _list));
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, null));
                }
            }
            catch (Exception ee)
            {
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, ee.Message + "\n" + ee.StackTrace));
            }
        }
        #endregion

        #region "Get Venues Details by Venue Id"
        [HttpGet]
        [Route("api/TicketingAPI/GetVenueDetailsbyId")]
        public HttpResponseMessage GetVenueDetailsbyId(int venueId)
        {
            GenericRepository<Venue> _TicketingVenues = new GenericRepository<Venue>(_unitOfWork);

            var _list = (from A1 in _TicketingVenues.Repository.GetAll().Where(p => p.VenueID == venueId)
                         select new TicketingVenues
                         {
                             VenueID = A1.VenueID,
                             VenueName = A1.VenueName,
                             Address = A1.Address,
                             Extended_Address = A1.Extended_Address,
                             VenueCity = A1.VenueCity,
                             VenueState = A1.VenueState,
                             Postal_Code = A1.Postal_Code
                         }).ToList();

            if (_list.Count > 0)
            {
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, _list));
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, null));
            }
        }
        #endregion

        #region "Get Venues Details by Venue Name"
        [HttpGet]
        [Route("api/TicketingAPI/GetVenueDetailsbyName")]
        public HttpResponseMessage GetVenueDetailsbyName(string name)
        {
            GenericRepository<Venue> _TicketingVenues = new GenericRepository<Venue>(_unitOfWork);

            var _list = (from A1 in _TicketingVenues.Repository.GetAll().Where(p => p.VenueName == name)
                         select new TicketingVenues
                         {
                             VenueID = A1.VenueID,
                             VenueName = A1.VenueName,
                             Address = A1.Address,
                             Extended_Address = A1.Extended_Address,
                             VenueCity = A1.VenueCity,
                             VenueState = A1.VenueState,
                             Postal_Code = A1.Postal_Code
                         }).ToList();

            if (_list.Count > 0)
            {
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, _list));
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, null));
            }
        }
        #endregion



        #region "Final Get Event Details by EventID"
        [AllowAnonymous]
        [Route("api/TicketingAPI/GetTicketingEventDetails")]
        [HttpGet]
        public HttpResponseMessage GetTicketingEventDetails(Int32 EventID)
        {
            DataSet ds = new DataSet();
            Models.EventReponse tmpResponse = new EventReponse();
            TicketingEventDetailsModel _list = new TicketingEventDetailsModel();
            GenericRepository<TicketingEventNewStaff> _StaffRepo = new GenericRepository<TicketingEventNewStaff>(_unitOfWork);
            GenericRepository<TicketingUsers> _TicketingUsersRepo = new GenericRepository<TicketingUsers>(_unitOfWork);
            try
            {
                ds = new Musika.Repository.SPRepository.SpRepository().SpGetTicketingEventDetails(EventID);
                List<Musika.Models.TicketingEventNewStaff> tmpTicketStaff = new List<TicketingEventNewStaff>();
                List<StaffDetail> tmpStaff = new List<StaffDetail>();
                tmpTicketStaff = _StaffRepo.Repository.GetAll().Where(p => p.EventId == EventID).ToList();
                if (tmpTicketStaff.Count > 0)
                {
                    List<Musika.Models.TicketingUsers> tmpTicketStaffUser = new List<TicketingUsers>();
                    //   tmpTicketStaffUser = _TicketingUsersRepo.Repository.GetAll().Where(p => p.UserID == tmpTicketStaff.).ToList();
                    for (int i = 0; i < tmpTicketStaff.Count; i++)
                    {
                        tmpTicketStaffUser = _TicketingUsersRepo.Repository.GetAll().Where(p => p.UserID == tmpTicketStaff[i].StaffId).ToList();
                        StaffDetail data = new StaffDetail();
                        data.UserId = tmpTicketStaffUser[0].UserID;// model.EventID;//
                        data.UserName = tmpTicketStaffUser[0].UserName;
                        tmpStaff.Add(data);
                    }
                }


                if (ds.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        DataRow dr = ds.Tables[0].Rows[i];
                        _list.EventID = Convert.ToInt32(dr["EventID"].ToString());
                        _list.EventTitle = dr["EventTitle"].ToString();
                        _list.EventLocation = dr["EventLocation"].ToString();
                        _list.VenueName = dr["VenueName"].ToString();
                        _list.Address1 = dr["Address1"].ToString();
                        _list.Address2 = dr["Address2"].ToString();
                        _list.City = dr["City"].ToString();
                        _list.State = dr["State"].ToString();
                        _list.ZipCode = dr["ZipCode"].ToString();
                        _list.StartDate = Convert.ToDateTime(dr["StartDate"].ToString());
                        _list.StartTime = dr["StartTime"].ToString();
                        _list.EndDate = Convert.ToDateTime(dr["EndDate"].ToString());
                        _list.EndTime = dr["EndTime"].ToString();
                        _list.EventImage = dr["EventImage"].ToString();
                        _list.EventDescription = dr["EventDescription"].ToString();
                        _list.OrganizerName = dr["OrganizerName"].ToString();
                        _list.OrganizerDescription = dr["OrganizerDescription"].ToString();
                        _list.TicketType = dr["TicketType"].ToString();
                        _list.EventType = dr["EventType"].ToString();
                        _list.ListingPrivacy = dr["ListingPrivacy"].ToString();
                        _list.EventTopic = dr["EventTopic"].ToString();
                        _list.ShowTicketNumbers = dr["ShowTicketNumbers"].ToString();
                        _list.CreatedBy = dr["CreatedBy"].ToString();
                        _list.CreatedOn = Convert.ToDateTime(dr["CreatedOn"].ToString());
                        _list.ArtistId = Convert.ToInt32(dr["ArtistId"].ToString());
                        _list.ArtistName = dr["ArtistName"].ToString();
                        _list.TicketType = dr["TicketType"].ToString();
                        _list.TicketPackage = dr["TicketCategory"].ToString();
                        _list.lststaff = tmpStaff;
                    }
                }
                if (_list != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, _list));
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, null));
                }
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, null));
            }
        }
        #endregion



        #region "Save Credit Card Details"
        [HttpPost]
        [Route("api/TicketingAPI/SaveCreditCardDetails")]
        public HttpResponseMessage SaveCreditCardDetails([FromBody] CreditCardDetails cardDetails)
        {
            bool res;
            Models.EventReponse tmpResponse = new EventReponse();

            GenericRepository<CreditCardDetails> _creditCardEntity = new GenericRepository<CreditCardDetails>(_unitOfWork);

            CreditCardDetails card = new CreditCardDetails();
            try
            {
                card.Auth1 = cardDetails.Auth1;
                card.Auth2 = cardDetails.Auth2;
                card.MerchantId = cardDetails.MerchantId;
                card.UserId = cardDetails.UserId;

                res = new Musika.Repository.SPRepository.SpRepository().SpAddCreditCardDetails(card);
                if (res == true)
                {
                    tmpResponse.ResponseId = 200;
                    tmpResponse.ReturnMessage = "Success";
                }
                else
                {
                    tmpResponse.ResponseId = 400;
                    tmpResponse.ReturnMessage = "Failure";
                }
            }
            catch (Exception ee)
            {
                string msg = ee.Message + "\n" + ee.StackTrace;
            }
            return Request.CreateResponse(HttpStatusCode.OK, tmpResponse);
        }
        #endregion


        #region "Save Credit Card Details"
        [HttpPost]
        [Route("api/TicketingAPI/SaveContactDetails")]
        public HttpResponseMessage SaveContactDetails([FromBody] Contact cardDetails)
        {
            bool res;
            Models.EventReponse tmpResponse = new EventReponse();

            GenericRepository<CreditCardDetails> _creditCardEntity = new GenericRepository<CreditCardDetails>(_unitOfWork);

            Contact card = new Contact();
            try
            {
                card.Name = cardDetails.Name;
                card.Address = cardDetails.Address;
                card.Email = cardDetails.Email;
                card.Message = cardDetails.Message;

                res = new Musika.Repository.SPRepository.SpRepository().SpAddContactDetails(card);
                if (res == true)
                {
                    //string html = "<p>Hi Musika Team," + "</p>";
                    //html += "<p>Message From <strong>" + cardDetails.Name.ToString() + "</strong> through " + WebConfigurationManager.AppSettings["AppName"] + "! </p>";

                    //html += "<p><br>Name : " + cardDetails.Name + "<p>";
                    //html += "<p><br>Address : " + cardDetails.Address + "<p>";
                    //html += "<p><br>Name : " + cardDetails.Name + "<p>";
                    //html += "<p><br>Email : " + cardDetails.Email + "<p>";
                    //html += "<p><br><br><strong>Thanks, The " + cardDetails.Name + " </strong></p>";
                    //SendEmailHelper.SendMail("support@MusikaApp.com",  "Message from new Contact ", html, "");
                    tmpResponse.ResponseId = 200;
                    tmpResponse.ReturnMessage = "Success";
                }
                else
                {
                    tmpResponse.ResponseId = 400;
                    tmpResponse.ReturnMessage = "Failure";
                }
            }
            catch (Exception ee)
            {
                string msg = ee.Message + "\n" + ee.StackTrace;
            }
            return Request.CreateResponse(HttpStatusCode.OK, tmpResponse);
        }
        #endregion

        #region "List Credit Card Details"
        [HttpGet]
        [Route("api/TicketingAPI/GetCreditCardList")]
        public HttpResponseMessage GetCreditCardList()
        {
            GenericRepository<CreditCardDetails> _CreditCardList = new GenericRepository<CreditCardDetails>(_unitOfWork);
            try
            {
                var _list = (from A1 in _CreditCardList.Repository.GetAll()
                             select new CreditCardDetails
                             {
                                 Id = Convert.ToInt32(A1.Id),
                                 Auth1 = A1.Auth1,
                                 Auth2 = A1.Auth2,
                                 MerchantId = A1.MerchantId
                             }).ToList();

                if (_list.Count > 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, _list));
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, null));
                }
            }
            catch (Exception ee)
            {
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, ee.Message + "\n" + ee.StackTrace));
            }
        }
        #endregion

        #region "Final Get Staff List for an particular event"        
        [HttpGet]
        [Route("api/TicketingAPI/GetStaffListOfEvent")]
        public HttpResponseMessage GetStaffListOfEvent(int eventid)
        {
            GenericRepository<TicketingUsers> _TicketingUsers = new GenericRepository<TicketingUsers>(_unitOfWork);
            GenericRepository<TicketingEventNewStaff> _staff = new GenericRepository<TicketingEventNewStaff>(_unitOfWork);

            try
            {

                List<Musika.Models.TicketingEventNewStaff> tmpTicketStaff = new List<TicketingEventNewStaff>();
                List<TicketingUsers> tmpStaff = new List<TicketingUsers>();
                tmpTicketStaff = _staff.Repository.GetAll().Where(p => p.EventId == eventid).ToList();
                if (tmpTicketStaff.Count > 0)
                {
                    List<Musika.Models.TicketingUsers> tmpTicketStaffUser = new List<TicketingUsers>();
                    for (int i = 0; i < tmpTicketStaff.Count; i++)
                    {
                        tmpTicketStaffUser = _TicketingUsers.Repository.GetAll(p => p.UserType == "Staff").Where(p => p.UserID == tmpTicketStaff[i].StaffId && p.RecordStatus != "Deleted").ToList();
                        TicketingUsers data = new TicketingUsers();
                        data.UserID = tmpTicketStaffUser[0].UserID;// model.EventID;//
                        data.UserName = tmpTicketStaffUser[0].UserName;
                        data.Email = tmpTicketStaffUser[0].Email;
                        data.PhoneNumber = tmpTicketStaffUser[0].PhoneNumber;
                        data.Country = tmpTicketStaffUser[0].Country;
                        tmpStaff.Add(data);
                    }
                }

                var _liststaff = tmpStaff;
                if (_liststaff.Count > 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, _liststaff));
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, null));
                }

            }
            catch (Exception ee)
            {
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, ee.Message + "\n" + ee.StackTrace));
            }
        }
        #endregion

        #region "Final Get Staff List for an particular event"
        [AllowAnonymous]
        [HttpGet]
        [Route("TicketingAPI/GetStaffListOfEventByEventID")]
        public List<TicketingUsers> GetStaffListOfEventByEventID(int eventid)
        {
            GenericRepository<TicketingUsers> _TicketingUsers = new GenericRepository<TicketingUsers>(_unitOfWork);
            GenericRepository<TicketingEventNewStaff> _staff = new GenericRepository<TicketingEventNewStaff>(_unitOfWork);

            List<TicketingUsers> lstTicketingUsers = new List<TicketingUsers>();

            GenericRepository<TicketingEventsNew> _TicketingEventNewRepo = new GenericRepository<TicketingEventsNew>(_unitOfWork);
            var _Event = _TicketingEventNewRepo.Repository.GetAll().Where(p => p.EventID == eventid).FirstOrDefault();

            GenericRepository<TicketingEventTicketsSummary> _TicketingSummaryRepo = new GenericRepository<TicketingEventTicketsSummary>(_unitOfWork);
            var _EventSummary = _TicketingSummaryRepo.Repository.GetAll().Where(p => p.EventID == eventid).FirstOrDefault();

            //System.Web.HttpContext.Current.Session["EventName"] = _Event.EventTitle;
            //System.Web.HttpContext.Current.Session["TotalTickets"] = _EventSummary.Quantity;

            try
            {
                var _staffId = _staff.Repository.GetAll().Where(p => p.EventId == eventid).ToList();
                var staffList = new List<TicketingStaffList>();
                if (_staffId.Count > 0)
                {
                    List<Musika.Models.TicketingEventNewStaff> tmpTicketStaff = new List<TicketingEventNewStaff>();
                    List<TicketingUsers> tmpStaff = new List<TicketingUsers>();
                    tmpTicketStaff = _staff.Repository.GetAll().Where(p => p.EventId == eventid).ToList();
                    if (tmpTicketStaff.Count > 0)
                    {
                        List<Musika.Models.TicketingUsers> tmpTicketStaffUser = new List<TicketingUsers>();
                        // tmpTicketStaffUser = _TicketingUsersRepo.Repository.GetAll().Where(p => p.UserID == tmpTicketStaff.).ToList();
                        for (int i = 0; i < tmpTicketStaff.Count; i++)
                        {
                            tmpTicketStaffUser = _TicketingUsers.Repository.GetAll(p => p.UserType == "Staff").Where(p => p.UserID == _staffId[i].StaffId && p.RecordStatus != "Deleted").ToList();
                            TicketingUsers data = new TicketingUsers();
                            data.UserID = tmpTicketStaffUser[0].UserID;
                            data.UserName = tmpTicketStaffUser[0].UserName;
                            data.Email = tmpTicketStaffUser[0].Email;
                            data.PhoneNumber = tmpTicketStaffUser[0].PhoneNumber;
                            data.Country = tmpTicketStaffUser[0].Country;
                            tmpStaff.Add(data);
                        }
                    }

                    //System.Web.HttpContext.Current.Session["SoldTickets"] = tmpStaff.Count;

                    var _liststaff = tmpStaff;

                    if (_liststaff.Count > 0)
                    {
                        //return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, _liststaff));
                        for (int i = 0; i < tmpStaff.Count; i++)
                        {
                            TicketingUsers user = new TicketingUsers();
                            user.UserID = Convert.ToInt32(tmpStaff[i].UserID);
                            user.UserName = tmpStaff[i].UserName;
                            user.Email = tmpStaff[i].Email;
                            user.PhoneNumber = tmpStaff[i].PhoneNumber;
                            user.Country = tmpStaff[i].Country;
                            lstTicketingUsers.Add(user);
                        }
                        return lstTicketingUsers;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    //return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, null));
                    return null;
                }
            }
            catch (Exception)
            {
                //return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, ee.Message + "\n" + ee.StackTrace));
                return null;
            }
        }
        #endregion

        #region "Get Card Detail by UserID"
        [HttpGet]
        [Route("api/TicketingAPI/GetCardDetailById")]
        public bool GetCardDetailById(Int32 ID)
        {
            try
            {
                GenericRepository<CreditCardDetails> _CreditCardDetailsRepo = new GenericRepository<CreditCardDetails>(_unitOfWork);
                var _UserID = _CreditCardDetailsRepo.Repository.GetAll(p => p.UserId == ID).FirstOrDefault();
                if (_UserID != null)
                {
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
            return false;
        }
        #endregion

        #region "Get Your Plan"
        [HttpGet]
        [Route("api/TicketingAPI/GetYourPlans")]
        public HttpResponseMessage GetYourPlans(Int32 UserID, Int16 Pageindex, Int16 Pagesize)
        {
            try
            {
                ViewYourPlans _ViewYourPlansDetail = new ViewYourPlans();

                GenericRepository<TourDate> _TourDateRepo = new GenericRepository<TourDate>(_unitOfWork);
                GenericRepository<Venue> _VenueRepo = new GenericRepository<Venue>(_unitOfWork);
                GenericRepository<Artists> _ArtistsRepo = new GenericRepository<Artists>(_unitOfWork);
                GenericRepository<UserGoing> _UserGoingRepo = new GenericRepository<UserGoing>(_unitOfWork);
                GenericRepository<TicketingUsers> _UsersRepo = new GenericRepository<TicketingUsers>(_unitOfWork);
                Models.TicketingUsers _Users = null;

                _Users = _UsersRepo.Repository.Get(p => p.UserID == UserID);

                if (_Users == null || _Users.RecordStatus != RecordStatus.Active.ToString())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.UserNotFound, "YourPlans"));
                }

                _ViewYourPlansDetail.UserID = _Users.UserID;
                _ViewYourPlansDetail.UserName = _Users.UserName;
                _ViewYourPlansDetail.Email = _Users.Email;
                _ViewYourPlansDetail.ThumbnailURL = _Users.ThumbnailURL ?? ConfigurationManager.AppSettings["SiteImgURL"] + "/default-user.png";
                _ViewYourPlansDetail.ImageURL = _Users.ImageURL;

                #region "To be implemented using SP"
                //Upcoming Events
                //DataSet ds = new DataSet();
                //ds = new SpRepository().GetViewYourPlansDetailByUserId(UserID);

                //List<ViewYourplanlst> lstViewYourplanlst = new List<ViewYourplanlst>();
                //if (ds.Tables[0].Rows.Count > 0)
                //{
                //    for (int i=0;i<ds.Tables[0].Rows.Count;i++)
                //    {
                //        ViewYourplanlst list = new ViewYourplanlst();
                //        DataRow dr = ds.Tables[0].Rows[i];
                //        list.TourDateID = Convert.ToInt32(dr["TourDateID"].ToString());
                //        list.ArtistID = Convert.ToInt32(dr["ArtistID"].ToString());
                //        list.ArtistName = Convert.ToString(dr["ArtistName"].ToString());
                //        list.ImageURL = Convert.ToString(dr["ImageURL"].ToString());
                //        list.BannerImage_URL = Convert.ToString(dr["BannerImage_URL"].ToString());
                //        list.Date_Local = Convert.ToDateTime(dr["Date_Local"].ToString()).ToString("d");
                //        list.Time_Local = Convert.ToDateTime(dr["Time_Local"].ToString()).ToString("t");
                //        list.VenueID = Convert.ToInt32(dr["VenueID"].ToString());
                //        list.VenueName = Convert.ToString(dr["VenueName"].ToString());
                //        list.Extended_Address = Convert.ToString(dr["Extended_Address"].ToString());
                //        list.Display_Location = Convert.ToString(dr["Display_Location"].ToString());
                //        list.Slug = Convert.ToString(dr["Slug"].ToString());
                //        list.Postal_Code = Convert.ToString(dr["Postal_Code"].ToString());
                //        list.Address = Convert.ToString(dr["Address"].ToString());
                //        list.Timezone = Convert.ToString(dr["Timezone"].ToString());
                //        list.VenueCity = Convert.ToString(dr["VenueCity"].ToString());
                //        list.VenueState = Convert.ToString(dr["VenueState"].ToString());
                //        list.VenueCountry = Convert.ToString(dr["VenueCountry"].ToString());
                //        list.VenueLat = Convert.ToDecimal(dr["VenueLat"].ToString());
                //        list.VenueLong = Convert.ToDecimal(dr["VenueLong"].ToString());
                //        lstViewYourplanlst.Add(list);
                //    }
                //}

                //_ViewYourPlansDetail.Plans = lstViewYourplanlst.OrderBy(p => p.Datetime_Local).ToPagedList(Pageindex - 1, Pagesize).ToList();
                #endregion

                _ViewYourPlansDetail.Plans = (from A in _UserGoingRepo.Repository.GetAll(p => p.UserID == UserID && p.RecordStatus == EUserGoing.Going.ToString())
                                              join B in _TourDateRepo.Repository.GetAll(p => p.Tour_Utcdate >= DateTime.UtcNow && !p.IsDeleted) on A.TourDateID equals B.TourDateID
                                              join C in _ArtistsRepo.Repository.GetAll() on B.ArtistID equals C.ArtistID
                                              join D in _VenueRepo.Repository.GetAll() on B.VenueID equals D.VenueID
                                              orderby B.Visible_Until_utc descending
                                              select new ViewYourplanlst
                                              {
                                                  TourDateID = B.TourDateID,
                                                  ArtistID = B.ArtistID,
                                                  ArtistName = C.ArtistName ?? "",
                                                  ImageURL = C.ImageURL,
                                                  BannerImage_URL = C.BannerImage_URL ?? "",
                                                  Datetime_Local = B.Datetime_Local.Value,
                                                  Date_Local = Convert.ToDateTime(B.Datetime_Local).ToString("d"),
                                                  Time_Local = Convert.ToDateTime(B.Datetime_Local).ToString("t"),
                                                  VenueID = B.VenueID,
                                                  VenueName = D.VenueName ?? "",
                                                  Extended_Address = D.Extended_Address ?? "",
                                                  Display_Location = D.Display_Location ?? "",
                                                  Slug = D.Slug ?? "",
                                                  Postal_Code = D.Postal_Code ?? "",
                                                  Address = D.Address ?? "",
                                                  Timezone = D.Timezone ?? "",
                                                  VenueCity = D.VenueCity ?? "",
                                                  VenueState = D.VenueState ?? "",
                                                  VenueCountry = D.VenueCountry ?? "",
                                                  VenueLat = D.VenueLat,
                                                  VenueLong = D.VenueLong
                                              }).OrderBy(p => p.Datetime_Local).ToPagedList(Pageindex - 1, Pagesize).ToList();

                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, _ViewYourPlansDetail, "YourPlans"));
            }
            catch (Exception ex)
            {
                LogHelper.CreateLog3(ex, Request);
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, ex.Message, "YourPlans"));
            }
        }
        #endregion


        #region "Final List of all TicketingTicketUsers"
        [AllowAnonymous]
        [HttpGet]
        [Route("api/TicketingAPI/GetAllTicketingTicketUsersOld")]
        public List<TicketingTicketUsers> GetAllTicketingTicketUsersOld(int eventId)
        {

            List<TicketingTicketUsers> lstUsers = new List<TicketingTicketUsers>();
            DataSet ds = new Musika.Repository.SPRepository.SpRepository().GetTicketingUserListForAnEvent(eventId);
            if (ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    TicketingTicketUsers temp = new TicketingTicketUsers();
                    temp.UserID = Convert.ToInt32(dr["UserID"].ToString());
                    temp.UserName = Convert.ToString(dr["UserName"].ToString());
                    temp.Address = Convert.ToString(dr["Address"].ToString());
                    temp.City = Convert.ToString(dr["City"].ToString());
                    temp.State = Convert.ToString(dr["State"].ToString());
                    temp.Country = Convert.ToString(dr["Country"].ToString());
                    temp.Email = Convert.ToString(dr["Email"].ToString());
                    temp.PhoneNumber = Convert.ToString(dr["PhoneNumber"].ToString());
                    temp.TicketEventID = Convert.ToInt32(dr["TicketEventID"].ToString());
                    lstUsers.Add(temp);
                }
            }
            return lstUsers;
        }
        #endregion

        #region "Final List of all TicketingTicketUsers - With Pagination"
        [AllowAnonymous]
        [HttpGet]
        [Route("api/TicketingAPI/GetAllTicketingTicketUsers")]
        public HttpResponseMessage GetAllTicketingTicketUsers(int eventId)
        {
            int Pageindex = 1;
            int Pagesize = 20;
            TicketingTicketUsers _listFinal = new TicketingTicketUsers();
            List<TicketingTicketUsers> lstUsers = new List<TicketingTicketUsers>();
            DataSet ds = new Musika.Repository.SPRepository.SpRepository().GetTicketingUserListForAnEvent(eventId);
            if (ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    TicketingTicketUsers temp = new TicketingTicketUsers();
                    temp.UserID = Convert.ToInt32(dr["UserID"].ToString());
                    temp.UserName = Convert.ToString(dr["UserName"].ToString());
                    temp.Address = Convert.ToString(dr["Address"].ToString());
                    temp.City = Convert.ToString(dr["City"].ToString());
                    temp.State = Convert.ToString(dr["State"].ToString());
                    temp.Country = Convert.ToString(dr["Country"].ToString());
                    temp.Email = Convert.ToString(dr["Email"].ToString());
                    temp.PhoneNumber = Convert.ToString(dr["PhoneNumber"].ToString());
                    temp.TicketEventID = Convert.ToInt32(dr["TicketEventID"].ToString());
                    temp.TicketSerialNumber = Convert.ToString(dr["TicketSerialNumber"].ToString());
                    lstUsers.Add(temp);
                }
            }

            //Pagination
            var _list2 = lstUsers.ToPagedList(Pageindex - 1, Pagesize);

            Dictionary<string, object> d = new Dictionary<string, object>();

            if (lstUsers.Count() > 0)
            {
                d.Add("Items", _list2);
                d.Add("PageCount", _list2.PageCount);
                d.Add("PageNumber", _list2.PageNumber);
                d.Add("PageSize", Pagesize);
                return Request.CreateResponse(HttpStatusCode.OK, d);
            }
            else
            {
                d.Add("Items", new List<string>());
                d.Add("PageCount", 0);
                d.Add("PageNumber", 0);
                d.Add("PageSize", 0);
                return Request.CreateResponse(HttpStatusCode.OK, d);
            }
        }
        #endregion

        #region "Coupon"
        [HttpGet]
        [Route("api/TicketingAPI/ValidateCouponCodeOld")]
        public HttpResponseMessage ValidateCouponCodeOld(string name)
        {
            GenericRepository<Coupons> _Coupons = new GenericRepository<Coupons>(_unitOfWork);
            Musika.Models.CouponsModel coupons = new CouponsModel();
            Models.EventReponse tmpResponse = new EventReponse();

            var _list = (from A1 in _Coupons.Repository.GetAll().Where(p => p.CouponCode == name)
                         select new Coupons
                         {
                             Id = A1.Id,
                             EventName = A1.EventName,
                             CouponCode = A1.CouponCode,
                             Discount = A1.Discount,
                             ExpiryDate = A1.ExpiryDate,
                             CreateOn = A1.CreateOn,
                             CreatedBy = A1.CreatedBy,
                             Status = A1.Status,
                             TicketCategory = A1.TicketCategory
                         }).ToList();

            if (_list.Count > 0)
            {
                coupons.Id = _list[0].Id;
                coupons.EventName = _list[0].EventName;
                coupons.CouponCode = _list[0].CouponCode;
                coupons.Discount = _list[0].Discount ?? 0;
                coupons.ExpiryDate = _list[0].ExpiryDate ?? Convert.ToDateTime("1/1/1900");
                coupons.CreateOn = _list[0].CreateOn ?? Convert.ToDateTime("1/1/1900");
                coupons.CreatedBy = _list[0].CreatedBy ?? -1;
                coupons.Status = _list[0].Status.Value == false ? 0 : 1;
                coupons.TicketCategory = _list[0].TicketCategory;

                if (coupons.ExpiryDate < DateTime.Now)
                {
                    tmpResponse.ResponseId = 400;
                    tmpResponse.ReturnMessage = "Coupon is Expired";
                }
                else if (coupons.Status == 1)
                {
                    tmpResponse.ResponseId = 400;
                    tmpResponse.ReturnMessage = "Coupon is Already Used";
                }
                else
                {
                    new SpRepository().UpdateCoupon(coupons.CouponCode);
                    tmpResponse.ResponseId = 200;
                    tmpResponse.ReturnMessage = "Success";
                }
                coupons.MessageResponse = tmpResponse;

                return Request.CreateResponse(HttpStatusCode.OK, coupons);
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, null));
            }
        }

        [HttpGet]
        [Route("api/TicketingAPI/ValidateCouponCode")]
        public HttpResponseMessage ValidateCouponCode(string name, string category)
        {
            GenericRepository<Coupons> _Coupons = new GenericRepository<Coupons>(_unitOfWork);
            Musika.Models.CouponsModel coupons = new CouponsModel();
            Models.EventReponse tmpResponse = new EventReponse();

            var _list = (from A1 in _Coupons.Repository.GetAll()
                         .Where(p => p.CouponCode == name && p.TicketCategory == category)
                         select new Coupons
                         {
                             Id = A1.Id,
                             EventName = A1.EventName,
                             CouponCode = A1.CouponCode,
                             Discount = A1.Discount,
                             ExpiryDate = A1.ExpiryDate,
                             CreateOn = A1.CreateOn,
                             CreatedBy = A1.CreatedBy,
                             Status = A1.Status,
                             TicketCategory = A1.TicketCategory
                         }).ToList();

            if (_list.Count > 0)
            {
                coupons.Id = _list[0].Id;
                coupons.EventName = _list[0].EventName;
                coupons.CouponCode = _list[0].CouponCode;
                coupons.Discount = _list[0].Discount ?? 0;
                coupons.ExpiryDate = _list[0].ExpiryDate ?? Convert.ToDateTime("1/1/1900");
                coupons.CreateOn = _list[0].CreateOn ?? Convert.ToDateTime("1/1/1900");
                coupons.CreatedBy = _list[0].CreatedBy ?? -1;
                coupons.Status = _list[0].Status.Value == false ? 0 : 1;
                coupons.TicketCategory = _list[0].TicketCategory;

                if (coupons.ExpiryDate < DateTime.Now)
                {
                    tmpResponse.ResponseId = 400;
                    tmpResponse.ReturnMessage = "Coupon is Expired";
                }
                else if (coupons.Status == 1)
                {
                    tmpResponse.ResponseId = 400;
                    tmpResponse.ReturnMessage = "Coupon is Already Used";
                }
                else
                {
                    new SpRepository().UpdateCoupon(coupons.CouponCode);
                    tmpResponse.ResponseId = 200;
                    tmpResponse.ReturnMessage = "Success";
                }
                coupons.MessageResponse = tmpResponse;

                return Request.CreateResponse(HttpStatusCode.OK, coupons);
            }
            else
            {
                tmpResponse.ResponseId = 400;
                tmpResponse.ReturnMessage = "Invalid Coupon";
                coupons.MessageResponse = tmpResponse;
                return Request.CreateResponse(HttpStatusCode.OK, coupons);
            }
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("api/TicketingAPI/GetEventNames")]
        public HttpResponseMessage GetEventNames()
        {
            GenericRepository<TicketingEventsNew> _Events = new GenericRepository<TicketingEventsNew>(_unitOfWork);
            Musika.Models.TicketingEventsNew events = new TicketingEventsNew();
            Models.EventReponse tmpResponse = new EventReponse();

            var _list = (from A1 in _Events.Repository.GetAll()
                         select new TicketingEventsNew
                         {
                             EventID = A1.EventID,
                             EventTitle = A1.EventTitle
                         }).ToList();
            if (_list.Count > 0)
            {
                return Request.CreateResponse(HttpStatusCode.OK, _list);
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, null));
            }
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("api/TicketingAPI/GetPackageNames/{eventName}")]
        public HttpResponseMessage GetPackageNames(string eventName)
        {
            GenericRepository<TicketingEventTicketsSummary> _EventSummary = new GenericRepository<TicketingEventTicketsSummary>(_unitOfWork);
            GenericRepository<TicketingEventsNew> _TicketingEvents = new GenericRepository<TicketingEventsNew>(_unitOfWork);

            Musika.Models.TicketingEventTicketsSummaryModel _TicketingEventsModel = new Musika.Models.TicketingEventTicketsSummaryModel();


            Models.EventReponse tmpResponse = new EventReponse();

            var _list = (from A in _EventSummary.Repository.GetAll()
                         join B in _TicketingEvents.Repository.GetAll() on A.EventID equals B.EventID
                         select new TicketingEventTicketsSummaryModel
                         {
                             EventName = B.EventTitle,
                             TicketCategory = A.TicketCategory,
                             TicketType = A.TicketType
                         }).Where(p => p.EventName == eventName).GroupBy(x => x.TicketCategory, (key, group) => group.First()).ToList();
            if (_list.Count > 0)
            {
                return Request.CreateResponse(HttpStatusCode.OK, _list);
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, null));
            }
        }
        #endregion

        #region "Coupons"

        [Route("api/TicketingAPI/GetCouponsByEventID")]
        [HttpGet]
        public HttpResponseMessage GetCouponsByEventID(string EventID)
        {
            try
            {
                GenericRepository<TicketingEventsNew> _TicketingEventRepo = new GenericRepository<TicketingEventsNew>(_unitOfWork);
                GenericRepository<TourDate> _TourDateRepo = new GenericRepository<TourDate>(_unitOfWork);
                CouponsModel model = new CouponsModel();
                DataSet ds = new DataSet();
                SpRepository _sp = new SpRepository();
                ds = _sp.GetCouponsListByEventId(Convert.ToInt32(EventID));
                var _list = General.DTtoList<CouponsModel>(ds.Tables[0]);

                _list = _list.GroupBy(x => x.CouponCode, (key, group) => group.First()).OrderBy(p => p.ExpiryDate).ToList();

                Models.EventReponse tmpResponse = new EventReponse();

                tmpResponse.ResponseId = 200;
                tmpResponse.ReturnMessage = "Success";

                model.MessageResponse = tmpResponse;
                return Request.CreateResponse(HttpStatusCode.OK, _list);
            }
            catch (Exception ex)
            {
                //LogHelper.CreateLog(ex);
                return Request.CreateResponse(HttpStatusCode.OK, ex.Message);
            }
        }

        [Route("api/TicketingAPI/GetCouponsByUserID")]
        [HttpGet]
        public HttpResponseMessage GetCouponsByUserID(string UserID)
        {
            try
            {
                GenericRepository<TicketingEventsNew> _TicketingEventRepo = new GenericRepository<TicketingEventsNew>(_unitOfWork);
                GenericRepository<TourDate> _TourDateRepo = new GenericRepository<TourDate>(_unitOfWork);
                CouponsModel model = new CouponsModel();
                DataSet ds = new DataSet();
                SpRepository _sp = new SpRepository();
                ds = _sp.GetCouponsListByUserId(Convert.ToInt32(UserID));
                var _list = General.DTtoList<CouponsModel>(ds.Tables[0]);

                _list = _list.GroupBy(x => x.CouponCode, (key, group) => group.First()).OrderByDescending(p => p.Id).ToList();

                Models.EventReponse tmpResponse = new EventReponse();

                tmpResponse.ResponseId = 200;
                tmpResponse.ReturnMessage = "Success";

                model.MessageResponse = tmpResponse;
                return Request.CreateResponse(HttpStatusCode.OK, _list);
            }
            catch (Exception ex)
            {
                //LogHelper.CreateLog(ex);
                return Request.CreateResponse(HttpStatusCode.OK, ex.Message);
            }
        }


        [Route("api/TicketingAPI/GetCouponByID")]
        [HttpGet]
        public HttpResponseMessage GetCouponByID(Int32 ID)
        {
            try
            {
                Models.CouponsModel _Coupons = null;
                GenericRepository<CouponsModel> _CouponsRepo = new GenericRepository<CouponsModel>(_unitOfWork);

                _Coupons = _CouponsRepo.Repository.Get(p => p.Id == ID);

                if (_Coupons != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, _Coupons);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "Record Not Found");
                }
            }
            catch (Exception ex)
            {
                //LogHelper.CreateLog(ex);
                return Request.CreateResponse(HttpStatusCode.ExpectationFailed, ex.Message);
            }
        }

        [Route("api/TicketingAPI/GetTicketEventSummaryByEventID")]
        [HttpGet]
        public HttpResponseMessage GetTicketEventSummaryByEventID(string EventID)
        {
            try
            {
                GenericRepository<TicketingEventTicketsSummary> _EventsRepo = new GenericRepository<TicketingEventTicketsSummary>(_unitOfWork);

                var _list = _EventsRepo.Repository.GetAll().Where(p => p.EventID == Convert.ToInt32(EventID) && p.TicketType == "Paid").Select(p => new { p.Id, p.TicketCategory }).ToList();

                if (_list != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, _list);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "Record Not Found");
                }
            }
            catch (Exception ex)
            {
                //LogHelper.CreateLog(ex);
                return Request.CreateResponse(HttpStatusCode.ExpectationFailed, ex.Message);
            }
        }


        [HttpPost]
        [Route("api/TicketingAPI/AddCoupon")]
        public AdminResponse AddCoupon([FromBody] CouponsUpdateModel coupon)
        {
            AdminResponse _AdminResponse = new AdminResponse();
            bool res;
            try
            {
                var _EventName = coupon.EventName;
                var _CouponCode = coupon.CouponCode;
                var _Discount = coupon.Discount;
                var _ExpiryDate = coupon.ExpiryDate;
                var _TicketCategory = coupon.TicketCategory;

                // Check Existence of Coupon Code
                res = new SpRepository().CheckCouponCode(_EventName, _CouponCode);
                if (res == false)
                {
                    new SpRepository().SpAddCouponsNew(_EventName, _CouponCode, Convert.ToDecimal(_Discount), Convert.ToDateTime(_ExpiryDate), 1, _TicketCategory);
                    _AdminResponse.Status = true;
                    _AdminResponse.RetMessage = "Coupons added successfully.";
                }
                else
                {
                    _AdminResponse.Status = false;
                    _AdminResponse.RetMessage = "Coupons Code Already Exists.";
                }
                return _AdminResponse;
            }
            catch (Exception ex)
            {
                _unitOfWork.RollBack();
                _AdminResponse.Status = false;
                _AdminResponse.RetMessage = ex.Message;
                return _AdminResponse;
            }
        }

        [HttpPost]
        [Route("api/TicketingAPI/UpdateCoupon")]
        public AdminResponse UpdateCoupon([FromBody] CouponsUpdateModel coupon)
        {
            AdminResponse _AdminResponse = new AdminResponse();
            bool res;
            try
            {
                var _CouponId = coupon.Id;
                var _EventName = coupon.EventName;
                var _CouponCode = coupon.CouponCode;
                var _Discount = coupon.Discount;
                var _ExpiryDate = coupon.ExpiryDate;
                var _CreatedBy = coupon.CreatedBy;
                var _TicketCategory = coupon.TicketCategory;

                // Check Existence of Coupon Code
                res = new SpRepository().CheckCouponCodeForEdit(_CouponId);
                if (res == true)
                {
                    new SpRepository().SpUpdateCouponsNew(_CouponId, _EventName, _CouponCode, Convert.ToDecimal(_Discount), Convert.ToDateTime(_ExpiryDate), Convert.ToInt32(_CreatedBy), _TicketCategory);
                    _AdminResponse.Status = true;
                    _AdminResponse.RetMessage = "Coupons updated successfully.";
                }
                else
                {
                    _AdminResponse.Status = false;
                    _AdminResponse.RetMessage = "Coupons Code Not Exists.";
                }
                return _AdminResponse;
            }
            catch (Exception ex)
            {
                _unitOfWork.RollBack();
                _AdminResponse.Status = false;
                _AdminResponse.RetMessage = ex.Message;
                return _AdminResponse;
            }
        }
        #region "Final Delete Coupon Event by ID"
        [HttpGet]
        [Route("api/TicketingAPI/DeleteCouponById")]
        public AdminResponse DeleteCouponById(string Id)
        {
            AdminResponse _AdminResponse = new AdminResponse();
            bool res;
            try
            {

                if (!string.IsNullOrEmpty(Id))
                {
                    res = new SpRepository().SpDeleteCoupon(Convert.ToInt32(Id));
                    if (res)
                    {
                        _AdminResponse.Status = true;
                        _AdminResponse.RetMessage = "Coupons Deleted successfully.";
                    }
                    else
                    {
                        _AdminResponse.Status = false;
                        _AdminResponse.RetMessage = "Coupons Not Deleted.";
                    }

                }
            }
            catch (Exception ex)
            {
                _AdminResponse.Status = false;
                _AdminResponse.RetMessage = ex.Message;

            }
            return _AdminResponse;
        }
        #endregion

        [Route("api/TicketingAPI/GetCouponsByID")]
        [HttpGet]
        public HttpResponseMessage GetCouponsByID(string Id)
        {
            try
            {
                GenericRepository<TicketingEventsNew> _TicketingEventRepo = new GenericRepository<TicketingEventsNew>(_unitOfWork);
                GenericRepository<TourDate> _TourDateRepo = new GenericRepository<TourDate>(_unitOfWork);
                CouponsModel model = new CouponsModel();
                DataSet ds = new DataSet();
                SpRepository _sp = new SpRepository();
                //ds = _sp.GetCouponsListByEventId(Convert.ToInt32(Id));
                ds = _sp.GetCouponsId(Convert.ToInt32(Id));
                var _list = General.DTtoList<CouponsModel>(ds.Tables[0]);

                _list = _list.GroupBy(x => x.CouponCode, (key, group) => group.First()).OrderBy(p => p.ExpiryDate).ToList();

                Models.EventReponse tmpResponse = new EventReponse();

                tmpResponse.ResponseId = 200;
                tmpResponse.ReturnMessage = "Success";

                model.MessageResponse = tmpResponse;
                return Request.CreateResponse(HttpStatusCode.OK, _list);
            }
            catch (Exception ex)
            {
                //LogHelper.CreateLog(ex);
                return Request.CreateResponse(HttpStatusCode.OK, ex.Message);
            }
        }


        public static string RemoveDiacritics(string value)
        {
            if (String.IsNullOrEmpty(value))
                return value;

            string normalized = value.Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder();

            foreach (char c in normalized)
            {
                if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }

            Encoding nonunicode = Encoding.GetEncoding(850);
            Encoding unicode = Encoding.Unicode;

            byte[] nonunicodeBytes = Encoding.Convert(unicode, nonunicode, unicode.GetBytes(sb.ToString()));
            char[] nonunicodeChars = new char[nonunicode.GetCharCount(nonunicodeBytes, 0, nonunicodeBytes.Length)];
            nonunicode.GetChars(nonunicodeBytes, 0, nonunicodeBytes.Length, nonunicodeChars, 0);

            return new string(nonunicodeChars);
        }
        #endregion

        #region "Search by ArtistName or TicketNumber"
        [HttpGet]
        [Route("api/TicketingAPI/GetUserEventsSearch")]
        public HttpResponseMessage GetUserEventsSearch(string searchText, int UserID)
        {
            DataSet dsEventList = new DataSet();
            if (string.IsNullOrEmpty(searchText))
            {
                searchText = "";
            }
            try
            {
                dsEventList = new SpRepository().GetUserEventsSearch(searchText, UserID);

                var _list = dsEventList.Tables[0].AsEnumerable().Select(
                           dataRow => new ViewTicketingEventListNew
                           {
                               EventID = dataRow.Field<Int32>("EventID"),
                               StartDate = Convert.ToDateTime(dataRow.Field<DateTime>("StartDate")),
                               StartTime = dataRow.Field<string>("StartTime"),
                               EndDate = Convert.ToDateTime(dataRow.Field<DateTime>("EndDate")),
                               EndTime = dataRow.Field<string>("EndTime"),
                               EventImage = dataRow.Field<string>("EventImage"),
                               VenueName = dataRow.Field<String>("VenueName"),
                               ArtistName = dataRow.Field<String>("ArtistName")
                           }).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, _list));
            }
            catch (Exception ee)
            {
                LogHelper.CreateLog3(ee, Request);
                return Request.CreateResponse(HttpStatusCode.BadRequest, "");
            }
        }
        #endregion
    }
}