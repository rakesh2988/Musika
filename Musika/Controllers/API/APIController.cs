using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Configuration;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Specialized;
using System.Xml.Serialization;
using System.Web.Caching;
using System.Net.Http.Headers;
using System.Drawing;
using Newtonsoft.Json;
using System.Configuration;
using Newtonsoft.Json.Linq;
using MvcPaging;
using System.Web.Script.Serialization;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

using Musika.Repository.Interface;
using Musika.Repository.UnitofWork;
using Musika.Repository.GRepository;
using Musika.Models;
using Musika.Library.Utilities;
using Musika.Enums;
using Musika.Library.API;
using Musika.Repository.SPRepository;
using Musika.Models.API.Input;
using Musika.Library.Multipart;
using Musika.Models.API.View;
using Musika.Library.CacheProvider;
using Musika.Library.JsonConverter;

using SpotifyWebAPI;
using SpotifyWebAPI.Web.Enums;
using SpotifyWebAPI.Web.Models;
using Musika.Library.Search;
using Musika.Library.PushNotfication;
using System.Data;
using ArtSeeker.Library.Common;
using System.Text;
using System.Threading;
using System.Collections;
using System.Data.Entity;
using System.Xml;

namespace Musika.Controllers.API
{

    public class APIController : ApiController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly HttpCache _Cache = new HttpCache();

        int _Imagethumbsize = 0;
        int _imageSize = 0;
        bool _ApiLogger = false;
        double _FuzzySearchCri = 0.33;

        private readonly string[] _GenreFilter =
                                    {"Axé","Axe","Bachata","Baião",
                                    "Baiao" ,"Bambuco","Banda","Batucada","Biguine",
                                    "Bolero","Bomba","Boogaloo","Bossa nova",
                                    "Brazilian rock","Cha-cha-cha","Cha cha cha","Changüí",
                                    "Charanga","Choro" ,"Compas","Conga","Conjunto",
                                    "Contradanza","Corrido","Cuarteto","Cueca","Cumbia",
                                    "Danza","Danzón","Danzon","Duranguense","Filin","Forró",
                                    "Forro","Frevo","Funk carioca","Guaguancó","Guaguanco",
                                    "Guaracha","Gwo ka","Huapango","Huayno","Jarabe","Joropo",
                                    "Lambada","Latin","Lundu","Mambo","Mariachi","Mazouk","Merengue",
                                    "Méringue","Meringue","Milonga","Música popular brasileira",
                                    "Musica popular brasileira", "Norteño","Norteno","Nueva canción",
                                    "Nueva cancion","Nueva trova","Orquesta típica","Orquesta tipica",
                                    "Pachanga","Pagode","Pambiche","Pasillo","Payada","Plena","Porro","Pop in spanish",
                                    "Punto guajiro","Ranchera","Rasin","Reggaeton","Rondalla","Rumba","Salsa","Samba",
                                    "Sertanejo","Seis","Son","Son jalisciense","Son Jarocho","Son montuno","Songo","Tango",
                                    "Tango Tejano","Timba","Tonada","Trío romántico","Tropicália","Tropicalia","Twoubadou",
                                    "Vallenato","Vals criollo","Zouk"};


        public APIController()
        {
            _unitOfWork = new UnitOfWork();
            _Imagethumbsize = Convert.ToInt16(WebConfigurationManager.AppSettings["ImageThumbSize"].ToString());
            _imageSize = Convert.ToInt16(WebConfigurationManager.AppSettings["ImageSize"].ToString());
            _ApiLogger = Convert.ToBoolean(WebConfigurationManager.AppSettings["Logger"].ToString());

        }


        #region "Users"

        private ViewUser UserDetail(Int64 _UserID, bool _IsNewUser)
        {
            ViewUser _viewUser = new ViewUser();
            GenericRepository<Users> _UsersRepo = new GenericRepository<Users>(_unitOfWork);

            Users entity;
            entity = _UsersRepo.Repository.Get(u => u.UserID == _UserID);

            if (entity != null)
            {
                _viewUser.UserID = entity.UserID;
                _viewUser.Email = entity.Email;
                _viewUser.UserName = entity.UserName;

                _viewUser.ImageURL = entity.ImageURL ?? string.Empty;
                _viewUser.ThumbnailURL = entity.ThumbnailURL ?? string.Empty;
                _viewUser.IsNewUser = _IsNewUser;
            }
            else
            {
                return null;
            }
            return _viewUser;
        }


        [HttpPost]
        [Route("api/Users/SetLanguage")]
        public HttpResponseMessage SetLanguage(InputLang input)
        {
            try
            {
                if (input.UserId > 0)
                {
                    Users user;
                    GenericRepository<Users> _UsersRepo = new GenericRepository<Users>(_unitOfWork);
                    if ((user = _UsersRepo.Repository.GetById(input.UserId)) != null ? true : false)
                    {
                        user.UserLanguage = input.Language.ToUpper() == EUserLanguage.ES.ToString() ? EUserLanguage.ES.ToString() : EUserLanguage.EN.ToString();
                        _UsersRepo.Repository.Update(user);
                        return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, null, "Updated"));
                    }
                }
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, "UserID required"));
            }
            catch (Exception ex)
            {
                LogHelper.CreateLog3(ex, Request);
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Exception, ex.Message));
            }
        }



        [HttpGet]
        [Route("api/Users/GetUserProfile")]
        public HttpResponseMessage GetUserProfile(Int64 UserID)
        {
            try
            {
                if (UserID == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, "UserID required"));
                }


                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, UserDetail(UserID, false), "UserData"));

            }
            catch (Exception ex)
            {
                LogHelper.CreateLog3(ex, Request);
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Exception, ex.Message));
            }
        }


        [HttpPost]
        [Route("api/Users/Signup")]
        public HttpResponseMessage Signup()
        {
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
                
                var context = new ValidationContext(Input, serviceProvider: null, items: null);
                var results = new List<ValidationResult>();
                var isValid = Validator.TryValidateObject(Input, context, results);

                //return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, "salman test 4"));
                
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
                Models.Users entity = new Models.Users();
                GenericRepository<Users> _userEntity = new GenericRepository<Users>(_unitOfWork);


                ///check USer Name 
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
                Users _user = null;

                //_user = _userEntity.Repository.Get(e => e.UserName == _UserName);
                //if (_user != null)
                //{
                //    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.UserNameAlreadyExists));
                //}

                //check if Email already exists
                _user = _userEntity.Repository.Get(e => e.Email == Input.Email && e.RecordStatus != RecordStatus.Deleted.ToString());
                if (_user != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.EmailAlreadyExists));
                }

                _unitOfWork.StartTransaction();// Start Transaction
                // _unitOfWork.Commit();// End Transaction


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
                    entity.RecordStatus = RecordStatus.Active.ToString();


                    //if (HttpContext.Current.Request.Files.Count == 0)
                    //{
                    entity.ThumbnailURL = WebConfigurationManager.AppSettings["SiteImgURL"] + "/default-user.png";
                    entity.ImageURL = ConfigurationManager.AppSettings["SiteImgURL"] + "/default-user.png";
                    //}

                    //if (HttpContext.Current.Request.Files.Count > 0)
                    //{
                    //    ImageResponse imgResponse = MultipartFiles.GetMultipartImage(HttpContext.Current.Request.Files, "Image", "profileimage", _Imagethumbsize, _Imagethumbsize, _imageSize, _imageSize, true, true, true, "profileimage");

                    //    if (imgResponse.IsSuccess == true)
                    //    {
                    //        entity.ThumbnailURL = imgResponse.ThumbnailURL;
                    //        entity.ImageURL = imgResponse.ImageURL;
                    //    }
                    //    else
                    //    {
                    //        _unitOfWork.RollBack();//RollBack Transaction
                    //        return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, imgResponse.ResponseMessage));
                    //    }
                    //}

                    _userEntity.Repository.Add(entity);

                    SetUserDevices(entity);
                    //OpenFireRepository _OpenFireRepository = new OpenFireRepository();
                    //_OpenFireRepository.InsertUser(WebConfigurationManager.AppSettings["XMPPPrefix"].ToString() + input.UserName, input.Email, input.Password);

                    GenericRepository<UserSettings> _UserSettingsEntity = new GenericRepository<UserSettings>(_unitOfWork);
                    UserSettings _UserSettings = new UserSettings();


                    _UserSettings.UserID = entity.UserID;
                    _UserSettings.SettingKey = EUserSettings.Musika.ToString();
                    _UserSettings.SettingValue = true;
                    _UserSettings.NotificationCount = 0;
                    _UserSettingsEntity.Repository.Add(_UserSettings);

                    //_UserSettings = new UserSettings();
                    //_UserSettings.UserID = entity.UserID;
                    //_UserSettings.SettingKey = EUserSettings.Orders.ToString();
                    //_UserSettings.SettingValue = true;
                    //_UserSettings.NotificationCount = 0;
                    //_UserSettingsEntity.Repository.Add(_UserSettings);

                    //_UserSettings = new UserSettings();
                    //_UserSettings.UserID = entity.UserID;
                    //_UserSettings.SettingKey = EUserSettings.Offers.ToString();
                    //_UserSettings.SettingValue = true;
                    //_UserSettings.NotificationCount = 0;
                    //_UserSettingsEntity.Repository.Add(_UserSettings);

                    //_UserSettings = new UserSettings();
                    //_UserSettings.UserID = entity.UserID;
                    //_UserSettings.SettingKey = EUserSettings.Messages.ToString();
                    //_UserSettings.SettingValue = true;
                    //_UserSettings.NotificationCount = 0;
                    //_UserSettingsEntity.Repository.Add(_UserSettings);


                    _unitOfWork.Commit();// End Transaction

                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, UserDetail(entity.UserID, true), "UserData"));
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

        [HttpGet]
        [Route("api/logout")]
        public ApiResponse Logout(int UserId, string DeviceToken)
        {
            try
            {
                GenericRepository<UserDevices> _UserDevices = new GenericRepository<UserDevices>(_unitOfWork);
                var userDevice = _UserDevices.Repository.Get(x => x.UserId == UserId && x.DeviceToken == DeviceToken);

                if (userDevice != null)
                    _UserDevices.Repository.DeletePermanent(userDevice.UserDeviceId);

                return JsonResponse.GetResponse(ResponseCode.Success, null);
            }
            catch (Exception ex)
            {
                LogHelper.CreateLog3(ex, Request);
                return JsonResponse.GetResponse(ResponseCode.Exception, JsonResponse.GetResponse(ResponseCode.Exception, ex.Message));
            }

        }


        [HttpPost]
        [Route("api/Users/signin")]
        public HttpResponseMessage Signin(InputSignIn input)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.OK, ModelState);
                }

                // var _st = AesCryptography.Decrypt("w2fl54iG1nrKcJbuSwHMCA==");


                Models.Users entity = null;
                GenericRepository<Users> _UsersRepo = new GenericRepository<Users>(_unitOfWork);

                string _UserName = "";
                string _Password = "";


                _UserName = WebConfigurationManager.AppSettings["XMPPPrefix"].ToString() + input.UserName;
                _Password = AesCryptography.Encrypt(input.Password);


                //entity = _UsersRepo.Repository.Get(p => p.UserName.ToLower() == _UserName.ToLower() && p.Password == _Password);

                //if (entity == null)
                //{
                entity = _UsersRepo.Repository.Get(p => p.Email.ToLower() == input.UserName.ToLower() && p.Password == _Password
                                                    && p.RecordStatus != RecordStatus.Deleted.ToString());
                // }

                if (entity != null)
                {

                    if (entity.RecordStatus != RecordStatus.Active.ToString())
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, "User has blocked"));
                    }
                    if (entity.Password != _Password)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.PasswordOrEmail));
                    }

                    entity.DeviceType = input.DeviceType;
                    entity.DeviceToken = input.DeviceToken;
                    entity.DeviceLong = input.DeviceLong;
                    entity.DeviceLat = input.DeviceLat;

                    _UsersRepo.Repository.Update(entity);


                    SetUserDevices(entity);

                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, UserDetail(entity.UserID, false), "UserData"));
                }
                else
                {

                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.PasswordOrEmail));

                }


            }
            catch (Exception ex)
            {
                LogHelper.CreateLog3(ex, Request);
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, ex.Message));

            }
        }

        [HttpPost]
        [Route("api/Users/GetForgetPassword")]
        public ApiResponse GetForgetPassword(dynamic data)
        {
            if (data.Email != null)
            {
                GenericRepository<Users> _UsersRepo = new GenericRepository<Users>(_unitOfWork);
                string _email = data.Email;

                var entity = _UsersRepo.Repository.Get(p => p.Email == _email);
                if (entity != null)
                {
                    string password = AesCryptography.Decrypt(entity.Password);

                    string html = "<p>Hi " + entity.UserName + "</p>";
                    html += "<p>Thanks for using " + WebConfigurationManager.AppSettings["AppName"] + "! </p>";
                    html += "<p>Your password is : " + password + "</p>";
                    html += "<p><br>If you would like to change your password, go to the profile area of the app</p>";
                    html += "<p><br><br><strong>Thanks,The " + WebConfigurationManager.AppSettings["AppName"] + " Team</strong></p>";


                    EmailHelper.SendEmail(entity.Email, WebConfigurationManager.AppSettings["AppName"] + " : Forget Password", html);

                    return JsonResponse.GetResponse(ResponseCode.Success, "Please check your email to get password.");
                }
                else
                {
                    return JsonResponse.GetResponse(ResponseCode.Failure, "User not found against this email.");
                }
            }
            else
            {
                return JsonResponse.GetResponse(ResponseCode.Failure, "Email cannot be empty.");
            }
        }

        [HttpPost]
        [Route("api/Users/signinthirdparty")]
        public HttpResponseMessage SigninThirdParty(InputSignInWithThirdParty input)
        {

            try
            {
                _unitOfWork.StartTransaction();// Start Transaction


                if (!ModelState.IsValid)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.OK, ModelState);
                }


                Models.Users entity = null;

                GenericRepository<Users> _UsersRepo = new GenericRepository<Users>(_unitOfWork);


                if (input.ThirdPartyType == ThirdPartyType.Facebook)
                {
                    entity = _UsersRepo.Repository.Get(p => p.FacebookID == input.ThirdPartyId
                                        && p.RecordStatus != RecordStatus.Deleted.ToString()
                                        );


                }


                string strThumbnailURLfordb = null;
                string strIamgeURLfordb = null;
                string strThumbnailURLfordbBlur = null;

                string _SiteRoot = WebConfigurationManager.AppSettings["SiteImgPath"];
                string _SiteURL = WebConfigurationManager.AppSettings["SiteImgURL"];
                byte[] converted;
                string strThumbnailImage = input.ImageURL;


                if (entity != null)
                {
                    //if (entity.RecordStatus == RecordStatus.Deleted.ToString())
                    //{
                    //    _unitOfWork.RollBack();
                    //    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, "User has been removed by the admin", "UserData"));
                    //}


                    if (entity.RecordStatus != RecordStatus.Active.ToString())
                    {
                        _unitOfWork.RollBack();// End Transaction
                        return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, "User has blocked"));
                    }


                    entity.UserName = input.UserName != null && input.UserName != "" ? input.UserName : entity.UserName;

                    entity.DeviceType = input.DeviceType != null && input.DeviceType != "" ? input.DeviceType : entity.DeviceType;
                    entity.DeviceToken = input.DeviceToken != null && input.DeviceToken != "" ? input.DeviceToken : entity.DeviceToken;

                    entity.DeviceLat = input.DeviceLat;
                    entity.DeviceLong = input.DeviceLong;

                    entity.ThumbnailURL = "";
                    entity.ImageURL = "";

                    if (input.ImageURL != null && input.ImageURL != "")
                    {

                        try
                        {


                            string strTempImageSave = ResizeImage.Download_Image(input.ImageURL);

                            //---temp Image path
                            string tempfilePath = WebConfigurationManager.AppSettings["SiteImgPath"].ToString() + @"\" + "TempImages" + @"\";

                            //---New Image path
                            string newFilePath = _SiteRoot + @"\" + "ProfileImage" + @"\";
                            Helper.CreateDirectories(_SiteRoot + @"\" + "ProfileImage" + @"\");

                            string thumbnailresizename = ResizeImage.Resize_Image_Thumb(tempfilePath + strTempImageSave, newFilePath, "_Thumb_" + strTempImageSave, _Imagethumbsize, _Imagethumbsize);
                            strThumbnailURLfordb = _SiteURL + "/ProfileImage/" + thumbnailresizename;

                            string imageresizename = ResizeImage.Resize_Image_Thumb(tempfilePath + strTempImageSave, newFilePath, "_Logo_" + strTempImageSave, _imageSize, _imageSize);
                            strIamgeURLfordb = _SiteURL + "/ProfileImage/" + imageresizename;


                            //Thumbnail Blur Filter

                            //string _BlurImage = ResizeImage.Get_BlurImage(newFilePath + thumbnailresizename, newFilePath, ImageBlurFilter.ExtBitmap.BlurType.Mean9x9);
                            //strThumbnailURLfordbBlur = _SiteURL + "/ProfileImage/" + _BlurImage;

                            entity.ThumbnailURL = strThumbnailURLfordb;
                            entity.ImageURL = strIamgeURLfordb;

                        }
                        catch (Exception ex)
                        {

                        }
                    }

                    _UsersRepo.Repository.Update(entity);

                    _unitOfWork.Commit();// End Transaction
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, UserDetail(entity.UserID, false), "UserData"));
                }
                else
                {
                    entity = new Models.Users();

                    entity.UserName = input.UserName;
                    entity.Email = input.Email;

                    bool Email = false;
                    Email = Helper.IsValidEmail(input.Email);

                    if (Email == false)
                    {
                        _unitOfWork.RollBack();// End Transaction
                        return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.Invalidemail, "UserData"));
                    }

                    entity.Password = "";

                    entity.DeviceType = input.DeviceType;
                    entity.DeviceToken = input.DeviceToken;

                    entity.DeviceLat = input.DeviceLat;
                    entity.DeviceLong = input.DeviceLong;
                    entity.CreatedDate = DateTime.Now;
                    entity.RecordStatus = RecordStatus.Active.ToString();

                    if (input.ThirdPartyType == ThirdPartyType.Facebook)
                    {
                        entity.FacebookID = input.ThirdPartyId;
                    }




                    if (input.ImageURL != null && input.ImageURL != "")
                    {

                        try
                        {

                            string strTempImageSave = ResizeImage.Download_Image(input.ImageURL);

                            //---temp Image path
                            string tempfilePath = WebConfigurationManager.AppSettings["SiteImgPath"].ToString() + @"\" + "TempImages" + @"\";

                            //---New Image path
                            string newFilePath = _SiteRoot + @"\" + "ProfileImage" + @"\";
                            Helper.CreateDirectories(_SiteRoot + @"\" + "ProfileImage" + @"\");

                            string thumbnailresizename = ResizeImage.Resize_Image_Thumb(tempfilePath + strTempImageSave, newFilePath, "_Thumb_" + strTempImageSave, _Imagethumbsize, _Imagethumbsize);
                            strThumbnailURLfordb = _SiteURL + "/ProfileImage/" + thumbnailresizename;

                            string imageresizename = ResizeImage.Resize_Image_Thumb(tempfilePath + strTempImageSave, newFilePath, "_Logo_" + strTempImageSave, _imageSize, _imageSize);
                            strIamgeURLfordb = _SiteURL + "/ProfileImage/" + imageresizename;


                            //Thumbnail Blur Filter

                            //string _BlurImage = ResizeImage.Get_BlurImage(newFilePath + thumbnailresizename, newFilePath, ImageBlurFilter.ExtBitmap.BlurType.Mean9x9);
                            //strThumbnailURLfordbBlur = _SiteURL + "/ProfileImage/" + _BlurImage;


                        }
                        catch (Exception ex)
                        {
                            strThumbnailURLfordb = strThumbnailImage;
                            strIamgeURLfordb = strThumbnailImage;
                        }
                    }

                    entity.ThumbnailURL = strThumbnailURLfordb;
                    entity.ImageURL = strIamgeURLfordb;



                    _UsersRepo.Repository.Add(entity);

                    SetUserDevices(entity);

                    GenericRepository<UserSettings> _UserSettingsEntity = new GenericRepository<UserSettings>(_unitOfWork);
                    UserSettings _UserSettings = new UserSettings();


                    _UserSettings.UserID = entity.UserID;
                    _UserSettings.SettingKey = EUserSettings.Musika.ToString();
                    _UserSettings.SettingValue = true;
                    _UserSettings.NotificationCount = 0;
                    _UserSettingsEntity.Repository.Add(_UserSettings);


                    //_UserSettings = new UserSettings();
                    //_UserSettings.UserID = entity.UserID;
                    //_UserSettings.SettingKey = EUserSettings.Orders.ToString();
                    //_UserSettings.SettingValue = true;
                    //_UserSettings.NotificationCount = 0;
                    //_UserSettingsEntity.Repository.Add(_UserSettings);

                    //_UserSettings = new UserSettings();
                    //_UserSettings.UserID = entity.UserID;
                    //_UserSettings.SettingKey = EUserSettings.Offers.ToString();
                    //_UserSettings.SettingValue = true;
                    //_UserSettings.NotificationCount = 0;
                    //_UserSettingsEntity.Repository.Add(_UserSettings);

                    //_UserSettings = new UserSettings();
                    //_UserSettings.UserID = entity.UserID;
                    //_UserSettings.SettingKey = EUserSettings.Messages.ToString();
                    //_UserSettings.SettingValue = true;
                    //_UserSettings.NotificationCount = 0;
                    //_UserSettingsEntity.Repository.Add(_UserSettings);

                    _unitOfWork.Commit();// End Transaction

                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, UserDetail(entity.UserID, true), "UserData"));

                }

            }
            catch (Exception ex)
            {
                LogHelper.CreateLog3(ex, Request);
                _unitOfWork.RollBack();// End Transaction
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, ex.Message, "UserData"));

            }
        }

        [HttpPost]
        [Route("api/Users/UpdateUserPicture")]
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


                Models.Users entity = null;
                GenericRepository<Users> _UsersRepo = new GenericRepository<Users>(_unitOfWork);

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

        [HttpPost]
        [Route("api/Users/UpdateUser")]
        public HttpResponseMessage UpdateUser(InputUpdateUser input)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.OK, ModelState);
                }


                Models.Users entity = null;
                Models.Users _Users = null;
                string _Email = "";
                string _Password = "";

                GenericRepository<Users> _UsersRepo = new GenericRepository<Users>(_unitOfWork);
                entity = _UsersRepo.Repository.Get(p => p.UserID == input.UserID);

                if (entity != null)
                {
                    string _password = "";

                    //Check the Current Password
                    if (input.NewPassword != null)
                    {
                        if (input.NewPassword != "")
                        {
                            if (input.CurrentPassword == null || input.CurrentPassword == "")
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, "Current password is not entered"));
                            }
                            _password = AesCryptography.Encrypt(input.CurrentPassword);

                            Models.Users _user = null;

                            _user = _UsersRepo.Repository.Get(e => e.Password == _password && e.UserID == input.UserID);

                            if (_user == null)
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.CurrentPasswordNotMatched));
                            }
                        }
                    }

                    //Check Email 
                    bool Email = false;

                    if (input.Email != null && input.Email != "")
                    {
                        Email = Helper.IsValidEmail(input.Email);
                        if (Email == false)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.Invalidemail));
                        }

                        //check if Email already exists
                        _Users = _UsersRepo.Repository.Get(e => e.Email == input.Email && e.UserID != input.UserID);
                        if (_Users != null)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.UserNameAlreadyExists));
                        }

                    }

                    //check if UserName already exists
                    //string _userName = WebConfigurationManager.AppSettings["XMPPPrefix"].ToString() + input.UserName;
                    //_Users = _UsersRepo.Repository.Get(e => e.UserName == _userName && e.UserID != input.UserID);
                    //if (_Users != null)
                    //{
                    //    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.UserNameAlreadyExists));
                    //}

                    //entity.Email = (input.Email != null) ? input.Email.ToString() != "" ? input.Email : entity.Email : entity.Email;
                    entity.UserName = (input.UserName != null) ? input.UserName.ToString() != "" ? WebConfigurationManager.AppSettings["XMPPPrefix"].ToString() + input.UserName : entity.UserName : entity.UserName;
                    entity.Password = (input.NewPassword != null) ? input.NewPassword.ToString() != "" ? AesCryptography.Encrypt(input.NewPassword) : entity.Password : entity.Password;


                    _UsersRepo.Repository.Update(entity);


                    //Sending Welcome Email
                    if (input.NewPassword != null)
                    {
                        if (input.NewPassword != "")
                        {
                            try
                            {
                                string html = "<p>Hi " + entity.UserName + "</p>";
                                html += "<p><br>Your password has been changed successfully.</p>";
                                html += "<p><br><br><strong>Thanks,The " + WebConfigurationManager.AppSettings["AppName"] + " Team</strong></p>";
                                EmailHelper.SendEmail(entity.Email, WebConfigurationManager.AppSettings["AppName"] + " Password Changed.", html);
                            }
                            catch (Exception ex)
                            {

                            }
                        }
                    }
                    //

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

        [HttpGet]
        [Route("api/Users/GetUserSetting")]
        public HttpResponseMessage GetUserSetting(Int64 UserID)
        {
            try
            {
                if (UserID == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, "UserID required"));
                }

                GenericRepository<UserSettings> _UserSetting = new GenericRepository<UserSettings>(_unitOfWork);

                List<UserSettings> list = _UserSetting.Repository.GetAll(p => p.UserID == UserID).ToList();

                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, list[0]));

            }
            catch (Exception ex)
            {
                LogHelper.CreateLog3(ex, Request);
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Exception, ex.Message));
            }


        }

        [HttpPost]
        [Route("api/Users/UpdateUserSetting")]
        public HttpResponseMessage UpdateUserSetting(InputUpdateUserSetting input)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.OK, ModelState);
                }


                Models.UserSettings entity = null;
                GenericRepository<UserSettings> _UserSettingsRep = new GenericRepository<UserSettings>(_unitOfWork);

                entity = _UserSettingsRep.Repository.Get(p => p.UserID == input.UserID && p.SettingKey == input.SettingKey.ToString());

                if (entity != null)
                {

                    entity.SettingValue = (input.SettingValue != null) ? input.SettingValue.ToString() != "" ? input.SettingValue : entity.SettingValue : entity.SettingValue;
                    entity.NotificationCount = (input.NotificationCount != null) ? input.NotificationCount.ToString() != "" ? input.NotificationCount : entity.NotificationCount : entity.NotificationCount;

                    _UserSettingsRep.Repository.Update(entity);

                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, entity));
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.UserSettingNotFound));
                }
            }
            catch (Exception ex)
            {
                LogHelper.CreateLog3(ex, Request);
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, ex.Message));
            }
        }

        [HttpPost]
        [Route("api/Users/ResetNotification")]
        public HttpResponseMessage ResetNotification(InputUpdateUserSetting input)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.OK, ModelState);
                }

                Models.UserSettings entity = null;
                GenericRepository<UserSettings> _UserSettingsRep = new GenericRepository<UserSettings>(_unitOfWork);

                entity = _UserSettingsRep.Repository.Get(p => p.UserID == input.UserID && p.SettingKey == input.SettingKey.ToString());

                if (entity != null)
                {
                    entity.NotificationCount = (input.NotificationCount != null) ? input.NotificationCount.ToString() != "" ? input.NotificationCount : entity.NotificationCount : entity.NotificationCount;

                    _UserSettingsRep.Repository.Update(entity);

                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, entity));
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.UserSettingNotFound));
                }
            }
            catch (Exception ex)
            {
                LogHelper.CreateLog3(ex, Request);
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, ex.Message));

            }
        }

        [HttpGet]
        [Route("api/Users/GetMusicSource")]
        public HttpResponseMessage GetMusicSource(string MusicSource)
        {
            try
            {
                GenericRepository<MSource> _MSourceRepo = new GenericRepository<MSource>(_unitOfWork);
                List<MSource> _lst = new List<MSource>();

                if (MusicSource == "IOS")
                {
                    _lst = (from A in _MSourceRepo.Repository.GetAll(p => p.RecordStatus == "IOS")
                            select new MSource
                            {
                                MSourceID = A.MSourceID,
                                MSourceName = A.MSourceName,
                                MSourceURL = WebConfigurationManager.AppSettings["WebPath"].ToString() + A.MSourceURL,
                                CreatedDate = A.CreatedDate,
                                ModifiedDate = A.ModifiedDate,
                                RecordStatus = A.RecordStatus
                            }).ToList();
                }
                else
                {
                    _lst = (from A in _MSourceRepo.Repository.GetAll(p => p.RecordStatus == "Android")
                            select new MSource
                            {
                                MSourceID = A.MSourceID,
                                MSourceName = A.MSourceName,
                                MSourceURL = WebConfigurationManager.AppSettings["WebPath"].ToString() + A.MSourceURL,
                                CreatedDate = A.CreatedDate,
                                ModifiedDate = A.ModifiedDate,
                                RecordStatus = A.RecordStatus
                            }).ToList();
                }


                if (_lst.Count > 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, _lst, "MusicSource"));
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.RecordNotFound));
                }
            }
            catch (Exception ex)
            {
                LogHelper.CreateLog3(ex, Request);
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Exception, ex.Message));
            }
        }

        [HttpPost]
        [Route("api/Users/UpdateMusicSource")]
        public HttpResponseMessage UpdateMusicSource(InputUpdateMusicSource input)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.OK, ModelState);
                }
                _unitOfWork.StartTransaction();

                GenericRepository<MusicSource> _MusicSourceRepo = new GenericRepository<MusicSource>(_unitOfWork);

                foreach (myMusicsource _item in input.MusicSource)
                {
                    var _lst = _MusicSourceRepo.Repository.GetAll(p => p.UserID == input.UserID && p.DeviceType == _item.DeviceType.ToString() && p.MsourceID == _item.MSourceID).Select(p => p.SourceID).ToList();
                    _MusicSourceRepo.Repository.DeletePermanent(_lst);

                    Models.MusicSource entity = new MusicSource();

                    entity.Source = _item.Source.ToString();
                    entity.UserID = input.UserID;
                    entity.MsourceID = _item.MSourceID;
                    entity.DeviceType = _item.DeviceType.ToString();
                    entity.CreatedDate = DateTime.Now;
                    entity.RecordStatus = RecordStatus.Active.ToString();

                    _MusicSourceRepo.Repository.Add(entity);
                }

                _unitOfWork.Commit();
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, null));

            }
            catch (Exception ex)
            {
                LogHelper.CreateLog3(ex, Request);
                if (_unitOfWork.IsTransaction == true) _unitOfWork.RollBack();
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, ex.Message));
            }
        }


        [Route("api/Users/syncFacebookFriends")]
        [HttpPost]
        public HttpResponseMessage syncFacebookFriends(JObject jObject)
        {
            /*{
             "UserID": 1,
             "UserFacebookID":"123232323",
                 "Friends": [
                   {
                     "name": "Kate Bell",
                     "id":"23232323"
                   },
                   {
                     "name": "alber don",
                     "id":"55656545" 
                   }
                  ]
             }*/

            try
            {
                _unitOfWork.StartTransaction();


                int myUserID = Convert.ToInt32((string)jObject["UserID"]);
                string myUserFacebookID = (string)jObject["UserFacebookID"];
                JArray Friends = (JArray)jObject["Friends"];


                GenericRepository<UserFriends> _UserFriendsRepo = new GenericRepository<UserFriends>(_unitOfWork);
                GenericRepository<Users> _UsersRepo = new GenericRepository<Users>(_unitOfWork);
                UserFriends _UserFriends = null;
                Users _myUser = null;

                _myUser = _UsersRepo.Repository.Get(p => p.UserID == myUserID);

                if (_myUser != null)
                {
                    if (String.IsNullOrEmpty(_myUser.FacebookID))
                    {
                        _myUser.SynFacebookID = myUserFacebookID;
                        _UsersRepo.Repository.Update(_myUser);
                    }

                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.UserNotFound, "FacebookFriends"));
                }

                foreach (var item in Friends)
                {
                    JObject jFriend = (JObject)item;

                    string jFriendName = (string)jFriend["name"];
                    string jFriendFacebookID = (string)jFriend["id"];

                    if (!String.IsNullOrEmpty(jFriendFacebookID))
                    {
                        var friendUsers = _UsersRepo.Repository.GetAll(x => x.UserID != myUserID && (x.FacebookID == jFriendFacebookID || x.SynFacebookID == jFriendFacebookID));

                        foreach (var friend in friendUsers)
                        {

                            if (!_UserFriendsRepo.Repository.AsQueryable().Any(x => x.UserID == myUserID && x.Matched_UserID == friend.UserID && x.FacebookID == jFriendFacebookID))
                            {
                                _UserFriends = new UserFriends();

                                _UserFriends.Name = jFriendName;
                                _UserFriends.UserID = _myUser.UserID;
                                _UserFriends.FacebookID = jFriendFacebookID;
                                _UserFriends.Matched_UserID = friend.UserID;
                                _UserFriends.CreatedDate = DateTime.Now;
                                _UserFriends.RecordStatus = RecordStatus.Active.ToString();

                                _UserFriendsRepo.Repository.Add(_UserFriends);
                            }

                            if (!_UserFriendsRepo.Repository.AsQueryable().Any(x => x.UserID == friend.UserID && x.Matched_UserID == myUserID && x.FacebookID == myUserFacebookID))
                            {
                                _UserFriends = new UserFriends();

                                _UserFriends.Name = _myUser.UserName;
                                _UserFriends.UserID = friend.UserID;
                                _UserFriends.FacebookID = myUserFacebookID;
                                _UserFriends.Matched_UserID = myUserID;
                                _UserFriends.CreatedDate = DateTime.Now;
                                _UserFriends.RecordStatus = RecordStatus.Active.ToString();


                                _UserFriendsRepo.Repository.Add(_UserFriends);
                            }
                        }
                    }
                }

                _unitOfWork.Commit();
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, null, "FacebookFriends"));
            }
            catch (Exception ex)
            {
                LogHelper.CreateLog3(ex, Request);
                _unitOfWork.RollBack();
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, ex.Message, "FacebookFriends"));
            }
        }


        [HttpGet]
        [Route("api/Users/GetLinkedAccount")]
        public HttpResponseMessage GetLinkedAccount(Int64 UserID, string devicetype)
        {
            try
            {
                if (UserID == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, "UserID required"));
                }

                if (devicetype == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, "Devicetype required"));
                }

                GenericRepository<MusicSource> _MusicSourceRepo = new GenericRepository<MusicSource>(_unitOfWork);
                GenericRepository<MSource> _MSourceRepo = new GenericRepository<MSource>(_unitOfWork);


                var _lst = (from A in _MusicSourceRepo.Repository.GetAll(p => p.UserID == UserID && p.DeviceType == devicetype)
                            join B in _MSourceRepo.Repository.GetAll() on A.MsourceID equals B.MSourceID
                            select new
                            {
                                B.MSourceID,
                                B.MSourceName,
                                MSourceURL = WebConfigurationManager.AppSettings["WebPath"].ToString() + B.MSourceURL
                            });

                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, _lst, "MusicSource"));
            }
            catch (Exception ex)
            {
                LogHelper.CreateLog3(ex, Request);
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Exception, ex.Message, "MusicSource"));
            }
        }

        #endregion


        #region "Artists"

        [HttpGet]
        [Route("api/Artists/GetArtistByName")]
        public HttpResponseMessage GetArtistByName(string search)
        {
            //changed
            try
            {
                //Spotify_SearchArtist(search);

                HttpCache _HttpCache = new HttpCache();
                List<MusicGraph.ArtistList> _ArtistList = new List<MusicGraph.ArtistList>();
                GenericRepository<ArtistsNotLatin> _ArtistNotLatinRepo = new GenericRepository<ArtistsNotLatin>(_unitOfWork);

                //Get Cache
                if (_HttpCache.Get(search, out _ArtistList) == true)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, _ArtistList, "Artist"));
                }


                //Check DB Artists First

                string pattern = @"[^a-zA-Z0-9áéíñóúüÁÉÍÑÓÚÜ\s]";
                search = Regex.Replace(search, pattern, "");

                if (search.Length < 3)
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, "", "Artist"));


                List<MusicGraph.ArtistList> _ArtistListDB = new List<MusicGraph.ArtistList>();

                var a = _unitOfWork.Db.Artists.AsQueryable().Select(x => new { x.Spotify_ID, x.ArtistName }).Where(x => x.ArtistName.Contains(search)).ToList();

                foreach (var artist in a)
                {
                    _ArtistListDB.Add(new MusicGraph.ArtistList
                    {
                        ID = string.IsNullOrEmpty(artist.Spotify_ID) ? artist.ArtistName : artist.Spotify_ID.ToString(),
                        Name = artist.ArtistName
                    });
                }

                if (_ArtistListDB.Count() > 3)
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, _ArtistListDB, "Artist"));

                //Then Check Music Graph API
                //Task<List<MusicGraph.Datum>> _Search_ByName = Task.Run(async () => await Spotify_SearchArtist(search));
                //_Search_ByName.Wait();

                string _MusicGrapgh_api_key = ConfigurationManager.AppSettings["MusicGrapgh_api_key"].ToString();
                string _result;

                var httpWebRequest = (HttpWebRequest)WebRequest.Create("http://api.musicgraph.com/api/v2/artist/suggest?api_key=" + _MusicGrapgh_api_key + "&limit=5&prefix=" + search);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "GET";

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    _result = streamReader.ReadToEnd();
                }


                // deserializing 
                var _Search_ByName = JsonConvert.DeserializeObject<MusicGraph.Search_ByName>(_result);

                bool isLatin = false;


                if (_Search_ByName != null)
                {
                    if (_Search_ByName.data.Count > 0)
                    {
                        _ArtistList = new List<MusicGraph.ArtistList>();

                        foreach (MusicGraph.Datum _datum in _Search_ByName.data)
                        {
                            if (!_ArtistListDB.Any(x => x.ID == _datum.id || x.Name == _datum.name))
                            {
                                //Check to filter Latin artist only
                                if (_GenreFilter.Contains(_datum.main_genre))
                                    isLatin = true;
                                else
                                {
                                    if (!isLatin)
                                        isLatin = CheckMusicGraphLatin(_datum.id, _unitOfWork);

                                    if (!isLatin)
                                    {
                                        //seat geek Latin check
                                        isLatin = CheckSeatGeekLatin(_datum.name, _unitOfWork);
                                    }

                                    if (!isLatin)
                                        isLatin = CheckLastResortSpotifyGenre(_datum.spotify_id);
                                }

                                if (isLatin)
                                {
                                    _ArtistList.Add(new MusicGraph.ArtistList
                                    {
                                        ID = _datum.id,
                                        Name = _datum.name
                                    });
                                }
                                else
                                {
                                    if (!_ArtistNotLatinRepo.Repository.AsQueryable().Any(x => x.ArtistName == _datum.name))
                                        _ArtistNotLatinRepo.Repository.Add(new ArtistsNotLatin() { ArtistName = _datum.name });
                                }
                            }
                            isLatin = false;
                        }

                        //Save cache
                        if (_ArtistList.Count > 0)
                        {
                            _ArtistList = _ArtistListDB.Union(_ArtistList).ToList();
                            _HttpCache.Set(search, _ArtistList, 24);
                        }
                        else
                            _ArtistList = _ArtistListDB;

                        return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, _ArtistList, "Artist"));
                    }

                    _ArtistList = _ArtistListDB;
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, _ArtistList, "Artist"));
                }

                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.InvalidArtistName, "Artist"));
            }
            catch (Exception ex)
            {
                LogHelper.CreateLog3(ex, Request);
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Exception, ex.Message, "Artist"));
            }
        }


        public bool CheckMusicGraphLatin(string mId, IUnitOfWork vUnitOfWork)
        {
            bool isLatin = false;

            try
            {
                HttpWebRequest httpWebRequest;
                HttpWebResponse httpResponse;
                string _result;
                string _MusicGrapgh_api_key = ConfigurationManager.AppSettings["MusicGrapgh_api_key"].ToString();
                httpWebRequest = (HttpWebRequest)WebRequest.Create("http://api.musicgraph.com/api/v2/artist/" + mId + "/profile?api_key=" + _MusicGrapgh_api_key);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "GET";

                httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    _result = streamReader.ReadToEnd();
                }

                JObject joResponse = JObject.Parse(_result);
                JArray items = (JArray)joResponse["data"];

                foreach (var i in items)
                {
                    if (i.Value<string>("type").ToLower() == "language" && i.Value<string>("tag").ToLower() == "spanish" && i.Value<int>("count") > 2)
                    {
                        isLatin = true;
                        break;
                    }
                }

            }
            catch (Exception e)
            { }
            return isLatin;
        }

        public bool CheckSeatGeekLatin(string artistName, IUnitOfWork vUnitOfWork)
        {
            bool isLatin = false;

            try
            {
                GenericRepository<ArtistsNotLatin> _ArtistNotLatinRepo = new GenericRepository<ArtistsNotLatin>(vUnitOfWork);

                if (!_ArtistNotLatinRepo.Repository.AsQueryable().Any(x => x.ArtistName == artistName))
                {

                    var serializer = new JavaScriptSerializer();
                    serializer.RegisterConverters(new[] { new DynamicJsonConverter() });

                    GenericRepository<Venue> _VenueRepo = new GenericRepository<Venue>(vUnitOfWork);
                    GenericRepository<TourDate> _TourDateRepo = new GenericRepository<TourDate>(vUnitOfWork);
                    GenericRepository<Artists> _ArtistsRepo = new GenericRepository<Artists>(vUnitOfWork);

                    GenericRepository<UserTourDate> _UserTourDateRepo = new GenericRepository<UserTourDate>(vUnitOfWork);
                    GenericRepository<ArtistGenre> _ArtistGenreRepo = new GenericRepository<ArtistGenre>(vUnitOfWork);
                    GenericRepository<Genre> _GenreRepo = new GenericRepository<Genre>(vUnitOfWork);

                    string _SeatGeek_client_id = ConfigurationManager.AppSettings["SeatGeek_client_id"].ToString();

                    string tempfilePath = WebConfigurationManager.AppSettings["SiteImgPath"].ToString() + @"\" + "TempImages" + @"\";

                    string _SiteRoot = WebConfigurationManager.AppSettings["SiteImgPath"];
                    string _SiteURL = WebConfigurationManager.AppSettings["SiteImgURL"];

                    HttpWebRequest httpWebRequest;
                    HttpWebResponse httpResponse;
                    string _result;
                    SeatGeek.Get_Performers _Get_Performers = null;

                    string pattern = @"[^a-zA-Z0-9áéíñóúüÁÉÍÑÓÚÜ\s]";
                    string cleanName = Regex.Replace(artistName, pattern, "");

                    httpWebRequest = (HttpWebRequest)WebRequest.Create("http://api.seatgeek.com/2/performers" + SGAuth() + "&taxonomies.id=2000000&taxonomies.id=2010000&taxonomies.id=3010100&taxonomies.id=3010200&taxonomies.id=3010300&q=" + cleanName.Trim());
                    httpWebRequest.ContentType = "application/json";
                    httpWebRequest.Method = "GET";

                    httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        _result = streamReader.ReadToEnd();
                    }

                    // deserializing 
                    _Get_Performers = JsonConvert.DeserializeObject<SeatGeek.Get_Performers>(_result);
                    SeatGeek.Performer _Performer = null;

                    if (_Get_Performers != null)
                    {
                        if (_Get_Performers.performers.Count > 0)
                        {
                            _Performer = (from A in _Get_Performers.performers
                                          where A.name == artistName.Trim()
                                          select A).OrderByDescending(p => p.score).FirstOrDefault();

                            if (_Performer == null)
                            {
                                _Performer = (from A in _Get_Performers.performers select A).OrderByDescending(p => p.score).FirstOrDefault();

                            }

                            #region "Loop through the Genre"
                            if (_Performer.genres != null && _Performer.genres.Count > 0)
                            {
                                foreach (SeatGeek.Genre _Ev in _Performer.genres)
                                {
                                    if (_GenreFilter.Contains(_Ev.name.Trim()))
                                        isLatin = true;

                                }
                            }
                            #endregion

                        }
                    }
                }
            }
            catch (Exception e)
            { }
            return isLatin;
        }


        [HttpGet]
        [Route("api/Artists/GetArtistInfoByID")]
        public async Task<HttpResponseMessage> GetArtistInfoByID(string ID, int UserID)
        {
            try
            {
                var serializer = new JavaScriptSerializer();
                serializer.RegisterConverters(new[] { new DynamicJsonConverter() });
                dynamic obj = null;

                _unitOfWork.StartTransaction();

                GenericRepository<Artists> _ArtistsRepo = new GenericRepository<Artists>(_unitOfWork);
                GenericRepository<Users> _UsersRepo = new GenericRepository<Users>(_unitOfWork);

                Models.Users _Users = null;
                _Users = _UsersRepo.Repository.Get(p => p.UserID == UserID);

                if (_Users == null || _Users.RecordStatus != RecordStatus.Active.ToString())
                {
                    _unitOfWork.RollBack();
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.UserNotFound, "ArtistDetail"));
                }

                Models.Artists _Artists = null;

                try
                {
                    int _ArtistID = Convert.ToInt16(ID);
                    _Artists = _ArtistsRepo.Repository.Get(p => p.ArtistID == _ArtistID);
                }
                catch { }


                if (_Artists == null)
                {
                    _Artists = _ArtistsRepo.Repository.Get(p => p.Musicgraph_ID == ID);
                }

                //check if artist mually added by admin
                if (_Artists == null)
                {
                    _Artists = _ArtistsRepo.Repository.Get(p => p.ArtistName == ID && string.IsNullOrEmpty(p.Musicgraph_ID));

                    if (_Artists != null)
                    {
                        var _search = await MusicGrapgh_GetArtistByName_Asyn(ID);
                        if (_search != null && _search.status.message == "Success")
                        {
                            foreach (MusicGraph.Datum _Datum in _search.data)
                            {
                                if (RemoveDiacritics(_Datum.name.ToLower()) == ID.ToLower())
                                {
                                    _Artists.Musicgraph_ID = _Datum.id;
                                    _ArtistsRepo.Repository.Update(_Artists);
                                }
                            }
                        }
                    }
                }

                //Get the artist information if found in Local DB and informatino is not more then 24 hours old

                if (_Artists != null)
                {
                    //TimeSpan span = DateTime.Now - Convert.ToDateTime(_Artists.ModifiedDate);
                    //if (span.Hours < 24 && span.Days == 0)
                    //{
                    //    _unitOfWork.RollBack();
                    //    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, GetArtistDetail_ByID(_Artists, UserID), "ArtistDetail"));
                    //}

                    //Get HashTags pictures
                    GetImagesFromInstagramFeed(_Artists);
                }

                string _result;

                string _Performer_ID = null;
                string _strEvent = null;

                Models.Venue _VenuEntity = null;
                Models.TourDate _TourDateEntity = null;
                Models.ArtistRelated _ArtistRelatedEntity = null;

                HttpWebRequest httpWebRequest;
                HttpWebResponse httpResponse;
                MusicGraph1.Search_ByID _Search_ByID = null;

                #region "Get Artist Info Using ID"


                if (_Artists == null)
                {
                    ID = _Artists != null ? _Artists.Musicgraph_ID : ID;

                    _Search_ByID = await MusicGrapgh_GetArtistByID_Asyn(ID);


                    if (_Search_ByID == null)
                    {
                        _unitOfWork.RollBack();
                        return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.InvalidArtistID, "Artist"));
                    }

                    if (_Search_ByID.status.message != "Success")
                    {
                        _unitOfWork.RollBack();
                        return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.InvalidArtistID, "Artist"));
                    }
                }
                #endregion


                if (_Artists == null)
                {
                    bool isLatin = false;

                    if (!isLatin)
                        isLatin = CheckMusicGraphLatin(_Search_ByID.data.id, _unitOfWork);

                    if (!isLatin)
                        isLatin = CheckSeatGeekLatin(_Search_ByID.data.name, _unitOfWork);

                    if (!isLatin)
                    {
                        isLatin = CheckLastResortSpotifyGenre(_Search_ByID.data.spotify_id);

                        if (!isLatin)
                        {
                            _unitOfWork.RollBack();
                            return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.InvalidArtistID, "Artist"));
                        }
                    }
                }

                await Task.Factory.StartNew(() =>
                {

                    #region "Extract data from Thirdparty API to Save/Update"

                    #region "Add New Rec"
                    //Start- Saving New Data 
                    if (_Artists == null)
                    {
                        _Artists = new Artists();

                        _Artists.ArtistName = _Search_ByID.data.name;//Regex.Replace(_Search_ByID.data.name, "[^A-Za-z0-9 _]", "");
                        _Artists.Gender = _Search_ByID.data.gender;
                        _Artists.Decade = _Search_ByID.data.decade;
                        _Artists.Main_Genre = _Search_ByID.data.main_genre;


                        _Artists.Musicgraph_ID = !String.IsNullOrEmpty(_Search_ByID.data.id) ? _Search_ByID.data.id : _Search_ByID.data.name;
                        _Artists.Artist_Ref_ID = _Search_ByID.data.artist_ref_id;
                        _Artists.Musicbrainz_ID = _Search_ByID.data.musicbrainz_id;
                        _Artists.Spotify_ID = _Search_ByID.data.spotify_id;
                        _Artists.Youtube_ID = _Search_ByID.data.youtube_id;
                        _Artists.Alternate_Names = _Search_ByID.data.alternate_names != null && _Search_ByID.data.alternate_names.Count > 0 ? _Search_ByID.data.alternate_names[0] : "";

                        _Artists.RecordStatus = RecordStatus.MusicGraph.ToString();
                        _Artists.CreatedDate = DateTime.Now;
                        _Artists.ModifiedDate = DateTime.Now;

                        _ArtistsRepo.Repository.Add(_Artists);

                        GetProfileImageFromSpotifyFeed(_Artists);

                        _unitOfWork.Commit();
                        _unitOfWork.StartTransaction();

                        #region "SeatGeek Api Implementation"
                        Task<bool> _seatGeek = SeatGeek_GetEventByArtistName_Asyn(UserID, _Artists, _unitOfWork, true);
                        #endregion
                        _seatGeek.Wait();// wait for the function to complete 

                        _unitOfWork.Commit();
                        _unitOfWork.StartTransaction();

                        #region "Eventful API Implementation"
                        Task<bool> _Eventful = EventFul_GetEventInfo_Asyn(UserID, _Artists, _unitOfWork, true);
                        #endregion
                        _Eventful.Wait();// wait for the function to complete 

                        #region "Get Artist Matrics (dont need this block while just updating the records)"
                        Task<MusicGraph2.ArtistMatrics_ByID> _ArtistMatrics_ByID = MusicGrapgh_GetArtistMatrics_Asyn(_Artists.Musicgraph_ID, _Artists.ArtistID, _unitOfWork);

                        _ArtistMatrics_ByID.Wait(); // wait for the completion of the matrics function


                        //Get Instagram ID from the MusicGraph matrcis 
                        if (_ArtistMatrics_ByID.Result.data.instagram != null && _ArtistMatrics_ByID.Result.data.instagram.url != null && _ArtistMatrics_ByID.Result.data.instagram.url != "")
                        {
                            _Artists.Instagram_Url = _ArtistMatrics_ByID.Result.data.instagram.url;
                            string _instaGram_ID = _ArtistMatrics_ByID.Result.data.instagram.url.Replace("http:", "https:").Replace("https://instagram.com/", "").Replace("/", "");
                            _Artists.Instagram_ID = _instaGram_ID;
                        }



                        #endregion



                        #region "Get Similar Artists (dont need this block while just updating the records)"
                        Task<MusicGraph5.GetSimilarArtists_ByID> _GetSimilarArtists_ByID = MusicGrapgh_GetSimilarArtists_Asyn(_Artists.Musicgraph_ID, _Artists.ArtistID, _unitOfWork);
                        #endregion

                        _GetSimilarArtists_ByID.Wait(); // wait for the function to complete 

                        #region "Instagram Api Implementation"
                        Task<bool> _Instagram = Instagram_GetPictures_Asyn(UserID, _Artists, _ArtistMatrics_ByID.Result, _unitOfWork, true, null);
                        #endregion

                        #region "Spotify Api Implementation
                        Task<bool> _Spotify = Spotify_GetSongInfo_Asyn(UserID, _Artists, _unitOfWork, true);
                        #endregion

                        _Instagram.Wait();// wait for the function to complete 

                        _Spotify.Wait();// wait for the function to complete 

                        string MusicGraphBio = "";
                        if (!String.IsNullOrEmpty(_Artists.Musicgraph_ID))
                            MusicGraphBio = GetMusicGraphBio(_Artists.Musicgraph_ID);
                        if (!String.IsNullOrEmpty(MusicGraphBio))
                            _Artists.About = MusicGraphBio;

                        _ArtistsRepo.Repository.Update(_Artists);
                        _unitOfWork.Commit();



                    }//End- Saving New Data 

                    #endregion

                    #region "Update Existing Record"
                    //Start - Update the data
                    else
                    {


                        //_Artists.ArtistName = Regex.Replace(_Search_ByID.data.name, "[^A-Za-z0-9 _]", "");
                        _Artists.ModifiedDate = DateTime.Now;

                        #region "Instagram Api Implementation"
                        Task<bool> _Instagram = Instagram_GetPictures_Asyn(UserID, _Artists, null, _unitOfWork, false, null);
                        #endregion

                        #region "SeatGeek Api Implementation"
                        Task<bool> _seatGeek = SeatGeek_GetEventByArtistName_Asyn(UserID, _Artists, _unitOfWork, false);
                        #endregion

                        _seatGeek.Wait();

                        #region "Eventful API Implementation"
                        Task<bool> _Eventful = EventFul_GetEventInfo_Asyn(UserID, _Artists, _unitOfWork, false);
                        #endregion

                        #region "Spotify Api Implementation
                        Task<bool> _Spotify = Spotify_GetSongInfo_Asyn(UserID, _Artists, _unitOfWork, false);
                        #endregion


                        _ArtistsRepo.Repository.Update(_Artists);
                        _Instagram.Wait();

                        _Eventful.Wait();
                        _Spotify.Wait();

                        _unitOfWork.Commit();

                    }
                    #endregion

                    #endregion
                }
                );

                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, GetArtistDetail_ByID(_Artists, UserID), "ArtistDetail"));
            }
            catch (Exception ex)
            {
                LogHelper.CreateLog3(ex, Request);

                if (_unitOfWork.IsTransaction == true) _unitOfWork.RollBack();
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Exception, ex.Message, "Artist"));
            }
        }

        [HttpPost]
        [Route("api/Artists/ScanAllUserLibrary")]
        public async Task<HttpResponseMessage> ScanAllUserLibrary(InputAllUserSanning input)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
                }
                var serializer = new JavaScriptSerializer();
                serializer.RegisterConverters(new[] { new DynamicJsonConverter() });
                dynamic obj = null;


                GenericRepository<Users> _UsersRepo = new GenericRepository<Users>(_unitOfWork);

                Models.Users _Users = null;
                _Users = _UsersRepo.Repository.Get(p => p.UserID == input.UserID);

                if (_Users == null || _Users.RecordStatus != RecordStatus.Active.ToString())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.UserNotFound, "ArtistDetail"));
                }

                string strThumbnailURLfordb = null;
                string strIamgeURLfordb = null;
                string strTempImageSave = null;

                string tempfilePath = WebConfigurationManager.AppSettings["SiteImgPath"].ToString() + @"\" + "TempImages" + @"\";

                string _SiteRoot = WebConfigurationManager.AppSettings["SiteImgPath"];
                string _SiteURL = WebConfigurationManager.AppSettings["SiteImgURL"];


                string _MusicGrapgh_api_key = ConfigurationManager.AppSettings["MusicGrapgh_api_key"].ToString();
                string _Instagram_access_token = ConfigurationManager.AppSettings["instagram.access_token"].ToString();

                string _Eventful_app_key = ConfigurationManager.AppSettings["Eventful_app_key"].ToString();

                string _SeatGeek_client_id = ConfigurationManager.AppSettings["SeatGeek_client_id"].ToString();
                string _SeatGeek_client_secret = ConfigurationManager.AppSettings["SeatGeek_client_secret"].ToString();

                string _Spotify_Country = ConfigurationManager.AppSettings["Spotify_Country"].ToString();

                string _result;

                string _Performer_ID = null;
                string _strEvent = null;

                Models.Venue _VenuEntity = null;
                Models.TourDate _TourDateEntity = null;
                Models.ArtistRelated _ArtistRelatedEntity = null;

                string _MG_Artist_ID = null;
                List<string> _TotalNames = new List<string>();

                GenericRepository<Artists> _ArtistsRepo = new GenericRepository<Artists>(_unitOfWork);
                GenericRepository<ArtistPhotos> _ArtistPhotosRepo = new GenericRepository<ArtistPhotos>(_unitOfWork);
                GenericRepository<ArtistRelated> _ArtistRelatedRepo = new GenericRepository<ArtistRelated>(_unitOfWork);
                GenericRepository<ArtistsNotLatin> _ArtistNotLatinRepo = new GenericRepository<ArtistsNotLatin>(_unitOfWork);
                GenericRepository<TourDate> _TourDateRepo = new GenericRepository<TourDate>(_unitOfWork);
                GenericRepository<Venue> _VenueRepo = new GenericRepository<Venue>(_unitOfWork);

                GenericRepository<UserArtists> _UserArtistsRepo = new GenericRepository<UserArtists>(_unitOfWork);
                GenericRepository<UserTourDate> _UserTourDateRepo = new GenericRepository<UserTourDate>(_unitOfWork);

                //Remove duplicates from artist list
                var names = input.ArtistNames.Select(x => x.Name);
                var uniqueNameList = names.Distinct().ToList();

                GenericRepository<MusicSource> _MusicSourceRepo = new GenericRepository<MusicSource>(_unitOfWork);
                Models.MusicSource _MusicSource = null;

                foreach (var m in input.AllMusicSource)
                {
                    _MusicSource = _MusicSourceRepo.Repository.Get(p => p.UserID == input.UserID && p.DeviceType == input.DeviceType.ToString() && p.MsourceID == m.MSourceID);

                    if (_MusicSource == null)
                    {
                        _MusicSource = new MusicSource();
                        _MusicSource.UserID = input.UserID;
                        _MusicSource.Source = m.MSourceName.ToString();
                        _MusicSource.MsourceID = m.MSourceID;
                        _MusicSource.DeviceType = input.DeviceType;
                        _MusicSource.CreatedDate = DateTime.Now;
                        _MusicSource.ModifiedDate = DateTime.Now;
                        _MusicSource.RecordStatus = RecordStatus.Active.ToString();

                        _MusicSourceRepo.Repository.Add(_MusicSource);

                    }
                    else
                    {
                        _MusicSource.ModifiedDate = DateTime.Now;
                        _MusicSourceRepo.Repository.Update(_MusicSource);
                    }
                }


                //Scanning the User Libraries
                foreach (var n in uniqueNameList)
                {
                    myArtistName _name = new myArtistName();
                    _name.Name = n;

                    bool isLatin = false;
                    TimeSpan span = TimeSpan.Zero;
                    Models.Artists _Artists = null;
                    Models.UserArtists _UserArtists = null;
                    Models.UserTourDate _UserTourDate = null;

                    _unitOfWork.StartTransaction();

                    _Artists = _ArtistsRepo.Repository.Get(p => p.ArtistName.ToLower() == _name.Name.ToLower());

                    //Get the artist information if found in Local DB and informatino is not more then 24 hours old

                    if (_Artists != null)
                    {
                        span = DateTime.Now - Convert.ToDateTime(_Artists.ModifiedDate);
                        //if (span.Hours < 24 && span.Days == 0)
                        //{
                        //    _unitOfWork.RollBack();
                        //    continue;
                        //}
                    }

                    #region "Get Artist Info by Artist Name"

                    HttpWebRequest httpWebRequest;
                    HttpWebResponse httpResponse;

                    if (_Artists == null)
                    {
                        string spotifyID = "";

                        if (_ArtistNotLatinRepo.Repository.AsQueryable().Any(x => x.ArtistName == _name.Name))
                        {
                            _unitOfWork.RollBack();
                            continue;
                        }

                        var _Search_ByName = await MusicGrapgh_GetArtistByName_Asyn(_name.Name.ToString()); // await for the funtion to completed

                        if (_Search_ByName != null)
                        {
                            if (_Search_ByName.data.Count > 0)
                            {
                                if (_Search_ByName.data.Count > 1)
                                {
                                    _Search_ByName.data = _Search_ByName.data.OrderByDescending(x => !String.IsNullOrEmpty(x.spotify_id)).ToList();
                                    _Search_ByName.data.RemoveRange(1, _Search_ByName.data.Count - 1);
                                }

                                foreach (MusicGraph.Datum _datum in _Search_ByName.data)
                                {
                                    _Artists = _ArtistsRepo.Repository.Get(p => p.Musicgraph_ID == _datum.id);
                                    if (_Artists != null)
                                    {
                                        isLatin = true;
                                        _MG_Artist_ID = _datum.id;

                                    }
                                    else
                                    {
                                        if (_name.Name.ToString().ToLower().Equals(_datum.name.ToString().ToLower()))
                                        {
                                            //Check to filter Latin artist only
                                            if (_GenreFilter.Contains(_datum.main_genre))
                                                isLatin = true;
                                            else
                                                isLatin = CheckMusicGraphLatin(_datum.id, _unitOfWork);

                                            spotifyID = _datum.spotify_id;

                                            _MG_Artist_ID = _datum.id;
                                            break;
                                        }
                                    }

                                }

                                if (_MG_Artist_ID == null)
                                {
                                    _unitOfWork.RollBack();
                                    continue;
                                }
                            }
                            else
                            {
                                _unitOfWork.RollBack();
                                continue;
                            }



                        }
                        else
                        {
                            _unitOfWork.RollBack();
                            continue;
                        }


                        //If music graph main genre is not latin then check seat geek sub genres for latin
                        if (!isLatin)
                        {
                            isLatin = await SeatGeek_CheckLatinGenre_Asyn(_name.Name, _unitOfWork);

                            if (!isLatin)
                            {
                                isLatin = CheckLastResortSpotifyGenre(spotifyID);

                                if (!isLatin)
                                {
                                    if (!_ArtistNotLatinRepo.Repository.AsQueryable().Any(x => x.ArtistName == _name.Name))
                                    {
                                        _ArtistNotLatinRepo.Repository.Add(new ArtistsNotLatin() { ArtistName = _name.Name });
                                        _unitOfWork.Commit();
                                        _unitOfWork.StartTransaction();
                                    }

                                    _MG_Artist_ID = null;
                                    _unitOfWork.RollBack();
                                    continue;
                                }
                            }
                        }

                    }

                    #endregion

                    #region "Get Artist Info using ID"

                    MusicGraph1.Search_ByID _Search_ByID = null;

                    if (_Artists == null)
                    {

                        _Search_ByID = await MusicGrapgh_GetArtistByID_Asyn(_MG_Artist_ID);// await for the funtion to completed


                        if (_Search_ByID == null)
                        {
                            _unitOfWork.RollBack();
                            continue;
                        }

                        if (_Search_ByID.status.message != "Success")
                        {
                            _unitOfWork.RollBack();
                            continue;
                        }

                        //If musicgraph ID already exists then rollback
                        if (_ArtistsRepo.Repository.AsQueryable().Any(p => p.Musicgraph_ID == _Search_ByID.data.id))
                        {
                            _unitOfWork.RollBack();
                            continue;
                        }


                    }
                    #endregion

                    await Task.Factory.StartNew(() =>
                    {
                        #region "Extract data from Thirdparty API to Save/Update"

                        #region "Add New Rec"
                        //Start- Saving New Data 
                        if (_Artists == null)
                        {
                            _Artists = new Artists();
                            _Artists.ArtistName = _Search_ByID.data.name;// Regex.Replace(_Search_ByID.data.name, "[^A-Za-z0-9 _]", "");


                            _Artists.Gender = _Search_ByID.data.gender;
                            _Artists.Decade = _Search_ByID.data.decade;
                            _Artists.Main_Genre = _Search_ByID.data.main_genre;

                            _Artists.Musicgraph_ID = !String.IsNullOrEmpty(_Search_ByID.data.id) ? _Search_ByID.data.id : _Search_ByID.data.name;
                            _Artists.Artist_Ref_ID = _Search_ByID.data.artist_ref_id;
                            _Artists.Musicbrainz_ID = _Search_ByID.data.musicbrainz_id;
                            _Artists.Spotify_ID = _Search_ByID.data.spotify_id;
                            _Artists.Youtube_ID = _Search_ByID.data.youtube_id;
                            _Artists.Alternate_Names = _Search_ByID.data.alternate_names != null && _Search_ByID.data.alternate_names.Count > 0 ? _Search_ByID.data.alternate_names[0] : "";

                            _Artists.RecordStatus = RecordStatus.MusicGraph.ToString();
                            _Artists.CreatedDate = DateTime.Now;
                            _Artists.ModifiedDate = DateTime.Now;

                            _ArtistsRepo.Repository.Add(_Artists);

                            GetProfileImageFromSpotifyFeed(_Artists);

                            _UserArtists = new UserArtists();
                            _UserArtists.UserID = input.UserID;
                            _UserArtists.ArtistID = _Artists.ArtistID;
                            _UserArtists.CreatedDate = DateTime.Now;
                            _UserArtists.RecordStatus = RecordStatus.Active.ToString();

                            _UserArtistsRepo.Repository.Add(_UserArtists);
                            _unitOfWork.Commit();
                            _unitOfWork.StartTransaction();

                            #region "Get Similar Artists (dont need this block while just updating the records)"
                            Task<MusicGraph5.GetSimilarArtists_ByID> _GetSimilarArtists_ByID = MusicGrapgh_GetSimilarArtists_Asyn(_Artists.Musicgraph_ID, _Artists.ArtistID, _unitOfWork);
                            #endregion
                            _GetSimilarArtists_ByID.Wait();


                            #region "Get Artist Matrics (dont need this block while just updating the records)"

                            Task<MusicGraph2.ArtistMatrics_ByID> _ArtistMatrics_ByID = MusicGrapgh_GetArtistMatrics_Asyn(_Artists.Musicgraph_ID, _Artists.ArtistID, _unitOfWork);

                            _ArtistMatrics_ByID.Wait(); // wait for the task to be completed

                            //Get Instagram ID from the MusicGraph matrcis 
                            if (_ArtistMatrics_ByID.Result.data.instagram != null && _ArtistMatrics_ByID.Result.data.instagram.url != null && _ArtistMatrics_ByID.Result.data.instagram.url != "")
                            {
                                _Artists.Instagram_Url = _ArtistMatrics_ByID.Result.data.instagram.url;
                                string _instaGram_ID = _ArtistMatrics_ByID.Result.data.instagram.url.Replace("http:", "https:").Replace("https://instagram.com/", "").Replace("/", "");
                                _Artists.Instagram_ID = _instaGram_ID;
                            }

                            #endregion



                            #region "SeatGeek Api Implementation"
                            Task<bool> _seatGeek = SeatGeek_GetEventByArtistName_Asyn(input.UserID, _Artists, _unitOfWork, true, true);
                            #endregion

                            _seatGeek.Wait();

                            _unitOfWork.Commit();
                            _unitOfWork.StartTransaction();

                            #region "Eventful API Implementation"
                            Task<bool> _Eventful = EventFul_GetEventInfo_Asyn(input.UserID, _Artists, _unitOfWork, true);
                            #endregion

                            _Eventful.Wait();

                            #region "Instagram Api Implementation"
                            Task<bool> _Instagram = Instagram_GetPictures_Asyn(input.UserID, _Artists, _ArtistMatrics_ByID.Result, _unitOfWork, true, null);
                            #endregion

                            _Instagram.Wait();

                            #region "Spotify Api Implementation
                            Task<bool> _Spotify = Spotify_GetSongInfo_Asyn(input.UserID, _Artists, _unitOfWork, true);
                            #endregion

                            _Spotify.Wait();




                            if (_TourDateRepo.Repository.AsQueryable().Any(x => x.ArtistID == _Artists.ArtistID && x.Datetime_Local >= DateTime.Now))
                            {
                                _Artists.OnTour = true;
                            }

                            string MusicGraphBio = "";
                            if (!String.IsNullOrEmpty(_Artists.Musicgraph_ID))
                                MusicGraphBio = GetMusicGraphBio(_Artists.Musicgraph_ID);
                            if (!String.IsNullOrEmpty(MusicGraphBio))
                                _Artists.About = MusicGraphBio;

                            _ArtistsRepo.Repository.Update(_Artists);
                            _unitOfWork.Commit();

                        }//End- Saving New Data 

                        #endregion

                        #region "Update Existing Record"
                        //Start - Update the data
                        else
                        {


                            _Artists.ModifiedDate = DateTime.Now;

                            _UserArtists = _UserArtistsRepo.Repository.Get(p => p.UserID == input.UserID && p.ArtistID == _Artists.ArtistID);

                            if (_UserArtists == null)
                            {
                                _UserArtists = new UserArtists();
                                _UserArtists.UserID = input.UserID;
                                _UserArtists.ArtistID = _Artists.ArtistID;
                                _UserArtists.CreatedDate = DateTime.Now;
                                _UserArtists.RecordStatus = RecordStatus.Active.ToString();

                                _UserArtistsRepo.Repository.Add(_UserArtists);
                            }

                            //if (span.Days > 1 || span.Hours > 24)
                            //{
                            #region "Instagram Api Implementation"
                            Task<bool> _Instagram = Instagram_GetPictures_Asyn(input.UserID, _Artists, null, _unitOfWork, false, null);
                            #endregion

                            _Instagram.Wait();

                            #region "SeatGeek Api Implementation"
                            Task<bool> _seatGeek = SeatGeek_GetEventByArtistName_Asyn(input.UserID, _Artists, _unitOfWork, false, true);
                            #endregion

                            _seatGeek.Wait();

                            #region "Eventful API Implementation"
                            Task<bool> _Eventful = EventFul_GetEventInfo_Asyn(input.UserID, _Artists, _unitOfWork, false);
                            #endregion

                            _Eventful.Wait();

                            #region "Spotify Api Implementation
                            Task<bool> _Spotify = Spotify_GetSongInfo_Asyn(input.UserID, _Artists, _unitOfWork, false);
                            #endregion

                            _Spotify.Wait();


                            //}

                            if (_TourDateRepo.Repository.AsQueryable().Any(x => x.ArtistID == _Artists.ArtistID && x.Datetime_Local >= DateTime.Now))
                                _Artists.OnTour = true;

                            _ArtistsRepo.Repository.Update(_Artists);

                            _unitOfWork.Commit();
                        }
                        #endregion

                        #endregion
                    });

                    _TotalNames.Add(_name.Name);
                }
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, _TotalNames.Count, "Scanned"));
            }
            catch (Exception ex)
            {
                LogHelper.CreateLog3(ex, Request);
                if (_unitOfWork.IsTransaction == true) _unitOfWork.RollBack();
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, ex.Message));
            }
        }

        [HttpPost]
        [Route("api/Artists/ScanUserLibrary")]
        public async Task<HttpResponseMessage> ScanUserLibrary(InputUserSanning input)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.OK, ModelState);
                }
                var serializer = new JavaScriptSerializer();
                serializer.RegisterConverters(new[] { new DynamicJsonConverter() });
                dynamic obj = null;

                GenericRepository<Users> _UsersRepo = new GenericRepository<Users>(_unitOfWork);

                Models.Users _Users = null;
                _Users = _UsersRepo.Repository.Get(p => p.UserID == input.UserID);

                if (_Users == null || _Users.RecordStatus != RecordStatus.Active.ToString())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.UserNotFound, "ArtistDetail"));
                }

                string strThumbnailURLfordb = null;
                string strIamgeURLfordb = null;
                string strTempImageSave = null;

                string tempfilePath = WebConfigurationManager.AppSettings["SiteImgPath"].ToString() + @"\" + "TempImages" + @"\";

                string _SiteRoot = WebConfigurationManager.AppSettings["SiteImgPath"];
                string _SiteURL = WebConfigurationManager.AppSettings["SiteImgURL"];

                string _MusicGrapgh_api_key = ConfigurationManager.AppSettings["MusicGrapgh_api_key"].ToString();
                string _Instagram_access_token = ConfigurationManager.AppSettings["instagram.access_token"].ToString();

                string _Eventful_app_key = ConfigurationManager.AppSettings["Eventful_app_key"].ToString();

                string _SeatGeek_client_id = ConfigurationManager.AppSettings["SeatGeek_client_id"].ToString();
                string _SeatGeek_client_secret = ConfigurationManager.AppSettings["SeatGeek_client_secret"].ToString();

                string _Spotify_Country = ConfigurationManager.AppSettings["Spotify_Country"].ToString();

                string _result;

                string _Performer_ID = null;
                string _strEvent = null;

                Models.Venue _VenuEntity = null;
                Models.TourDate _TourDateEntity = null;
                Models.ArtistRelated _ArtistRelatedEntity = null;

                string _MG_Artist_ID = null;
                List<string> _TotalNames = new List<string>();

                GenericRepository<Artists> _ArtistsRepo = new GenericRepository<Artists>(_unitOfWork);
                GenericRepository<ArtistPhotos> _ArtistPhotosRepo = new GenericRepository<ArtistPhotos>(_unitOfWork);
                GenericRepository<ArtistRelated> _ArtistRelatedRepo = new GenericRepository<ArtistRelated>(_unitOfWork);
                GenericRepository<TourDate> _TourDateRepo = new GenericRepository<TourDate>(_unitOfWork);
                GenericRepository<Venue> _VenueRepo = new GenericRepository<Venue>(_unitOfWork);
                GenericRepository<ArtistsNotLatin> _ArtistNotLatinRepo = new GenericRepository<ArtistsNotLatin>(_unitOfWork);

                GenericRepository<UserArtists> _UserArtistsRepo = new GenericRepository<UserArtists>(_unitOfWork);
                GenericRepository<UserTourDate> _UserTourDateRepo = new GenericRepository<UserTourDate>(_unitOfWork);

                //Remove duplicates from artist list
                var names = input.ArtistNames.Select(x => x.Name);
                var uniqueNameList = names.Distinct().ToList();

                GenericRepository<MusicSource> _MusicSourceRepo = new GenericRepository<MusicSource>(_unitOfWork);
                Models.MusicSource _MusicSource = null;
                _MusicSource = _MusicSourceRepo.Repository.Get(p => p.UserID == input.UserID && p.DeviceType == input.DeviceType.ToString() && p.MsourceID == input.MSourceID);

                if (_MusicSource == null)
                {
                    _MusicSource = new MusicSource();
                    _MusicSource.UserID = input.UserID;
                    _MusicSource.Source = input.MusicSource.ToString();
                    _MusicSource.MsourceID = input.MSourceID;
                    _MusicSource.DeviceType = input.DeviceType;
                    _MusicSource.CreatedDate = DateTime.Now;
                    _MusicSource.ModifiedDate = DateTime.Now;
                    _MusicSource.RecordStatus = RecordStatus.Active.ToString();

                    _MusicSourceRepo.Repository.Add(_MusicSource);
                }
                else
                {
                    _MusicSource.ModifiedDate = DateTime.Now;
                    _MusicSourceRepo.Repository.Update(_MusicSource);
                }

                //Scanning the User Libraries
                foreach (var n in uniqueNameList)
                {
                    myArtistName _name = new myArtistName();
                    _name.Name = n;

                    bool isLatin = false;
                    TimeSpan span = TimeSpan.Zero;
                    Models.Artists _Artists = null;
                    Models.UserArtists _UserArtists = null;
                    Models.UserTourDate _UserTourDate = null;

                    _unitOfWork.StartTransaction();

                    _Artists = _ArtistsRepo.Repository.Get(p => p.ArtistName.ToLower() == _name.Name.ToLower());

                    //Get the artist information if found in Local DB and informatino is not more then 24 hours old

                    if (_Artists != null)
                    {
                        span = DateTime.Now - Convert.ToDateTime(_Artists.ModifiedDate);
                        //if (span.Hours < 24 && span.Days == 0)
                        //{
                        //    _unitOfWork.RollBack();
                        //    continue;
                        //}

                    }

                    #region "Get Artist Info by Artist Name"

                    HttpWebRequest httpWebRequest;
                    HttpWebResponse httpResponse;

                    if (_Artists == null)
                    {
                        string spotifyId = "";

                        if (_ArtistNotLatinRepo.Repository.AsQueryable().Any(x => x.ArtistName == _name.Name))
                        {
                            _unitOfWork.RollBack();
                            continue;
                        }

                        var _Search_ByName = await MusicGrapgh_GetArtistByName_Asyn(_name.Name.ToString()); // await for the funtion to completed

                        if (_Search_ByName != null)
                        {
                            if (_Search_ByName.data.Count > 0)
                            {
                                if (_Search_ByName.data.Count > 1)
                                {
                                    _Search_ByName.data = _Search_ByName.data.OrderByDescending(x => !String.IsNullOrEmpty(x.spotify_id)).ToList();
                                    _Search_ByName.data.RemoveRange(1, _Search_ByName.data.Count - 1);
                                }

                                foreach (MusicGraph.Datum _datum in _Search_ByName.data)
                                {
                                    _Artists = _ArtistsRepo.Repository.Get(p => p.Musicgraph_ID == _datum.id);
                                    if (_Artists != null)
                                    {
                                        isLatin = true;
                                        _MG_Artist_ID = _datum.id;

                                    }
                                    else
                                    {
                                        if (_name.Name.ToString().ToLower().Equals(_datum.name.ToString().ToLower()))
                                        {
                                            //Check to filter Latin artist only
                                            if (_GenreFilter.Contains(_datum.main_genre))
                                                isLatin = true;
                                            else
                                                isLatin = CheckMusicGraphLatin(_datum.id, _unitOfWork);

                                            spotifyId = _datum.spotify_id;

                                            _MG_Artist_ID = _datum.id;
                                            break;
                                        }
                                    }

                                }

                                if (_MG_Artist_ID == null)
                                {
                                    _unitOfWork.RollBack();
                                    continue;
                                }
                            }
                            else
                            {
                                _unitOfWork.RollBack();
                                continue;
                            }



                        }
                        else
                        {
                            _unitOfWork.RollBack();
                            continue;
                        }


                        //If music graph main genre is not latin then check seat geek sub genres for latin
                        if (!isLatin)
                        {
                            isLatin = await SeatGeek_CheckLatinGenre_Asyn(_name.Name, _unitOfWork);

                            if (!isLatin)
                            {
                                isLatin = CheckLastResortSpotifyGenre(spotifyId);

                                if (!isLatin)
                                {
                                    if (!_ArtistNotLatinRepo.Repository.AsQueryable().Any(x => x.ArtistName == _name.Name))
                                    {
                                        _ArtistNotLatinRepo.Repository.Add(new ArtistsNotLatin() { ArtistName = _name.Name });
                                        _unitOfWork.Commit();
                                        _unitOfWork.StartTransaction();
                                    }

                                    _MG_Artist_ID = null;
                                    _unitOfWork.RollBack();
                                    continue;
                                }
                            }
                        }

                    }

                    #endregion

                    #region "Get Artist Info using ID"

                    MusicGraph1.Search_ByID _Search_ByID = null;

                    if (_Artists == null)
                    {

                        _Search_ByID = await MusicGrapgh_GetArtistByID_Asyn(_MG_Artist_ID);// await for the funtion to completed


                        if (_Search_ByID == null)
                        {
                            _unitOfWork.RollBack();
                            continue;
                        }

                        if (_Search_ByID.status.message != "Success")
                        {
                            _unitOfWork.RollBack();
                            continue;
                        }

                        //If musicgraph ID already exists then rollback
                        if (_ArtistsRepo.Repository.AsQueryable().Any(p => p.Musicgraph_ID == _Search_ByID.data.id))
                        {
                            _unitOfWork.RollBack();
                            continue;
                        }


                    }
                    #endregion

                    await Task.Factory.StartNew(() =>
                    {
                        #region "Extract data from Thirdparty API to Save/Update"

                        #region "Add New Rec"
                        //Start- Saving New Data 
                        if (_Artists == null)
                        {
                            _Artists = new Artists();
                            _Artists.ArtistName = _Search_ByID.data.name;// Regex.Replace(_Search_ByID.data.name, "[^A-Za-z0-9 _]", "");

                            _Artists.Gender = _Search_ByID.data.gender;
                            _Artists.Decade = _Search_ByID.data.decade;
                            _Artists.Main_Genre = _Search_ByID.data.main_genre;

                            _Artists.Musicgraph_ID = !String.IsNullOrEmpty(_Search_ByID.data.id) ? _Search_ByID.data.id : _Search_ByID.data.name;
                            _Artists.Artist_Ref_ID = _Search_ByID.data.artist_ref_id;
                            _Artists.Musicbrainz_ID = _Search_ByID.data.musicbrainz_id;
                            _Artists.Spotify_ID = _Search_ByID.data.spotify_id;
                            _Artists.Youtube_ID = _Search_ByID.data.youtube_id;
                            _Artists.Alternate_Names = _Search_ByID.data.alternate_names != null && _Search_ByID.data.alternate_names.Count > 0 ? _Search_ByID.data.alternate_names[0] : "";



                            _Artists.RecordStatus = RecordStatus.MusicGraph.ToString();
                            _Artists.CreatedDate = DateTime.Now;
                            _Artists.ModifiedDate = DateTime.Now;

                            _ArtistsRepo.Repository.Add(_Artists);

                            GetProfileImageFromSpotifyFeed(_Artists);

                            _UserArtists = new UserArtists();
                            _UserArtists.UserID = input.UserID;
                            _UserArtists.ArtistID = _Artists.ArtistID;
                            _UserArtists.CreatedDate = DateTime.Now;
                            _UserArtists.RecordStatus = RecordStatus.Active.ToString();

                            _UserArtistsRepo.Repository.Add(_UserArtists);
                            _unitOfWork.Commit();
                            _unitOfWork.StartTransaction();

                            #region "Get Similar Artists (dont need this block while just updating the records)"
                            Task<MusicGraph5.GetSimilarArtists_ByID> _GetSimilarArtists_ByID = MusicGrapgh_GetSimilarArtists_Asyn(_Artists.Musicgraph_ID, _Artists.ArtistID, _unitOfWork);
                            #endregion
                            _GetSimilarArtists_ByID.Wait();


                            #region "Get Artist Matrics (dont need this block while just updating the records)"
                            Task<MusicGraph2.ArtistMatrics_ByID> _ArtistMatrics_ByID = MusicGrapgh_GetArtistMatrics_Asyn(_Artists.Musicgraph_ID, _Artists.ArtistID, _unitOfWork);

                            _ArtistMatrics_ByID.Wait(); // wait for the task to be completed

                            //Get Instagram ID from the MusicGraph matrcis 
                            if (_ArtistMatrics_ByID.Result.data.instagram != null && _ArtistMatrics_ByID.Result.data.instagram.url != null && _ArtistMatrics_ByID.Result.data.instagram.url != "")
                            {
                                _Artists.Instagram_Url = _ArtistMatrics_ByID.Result.data.instagram.url;
                                string _instaGram_ID = _ArtistMatrics_ByID.Result.data.instagram.url.Replace("http:", "https:").Replace("https://instagram.com/", "").Replace("/", "");
                                _Artists.Instagram_ID = _instaGram_ID;
                            }

                            #endregion



                            #region "SeatGeek Api Implementation"
                            Task<bool> _seatGeek = SeatGeek_GetEventByArtistName_Asyn(input.UserID, _Artists, _unitOfWork, true, true);
                            #endregion

                            _seatGeek.Wait();

                            _unitOfWork.Commit();
                            _unitOfWork.StartTransaction();

                            #region "Eventful API Implementation"
                            Task<bool> _Eventful = EventFul_GetEventInfo_Asyn(input.UserID, _Artists, _unitOfWork, true);
                            #endregion

                            _Eventful.Wait();

                            #region "Instagram Api Implementation"
                            Task<bool> _Instagram = Instagram_GetPictures_Asyn(input.UserID, _Artists, _ArtistMatrics_ByID.Result, _unitOfWork, true, null);
                            #endregion

                            _Instagram.Wait();

                            #region "Spotify Api Implementation
                            Task<bool> _Spotify = Spotify_GetSongInfo_Asyn(input.UserID, _Artists, _unitOfWork, true);
                            #endregion

                            _Spotify.Wait();




                            if (_TourDateRepo.Repository.AsQueryable().Any(x => x.ArtistID == _Artists.ArtistID && x.Datetime_Local >= DateTime.Now))
                            {
                                _Artists.OnTour = true;
                            }

                            string MusicGraphBio = "";
                            if (!String.IsNullOrEmpty(_Artists.Musicgraph_ID))
                                MusicGraphBio = GetMusicGraphBio(_Artists.Musicgraph_ID);
                            if (!String.IsNullOrEmpty(MusicGraphBio))
                                _Artists.About = MusicGraphBio;

                            _ArtistsRepo.Repository.Update(_Artists);
                            _unitOfWork.Commit();

                        }//End- Saving New Data 

                        #endregion

                        #region "Update Existing Record"
                        //Start - Update the data
                        else
                        {


                            _Artists.ModifiedDate = DateTime.Now;

                            _UserArtists = _UserArtistsRepo.Repository.Get(p => p.UserID == input.UserID && p.ArtistID == _Artists.ArtistID);

                            if (_UserArtists == null)
                            {
                                _UserArtists = new UserArtists();
                                _UserArtists.UserID = input.UserID;
                                _UserArtists.ArtistID = _Artists.ArtistID;
                                _UserArtists.CreatedDate = DateTime.Now;
                                _UserArtists.RecordStatus = RecordStatus.Active.ToString();

                                _UserArtistsRepo.Repository.Add(_UserArtists);
                            }

                            if (span.Days > 1 || span.Hours > 24)
                            {
                                #region "Instagram Api Implementation"
                                Task<bool> _Instagram = Instagram_GetPictures_Asyn(input.UserID, _Artists, null, _unitOfWork, false, null);
                                #endregion

                                _Instagram.Wait();

                                #region "SeatGeek Api Implementation"
                                Task<bool> _seatGeek = SeatGeek_GetEventByArtistName_Asyn(input.UserID, _Artists, _unitOfWork, false, true);
                                #endregion

                                _seatGeek.Wait();

                                #region "Eventful API Implementation"
                                Task<bool> _Eventful = EventFul_GetEventInfo_Asyn(input.UserID, _Artists, _unitOfWork, false);
                                #endregion

                                _Eventful.Wait();

                                #region "Spotify Api Implementation
                                Task<bool> _Spotify = Spotify_GetSongInfo_Asyn(input.UserID, _Artists, _unitOfWork, false);
                                #endregion

                                _Spotify.Wait();


                            }

                            if (_TourDateRepo.Repository.AsQueryable().Any(x => x.ArtistID == _Artists.ArtistID && x.Datetime_Local >= DateTime.Now))
                                _Artists.OnTour = true;

                            _ArtistsRepo.Repository.Update(_Artists);


                            _unitOfWork.Commit();


                        }
                        #endregion

                        #endregion
                    });

                    _TotalNames.Add(_name.Name);
                }

                #region "Commented Code"
                //GenericRepository<MusicSource> _MusicSourceRepo = new GenericRepository<MusicSource>(_unitOfWork);
                //Models.MusicSource _MusicSource = null;
                //_MusicSource = _MusicSourceRepo.Repository.Get(p => p.UserID == input.UserID && p.DeviceType == input.DeviceType.ToString() && p.MsourceID == input.MSourceID);

                //if (_MusicSource == null)
                //{
                //    _MusicSource = new MusicSource();
                //    _MusicSource.UserID = input.UserID;
                //    _MusicSource.Source = input.MusicSource.ToString();
                //    _MusicSource.MsourceID = input.MSourceID;
                //    _MusicSource.DeviceType = input.DeviceType;
                //    _MusicSource.CreatedDate = DateTime.Now;
                //    _MusicSource.ModifiedDate = DateTime.Now;
                //    _MusicSource.RecordStatus = RecordStatus.Active.ToString();

                //    _MusicSourceRepo.Repository.Add(_MusicSource);

                //}
                //else
                //{
                //    _MusicSource.ModifiedDate = DateTime.Now;
                //    _MusicSourceRepo.Repository.Update(_MusicSource);
                //}
                #endregion

                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, _TotalNames.Count, "Scanned"));
            }
            catch (Exception ex)
            {
                LogHelper.CreateLog3(ex, Request);
                if (_unitOfWork.IsTransaction == true) _unitOfWork.RollBack();
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, ex.Message));

            }
        }

        public ViewArtist GetArtistDetail_ByID(Artists _Artist, Int32 UserID)
        {

            ViewArtist _ViewArtist = new ViewArtist();
            GenericRepository<TourDate> _TourDateRepo = new GenericRepository<TourDate>(_unitOfWork);
            GenericRepository<ArtistPhotos> _ArtistPhotosRepo = new GenericRepository<ArtistPhotos>(_unitOfWork);
            GenericRepository<ArtistRelated> _ArtistRelatedRepo = new GenericRepository<ArtistRelated>(_unitOfWork);
            GenericRepository<Venue> _VenueRepo = new GenericRepository<Venue>(_unitOfWork);
            GenericRepository<UserArtists> _UserArtistsRepo = new GenericRepository<UserArtists>(_unitOfWork);
            GenericRepository<Users> _UsersRepo = new GenericRepository<Users>(_unitOfWork);

            var _users = _UsersRepo.Repository.GetById(UserID);

            Models.UserArtists _UserArtists = null;

            if (String.IsNullOrEmpty(_Artist.ImageURL))
                GetProfileImageFromSpotifyFeed(_Artist);

            _ViewArtist.ArtistID = _Artist.ArtistID;
            _ViewArtist.ArtistName = _Artist.ArtistName;
            _ViewArtist.About = _Artist.AboutES != null && _users.UserLanguage == EUserLanguage.ES.ToString() ? _Artist.AboutES : _Artist.About;

            if (_ViewArtist.About == null)
                _ViewArtist.About = "";

            _ViewArtist.ImageURL = _Artist.ImageURL ?? "";
            _ViewArtist.BannerImage_URL = _Artist.ImageURL ?? "";

            _ViewArtist.OnTour = _Artist.OnTour;
            _ViewArtist.Gender = _Artist.Gender ?? "";
            _ViewArtist.Main_Genre = _Artist.Main_Genre ?? "";
            _ViewArtist.Decade = _Artist.Decade ?? "";
            _ViewArtist.Alternate_Names = _Artist.Alternate_Names ?? "";
            _ViewArtist.CreatedDate = _Artist.CreatedDate;
            _ViewArtist.ModifiedDate = _Artist.ModifiedDate;
            _ViewArtist.RecordStatus = _Artist.RecordStatus;

            _UserArtists = _UserArtistsRepo.Repository.Get(p => p.ArtistID == _Artist.ArtistID && p.UserID == UserID && p.RecordStatus != RecordStatus.Deleted.ToString());

            if (_UserArtists == null)
            {
                _ViewArtist.IsTracking = false;
            }
            else
            {
                _ViewArtist.IsTracking = true;
            }


            _ViewArtist.Spotify_URL = _Artist.Spotify_Url ?? "";
            _ViewArtist.Spotify_URL_Name = _Artist.Spotify_URL_Name ?? "";
            _ViewArtist.Spotify_Follow = "http://open.spotify.com/artist/" + _Artist.Spotify_ID;
            _ViewArtist.Instagram_Tag = _Artist.Instagram_Tag != null && _Artist.Instagram_Tag != "" ? "#" + _Artist.Instagram_Tag :
                                            (_Artist.ArtistName != "" && _Artist.ArtistName != null) ? "#" + _Artist.ArtistName : "";


            #region "Commented Code"
            //    Models.UserTracking _UserTracking = null;
            //_UserTracking = _UserTrackingRepo.Repository.Get(p => p.ArtistID == _Artist.ArtistID);

            //if (_UserTracking != null)
            //{
            //    _ViewArtist.IsTracking = true;
            //}
            //else {
            //    _ViewArtist.IsTracking = false;
            //}
            #endregion

            //Get Tour Dates
            _ViewArtist.TourDates = (from A in _TourDateRepo.Repository.GetAll(p => p.ArtistID == _Artist.ArtistID)
                                     join B in _VenueRepo.Repository.GetAll() on A.VenueID equals B.VenueID
                                     where !A.IsDeleted && Helper.GetUTCDateTime(DateTime.Now) < A.Tour_Utcdate && B.VenueLat.HasValue && B.VenueLong.HasValue && B.VenueLat.Value != 0 && B.VenueLong != 0
                                     select new viewTour
                                     {
                                         Address = B.Address ?? "",
                                         Announce_Date = A.Announce_Date != null ? Convert.ToDateTime(A.Announce_Date).ToString("d") : "",
                                         Datetime_Local = Convert.ToDateTime(A.Datetime_Local).ToString("g"),
                                         Display_Location = B.Display_Location ?? "",
                                         Extended_Address = B.Extended_Address ?? "",
                                         ImageURL = B.ImageURL ?? "",
                                         Postal_Code = B.Postal_Code ?? "",
                                         Slug = B.Slug ?? "",
                                         Timezone = B.Timezone ?? "",
                                         Tour_Utcdate = Convert.ToDateTime(A.Tour_Utcdate).ToString("d"),
                                         TourDateID = A.TourDateID,
                                         VenueCity = B.VenueCity ?? "",
                                         VenueCountry = B.VenueCountry ?? "",
                                         VenueID = B.VenueID,
                                         VenueLat = B.VenueLat,
                                         VenueLong = B.VenueLong,
                                         VenueName = B.VenueName ?? "",
                                         VenueState = B.VenueState ?? ""
                                     }).OrderBy(p => Convert.ToDateTime(p.Datetime_Local)).ToList();

            if (_ViewArtist.TourDates.Count > 0)
                _ViewArtist.OnTour = true;
            else
                _ViewArtist.OnTour = false;


            //Get HashTags pictures
            //GetImagesFromInstagramFeed(_Artist);

            _ViewArtist.TourPhotos = (from A in _ArtistPhotosRepo.Repository.GetAll(p => p.ArtistID == _Artist.ArtistID && p.RecordStatus == RecordStatus.Active.ToString())
                                      select new viewTourPhoto
                                      {
                                          ImageURL = A.ImageUrl,
                                          ImageThumbnailURL = A.ImageThumbnailUrl ?? "",
                                          HashTagName = A.HashTagName ?? "",
                                          PhotoID = A.PhotoID
                                      }).ToList();

            //Get Similar Artists
            _ViewArtist.ArtistRelated = (from A in _ArtistRelatedRepo.Repository.GetAll(p => p.ArtistID == _Artist.ArtistID)
                                         select new viewRelated
                                         {
                                             Musicgraph_ID = A.Musicgraph_ID,
                                             RelatedArtistName = A.RelatedArtistName ?? "",
                                             Similarity = A.Similarity
                                         }).ToList();

            return _ViewArtist;
        }

        #region "Commented Code"
        //[HttpPost]
        //[Route("api/Artists/UpdateArtsistTracking")]
        //public HttpResponseMessage UpdateArtsistTracking(InputUpdateArtistTracking input)
        //{
        //    try
        //    {
        //        if (!ModelState.IsValid)
        //        {
        //            return Request.CreateErrorResponse(HttpStatusCode.OK, ModelState);
        //        }

        //        GenericRepository<Artists> _ArtistsRepo = new GenericRepository<Artists>(_unitOfWork);
        //        GenericRepository<UserArtists> _UserArtistsRepo = new GenericRepository<UserArtists>(_unitOfWork);

        //        Models.Artists _Artists = null;
        //        Models.UserArtists _UserArtists = null;

        //        _Artists = _ArtistsRepo.Repository.Get(p => p.ArtistID == input.ArtistID);
        //        if (_Artists == null)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.ArtistNotFound));
        //        }

        //        _UserArtists = _UserArtistsRepo.Repository.Get(p => p.ArtistID == input.ArtistID && p.UserID == input.UserID);

        //        if (input.Tracking == true)
        //        {
        //            if (_UserArtists == null)
        //            {

        //                _UserArtists = new UserArtists();
        //                _UserArtists.UserID = input.UserID;
        //                _UserArtists.ArtistID = input.ArtistID;
        //                _UserArtists.CreatedDate = DateTime.Now;
        //                _UserArtists.RecordStatus = RecordStatus.Active.ToString();

        //                _UserArtistsRepo.Repository.Add(_UserArtists);
        //            }

        //        }
        //        else
        //        {
        //            if (_UserArtists != null)
        //            {
        //                _UserArtistsRepo.Repository.DeletePermanent(_UserArtists.UserArtistsID);
        //            }
        //        }

        //        return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, null));

        //    }
        //    catch (Exception ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, ex.Message));

        //    }
        //}
        #endregion

        [HttpGet]
        [Route("api/Artists/GetArtistsList")]
        public HttpResponseMessage GetArtistsList(Int32 UserID)
        {
            try
            {
                if (UserID == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, "UserID required"));
                }

                GenericRepository<Users> _UsersRepo = new GenericRepository<Users>(_unitOfWork);
                Models.Users _Users = null;

                _Users = _UsersRepo.Repository.Get(p => p.UserID == UserID);

                if (_Users == null || _Users.RecordStatus != RecordStatus.Active.ToString())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.UserNotFound, "EventsDetail"));
                }

                GenericRepository<Artists> _ArtistsRepo = new GenericRepository<Artists>(_unitOfWork);
                GenericRepository<UserArtists> _UserArtistsRepo = new GenericRepository<UserArtists>(_unitOfWork);
                GenericRepository<TourDate> _TourDateRepo = new GenericRepository<TourDate>(_unitOfWork);

                // check if user artist list is not empty, if empty then fill it up with default artists
                var _check = _UserArtistsRepo.Repository.GetAll(p => p.UserID == UserID);
                if (_check != null && _check.Count < 15)
                {
                    var _artistIds = _ArtistsRepo.Repository.GetAll(p => p.Isdefault == true && p.RecordStatus != RecordStatus.Deleted.ToString()).Select(p => p.ArtistID);
                    foreach (Int32 _artistid in _artistIds)
                    {
                        var _AlreadyExists = _UserArtistsRepo.Repository.Get(p => p.UserID == UserID && p.ArtistID == _artistid);

                        if (_AlreadyExists == null)
                        {
                            var _UserArtists = new UserArtists();
                            _UserArtists.UserID = UserID;
                            _UserArtists.ArtistID = _artistid;
                            _UserArtists.CreatedDate = DateTime.Now;
                            _UserArtists.RecordStatus = RecordStatus.Active.ToString();
                            _UserArtistsRepo.Repository.Add(_UserArtists);
                        }
                    }
                }


                var _list = (from A in _UserArtistsRepo.Repository.GetAll(p => p.UserID == UserID)
                             join B in _ArtistsRepo.Repository.GetAll() on A.ArtistID equals B.ArtistID
                             select new ViewArtistList
                             {
                                 ArtistID = B.ArtistID,
                                 ArtistName = B.ArtistName ?? "",
                                 ImageURL = B.ThumbnailURL ?? B.ImageURL ?? "",
                                 BannerImage_URL = B.BannerImage_URL ?? "",
                                 OnTour = B.OnTour
                             }).ToList();

                foreach (var l in _list)
                {
                    var onTour = _TourDateRepo.Repository.AsQueryable().Any(x => x.ArtistID == l.ArtistID && x.Tour_Utcdate.Value > DateTime.UtcNow && !x.IsDeleted);
                    l.OnTour = onTour;
                }

                _list = _list.OrderBy(p => p.FirstLetter).ToList();

                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, _list, "ArtistList"));

            }
            catch (Exception ex)
            {
                LogHelper.CreateLog3(ex, Request);
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Exception, ex.Message));
            }


        }


        [HttpPost]
        [Route("api/Artists/UpdateTrackArtist")]
        public HttpResponseMessage UpdateTrackArtist(InputTrackArtist input)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.OK, ModelState);
                }

                GenericRepository<Users> _UsersRepo = new GenericRepository<Users>(_unitOfWork);
                GenericRepository<Artists> _ArtistsRepo = new GenericRepository<Artists>(_unitOfWork);
                GenericRepository<UserArtists> _UserArtistsRepo = new GenericRepository<UserArtists>(_unitOfWork);


                Models.Users _Users = null;
                Models.Artists _Artists = null;
                Models.UserArtists _UserArtists = null;


                _Artists = _ArtistsRepo.Repository.Get(p => p.ArtistID == input.ArtistID);
                if (_Artists == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.ArtistNotFound));
                }

                _UserArtists = _UserArtistsRepo.Repository.Get(p => p.ArtistID == input.ArtistID && p.UserID == input.UserID);

                //Tracking
                if (input.TrackArtist.ToString() == ETracking.Tracking.ToString())
                {
                    if (_UserArtists == null)
                    {

                        _UserArtists = new UserArtists();
                        _UserArtists.UserID = input.UserID;
                        _UserArtists.ArtistID = input.ArtistID;
                        _UserArtists.CreatedDate = DateTime.Now;
                        _UserArtists.RecordStatus = RecordStatus.Active.ToString();

                        _UserArtistsRepo.Repository.Add(_UserArtists);
                    }


                }
                else
                {

                    if (_UserArtists != null)
                    {
                        _UserArtistsRepo.Repository.DeletePermanent(_UserArtists.UserArtistsID);
                    }

                }


                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, input.TrackArtist.ToString(), "TrackArtist"));

            }
            catch (Exception ex)
            {
                LogHelper.CreateLog3(ex, Request);
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, ex.Message, "TrackArtist"));

            }
        }

        #endregion


        #region "Events"

        [HttpGet]
        [Route("api/Events/GetEventsList")]
        public HttpResponseMessage GetEventsList(Int32 UserID, double? Lat, double? lon, decimal? radius, string from, string to)
        {
            try
            {
                if (UserID == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, "UserID required"));
                }

                GenericRepository<Users> _UsersRepo = new GenericRepository<Users>(_unitOfWork);
                Models.Users _Users = null;

                _Users = _UsersRepo.Repository.Get(p => p.UserID == UserID);

                if (_Users == null || _Users.RecordStatus != RecordStatus.Active.ToString())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.UserNotFound, "EventList"));
                }

                GenericRepository<Artists> _ArtistsRepo = new GenericRepository<Artists>(_unitOfWork);
                GenericRepository<Venue> _VenueRepo = new GenericRepository<Venue>(_unitOfWork);
                GenericRepository<TourDate> _TourDateRepo = new GenericRepository<TourDate>(_unitOfWork);
                GenericRepository<UserTourDate> _UserTourDateRepo = new GenericRepository<UserTourDate>(_unitOfWork);


                //  DateTime? _st = Helper.GetUTCDateTime(DateTime.Now);


                var _list = (from A1 in _UserTourDateRepo.Repository.GetAll(p => p.UserID == UserID)
                             join A in _TourDateRepo.Repository.GetAll() on A1.TourDateID equals A.TourDateID
                             join B in _ArtistsRepo.Repository.GetAll() on A.ArtistID equals B.ArtistID
                             join C in _VenueRepo.Repository.GetAll() on A.VenueID equals C.VenueID
                             where Helper.GetUTCDateTime(DateTime.Now) < A.Tour_Utcdate && !A.IsDeleted
                             select new ViewEventsList
                             {
                                 TourDateID = A.TourDateID,
                                 ArtistID = A.ArtistID.Value,
                                 ArtistName = B.ArtistName,
                                 Datetime_dt =A.Datetime_Local,
                                 ImageURL = B.ImageURL,
                                 BannerImage_URL = B.BannerImage_URL,
                                 OnTour = B.OnTour,
                                 VenueName = C.VenueName,
                                 VenuID = C.VenueID
                             }).ToList();

                _list = _list.OrderBy(p => p.Datetime_Local).ToList();

                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, _list, "EventList"));

            }
            catch (Exception ex)
            {
                LogHelper.CreateLog3(ex, Request);
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Exception, ex.Message));
            }


        }


        private class dbEventSearchModel
        {
            public string id { get; set; }
            public string name { get; set; }
            public string type { get; set; }
        }

        private List<dbEventSearchModel> DbEventLookup(string s)
        {
            Dictionary<string, object> dblookups = new Dictionary<string, object>();


            List<SeatGeek3.ViewEventLookup> _ViewDBLookup = new List<SeatGeek3.ViewEventLookup>();
            GenericRepository<TourDate> _TourDateRepo = new GenericRepository<TourDate>(_unitOfWork);
            GenericRepository<Venue> _VenueRepo = new GenericRepository<Venue>(_unitOfWork);
            GenericRepository<Artists> _ArtistsRepo = new GenericRepository<Artists>(_unitOfWork);


            string pattern = @"[^a-zA-Z0-9áéíñóúüÁÉÍÑÓÚÜ&\s]";
            var search = Regex.Replace(s, pattern, "");

            var venueByName = _VenueRepo.Repository.AsQueryable().Where(x => x.VenueName.Length < 60 && x.VenueName.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0).Select(x => new dbEventSearchModel { id = x.VenueID.ToString(), name = x.VenueName, type = EventSearchType.Venue.ToString() }).ToList();
            //var venueByCity = _VenueRepo.Repository.AsQueryable().Where(x => x.VenueCity.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0).Select(x => new dbEventSearchModel { id = x.VenueID.ToString(), name = x.VenueCity, type = EventSearchType.Venue.ToString() }).ToList();
            var artistByName = _ArtistsRepo.Repository.AsQueryable().Where(x => x.ArtistName.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0).Select(x => new dbEventSearchModel { id = x.ArtistID.ToString(), name = x.ArtistName, type = EventSearchType.Artist.ToString() }).ToList();
            //var eventByName = _TourDateRepo.Repository.AsQueryable().Where(x => x.EventName != null && x.EventName.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0).Select(x => new dbEventSearchModel { id = x.TourDateID.ToString(), name = x.EventName, type = EventSearchType.Event.ToString() }).ToList();


            var tourDates = venueByName.Union(artistByName).GroupBy(x => x.name).Select(g => g.FirstOrDefault()).ToList();

            return tourDates;


        }

        private List<ViewEventsPreList> DbEventListCheck(Int32 userId, string id, string name, string type)
        {
            List<SeatGeek3.ViewEventLookup> _ViewDBLookup = new List<SeatGeek3.ViewEventLookup>();
            GenericRepository<TourDate> _TourDateRepo = new GenericRepository<TourDate>(_unitOfWork);
            GenericRepository<Venue> _VenueRepo = new GenericRepository<Venue>(_unitOfWork);
            GenericRepository<Artists> _ArtistsRepo = new GenericRepository<Artists>(_unitOfWork);

            GenericRepository<UserTourDate> _UserTourDateRepo = new GenericRepository<UserTourDate>(_unitOfWork);

            List<TourDate> dbTourDateList = null;
            List<ViewEventsPreList> dbViewEventsList = new List<ViewEventsPreList>();

            if (type == EventSearchType.Artist.ToString())
            {
                dbTourDateList = _TourDateRepo.Repository.AsQueryable().Where(x => x.ArtistID.ToString() == id)
                    .OrderBy(x => x.Datetime_Local).ToList();
            }
            else if (type == EventSearchType.Venue.ToString())
            {
                dbTourDateList = _TourDateRepo.Repository.AsQueryable().Where(x => x.VenueID.ToString() == id)
                    .OrderBy(x => x.Datetime_Local).ToList();
            }
            else
            {
                dbTourDateList = _TourDateRepo.Repository.AsQueryable().Where(x => x.EventName == name)
                    .OrderBy(x => x.Datetime_Local).ToList();
            }



            foreach (var d in dbTourDateList)
            {
                //date
                //if (dbTourDateList.Count(x => x.Datetime_Local == d.Datetime_Local) > 1 && d.RecordStatus == RecordStatus.Eventful.ToString())
                //{ continue; }
                //else
                //{
                var _list = (from A in _TourDateRepo.Repository.GetAll(x => x.TourDateID == d.TourDateID)
                             join B in _ArtistsRepo.Repository.GetAll() on A.ArtistID equals B.ArtistID
                             join C in _VenueRepo.Repository.GetAll() on A.VenueID equals C.VenueID
                             where Helper.GetUTCDateTime(DateTime.Now) < A.Tour_Utcdate
                             select new ViewEventsPreList
                             {
                                 TourDateID = A.TourDateID,
                                 ArtistID = A.ArtistID.Value,
                                 ArtistName = B.ArtistName,
                                 Datetime_dt = A.Datetime_Local,
                                 ImageURL = B.ImageURL,
                                 BannerImage_URL = B.BannerImage_URL,
                                 OnTour = B.OnTour,
                                 VenueName = C.VenueName,
                                 VenuID = C.VenueID,
                                 VenueLat = C.VenueLat,
                                 VenueLon = C.VenueLong,
                                 IsDeleted = A.IsDeleted
                             }).ToList();


                dbViewEventsList.AddRange(_list);
                //}
            }


            return dbViewEventsList.OrderBy(x => x.Datetime_Local).ToList();

        }


        [HttpGet]
        [Route("api/Events/GetEventsSearchResult")]
        public HttpResponseMessage GetEventsSearchResult(string search, int UserID)
        {
            try
            {
                List<dbEventSearchModel> _eventDBLookup;
                GenericRepository<Users> _UsersRepo = new GenericRepository<Users>(_unitOfWork);
                GenericRepository<ArtistsNotLatin> _ArtistNotLatinRepo = new GenericRepository<ArtistsNotLatin>(_unitOfWork);

                Models.Users _Users = null;
                string pattern = @"[^a-zA-Z0-9&\s]";
                search = search ?? "";
                search = Regex.Replace(search, pattern, "");

                if (search.Length < 3)
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, "", "Events"));


                _Users = _UsersRepo.Repository.Get(p => p.UserID == UserID);

                if (_Users == null || _Users.RecordStatus != RecordStatus.Active.ToString())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.UserNotFound, "Events"));
                }
                search = search.ToLower();
                string strThumbnailURLfordb = null;
                string strIamgeURLfordb = null;
                string strTempImageSave = null;

                string tempfilePath = WebConfigurationManager.AppSettings["SiteImgPath"].ToString() + @"\" + "TempImages" + @"\";

                string _SiteRoot = WebConfigurationManager.AppSettings["SiteImgPath"];
                string _SiteURL = WebConfigurationManager.AppSettings["SiteImgURL"];


                string _MusicGrapgh_api_key = ConfigurationManager.AppSettings["MusicGrapgh_api_key"].ToString();
                string _Instagram_access_token = ConfigurationManager.AppSettings["instagram.access_token"].ToString();

                string _Eventful_app_key = ConfigurationManager.AppSettings["Eventful_app_key"].ToString();

                string _SeatGeek_client_id = ConfigurationManager.AppSettings["SeatGeek_client_id"].ToString();
                string _SeatGeek_client_secret = ConfigurationManager.AppSettings["SeatGeek_client_secret"].ToString();

                string _result;



                _eventDBLookup = DbEventLookup(search);


                if (_eventDBLookup == null || _eventDBLookup.Count() < 10)
                {
                    try
                    {
                        //Search for artist name
                        List<dbEventSearchModel> _ArtistList = new List<dbEventSearchModel>();
                        try
                        {

                            var httpMusicRequest = (HttpWebRequest)WebRequest.Create("http://api.musicgraph.com/api/v2/artist/suggest?api_key=" + _MusicGrapgh_api_key + "&limit=10&prefix=" + search);
                            httpMusicRequest.ContentType = "application/json";
                            httpMusicRequest.Method = "GET";

                            var httpMusicResponse = (HttpWebResponse)httpMusicRequest.GetResponse();
                            using (var streamReader = new StreamReader(httpMusicResponse.GetResponseStream()))
                            {
                                _result = streamReader.ReadToEnd();
                            }


                            // deserializing 
                            var _Search_ByName = JsonConvert.DeserializeObject<MusicGraph.Search_ByName>(_result);
                            bool isLatin = false;


                            if (_Search_ByName != null)
                            {
                                if (_Search_ByName.data.Count > 0)
                                {
                                    foreach (MusicGraph.Datum _datum in _Search_ByName.data)
                                    {
                                        if (_datum.name.Length < 30)
                                        {
                                            if (!_ArtistNotLatinRepo.Repository.AsQueryable().Any(x => x.ArtistName == _datum.name))
                                            {
                                                //Check to filter Latin artist only
                                                if (_GenreFilter.Contains(_datum.main_genre))
                                                    isLatin = true;
                                                else
                                                {
                                                    if (!isLatin)
                                                        isLatin = CheckMusicGraphLatin(_datum.id, _unitOfWork);
                                                    if (!isLatin)
                                                        isLatin = CheckSeatGeekLatin(_datum.name, _unitOfWork);
                                                    if (!isLatin)
                                                    {
                                                        isLatin = CheckLastResortSpotifyGenre(_datum.spotify_id);
                                                    }
                                                }

                                                if (isLatin)
                                                {
                                                    _ArtistList.Add(new dbEventSearchModel
                                                    {
                                                        id = _datum.id,
                                                        name = _datum.name,
                                                        type = EventSearchType.Artist.ToString()

                                                    });
                                                }
                                                else
                                                {
                                                    if (!_ArtistNotLatinRepo.Repository.AsQueryable().Any(x => x.ArtistName == _datum.name))
                                                        _ArtistNotLatinRepo.Repository.Add(new ArtistsNotLatin() { ArtistName = _datum.name });
                                                }
                                            }
                                        }

                                        isLatin = false;
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        { }

                        List<dbEventSearchModel> seatGeekVenueSearch = new List<dbEventSearchModel>();
                        seatGeekVenueSearch = SeatGeekVenueSearch(search);


                        if (seatGeekVenueSearch.Count() < 10)
                        {
                            //var venueWordMatches = new List<string>();
                            foreach (var d in _eventDBLookup)
                            {
                                if (seatGeekVenueSearch.Count() < 15)
                                {
                                    if (d.type == EventSearchType.Venue.ToString())
                                    {
                                        var str = d.name.Split(' ').FirstOrDefault(x => x.ToLower().Contains(search));
                                        if (!String.IsNullOrEmpty(str))
                                        {
                                            seatGeekVenueSearch = seatGeekVenueSearch.Union(SeatGeekVenueSearch(str)).ToList();
                                        }
                                    }
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }

                        _eventDBLookup = _eventDBLookup.Union(_ArtistList).Union(seatGeekVenueSearch).GroupBy(x => x.name).Select(g => g.FirstOrDefault()).ToList();

                    }
                    catch (Exception e)
                    { }


                }


                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, _eventDBLookup, "Events"));
            }
            catch (Exception ex)
            {
                LogHelper.CreateLog3(ex, Request);
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, ex.Message, "Events"));
            }
        }

        private List<dbEventSearchModel> SeatGeekVenueSearch(string search)
        {
            //search for venue name
            string _result;
            List<dbEventSearchModel> seatGeekVenueSearch = new List<dbEventSearchModel>();
            string _concat = SGAuth() + "&";

            var strEncoded = HttpUtility.UrlEncode(search);
            _concat = _concat + "name=" + strEncoded + "&per_page=10&per_page=1";

            var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://api.seatgeek.com/2/venues" + _concat);

            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "GET";

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                _result = streamReader.ReadToEnd();
            }


            //LogHelper.CreateLog(_result);

            //_result = "";
            var serializer = new JavaScriptSerializer();
            serializer.RegisterConverters(new[] { new DynamicJsonConverter() });
            dynamic obj = null;


            // deserializing 
            var _Get_Events_ByLat = JsonConvert.DeserializeObject<SeatGeek3.Get_Venues>(_result);
            //dynamic _Get_Events_ByLat = serializer.Deserialize(_result, typeof(object));


            if (_Get_Events_ByLat != null)
            {
                if (_Get_Events_ByLat.venues != null && _Get_Events_ByLat.venues.Count > 0)
                {
                    foreach (SeatGeek3.Venue _Event in _Get_Events_ByLat.venues)
                    {
                        if (_Event != null && _Event.name != null && _Event.name.Length < 30)
                        {
                            if (!seatGeekVenueSearch.Any(x => x.name == _Event.name))
                            {
                                seatGeekVenueSearch.Add(new dbEventSearchModel { id = _Event.id.ToString(), name = _Event.name, type = EventSearchType.Venue.ToString() });
                            }
                        }
                    }
                }
            }

            return seatGeekVenueSearch;
        }


        [HttpGet]
        [Route("api/Events/GetEventsSearchList")]
        public HttpResponseMessage GetEventsSearchList(Int32 UserID, string id, string name, string type, double? Lat, double? Lon, decimal? radius, string from, string to)
        {
            try
            {
                if (UserID == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, "UserID required"));
                }


                GenericRepository<Artists> _ArtistsRepo = new GenericRepository<Artists>(_unitOfWork);
                GenericRepository<Venue> _VenueRepo = new GenericRepository<Venue>(_unitOfWork);
                GenericRepository<TourDate> _TourDateRepo = new GenericRepository<TourDate>(_unitOfWork);
                GenericRepository<UserTourDate> _UserTourDateRepo = new GenericRepository<UserTourDate>(_unitOfWork);
                GenericRepository<UserArtists> _UserArtistsRepo = new GenericRepository<UserArtists>(_unitOfWork);
                GenericRepository<ArtistsNotLatin> _ArtistNotLatinRepo = new GenericRepository<ArtistsNotLatin>(_unitOfWork);
                //  DateTime? _st = Helper.GetUTCDateTime(DateTime.Now);


                GenericRepository<Users> _UsersRepo = new GenericRepository<Users>(_unitOfWork);


                Models.Users _Users = null;
                HttpCache _HttpCache = new HttpCache();
                //List<SeatGeek3.ViewEventLookup> _ViewEventLookup = new List<SeatGeek3.ViewEventLookup>();
                List<ViewEventsPreList> _ViewEventsList = new List<ViewEventsPreList>();

                _Users = _UsersRepo.Repository.Get(p => p.UserID == UserID);

                if (_Users == null || _Users.RecordStatus != RecordStatus.Active.ToString())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.UserNotFound, "EventList"));
                }

                if (type == EventSearchType.Venue.ToString()) //If venue search ignore lat lon radius filter
                {
                    radius = 0;
                    Lat = 0;
                    Lon = 0;
                }



                if (String.IsNullOrEmpty(type))
                {
                    List<ViewEventsPreList> _list = null;

                    if (Lat != null && Lat != 0 && Lon != null && Lon != 0)
                    {
                        if (radius == 0)
                        {
                            radius = 25;
                        }
                    }


                    if (radius != null && radius > 0 && Lat != null && Lat != 0 && Lon != null && Lon != 0)
                    {

                        _list = (from B in _ArtistsRepo.Repository.GetAll()
                                 join A in _TourDateRepo.Repository.GetAll() on B.ArtistID equals A.ArtistID
                                 join C in _VenueRepo.Repository.GetAll() on A.VenueID equals C.VenueID
                                 where Helper.GetUTCDateTime(DateTime.Now) < A.Tour_Utcdate && C.VenueLat.HasValue && C.VenueLong.HasValue && C.VenueLat.Value != 0 && C.VenueLong != 0
                                 select new ViewEventsPreList
                                 {
                                     TourDateID = A.TourDateID,
                                     ArtistID = A.ArtistID.Value,
                                     ArtistName = B.ArtistName,
                                     Datetime_dt =A.Datetime_Local,
                                     ImageURL = B.ImageURL,
                                     BannerImage_URL = B.BannerImage_URL,
                                     OnTour = B.OnTour,
                                     VenueName = C.VenueName,
                                     VenuID = C.VenueID,
                                     VenueLat = C.VenueLat,
                                     VenueLon = C.VenueLong,
                                     IsDeleted = A.IsDeleted
                                 }).ToList();
                    }
                    else
                    {
                        _list = (from A1 in _UserArtistsRepo.Repository.GetAll(p => p.UserID == UserID)
                                 join A in _TourDateRepo.Repository.GetAll() on A1.ArtistID equals A.ArtistID
                                 join B in _ArtistsRepo.Repository.GetAll() on A.ArtistID equals B.ArtistID
                                 join C in _VenueRepo.Repository.GetAll() on A.VenueID equals C.VenueID
                                 where Helper.GetUTCDateTime(DateTime.Now) < A.Tour_Utcdate && C.VenueLat.HasValue && C.VenueLong.HasValue && C.VenueLat.Value != 0 && C.VenueLong != 0
                                 select new ViewEventsPreList
                                 {
                                     TourDateID = A.TourDateID,
                                     ArtistID = A.ArtistID.Value,
                                     ArtistName = B.ArtistName,
                                     Datetime_dt = A.Datetime_Local,
                                     ImageURL = B.ImageURL,
                                     BannerImage_URL = B.BannerImage_URL,
                                     OnTour = B.OnTour,
                                     VenueName = C.VenueName,
                                     VenuID = C.VenueID,
                                     VenueLat = C.VenueLat,
                                     VenueLon = C.VenueLong,
                                     IsDeleted = A.IsDeleted
                                 }).ToList();

                    }

                    if (_list.Count() > 0)
                    {
                        if (radius != null && radius > 0 && Lat != null && Lat != 0 && Lon != null && Lon != 0) //using the filter
                            _list.RemoveAll(p => CalculateDistance.distance(p.VenueLat.GetDouble(), p.VenueLon.GetDouble(), Lat.GetDouble(), Lon.GetDouble(), _Users.UserLanguage == EUserLanguage.EN.ToString() ? CalculateDistance.Measure.M : CalculateDistance.Measure.K) > radius.GetDouble());

                        if (_list.Count() > 0)
                        {
                            DateTime? fromDate = null;
                            DateTime? toDate = null;

                            if (!String.IsNullOrEmpty(to))
                                toDate = Convert.ToDateTime(to);

                            if (!String.IsNullOrEmpty(from))
                                fromDate = Convert.ToDateTime(from);

                            if (fromDate == null && toDate != null)
                                fromDate = DateTime.Now;
                            else if (fromDate != null && toDate == null)
                                toDate = DateTime.MaxValue;

                            if (fromDate != null && toDate != null)
                            {
                                string strFromDate = fromDate.Value.ToString();
                                string strToDate = toDate.Value.ToString();
                                _list.RemoveAll(x => Convert.ToDateTime(x.Datetime_Local) < fromDate || Convert.ToDateTime(x.Datetime_Local) > toDate);
                            }

                            /*var _returnlist = _list.OrderBy(x => Convert.ToDateTime(x.Datetime_Local)).Select(x => new ViewEventsList
                            { ArtistID = x.ArtistID, BannerImage_URL = x.BannerImage_URL, ArtistName = x.ArtistName, Datetime_Local = x.Datetime_Local, ImageURL = x.ImageURL, OnTour = x.OnTour, TourDateID = x.TourDateID, VenueName = x.VenueName,  });*/

                            _list = _list.OrderBy(p => Convert.ToDateTime(p.Datetime_Local)).ToList();

                        }
                    }

                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, ViewEventsPreList.GetEventsList(_list), "EventList"));
                }

                //return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, _ViewEventLookup, "Events"));

                string strThumbnailURLfordb = null;
                string strIamgeURLfordb = null;
                string strTempImageSave = null;

                string tempfilePath = WebConfigurationManager.AppSettings["SiteImgPath"].ToString() + @"\" + "TempImages" + @"\";

                string _SiteRoot = WebConfigurationManager.AppSettings["SiteImgPath"];
                string _SiteURL = WebConfigurationManager.AppSettings["SiteImgURL"];

                string _MusicGrapgh_api_key = ConfigurationManager.AppSettings["MusicGrapgh_api_key"].ToString();
                string _Instagram_access_token = ConfigurationManager.AppSettings["instagram.access_token"].ToString();

                string _Eventful_app_key = ConfigurationManager.AppSettings["Eventful_app_key"].ToString();

                string _SeatGeek_client_id = ConfigurationManager.AppSettings["SeatGeek_client_id"].ToString();
                string _SeatGeek_client_secret = ConfigurationManager.AppSettings["SeatGeek_client_secret"].ToString();

                string _result;


                //Get Cache
                if (_HttpCache.Get(UserID.ToString() + name + Lat.ToString() + Lon.ToString() + radius.ToString() + from + to, out _ViewEventsList) == true)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, ViewEventsPreList.GetEventsList(_ViewEventsList), "EventList"));
                }

                List<ViewEventsPreList> dbEventsList = new List<ViewEventsPreList>();
                dbEventsList = DbEventListCheck(UserID, id, name, type);

                try
                {
                    if (!String.IsNullOrEmpty(name)) //If searching by typing in the search box
                    {
                        radius = 0;
                        Lat = 0;
                        Lon = 0;
                    }

                    #region "Get SeatGeek Events by Lat/lon/fromdate/todate/radius"
                    #region SearchString
                    string _concat = SGAuth() + "&";

                    //Default login time lat/lon if lat/lon not provided 
                    if (Lat != null && Lon != null && Lat != 0 && Lon != 0)
                    {
                        _concat = _concat + "lat=" + Lat + "&lon=" + Lon + "&";
                    }
                    //else
                    //{
                    //    _concat = _concat + "lat=" + _Users.DeviceLat + "&lon=" + _Users.DeviceLong + "&";
                    //}

                    //Default is 25mi if radius not provided
                    if (radius != null && radius != 0)
                    {
                        _concat = _concat + "range=" + radius + "mi&";
                    }
                    //else
                    //{
                    //    _concat = _concat + "range=25mi&";
                    //}

                    //set from-date if avaialble
                    if (from != null && from != "")
                    {
                        string _stfrom = DateTime.ParseExact(from, "MM/dd/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None).ToString("yyyy-MM-dd");
                        _concat = _concat + "datetime_local.gte=" + _stfrom + "&";
                    }

                    //Default is 6 month from the current date if To-Date not provided
                    if (to != null && to != "")
                    {
                        string _stto = DateTime.ParseExact(to, "MM/dd/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None).ToString("yyyy-MM-dd");
                        _concat = _concat + "datetime_local.lte=" + _stto + "&";
                    }
                    else
                    {
                        string _stto = DateTime.ParseExact(DateTime.Now.AddDays(180).ToString("MM/dd/yyyy"), "MM/dd/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None).ToString("yyyy-MM-dd");
                        _concat = _concat + "datetime_local.lte=" + _stto + "&";
                    }

                    #endregion

                    _ViewEventsList = new List<ViewEventsPreList>();


                    //List<dbEventSearchModel> dbLookupList = new List<dbEventSearchModel>();

                    //if (key != "")
                    //dbLookupList = dbLookup[key] as List<dbEventSearchModel>;
                    // else
                    //     dbLookupList.Add(new dbEventSearchModel { id = 0, name = search });

                    var strEncoded = HttpUtility.UrlEncode(name);
                    //var strEncoded = HttpUtility.UrlEncode(name);

                    if (type == EventSearchType.Venue.ToString())
                    {
                        _concat = _concat + "venue.name=" + strEncoded + "&taxonomies.id=2000000&taxonomies.id=2010000&taxonomies.id=3010100&taxonomies.id=3010200&taxonomies.id=3010300&per_page=100&page=1";
                    }
                    else if (type == EventSearchType.Artist.ToString())
                    {
                        //name = name.ToLower().Replace(" ", "-");

                        _concat = _concat + "q=" + strEncoded + "&taxonomies.id=2000000&taxonomies.id=2010000&taxonomies.id=3010100&taxonomies.id=3010200&taxonomies.id=3010300&per_page=100&page=1";
                    }
                    else
                    {
                        _concat = _concat + "q=" + strEncoded + "&taxonomies.id=2000000&taxonomies.id=2010000&taxonomies.id=3010100&taxonomies.id=3010200&taxonomies.id=3010300&per_page=100&page=1";
                    }

                    //LogHelper.CreateLog(_concat);

                    var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://api.seatgeek.com/2/events" + _concat);

                    httpWebRequest.ContentType = "application/json";
                    httpWebRequest.Method = "GET";

                    var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        _result = streamReader.ReadToEnd();
                    }

                    //LogHelper.CreateLog(_result);

                    //_result = "";
                    var serializer = new JavaScriptSerializer();
                    serializer.RegisterConverters(new[] { new DynamicJsonConverter() });
                    dynamic obj = null;

                    // deserializing 
                    var _Get_Events_ByLat = JsonConvert.DeserializeObject<SeatGeek3.Get_Events_ByLat>(_result);
                    //dynamic _Get_Events_ByLat = serializer.Deserialize(_result, typeof(object));

                    if (_Get_Events_ByLat != null)
                    {
                        if (_Get_Events_ByLat.events != null && _Get_Events_ByLat.events.Count > 0)
                        {
                            foreach (SeatGeek3.Event _Event in _Get_Events_ByLat.events.OrderBy(x => x.datetime_local).ToList())
                            {
                                if (!_ArtistNotLatinRepo.Repository.AsQueryable().Any(x => x.ArtistName == _Event.title))
                                {
                                    bool anyLatin = false;
                                    if (_Event.performers.Count() > 0)
                                    {
                                        //anyLatin = true;

                                        foreach (var p in _Event.performers)
                                        {
                                            try
                                            {
                                                if (_ArtistsRepo.Repository.AsQueryable().Any(x => x.ArtistName == p.name))
                                                {
                                                    anyLatin = true;
                                                    break;
                                                }
                                                else if (!_ArtistsRepo.Repository.AsQueryable().Any(x => x.ArtistName == p.name))
                                                {
                                                    anyLatin = CheckSeatGeekLatin(p.name, _unitOfWork);
                                                }
                                                else
                                                { }
                                            }
                                            catch (Exception ex)
                                            {

                                            }
                                        }
                                    }

                                    if (anyLatin || _Event.performers.Count() == 0)
                                    {
                                        var localtime = Convert.ToDateTime(_Event.datetime_local);

                                        if (localtime > DateTime.Now)
                                        {
                                            var ddate = Convert.ToDateTime(_Event.datetime_local).ToString("d");

                                            if (!dbEventsList.Any(x => x.TourDateID == _Event.id || x.Datetime_Local == ddate))
                                            {
                                                var localDateString = localtime.ToString("MM/dd");
                                                var eventName = _Event.title;
                                                if (eventName.Length > 38)
                                                    eventName = eventName.Substring(0, 38) + "...";

                                                _ViewEventsList.Add(new ViewEventsPreList
                                                {
                                                    TourDateID = _Event.id,
                                                    ArtistName = eventName,
                                                    Datetime_dt =Convert.ToDateTime(ddate),
                                                    VenuID = _Event.venue.id,
                                                    VenueName = _Event.venue.name,
                                                    OnTour = true,
                                                    ImageURL = "",
                                                    BannerImage_URL = "",
                                                    ArtistID = 0,
                                                    IsDeleted = false

                                                });
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    #endregion

                    if (_ViewEventsList != null && _ViewEventsList.Count() > 50)
                        _ViewEventsList = _ViewEventsList.Take(50).ToList();

                    //_ViewEventLookup = new List<SeatGeek3.ViewEventLookup>();

                    if (dbEventsList != null && dbEventsList.Count() > 0)
                    {
                        _ViewEventsList.InsertRange(0, dbEventsList);//   .Union(_ViewDBLookup).OrderBy(x=>x.TourID.GetInt()).GroupBy(x => x.TourID);
                    }

                    if (_ViewEventsList.Count() > 0)
                    {
                        if (radius != null && radius > 0 && Lat != null && Lat != 0 && Lon != null && Lon != 0) //using the filter
                            _ViewEventsList.RemoveAll(p => CalculateDistance.distance(p.VenueLat.GetDouble(), p.VenueLon.GetDouble(), Lat.GetDouble(), Lon.GetDouble(), _Users.UserLanguage == EUserLanguage.EN.ToString() ? CalculateDistance.Measure.M : CalculateDistance.Measure.K) > radius.GetDouble());

                        if (_ViewEventsList.Count() > 0)
                        {
                            DateTime? fromDate = null;
                            DateTime? toDate = null;

                            if (!String.IsNullOrEmpty(to))
                                toDate = Convert.ToDateTime(to);

                            if (!String.IsNullOrEmpty(from))
                                fromDate = Convert.ToDateTime(from);

                            if (fromDate == null && toDate != null)
                                fromDate = DateTime.Now;
                            else if (fromDate != null && toDate == null)
                                toDate = DateTime.MaxValue;

                            if (fromDate != null && toDate != null)
                            {
                                string strFromDate = fromDate.Value.ToString();
                                string strToDate = toDate.Value.ToString();
                                _ViewEventsList.RemoveAll(x => Convert.ToDateTime(x.Datetime_Local) < fromDate || Convert.ToDateTime(x.Datetime_Local) > toDate);
                            }

                            /*var _returnlist = _list.OrderBy(x => Convert.ToDateTime(x.Datetime_Local)).Select(x => new ViewEventsList
                            { ArtistID = x.ArtistID, BannerImage_URL = x.BannerImage_URL, ArtistName = x.ArtistName, Datetime_Local = x.Datetime_Local, ImageURL = x.ImageURL, OnTour = x.OnTour, TourDateID = x.TourDateID, VenueName = x.VenueName,  });*/

                            _ViewEventsList = _ViewEventsList.OrderBy(p => Convert.ToDateTime(p.Datetime_Local)).ToList();
                        }
                    }

                    //_ViewEventsList = _ViewEventsList.OrderBy(x => Convert.ToDateTime(x.Datetime_Local)).ToList();
                    //Save cache
                    if (_ViewEventsList.Count > 0)
                    {
                        _HttpCache.Set(UserID.ToString() + name + Lat.ToString() + Lon.ToString() + radius.ToString() + from + to, _ViewEventsList, 24);
                    }

                    var _listing = ViewEventsPreList.GetEventsList(_ViewEventsList);

                    if (_listing.Count > 0)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, _listing, "EventList"));
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, "We have found no results. Please try again.", "EventList"));
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.CreateLog3(ex, Request);
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, ex.Message, "EventList"));
                }
                //return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, _ViewEventsList, "EventList"));
            }
            catch (Exception ex)
            {
                LogHelper.CreateLog3(ex, Request);
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Exception, ex.Message));
            }
        }
        /*
                [HttpGet]
                [Route("api/Events/GetEventsLookup")]
                public HttpResponseMessage GetEventsLookup(string search, int UserID, double? Lat, double? lon, decimal? radius, string from, string to)
                {
                    try
                    {
                        GenericRepository<Users> _UsersRepo = new GenericRepository<Users>(_unitOfWork);
                

                        Models.Users _Users = null;
                        HttpCache _HttpCache = new HttpCache();
                        List<SeatGeek3.ViewEventLookup> _ViewEventLookup = new List<SeatGeek3.ViewEventLookup>();
                


                        string pattern = @"[^a-zA-Z0-9]";
                        search = Regex.Replace(search, pattern, "");

                        if (search.Length < 3)
                            return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, "", "Events"));


                        _Users = _UsersRepo.Repository.Get(p => p.UserID == UserID);

                        if (_Users == null)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.UserNotFound, "Events"));
                        }


  
                        //return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, _ViewEventLookup, "Events"));


                        string strThumbnailURLfordb = null;
                        string strIamgeURLfordb = null;
                        string strTempImageSave = null;

                        string tempfilePath = WebConfigurationManager.AppSettings["SiteImgPath"].ToString() + @"\" + "TempImages" + @"\";

                        string _SiteRoot = WebConfigurationManager.AppSettings["SiteImgPath"];
                        string _SiteURL = WebConfigurationManager.AppSettings["SiteImgURL"];


                        string _MusicGrapgh_api_key = ConfigurationManager.AppSettings["MusicGrapgh_api_key"].ToString();
                        string _Instagram_access_token = ConfigurationManager.AppSettings["instagram.access_token"].ToString();

                        string _Eventful_app_key = ConfigurationManager.AppSettings["Eventful_app_key"].ToString();

                        string _SeatGeek_client_id = ConfigurationManager.AppSettings["SeatGeek_client_id"].ToString();
                        string _SeatGeek_client_secret = ConfigurationManager.AppSettings["SeatGeek_client_secret"].ToString();

                        string _result;


                        //Get Cache
                        if (_HttpCache.Get(UserID.ToString()+ search + Lat.ToString() + lon.ToString() + radius.ToString() + from + to, out _ViewEventLookup) == true)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, _ViewEventLookup, "Events"));
                        }


                        //var dbLookup =

                        List<dbEventSearchModel> _ViewDBLookup = DbEventLookup(search);//dbLookup["tourdates"] as List<SeatGeek3.ViewEventLookup>;

                
                        try
                        {
                            #region "Get SeatGeek Events by Lat/lon/fromdate/todate/radius"
                            #region SearchString
                            string _concat = "?";

                            //Default login time lat/lon if lat/lon not provided 
                            if (Lat != null && lon != null && Lat != 0 && lon != 0)
                            {
                                _concat = _concat + "lat=" + Lat + "&lon=" + lon + "&";
                            }
                            else {
                                _concat = _concat + "lat=" + _Users.DeviceLat + "&lon=" + _Users.DeviceLong + "&";
                            }
                    
                            //Default is 25mi if radius not provided
                            if (radius != null && radius != 0)
                            {
                                _concat = _concat + "range=" + radius + "mi&";
                            }
                            else {
                                _concat = _concat + "range=25mi&";
                            }

                            //set from-date if avaialble
                            if (from != null && from != "")
                            {
                                 string _stfrom = DateTime.ParseExact(from, "MM/dd/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None).ToString("yyyy-MM-dd");
                                 _concat = _concat + "datetime_local.gte=" + _stfrom + "&";
                            }

                            //Default is 3 month from the current date if To-Date not provided
                            if (to != null && to != "")
                            {
                                string _stto = DateTime.ParseExact(to, "MM/dd/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None).ToString("yyyy-MM-dd");
                                _concat = _concat + "datetime_local.lte=" + _stto + "&";
                            }
                            else {
                                string _stto = DateTime.ParseExact(DateTime.Now.AddDays(90).ToString("MM/dd/yyyy"), "MM/dd/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None).ToString("yyyy-MM-dd");
                                _concat = _concat + "datetime_local.lte=" + _stto + "&";
                            }

                            #endregion


                            List<string> dbLookupKeys = new List<string>() { "venue", "artist" };
                            _ViewEventLookup = new List<SeatGeek3.ViewEventLookup>();

                            foreach (var key in dbLookupKeys)
                            {

                                //List<dbEventSearchModel> dbLookupList = new List<dbEventSearchModel>();

                                //if (key != "")
                                    //dbLookupList = dbLookup[key] as List<dbEventSearchModel>;
                               // else
                               //     dbLookupList.Add(new dbEventSearchModel { id = 0, name = search });

                                foreach (var d in dbLookupList)
                                {
                                    var name = d.name;
                                    name = Regex.Replace(name, pattern, "");


                                    if(key == "venue")
                                    {
                                        _concat = _concat + "venue.name=" + name + "&taxonomies.name=music festival&taxonomies.name=concert&per_page=100&per_page=1";
                                    }
                                    else if (key == "artist")
                                    {
                                        name = d.name.ToLower().Replace(" ", "-");
                                
                                        _concat = _concat + "performers.slug=" + name + "&taxonomies.name=music festival&taxonomies.name=concert&per_page=100&per_page=1";
                                    }
                                    else
                                    {
                                        _concat = _concat + "q=" + name + "&taxonomies.name=music festival&taxonomies.name=Concert&per_page=100&per_page=1";
                                    }



                                    var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://api.seatgeek.com/2/events" + _concat);

                                    httpWebRequest.ContentType = "application/json";
                                    httpWebRequest.Method = "GET";

                                    var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                                    {
                                        _result = streamReader.ReadToEnd();
                                    }

                                    //LogHelper.CreateLog(_result);

                                    //_result = "";
                                    var serializer = new JavaScriptSerializer();
                                    serializer.RegisterConverters(new[] { new DynamicJsonConverter() });
                                    dynamic obj = null;

                                    // deserializing 
                                    var _Get_Events_ByLat = JsonConvert.DeserializeObject<SeatGeek3.Get_Events_ByLat>(_result);
                                    //dynamic _Get_Events_ByLat = serializer.Deserialize(_result, typeof(object));

                            

                                    if (_Get_Events_ByLat != null)
                                    {
                                        if (_Get_Events_ByLat.events != null && _Get_Events_ByLat.events.Count > 0)
                                        {

                                            foreach (SeatGeek3.Event _Event in _Get_Events_ByLat.events)
                                            {

                                                var localtime = Convert.ToDateTime(_Event.datetime_local);

                                                if (localtime > DateTime.Now)
                                                {
                                                    var localDateString = localtime.ToString("MM/dd");
                                                    var eventName = _Event.title;
                                                    if (eventName.Length > 38)
                                                        eventName = eventName.Substring(0, 38) + "...";

                                                    _ViewEventLookup.Add(new SeatGeek3.ViewEventLookup
                                                    {
                                                        TourID = _Event.id.ToString(),
                                                        Title = localDateString + " - " + eventName,
                                                        Datetime_Local = Convert.ToDateTime(_Event.datetime_local).ToString("d"),
                                                        VenueID = _Event.venue.id.ToString(),
                                                        VenueName = _Event.venue.name,
                                                        Taxonomies = _Event.taxonomies,
                                                        Performers = _Event.performers.Select(p => new SeatGeek3.Performer4
                                                        {
                                                            PerformerID = p.id.ToString()
                                                            ,
                                                            PerformerName = p.name
                                                            ,
                                                            PerformerImageURL = ""
                                                        }).ToList()

                                                    });
                                                }
                                            }


                                        }


                                    }
                                }

                            }
                            #endregion


                            if (_ViewEventLookup != null && _ViewEventLookup.Count() > 50)
                                _ViewEventLookup = _ViewEventLookup.Take(50).ToList();
                    
                        _ViewEventLookup = new List<SeatGeek3.ViewEventLookup>();

                           if (_ViewDBLookup != null && _ViewDBLookup.Count() > 0)
                            {
                                _ViewEventLookup.InsertRange(0, _ViewDBLookup);//   .Union(_ViewDBLookup).OrderBy(x=>x.TourID.GetInt()).GroupBy(x => x.TourID);
                                _ViewEventLookup = _ViewEventLookup.OrderBy(x => x.Title).ToList();
                            }

                
                            //Save cache
                            if (_ViewEventLookup.Count > 0)
                            {
                                _HttpCache.Set(UserID.ToString()+search + Lat.ToString() + lon.ToString() + radius.ToString() + from + to, _ViewEventLookup, 24);
                            }

                            return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, _ViewEventLookup, "Events"));
                        }
                        catch (Exception ex)
                        {
                   
                            return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, ex.Message, "Events"));
                        }


                        return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.InvalidSearchCriteria, "Events"));
                    }
                    catch (Exception ex)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, ex.Message, "Events"));
                    }


                }

                */

        [HttpGet]
        [Route("api/Events/GetEventByID")]
        public async Task<HttpResponseMessage> GetEventByID(string TourID, int UserID)
        {
            try
            {
                GenericRepository<Users> _UsersRepo = new GenericRepository<Users>(_unitOfWork);
                Models.Users _Users = null;

                _Users = _UsersRepo.Repository.Get(p => p.UserID == UserID);

                if (_Users == null || _Users.RecordStatus != RecordStatus.Active.ToString())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.UserNotFound, "EventsDetail"));
                }

                string strThumbnailURLfordb = null;
                string strIamgeURLfordb = null;
                string strTempImageSave = null;

                string tempfilePath = WebConfigurationManager.AppSettings["SiteImgPath"].ToString() + @"\" + "TempImages" + @"\";

                string _SiteRoot = WebConfigurationManager.AppSettings["SiteImgPath"];
                string _SiteURL = WebConfigurationManager.AppSettings["SiteImgURL"];


                string _MusicGrapgh_api_key = ConfigurationManager.AppSettings["MusicGrapgh_api_key"].ToString();
                string _Instagram_access_token = ConfigurationManager.AppSettings["instagram.access_token"].ToString();

                string _Eventful_app_key = ConfigurationManager.AppSettings["Eventful_app_key"].ToString();

                string _SeatGeek_client_id = ConfigurationManager.AppSettings["SeatGeek_client_id"].ToString();
                string _SeatGeek_client_secret = ConfigurationManager.AppSettings["SeatGeek_client_secret"].ToString();

                string _Spotify_Country = ConfigurationManager.AppSettings["Spotify_Country"].ToString();

                string _result;

                string _MusicGraph_ID = null;
                string _Performer_ID = null;
                string _strEvent = null;

                Models.Venue _VenuEntity = null;
                Models.TourDate _TourDateEntity = null;
                Models.ArtistRelated _ArtistRelatedEntity = null;
                Models.Artists _Artists = null;
                Models.TourPerformers _TourPerformers = null;

                Models.ArtistGenre _ArtistGenre = null;
                Models.Genre _Genre = null;

                string _MG_Artist_ID = null;

                _unitOfWork.StartTransaction();

                GenericRepository<Artists> _ArtistsRepo = new GenericRepository<Artists>(_unitOfWork);
                GenericRepository<ArtistPhotos> _ArtistPhotosRepo = new GenericRepository<ArtistPhotos>(_unitOfWork);
                GenericRepository<ArtistRelated> _ArtistRelatedRepo = new GenericRepository<ArtistRelated>(_unitOfWork);
                GenericRepository<TourDate> _TourDateRepo = new GenericRepository<TourDate>(_unitOfWork);
                GenericRepository<Venue> _VenueRepo = new GenericRepository<Venue>(_unitOfWork);
                GenericRepository<TourPerformers> _TourPerformersRepo = new GenericRepository<TourPerformers>(_unitOfWork);

                GenericRepository<ArtistGenre> _ArtistGenreRepo = new GenericRepository<ArtistGenre>(_unitOfWork);
                GenericRepository<Genre> _GenreRepo = new GenericRepository<Genre>(_unitOfWork);


                var serializer = new JavaScriptSerializer();
                serializer.RegisterConverters(new[] { new DynamicJsonConverter() });
                dynamic obj = null;


                //Check Tour Date in Local DB
                try
                {
                    Int64 _TourID = Convert.ToInt64(TourID);
                    _TourDateEntity = _TourDateRepo.Repository.Get(p => p.TourDateID == _TourID);
                }
                catch { }


                if (_TourDateEntity == null)
                {
                    _TourDateEntity = _TourDateRepo.Repository.Get(p => p.SeatGeek_TourID == TourID);
                }


                if (_TourDateEntity != null)
                {
                    TimeSpan span = DateTime.Now - Convert.ToDateTime(_TourDateEntity.ModifiedDate);
                    if (span.Hours < 24 && span.Days == 0)
                    {
                        _unitOfWork.RollBack();
                        return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, GetTourDetail_ByID(_TourDateEntity, UserID), "EventsDetail"));
                    }

                }

                try
                {
                    if (_TourDateEntity == null)
                    {
                        #region "Get SeatGeek Event By ID"



                        var _Get_Events_ByID = await SeatGeek_GetEventByID_Asyn(_TourDateEntity == null ? TourID : _TourDateEntity.SeatGeek_TourID);//await for the function to be completed.




                        if (_Get_Events_ByID != null)
                        {
                            #region "Add Venu Information"

                            SeatGeek4.Venue _Venue = _Get_Events_ByID.venue;

                            //Venu information
                            //_VenuEntity = _VenueRepo.Repository.Get(p => p.SeatGeek_VenuID == _Venue.id.ToString());

                            _VenuEntity = (from A in _VenueRepo.Repository.GetAll()
                                           where (A.SeatGeek_VenuID == _Venue.id.ToString())
                                           select A).FirstOrDefault();


                            //search the venu using fuzzy searching
                            if (_VenuEntity == null)
                            {
                                /*_VenuEntity = (from A in _VenueRepo.Repository.GetAll()
                                               where (DiceCoefficientExtensions.DiceCoefficient(A.VenueName.ToLower(), _VenueName) >= _FuzzySearchCri)
                                               && A.RecordStatus == RecordStatus.Eventful.ToString()
                                               select A).FirstOrDefault();
                                               */

                                string pattern = @"[^a-zA-Z0-9áéíñóúüÁÉÍÑÓÚÜ&\s]";
                                var search = Regex.Replace(_Venue.name.ToLower(), pattern, "");

                                var venueByName = _VenueRepo.Repository.AsQueryable().Where(x => x.VenueName.Length < 60 && x.RecordStatus == RecordStatus.Eventful.ToString() && x.VenueName.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                                if (venueByName == null || venueByName.Count() == 0)
                                {
                                    var allEventfulVenues = _VenueRepo.Repository.AsQueryable().Where(x => x.VenueName.Length < 80 && x.RecordStatus == RecordStatus.Eventful.ToString()).ToList();
                                    foreach (var a in allEventfulVenues)
                                    {
                                        var eventfulVenue = Regex.Replace(a.VenueName, pattern, "");
                                        if (eventfulVenue.ToLower() == search.ToLower())
                                        {
                                            _VenuEntity = a;
                                            break;
                                        }
                                        if (search.Length > 11 && eventfulVenue.ToLower().Contains(search.ToLower().Substring(2, search.Length - 2)))
                                        {
                                            _VenuEntity = a;
                                            break;
                                        }
                                        if (search.Length > 20 && eventfulVenue.ToLower().Contains(search.ToLower().Substring(5, search.Length - 5)))
                                        {
                                            _VenuEntity = a;
                                            break;
                                        }
                                        if (search.Length > 40 && eventfulVenue.ToLower().Contains(search.ToLower().Substring(10, search.Length - 10)))
                                        {
                                            _VenuEntity = a;
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    _VenuEntity = venueByName.First();
                                }

                            }

                            if (_VenuEntity == null)
                            {
                                _VenuEntity = new Venue();
                                _VenuEntity.SeatGeek_VenuID = _Venue.id.ToString();
                                _VenuEntity.VenueName = _Venue.name;
                                _VenuEntity.Extended_Address = _Venue.extended_address;
                                _VenuEntity.VenueCountry = _Venue.country;
                                _VenuEntity.Display_Location = _Venue.display_location;
                                _VenuEntity.Slug = _Venue.slug;
                                _VenuEntity.VenueState = _Venue.state;
                                _VenuEntity.Postal_Code = _Venue.postal_code;
                                _VenuEntity.VenueCity = _Venue.city;
                                _VenuEntity.Address = _Venue.address;
                                _VenuEntity.Timezone = _Venue.timezone;

                                if (_Venue.location != null)
                                {
                                    _VenuEntity.VenueLat = _Venue.location.lat;
                                    _VenuEntity.VenueLong = _Venue.location.lon;
                                }



                                _VenuEntity.CreatedDate = DateTime.Now;
                                _VenuEntity.RecordStatus = RecordStatus.SeatGeek.ToString();

                                _VenueRepo.Repository.Add(_VenuEntity);

                            }
                            #endregion




                            //loop through to get Artist Information
                            #region "Get Artists/Instagram Information"
                            List<SeatGeek4.Performer> _Performers = new List<SeatGeek4.Performer>();
                            SeatGeek4.Performer _PerformersChk = null;

                            _Performers = _Get_Events_ByID.performers.Where(p => p.type != "theater" && p.type == "band").ToList();
                            _Performers = _Performers.OrderByDescending(p => p.primary).ToList();
                            _PerformersChk = _Performers.Where(p => p.primary == true).FirstOrDefault();



                            foreach (SeatGeek4.Performer _perfomer in _Performers)
                            {
                                _MusicGraph_ID = null;

                                #region "Get Artist Info Using Name  (MusicGraph)"
                                try
                                {
                                    var _Search_ByName = await MusicGrapgh_GetArtistByName_Asyn(_perfomer.name.Trim());//await for the function to be completed


                                    if (_Search_ByName != null && _Search_ByName.status.message == "Success")
                                    {
                                        foreach (MusicGraph.Datum _Datum in _Search_ByName.data)
                                        {


                                            if (RemoveDiacritics(_Datum.name.ToLower()) == _perfomer.name.ToLower())
                                            {
                                                _MusicGraph_ID = _Datum.id;
                                                _Artists = _ArtistsRepo.Repository.Get(p => p.Musicgraph_ID == _MusicGraph_ID);



                                                #region "Add New"
                                                if (_Artists == null)
                                                {
                                                    bool isLatin = false;

                                                    if (!isLatin)
                                                        isLatin = CheckMusicGraphLatin(_Datum.id, _unitOfWork);
                                                    if (!isLatin)
                                                        isLatin = CheckSeatGeekLatin(_Datum.name, _unitOfWork);
                                                    if (!isLatin)
                                                        isLatin = CheckLastResortSpotifyGenre(_Datum.spotify_id);


                                                    if (!isLatin)
                                                    {
                                                        _unitOfWork.RollBack();
                                                        return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, "Its not a latin artist event", "EventsDetail"));
                                                    }

                                                    if (isLatin)
                                                    {
                                                        _Artists = new Artists();

                                                        _Artists.ArtistName = _Datum.name; // Regex.Replace(_Datum.name, "[^A-Za-z0-9 _]", "");
                                                        _Artists.Gender = _Datum.gender;
                                                        _Artists.Decade = _Datum.decade;
                                                        _Artists.Main_Genre = _Datum.main_genre;

                                                        _Artists.Seatgeek_ID = _perfomer.id.ToString();

                                                        _Artists.Musicgraph_ID = !String.IsNullOrEmpty(_Datum.id) ? _Datum.id : _Datum.name;
                                                        _Artists.Artist_Ref_ID = _Datum.artist_ref_id;
                                                        _Artists.Musicbrainz_ID = _Datum.musicbrainz_id;
                                                        _Artists.Spotify_ID = _Datum.spotify_id;
                                                        _Artists.Youtube_ID = _Datum.youtube_id;
                                                        _Artists.Alternate_Names = _Datum.alternate_names != null && _Datum.alternate_names.Count > 0 ? _Datum.alternate_names[0] : "";

                                                        _Artists.RecordStatus = RecordStatus.MusicGraph.ToString();
                                                        _Artists.CreatedDate = DateTime.Now;
                                                        _Artists.ModifiedDate = DateTime.Now;

                                                        _Artists.OnTour = _perfomer.has_upcoming_events == true ? true : false;

                                                        _ArtistsRepo.Repository.Add(_Artists);

                                                        GetProfileImageFromSpotifyFeed(_Artists);

                                                        #region "Loop through the Genre"
                                                        if (_perfomer.genres != null && _perfomer.genres.Count > 0)
                                                        {
                                                            foreach (SeatGeek4.Genre _Ev in _perfomer.genres)
                                                            {
                                                                _Genre = _GenreRepo.Repository.Get(p => p.Name == _Ev.name.Trim());

                                                                if (_Genre == null)
                                                                {
                                                                    _Genre = new Genre();
                                                                    _Genre.Name = _Ev.name;
                                                                    _Genre.CreatedDate = DateTime.Now;
                                                                    _Genre.RecordStatus = RecordStatus.SeatGeek.ToString();
                                                                    _GenreRepo.Repository.Add(_Genre);

                                                                }

                                                                var check = !_ArtistGenreRepo.Repository.AsQueryable().Any(x => x.ArtistID == _Artists.ArtistID && x.GenreID == _Genre.GenreID);

                                                                if (check)
                                                                {
                                                                    _ArtistGenre = new ArtistGenre();
                                                                    _ArtistGenre.ArtistID = _Artists.ArtistID;
                                                                    _ArtistGenre.Slug = _Ev.slug;
                                                                    _ArtistGenre.Primary = _Ev.primary;
                                                                    _ArtistGenre.Name = _Ev.name;
                                                                    _ArtistGenre.CreatedDate = DateTime.Now;
                                                                    _ArtistGenre.RecordStatus = RecordStatus.SeatGeek.ToString();

                                                                    _ArtistGenreRepo.Repository.Add(_ArtistGenre);
                                                                }
                                                            }
                                                        }
                                                        #endregion

                                                        await Task.Factory.StartNew(() =>
                                                        {
                                                            if (WebConfigurationManager.AppSettings["SeatGeek.ArtistPicture"].ToString() == "True")
                                                            {
                                                                #region "Get Artist Picture "
                                                                try
                                                                {
                                                                    if (_perfomer.images != null)
                                                                    {
                                                                        if (_perfomer.images.huge != null)
                                                                        {
                                                                            Task<bool> _ArtistPic = ArtistProfilePicture_Asyn(UserID, _Artists, _unitOfWork, true, _perfomer.images.huge);
                                                                        }
                                                                    }
                                                                }
                                                                catch (Exception ex)
                                                                {

                                                                }
                                                                #endregion

                                                            }

                                                            #region "Get Similar Artists (dont need this block while just updating the records)"
                                                            Task<MusicGraph5.GetSimilarArtists_ByID> _GetSimilarArtists_ByID = MusicGrapgh_GetSimilarArtists_Asyn(_Artists.Musicgraph_ID, _Artists.ArtistID, _unitOfWork);
                                                            #endregion

                                                            _GetSimilarArtists_ByID.Wait();

                                                            #region "Get Artist Matrics (dont need this block while just updating the records)"
                                                            Task<MusicGraph2.ArtistMatrics_ByID> _ArtistMatrics_ByID = MusicGrapgh_GetArtistMatrics_Asyn(_Artists.Musicgraph_ID, _Artists.ArtistID, _unitOfWork);

                                                            _ArtistMatrics_ByID.Wait(); // wait for the function to complete

                                                            //Get Instagram ID from the MusicGraph matrcis 
                                                            if (_ArtistMatrics_ByID.Result.data.instagram != null && _ArtistMatrics_ByID.Result.data.instagram.url != null && _ArtistMatrics_ByID.Result.data.instagram.url != "")
                                                            {
                                                                _Artists.Instagram_Url = _ArtistMatrics_ByID.Result.data.instagram.url;
                                                                string _instaGram_ID = _ArtistMatrics_ByID.Result.data.instagram.url.Replace("http:", "https:").Replace("https://instagram.com/", "").Replace("/", "");
                                                                _Artists.Instagram_ID = _instaGram_ID;
                                                            }

                                                            #endregion



                                                            #region "Eventful API Implementation"
                                                            Task<bool> _Eventful = EventFul_GetEventInfo_Asyn(UserID, _Artists, _unitOfWork, true);
                                                            #endregion



                                                            #region "Instagram Api Implementation"
                                                            Task<bool> _Instagram = Instagram_GetPictures_Asyn(UserID, _Artists, _ArtistMatrics_ByID.Result, _unitOfWork, true, _TourDateEntity);
                                                            #endregion

                                                            #region "Spotify Api Implementation
                                                            Task<bool> _Spotify = Spotify_GetSongInfo_Asyn(UserID, _Artists, _unitOfWork, true);
                                                            #endregion

                                                        });
                                                    }
                                                }
                                                #endregion

                                                #region "Update Existing"
                                                else
                                                {
                                                    _Artists.ModifiedDate = DateTime.Now;
                                                    _Artists.OnTour = _perfomer.has_upcoming_events == true ? true : false;
                                                    await Task.Factory.StartNew(() =>
                                                    {
                                                        if (WebConfigurationManager.AppSettings["SeatGeek.ArtistPicture"].ToString() == "True")
                                                        {
                                                            #region "Get Artist Picture "
                                                            try
                                                            {
                                                                if (_perfomer.images != null)
                                                                {
                                                                    if (_perfomer.images.huge != null)
                                                                    {
                                                                        Task<bool> _ArtistPic = ArtistProfilePicture_Asyn(UserID, _Artists, _unitOfWork, true, _perfomer.images.huge);
                                                                    }
                                                                }
                                                            }
                                                            catch (Exception ex)
                                                            {

                                                            }
                                                            #endregion
                                                        }

                                                        #region "Instagram Api Implementation"
                                                        Task<bool> _Instagram = Instagram_GetPictures_Asyn(UserID, _Artists, null, _unitOfWork, false, _TourDateEntity);
                                                        #endregion

                                                        #region "Eventful API Implementation"
                                                        Task<bool> _Eventful = EventFul_GetEventInfo_Asyn(UserID, _Artists, _unitOfWork, false);
                                                        #endregion

                                                        #region "Spotify Api Implementation
                                                        Task<bool> _Spotify = Spotify_GetSongInfo_Asyn(UserID, _Artists, _unitOfWork, false);
                                                        #endregion



                                                    });
                                                }
                                                #endregion


                                                //_TourDateEntity.TicketURL = "8";
                                                //_TourDateRepo.Repository.Update(_TourDateEntity);
                                                //_unitOfWork.Commit();

                                                if (_perfomer.primary == true || _PerformersChk == null)
                                                {
                                                    _PerformersChk = _perfomer;
                                                    #region "Add/Update Tour Information"
                                                    //Entering Tour records
                                                    //_TourDateEntity = _TourDateRepo.Repository.Get(p => p.SeatGeek_TourID == _Get_Events_ByID.id.ToString() && p.ArtistID == _Artists.ArtistID);

                                                    DateTime _datetime_local = Convert.ToDateTime(_Get_Events_ByID.datetime_local);


                                                    _TourDateEntity = (from A in _VenueRepo.Repository.GetAll(p => p.VenueID == _VenuEntity.VenueID)
                                                                       join B in _TourDateRepo.Repository.GetAll(p =>
                                                                                                       (p.SeatGeek_TourID == _Get_Events_ByID.id.ToString() && p.ArtistID == _Artists.ArtistID)
                                                                                                    || (DbFunctions.TruncateTime(p.Datetime_Local).Value.Month == _datetime_local.Month
                                                                                                        && DbFunctions.TruncateTime(p.Datetime_Local).Value.Year == _datetime_local.Year
                                                                                                        && DbFunctions.TruncateTime(p.Datetime_Local).Value.Day == _datetime_local.Day
                                                                                                        && p.ArtistID == _Artists.ArtistID
                                                                                                        && p.RecordStatus == RecordStatus.Eventful.ToString())
                                                                                                    ) on A.VenueID equals B.VenueID
                                                                       where B.ArtistID == _Artists.ArtistID
                                                                       select B).FirstOrDefault();


                                                    //_TourDateEntity.TicketURL = "8";
                                                    //_TourDateRepo.Repository.Update(_TourDateEntity);
                                                    //_unitOfWork.Commit();

                                                    if (_TourDateEntity == null)
                                                    {
                                                        _TourDateEntity = new TourDate();

                                                        _TourDateEntity.SeatGeek_TourID = _Get_Events_ByID.id.ToString();
                                                        _TourDateEntity.ArtistID = _Artists.ArtistID;
                                                        _TourDateEntity.VenueID = _VenuEntity.VenueID;

                                                        _TourDateEntity.EventID = null;
                                                        _TourDateEntity.Score = _Get_Events_ByID.score;

                                                        _TourDateEntity.Announce_Date = Convert.ToDateTime(_Get_Events_ByID.announce_date);
                                                        _TourDateEntity.Visible_Until_utc = Convert.ToDateTime(_Get_Events_ByID.visible_until_utc);
                                                        _TourDateEntity.Tour_Utcdate = Convert.ToDateTime(_Get_Events_ByID.datetime_utc);
                                                        _TourDateEntity.Datetime_Local = Convert.ToDateTime(_Get_Events_ByID.datetime_local);

                                                        _TourDateEntity.CreatedDate = DateTime.Now;
                                                        _TourDateEntity.RecordStatus = RecordStatus.SeatGeek.ToString();

                                                        if (!String.IsNullOrEmpty(_Get_Events_ByID.url))
                                                            _TourDateEntity.TicketURL = _Get_Events_ByID.url;
                                                        else if (_Get_Events_ByID.venue != null && !String.IsNullOrEmpty(_Get_Events_ByID.venue.url))
                                                            _TourDateEntity.TicketURL = _Get_Events_ByID.venue.url;
                                                        else
                                                            _TourDateEntity.TicketURL = "https://seatgeek.com/";


                                                        _TourDateRepo.Repository.Add(_TourDateEntity);
                                                    }
                                                    else
                                                    {

                                                        _TourDateEntity.SeatGeek_TourID = _Get_Events_ByID.id.ToString();
                                                        _TourDateEntity.ArtistID = _Artists.ArtistID;
                                                        _TourDateEntity.VenueID = _VenuEntity.VenueID;
                                                        _TourDateEntity.Score = _Get_Events_ByID.score;

                                                        _TourDateEntity.Announce_Date = Convert.ToDateTime(_Get_Events_ByID.announce_date);
                                                        _TourDateEntity.Visible_Until_utc = Convert.ToDateTime(_Get_Events_ByID.visible_until_utc);
                                                        _TourDateEntity.Tour_Utcdate = Convert.ToDateTime(_Get_Events_ByID.datetime_utc);

                                                        _TourDateEntity.Datetime_Local = Convert.ToDateTime(_Get_Events_ByID.datetime_local);

                                                        _TourDateEntity.ModifiedDate = DateTime.Now;
                                                        _TourDateEntity.RecordStatus = RecordStatus.SeatGeek.ToString();

                                                        if (!String.IsNullOrEmpty(_Get_Events_ByID.url))
                                                            _TourDateEntity.TicketURL = _Get_Events_ByID.url;
                                                        else if (_Get_Events_ByID.venue != null && !String.IsNullOrEmpty(_Get_Events_ByID.venue.url))
                                                            _TourDateEntity.TicketURL = _Get_Events_ByID.venue.url;
                                                        else
                                                            _TourDateEntity.TicketURL = "https://seatgeek.com/";

                                                        _TourDateRepo.Repository.Update(_TourDateEntity);

                                                    }
                                                    #endregion
                                                }
                                                else
                                                {
                                                    #region "Save Other Artist in Tour Perfromer"
                                                    _TourPerformers = _TourPerformersRepo.Repository.Get(p => p.ArtistID == _Artists.ArtistID && p.TourDateID == _TourDateEntity.TourDateID);

                                                    if (_TourPerformers == null)
                                                    {
                                                        _TourPerformers = new TourPerformers();

                                                        _TourPerformers.TourDateID = _TourDateEntity.TourDateID;
                                                        _TourPerformers.ArtistID = _Artists.ArtistID;
                                                        _TourPerformers.CreatedDate = DateTime.Now;
                                                        _TourPerformers.RecordStatus = RecordStatus.MusicGraph.ToString();

                                                        _TourPerformersRepo.Repository.Add(_TourPerformers);
                                                    }
                                                    #endregion
                                                }

                                                string MusicGraphBio = "";
                                                if (!String.IsNullOrEmpty(_Artists.Musicgraph_ID))
                                                    MusicGraphBio = GetMusicGraphBio(_Artists.Musicgraph_ID);
                                                if (!String.IsNullOrEmpty(MusicGraphBio))
                                                    _Artists.About = MusicGraphBio;

                                                _ArtistsRepo.Repository.Update(_Artists);

                                                break;
                                            }
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {

                                }



                                #endregion

                                //if Artist not found in Music Graph (Save Direct)
                                if (_MusicGraph_ID == null)
                                {
                                    #region "use this block if Artist not found in (MusicGrapgh)"
                                    try
                                    {
                                        _Artists = _ArtistsRepo.Repository.Get(p => p.Seatgeek_ID == _perfomer.id.ToString());

                                        #region "Add New"
                                        if (_Artists == null)
                                        {
                                            bool isLatin = false;

                                            if (!isLatin)
                                                isLatin = CheckSeatGeekLatin(_perfomer.name, _unitOfWork);

                                            if (isLatin)
                                            {

                                                _Artists = new Artists();

                                                _Artists.ArtistName = _perfomer.name;//  Regex.Replace(_perfomer.name, "[^A-Za-z0-9 _]", "");
                                                _Artists.Gender = null;
                                                _Artists.Decade = null;

                                                if (_perfomer.genres != null)
                                                {
                                                    _Artists.Main_Genre = _perfomer.genres[0].name;
                                                }

                                                _Artists.Seatgeek_ID = _perfomer.id.ToString();

                                                _Artists.Musicgraph_ID = _perfomer.name;
                                                _Artists.Artist_Ref_ID = null;
                                                _Artists.Musicbrainz_ID = null;
                                                _Artists.Spotify_ID = null;
                                                _Artists.Youtube_ID = null;
                                                _Artists.Alternate_Names = null;

                                                _Artists.RecordStatus = RecordStatus.SeatGeek.ToString();
                                                _Artists.CreatedDate = DateTime.Now;
                                                _Artists.ModifiedDate = DateTime.Now;

                                                _Artists.OnTour = _perfomer.has_upcoming_events == true ? true : false;

                                                _ArtistsRepo.Repository.Add(_Artists);

                                                GetProfileImageFromSpotifyFeed(_Artists);

                                                _unitOfWork.Commit();
                                                _unitOfWork.StartTransaction();

                                                #region "Loop through the Genre"
                                                if (_perfomer.genres != null && _perfomer.genres.Count > 0)
                                                {
                                                    foreach (SeatGeek4.Genre _Ev in _perfomer.genres)
                                                    {
                                                        _Genre = _GenreRepo.Repository.Get(p => p.Name == _Ev.name.Trim());

                                                        if (_Genre == null)
                                                        {
                                                            _Genre = new Genre();
                                                            _Genre.Name = _Ev.name;
                                                            _Genre.CreatedDate = DateTime.Now;
                                                            _Genre.RecordStatus = RecordStatus.SeatGeek.ToString();
                                                            _GenreRepo.Repository.Add(_Genre);

                                                        }

                                                        var check = !_ArtistGenreRepo.Repository.AsQueryable().Any(x => x.ArtistID == _Artists.ArtistID && x.GenreID == _Genre.GenreID);

                                                        if (check)
                                                        {
                                                            _ArtistGenre = new ArtistGenre();
                                                            _ArtistGenre.GenreID = _Genre.GenreID;
                                                            _ArtistGenre.ArtistID = _Artists.ArtistID;
                                                            _ArtistGenre.Slug = _Ev.slug;
                                                            _ArtistGenre.Primary = _Ev.primary;
                                                            _ArtistGenre.Name = _Ev.name;
                                                            _ArtistGenre.CreatedDate = DateTime.Now;
                                                            _ArtistGenre.RecordStatus = RecordStatus.SeatGeek.ToString();

                                                            _ArtistGenreRepo.Repository.Add(_ArtistGenre);
                                                        }
                                                    }
                                                }
                                                #endregion

                                                await Task.Factory.StartNew(() =>
                                                {
                                                    if (WebConfigurationManager.AppSettings["SeatGeek.ArtistPicture"].ToString() == "True")
                                                    {
                                                        #region "Get Artist Picture "
                                                        try
                                                        {
                                                            if (_perfomer.images != null)
                                                            {
                                                                if (_perfomer.images.huge != null)
                                                                {
                                                                    Task<bool> _ArtistPic = ArtistProfilePicture_Asyn(UserID, _Artists, _unitOfWork, true, _perfomer.images.huge);
                                                                }
                                                            }
                                                        }
                                                        catch (Exception ex)
                                                        {

                                                        }
                                                        #endregion
                                                    }

                                                    #region "Instagram Api Implementation"
                                                    Task<bool> _Instagram = Instagram_GetPictures_Asyn(UserID, _Artists, null, _unitOfWork, true, _TourDateEntity);
                                                    #endregion

                                                    #region "Eventful API Implementation"
                                                    Task<bool> _Eventful = EventFul_GetEventInfo_Asyn(UserID, _Artists, _unitOfWork, true);
                                                    #endregion

                                                });
                                            }
                                        }
                                        #endregion

                                        #region "Update Existing"
                                        else
                                        {
                                            _Artists.ModifiedDate = DateTime.Now;
                                            _Artists.OnTour = _perfomer.has_upcoming_events == true ? true : false;

                                            await Task.Factory.StartNew(() =>
                                            {
                                                if (WebConfigurationManager.AppSettings["SeatGeek.ArtistPicture"].ToString() == "True")
                                                {
                                                    #region "Get Artist Picture "
                                                    try
                                                    {
                                                        if (_perfomer.images != null)
                                                        {
                                                            if (_perfomer.images.huge != null)
                                                            {
                                                                Task<bool> _ArtistPic = ArtistProfilePicture_Asyn(UserID, _Artists, _unitOfWork, true, _perfomer.images.huge);
                                                            }
                                                        }
                                                    }
                                                    catch (Exception ex)
                                                    {

                                                    }
                                                    #endregion

                                                }
                                                #region "Instagram Api Implementation"
                                                Task<bool> _Instagram = Instagram_GetPictures_Asyn(UserID, _Artists, null, _unitOfWork, false, _TourDateEntity);
                                                #endregion

                                                #region "Eventful API Implementation"
                                                Task<bool> _Eventful = EventFul_GetEventInfo_Asyn(UserID, _Artists, _unitOfWork, false);
                                                #endregion
                                            });
                                        }
                                        #endregion

                                        if (_perfomer.primary == true || _PerformersChk == null)
                                        {
                                            _PerformersChk = _perfomer;
                                            #region "Add/Update Tour Information"
                                            //Entering Tour records
                                            //_TourDateEntity = _TourDateRepo.Repository.Get(p => p.SeatGeek_TourID == _Get_Events_ByID.id.ToString() && p.ArtistID == _Artists.ArtistID);
                                            DateTime _datetime_local = Convert.ToDateTime(_Get_Events_ByID.datetime_local);

                                            _TourDateEntity = (from A in _VenueRepo.Repository.GetAll(p => p.VenueID == _VenuEntity.VenueID)
                                                               join B in _TourDateRepo.Repository.GetAll(p =>
                                                                                               (p.SeatGeek_TourID == _Get_Events_ByID.id.ToString() && p.ArtistID == _Artists.ArtistID)
                                                                                            || (DbFunctions.TruncateTime(p.Datetime_Local).Value.Month == _datetime_local.Month
                                                                                                && DbFunctions.TruncateTime(p.Datetime_Local).Value.Year == _datetime_local.Year
                                                                                                && DbFunctions.TruncateTime(p.Datetime_Local).Value.Day == _datetime_local.Day)
                                                                                                && p.ArtistID == _Artists.ArtistID
                                                                                                && p.RecordStatus == RecordStatus.Eventful.ToString()
                                                                                            ) on A.VenueID equals B.VenueID
                                                               select B).FirstOrDefault();

                                            if (_TourDateEntity == null)
                                            {
                                                _TourDateEntity = new TourDate();

                                                _TourDateEntity.SeatGeek_TourID = _Get_Events_ByID.id.ToString();
                                                _TourDateEntity.ArtistID = _Artists.ArtistID;
                                                _TourDateEntity.VenueID = _VenuEntity.VenueID;

                                                _TourDateEntity.EventID = null;
                                                _TourDateEntity.Score = _Get_Events_ByID.score;

                                                _TourDateEntity.Announce_Date = Convert.ToDateTime(_Get_Events_ByID.announce_date);
                                                _TourDateEntity.Visible_Until_utc = Convert.ToDateTime(_Get_Events_ByID.visible_until_utc);
                                                _TourDateEntity.Tour_Utcdate = Convert.ToDateTime(_Get_Events_ByID.datetime_utc);
                                                _TourDateEntity.Datetime_Local = Convert.ToDateTime(_Get_Events_ByID.datetime_local);

                                                _TourDateEntity.CreatedDate = DateTime.Now;
                                                _TourDateEntity.RecordStatus = RecordStatus.SeatGeek.ToString();

                                                if (!String.IsNullOrEmpty(_Get_Events_ByID.url))
                                                    _TourDateEntity.TicketURL = _Get_Events_ByID.url;
                                                else if (_Get_Events_ByID.venue != null && !String.IsNullOrEmpty(_Get_Events_ByID.venue.url))
                                                    _TourDateEntity.TicketURL = _Get_Events_ByID.venue.url;
                                                else
                                                    _TourDateEntity.TicketURL = "https://seatgeek.com/";

                                                _TourDateRepo.Repository.Add(_TourDateEntity);
                                            }
                                            else
                                            {

                                                _TourDateEntity.SeatGeek_TourID = _Get_Events_ByID.id.ToString();
                                                _TourDateEntity.ArtistID = _Artists.ArtistID;
                                                _TourDateEntity.VenueID = _VenuEntity.VenueID;
                                                _TourDateEntity.Score = _Get_Events_ByID.score;

                                                _TourDateEntity.Announce_Date = Convert.ToDateTime(_Get_Events_ByID.announce_date);
                                                _TourDateEntity.Visible_Until_utc = Convert.ToDateTime(_Get_Events_ByID.visible_until_utc);
                                                _TourDateEntity.Tour_Utcdate = Convert.ToDateTime(_Get_Events_ByID.datetime_utc);

                                                _TourDateEntity.Datetime_Local = Convert.ToDateTime(_Get_Events_ByID.datetime_local);

                                                _TourDateEntity.ModifiedDate = DateTime.Now;

                                                if (!String.IsNullOrEmpty(_Get_Events_ByID.url))
                                                    _TourDateEntity.TicketURL = _Get_Events_ByID.url;
                                                else if (_Get_Events_ByID.venue != null && !String.IsNullOrEmpty(_Get_Events_ByID.venue.url))
                                                    _TourDateEntity.TicketURL = _Get_Events_ByID.venue.url;
                                                else
                                                    _TourDateEntity.TicketURL = "https://seatgeek.com/";

                                                _TourDateRepo.Repository.Update(_TourDateEntity);

                                            }
                                            #endregion
                                        }
                                        else
                                        {
                                            #region "Save Other Artist in Tour Perfromer"
                                            _TourPerformers = _TourPerformersRepo.Repository.Get(p => p.ArtistID == _Artists.ArtistID && p.TourDateID == _TourDateEntity.TourDateID);

                                            if (_TourPerformers == null)
                                            {
                                                _TourPerformers = new TourPerformers();

                                                _TourPerformers.TourDateID = _TourDateEntity.TourDateID;
                                                _TourPerformers.ArtistID = _Artists.ArtistID;
                                                _TourPerformers.CreatedDate = DateTime.Now;
                                                _TourPerformers.RecordStatus = RecordStatus.MusicGraph.ToString();

                                                _TourPerformersRepo.Repository.Add(_TourPerformers);
                                            }
                                            #endregion
                                        }

                                        string MusicGraphBio = "";
                                        if (!String.IsNullOrEmpty(_Artists.Musicgraph_ID))
                                            MusicGraphBio = GetMusicGraphBio(_Artists.Musicgraph_ID);
                                        if (!String.IsNullOrEmpty(MusicGraphBio))
                                            _Artists.About = MusicGraphBio;

                                        _ArtistsRepo.Repository.Update(_Artists);

                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                    #endregion
                                }
                            }
                            #endregion



                        }
                        #endregion
                    }
                    else
                    {

                        await Task.Factory.StartNew(() =>
                        {
                            #region "Instagram Api Implementation"
                            Task<bool> _Instagram = Instagram_GetPictures_Asyn(UserID, _Artists, null, _unitOfWork, false, _TourDateEntity);
                            #endregion

                            _Instagram.Wait();
                        });

                        #region "EventFul"
                        if (_TourDateEntity.Eventful_TourID != null)
                        {

                        }
                        #endregion


                        #region "SeetGeek"
                        if (_TourDateEntity.SeatGeek_TourID != null)
                        {

                        }
                        #endregion


                    }

                    _unitOfWork.Commit();

                    if (_TourDateEntity != null)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, GetTourDetail_ByID(_TourDateEntity, UserID), "EventsDetail"));
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, "Event not found", "EventsDetail"));
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.CreateLog3(ex, Request);
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, ex.Message, "EventsDetail"));
                }


                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.InvalidSearchCriteria, "EventsDetail"));
            }
            catch (Exception ex)
            {
                LogHelper.CreateLog3(ex, Request);
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, ex.Message, "EventsDetail"));
            }


        }


        public ViewEventDetail GetTourDetail_ByID(TourDate _TourDate, int UserID)
        {
            ViewEventDetail _ViewEventDetail = new ViewEventDetail();

            GenericRepository<TourDate> _TourDateRepo = new GenericRepository<TourDate>(_unitOfWork);
            GenericRepository<ArtistPhotos> _ArtistPhotosRepo = new GenericRepository<ArtistPhotos>(_unitOfWork);
            GenericRepository<TourPhoto> _TourPhotosRepo = new GenericRepository<TourPhoto>(_unitOfWork);
            GenericRepository<ArtistRelated> _ArtistRelatedRepo = new GenericRepository<ArtistRelated>(_unitOfWork);
            GenericRepository<Venue> _VenueRepo = new GenericRepository<Venue>(_unitOfWork);
            GenericRepository<Artists> _ArtistsRepo = new GenericRepository<Artists>(_unitOfWork);
            GenericRepository<UserTourDate> UserTourDateRepo = new GenericRepository<UserTourDate>(_unitOfWork);
            GenericRepository<UserFriends> _UserFriendsRepo = new GenericRepository<UserFriends>(_unitOfWork);
            GenericRepository<UserGoing> _UserGoingRepo = new GenericRepository<UserGoing>(_unitOfWork);
            GenericRepository<Users> _UsersRepo = new GenericRepository<Users>(_unitOfWork);
            GenericRepository<UserTourDate> _UserTourDateRepo = new GenericRepository<UserTourDate>(_unitOfWork);

            Models.UserTourDate _UserTourDate = null;
            Models.Artists _Artist = null;

            var _users = _UsersRepo.Repository.GetById(UserID);

            _Artist = _ArtistsRepo.Repository.Get(p => p.ArtistID == _TourDate.ArtistID);

            _ViewEventDetail.TourID = _TourDate.TourDateID;
            _ViewEventDetail.ArtistID = _Artist.ArtistID;
            _ViewEventDetail.ArtistName = _Artist.ArtistName;

            _ViewEventDetail.About = _Artist.AboutES != null && _users.UserLanguage == EUserLanguage.ES.ToString() ? _Artist.AboutES : _Artist.About;

            if (_ViewEventDetail.About == null)
                _ViewEventDetail.About = "";

            _ViewEventDetail.ImageURL = _Artist.ImageURL ?? "";
            _ViewEventDetail.BannerImage_URL = _Artist.ImageURL ?? "";

            _ViewEventDetail.OnTour = _Artist.OnTour;
            _ViewEventDetail.Gender = _Artist.Gender ?? "";
            _ViewEventDetail.Main_Genre = _Artist.Main_Genre ?? "";
            _ViewEventDetail.Decade = _Artist.Decade ?? "";
            _ViewEventDetail.Alternate_Names = _Artist.Alternate_Names ?? "";
            _ViewEventDetail.CreatedDate = _Artist.CreatedDate;
            _ViewEventDetail.ModifiedDate = _Artist.ModifiedDate;
            _ViewEventDetail.RecordStatus = _Artist.RecordStatus;
            _ViewEventDetail.Instagram_Tag = _Artist.Instagram_Tag != null && _Artist.Instagram_Tag != "" ? _Artist.Instagram_Tag : _Artist.ArtistName;


            _UserTourDate = _UserTourDateRepo.Repository.Get(p => p.TourDateID == _TourDate.TourDateID && p.UserID == UserID && p.RecordStatus != RecordStatus.Deleted.ToString());

            if (_UserTourDate == null)
            {
                _ViewEventDetail.IsTracking = false;
            }
            else
            {
                _ViewEventDetail.IsTracking = true;
            }

            _ViewEventDetail.Date_Local = Convert.ToDateTime(_TourDate.Datetime_Local).ToString("d");
            _ViewEventDetail.Time_Local = Convert.ToDateTime(_TourDate.Datetime_Local).ToString("t");

            _ViewEventDetail.TicketURL = _TourDate.TicketURL;

            Models.UserGoing _UserGoing = null;
            _UserGoing = _UserGoingRepo.Repository.Get(p => p.TourDateID == _TourDate.TourDateID && p.UserID == UserID);

            if (_UserGoing != null)
            {
                _ViewEventDetail.IsGoing = _UserGoing.RecordStatus == EUserGoing.Going.ToString() ? true : false;
            }
            else
            {
                _ViewEventDetail.IsGoing = false;
            }



            //    Models.UserTracking _UserTracking = null;
            //_UserTracking = _UserTrackingRepo.Repository.Get(p => p.ArtistID == _Artist.ArtistID);

            //if (_UserTracking != null)
            //{
            //    _ViewEventDetail.IsTracking = true;
            //}
            //else {
            //    _ViewEventDetail.IsTracking = false;

            Models.Venue _Venue = null;
            _Venue = _VenueRepo.Repository.Get(p => p.VenueID == _TourDate.VenueID);

            ViewEventVenue _ViewEventVenue = new ViewEventVenue();
            if (_Venue != null)
            {


                _ViewEventVenue.Address = _Venue.Address ?? "";
                _ViewEventVenue.VenueID = _Venue.VenueID;
                _ViewEventVenue.VenueName = _Venue.VenueName ?? "";
                _ViewEventVenue.ImageURL = _Venue.ImageURL ?? "";
                _ViewEventVenue.Extended_Address = _Venue.Extended_Address ?? "";
                _ViewEventVenue.Display_Location = _Venue.Display_Location ?? "";
                _ViewEventVenue.Slug = _Venue.Slug ?? "";
                _ViewEventVenue.Postal_Code = _Venue.Postal_Code ?? "";
                _ViewEventVenue.Address = _Venue.Address ?? "";
                _ViewEventVenue.Timezone = _Venue.Timezone ?? "";
                _ViewEventVenue.VenueCity = _Venue.VenueCity ?? "";
                _ViewEventVenue.VenueState = _Venue.VenueState ?? "";
                _ViewEventVenue.VenueCountry = _Venue.VenueCountry ?? "";
                _ViewEventVenue.VenueLat = _Venue.VenueLat ?? 0;
                _ViewEventVenue.VenueLong = _Venue.VenueLong ?? 0;


            }
            _ViewEventDetail.Venue = _ViewEventVenue;

            if (!String.IsNullOrEmpty(_TourDate.HashTag))
            {
                _ViewEventDetail.TourPhotos = (from A in _TourPhotosRepo.Repository.GetAll(p => p.TourDateId == _TourDate.TourDateID)
                                               select new viewEventTourPhoto
                                               {
                                                   ImageURL = A.ImageURL,
                                                   ImageThumbnailURL = A.ImageThumbnailURL,
                                                   HashTagName = _TourDate.HashTag,
                                                   PhotoID = A.TourPhotoID
                                               }).OrderByDescending(p => p.PhotoID).ToList();
            }
            else
            {
                //Get HashTags pictures
                _ViewEventDetail.TourPhotos = (from A in _ArtistPhotosRepo.Repository.GetAll(p => p.ArtistID == _Artist.ArtistID && p.RecordStatus == RecordStatus.Active.ToString())
                                               select new viewEventTourPhoto
                                               {
                                                   ImageURL = A.ImageUrl,
                                                   ImageThumbnailURL = A.ImageThumbnailUrl,
                                                   HashTagName = A.HashTagName ?? "",
                                                   PhotoID = A.PhotoID
                                               }).OrderByDescending(p => p.PhotoID).ToList();
            }

            var _lst = (from A1 in _UserFriendsRepo.Repository.GetAll(p => p.UserID == UserID && p.Matched_UserID != null)
                        join A in _UserGoingRepo.Repository.GetAll(p => p.TourDateID == _TourDate.TourDateID && p.RecordStatus == EUserGoing.Going.ToString()) on A1.Matched_UserID equals A.UserID
                        join B in _UsersRepo.Repository.GetAll() on A.UserID equals B.UserID
                        select new ViewEventUsers
                        {
                            ThumbnailURL = B.ThumbnailURL ?? "",
                            UserID = A.UserID.Value,
                            UserName = B.UserName.ToString(),
                            Email = B.Email ?? "",
                            CreatedDate = Convert.ToDateTime(A.CreatedDate).ToString("d")
                        }).OrderByDescending(p => p.CreatedDate).ToList();



            //who else is going
            _ViewEventDetail.UsersGoing = _lst.ToList();

            _ViewEventDetail.NoOfUserGoing = _lst.Count();


            //Get Similar Artists
            _ViewEventDetail.ArtistRelated = (from A in _ArtistRelatedRepo.Repository.GetAll(p => p.ArtistID == _Artist.ArtistID)
                                              select new viewEventRelated
                                              {
                                                  Musicgraph_ID = A.Musicgraph_ID,
                                                  RelatedArtistName = A.RelatedArtistName,
                                                  Similarity = A.Similarity
                                              }).ToList();

            return _ViewEventDetail;
        }


        [HttpPost]
        [Route("api/Events/UpdateIsGoing")]
        public HttpResponseMessage UpdateIsGoing(InputUpdateisGoing input)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.OK, ModelState);
                }

                GenericRepository<Users> _UsersRepo = new GenericRepository<Users>(_unitOfWork);
                GenericRepository<UserGoing> _UserGoingRepo = new GenericRepository<UserGoing>(_unitOfWork);

                Models.Users _Users = null;
                Models.UserGoing _UserGoing = null;

                _Users = _UsersRepo.Repository.Get(p => p.UserID == input.UserID);

                if (_Users == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.UserNotFound, "IsGoing"));
                }


                //User Going
                if (input.UserGoing.ToString() == EUserGoing.Going.ToString())
                {
                    _UserGoing = _UserGoingRepo.Repository.Get(p => p.TourDateID == input.TourID && p.UserID == input.UserID);
                    if (_UserGoing == null)
                    {
                        _UserGoing = new UserGoing();
                        _UserGoing.UserID = input.UserID;
                        _UserGoing.TourDateID = input.TourID;
                        _UserGoing.CreatedDate = DateTime.Now;
                        _UserGoing.RecordStatus = input.UserGoing.ToString();

                        _UserGoingRepo.Repository.Add(_UserGoing);
                    }
                    else
                    {
                        _UserGoing.RecordStatus = input.UserGoing.ToString();
                        _UserGoing.ModifiedDate = DateTime.Now;

                        _UserGoingRepo.Repository.Update(_UserGoing);
                    }

                }



                //User Not Going
                if (input.UserGoing.ToString() == EUserGoing.NotGoing.ToString())
                {
                    _UserGoing = _UserGoingRepo.Repository.Get(p => p.TourDateID == input.TourID && p.UserID == input.UserID);
                    if (_UserGoing == null)
                    {
                        _UserGoing = new UserGoing();
                        _UserGoing.UserID = input.UserID;
                        _UserGoing.TourDateID = input.TourID;
                        _UserGoing.CreatedDate = DateTime.Now;
                        _UserGoing.RecordStatus = input.UserGoing.ToString();

                        _UserGoingRepo.Repository.Add(_UserGoing);
                    }
                    else
                    {
                        _UserGoing.RecordStatus = input.UserGoing.ToString();
                        _UserGoing.ModifiedDate = DateTime.Now;

                        _UserGoingRepo.Repository.Update(_UserGoing);
                    }


                }


                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, input.UserGoing.ToString(), "IsGoing"));

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, ex.Message, "IsGoing"));

            }
        }


        [HttpGet]
        [Route("api/Events/GetPeopleGoing")]
        public HttpResponseMessage GetPeopleGoing(Int32 TourID, int UserID, int Pageindex, int Pagesize)
        {

            try
            {
                ViewPeopleGoing _ViewPeopleGoing = new ViewPeopleGoing();

                GenericRepository<TourDate> _TourDateRepo = new GenericRepository<TourDate>(_unitOfWork);
                GenericRepository<Venue> _VenueRepo = new GenericRepository<Venue>(_unitOfWork);
                GenericRepository<Artists> _ArtistsRepo = new GenericRepository<Artists>(_unitOfWork);
                GenericRepository<UserGoing> _UserGoingRepo = new GenericRepository<UserGoing>(_unitOfWork);
                GenericRepository<Users> _UsersRepo = new GenericRepository<Users>(_unitOfWork);

                GenericRepository<UserFriends> _UserFriendsRepo = new GenericRepository<UserFriends>(_unitOfWork);



                Models.Artists _Artist = null;
                Models.TourDate _TourDate = null;

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



        [HttpPost]
        [Route("api/Events/UpdateTrackEvent")]
        public HttpResponseMessage UpdateTrackEvent(InputTrackEvent input)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.OK, ModelState);
                }

                GenericRepository<Users> _UsersRepo = new GenericRepository<Users>(_unitOfWork);
                GenericRepository<TourDate> _TourDateRepo = new GenericRepository<TourDate>(_unitOfWork);
                GenericRepository<UserTourDate> _UserTourDateRepo = new GenericRepository<UserTourDate>(_unitOfWork);


                Models.Users _Users = null;
                Models.TourDate _TourDate = null;
                Models.UserTourDate _UserTourDate = null;

                _TourDate = _TourDateRepo.Repository.Get(p => p.TourDateID == input.TourID);
                if (_TourDate == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.EventNotDound, "TrackEvent"));
                }

                _UserTourDate = _UserTourDateRepo.Repository.Get(p => p.TourDateID == input.TourID && p.UserID == input.UserID);

                //Tracking
                if (input.TrackEvent.ToString() == ETracking.Tracking.ToString())
                {
                    if (_UserTourDate == null)
                    {
                        _UserTourDate = new UserTourDate();
                        _UserTourDate.TourDateID = _TourDate.TourDateID;
                        _UserTourDate.UserID = input.UserID;
                        _UserTourDate.CreatedDate = DateTime.Now;
                        _UserTourDate.RecordStatus = RecordStatus.Active.ToString();

                        _UserTourDateRepo.Repository.Add(_UserTourDate);
                    }
                }
                else
                {
                    if (_UserTourDate != null)
                    {
                        _UserTourDateRepo.Repository.DeletePermanent(_UserTourDate.UserTourDateID);
                    }
                }

                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, input.TrackEvent.ToString(), "TrackEvent"));
            }
            catch (Exception ex)
            {
                LogHelper.CreateLog3(ex, Request);
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, ex.Message, "TrackEvent"));

            }
        }



        #endregion


        #region "Venue"

        [HttpGet]
        [Route("api/Venue/GetVenueDetail")]
        public HttpResponseMessage GetVenueDetail(Int32 VenueID, Int32 UserID, Int16 Pageindex, Int16 Pagesize)
        {

            try
            {
                ViewVenueDetail _ViewVenueDetail = new ViewVenueDetail();

                GenericRepository<TourDate> _TourDateRepo = new GenericRepository<TourDate>(_unitOfWork);
                GenericRepository<Venue> _VenueRepo = new GenericRepository<Venue>(_unitOfWork);
                GenericRepository<Artists> _ArtistsRepo = new GenericRepository<Artists>(_unitOfWork);
                GenericRepository<UserGoing> _UserGoingRepo = new GenericRepository<UserGoing>(_unitOfWork);
                GenericRepository<Users> _UsersRepo = new GenericRepository<Users>(_unitOfWork);


                Models.Artists _Artist = null;
                Models.TourDate _TourDate = null;
                Models.Users _Users = null;
                Models.Venue _Venue = null;

                _Users = _UsersRepo.Repository.Get(p => p.UserID == UserID);

                if (_Users == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.UserNotFound, "VenueDetail"));
                }


                _Venue = _VenueRepo.Repository.Get(p => p.VenueID == VenueID);

                if (_Venue == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.VenueNotFound, "VenueDetail"));
                }

                _ViewVenueDetail.VenueID = _Venue.VenueID;
                _ViewVenueDetail.VenueName = _Venue.VenueName ?? "";
                _ViewVenueDetail.ImageURL = _Venue.ImageURL ?? WebConfigurationManager.AppSettings["WebPath"].ToString() + "/Content/Upload/Default-Venue.jpg";
                _ViewVenueDetail.Extended_Address = _Venue.Extended_Address ?? "";
                _ViewVenueDetail.Display_Location = _Venue.Display_Location ?? "";
                _ViewVenueDetail.Slug = _Venue.Slug ?? "";
                _ViewVenueDetail.Postal_Code = _Venue.Postal_Code ?? "";
                _ViewVenueDetail.Address = _Venue.Address ?? "";
                _ViewVenueDetail.Timezone = _Venue.Timezone ?? "";
                _ViewVenueDetail.VenueCity = _Venue.VenueCity ?? "";
                _ViewVenueDetail.VenueState = _Venue.VenueState ?? "";
                _ViewVenueDetail.VenueCountry = _Venue.VenueCountry ?? "";
                _ViewVenueDetail.VenueLat = _Venue.VenueLat ?? 0;
                _ViewVenueDetail.VenueLong = _Venue.VenueLong ?? 0;




                //Upcoming Events
                _ViewVenueDetail.UpcomingEvents = (from A in _TourDateRepo.Repository.GetAll(p => p.VenueID == _Venue.VenueID)
                                                   join B in _ArtistsRepo.Repository.GetAll() on A.ArtistID equals B.ArtistID
                                                   where !A.IsDeleted && (A.Visible_Until_utc >= DateTime.UtcNow &&
                                                           A.Visible_Until_utc <= DateTime.UtcNow.AddDays(180))
                                                   orderby A.Visible_Until_utc
                                                   select new ViewVenueTours
                                                   {
                                                       TourDateID = A.TourDateID,
                                                       ArtistID = A.ArtistID,
                                                       ArtistName = B.ArtistName,
                                                       ImageURL = B.ImageURL ?? "",
                                                       BannerImage_URL = B.BannerImage_URL ?? "",
                                                       Date_Local = Convert.ToDateTime(A.Datetime_Local).ToString("d"),
                                                       Time_Local = Convert.ToDateTime(A.Datetime_Local).ToString("t")
                                                   }).ToPagedList(Pageindex - 1, Pagesize).ToList();




                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, _ViewVenueDetail, "VenueDetail"));

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, ex.Message, "VenueDetail"));

            }

        }


        [HttpGet]
        [Route("api/Venue/GetYourPlans")]
        public HttpResponseMessage GetYourPlans(Int32 UserID, Int16 Pageindex, Int16 Pagesize)
        {

            try
            {
                ViewYourPlans _ViewYourPlansDetail = new ViewYourPlans();

                GenericRepository<TourDate> _TourDateRepo = new GenericRepository<TourDate>(_unitOfWork);
                GenericRepository<Venue> _VenueRepo = new GenericRepository<Venue>(_unitOfWork);
                GenericRepository<Artists> _ArtistsRepo = new GenericRepository<Artists>(_unitOfWork);
                GenericRepository<UserGoing> _UserGoingRepo = new GenericRepository<UserGoing>(_unitOfWork);
                GenericRepository<Users> _UsersRepo = new GenericRepository<Users>(_unitOfWork);


                Models.Artists _Artist = null;
                Models.TourDate _TourDate = null;
                Models.Users _Users = null;
                Models.Venue _Venue = null;

                _Users = _UsersRepo.Repository.Get(p => p.UserID == UserID);

                if (_Users == null || _Users.RecordStatus != RecordStatus.Active.ToString())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.UserNotFound, "YourPlans"));
                }

                _ViewYourPlansDetail.UserID = _Users.UserID;
                _ViewYourPlansDetail.UserName = _Users.UserName;
                _ViewYourPlansDetail.Email = _Users.Email;
                _ViewYourPlansDetail.ThumbnailURL = _Users.ThumbnailURL;
                _ViewYourPlansDetail.ImageURL = _Users.ImageURL;


                //Upcoming Events
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
                                                  ImageURL = C.ImageURL ?? "",
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


        [HttpGet]
        [Route("api/Venue/GetYourFriendPlans")]
        public HttpResponseMessage GetYourFriendPlans(Int32 UserID, Int16 Pageindex, Int16 Pagesize)
        {

            try
            {
                List<ViewYourFreiendplanlst> _ViewYourFriendPlans = new List<ViewYourFreiendplanlst>();

                GenericRepository<TourDate> _TourDateRepo = new GenericRepository<TourDate>(_unitOfWork);
                GenericRepository<Venue> _VenueRepo = new GenericRepository<Venue>(_unitOfWork);
                GenericRepository<Artists> _ArtistsRepo = new GenericRepository<Artists>(_unitOfWork);
                GenericRepository<UserGoing> _UserGoingRepo = new GenericRepository<UserGoing>(_unitOfWork);
                GenericRepository<Users> _UsersRepo = new GenericRepository<Users>(_unitOfWork);
                GenericRepository<UserFriends> _UserFriendsRepo = new GenericRepository<UserFriends>(_unitOfWork);


                Models.Artists _Artist = null;
                Models.TourDate _TourDate = null;
                Models.Users _Users = null;
                Models.Venue _Venue = null;

                _Users = _UsersRepo.Repository.Get(p => p.UserID == UserID);

                if (_Users == null || _Users.RecordStatus != RecordStatus.Active.ToString())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.UserNotFound, "FriendPlans"));
                }


                var _GoingCount =
                               (from A1 in _UserFriendsRepo.Repository.GetAll(p => p.UserID == UserID && p.Matched_UserID != null)
                                join p in _UserGoingRepo.Repository.GetAll(p => p.RecordStatus == EUserGoing.Going.ToString()) on A1.Matched_UserID equals p.UserID
                                group p by p.TourDateID into g
                                select new { TourID = g.Key, GiongCount = g.Count() }).AsQueryable();


                //Upcoming Events
                _ViewYourFriendPlans = (from A in _GoingCount.Where(p => p.GiongCount > 0)
                                            // join B in _TourDateRepo.Repository.GetAll() on A.TourID equals B.TourDateID
                                        join B in _TourDateRepo.Repository.GetAll(p => p.Tour_Utcdate >= DateTime.UtcNow && !p.IsDeleted) on A.TourID equals B.TourDateID
                                        // join B in _TourDateRepo.Repository.GetAll() on A.TourID equals B.TourDateID
                                        join C in _ArtistsRepo.Repository.GetAll() on B.ArtistID equals C.ArtistID
                                        join D in _VenueRepo.Repository.GetAll() on B.VenueID equals D.VenueID
                                        where B.TourDateID == A.TourID
                                        orderby B.Visible_Until_utc descending
                                        select new ViewYourFreiendplanlst
                                        {
                                            TourDateID = B.TourDateID,
                                            ArtistID = B.ArtistID,
                                            ArtistName = C.ArtistName,
                                            ImageURL = C.ImageURL ?? "",
                                            BannerImage_URL = C.BannerImage_URL ?? "",
                                            Date_Local = Convert.ToDateTime(B.Datetime_Local).ToString("d"),
                                            Time_Local = Convert.ToDateTime(B.Datetime_Local).ToString("t"),
                                            VenueID = B.VenueID,
                                            VenueName = D.VenueName,
                                            Datetime_Local = B.Datetime_Local.Value,
                                            GoingCount = A.GiongCount > 3 ? A.GiongCount - 3 : 0,
                                            Going = (from E in _UserFriendsRepo.Repository.GetAll(p => p.UserID == UserID && p.Matched_UserID != null)
                                                     join F in _UserGoingRepo.Repository.GetAll(p => p.RecordStatus == EUserGoing.Going.ToString() && p.TourDateID == B.TourDateID) on E.Matched_UserID equals F.UserID
                                                     join G in _UsersRepo.Repository.GetAll() on E.Matched_UserID equals G.UserID
                                                     orderby F.GoingID descending
                                                     select new ViewFriendPlans
                                                     {
                                                         Email = G.Email ?? "",
                                                         ImageURL = G.ImageURL ?? "",
                                                         ThumbnailURL = G.ThumbnailURL ?? "",
                                                         UserID = G.UserID,
                                                         UserName = G.UserName
                                                     }).ToList()
                                        }).OrderBy(p => p.Datetime_Local).ToPagedList(Pageindex - 1, Pagesize).ToList();




                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, _ViewYourFriendPlans, "FriendPlans"));

            }
            catch (Exception ex)
            {
                LogHelper.CreateLog3(ex, Request);
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, ex.Message, "FriendPlans"));

            }

        }



        [HttpGet]
        [Route("api/Venue/GetDiscoverDetail")]
        public HttpResponseMessage GetDiscoverDetail(Int32 UserID, double Lat, double Lon)
        {
            try
            {

                ViewDiscoverDetail _ViewDiscoverDetail = new ViewDiscoverDetail();
                GenericRepository<Users> _UsersRepo = new GenericRepository<Users>(_unitOfWork);

                Models.Users _Users = null;

                _Users = _UsersRepo.Repository.Get(p => p.UserID == UserID);

                if (_Users == null || _Users.RecordStatus != RecordStatus.Active.ToString())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.UserNotFound, "DiscoverDetail"));
                }

                DataSet ds;
                SpRepository _sp = new SpRepository();
                ds = _sp.SpGetDiscoverDetail(UserID, Lat, Lon, 12);

                if (ds != null)
                {
                    //Most Popular
                    if (ds.Tables.Count >= 1)
                    {
                        _ViewDiscoverDetail.MostPopular = General.DTtoList<ViewDiscoverlst>(ds.Tables[0]);
                    }

                    //Recommended
                    if (ds.Tables.Count >= 2)
                    {
                        _ViewDiscoverDetail.Recommended = General.DTtoList<ViewDiscoverlst>(ds.Tables[1]);
                    }

                    //Hot Events
                    if (ds.Tables.Count >= 3)
                    {
                        _ViewDiscoverDetail.HotNewTour = General.DTtoList<ViewDiscoverlst>(ds.Tables[2]);
                    }
                }


                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, _ViewDiscoverDetail, "DiscoverDetail"));

            }
            catch (Exception ex)
            {
                LogHelper.CreateLog3(ex, Request);
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, ex.Message, "DiscoverDetail"));

            }

        }

        #endregion


        #region "Third party API"

        [HttpGet]
        [Route("api/thirdpartyapi/GetSpotifyAccessToken")]
        public HttpResponseMessage GetSpotifyAccessToken()
        {
            try
            {

                ClientCredentialsAuth _ClientCredentialsAuth = new ClientCredentialsAuth();
                _ClientCredentialsAuth.ClientId = WebConfigurationManager.AppSettings["Spotify_client_id"].ToString();
                _ClientCredentialsAuth.ClientSecret = WebConfigurationManager.AppSettings["Spotify_client_secret"].ToString();
                _ClientCredentialsAuth.Scope = Scope.Streaming;

                //_ClientCredentialsAuth.Scope = Scope.UserLibrarayRead;
                Token _Token = _ClientCredentialsAuth.DoAuth();

                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, _Token, "Spotify"));

            }
            catch (Exception ex)
            {
                LogHelper.CreateLog3(ex, Request);
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Exception, ex.Message));
            }


        }


        public string RetSpotifyAccessToken()
        {
            try
            {

                ClientCredentialsAuth _ClientCredentialsAuth = new ClientCredentialsAuth();
                _ClientCredentialsAuth.ClientId = WebConfigurationManager.AppSettings["Spotify_client_id"].ToString();
                _ClientCredentialsAuth.ClientSecret = WebConfigurationManager.AppSettings["Spotify_client_secret"].ToString();
                _ClientCredentialsAuth.Scope = Scope.Streaming;

                //_ClientCredentialsAuth.Scope = Scope.UserLibrarayRead;
                Token _Token = _ClientCredentialsAuth.DoAuth();

                return _Token.AccessToken;

            }
            catch (Exception ex)
            {
                LogHelper.CreateLog3(ex, Request);
                return "";
            }


        }

        #endregion


        #region "Asynchronous"

        #region "Music Graph"
        private Task<MusicGraph.Search_ByName> MusicGrapgh_GetArtistByName_Asyn(string vMusicgraph_Name)
        {

            string _MusicGrapgh_api_key = ConfigurationManager.AppSettings["MusicGrapgh_api_key"].ToString();
            HttpWebRequest httpWebRequest;
            HttpWebResponse httpResponse;
            string _result;

            //LogHelper.CreateLog("MusicGrapgh_GetArtistByName_Asyn (" + vMusicgraph_Name + ")");

            return Task.Factory.StartNew(() =>
            {
                try
                {
                    httpWebRequest = (HttpWebRequest)WebRequest.Create("http://api.musicgraph.com/api/v2/artist/search?api_key=" + _MusicGrapgh_api_key + "&name=" + vMusicgraph_Name.Trim());
                    httpWebRequest.ContentType = "application/json";
                    httpWebRequest.Method = "GET";

                    httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        _result = streamReader.ReadToEnd();
                    }


                    // deserializing 
                    return JsonConvert.DeserializeObject<MusicGraph.Search_ByName>(_result);

                }
                catch (Exception ex)
                {
                    LogHelper.CreateLog3(ex, Request);
                    LogHelper.CreateLog("MusicGrapgh_GetArtistByName_Asyn " + ex.Message);
                    return null;
                }
            }


                );


        }

        private Task<MusicGraph1.Search_ByID> MusicGrapgh_GetArtistByID_Asyn(string vMusicgraph_ID)
        {

            string _MusicGrapgh_api_key = ConfigurationManager.AppSettings["MusicGrapgh_api_key"].ToString();

            //LogHelper.CreateLog("MusicGrapgh_GetArtistByID_Asyn (" + vMusicgraph_ID + ")");

            return Task.Factory.StartNew(() =>
            {
                try
                {

                    HttpWebRequest httpWebRequest;
                    HttpWebResponse httpResponse;
                    string _result;
                    MusicGraph1.Search_ByID _Search_ByID = null;

                    httpWebRequest = (HttpWebRequest)WebRequest.Create("http://api.musicgraph.com/api/v2/artist/" + vMusicgraph_ID + "?api_key=" + _MusicGrapgh_api_key);
                    httpWebRequest.ContentType = "application/json";
                    httpWebRequest.Method = "GET";

                    httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        _result = streamReader.ReadToEnd();
                    }

                    // deserializing 
                    _Search_ByID = JsonConvert.DeserializeObject<MusicGraph1.Search_ByID>(_result);

                    return _Search_ByID;

                }
                catch (Exception ex)
                {
                    LogHelper.CreateLog3(ex, Request);
                    LogHelper.CreateLog("MusicGrapgh_GetArtistByID_Asyn " + ex.Message);
                    return null;
                }
            }


                );


        }

        private async Task<MusicGraph5.GetSimilarArtists_ByID> MusicGrapgh_GetSimilarArtists_Asyn(string vMusicgraph_ID, Int32 vArtistID, IUnitOfWork vUnitOfWork)
        {

            string _MusicGrapgh_api_key = ConfigurationManager.AppSettings["MusicGrapgh_api_key"].ToString();
            GenericRepository<ArtistRelated> _ArtistRelatedRepo = new GenericRepository<ArtistRelated>(vUnitOfWork);
            GenericRepository<ArtistsNotLatin> _ArtistNotLatinRepo = new GenericRepository<ArtistsNotLatin>(vUnitOfWork);

            MusicGraph5.GetSimilarArtists_ByID _GetSimilarArtists_ByID = await Task.Factory.StartNew(() =>
            {

                try
                {

                    HttpWebRequest httpWebRequest;
                    HttpWebResponse httpResponse = null;
                    string _result;

                    httpWebRequest = (HttpWebRequest)WebRequest.Create("http://api.musicgraph.com/api/v2/artist/" + vMusicgraph_ID + "/similar?api_key=" + _MusicGrapgh_api_key);
                    httpWebRequest.ContentType = "application/json";
                    httpWebRequest.Method = "GET";

                    for (int i = 0; i <= 1; i++)
                    {
                        try
                        {
                            httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                            if (httpResponse != null)
                                break;
                        }
                        catch (Exception e)
                        {
                            if (i == 1)
                            {
                                LogHelper.CreateLog("MusicGrapgh_GetSimilarArtists_Asyn " + e.Message);
                                return null;
                            }

                            Thread.Sleep(2000);
                        }
                    }

                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        _result = streamReader.ReadToEnd();
                    }


                    // deserializing 
                    return JsonConvert.DeserializeObject<MusicGraph5.GetSimilarArtists_ByID>(_result);

                }
                catch (Exception ex)
                {
                    LogHelper.CreateLog3(ex, Request);
                    LogHelper.CreateLog("MusicGrapgh_GetSimilarArtists_Asyn " + ex.Message);
                    return null;
                }
            }
            );

            #region "Continue if above function return successfully"

            await Task.Factory.StartNew(() =>
            {
                Models.ArtistRelated _ArtistRelatedEntity = null;

                if (_GetSimilarArtists_ByID != null)
                {

                    if (_GetSimilarArtists_ByID.data.Count > 0)
                    {
                        var _similarlst = (from A in _GetSimilarArtists_ByID.data
                                           select A).OrderByDescending(p => p.similarity).Take(5).ToList();


                        foreach (MusicGraph5.Datum _related in _similarlst)
                        {
                            if (!_ArtistNotLatinRepo.Repository.AsQueryable().Any(x => x.ArtistName == _related.name))
                            {
                                bool isLatin = false;

                                isLatin = CheckMusicGraphLatin(_related.id, _unitOfWork);

                                if (!isLatin)
                                {
                                    Task<bool> _mytastk = SeatGeek_CheckLatinGenre_Asyn(_related.name, _unitOfWork);
                                    isLatin = _mytastk.Result;
                                }
                                if (!isLatin)
                                {
                                    isLatin = CheckLastResortSpotifyGenre(_related.spotify_id);

                                }

                                if (isLatin)
                                {
                                    _ArtistRelatedEntity = new ArtistRelated();
                                    _ArtistRelatedEntity.Musicgraph_ID = !String.IsNullOrEmpty(_related.id) ? _related.id : _related.name;
                                    _ArtistRelatedEntity.RelatedArtistName = _related.name;
                                    _ArtistRelatedEntity.Similarity = _related.similarity;

                                    _ArtistRelatedEntity.ArtistID = vArtistID;
                                    _ArtistRelatedEntity.CreatedDate = DateTime.Now;
                                    _ArtistRelatedEntity.RecordStatus = RecordStatus.MusicGraph.ToString();

                                    _ArtistRelatedRepo.Repository.Add(_ArtistRelatedEntity);
                                }
                                else
                                {

                                    if (!_ArtistNotLatinRepo.Repository.AsQueryable().Any(x => x.ArtistName == _related.name))
                                        _ArtistNotLatinRepo.Repository.Add(new ArtistsNotLatin() { ArtistName = _related.name });
                                }
                            }
                        }
                    }
                }
            });

            #endregion


            return _GetSimilarArtists_ByID;
        }

        private string GetMusicGraphBio(string vMusicgraph_ID)
        {
            try
            {
                string _MusicGrapgh_api_key = ConfigurationManager.AppSettings["MusicGrapgh_api_key"].ToString();

                HttpWebRequest httpWebRequest;
                HttpWebResponse httpResponse;
                string _result;
                string bioNoTags = "";
                string bioWithTags = "";

                httpWebRequest = (HttpWebRequest)WebRequest.Create("http://api.musicgraph.com/api/v2/artist/" + vMusicgraph_ID + "/biography?api_key=" + _MusicGrapgh_api_key);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "GET";

                httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    _result = streamReader.ReadToEnd();
                }

                if (!String.IsNullOrEmpty(_result))
                {
                    var job = JObject.Parse(_result);
                    var data = job.SelectToken("data.artist_bio_nlp_no_tags");


                    if (data != null)
                    {
                        bioNoTags = data.Value<string>();
                    }

                    if (data == null || String.IsNullOrEmpty(bioNoTags))
                    {
                        data = job.SelectToken("data.artist_bio_nlp");
                        if (data != null)
                        {
                            bioWithTags = data.Value<string>();
                            if (!String.IsNullOrEmpty(bioWithTags))
                            {
                                string input = bioWithTags;
                                string regex = "\\[\\S*_?\\S*\\]\\b|\\[\\S*_?\\S*\\]";//"(\\[.*\\])";
                                bioWithTags = Regex.Replace(input, regex, "");
                                return bioWithTags;
                            }
                        }

                    }
                    else
                        return bioNoTags;

                }
            }
            catch (Exception e)
            { }

            return "";
        }

        private Task<MusicGraph2.ArtistMatrics_ByID> MusicGrapgh_GetArtistMatrics_Asyn(string vMusicgraph_ID, Int32 vArtistID, IUnitOfWork vUnitOfWork)
        {

            string _MusicGrapgh_api_key = ConfigurationManager.AppSettings["MusicGrapgh_api_key"].ToString();

            return Task.Factory.StartNew(() =>
            {
                try
                {

                    HttpWebRequest httpWebRequest;
                    HttpWebResponse httpResponse;
                    string _result;


                    httpWebRequest = (HttpWebRequest)WebRequest.Create("http://api.musicgraph.com/api/v2/artist/" + vMusicgraph_ID + "/metrics?api_key=" + _MusicGrapgh_api_key);
                    httpWebRequest.ContentType = "application/json";
                    httpWebRequest.Method = "GET";

                    httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        _result = streamReader.ReadToEnd();
                    }

                    // deserializing 
                    return JsonConvert.DeserializeObject<MusicGraph2.ArtistMatrics_ByID>(_result);
                }
                catch (Exception ex)
                {
                    LogHelper.CreateLog3(ex, Request);
                    LogHelper.CreateLog("MusicGrapgh_GetArtistMatrics_Asyn " + ex.Message);
                    return null;
                }
            }
            );

        }

        #endregion

        #region "Instagram"


        private void GetImagesFromInstagramFeed_Depricated(Artists artist)
        {
            GenericRepository<ArtistPhotos> _ArtistPhotosRepository = new GenericRepository<ArtistPhotos>(_unitOfWork);
            int photoLikeRequirement = 0;
            int totalPhotosAllowed = 30;

            try
            {
                //List<ArtistPhotos> artistTourPhotos = new List<ArtistPhotos>();
                var currentDate = DateTime.Now;

                var artistTourPhotos = _ArtistPhotosRepository.Repository.GetAll(x => x.ArtistID == artist.ArtistID && x.RecordStatus == RecordStatus.Active.ToString()).ToList();
                //var artistTourPhotos = _ArtistPhotosRepository.Repository.AsQueryable().Where(p=>p.ArtistID == artist.ArtistID).ToList();
                var oldRecord = DateTime.Now.AddDays(-7);



                foreach (var tourphoto in artistTourPhotos)
                {
                    if (tourphoto.ModifiedDate < oldRecord)
                    {
                        tourphoto.RecordStatus = RecordStatus.InActive.ToString();
                        tourphoto.ModifiedDate = currentDate;
                        //_ArtistPhotosRepository.Repository.Update(tourphoto);
                    }
                }

                int count = artistTourPhotos.Count(x => x.RecordStatus == RecordStatus.Active.ToString());

                if (count < totalPhotosAllowed)
                {

                    try
                    {
                        string name = artist.ArtistName;


                        name = name.Replace(" ", "");
                        string pattern = @"[^a-zA-Z0-9áéíñóúüÁÉÍÑÓÚÜ\s]";
                        string cleanName = Regex.Replace(name, pattern, "");
                        //x.ArtistName.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)
                        //_ArtistsRepo.Repository.AsQueryable().Where(x => x.ArtistName.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0).Select


                        HttpWebRequest httpWebRequest;
                        HttpWebResponse httpResponse;
                        string _result;

                        if (String.IsNullOrEmpty(artist.Instagram_Url))
                        {
                            httpWebRequest = (HttpWebRequest)WebRequest.Create("https://www.instagram.com/" + cleanName + "/media/");
                            photoLikeRequirement = ConfigurationManager.AppSettings["MinLikesInstagramCalc"].GetInt();
                        }
                        else
                        {
                            httpWebRequest = (HttpWebRequest)WebRequest.Create(artist.Instagram_Url + "/media/");
                            photoLikeRequirement = ConfigurationManager.AppSettings["MinLikesInstagramUrl"].GetInt();
                        }


                        httpWebRequest.ContentType = "application/json";
                        httpWebRequest.Method = "GET";

                        httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                        {
                            _result = streamReader.ReadToEnd();
                        }



                        JObject joResponse = JObject.Parse(_result);
                        JArray items = (JArray)joResponse["items"];

                        count = totalPhotosAllowed - count;

                        foreach (var i in items)
                        {
                            int imagelikes = i["likes"].Value<int>("count");
                            string imageId = i.Value<string>("id");
                            if (imagelikes > photoLikeRequirement)
                            {
                                var artistRecord = artistTourPhotos.FirstOrDefault(x => x.InstagramPostId == imageId);

                                if (artistRecord == null)
                                {
                                    ArtistPhotos tp = new ArtistPhotos();
                                    tp.ImageThumbnailUrl = i["images"]["thumbnail"].Value<string>("url");
                                    tp.ImageUrl = i["images"]["standard_resolution"].Value<string>("url");

                                    try
                                    {
                                        //tp.ImageThumbnailUrl = tp.ImageThumbnailUrl.Remove(tp.ImageThumbnailUrl.LastIndexOf('?'));
                                        tp.ImageThumbnailUrl = tp.ImageThumbnailUrl.Replace("?", "");
                                    }
                                    catch
                                    { }

                                    try
                                    {
                                        // tp.ImageUrl = tp.ImageUrl.Remove(tp.ImageUrl.LastIndexOf('?'));
                                        tp.ImageUrl = tp.ImageUrl.Replace("?", "");
                                    }
                                    catch { }

                                    tp.InstagramPostId = i.Value<string>("id");
                                    tp.ArtistID = artist.ArtistID;
                                    tp.HashTagName = cleanName;
                                    tp.CreatedDate = DateTime.Now;
                                    tp.ModifiedDate = DateTime.Now;
                                    tp.RecordStatus = RecordStatus.Active.ToString();
                                    _ArtistPhotosRepository.Repository.Add(tp);

                                    count--;
                                }
                                else
                                {
                                    if (artistRecord.RecordStatus == RecordStatus.InActive.ToString())
                                    {
                                        artistRecord.RecordStatus = RecordStatus.Active.ToString();
                                        artistRecord.ModifiedDate = DateTime.Now;
                                        _ArtistPhotosRepository.Repository.Update(artistRecord);

                                        count--;
                                    }
                                }

                                if (count == 0)
                                    break;

                            }

                        }
                    }
                    catch (Exception)
                    { }

                    foreach (var a in artistTourPhotos)
                    {
                        //    if (a.RecordStatus == RecordStatus.InActive.ToString() && a.ModifiedDate == currentDate)
                        //    {
                        //        _ArtistPhotosRepository.Repository.Update(a);
                        //    }
                        //    else 
                        if (a.RecordStatus == RecordStatus.InActive.ToString())
                        {
                            _ArtistPhotosRepository.Repository.DeletePermanent(a.PhotoID);
                        }
                    }


                    //JObject ojObject = (JObject)joResponse["items"];
                    //JArray imgs = (JArray)items["images"];
                    //JArray likes = (JArray)items["likes"];


                    // deserializing 
                    //return JsonConvert.DeserializeObject<MusicGraph5.GetSimilarArtists_ByID>(_result);
                }
            }
            catch (Exception ex)
            {
                //  LogHelper.CreateLog("MusicGrapgh_GetSimilarArtists_Asyn " + ex.Message);
                //  return null;
            }


        }

        private void GetImagesFromInstagramFeed(Artists artist)
        {
            GenericRepository<ArtistPhotos> _ArtistPhotosRepository = new GenericRepository<ArtistPhotos>(_unitOfWork);
            int photoLikeRequirement = 0;
            int totalPhotosAllowed = 30;

            try
            {
                //List<ArtistPhotos> artistTourPhotos = new List<ArtistPhotos>();
                var currentDate = DateTime.Now;

                var artistTourPhotos = _ArtistPhotosRepository.Repository.GetAll(x => x.ArtistID == artist.ArtistID && x.RecordStatus == RecordStatus.Active.ToString()).ToList();
                //var artistTourPhotos = _ArtistPhotosRepository.Repository.AsQueryable().Where(p=>p.ArtistID == artist.ArtistID).ToList();
                var oldRecord = DateTime.Now.AddDays(-7);



                foreach (var tourphoto in artistTourPhotos)
                {
                    if (tourphoto.ModifiedDate < oldRecord)
                    {
                        tourphoto.RecordStatus = RecordStatus.InActive.ToString();
                        tourphoto.ModifiedDate = currentDate;
                        //_ArtistPhotosRepository.Repository.Update(tourphoto);
                    }
                }

                int count = artistTourPhotos.Count(x => x.RecordStatus == RecordStatus.Active.ToString());

                if (count < totalPhotosAllowed)
                {

                    try
                    {
                        string name = artist.ArtistName;


                        name = name.Replace(" ", "");
                        string pattern = @"[^a-zA-Z0-9áéíñóúüÁÉÍÑÓÚÜ\s]";
                        string cleanName = Regex.Replace(name, pattern, "");
                        //x.ArtistName.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)
                        //_ArtistsRepo.Repository.AsQueryable().Where(x => x.ArtistName.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0).Select


                        HttpWebRequest httpWebRequest;
                        HttpWebResponse httpResponse;
                        string _result;

                        if (String.IsNullOrEmpty(artist.Instagram_Url))
                        {
                            //httpWebRequest = (HttpWebRequest)WebRequest.Create("https://www.instagram.com/" + cleanName + "/media/");
                            httpWebRequest = (HttpWebRequest)WebRequest.Create("https://www.instagram.com/" + cleanName + "/?__a=1");
                            photoLikeRequirement = ConfigurationManager.AppSettings["MinLikesInstagramCalc"].GetInt();
                        }
                        else
                        {
                            //httpWebRequest = (HttpWebRequest)WebRequest.Create(artist.Instagram_Url + "/media/");
                            httpWebRequest = (HttpWebRequest)WebRequest.Create(artist.Instagram_Url + "/?__a=1");
                            photoLikeRequirement = ConfigurationManager.AppSettings["MinLikesInstagramUrl"].GetInt();
                        }


                        httpWebRequest.ContentType = "application/json";
                        httpWebRequest.Method = "GET";

                        httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                        {
                            _result = streamReader.ReadToEnd();
                        }



                        JObject joResponse = JObject.Parse(_result);
                        JArray items = (JArray)joResponse["user"]["media"]["nodes"];

                        count = totalPhotosAllowed - count;

                        foreach (var i in items)
                        {
                            int imagelikes = i["likes"].Value<int>("count");
                            string imageId = i.Value<string>("id");
                            if (imagelikes > photoLikeRequirement)
                            {
                                var artistRecord = artistTourPhotos.FirstOrDefault(x => x.InstagramPostId == imageId);

                                if (artistRecord == null)
                                {
                                    ArtistPhotos tp = new ArtistPhotos();
                                    tp.ImageThumbnailUrl = i["thumbnail_resources"].ElementAt(0).Value<string>("src");

                                    try
                                    {
                                        tp.ImageUrl = i["thumbnail_resources"].ElementAt(3).Value<string>("src");
                                    }
                                    catch
                                    {
                                        tp.ImageUrl = i["thumbnail_resources"].ElementAt(0).Value<string>("src");
                                    }

                                    try
                                    {
                                        //tp.ImageThumbnailUrl = tp.ImageThumbnailUrl.Remove(tp.ImageThumbnailUrl.LastIndexOf('?'));
                                        tp.ImageThumbnailUrl = tp.ImageThumbnailUrl.Replace("", "");
                                    }
                                    catch
                                    { }

                                    //try
                                    //{
                                    //    // tp.ImageUrl = tp.ImageUrl.Remove(tp.ImageUrl.LastIndexOf('?'));
                                    //    tp.ImageUrl = tp.ImageUrl.Replace("s150x150", "s320x320");
                                    //}
                                    //catch { }

                                    tp.InstagramPostId = i.Value<string>("id");
                                    tp.ArtistID = artist.ArtistID;
                                    tp.HashTagName = cleanName;
                                    tp.CreatedDate = DateTime.Now;
                                    tp.ModifiedDate = DateTime.Now;
                                    tp.RecordStatus = RecordStatus.Active.ToString();
                                    _ArtistPhotosRepository.Repository.Add(tp);

                                    count--;
                                }
                                else
                                {
                                    if (artistRecord.RecordStatus == RecordStatus.InActive.ToString())
                                    {
                                        artistRecord.RecordStatus = RecordStatus.Active.ToString();
                                        artistRecord.ModifiedDate = DateTime.Now;
                                        _ArtistPhotosRepository.Repository.Update(artistRecord);

                                        count--;
                                    }
                                }

                                if (count == 0)
                                    break;

                            }

                        }
                    }
                    catch (Exception)
                    { }

                    foreach (var a in artistTourPhotos)
                    {
                        //    if (a.RecordStatus == RecordStatus.InActive.ToString() && a.ModifiedDate == currentDate)
                        //    {
                        //        _ArtistPhotosRepository.Repository.Update(a);
                        //    }
                        //    else 
                        if (a.RecordStatus == RecordStatus.InActive.ToString())
                        {
                            _ArtistPhotosRepository.Repository.DeletePermanent(a.PhotoID);
                        }
                    }


                    //JObject ojObject = (JObject)joResponse["items"];
                    //JArray imgs = (JArray)items["images"];
                    //JArray likes = (JArray)items["likes"];


                    // deserializing 
                    //return JsonConvert.DeserializeObject<MusicGraph5.GetSimilarArtists_ByID>(_result);
                }
            }
            catch (Exception ex)
            {
                //  LogHelper.CreateLog("MusicGrapgh_GetSimilarArtists_Asyn " + ex.Message);
                //  return null;
            }


        }

        private void GetInstagramHashTagPhotos()
        {
            /*var httpWebRequest = httpWebRequest = (HttpWebRequest)WebRequest.Create("https://api.instagram.com/v1/tags/musikaFake/media/recent?access_token=" + vArtists.Instagram_ID + "/?access_token=" + _Instagram_access_token);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "GET";

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            string _result = "";
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                _result = streamReader.ReadToEnd();
            }
            */

        }

        private async Task<bool> Instagram_GetPictures_Asyn(Int32 vUserID, Artists vArtists, MusicGraph2.ArtistMatrics_ByID vArtistMatrics_ByID, IUnitOfWork vUnitOfWork, bool vNew, TourDate vTour)
        {
            string strThumbnailURLfordb = null;
            string strIamgeURLfordb = null;
            string strTempImageSave = null;

            string tempfilePath = WebConfigurationManager.AppSettings["SiteImgPath"].ToString() + @"\" + "TempImages" + @"\";

            string _SiteRoot = WebConfigurationManager.AppSettings["SiteImgPath"];
            string _SiteURL = WebConfigurationManager.AppSettings["SiteImgURL"];

            HttpWebRequest httpWebRequest;
            HttpWebResponse httpResponse;


            string _Instagram_access_token = ConfigurationManager.AppSettings["instagram.access_token"].ToString();

            Instagram.Instagram_Search _Instagram_Search = null;
            Instagram.Instagram_Search _Instagram_Search2 = null;
            Instagram.Instagram_Search _Instagram_Search3 = null;

            GenericRepository<ArtistPhotos> _ArtistPhotosRepo = new GenericRepository<ArtistPhotos>(vUnitOfWork);
            GenericRepository<TourPhoto> _TourPhotosRepo = new GenericRepository<TourPhoto>(vUnitOfWork);
            GenericRepository<Artists> _ArtistsRepo = new GenericRepository<Artists>(vUnitOfWork);

            #region "New Rec"

            if (vNew == true) //used only for the new Artist 
            {
                #region "Task 1 to get Instagram Detail using Matrics ID"

                Task<Instagram.Instagram_Search> _Instagram_Search_Task1 = Task.Factory.StartNew(() =>
                {
                    try
                    {
                        string _result;
                        if (vArtistMatrics_ByID != null)
                        {
                            if (vArtistMatrics_ByID.data.instagram != null && vArtistMatrics_ByID.data.instagram.url != null && vArtistMatrics_ByID.data.instagram.url != "")
                            {
                                vArtists.Instagram_Url = vArtistMatrics_ByID.data.instagram.url;
                                return _Instagram_Search;

                                string _instaGram_ID = vArtistMatrics_ByID.data.instagram.url.Replace("http:", "https:").Replace("https://instagram.com/", "").Replace("/", "");
                                string _Instagram_Tag = null;

                                #region "Get Instagram ID (dont need this block while just updating the records)"

                                httpWebRequest = (HttpWebRequest)WebRequest.Create("https://api.instagram.com/v1/users/search?q=" + _instaGram_ID + "&access_token=" + _Instagram_access_token);
                                httpWebRequest.ContentType = "application/json";
                                httpWebRequest.Method = "GET";

                                httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                                {
                                    _result = streamReader.ReadToEnd();
                                }

                                // deserializing 
                                _Instagram_Search = JsonConvert.DeserializeObject<Instagram.Instagram_Search>(_result);

                                if (_Instagram_Search != null)
                                {
                                    if (_Instagram_Search.data != null && _Instagram_Search.data.Count > 0)
                                    {
                                        vArtists.Instagram_ID = _Instagram_Search.data[0].id;
                                    }
                                }
                                #endregion
                            }
                        }

                        // deserializing 
                        return _Instagram_Search;

                    }
                    catch (Exception ex)
                    {
                        LogHelper.CreateLog3(ex, Request);
                        LogHelper.CreateLog("Instagram_GetPictures_Asyn (Task 1) " + ex.Message);
                        return null;
                    }
                }
                );

                #endregion

                _Instagram_Search_Task1.Wait();
                return true;
                #region "Task 2 to get Instagram Detail using Artist Name (use if Matrics ID does'nt contain the Instagram Information"

                Task<Instagram.Instagram_Search> _Instagram_Search_Task2 = Task.Factory.StartNew(() =>
                {
                    try
                    {
                        string _result;
                        // If Instagram ID not found in MusicGraph Matrics then try direct Attempt
                        if (vArtists.Instagram_ID == null)
                        {
                            #region "Get Instagram ID (dont need this block while just updating the records)"

                            httpWebRequest = (HttpWebRequest)WebRequest.Create("https://api.instagram.com/v1/users/search?q=" + vArtists.ArtistName.Trim() + "&access_token=" + _Instagram_access_token);
                            httpWebRequest.ContentType = "application/json";
                            httpWebRequest.Method = "GET";

                            httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                            {
                                _result = streamReader.ReadToEnd();
                            }

                            // deserializing 
                            _Instagram_Search2 = JsonConvert.DeserializeObject<Instagram.Instagram_Search>(_result);

                            if (_Instagram_Search2 != null)
                            {
                                if (_Instagram_Search2.data != null && _Instagram_Search2.data.Count > 0)
                                {
                                    vArtists.Instagram_ID = _Instagram_Search2.data[0].id;
                                }
                            }

                            #endregion
                        }

                        return _Instagram_Search2;

                    }
                    catch (Exception ex)
                    {
                        LogHelper.CreateLog3(ex, Request);
                        LogHelper.CreateLog("Instagram_GetPictures_Asyn (Task 2) " + ex.Message);
                        return null;
                    }
                }
                );

                #endregion

                _Instagram_Search_Task1.Wait(); // wait for the task1 to complete first 
                _Instagram_Search_Task2.Wait();

                if (_Instagram_Search_Task1.Result == null || _Instagram_Search_Task1.Result.data.Count == 0)
                {
                    _Instagram_Search_Task2.Wait();
                    if (_Instagram_Search_Task2.Result != null)
                    {
                        _Instagram_Search3 = _Instagram_Search_Task2.Result;
                    }
                }
                else
                {
                    _Instagram_Search3 = _Instagram_Search_Task1.Result;
                }


                await Task.Factory.StartNew(() =>
                {
                    try
                    {
                        if (WebConfigurationManager.AppSettings["Instagram.ArtistPicture"].ToString() == "True")
                        {
                            #region "Get Instagram Profile Picture"

                            string strpThumbnailURLfordb = null;
                            string strpIamgeURLfordb = null;
                            string strpTempImageSave = null;
                            string _result;

                            if (vArtists.Instagram_ID != null && vArtists.Instagram_ID != "")
                            {

                                httpWebRequest = (HttpWebRequest)WebRequest.Create("https://api.instagram.com/v1/users/" + vArtists.Instagram_ID + "/?access_token=" + _Instagram_access_token);
                                httpWebRequest.ContentType = "application/json";
                                httpWebRequest.Method = "GET";

                                httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                                {
                                    _result = streamReader.ReadToEnd();
                                }

                                // deserializing 
                                var _Profile_Picture = JsonConvert.DeserializeObject<Instagram2.Profile_Picture>(_result);

                                //download and save profile picture of the artists
                                if (_Profile_Picture != null)
                                {
                                    if (_Profile_Picture.data.profile_picture != "")
                                    {
                                        Task<bool> _ArtistPic = ArtistProfilePicture_Asyn(vUserID, vArtists, vUnitOfWork, true, _Profile_Picture.data.profile_picture.Replace("s150x150/", ""));
                                    }
                                }


                            }
                            #endregion
                        }

                    }
                    catch (Exception ex)
                    {
                        LogHelper.CreateLog3(ex, Request);
                        LogHelper.CreateLog("Instagram_GetPictures_Asyn (Profile Picture ) " + ex.Message);
                    }
                });


                await Task.Factory.StartNew(() =>
                {
                    try
                    {
                        string _result;

                        #region "Get Instagram Hash Tag (Recent) (Not coded yet)"
                        if (WebConfigurationManager.AppSettings["Instagram.HashTag"].ToString() == "True" && vTour != null)
                        {
                            httpWebRequest = (HttpWebRequest)WebRequest.Create("https://api.instagram.com/v1/tags/" + vArtists.Instagram_Tag.ToString() + "/media/recent?access_token=" + _Instagram_access_token);
                            httpWebRequest.ContentType = "application/json";
                            httpWebRequest.Method = "GET";

                            httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                            {
                                _result = streamReader.ReadToEnd();
                            }

                            // deserializing 
                            var _User_TagPictures = JsonConvert.DeserializeObject<Instagram4.Tags_Images>(_result);
                        }
                        #endregion

                        #region "Get Instagram Recent Pictures (Using TourDate Hash Tag)"
                        //Change to tour date hash tage
                        if (vTour != null && !String.IsNullOrEmpty(vTour.HashTag))
                        {
                            httpWebRequest = (HttpWebRequest)WebRequest.Create("https://api.instagram.com/v1/tags/" + vTour.HashTag + "/media/recent?access_token=" + _Instagram_access_token);
                            httpWebRequest.ContentType = "application/json";
                            httpWebRequest.Method = "GET";

                            httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                            {
                                _result = streamReader.ReadToEnd();
                            }

                            // deserializing 
                            var _User_TagPictures = JsonConvert.DeserializeObject<Instagram4.Tags_Images>(_result);

                            if (_User_TagPictures != null)
                            {
                                if (_User_TagPictures.data.Count > 0)
                                {
                                    // Helper.DeleteDirectory(_SiteRoot +  @"\" + "Artists" + @"\" + vArtists.ArtistID + @"\" + "Recent" + @"\");
                                    //var _dellst = _ArtistPhotosRepo.Repository.GetAll(p => p.ArtistID == vArtists.ArtistID).Select(p => p.PhotoID).ToList();
                                    //_ArtistPhotosRepo.Repository.DeletePermanent(_dellst);

                                    List<Instagram4.Datum> _Data = _User_TagPictures.data.Where(p => p.type == "image").ToList();

                                    int _chk = 0;
                                    int _count = 0;

                                    if (_Data.Count > 29)
                                    {
                                        _count = 29;
                                    }
                                    else
                                    {
                                        _count = _Data.Count;
                                    }

                                    foreach (Instagram4.Datum _d in _Data)
                                    {
                                        try
                                        {

                                            Instagram4.Images _image = _d.images;

                                            if (_image.standard_resolution.url != "")
                                            {
                                                Models.TourPhoto _tourPhotos = new TourPhoto();
                                                /*string urll = _image.standard_resolution.url;
                                                strTempImageSave = ResizeImage.Download_Image(_image.standard_resolution.url);

                                                //---New Image path
                                                string newFilePath = _SiteRoot + @"\" + "Tours" + @"\" + vTour.TourDateID + @"\" + "Recent" + @"\";
                                                Helper.CreateDirectories(_SiteRoot + @"\" + "Tours" + @"\" + vTour.TourDateID + @"\" + "Recent" + @"\");

                                                string imageresizename = ResizeImage.Resize_Image_Thumb(tempfilePath + strTempImageSave, newFilePath, "_R_" + strTempImageSave, _Imagethumbsize, _Imagethumbsize);
                                                strIamgeURLfordb = _SiteURL + "/Tours" + "/" + vTour.TourDateID + "/Recent/" + imageresizename;
                                                */

                                                strIamgeURLfordb = _image.standard_resolution.url;
                                                strIamgeURLfordb = strIamgeURLfordb.Remove(strIamgeURLfordb.LastIndexOf('?'));
                                                _tourPhotos.ImageURL = strIamgeURLfordb;// urll.Remove(urll.IndexOf('?'), urll.Length);
                                                _tourPhotos.ImageThumbnailURL = _image.thumbnail.url.Remove(_image.thumbnail.url.LastIndexOf('?'));
                                                _tourPhotos.CreatedDate = DateTime.Now;
                                                //_tourPhotos.ArtistID = vArtists.ArtistID;
                                                _tourPhotos.RecordStatus = RecordStatus.Instagram.ToString();
                                                _tourPhotos.TourDateId = vTour.TourDateID;
                                                _TourPhotosRepo.Repository.Add(_tourPhotos);
                                            }

                                            _chk += 1;
                                            if (_chk > _count)
                                            {
                                                break;
                                            }

                                        }
                                        catch (Exception ex)
                                        {
                                            LogHelper.CreateLog("Instagram_GetPictures_Asyn (Recent Picture (Hash Tag) - Lopp) " + ex.Message);
                                        }
                                    }
                                }
                            }

                            vTour.ModifiedDate = DateTime.Now;
                        }
                        #endregion
                        /*
                        #region "Get Instagram Recent Pictures if Hash Tag not provided"
                        if (vArtists.Instagram_ID != null && vArtists.Instagram_ID != "" && string.IsNullOrEmpty(vArtists.Instagram_Tag))
                        {

                            httpWebRequest = (HttpWebRequest)WebRequest.Create("https://api.instagram.com/v1/users/" + vArtists.Instagram_ID + "/media/recent/?access_token=" + _Instagram_access_token);
                            httpWebRequest.ContentType = "application/json";
                            httpWebRequest.Method = "GET";

                            httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                            {
                                _result = streamReader.ReadToEnd();
                            }

                            // deserializing 
                            var _User_RecentPictures = JsonConvert.DeserializeObject<Instagram5.User_RecentPictures>(_result);

                            if (_User_RecentPictures != null)
                            {
                                if (_User_RecentPictures.data.Count > 0)
                                {
                                    int _count = 0;
                                    if (_User_RecentPictures.data.Count > 11)
                                    {
                                        _count = 11;
                                    }
                                    else
                                    {
                                        _count = _User_RecentPictures.data.Count;
                                    }


                                    int _chk = 0;
                                    foreach (Instagram5.Datum _data in _User_RecentPictures.data)
                                    {
                                        try
                                        {
                                            Instagram5.Images _image = _data.images;

                                            if (_image.standard_resolution.url != "")
                                            {
                                                Models.ArtistPhotos _ArtistPhotos = new ArtistPhotos();

                                                strTempImageSave = ResizeImage.Download_Image(_image.standard_resolution.url);

                                                //---New Image path
                                                string newFilePath = _SiteRoot + @"\" + "Artists" + @"\" + vArtists.ArtistID + @"\" + "Recent" + @"\";
                                                Helper.CreateDirectories(_SiteRoot + @"\" + "Artists" + @"\" + vArtists.ArtistID + @"\" + "Recent" + @"\");

                                                string imageresizename = ResizeImage.Resize_Image_Thumb(tempfilePath + strTempImageSave, newFilePath, "_R_" + strTempImageSave, _Imagethumbsize, _Imagethumbsize);
                                                strIamgeURLfordb = _SiteURL + "/Artists" + "/" + vArtists.ArtistID + "/Recent/" + imageresizename;

                                                _ArtistPhotos.ImageUrl = strIamgeURLfordb;
                                                _ArtistPhotos.CreatedDate = DateTime.Now;
                                                _ArtistPhotos.ArtistID = vArtists.ArtistID;
                                                _ArtistPhotos.RecordStatus = RecordStatus.Instagram.ToString();

                                                _ArtistPhotosRepo.Repository.Add(_ArtistPhotos);
                                            }

                                            _chk += 1;
                                            if (_chk > _count)
                                            {
                                                break;
                                            }

                                        }
                                        catch (Exception ex)
                                        {
                                             LogHelper.CreateLog("Instagram_GetPictures_Asyn (Recent Picture Loop) " + ex.Message);
                                        }
                                    }
                                }
                            }


                        }
                        #endregion
                        */
                    }
                    catch (Exception ex)
                    {
                        LogHelper.CreateLog("Instagram_GetPictures_Asyn (Recent Picture - Update) " + ex.Message);
                    }
                });



            }//End new artist condition here
            #endregion

            #region "Update Rec"

            else
            {
                //return true;
                await Task.Factory.StartNew(() =>
                {
                    try
                    {
                        if (WebConfigurationManager.AppSettings["Instagram.ArtistPicture"].ToString() == "True")
                        {
                            #region "Get Instagram Profile Picture"

                            string strpThumbnailURLfordb = null;
                            string strpIamgeURLfordb = null;
                            string strpTempImageSave = null;
                            string _result;

                            if (vArtists.Instagram_ID != null && vArtists.Instagram_ID != "")
                            {

                                httpWebRequest = (HttpWebRequest)WebRequest.Create("https://api.instagram.com/v1/users/" + vArtists.Instagram_ID + "/?access_token=" + _Instagram_access_token);
                                httpWebRequest.ContentType = "application/json";
                                httpWebRequest.Method = "GET";

                                httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                                {
                                    _result = streamReader.ReadToEnd();
                                }

                                // deserializing 
                                var _Profile_Picture = JsonConvert.DeserializeObject<Instagram2.Profile_Picture>(_result);

                                //download and save profile picture of the artists
                                if (_Profile_Picture != null)
                                {
                                    if (_Profile_Picture.data.profile_picture != "")
                                    {
                                        Task<bool> _ArtistPic = ArtistProfilePicture_Asyn(vUserID, vArtists, vUnitOfWork, true, _Profile_Picture.data.profile_picture.Replace("s150x150/", ""));
                                    }
                                }

                            }
                            #endregion
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.CreateLog("Instagram_GetPictures_Asyn (Profile Picture - Update) " + ex.Message);
                    }
                });


                await Task.Factory.StartNew(() =>
                {
                    try
                    {

                        string _result;

                        #region "Recheck if the Instagram ID has provided (If failed to get at first place)"

                        //if ((vArtists.Instagram_ID != null || vArtists.Instagram_ID != "") && string.IsNullOrEmpty(vArtists.Instagram_Tag))
                        //    {
                        //        #region "Task 1 to get Instagram Detail using Matrics ID"

                        //        Task<Instagram.Instagram_Search> _Instagram_Search_Task1 = Task.Factory.StartNew(() =>
                        //        {
                        //            try
                        //            {
                        //                if (vArtistMatrics_ByID != null)
                        //                {
                        //                    if (vArtistMatrics_ByID.data.instagram != null && vArtistMatrics_ByID.data.instagram.url != null && vArtistMatrics_ByID.data.instagram.url != "")
                        //                    {
                        //                        vArtists.Instagram_Url = vArtistMatrics_ByID.data.instagram.url;
                        //                        string _instaGram_ID = vArtistMatrics_ByID.data.instagram.url.Replace("http:", "https:").Replace("https://instagram.com/", "").Replace("/", "");
                        //                        string _Instagram_Tag = null;

                        //                        #region "Get Instagram ID (dont need this block while just updating the records)"

                        //                        httpWebRequest = (HttpWebRequest)WebRequest.Create("https://api.instagram.com/v1/users/search?q=" + _instaGram_ID + "&access_token=" + _Instagram_access_token);
                        //                        httpWebRequest.ContentType = "application/json";
                        //                        httpWebRequest.Method = "GET";

                        //                        httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                        //                        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                        //                        {
                        //                            _result = streamReader.ReadToEnd();
                        //                        }

                        //                        // deserializing 
                        //                        _Instagram_Search = JsonConvert.DeserializeObject<Instagram.Instagram_Search>(_result);

                        //                        if (_Instagram_Search != null)
                        //                        {
                        //                            if (_Instagram_Search.data != null && _Instagram_Search.data.Count > 0)
                        //                            {
                        //                                vArtists.Instagram_ID = _Instagram_Search.data[0].id;
                        //                            }
                        //                        }
                        //                        #endregion
                        //                    }
                        //                }

                        //                // deserializing 
                        //                return _Instagram_Search;

                        //            }
                        //            catch (Exception ex)
                        //            {
                        //                 LogHelper.CreateLog("Instagram_GetPictures_Asyn (Task 1) " + ex.Message);
                        //                return null;
                        //            }
                        //        }
                        //        );

                        //        #endregion

                        //         _Instagram_Search_Task1.Wait();

                        //        #region "Task 2 to get Instagram Detail using Artist Name (use if Matrics ID does'nt contain the Instagram Information"

                        //        Task<Instagram.Instagram_Search> _Instagram_Search_Task2 = Task.Factory.StartNew(() =>
                        //        {
                        //            try
                        //            {
                        //                // If Instagram ID not found in MusicGraph Matrics then try direct Attempt
                        //                if (vArtists.Instagram_ID == null)
                        //                {
                        //                    #region "Get Instagram ID (dont need this block while just updating the records)"

                        //                    httpWebRequest = (HttpWebRequest)WebRequest.Create("https://api.instagram.com/v1/users/search?q=" + vArtists.ArtistName.Trim() + "&access_token=" + _Instagram_access_token);
                        //                    httpWebRequest.ContentType = "application/json";
                        //                    httpWebRequest.Method = "GET";

                        //                    httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                        //                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                        //                    {
                        //                        _result = streamReader.ReadToEnd();
                        //                    }

                        //                    // deserializing 
                        //                    _Instagram_Search2 = JsonConvert.DeserializeObject<Instagram.Instagram_Search>(_result);

                        //                    if (_Instagram_Search2 != null)
                        //                    {
                        //                        if (_Instagram_Search2.data != null && _Instagram_Search2.data.Count > 0)
                        //                        {
                        //                            vArtists.Instagram_ID = _Instagram_Search2.data[0].id;
                        //                        }
                        //                    }

                        //                    #endregion
                        //                }

                        //                return _Instagram_Search2;

                        //            }
                        //            catch (Exception ex)
                        //            {
                        //                 LogHelper.CreateLog("Instagram_GetPictures_Asyn (Task 2) " + ex.Message);
                        //                return null;
                        //            }
                        //        }
                        //        );

                        //        #endregion

                        //        _Instagram_Search_Task1.Wait(); // wait for the task1 to complete first 

                        //        if (_Instagram_Search_Task1.Result == null || _Instagram_Search_Task1.Result.data.Count == 0)
                        //        {
                        //            _Instagram_Search_Task2.Wait();
                        //            if (_Instagram_Search_Task2.Result != null)
                        //            {
                        //                _Instagram_Search3 = _Instagram_Search_Task2.Result;
                        //            }
                        //        }
                        //        else
                        //        {
                        //            _Instagram_Search3 = _Instagram_Search_Task1.Result;
                        //        }

                        //    }
                        #endregion
                        #region "Get Instagram Recent Pictures (Using TourDate Hash Tag)"
                        //Change to tour date hash tage
                        if (vTour != null && !String.IsNullOrEmpty(vTour.HashTag))
                        {
                            httpWebRequest = (HttpWebRequest)WebRequest.Create("https://api.instagram.com/v1/tags/" + vTour.HashTag + "/media/recent?access_token=" + _Instagram_access_token);
                            httpWebRequest.ContentType = "application/json";
                            httpWebRequest.Method = "GET";

                            httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                            {
                                _result = streamReader.ReadToEnd();
                            }

                            // deserializing 
                            var _User_TagPictures = JsonConvert.DeserializeObject<Instagram4.Tags_Images>(_result);

                            if (_User_TagPictures != null)
                            {
                                if (_User_TagPictures.data.Count > 0)
                                {
                                    // Helper.DeleteDirectory(_SiteRoot +  @"\" + "Artists" + @"\" + vArtists.ArtistID + @"\" + "Recent" + @"\");
                                    //var _dellst = _ArtistPhotosRepo.Repository.GetAll(p => p.ArtistID == vArtists.ArtistID).Select(p => p.PhotoID).ToList();
                                    //_ArtistPhotosRepo.Repository.DeletePermanent(_dellst);

                                    List<Instagram4.Datum> _Data = _User_TagPictures.data.Where(p => p.type == "image").ToList();

                                    int _chk = 0;
                                    int _count = 0;

                                    if (_Data.Count > 29)
                                    {
                                        _count = 29;
                                    }
                                    else
                                    {
                                        _count = _Data.Count;
                                    }

                                    foreach (Instagram4.Datum _d in _Data)
                                    {
                                        try
                                        {

                                            Instagram4.Images _image = _d.images;

                                            if (_image.standard_resolution.url != "")
                                            {
                                                Models.TourPhoto _tourPhotos = new TourPhoto();
                                                /*string urll = _image.standard_resolution.url;
                                                strTempImageSave = ResizeImage.Download_Image(_image.standard_resolution.url);

                                                //---New Image path
                                                string newFilePath = _SiteRoot + @"\" + "Tours" + @"\" + vTour.TourDateID + @"\" + "Recent" + @"\";
                                                Helper.CreateDirectories(_SiteRoot + @"\" + "Tours" + @"\" + vTour.TourDateID + @"\" + "Recent" + @"\");

                                                string imageresizename = ResizeImage.Resize_Image_Thumb(tempfilePath + strTempImageSave, newFilePath, "_R_" + strTempImageSave, _Imagethumbsize, _Imagethumbsize);
                                                strIamgeURLfordb = _SiteURL + "/Tours" + "/" + vTour.TourDateID + "/Recent/" + imageresizename;
                                                */
                                                strIamgeURLfordb = _image.standard_resolution.url;
                                                strIamgeURLfordb = strIamgeURLfordb.Remove(strIamgeURLfordb.LastIndexOf('?'));
                                                _tourPhotos.ImageURL = strIamgeURLfordb;// urll.Remove(urll.IndexOf('?'), urll.Length);
                                                _tourPhotos.ImageThumbnailURL = _image.thumbnail.url.Remove(_image.thumbnail.url.LastIndexOf('?'));
                                                _tourPhotos.CreatedDate = DateTime.Now;
                                                //_tourPhotos.ArtistID = vArtists.ArtistID;
                                                _tourPhotos.RecordStatus = RecordStatus.Instagram.ToString();
                                                _tourPhotos.TourDateId = vTour.TourDateID;
                                                _TourPhotosRepo.Repository.Add(_tourPhotos);

                                            }

                                            _chk += 1;
                                            if (_chk > _count)
                                            {
                                                break;
                                            }

                                        }
                                        catch (Exception ex)
                                        {
                                            LogHelper.CreateLog("Instagram_GetPictures_Asyn (Recent Picture (Hash Tag) - Lopp) " + ex.Message);
                                        }
                                    }
                                }
                            }
                            vTour.ModifiedDate = DateTime.Now;
                        }
                        #endregion
                        /*
                        #region "Get Instagram Recent Pictures (Using Hash Tag)"
                        if (vArtists != null && !string.IsNullOrEmpty(vArtists.Instagram_Tag))
                        {
                            httpWebRequest = (HttpWebRequest)WebRequest.Create("https://api.instagram.com/v1/tags/" + vArtists.Instagram_Tag.ToString() + "/media/recent?access_token=" + _Instagram_access_token);
                            httpWebRequest.ContentType = "application/json";
                            httpWebRequest.Method = "GET";

                            httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                            {
                                _result = streamReader.ReadToEnd();
                            }

                            // deserializing 
                            var _User_TagPictures = JsonConvert.DeserializeObject<Instagram4.Tags_Images>(_result);

                            if (_User_TagPictures != null)
                            {
                                if (_User_TagPictures.data.Count > 0)
                                {
                                    Helper.DeleteDirectory(_SiteRoot + @"\" + "Artists" + @"\" + vArtists.ArtistID + @"\" + "Recent" + @"\", true);
                                    var _dellst = _ArtistPhotosRepo.Repository.GetAll(p => p.ArtistID == vArtists.ArtistID).Select(p => p.PhotoID).ToList();
                                    _ArtistPhotosRepo.Repository.Delete(_dellst);

                                    List<Instagram4.Datum> _Data = _User_TagPictures.data.Where(p => p.type == "image").ToList();

                                    int _chk = 0;
                                    int _count = 0;

                                    if (_Data.Count > 11)
                                    {
                                        _count = 11;
                                    }
                                    else
                                    {
                                        _count = _Data.Count;
                                    }

                                    foreach (Instagram4.Datum _d in _Data)
                                    {
                                        try
                                        {

                                            Instagram4.Images _image = _d.images;

                                            if (_image.standard_resolution.url != "")
                                            {
                                                Models.ArtistPhotos _ArtistPhotos = new ArtistPhotos();

                                                strTempImageSave = ResizeImage.Download_Image(_image.standard_resolution.url);

                                                //---New Image path
                                                string newFilePath = _SiteRoot + @"\" + "Artists" + @"\" + vArtists.ArtistID + @"\" + "Recent" + @"\";
                                                Helper.CreateDirectories(_SiteRoot + @"\" + "Artists" + @"\" + vArtists.ArtistID + @"\" + "Recent" + @"\");

                                                string imageresizename = ResizeImage.Resize_Image_Thumb(tempfilePath + strTempImageSave, newFilePath, "_R_" + strTempImageSave, _Imagethumbsize, _Imagethumbsize);

                                                strIamgeURLfordb = _SiteURL + "/Artists" + "/" + vArtists.ArtistID + "/Recent/" + imageresizename;

                                                _ArtistPhotos.ImageUrl = strIamgeURLfordb;
                                                _ArtistPhotos.CreatedDate = DateTime.Now;
                                                _ArtistPhotos.ArtistID = vArtists.ArtistID;
                                                _ArtistPhotos.RecordStatus = RecordStatus.Instagram.ToString();

                                                _ArtistPhotosRepo.Repository.Add(_ArtistPhotos);
                                                vTour.ModifiedDate = DateTime.Now;
                                            }

                                            _chk += 1;
                                            if (_chk > _count)
                                            {
                                                break;
                                            }

                                        }
                                        catch (Exception ex)
                                        {
                                             LogHelper.CreateLog("Instagram_GetPictures_Asyn (Recent Picture (Hash Tag) - Lopp) " + ex.Message);
                                        }
                                    }
                                }
                            }
                        }
                        #endregion*/
                        /*
                        #region "Get Instagram Recent Pictures if Hash Tag not provided"

                        if (vArtists.Instagram_ID != null && vArtists.Instagram_ID != "" && string.IsNullOrEmpty(vArtists.Instagram_Tag))
                        {

                            httpWebRequest = (HttpWebRequest)WebRequest.Create("https://api.instagram.com/v1/users/" + vArtists.Instagram_ID + "/media/recent/?access_token=" + _Instagram_access_token);
                            httpWebRequest.ContentType = "application/json";
                            httpWebRequest.Method = "GET";

                            httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                            {
                                _result = streamReader.ReadToEnd();
                            }

                            // deserializing 
                            var _User_RecentPictures = JsonConvert.DeserializeObject<Instagram5.User_RecentPictures>(_result);

                            if (_User_RecentPictures != null)
                            {
                                if (_User_RecentPictures.data.Count > 0)
                                {
                                    Helper.DeleteDirectory(_SiteRoot + @"\" + vUserID.ToString() + @"\" + "Artists" + @"\" + vArtists.ArtistID + @"\" + "Recent" + @"\", true);

                                    var _dellst = _ArtistPhotosRepo.Repository.GetAll(p => p.ArtistID == vArtists.ArtistID).Select(p => p.PhotoID).ToList();
                                    _ArtistPhotosRepo.Repository.Delete(_dellst);


                                    int _count = 0;
                                    if (_User_RecentPictures.data.Count > 11)
                                    {
                                        _count = 11;
                                    }
                                    else
                                    {
                                        _count = _User_RecentPictures.data.Count;
                                    }



                                    int _chk = 0;
                                    foreach (Instagram5.Datum _data in _User_RecentPictures.data)
                                    {
                                        try
                                        {
                                            Instagram5.Images _image = _data.images;

                                            if (_image.standard_resolution.url != "")
                                            {
                                                Models.ArtistPhotos _ArtistPhotos = new ArtistPhotos();

                                                strTempImageSave = ResizeImage.Download_Image(_image.standard_resolution.url);

                                                //---New Image path
                                                string newFilePath = _SiteRoot + @"\" + vUserID.ToString() + @"\" + "Artists" + @"\" + vArtists.ArtistID + @"\" + "Recent" + @"\";
                                                Helper.CreateDirectories(_SiteRoot + @"\" + vUserID.ToString() + @"\" + "Artists" + @"\" + vArtists.ArtistID + @"\" + "Recent" + @"\");

                                                string imageresizename = ResizeImage.Resize_Image_Thumb(tempfilePath + strTempImageSave, newFilePath, "_R_" + strTempImageSave, _Imagethumbsize, _Imagethumbsize);
                                                strIamgeURLfordb = _SiteURL + "/" + vUserID.ToString() + "/Artists" + "/" + vArtists.ArtistID + "/Recent/" + imageresizename;

                                                _ArtistPhotos.ImageUrl = strIamgeURLfordb;
                                                _ArtistPhotos.CreatedDate = DateTime.Now;
                                                _ArtistPhotos.ArtistID = vArtists.ArtistID;
                                                _ArtistPhotos.RecordStatus = RecordStatus.Instagram.ToString();

                                                _ArtistPhotosRepo.Repository.Add(_ArtistPhotos);
                                            }

                                            _chk += 1;
                                            if (_chk > _count)
                                            {
                                                break;
                                            }

                                        }
                                        catch (Exception ex)
                                        {
                                             LogHelper.CreateLog("Instagram_GetPictures_Asyn (Recent Picture - Lopp) " + ex.Message);
                                        }
                                    }
                                }
                            }


                        }
                        #endregion
                        */
                    }
                    catch (Exception ex)
                    {
                        LogHelper.CreateLog("Instagram_GetPictures_Asyn (Recent Picture - Upadte) " + ex.Message);
                    }
                });
            }
            #endregion

            // _ArtistsRepo.Repository.Update(vArtists);

            return true;
        }

        #endregion

        #region "SeatGeek"
        private string SGAuth()
        {
            return "?client_id=" + WebConfigurationManager.AppSettings["SeatGeek_client_id"] + "&client_secret=" + WebConfigurationManager.AppSettings["SeatGeek_client_secret"];
        }

        private Task<SeatGeek4.Get_Events_ByID> SeatGeek_GetEventByID_Asyn(string vSeatGeekID)
        {

            HttpWebRequest httpWebRequest;
            HttpWebResponse httpResponse;
            string _result;
            string _SeatGeek_client_id = ConfigurationManager.AppSettings["SeatGeek_client_id"].ToString();


            LogHelper.CreateLog("SeatGeek_GetEventByID_Asyn (" + vSeatGeekID + ")");

            return Task.Factory.StartNew(() =>
            {
                try
                {
                    string seatid = vSeatGeekID;
                    if (String.IsNullOrEmpty(vSeatGeekID))
                        seatid = "0";

                    httpWebRequest = (HttpWebRequest)WebRequest.Create("https://api.seatgeek.com/2/events/" + seatid + SGAuth());
                    httpWebRequest.ContentType = "application/json";
                    httpWebRequest.Method = "GET";
                    httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        _result = streamReader.ReadToEnd();
                    }

                    // deserializing 
                    return JsonConvert.DeserializeObject<SeatGeek4.Get_Events_ByID>(_result);

                }
                catch (Exception ex)
                {
                    LogHelper.CreateLog3(ex, Request);
                    LogHelper.CreateLog("SeatGeek_GetEventByID_Asyn " + ex.Message);
                    return null;
                }
            }


                );


        }


        private async Task<bool> SeatGeek_CheckLatinGenre_Asyn(string artistName, IUnitOfWork vUnitOfWork)
        {

            var serializer = new JavaScriptSerializer();
            serializer.RegisterConverters(new[] { new DynamicJsonConverter() });

            GenericRepository<Venue> _VenueRepo = new GenericRepository<Venue>(vUnitOfWork);
            GenericRepository<TourDate> _TourDateRepo = new GenericRepository<TourDate>(vUnitOfWork);
            GenericRepository<Artists> _ArtistsRepo = new GenericRepository<Artists>(vUnitOfWork);

            GenericRepository<UserTourDate> _UserTourDateRepo = new GenericRepository<UserTourDate>(vUnitOfWork);
            GenericRepository<ArtistGenre> _ArtistGenreRepo = new GenericRepository<ArtistGenre>(vUnitOfWork);
            GenericRepository<Genre> _GenreRepo = new GenericRepository<Genre>(vUnitOfWork);

            string _SeatGeek_client_id = ConfigurationManager.AppSettings["SeatGeek_client_id"].ToString();

            string tempfilePath = WebConfigurationManager.AppSettings["SiteImgPath"].ToString() + @"\" + "TempImages" + @"\";

            string _SiteRoot = WebConfigurationManager.AppSettings["SiteImgPath"];
            string _SiteURL = WebConfigurationManager.AppSettings["SiteImgURL"];

            HttpWebRequest httpWebRequest;
            HttpWebResponse httpResponse;
            string _result;
            SeatGeek.Get_Performers _Get_Performers = null;


            bool isLatin = false;

            SeatGeek.Get_Performers _Get_PerformersRet = await Task.Factory.StartNew(() =>
            {
                try
                {
                    #region "Get SeatGeek Performers (Dont use this while updating the records)"

                    string pattern = @"[^a-zA-Z0-9áéíñóúüÁÉÍÑÓÚÜ\s]";
                    string cleanName = Regex.Replace(artistName, pattern, "");

                    httpWebRequest = (HttpWebRequest)WebRequest.Create("http://api.seatgeek.com/2/performers" + SGAuth() + "&taxonomies.id=2000000&taxonomies.id=2010000&taxonomies.id=3010100&taxonomies.id=3010200&taxonomies.id=3010300&q=" + cleanName.Trim());
                    httpWebRequest.ContentType = "application/json";
                    httpWebRequest.Method = "GET";

                    httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        _result = streamReader.ReadToEnd();
                    }

                    // deserializing 
                    _Get_Performers = JsonConvert.DeserializeObject<SeatGeek.Get_Performers>(_result);
                    SeatGeek.Performer _Performer = null;

                    if (_Get_Performers != null)
                    {
                        if (_Get_Performers.performers.Count > 0)
                        {
                            _Performer = (from A in _Get_Performers.performers
                                          where A.name == artistName.Trim()
                                          select A).OrderByDescending(p => p.score).FirstOrDefault();

                            if (_Performer == null)
                            {
                                _Performer = (from A in _Get_Performers.performers select A).OrderByDescending(p => p.score).FirstOrDefault();

                            }

                            #region "Loop through the Genre"
                            if (_Performer.genres != null && _Performer.genres.Count > 0)
                            {
                                foreach (SeatGeek.Genre _Ev in _Performer.genres)
                                {
                                    if (_GenreFilter.Contains(_Ev.name.Trim()))
                                        isLatin = true;

                                }
                            }
                            #endregion

                        }

                    }



                    // deserializing 
                    return _Get_Performers;

                }
                catch (Exception ex)
                {
                    LogHelper.CreateLog("SeatGeek_CheckLatinGenre_Asyn " + ex.Message);
                    return null;
                }
            }
            #endregion

);
            return isLatin;
        }


        private async Task<bool> SeatGeek_GetEventByArtistName_Asyn(Int32 vUserID, Artists vArtists, IUnitOfWork vUnitOfWork, bool vNew, bool vScan = false)
        {
            string strThumbnailURLfordb = null;
            string strIamgeURLfordb = null;
            string strTempImageSave = null;


            var serializer = new JavaScriptSerializer();
            serializer.RegisterConverters(new[] { new DynamicJsonConverter() });
            dynamic obj = null;


            GenericRepository<Venue> _VenueRepo = new GenericRepository<Venue>(vUnitOfWork);
            GenericRepository<TourDate> _TourDateRepo = new GenericRepository<TourDate>(vUnitOfWork);
            GenericRepository<Artists> _ArtistsRepo = new GenericRepository<Artists>(vUnitOfWork);

            GenericRepository<UserTourDate> _UserTourDateRepo = new GenericRepository<UserTourDate>(vUnitOfWork);
            GenericRepository<ArtistGenre> _ArtistGenreRepo = new GenericRepository<ArtistGenre>(vUnitOfWork);
            GenericRepository<Genre> _GenreRepo = new GenericRepository<Genre>(vUnitOfWork);

            string _SeatGeek_client_id = ConfigurationManager.AppSettings["SeatGeek_client_id"].ToString();

            string tempfilePath = WebConfigurationManager.AppSettings["SiteImgPath"].ToString() + @"\" + "TempImages" + @"\";

            string _SiteRoot = WebConfigurationManager.AppSettings["SiteImgPath"];
            string _SiteURL = WebConfigurationManager.AppSettings["SiteImgURL"];

            HttpWebRequest httpWebRequest;
            HttpWebResponse httpResponse;
            string _result;
            SeatGeek.Get_Performers _Get_Performers = null;


            string _Performer_ID = null;
            string _strEvent = null;

            Models.Venue _VenuEntity = null;
            Models.TourDate _TourDateEntity = null;
            Models.UserTourDate _UserTourDate = null;
            Models.ArtistGenre _ArtistGenre = null;
            Models.Genre _Genre = null;

            #region "New Rec"
            if (vNew == true) // for New artist
            {
                #region "Task 1 to get SeatGeek Detail using Artist Name"

                SeatGeek.Get_Performers _Get_PerformersRet = await Task.Factory.StartNew(() =>
                {
                    try
                    {
                        #region "Get SeatGeek Performers (Dont use this while updating the records)"

                        string pattern = @"[^a-zA-Z0-9áéíñóúüÁÉÍÑÓÚÜ\s]";
                        string cleanName = Regex.Replace(vArtists.ArtistName, pattern, "");

                        httpWebRequest = (HttpWebRequest)WebRequest.Create("http://api.seatgeek.com/2/performers" + SGAuth() + "&taxonomies.id=2000000&taxonomies.id=2010000&taxonomies.id=3010100&taxonomies.id=3010200&taxonomies.id=3010300&q=" + cleanName.Trim());
                        httpWebRequest.ContentType = "application/json";
                        httpWebRequest.Method = "GET";

                        httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                        {
                            _result = streamReader.ReadToEnd();
                        }

                        // deserializing 
                        _Get_Performers = JsonConvert.DeserializeObject<SeatGeek.Get_Performers>(_result);
                        //dynamic _Get_Performers = serializer.Deserialize(_result, typeof(object));


                        if (_Get_Performers != null)
                        {
                            if (_Get_Performers.performers.Count > 0)
                            {
                                SeatGeek.Performer _Performer = new SeatGeek.Performer();




                                _Performer = (from A in _Get_Performers.performers
                                              where DiceCoefficientExtensions.DiceCoefficient(A.name.ToLower(), vArtists.ArtistName.Trim()) >= _FuzzySearchCri && CheckSeatGeekLatin(A.name, _unitOfWork)
                                              select A).OrderByDescending(p => p.score).FirstOrDefault();

                                if (_Performer != null)
                                {
                                    vArtists.OnTour = _Performer.has_upcoming_events == true ? true : false;
                                    _Performer_ID = _Performer.id.ToString();
                                    vArtists.Seatgeek_ID = _Performer_ID;

                                    if (_Performer.taxonomies != null && _Performer.taxonomies.Count > 0)
                                    {
                                        foreach (SeatGeek.Taxonomy _Taxonomy in _Performer.taxonomies)
                                        {
                                            _strEvent = _strEvent + _Taxonomy.name + " ,";
                                        }
                                    }

                                    if (_strEvent.ToString() == "") _strEvent = vArtists.ArtistName + " Event";


                                    #region "Loop through the Genre"
                                    if (_Performer.genres != null && _Performer.genres.Count > 0)
                                    {
                                        foreach (SeatGeek.Genre _Ev in _Performer.genres)
                                        {
                                            _Genre = _GenreRepo.Repository.Get(p => p.Name == _Ev.name.Trim());

                                            if (_Genre == null)
                                            {
                                                _Genre = new Genre();
                                                _Genre.Name = _Ev.name;
                                                _Genre.CreatedDate = DateTime.Now;
                                                _Genre.RecordStatus = RecordStatus.SeatGeek.ToString();
                                                _GenreRepo.Repository.Add(_Genre);

                                            }

                                            var check = !_ArtistGenreRepo.Repository.AsQueryable().Any(x => x.ArtistID == vArtists.ArtistID && x.GenreID == _Genre.GenreID);

                                            if (check)
                                            {
                                                _ArtistGenre = new ArtistGenre();
                                                _ArtistGenre.GenreID = _Genre.GenreID;
                                                _ArtistGenre.ArtistID = vArtists.ArtistID;
                                                _ArtistGenre.Slug = _Ev.slug;
                                                _ArtistGenre.Primary = _Ev.primary;
                                                _ArtistGenre.Name = _Ev.name;
                                                _ArtistGenre.CreatedDate = DateTime.Now;
                                                _ArtistGenre.RecordStatus = RecordStatus.SeatGeek.ToString();

                                                _ArtistGenreRepo.Repository.Add(_ArtistGenre);
                                            }
                                        }
                                    }
                                    #endregion


                                    if (String.IsNullOrEmpty(vArtists.ImageURL))
                                        GetProfileImageFromSpotifyFeed(vArtists);

                                    if (WebConfigurationManager.AppSettings["SeatGeek.ArtistPicture"].ToString() == "True")
                                    {
                                        #region "Get Artist Picture "
                                        try
                                        {
                                            if (_Performer.images != null)
                                            {
                                                if (_Performer.images.huge != null)
                                                {
                                                    Task<bool> _ArtistPic = ArtistProfilePicture_Asyn(vUserID, vArtists, vUnitOfWork, true, _Performer.images.huge);
                                                }
                                            }
                                        }
                                        catch (Exception ex)
                                        {

                                        }
                                        #endregion
                                    }
                                }
                            }
                            else
                            {
                                vArtists.OnTour = false;
                            }
                        }
                        else
                        {
                            vArtists.OnTour = false;
                        }




                        #endregion


                        // deserializing 
                        return _Get_Performers;

                    }
                    catch (Exception ex)
                    {
                        LogHelper.CreateLog("SeatGeek_GetEventByArtistName_Asyn (Task 1) " + ex.Message);
                        return null;
                    }
                }
                );

                #endregion

                #region "Task2 if above task completed successfully"

                await Task.Factory.StartNew(() =>
                {
                    try
                    {
                        #region "Get SeatGeek Performer Tours/Events"

                        //_Events.SeatGeek_ID

                        string seatid = vArtists.Seatgeek_ID;
                        if (String.IsNullOrEmpty(vArtists.Seatgeek_ID))
                            seatid = "0";

                        httpWebRequest = (HttpWebRequest)WebRequest.Create("http://api.seatgeek.com/2/events" + SGAuth() + "&taxonomies.id=2000000&taxonomies.id=2010000&taxonomies.id=3010100&taxonomies.id=3010200&taxonomies.id=3010300&performers.id=" + seatid + "&page=1&per_page=100");
                        httpWebRequest.ContentType = "application/json";
                        httpWebRequest.Method = "GET";

                        httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                        {
                            _result = streamReader.ReadToEnd();
                        }

                        // deserializing 
                        //var _Get_Performers_Events = JsonConvert.DeserializeObject<SeatGeek2.Get_Performers_Events>(_result);
                        dynamic _Get_Performers_Events = serializer.Deserialize(_result, typeof(object));



                        if (_Get_Performers_Events != null)
                        {
                            if (_Get_Performers_Events.events.Count > 0)
                            {
                                #region "loop through the events"


                                //foreach (SeatGeek2.Event _Ev in _Get_Performers_Events.events)
                                foreach (dynamic _Ev in _Get_Performers_Events.events)
                                {


                                    //SeatGeek2.Venue _Venue = _Ev.venue;
                                    dynamic _Venue = _Ev.venue;

                                    //Venu information
                                    //_VenuEntity = _VenueRepo.Repository.Get(p => p.SeatGeek_VenuID == _Venue.id.ToString());

                                    string _VenueID = _Venue.id.ToString();
                                    string _VenueName = _Venue.name.ToLower();

                                    _VenuEntity = (from A in _VenueRepo.Repository.GetAll()
                                                   where (A.SeatGeek_VenuID == _VenueID)
                                                   select A).FirstOrDefault();

                                    //search the venu using fuzzy searching
                                    if (_VenuEntity == null)
                                    {
                                        /*_VenuEntity = (from A in _VenueRepo.Repository.GetAll()
                                                       where (DiceCoefficientExtensions.DiceCoefficient(A.VenueName.ToLower(), _VenueName) >= _FuzzySearchCri)
                                                       && A.RecordStatus == RecordStatus.Eventful.ToString()
                                                       select A).FirstOrDefault();
                                                       */

                                        string pattern = @"[^a-zA-Z0-9áéíñóúüÁÉÍÑÓÚÜ&\s]";
                                        var search = Regex.Replace(_VenueName, pattern, "");

                                        var venueByName = _VenueRepo.Repository.AsQueryable().Where(x => x.VenueName.Length < 60 && x.RecordStatus == RecordStatus.Eventful.ToString() && x.VenueName.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                                        if (venueByName == null || venueByName.Count() == 0)
                                        {
                                            var allEventfulVenues = _VenueRepo.Repository.AsQueryable().Where(x => x.VenueName.Length < 80 && x.RecordStatus == RecordStatus.Eventful.ToString()).ToList();
                                            foreach (var a in allEventfulVenues)
                                            {
                                                var eventfulVenue = Regex.Replace(a.VenueName, pattern, "");
                                                if (eventfulVenue.ToLower() == search.ToLower())
                                                {
                                                    _VenuEntity = a;
                                                    break;
                                                }
                                                if (search.Length > 11 && eventfulVenue.ToLower().Contains(search.ToLower().Substring(2, search.Length - 2)))
                                                {
                                                    _VenuEntity = a;
                                                    break;
                                                }
                                                if (search.Length > 20 && eventfulVenue.ToLower().Contains(search.ToLower().Substring(5, search.Length - 5)))
                                                {
                                                    _VenuEntity = a;
                                                    break;
                                                }
                                                if (search.Length > 40 && eventfulVenue.ToLower().Contains(search.ToLower().Substring(10, search.Length - 10)))
                                                {
                                                    _VenuEntity = a;
                                                    break;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            _VenuEntity = venueByName.First();
                                        }

                                    }


                                    if (_VenuEntity == null)
                                    {
                                        _VenuEntity = new Venue();
                                        _VenuEntity.SeatGeek_VenuID = _Venue.id.ToString();
                                        _VenuEntity.VenueName = _Venue.name;
                                        _VenuEntity.Extended_Address = _Venue.extended_address;
                                        _VenuEntity.VenueCountry = _Venue.country;
                                        _VenuEntity.Display_Location = _Venue.display_location;
                                        _VenuEntity.Slug = _Venue.slug;
                                        _VenuEntity.VenueState = _Venue.state;
                                        _VenuEntity.Postal_Code = _Venue.postal_code;
                                        _VenuEntity.VenueCity = _Venue.city;
                                        _VenuEntity.Address = _Venue.address;
                                        _VenuEntity.Timezone = _Venue.timezone;

                                        if (_Venue.location != null)
                                        {
                                            _VenuEntity.VenueLat = _Venue.location.lat;
                                            _VenuEntity.VenueLong = _Venue.location.lon;
                                        }

                                        //try
                                        //{
                                        //    if (_Venue.url != "")
                                        //    {
                                        //        strTempImageSave = ResizeImage.Download_Image(_Venue.url);

                                        //        //---New Image path
                                        //        string newFilePath = _SiteRoot + @"\" + "Venu" + @"\" ;
                                        //        Helper.CreateDirectories(_SiteRoot + @"\" + "Venu" + @"\" );

                                        //        string imageresizename = ResizeImage.Resize_Image_Thumb(tempfilePath + strTempImageSave, newFilePath, "_V_" + strTempImageSave, _Imagethumbsize, _Imagethumbsize);
                                        //        strIamgeURLfordb = _SiteURL + "/Venu/" + imageresizename;

                                        //        _Events.ImageURL = strIamgeURLfordb;
                                        //    }
                                        //}
                                        //catch(Exception ex) { }

                                        _VenuEntity.CreatedDate = DateTime.Now;
                                        _VenuEntity.RecordStatus = RecordStatus.SeatGeek.ToString();

                                        _VenueRepo.Repository.Add(_VenuEntity);

                                    }

                                    //Entering Tour records
                                    // _TourDateEntity = _TourDateRepo.Repository.Get(p => p.SeatGeek_TourID == _Ev.id.ToString() );
                                    string _Id = _Ev.id.ToString();
                                    string _datetime_local = _Ev.datetime_local;

                                    DateTime _datetime_local2 = Convert.ToDateTime(_datetime_local);
                                    _TourDateEntity = (from A in _VenueRepo.Repository.GetAll(p => p.VenueID == _VenuEntity.VenueID)
                                                       join B in _TourDateRepo.Repository.GetAll(p =>
                                                                                       (p.SeatGeek_TourID == _Id && p.ArtistID == vArtists.ArtistID)
                                                                                    || (DbFunctions.TruncateTime(p.Datetime_Local).Value.Month == _datetime_local2.Month
                                                                                        && DbFunctions.TruncateTime(p.Datetime_Local).Value.Year == _datetime_local2.Year
                                                                                        && DbFunctions.TruncateTime(p.Datetime_Local).Value.Day == _datetime_local2.Day
                                                                                        && p.ArtistID == vArtists.ArtistID
                                                                                        && p.RecordStatus == RecordStatus.Eventful.ToString())
                                                                                    ) on A.VenueID equals B.VenueID
                                                       where B.ArtistID == vArtists.ArtistID
                                                       select B).FirstOrDefault();


                                    if (_TourDateEntity == null)
                                    {
                                        _TourDateEntity = new TourDate();

                                        _TourDateEntity.SeatGeek_TourID = _Ev.id.ToString();
                                        _TourDateEntity.ArtistID = vArtists.ArtistID;
                                        _TourDateEntity.VenueID = _VenuEntity.VenueID;
                                        _TourDateEntity.EventName = _Ev.title;

                                        _TourDateEntity.EventID = null;
                                        _TourDateEntity.Score = _Ev.score;

                                        _TourDateEntity.Announce_Date = Convert.ToDateTime(_Ev.announce_date);
                                        _TourDateEntity.Visible_Until_utc = Convert.ToDateTime(_Ev.visible_until_utc);
                                        _TourDateEntity.Tour_Utcdate = Convert.ToDateTime(_Ev.datetime_utc);
                                        _TourDateEntity.Datetime_Local = Convert.ToDateTime(_Ev.datetime_local);

                                        _TourDateEntity.CreatedDate = DateTime.Now;
                                        _TourDateEntity.RecordStatus = RecordStatus.SeatGeek.ToString();

                                        if (!String.IsNullOrEmpty(_Ev.url))
                                            _TourDateEntity.TicketURL = _Ev.url;
                                        else if (_Ev.venue != null && !String.IsNullOrEmpty(_Ev.venue.url))
                                            _TourDateEntity.TicketURL = _Ev.venue.url;
                                        else
                                            _TourDateEntity.TicketURL = "https://seatgeek.com/";


                                        _TourDateRepo.Repository.Add(_TourDateEntity);

                                        if (vScan == true)
                                        {
                                            _UserTourDate = new UserTourDate();
                                            _UserTourDate.TourDateID = _TourDateEntity.TourDateID;
                                            _UserTourDate.UserID = vUserID;
                                            _UserTourDate.CreatedDate = DateTime.Now;
                                            _UserTourDate.RecordStatus = RecordStatus.Active.ToString();

                                            _UserTourDateRepo.Repository.Add(_UserTourDate);
                                        }

                                    }
                                    else
                                    {

                                        _TourDateEntity.SeatGeek_TourID = _Ev.id.ToString();
                                        _TourDateEntity.ArtistID = vArtists.ArtistID;
                                        _TourDateEntity.VenueID = _VenuEntity.VenueID;
                                        _TourDateEntity.Score = _Ev.score;
                                        _TourDateEntity.EventName = _Ev.title;

                                        _TourDateEntity.Announce_Date = Convert.ToDateTime(_Ev.announce_date);
                                        _TourDateEntity.Visible_Until_utc = Convert.ToDateTime(_Ev.visible_until_utc);
                                        _TourDateEntity.Tour_Utcdate = Convert.ToDateTime(_Ev.datetime_utc);

                                        _TourDateEntity.Datetime_Local = Convert.ToDateTime(_Ev.datetime_local);

                                        _TourDateEntity.ModifiedDate = DateTime.Now;

                                        if (!String.IsNullOrEmpty(_Ev.url))
                                            _TourDateEntity.TicketURL = _Ev.url;
                                        else if (_Ev.venue != null && !String.IsNullOrEmpty(_Ev.venue.url))
                                            _TourDateEntity.TicketURL = _Ev.venue.url;
                                        else
                                            _TourDateEntity.TicketURL = "https://seatgeek.com/";


                                        _TourDateRepo.Repository.Update(_TourDateEntity);


                                        if (vScan == true)
                                        {
                                            _UserTourDate = _UserTourDateRepo.Repository.Get(p => p.UserID == vUserID && p.TourDateID == _TourDateEntity.TourDateID);

                                            if (_UserTourDate == null)
                                            {
                                                _UserTourDate = new UserTourDate();
                                                _UserTourDate.TourDateID = _TourDateEntity.TourDateID;
                                                _UserTourDate.UserID = vUserID;
                                                _UserTourDate.CreatedDate = DateTime.Now;
                                                _UserTourDate.RecordStatus = RecordStatus.Active.ToString();

                                                _UserTourDateRepo.Repository.Add(_UserTourDate);
                                            }
                                        }

                                    }



                                }

                                #endregion

                            }
                        }


                        #endregion
                    }
                    catch (Exception ex)
                    {
                        LogHelper.CreateLog("SeatGeek_GetEventByArtistName_Asyn (Task 2) " + ex.Message);
                    }
                }
                );
                #endregion

            }//end new artist
            #endregion

            #region "Update Rec"
            else
            {
                if (vArtists.Seatgeek_ID == null)
                {
                    #region "Task 1 to get SeatGeek Detail using Artist Name"

                    SeatGeek.Get_Performers _Get_PerformersRet = await Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            #region "Get SeatGeek Performers (Dont use this while updating the records)"

                            string pattern = @"[^a-zA-Z0-9áéíñóúüÁÉÍÑÓÚÜ\s]";
                            string cleanName = Regex.Replace(vArtists.ArtistName, pattern, "");

                            httpWebRequest = (HttpWebRequest)WebRequest.Create("http://api.seatgeek.com/2/performers" + SGAuth() + "&taxonomies.id=2000000&taxonomies.id=2010000&taxonomies.id=3010100&taxonomies.id=3010200&taxonomies.id=3010300&q=" + cleanName.Trim());
                            httpWebRequest.ContentType = "application/json";
                            httpWebRequest.Method = "GET";

                            httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                            {
                                _result = streamReader.ReadToEnd();
                            }

                            // deserializing 
                            _Get_Performers = JsonConvert.DeserializeObject<SeatGeek.Get_Performers>(_result);

                            if (_Get_Performers != null)
                            {
                                if (_Get_Performers.performers.Count > 0)
                                {

                                    SeatGeek.Performer _Performer = new SeatGeek.Performer();

                                    _Performer = (from A in _Get_Performers.performers
                                                  where DiceCoefficientExtensions.DiceCoefficient(A.name.ToLower(), vArtists.ArtistName.Trim()) >= _FuzzySearchCri && CheckSeatGeekLatin(A.name, _unitOfWork)
                                                  select A).OrderByDescending(p => p.score).FirstOrDefault();


                                    if (_Performer == null)
                                    {
                                        _Performer = _Get_Performers.performers[0];
                                    }

                                    if (_Performer != null)
                                    {
                                        vArtists.OnTour = _Performer.has_upcoming_events == true ? true : false;
                                        _Performer_ID = _Performer.id.ToString();
                                        vArtists.Seatgeek_ID = _Performer_ID;

                                        if (_Performer.taxonomies != null && _Performer.taxonomies.Count > 0)
                                        {
                                            foreach (SeatGeek.Taxonomy _Taxonomy in _Performer.taxonomies)
                                            {
                                                _strEvent = _strEvent + _Taxonomy.name + " ,";
                                            }
                                        }

                                        if (_strEvent.ToString() == "") _strEvent = vArtists.ArtistName + " Event";


                                        #region "Loop through the Genre"
                                        if (_Performer.genres != null && _Performer.genres.Count > 0)
                                        {
                                            foreach (SeatGeek.Genre _Ev in _Performer.genres)
                                            {
                                                _Genre = _GenreRepo.Repository.Get(p => p.Name == _Ev.name.Trim());

                                                if (_Genre == null)
                                                {
                                                    _Genre = new Genre();
                                                    _Genre.Name = _Ev.name;
                                                    _Genre.CreatedDate = DateTime.Now;
                                                    _Genre.RecordStatus = RecordStatus.SeatGeek.ToString();
                                                    _GenreRepo.Repository.Add(_Genre);

                                                }
                                                var check = !_ArtistGenreRepo.Repository.AsQueryable().Any(x => x.ArtistID == vArtists.ArtistID && x.GenreID == _Genre.GenreID);

                                                if (check)
                                                {

                                                    _ArtistGenre = new ArtistGenre();
                                                    _ArtistGenre.GenreID = _Genre.GenreID;
                                                    _ArtistGenre.ArtistID = vArtists.ArtistID;
                                                    _ArtistGenre.Slug = _Ev.slug;
                                                    _ArtistGenre.Primary = _Ev.primary;
                                                    _ArtistGenre.Name = _Ev.name;
                                                    _ArtistGenre.CreatedDate = DateTime.Now;
                                                    _ArtistGenre.RecordStatus = RecordStatus.SeatGeek.ToString();

                                                    _ArtistGenreRepo.Repository.Add(_ArtistGenre);
                                                }
                                            }
                                        }
                                        #endregion

                                        if (String.IsNullOrEmpty(vArtists.ImageURL))
                                            GetProfileImageFromSpotifyFeed(vArtists);

                                        if (WebConfigurationManager.AppSettings["SeatGeek.ArtistPicture"].ToString() == "True")
                                        {
                                            #region "Get Artist Picture "
                                            try
                                            {
                                                if (_Performer.images != null)
                                                {
                                                    if (_Performer.images.huge != null)
                                                    {
                                                        Task<bool> _ArtistPic = ArtistProfilePicture_Asyn(vUserID, vArtists, vUnitOfWork, true, _Performer.images.huge);
                                                    }
                                                }
                                            }
                                            catch (Exception ex)
                                            {

                                            }
                                            #endregion
                                        }
                                    }
                                }
                                else
                                {
                                    vArtists.OnTour = false;
                                }
                            }
                            else
                            {
                                vArtists.OnTour = false;
                            }




                            #endregion


                            // deserializing 
                            return _Get_Performers;

                        }
                        catch (Exception ex)
                        {
                            LogHelper.CreateLog("SeatGeek_GetEventByArtistName_Asyn (Task 1) " + ex.Message);
                            return null;
                        }
                    }
                    );

                    #endregion
                }

                #region "Task2"

                await Task.Factory.StartNew(() =>
                {
                    try
                    {


                        #region "Get SeatGeek Performer Tours/Events"

                        //_Events.SeatGeek_ID
                        string seatid = vArtists.Seatgeek_ID;
                        if (String.IsNullOrEmpty(vArtists.Seatgeek_ID))
                            seatid = "0";

                        httpWebRequest = (HttpWebRequest)WebRequest.Create("http://api.seatgeek.com/2/events" + SGAuth() + "&taxonomies.id=2000000&taxonomies.id=2010000&taxonomies.id=3010100&taxonomies.id=3010200&taxonomies.id=3010300&performers.id=" + seatid + "&page=1&per_page=100");
                        httpWebRequest.ContentType = "application/json";
                        httpWebRequest.Method = "GET";

                        httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                        {
                            _result = streamReader.ReadToEnd();
                        }

                        // deserializing 
                        //var _Get_Performers_Events = JsonConvert.DeserializeObject<SeatGeek2.Get_Performers_Events>(_result);

                        dynamic _Get_Performers_Events = serializer.Deserialize(_result, typeof(object));

                        if (String.IsNullOrEmpty(vArtists.ImageURL))
                            GetProfileImageFromSpotifyFeed(vArtists);

                        if (_Get_Performers_Events != null)
                        {
                            if (_Get_Performers_Events.events.Count > 0)
                            {
                                if (WebConfigurationManager.AppSettings["SeatGeek.ArtistPicture"].ToString() == "True")
                                {
                                    #region "Get Artist Picture "
                                    try
                                    {

                                        if (_Get_Performers_Events.events[0].performers[0].image != null)
                                        {
                                            if (_Get_Performers_Events.events[0].performers[0].image != null)
                                            {
                                                Task<bool> _ArtistPic = ArtistProfilePicture_Asyn(vUserID, vArtists, vUnitOfWork, true, _Get_Performers_Events.events[0].performers[0].image);
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                    #endregion
                                }

                                #region "Loop through the Events"
                                //foreach (SeatGeek2.Event _Ev in _Get_Performers_Events.events)
                                foreach (dynamic _Ev in _Get_Performers_Events.events)
                                {
                                    //SeatGeek2.Venue _Venue = _Ev.venue;
                                    dynamic _Venue = _Ev.venue;

                                    string _VenueID = _Venue.id.ToString();
                                    string _VenueName = _Venue.name.ToLower();

                                    //Venu information
                                    // _VenuEntity = _VenueRepo.Repository.Get(p => p.SeatGeek_VenuID == _Venue.id.ToString());

                                    _VenuEntity = (from A in _VenueRepo.Repository.GetAll()
                                                   where (A.SeatGeek_VenuID == _VenueID)
                                                   select A).FirstOrDefault();

                                    //search the venu using fuzzy searching
                                    if (_VenuEntity == null)
                                    {
                                        /*_VenuEntity = (from A in _VenueRepo.Repository.GetAll()
                                                       where (DiceCoefficientExtensions.DiceCoefficient(A.VenueName.ToLower(), _VenueName) >= _FuzzySearchCri)
                                                       && A.RecordStatus == RecordStatus.Eventful.ToString()
                                                       select A).FirstOrDefault();
                                                       */

                                        string pattern = @"[^a-zA-Z0-9áéíñóúüÁÉÍÑÓÚÜ&\s]";
                                        var search = Regex.Replace(_VenueName, pattern, "");

                                        var venueByName = _VenueRepo.Repository.AsQueryable().Where(x => x.VenueName.Length < 60 && x.RecordStatus == RecordStatus.Eventful.ToString() && x.VenueName.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                                        if (venueByName == null || venueByName.Count() == 0)
                                        {
                                            var allEventfulVenues = _VenueRepo.Repository.AsQueryable().Where(x => x.VenueName.Length < 80 && x.RecordStatus == RecordStatus.Eventful.ToString()).ToList();
                                            foreach (var a in allEventfulVenues)
                                            {
                                                var eventfulVenue = Regex.Replace(a.VenueName, pattern, "");
                                                if (eventfulVenue.ToLower() == search.ToLower())
                                                {
                                                    _VenuEntity = a;
                                                    break;
                                                }
                                                if (search.Length > 11 && eventfulVenue.ToLower().Contains(search.ToLower().Substring(2, search.Length - 2)))
                                                {
                                                    _VenuEntity = a;
                                                    break;
                                                }
                                                if (search.Length > 20 && eventfulVenue.ToLower().Contains(search.ToLower().Substring(5, search.Length - 5)))
                                                {
                                                    _VenuEntity = a;
                                                    break;
                                                }
                                                if (search.Length > 40 && eventfulVenue.ToLower().Contains(search.ToLower().Substring(10, search.Length - 10)))
                                                {
                                                    _VenuEntity = a;
                                                    break;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            _VenuEntity = venueByName.First();
                                        }

                                    }

                                    if (_VenuEntity == null)
                                    {
                                        _VenuEntity = new Venue();
                                        _VenuEntity.SeatGeek_VenuID = _Venue.id.ToString();
                                        _VenuEntity.VenueName = _Venue.name;
                                        _VenuEntity.Extended_Address = _Venue.extended_address;
                                        _VenuEntity.VenueCountry = _Venue.country;
                                        _VenuEntity.Display_Location = _Venue.display_location;
                                        _VenuEntity.Slug = _Venue.slug;
                                        _VenuEntity.VenueState = _Venue.state;
                                        _VenuEntity.Postal_Code = _Venue.postal_code;
                                        _VenuEntity.VenueCity = _Venue.city;
                                        _VenuEntity.Address = _Venue.address;
                                        _VenuEntity.Timezone = _Venue.timezone;

                                        if (_Venue.location != null)
                                        {
                                            _VenuEntity.VenueLat = _Venue.location.lat;
                                            _VenuEntity.VenueLong = _Venue.location.lon;
                                        }

                                        //try
                                        //{
                                        //    if (_Venue.url != "")
                                        //    {
                                        //        strTempImageSave = ResizeImage.Download_Image(_Venue.url);

                                        //        //---New Image path
                                        //        string newFilePath = _SiteRoot + @"\" + "Venu" + @"\" ;
                                        //        Helper.CreateDirectories(_SiteRoot + @"\" + "Venu" + @"\" );

                                        //        string imageresizename = ResizeImage.Resize_Image_Thumb(tempfilePath + strTempImageSave, newFilePath, "_V_" + strTempImageSave, _Imagethumbsize, _Imagethumbsize);
                                        //        strIamgeURLfordb = _SiteURL + "/Venu/" + imageresizename;

                                        //        _Events.ImageURL = strIamgeURLfordb;
                                        //    }
                                        //}
                                        //catch(Exception ex) { }

                                        _VenuEntity.CreatedDate = DateTime.Now;
                                        _VenuEntity.RecordStatus = RecordStatus.SeatGeek.ToString();

                                        _VenueRepo.Repository.Add(_VenuEntity);

                                    }

                                    //Entering Tour records
                                    //_TourDateEntity = _TourDateRepo.Repository.Get(p => p.SeatGeek_TourID == _Ev.id.ToString());
                                    string _Id = _Ev.id.ToString();
                                    string _datetime_local = _Ev.datetime_local;

                                    DateTime _datetime_local2 = Convert.ToDateTime(_datetime_local);
                                    _TourDateEntity = (from A in _VenueRepo.Repository.GetAll(p => p.VenueID == _VenuEntity.VenueID)
                                                       join B in _TourDateRepo.Repository.GetAll(p =>
                                                                                       (p.SeatGeek_TourID == _Id && p.ArtistID == vArtists.ArtistID)
                                                                                    || (DbFunctions.TruncateTime(p.Datetime_Local).Value.Month == _datetime_local2.Month
                                                                                        && DbFunctions.TruncateTime(p.Datetime_Local).Value.Year == _datetime_local2.Year
                                                                                        && DbFunctions.TruncateTime(p.Datetime_Local).Value.Day == _datetime_local2.Day
                                                                                        && p.ArtistID == vArtists.ArtistID
                                                                                        && p.RecordStatus == RecordStatus.Eventful.ToString())
                                                                                    ) on A.VenueID equals B.VenueID
                                                       where B.ArtistID == vArtists.ArtistID
                                                       select B).FirstOrDefault();


                                    if (_TourDateEntity == null)
                                    {
                                        _TourDateEntity = new TourDate();

                                        _TourDateEntity.SeatGeek_TourID = _Ev.id.ToString();
                                        _TourDateEntity.ArtistID = vArtists.ArtistID;
                                        _TourDateEntity.VenueID = _VenuEntity.VenueID;
                                        _TourDateEntity.EventName = _Ev.title;
                                        _TourDateEntity.EventID = null;
                                        _TourDateEntity.Score = _Ev.score;

                                        _TourDateEntity.Announce_Date = Convert.ToDateTime(_Ev.announce_date);
                                        _TourDateEntity.Visible_Until_utc = Convert.ToDateTime(_Ev.visible_until_utc);
                                        _TourDateEntity.Tour_Utcdate = Convert.ToDateTime(_Ev.datetime_utc);
                                        _TourDateEntity.Datetime_Local = Convert.ToDateTime(_Ev.datetime_local);

                                        _TourDateEntity.CreatedDate = DateTime.Now;
                                        _TourDateEntity.RecordStatus = RecordStatus.SeatGeek.ToString();

                                        if (!String.IsNullOrEmpty(_Ev.url))
                                            _TourDateEntity.TicketURL = _Ev.url;
                                        else if (_Ev.venue != null && !String.IsNullOrEmpty(_Ev.venue.url))
                                            _TourDateEntity.TicketURL = _Ev.venue.url;
                                        else
                                            _TourDateEntity.TicketURL = "https://seatgeek.com/";


                                        _TourDateRepo.Repository.Add(_TourDateEntity);
                                    }
                                    else
                                    {

                                        _TourDateEntity.SeatGeek_TourID = _Ev.id.ToString();
                                        _TourDateEntity.ArtistID = vArtists.ArtistID;
                                        _TourDateEntity.VenueID = _VenuEntity.VenueID;
                                        _TourDateEntity.Score = _Ev.score;
                                        _TourDateEntity.EventName = _Ev.title;

                                        _TourDateEntity.Announce_Date = Convert.ToDateTime(_Ev.announce_date);
                                        _TourDateEntity.Visible_Until_utc = Convert.ToDateTime(_Ev.visible_until_utc);
                                        _TourDateEntity.Tour_Utcdate = Convert.ToDateTime(_Ev.datetime_utc);

                                        _TourDateEntity.Datetime_Local = Convert.ToDateTime(_Ev.datetime_local);

                                        if (!String.IsNullOrEmpty(_Ev.url))
                                            _TourDateEntity.TicketURL = _Ev.url;
                                        else if (_Ev.venue != null && !String.IsNullOrEmpty(_Ev.venue.url))
                                            _TourDateEntity.TicketURL = _Ev.venue.url;
                                        else
                                            _TourDateEntity.TicketURL = "https://seatgeek.com/";


                                        _TourDateEntity.ModifiedDate = DateTime.Now;

                                        _TourDateRepo.Repository.Update(_TourDateEntity);

                                    }



                                }
                                #endregion
                            }
                        }


                        #endregion
                    }
                    catch (Exception ex)
                    {
                        LogHelper.CreateLog("SeatGeek_GetEventByArtistName_Asyn (Task 2 - Update) " + ex.Message);
                    }
                }
                );
                #endregion

            }
            #endregion

            // _ArtistsRepo.Repository.Update(vArtists);

            return true;
        }

        #endregion

        #region "Eventful"

        private async Task<bool> EventFul_GetEventInfo_Asyn(Int32 vUserID, Artists vArtists, IUnitOfWork vUnitOfWork, bool vNew)
        {
            string strThumbnailURLfordb = null;
            string strIamgeURLfordb = null;
            string strTempImageSave = null;

            GenericRepository<Venue> _VenueRepo = new GenericRepository<Venue>(vUnitOfWork);
            GenericRepository<TourDate> _TourDateRepo = new GenericRepository<TourDate>(vUnitOfWork);
            GenericRepository<UserTourDate> _UserTourDateRepo = new GenericRepository<UserTourDate>(_unitOfWork);

            Models.Venue _VenuEntity = null;
            Models.TourDate _TourDateEntity = null;
            UserTourDate _UserTourDate = null;

            string _Eventful_app_key = ConfigurationManager.AppSettings["Eventful_app_key"].ToString();


            HttpWebRequest httpWebRequest;
            HttpWebResponse httpResponse;
            string _result;

            var serializer = new JavaScriptSerializer();
            serializer.RegisterConverters(new[] { new DynamicJsonConverter() });
            dynamic obj = null;

            #region "New Rec"
            if (vNew == true)//if new artist
            {
                #region "Task 1 to get Eventful Detail using Artist Name"

                bool _Eventful = await Task.Factory.StartNew(() =>
                {
                    try
                    {
                        #region "Use Eventfull API to Get Performer ID (Dont use this while updating the record"
                        //HttpClient client = new HttpClient(ServiceUri);

                        httpWebRequest = (HttpWebRequest)WebRequest.Create("http://api.eventful.com/json/performers/search?app_key=" + _Eventful_app_key + "&keywords=" + vArtists.ArtistName.Trim());
                        httpWebRequest.ContentType = "application/json";
                        httpWebRequest.Method = "GET";

                        httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                        {
                            _result = streamReader.ReadToEnd();
                        }

                        // deserializing 
                        dynamic _Search_Artist = serializer.Deserialize(_result, typeof(object));

                        // var _Search_Artist = JsonConvert.DeserializeObject<EventFull.Search_Performer>(_result);

                        if (_Search_Artist != null)
                        {
                            if (_Search_Artist.performers != null && String.IsNullOrEmpty(vArtists.About))
                            {
                                if (Convert.ToInt16(_Search_Artist.total_items) > 1)
                                {
                                    vArtists.About = _Search_Artist.performers.performer[0].short_bio;
                                    // _Events.EventFul_ID = _Search_Artist.performers.performer[0].id;
                                    vArtists.Eventful_ID = _Search_Artist.performers.performer[0].id;
                                }
                                else
                                {
                                    vArtists.About = _Search_Artist.performers.performer.short_bio;
                                    // _Events.EventFul_ID = _Search_Artist.performers.performer.id;
                                    vArtists.Eventful_ID = _Search_Artist.performers.performer.id;
                                }
                            }
                        }

                        #endregion

                        if (_Search_Artist != null)
                        {
                            if (_Search_Artist.performers != null)
                            {
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
                    catch (Exception ex)
                    {
                        LogHelper.CreateLog("EventFul_GetEventInfo_Asyn (Task 1) " + ex.Message);
                        return false;
                    }
                }
                );

                #endregion

                #region "Task2 will execute if above task completed sucessfully"
                if (_Eventful == true)
                {
                    bool _Eventful2 = await Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            #region "Use Eventfull API To Get Performer Detail"

                            httpWebRequest = (HttpWebRequest)WebRequest.Create("http://api.eventful.com/json/performers/get?app_key=" + _Eventful_app_key + "&id=" + vArtists.Eventful_ID + "&show_events=true&image_sizes=large");
                            httpWebRequest.ContentType = "application/json";
                            httpWebRequest.Method = "GET";

                            httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                            {
                                _result = streamReader.ReadToEnd();
                            }



                            // deserializing 
                            //var _Get_Performer_Events = JsonConvert.DeserializeObject<EventFull2.Get_Performer_Events>(_result);
                            dynamic _Get_Performer_Events = serializer.Deserialize(_result, typeof(object));

                            if (String.IsNullOrEmpty(vArtists.ImageURL))
                                GetProfileImageFromSpotifyFeed(vArtists);


                            if (_Get_Performer_Events != null)
                            {
                                vArtists.About = _Get_Performer_Events.long_bio;

                                #region "Get Artist Picture "
                                if (_Get_Performer_Events.images != null)
                                {

                                    if (_Get_Performer_Events.images.image.Count > 0)
                                    {
                                        if (WebConfigurationManager.AppSettings["Eventful.ArtistPicture"].ToString() == "True")
                                        {
                                            if (_Get_Performer_Events.images.image[0].large != null)
                                            {
                                                Task<bool> _ArtistPic = ArtistProfilePicture_Asyn(vUserID, vArtists, vUnitOfWork, true, _Get_Performer_Events.images.image[0].large.url);
                                            }
                                        }

                                    }
                                    /*else
                                    {
                                        if (WebConfigurationManager.AppSettings["Eventful.ArtistPicture"].ToString() == "True")
                                        {
                                            if (_Get_Performer_Events.images.image.large != null)
                                            {
                                                Task<bool> _ArtistPic = ArtistProfilePicture_Asyn(vUserID, vArtists, vUnitOfWork, true, _Get_Performer_Events.images.image.large.url);
                                            }
                                        }
                                    }*/


                                }
                                #endregion

                                #region "Loop through the events to get  Tour/Venue"
                                if (_Get_Performer_Events.events != null)
                                {
                                    if (_Get_Performer_Events.events.@event is IEnumerable)
                                    {

                                        foreach (dynamic _event in _Get_Performer_Events.events.@event)
                                        {
                                            try
                                            {
                                                httpWebRequest = (HttpWebRequest)WebRequest.Create("http://api.eventful.com/json/events/get?app_key=" + _Eventful_app_key + "&id=" + _event.id + "");
                                                httpWebRequest.ContentType = "application/json";
                                                httpWebRequest.Method = "GET";

                                                httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                                                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                                                {
                                                    _result = streamReader.ReadToEnd();
                                                }

                                                // deserializing 
                                                //var _Get_Event_ByID = JsonConvert.DeserializeObject<EventFull3.Get_Event_ByID>(_result);
                                                dynamic _Get_Event_ByID = serializer.Deserialize(_result, typeof(object));

                                                if (_Get_Event_ByID != null)
                                                {
                                                    //Venu information
                                                    //_VenuEntity = _VenueRepo.Repository.Get(p => (p.Eventful_VenueID == _Get_Event_ByID.id.ToString()) ||
                                                    //                                             (p.VenueName.ToLower() == _Get_Event_ByID.venue_name.ToLower())
                                                    //        );


                                                    //using fuzzy searching techinque here
                                                    _VenuEntity = (from A in _VenueRepo.Repository.GetAll()
                                                                   where (A.Eventful_VenueID == _Get_Event_ByID.venue_id.ToString())
                                                                   select A).FirstOrDefault();


                                                    //search the venu using fuzzy searching
                                                    if (_VenuEntity == null)
                                                    {
                                                        /* _VenuEntity = (from A in _VenueRepo.Repository.GetAll()
                                                                       where (DiceCoefficientExtensions.DiceCoefficient(A.VenueName.ToLower(), _Get_Event_ByID.venue_name.ToLower()) >= _FuzzySearchCri)
                                                                       && A.RecordStatus == RecordStatus.SeatGeek.ToString()
                                                                       select A).FirstOrDefault();
                                                                       */

                                                        string name = _Get_Event_ByID.venue_name.ToLower();
                                                        string pattern = @"[^a-zA-Z0-9áéíñóúüÁÉÍÑÓÚÜ&\s]";
                                                        var search = Regex.Replace(name, pattern, "");

                                                        var venueByName = _VenueRepo.Repository.AsQueryable().Where(x => x.VenueName.Length < 60 && x.RecordStatus == RecordStatus.SeatGeek.ToString() && x.VenueName.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                                                        if (venueByName == null || venueByName.Count() == 0)
                                                        {
                                                            var allEventfulVenues = _VenueRepo.Repository.AsQueryable().Where(x => x.VenueName.Length < 80 && x.RecordStatus == RecordStatus.SeatGeek.ToString()).ToList();
                                                            foreach (var a in allEventfulVenues)
                                                            {
                                                                var eventfulVenue = Regex.Replace(a.VenueName, pattern, "");
                                                                if (eventfulVenue.ToLower() == search.ToLower())
                                                                {
                                                                    _VenuEntity = a;
                                                                    break;
                                                                }
                                                                if (search.Length > 11 && eventfulVenue.ToLower().Contains(search.ToLower().Substring(2, search.Length - 2)))
                                                                {
                                                                    _VenuEntity = a;
                                                                    break;
                                                                }
                                                                if (search.Length > 20 && eventfulVenue.ToLower().Contains(search.ToLower().Substring(5, search.Length - 5)))
                                                                {
                                                                    _VenuEntity = a;
                                                                    break;
                                                                }
                                                                if (search.Length > 40 && eventfulVenue.ToLower().Contains(search.ToLower().Substring(10, search.Length - 10)))
                                                                {
                                                                    _VenuEntity = a;
                                                                    break;
                                                                }
                                                            }
                                                        }
                                                        else
                                                        {
                                                            _VenuEntity = venueByName.First();
                                                        }

                                                    }
                                                    if (_VenuEntity == null)
                                                    {
                                                        _VenuEntity = new Venue();
                                                        _VenuEntity.Eventful_VenueID = _Get_Event_ByID.venue_id.ToString();
                                                        _VenuEntity.SeatGeek_VenuID = null;

                                                        _VenuEntity.VenueName = _Get_Event_ByID.venue_name;
                                                        _VenuEntity.Extended_Address = _Get_Event_ByID.address;
                                                        _VenuEntity.VenueCountry = _Get_Event_ByID.country;
                                                        _VenuEntity.Display_Location = "";
                                                        _VenuEntity.Slug = "";
                                                        _VenuEntity.VenueState = "";
                                                        _VenuEntity.Postal_Code = _Get_Event_ByID.postal_code;
                                                        _VenuEntity.VenueCity = _Get_Event_ByID.city;
                                                        _VenuEntity.Address = _Get_Event_ByID.address;
                                                        _VenuEntity.Timezone = "";

                                                        _VenuEntity.VenueLat = Convert.ToDecimal(_Get_Event_ByID.latitude);
                                                        _VenuEntity.VenueLong = Convert.ToDecimal(_Get_Event_ByID.longitude);

                                                        _VenuEntity.CreatedDate = DateTime.Now;
                                                        _VenuEntity.RecordStatus = RecordStatus.Eventful.ToString();

                                                        _VenueRepo.Repository.Add(_VenuEntity);

                                                    }

                                                    string _Event_ID = _Get_Event_ByID.id.ToString();
                                                    string _start_time = _Get_Event_ByID.start_time;


                                                    //Entering Tour records
                                                    /* _TourDateEntity = (from A in _VenueRepo.Repository.GetAll(p => p.VenueID == _VenuEntity.VenueID)
                                                                        join B in _TourDateRepo.Repository.GetAll(p =>
                                                                                                        (p.Eventful_TourID == _Event_ID && p.ArtistID == vArtists.ArtistID)
                                                                                                     || (Convert.ToDateTime(p.Datetime_Local).Month == Convert.ToDateTime(_start_time).Month
                                                                                                         && Convert.ToDateTime(p.Datetime_Local).Year == Convert.ToDateTime(_start_time).Year
                                                                                                         && Convert.ToDateTime(p.Datetime_Local).Day == Convert.ToDateTime(_start_time).Day
                                                                                                         && p.ArtistID == vArtists.ArtistID
                                                                                                         && p.RecordStatus == RecordStatus.SeatGeek.ToString())
                                                                                                     ) on A.VenueID equals B.VenueID
                                                                        where B.ArtistID == vArtists.ArtistID
                                                                        select B).FirstOrDefault();
                                                                        */




                                                    if (_TourDateEntity == null)
                                                    {
                                                        DateTime local = Convert.ToDateTime(_Get_Event_ByID.start_time);
                                                        if (local != null)
                                                        {
                                                            string strLocal = local.ToString("MM/dd/yyyy");
                                                            _TourDateEntity = _TourDateRepo.Repository.AsQueryable().Where(x => x.ArtistID == vArtists.ArtistID && (x.Datetime_Local.DateToString() == strLocal || x.Visible_Until_utc.DateToString() == strLocal || x.Tour_Utcdate.DateToString() == strLocal)).FirstOrDefault();
                                                        }

                                                        if (_TourDateEntity == null)
                                                        {
                                                            _TourDateEntity = new TourDate();

                                                            _TourDateEntity.Eventful_TourID = _Get_Event_ByID.id.ToString();
                                                            _TourDateEntity.SeatGeek_TourID = null;

                                                            _TourDateEntity.ArtistID = vArtists.ArtistID;
                                                            _TourDateEntity.VenueID = _VenuEntity.VenueID;
                                                            _TourDateEntity.EventName = _Get_Event_ByID.title;
                                                            _TourDateEntity.EventID = null;
                                                            _TourDateEntity.Score = 0;

                                                            _TourDateEntity.Announce_Date = null;
                                                            _TourDateEntity.Visible_Until_utc = Convert.ToDateTime(_Get_Event_ByID.start_time);
                                                            _TourDateEntity.Tour_Utcdate = Convert.ToDateTime(_Get_Event_ByID.start_time);
                                                            _TourDateEntity.Datetime_Local = Convert.ToDateTime(_Get_Event_ByID.start_time);

                                                            _TourDateEntity.CreatedDate = DateTime.Now;
                                                            _TourDateEntity.RecordStatus = RecordStatus.Eventful.ToString();

                                                            try
                                                            {
                                                                if (_Get_Event_ByID.url != null)
                                                                    _TourDateEntity.TicketURL = _Get_Event_ByID.url;
                                                                else if (_Get_Event_ByID.links != null)
                                                                    _TourDateEntity.TicketURL = _Get_Event_ByID.links.link[0].url;
                                                            }
                                                            catch { }

                                                            if (String.IsNullOrEmpty(_TourDateEntity.TicketURL))
                                                                _TourDateEntity.TicketURL = "http://eventful.com/";


                                                            //if(_TourDateRepo.Repository.AsQueryable().Any(x=>x.TourDateID == _TourDateEntity.TourDateID)
                                                            _TourDateRepo.Repository.Add(_TourDateEntity);

                                                            if (_UserTourDate == null)
                                                            {
                                                                _UserTourDate = new UserTourDate();
                                                                _UserTourDate.TourDateID = _TourDateEntity.TourDateID;
                                                                _UserTourDate.UserID = vUserID;
                                                                _UserTourDate.CreatedDate = DateTime.Now;
                                                                _UserTourDate.RecordStatus = RecordStatus.Active.ToString();

                                                                /*if (_Get_Event_ByID.links != null)
                                                                {
                                                                    try
                                                                    {
                                                                        _TourDateEntity.TicketURL = _Get_Event_ByID.links.link[0].url;
                                                                    }
                                                                    catch { }
                                                                }
                                                                else
                                                                {
                                                                    _TourDateEntity.TicketURL = "http://eventful.com/";
                                                                }*/

                                                                _UserTourDateRepo.Repository.Add(_UserTourDate);
                                                            }

                                                        }
                                                    }




                                                    /*SOMETHING HERE USERTOURDATE*/

                                                    //else
                                                    //{

                                                    //    _TourDateEntity.SeatGeek_TourID = _Get_Event_ByID.id.ToString();
                                                    //    _TourDateEntity.ArtistID = vArtists.ArtistID;
                                                    //    _TourDateEntity.VenueID = _VenuEntity.VenueID;
                                                    //    _TourDateEntity.EventName = _Get_Event_ByID.title;
                                                    //    _TourDateEntity.EventID = null;
                                                    //    _TourDateEntity.Score = 0;

                                                    //    _TourDateEntity.Announce_Date = null;
                                                    //    _TourDateEntity.Visible_Until_utc = Convert.ToDateTime(_Get_Event_ByID.start_time);
                                                    //    _TourDateEntity.Tour_Utcdate = Convert.ToDateTime(_Get_Event_ByID.start_time);
                                                    //    _TourDateEntity.Datetime_Local = Convert.ToDateTime(_Get_Event_ByID.start_time);

                                                    //    _TourDateEntity.ModifiedDate = DateTime.Now;

                                                    //    _TourDateRepo.Repository.Update(_TourDateEntity);

                                                    //}

                                                }

                                            }
                                            catch (Exception ex)
                                            {
                                            }

                                        }

                                    }
                                    #endregion
                                }
                            }

                            #endregion

                            if (_Get_Performer_Events != null)
                            {
                                return true;
                            }
                            else
                            {
                                return false;
                            }


                        }
                        catch (Exception ex)
                        {
                            LogHelper.CreateLog("EventFul_GetEventInfo_Asyn (Task 2) " + ex.Message + " API : " +
                               "http://api.eventful.com/json/performers/get?app_key=" + _Eventful_app_key + "&id=" + vArtists.Eventful_ID + "&show_events=true&image_sizes=large");
                            return false;
                        }
                    }
              );


                }
                #endregion

            }//end new artist
            #endregion

            #region "Update Rec"
            else
            {

                #region "Task2 "

                bool _Eventful2 = await Task.Factory.StartNew(() =>
                {
                    try
                    {
                        #region "Use Eventfull API To Get Performer Detail"

                        httpWebRequest = (HttpWebRequest)WebRequest.Create("http://api.eventful.com/json/performers/get?app_key=" + _Eventful_app_key + "&id=" + vArtists.Eventful_ID + "&show_events=true&image_sizes=large");
                        httpWebRequest.ContentType = "application/json";
                        httpWebRequest.Method = "GET";

                        httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                        {
                            _result = streamReader.ReadToEnd();
                        }

                        // deserializing 
                        //var _Get_Performer_Events = JsonConvert.DeserializeObject<EventFull2.Get_Performer_Events>(_result);
                        dynamic _Get_Performer_Events = serializer.Deserialize(_result, typeof(object));


                        if (_Get_Performer_Events != null)
                        {
                            if (String.IsNullOrEmpty(vArtists.About))
                                vArtists.About = _Get_Performer_Events.long_bio;

                            #region "Get Artist Picture "
                            if (_Get_Performer_Events.images != null)
                            {
                                if (_Get_Performer_Events.images.image.Count > 0)
                                {

                                    if (WebConfigurationManager.AppSettings["Eventful.ArtistPicture"].ToString() == "True")
                                    {
                                        if (_Get_Performer_Events.images.image[0].large != null)
                                        {
                                            Task<bool> _ArtistPic = ArtistProfilePicture_Asyn(vUserID, vArtists, vUnitOfWork, true, _Get_Performer_Events.images.image[0].large.url);
                                        }
                                    }

                                }
                                else
                                {
                                    if (WebConfigurationManager.AppSettings["Eventful.ArtistPicture"].ToString() == "True")
                                    {
                                        if (_Get_Performer_Events.images.image.large != null)
                                        {
                                            Task<bool> _ArtistPic = ArtistProfilePicture_Asyn(vUserID, vArtists, vUnitOfWork, true, _Get_Performer_Events.images.image.large.url);
                                        }
                                    }

                                }
                            }
                            #endregion

                            #region "Loop through the events to get  Tour/Venue"

                            if (_Get_Performer_Events.events != null)
                            {
                                if (_Get_Performer_Events.events.@event is IEnumerable)
                                {
                                    foreach (dynamic _event in _Get_Performer_Events.events.@event)
                                    {
                                        try
                                        {
                                            httpWebRequest = (HttpWebRequest)WebRequest.Create("http://api.eventful.com/json/events/get?app_key=" + _Eventful_app_key + "&id=" + _event.id + "");
                                            httpWebRequest.ContentType = "application/json";
                                            httpWebRequest.Method = "GET";

                                            httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                                            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                                            {
                                                _result = streamReader.ReadToEnd();
                                            }

                                            // deserializing 
                                            //var _Get_Event_ByID = JsonConvert.DeserializeObject<EventFull3.Get_Event_ByID>(_result);
                                            dynamic _Get_Event_ByID = serializer.Deserialize(_result, typeof(object));

                                            if (_Get_Event_ByID != null)
                                            {
                                                //Venu information
                                                //_VenuEntity = _VenueRepo.Repository.Get(p => (p.Eventful_VenueID == _Get_Event_ByID.venue_id.ToString()) ||
                                                //                                       (p.VenueName.ToLower() == _Get_Event_ByID.venue_name.ToLower())
                                                //        );


                                                //using fuzzy searching techinque here
                                                _VenuEntity = (from A in _VenueRepo.Repository.GetAll()
                                                               where (A.Eventful_VenueID == _Get_Event_ByID.venue_id.ToString())
                                                               select A).FirstOrDefault();

                                                //search the venu using fuzzy searching
                                                if (_VenuEntity == null)
                                                {
                                                    /* _VenuEntity = (from A in _VenueRepo.Repository.GetAll()
                                                                   where (DiceCoefficientExtensions.DiceCoefficient(A.VenueName.ToLower(), _Get_Event_ByID.venue_name.ToLower()) >= _FuzzySearchCri)
                                                                   && A.RecordStatus == RecordStatus.SeatGeek.ToString()
                                                                   select A).FirstOrDefault();
                                                                   */

                                                    string name = _Get_Event_ByID.venue_name.ToLower();
                                                    string pattern = @"[^a-zA-Z0-9áéíñóúüÁÉÍÑÓÚÜ&\s]";
                                                    var search = Regex.Replace(name, pattern, "");

                                                    var venueByName = _VenueRepo.Repository.AsQueryable().Where(x => x.VenueName.Length < 60 && x.RecordStatus == RecordStatus.SeatGeek.ToString() && x.VenueName.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                                                    if (venueByName == null || venueByName.Count() == 0)
                                                    {
                                                        var allEventfulVenues = _VenueRepo.Repository.AsQueryable().Where(x => x.VenueName.Length < 80 && x.RecordStatus == RecordStatus.SeatGeek.ToString()).ToList();
                                                        foreach (var a in allEventfulVenues)
                                                        {
                                                            var eventfulVenue = Regex.Replace(a.VenueName, pattern, "");
                                                            if (eventfulVenue.ToLower() == search.ToLower())
                                                            {
                                                                _VenuEntity = a;
                                                                break;
                                                            }
                                                            if (search.Length > 11 && eventfulVenue.ToLower().Contains(search.ToLower().Substring(2, search.Length - 2)))
                                                            {
                                                                _VenuEntity = a;
                                                                break;
                                                            }
                                                            if (search.Length > 20 && eventfulVenue.ToLower().Contains(search.ToLower().Substring(5, search.Length - 5)))
                                                            {
                                                                _VenuEntity = a;
                                                                break;
                                                            }
                                                            if (search.Length > 40 && eventfulVenue.ToLower().Contains(search.ToLower().Substring(10, search.Length - 10)))
                                                            {
                                                                _VenuEntity = a;
                                                                break;
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        _VenuEntity = venueByName.First();
                                                    }

                                                }

                                                if (_VenuEntity == null)
                                                {
                                                    _VenuEntity = new Venue();
                                                    _VenuEntity.Eventful_VenueID = _Get_Event_ByID.venue_id.ToString();
                                                    _VenuEntity.VenueName = _Get_Event_ByID.venue_name;
                                                    _VenuEntity.Extended_Address = _Get_Event_ByID.address;
                                                    _VenuEntity.VenueCountry = _Get_Event_ByID.country;
                                                    _VenuEntity.Display_Location = "";
                                                    _VenuEntity.Slug = "";
                                                    _VenuEntity.VenueState = "";
                                                    _VenuEntity.Postal_Code = _Get_Event_ByID.postal_code;
                                                    _VenuEntity.VenueCity = _Get_Event_ByID.city;
                                                    _VenuEntity.Address = _Get_Event_ByID.address;
                                                    _VenuEntity.Timezone = "";

                                                    _VenuEntity.VenueLat = Convert.ToDecimal(_Get_Event_ByID.latitude);
                                                    _VenuEntity.VenueLong = Convert.ToDecimal(_Get_Event_ByID.longitude);

                                                    _VenuEntity.CreatedDate = DateTime.Now;
                                                    _VenuEntity.RecordStatus = RecordStatus.Eventful.ToString();

                                                    _VenueRepo.Repository.Add(_VenuEntity);

                                                }

                                                if ("E0-001-094001471-9@2016072021" == _Get_Event_ByID.id.ToString())
                                                {
                                                    string _st = _Get_Event_ByID.id.ToString();
                                                }
                                                string _Event_ID = _Get_Event_ByID.id.ToString();
                                                string _start_time = _Get_Event_ByID.start_time;


                                                //Entering Tour records
                                                DateTime _datetime_local = Convert.ToDateTime(_start_time);
                                                _TourDateEntity = (from A in _VenueRepo.Repository.GetAll(p => p.VenueID == _VenuEntity.VenueID)
                                                                   join B in _TourDateRepo.Repository.GetAll(p =>
                                                                                                   (p.Eventful_TourID == _Event_ID && p.ArtistID == vArtists.ArtistID)
                                                                                                || (DbFunctions.TruncateTime(p.Datetime_Local).Value.Month == _datetime_local.Month
                                                                                                    && DbFunctions.TruncateTime(p.Datetime_Local).Value.Year == _datetime_local.Year
                                                                                                    && DbFunctions.TruncateTime(p.Datetime_Local).Value.Day == _datetime_local.Day
                                                                                                    && p.ArtistID == vArtists.ArtistID
                                                                                                    && p.RecordStatus == RecordStatus.SeatGeek.ToString())
                                                                                                ) on A.VenueID equals B.VenueID
                                                                   where B.ArtistID == vArtists.ArtistID
                                                                   select B).FirstOrDefault();







                                                if (_TourDateEntity == null)
                                                {
                                                    DateTime local = Convert.ToDateTime(_Get_Event_ByID.start_time);
                                                    if (local != null)
                                                    {
                                                        string strLocal = local.ToString("MM/dd/yyyy");
                                                        _TourDateEntity = _TourDateRepo.Repository.AsQueryable().Where(x => x.ArtistID == vArtists.ArtistID && (x.Datetime_Local.DateToString() == strLocal || x.Visible_Until_utc.DateToString() == strLocal || x.Tour_Utcdate.DateToString() == strLocal)).FirstOrDefault();
                                                    }
                                                }

                                                if (_TourDateEntity == null)
                                                {
                                                    _TourDateEntity = new TourDate();

                                                    _TourDateEntity.Eventful_TourID = _Get_Event_ByID.id.ToString();
                                                    _TourDateEntity.ArtistID = vArtists.ArtistID;
                                                    _TourDateEntity.VenueID = _VenuEntity.VenueID;
                                                    _TourDateEntity.EventName = _Get_Event_ByID.title;
                                                    _TourDateEntity.EventID = null;
                                                    _TourDateEntity.Score = 0;

                                                    _TourDateEntity.Announce_Date = null;
                                                    _TourDateEntity.Visible_Until_utc = Convert.ToDateTime(_Get_Event_ByID.start_time);
                                                    _TourDateEntity.Tour_Utcdate = Convert.ToDateTime(_Get_Event_ByID.start_time);
                                                    _TourDateEntity.Datetime_Local = Convert.ToDateTime(_Get_Event_ByID.start_time);

                                                    _TourDateEntity.CreatedDate = DateTime.Now;
                                                    _TourDateEntity.RecordStatus = RecordStatus.Eventful.ToString();

                                                    try
                                                    {
                                                        if (_Get_Event_ByID.url != null)
                                                            _TourDateEntity.TicketURL = _Get_Event_ByID.url;
                                                        else if (_Get_Event_ByID.links != null)
                                                            _TourDateEntity.TicketURL = _Get_Event_ByID.links.link[0].url;
                                                    }
                                                    catch { }

                                                    if (String.IsNullOrEmpty(_TourDateEntity.TicketURL))
                                                        _TourDateEntity.TicketURL = "http://eventful.com/";

                                                    _TourDateRepo.Repository.Add(_TourDateEntity);



                                                    _UserTourDate = _UserTourDateRepo.Repository.Get(p => p.UserID == vUserID && p.TourDateID == _TourDateEntity.TourDateID);

                                                    if (_UserTourDate == null)
                                                    {
                                                        _UserTourDate = new UserTourDate();
                                                        _UserTourDate.TourDateID = _TourDateEntity.TourDateID;
                                                        _UserTourDate.UserID = vUserID;
                                                        _UserTourDate.CreatedDate = DateTime.Now;
                                                        _UserTourDate.RecordStatus = RecordStatus.Active.ToString();

                                                        /*if (_Get_Event_ByID.links != null)
                                                        {
                                                            try
                                                            {
                                                                _TourDateEntity.TicketURL = _Get_Event_ByID.links.link[0].url;
                                                            }
                                                            catch { }
                                                        }
                                                        else
                                                        {
                                                            _TourDateEntity.TicketURL = "http://eventful.com/";
                                                        }*/

                                                        _UserTourDateRepo.Repository.Add(_UserTourDate);
                                                    }

                                                }





                                                //else
                                                //{

                                                //    _TourDateEntity.Eventful_TourID = _Get_Event_ByID.id.ToString();
                                                //    _TourDateEntity.ArtistID = vArtists.ArtistID;
                                                //    _TourDateEntity.VenueID = _VenuEntity.VenueID;
                                                //    _TourDateEntity.EventName = _Get_Event_ByID.title;
                                                //    _TourDateEntity.EventID = null;
                                                //    _TourDateEntity.Score = 0;

                                                //    _TourDateEntity.Announce_Date = null;
                                                //    _TourDateEntity.Visible_Until_utc = Convert.ToDateTime(_Get_Event_ByID.start_time);
                                                //    _TourDateEntity.Tour_Utcdate = Convert.ToDateTime(_Get_Event_ByID.start_time);
                                                //    _TourDateEntity.Datetime_Local = Convert.ToDateTime(_Get_Event_ByID.start_time);

                                                //    _TourDateEntity.ModifiedDate = DateTime.Now;

                                                //    _TourDateRepo.Repository.Update(_TourDateEntity);

                                                //}

                                            }

                                        }
                                        catch (Exception ex)
                                        {
                                        }

                                    }
                                }
                            }
                            #endregion

                        }


                        #endregion

                        if (_Get_Performer_Events != null)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }


                    }
                    catch (Exception ex)
                    {
                        LogHelper.CreateLog("EventFul_GetEventInfo_Asyn (Task 2 - Update) " + ex.Message);
                        return false;
                    }
                }
             );



                #endregion
            }
            #endregion

            return true;
        }

        #endregion

        #region "Spotify"
        private bool CheckLastResortSpotifyGenre(string spotifyID)
        {
            try
            {
                if (!String.IsNullOrEmpty(spotifyID))
                {
                    string spotifyUrl = "https://api.spotify.com/v1/artists/" + spotifyID;
                    HttpWebRequest httpWebRequest;
                    HttpWebResponse httpResponse;
                    string _result;
                    string imageUrl = "";

                    httpWebRequest = (HttpWebRequest)WebRequest.Create(spotifyUrl);
                    httpWebRequest.ContentType = "application/json";
                    httpWebRequest.Method = "GET";
                    httpWebRequest.Headers.Add("Authorization", "Bearer " + RetSpotifyAccessToken());

                    httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        _result = streamReader.ReadToEnd();
                    }



                    JObject joResponse = JObject.Parse(_result);
                    JArray items = (JArray)joResponse["genres"];

                    if (items != null && items.Count > 0)
                    {
                        foreach (var i in items)
                        {
                            if (_GenreFilter.Any(x => x.ToLower() == i.ToString()))
                                return true;
                        }
                    }
                    else
                        return false;

                }

                return false;
            }
            catch (Exception e)
            { return false; }
        }


        private void GetProfileImageFromSpotifyFeed(Artists artist)
        {
            GenericRepository<Artists> _ArtistRepository = new GenericRepository<Artists>(_unitOfWork);

            try
            {

                if ((!String.IsNullOrEmpty(artist.Spotify_ID) && (String.IsNullOrEmpty(artist.ImageURL) || String.IsNullOrEmpty(artist.BannerImage_URL) || String.IsNullOrEmpty(artist.ThumbnailURL))))
                {
                    string spotifyUrl = "https://api.spotify.com/v1/artists/" + artist.Spotify_ID;
                    HttpWebRequest httpWebRequest;
                    HttpWebResponse httpResponse;
                    string _result;
                    string imageUrl = "";

                    httpWebRequest = (HttpWebRequest)WebRequest.Create(spotifyUrl);
                    httpWebRequest.ContentType = "application/json";
                    httpWebRequest.Method = "GET";
                    httpWebRequest.Headers.Add("Authorization", "Bearer " + RetSpotifyAccessToken());

                    httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        _result = streamReader.ReadToEnd();
                    }

                    JObject joResponse = JObject.Parse(_result);
                    JArray items = (JArray)joResponse["images"];

                    var image = items.FirstOrDefault();
                    if (image != null)
                        imageUrl = image.Value<string>("url");

                    if (!String.IsNullOrEmpty(imageUrl))
                    {
                        Task<bool> _ArtistPic = SpotifyProfilePicture_Asyn(0, artist, _ArtistRepository, true, imageUrl);

                        /*if (String.IsNullOrEmpty(artist.ImageURL))
                            artist.ImageURL = imageUrl;

                        if (String.IsNullOrEmpty(artist.BannerImage_URL))
                            artist.BannerImage_URL = imageUrl;

                        if (String.IsNullOrEmpty(artist.ThumbnailURL))
                            artist.ThumbnailURL = imageUrl;

                        _ArtistRepository.Repository.Update(artist);*/

                    }



                }

            }
            catch (Exception ex)
            {
                //  LogHelper.CreateLog("MusicGrapgh_GetSimilarArtists_Asyn " + ex.Message);
                //  return null;
            }


        }

        private async Task<bool> Spotify_GetSongInfo_Asyn(Int32 vUserID, Artists vArtists, IUnitOfWork vUnitOfWork, bool vNew)
        {
            HttpWebRequest httpWebRequest;
            HttpWebResponse httpResponse;
            string _result;

            string _Spotify_Country = ConfigurationManager.AppSettings["Spotify_Country"].ToString();

            #region "New Rec"
            if (vNew == true)
            {
                #region "Task 1 to get Spotify Detail using Spotify ID"
                await Task.Factory.StartNew(() =>
                {
                    try
                    {
                        #region "Start Spotify to get music Preview"
                        if (vArtists.Spotify_ID != null && vArtists.Spotify_ID != "")
                        {
                            List<Track> _task = Task.Run(async () => await Track.GetArtistTopTracks(vArtists.Spotify_ID, _Spotify_Country)).Result;

                            if (_task != null && _task.Count > 0)
                            {
                                _task = _task.OrderByDescending(t => t.Popularity).ToList();

                                if (_task.Count > 2)
                                {
                                    vArtists.Spotify_Url = _task[2].PreviewUrl;
                                    vArtists.Spotify_URL_Name = _task[2].Name;
                                }
                                else
                                {
                                    vArtists.Spotify_Url = _task[0].PreviewUrl;
                                    vArtists.Spotify_URL_Name = _task[0].Name;
                                }
                            }
                        }
                        #endregion
                    }
                    catch (Exception ex)
                    {
                        LogHelper.CreateLog("Spotify_GetSongInfo_Asyn " + ex.Message);
                    }
                }
                 );

                #endregion
            }
            #endregion

            #region "Update Rec"
            else
            {

                #region "Task 1 to get Spotify Detail using Spotify ID"

                await Task.Factory.StartNew(() =>
                {
                    try
                    {
                        #region "Start Spotify to get music Preview"

                        if (vArtists.Spotify_ID != null && vArtists.Spotify_ID != "")
                        {
                            List<Track> _task = Task.Run(async () => await Track.GetArtistTopTracks(vArtists.Spotify_ID, _Spotify_Country)).Result;

                            if (_task != null && _task.Count > 0)
                            {
                                _task = _task.OrderByDescending(t => t.Popularity).ToList();

                                if (_task.Count > 2)
                                {
                                    vArtists.Spotify_Url = _task[2].PreviewUrl;
                                    vArtists.Spotify_URL_Name = _task[2].Name;
                                }
                                else
                                {
                                    vArtists.Spotify_Url = _task[0].PreviewUrl;
                                    vArtists.Spotify_URL_Name = _task[0].Name;
                                }
                            }

                        }

                        #endregion

                    }
                    catch (Exception ex)
                    {
                        LogHelper.CreateLog("Spotify_GetSongInfo_Asyn (Update)" + ex.Message);
                    }
                }
                );

                #endregion
            }
            #endregion

            return true;
        }



        //New API's for spotify
        private async Task<List<MusicGraph.Datum>> Spotify_SearchArtist(string _q)
        {
            try
            {
                List<MusicGraph.Datum> _artistlst = new List<MusicGraph.Datum>();

                string _Spotify_Country = ConfigurationManager.AppSettings["Spotify_Country"].ToString();

                //Task.Factory.StartNew(() =>
                {
                    Page<Artist> _lst = await Artist.Search(_q);

                    foreach (var item in _lst.Items)
                    {
                        string _Genre = "";
                        bool _Islatin = false;

                        if (item.Genres != null && item.Genres.Count > 0)
                        {
                            _Genre = item.Genres[0];

                            foreach (var _gen in item.Genres)
                            {
                                if (_GenreFilter.Any(x => x.ToLower() == _gen.ToString()))
                                {
                                    _Islatin = true;
                                    break;
                                }
                            }
                        }
                        _artistlst.Add(new MusicGraph.Datum
                        {
                            id = item.Id,
                            name = item.Name,
                            main_genre = _Genre,
                            IsLatin = _Islatin
                        });
                    }
                    return _artistlst;
                }
                //);
            }
            catch (Exception ex)
            {
                LogHelper.CreateLog(ex);
                return null;
            }

        }
        #endregion

        #region "Profile Picture"

        private async Task<bool> ArtistProfilePicture_Asyn(Int32 vUserID, Artists vArtists, IUnitOfWork vUnitOfWork, bool vNew, string Url)
        {
            await Task.Factory.StartNew(() =>
            {

                try
                {
                    if (String.IsNullOrEmpty(vArtists.ImageURL))
                        GetProfileImageFromSpotifyFeed(vArtists);

                    if (String.IsNullOrEmpty(vArtists.ImageURL))
                    {

                        string strpThumbnailURLfordb = null;
                        string strpBannerURLfordb = null;
                        string strpIamgeURLfordb = null;
                        string strpTempImageSave = null;

                        string tempfilePath = WebConfigurationManager.AppSettings["SiteImgPath"].ToString() + @"\" + "TempImages" + @"\";

                        string _SiteRoot = WebConfigurationManager.AppSettings["SiteImgPath"];
                        string _SiteURL = WebConfigurationManager.AppSettings["SiteImgURL"];


                        strpTempImageSave = ResizeImage.Download_Image(Url);

                        if (strpTempImageSave.Contains(".png"))
                        {
                            //---New Image path
                            string newFilePath = _SiteRoot + @"\" + "Artists" + @"\" + vArtists.ArtistID + @"\";
                            Helper.CreateDirectories(_SiteRoot + @"\" + "Artists" + @"\" + vArtists.ArtistID + @"\");

                            //Delete old files if any 
                            Helper.DeleteFiles(_SiteRoot + @"\" + "Artists" + @"\" + vArtists.ArtistID + @"\");

                            //Thumbnail image (Artist Listing)
                            string imageresizename = ResizeImage.Resize_Image_Thumb(tempfilePath + strpTempImageSave, newFilePath, "_T_" + strpTempImageSave, 400, 400);
                            strpThumbnailURLfordb = _SiteURL + "/Artists/" + vArtists.ArtistID + "/" + imageresizename;

                            //Scale up the image 
                            //string imageresizenametmp = ResizeImage.ScaleImage(tempfilePath + strpTempImageSave, tempfilePath, "_S_" + strpTempImageSave, 1430, 1430);
                            string imageresizenametmp = ResizeImage.ScaleImage(tempfilePath + strpTempImageSave, tempfilePath, "_S_" + strpTempImageSave, 650, 650);

                            //Banner Image (event listing)
                            //imageresizename = ResizeImage.CropImage(tempfilePath + imageresizenametmp, newFilePath, "_B_" + strpTempImageSave, 0, 100, 1428, 476);
                            imageresizename = ResizeImage.CropImage(tempfilePath + imageresizenametmp, newFilePath, "_B_" + strpTempImageSave, 0, 100, 640, 270);
                            strpBannerURLfordb = _SiteURL + "/Artists/" + vArtists.ArtistID + "/" + imageresizename;

                            //Image  (used in artist detail screen)
                            //imageresizename = ResizeImage.Resize_Image_Thumb(tempfilePath + imageresizenametmp, newFilePath, "_A_" + strpTempImageSave, 1428, 689);
                            //imageresizename = ResizeImage.CropImage(tempfilePath + imageresizenametmp, newFilePath, "_A_" + strpTempImageSave, 0, 0, 1428, 689);
                            imageresizename = ResizeImage.CropImage(tempfilePath + imageresizenametmp, newFilePath, "_A_" + strpTempImageSave, 0, 50, 640, 360);
                            strpIamgeURLfordb = _SiteURL + "/Artists/" + vArtists.ArtistID + "/" + imageresizename;


                            vArtists.ImageURL = strpIamgeURLfordb;
                            vArtists.ThumbnailURL = strpThumbnailURLfordb;
                            vArtists.BannerImage_URL = strpBannerURLfordb;
                        }
                    }
                }
                catch (Exception ex)
                {

                    LogHelper.CreateLog("ArtistProfilePicture_Asyn" + ex.Message);
                }
            });

            return true;
        }


        private async Task<bool> SpotifyProfilePicture_Asyn(Int32 vUserID, Artists vArtists, GenericRepository<Artists> _artistrepo, bool vNew, string Url)
        {
            await Task.Factory.StartNew(() =>
            {

                try
                {

                    string strpThumbnailURLfordb = null;
                    string strpBannerURLfordb = null;
                    string strpIamgeURLfordb = null;
                    string strpTempImageSave = null;

                    string tempfilePath = WebConfigurationManager.AppSettings["SiteImgPath"].ToString() + @"\" + "TempImages" + @"\";

                    string _SiteRoot = WebConfigurationManager.AppSettings["SiteImgPath"];
                    string _SiteURL = WebConfigurationManager.AppSettings["SiteImgURL"];


                    strpTempImageSave = ResizeImage.Download_Image(Url);

                    if (strpTempImageSave.Contains(".png"))
                    {
                        //---New Image path
                        string newFilePath = _SiteRoot + @"\" + "Artists" + @"\" + vArtists.ArtistID + @"\";
                        Helper.CreateDirectories(_SiteRoot + @"\" + "Artists" + @"\" + vArtists.ArtistID + @"\");

                        //Delete old files if any 
                        Helper.DeleteFiles(_SiteRoot + @"\" + "Artists" + @"\" + vArtists.ArtistID + @"\");

                        //Thumbnail image (Artist Listing)
                        string imageresizename = ResizeImage.Resize_Image_Thumb(tempfilePath + strpTempImageSave, newFilePath, "_T_" + strpTempImageSave, 400, 400);
                        strpThumbnailURLfordb = _SiteURL + "/Artists/" + vArtists.ArtistID + "/" + imageresizename;

                        //Scale up the image 
                        //string imageresizenametmp = ResizeImage.ScaleImage(tempfilePath + strpTempImageSave, tempfilePath, "_S_" + strpTempImageSave, 1430, 1430);
                        /*string imageresizenametmp = ResizeImage.ScaleImage(tempfilePath + strpTempImageSave, tempfilePath, "_S_" + strpTempImageSave, 650, 650);

                        //Banner Image (event listing)
                        //imageresizename = ResizeImage.CropImage(tempfilePath + imageresizenametmp, newFilePath, "_B_" + strpTempImageSave, 0, 100, 1428, 476);
                        imageresizename = ResizeImage.CropImage(tempfilePath + imageresizenametmp, newFilePath, "_B_" + strpTempImageSave, 0, 100, 640, 270);
                        strpBannerURLfordb = _SiteURL + "/Artists/" + vArtists.ArtistID + "/" + imageresizename;

                        //Image  (used in artist detail screen)
                        //imageresizename = ResizeImage.Resize_Image_Thumb(tempfilePath + imageresizenametmp, newFilePath, "_A_" + strpTempImageSave, 1428, 689);
                        //imageresizename = ResizeImage.CropImage(tempfilePath + imageresizenametmp, newFilePath, "_A_" + strpTempImageSave, 0, 0, 1428, 689);
                        imageresizename = ResizeImage.CropImage(tempfilePath + imageresizenametmp, newFilePath, "_A_" + strpTempImageSave, 0, 50, 640, 360);
                        strpIamgeURLfordb = _SiteURL + "/Artists/" + vArtists.ArtistID + "/" + imageresizename;
                        */

                        vArtists.ImageURL = Url;
                        vArtists.ThumbnailURL = strpThumbnailURLfordb;
                        vArtists.BannerImage_URL = Url;
                        _artistrepo.Repository.Update(vArtists);
                    }

                }
                catch (Exception ex)
                {

                    LogHelper.CreateLog("ArtistProfilePicture_Asyn" + ex.Message);
                }
            });

            return true;
        }

        #endregion

        #endregion

        #region ADs


        [Route("api/Musika/GetBannerList/{UserID}")]
        [HttpGet]
        public HttpResponseMessage GetBannerList(int UserID)
        {
            try
            {
                GenericRepository<Users> _UsersRepo = new GenericRepository<Users>(_unitOfWork);
                var _Users = _UsersRepo.Repository.Get(x => x.UserID == UserID);

                string CityName = "";
                if (_Users != null)
                {
                    string Lat = "";
                    if (_Users.DeviceLat != null)
                    {
                        Lat = Convert.ToString(_Users.DeviceLat);
                    }
                    string Log = "";
                    if (_Users.DeviceLat != null)
                    {
                        Log = Convert.ToString(_Users.DeviceLong);
                    }
                    CityName = ReverseGeoCode(Lat, Log);
                }
                GenericRepository<Ads> _AdsRepo = new GenericRepository<Ads>(_unitOfWork);
                var _AdsList = _AdsRepo.Repository.GetAll(t => t.City == CityName && t.Recordstatus == RecordStatus.Active.ToString()).Select(x => new { x.AdId, x.ImageURL, x.LinkURL });
                //var _AdsList = _AdsRepo.Repository.GetAll(t => t.City == "Lahore" && t.Recordstatus == RecordStatus.Active.ToString()).Select(x => new { x.AdId, x.ImageURL, x.LinkURL });

                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, _AdsList, "BannerList"));
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, ex.Message));
            }
        }
        public string ReverseGeoCode(string lat, string lng)
        {
            try
            {
                //&key=" + APIkey;
                //string APIKey = "AIzaSyDQTpXj82d8UpCi97wzo_nKXL7nYrd4G70";//ConfigurationManager.AppSettings["GoogleAppID"].ToString();
                string APIKey = "AIzaSyDCeTiDNn6CjjcBG1iOxOFCcHiwZET6iNc";
                string coordinate = lat + "," + lng;
                XmlDocument xDoc = new XmlDocument();
                xDoc.Load("https://maps.googleapis.com/maps/api/geocode/xml?latlng=" + coordinate + "&key=" + APIKey);

                XmlNodeList xNodelst = xDoc.GetElementsByTagName("result");
                XmlNode xNode = xNodelst.Item(0);
                string FullAddress = xNode.SelectSingleNode("formatted_address").InnerText;
                string city = "";

                try
                {
                    if (xNode.SelectSingleNode("address_component[1]/type").InnerText == "country")
                    {
                        city = xNode.SelectSingleNode("address_component[1]/long_name").InnerText;
                    }


                    if (xNode.SelectSingleNode("address_component[2]/type").InnerText == "country" && city == "")
                    {
                        city = xNode.SelectSingleNode("address_component[2]/long_name").InnerText;
                    }

                    if (xNode.SelectSingleNode("address_component[3]/type").InnerText == "country" && city == "")
                    {
                        city = xNode.SelectSingleNode("address_component[3]/long_name").InnerText;
                    }

                    if (xNode.SelectSingleNode("address_component[4]/type").InnerText == "country" && city == "")
                    {
                        city = xNode.SelectSingleNode("address_component[4]/long_name").InnerText;
                    }

                    if (xNode.SelectSingleNode("address_component[5]/type").InnerText == "country" && city == "")
                    {
                        city = xNode.SelectSingleNode("address_component[5]/long_name").InnerText;
                    }
                    if (xNode.SelectSingleNode("address_component[6]/type").InnerText == "country" && city == "")
                    {
                        city = xNode.SelectSingleNode("address_component[6]/long_name").InnerText;
                    }
                    if (xNode.SelectSingleNode("address_component[7]/type").InnerText == "country" && city == "")
                    {
                        city = xNode.SelectSingleNode("address_component[7]/long_name").InnerText;
                    }
                    if (xNode.SelectSingleNode("address_component[8]/type").InnerText == "country" && city == "")
                    {
                        city = xNode.SelectSingleNode("address_component[8]/long_name").InnerText;
                    }
                    if (xNode.SelectSingleNode("address_component[9]/type").InnerText == "country" && city == "")
                    {
                        city = xNode.SelectSingleNode("address_component[9]/long_name").InnerText;
                    }
                }
                catch
                {

                }

                //string code = xNode.SelectSingleNode("address_component[5]/short_name").InnerText;
                //string code1 = xNode.SelectSingleNode("address_component[5]/long_name").InnerText;


                //string Number = xNode.SelectSingleNode("address_component[1]/long_name").InnerText;
                //string Street = xNode.SelectSingleNode("address_component[2]/long_name").InnerText;
                //string Village = xNode.SelectSingleNode("address_component[3]/long_name").InnerText;
                //string Area = xNode.SelectSingleNode("address_component[4]/long_name").InnerText;
                //string County = xNode.SelectSingleNode("address_component[5]/long_name").InnerText;
                //string State = xNode.SelectSingleNode("address_component[6]/long_name").InnerText;
                //string Zip = xNode.SelectSingleNode("address_component[8]/long_name").InnerText;
                //string Country = xNode.SelectSingleNode("address_component[7]/long_name").InnerText;

                //dynamic res = new { FullAddress = FullAddress, ShortAddress = city + ", " + code }; ;

                return city;

            }
            catch (Exception ex)
            {
                return "";
                //throw;
            }
        }

        #endregion

        private void SetUserDevices(Users user)
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

        #region "Send Push Notification"

        public string SetMessageLang(string lang, DateTime? tourDate, string artistName, string venueName)
        {
            var message = "";
            if (tourDate.HasValue)
            {
                var dayOfTheWeek = tourDate.Value.DayOfWeek.ToString();
                var isTwoDays = tourDate.Value > DateTime.UtcNow.AddDays(1);

                if (isTwoDays)
                {
                    if (lang == EUserLanguage.EN.ToString())
                        message = NotificationResponse.TwoDayEnglish;
                    else
                        message = NotificationResponse.TwoDaySpanish;
                }
                else
                {
                    if (lang == EUserLanguage.EN.ToString())
                        message = NotificationResponse.OneDayEnglish;
                    else
                        message = NotificationResponse.OneDaySpanish;
                }
                if (!String.IsNullOrEmpty(message))
                {
                    if (message.Contains("[day_of_week]"))
                        message = message.Replace("[day_of_week]", dayOfTheWeek);

                    if (message.Contains("[artist_name]"))
                        message = message.Replace("[artist_name]", artistName);

                    if (message.Contains("[venue_name]"))
                        message = message.Replace("[venue_name]", venueName);
                }
            }
            return message;
        }


        [AllowAnonymous]
        [Route("api/Musika/SendNotification/{fromUserId}/{toUserId}/{message}/{tourDateId}/{artistName}/{venueName}")]
        [HttpGet]
        public HttpResponseMessage SendNotification(int fromUserId, int toUserId, string message, int tourDateId, string artistName, string venueName, string type)
        {
            try
            {
                GenericRepository<Users> _UsersRepo = new GenericRepository<Users>(_unitOfWork);
                GenericRepository<UserSettings> _UserSettingsRepo = new GenericRepository<UserSettings>(_unitOfWork);
                GenericRepository<UserDevices> _UserDevicesRepo = new GenericRepository<UserDevices>(_unitOfWork);
                GenericRepository<TourDate> _TourDateRepo = new GenericRepository<TourDate>(_unitOfWork);

                Users _UsersTo = null;
                UserSettings _UserSettings = null;
                TourDate _TourEntity = null;


                _UsersTo = _UsersRepo.Repository.Get(p => p.UserID == toUserId);

                //_UsersTo.UserLanguage
                _UserSettings = _UserSettingsRepo.Repository.Get(p => p.UserID == _UsersTo.UserID && p.SettingKey == EUserSettings.Musika.ToString());


                PushNotifications pNoty = new PushNotifications();


                var deviceList = _UserDevicesRepo.Repository.GetAll(x => x.UserId == toUserId);


                if (tourDateId > 0)
                    _TourEntity = _TourDateRepo.Repository.GetById(tourDateId);

                if (_UserSettings.SettingValue == false)
                {
                    LogHelper.CreateLog2(message + " - NOTIFICATION OFF - ToUserID : " + toUserId.ToString(), Musika.Library.Utilities.LogHelper.ErrorType.Notification);
                }


                //for multiple devices
                if (deviceList != null && deviceList.Count > 0 && _UserSettings.SettingValue == true && _UsersTo.RecordStatus == RecordStatus.Active.ToString() && _TourEntity != null)
                {
                    bool updateCount = false;



                    message = SetMessageLang(_UsersTo.UserLanguage, _TourEntity.Tour_Utcdate, artistName, venueName);

                    foreach (var d in deviceList)
                    {
                        if (string.IsNullOrEmpty(d.DeviceToken))
                        {
                            LogHelper.CreateLog2(message + " - Device Token Not found - ToUserID : " + toUserId.ToString(), Musika.Library.Utilities.LogHelper.ErrorType.Notification);
                        }

                        if (d.DeviceType == "IOS" && !string.IsNullOrEmpty(d.DeviceToken))
                        {
                            LogHelper.CreateLog2(message + " - IOS - ToUserID : " + toUserId.ToString(), Musika.Library.Utilities.LogHelper.ErrorType.Notification);

                            pNoty.SendNotification_IOS(d.DeviceToken, message, type);
                            //pNoty.SendNotification_IOS(d.DeviceToken, message, Numerics.GetInt(_UserSettings.NotificationCount + 1));
                            updateCount = true;
                        }
                        else if (d.DeviceType == "Android" && !string.IsNullOrEmpty(d.DeviceToken))
                        {
                            LogHelper.CreateLog2(message + " - ANDROID - ToUserID : " + toUserId.ToString(), Musika.Library.Utilities.LogHelper.ErrorType.Notification);

                            pNoty.SendNotification_Android(d.DeviceToken, message, type);
                            updateCount = true;
                        }
                    }
                    if (updateCount)
                    {
                        _UserSettings.NotificationCount += 1;
                        _UserSettingsRepo.Repository.Update(_UserSettings);
                    }
                }

                // for single device
                if (deviceList.Count == 0 && _UserSettings.SettingValue == true && _UsersTo.RecordStatus == RecordStatus.Active.ToString() && _TourEntity != null)
                {
                    bool updateCount = false;

                    message = SetMessageLang(_UsersTo.UserLanguage, _TourEntity.Tour_Utcdate, artistName, venueName);

                    if (string.IsNullOrEmpty(_UsersTo.DeviceToken))
                    {
                        LogHelper.CreateLog2(message + " - Device Token Not found - ToUserID : " + toUserId.ToString(), Musika.Library.Utilities.LogHelper.ErrorType.Notification);
                    }

                    if (_UsersTo.DeviceType == "IOS" && !string.IsNullOrEmpty(_UsersTo.DeviceToken))
                    {
                        LogHelper.CreateLog2(message + " - IOS - ToUserID : " + toUserId.ToString() + " devicetoken= " + _UsersTo.DeviceToken, Musika.Library.Utilities.LogHelper.ErrorType.Notification);

                        pNoty.SendNotification_IOS(_UsersTo.DeviceToken, message, type);
                        //pNoty.SendNotification_IOS(_UsersTo.DeviceToken, message, Numerics.GetInt(_UserSettings.NotificationCount + 1));
                        _UserSettings.NotificationCount += 1;
                        _UserSettingsRepo.Repository.Update(_UserSettings);
                    }
                    else if (_UsersTo.DeviceType == "Android" && !string.IsNullOrEmpty(_UsersTo.DeviceToken))
                    {
                        LogHelper.CreateLog2(message + " - ANDROID - ToUserID : " + toUserId.ToString(), Musika.Library.Utilities.LogHelper.ErrorType.Notification);

                        pNoty.SendNotification_Android(_UsersTo.DeviceToken, message, type);
                        _UserSettings.NotificationCount += 1;
                        _UserSettingsRepo.Repository.Update(_UserSettings);
                    }
                }

                #region "Commented Code"
                /*
                if (_UserSettings.SettingValue == true)
                {
                    if (_UsersTo.DeviceType == "IOS")
                    {
                        pNoty.SendNotification_IOS(_UsersTo.DeviceToken, message, Numerics.GetInt(_UserSettings.NotificationCount + 1));
                        _UserSettings.NotificationCount += 1;
                        _UserSettingsRepo.Repository.Update(_UserSettings);
                    }
                    else if (_UsersTo.DeviceType == "Android")
                    {
                        pNoty.SendNotification_Android(_UsersTo.DeviceToken, message);
                        _UserSettings.NotificationCount += 1;
                        _UserSettingsRepo.Repository.Update(_UserSettings);
                    }
                }
                */
                #endregion

                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, null));
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, ex.Message));
            }
        }

        #endregion

        #region "Function"

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

        public string CleanName(string name)
        {
            name = name.Replace(" ", "");
            string pattern = @"[^a-zA-Z0-9áéíñóúüÁÉÍÑÓÚÜ\s]";
            string _st = Regex.Replace(name, pattern, "");

            return Regex.Replace(name, pattern, "");
        }

        #endregion

    }
}