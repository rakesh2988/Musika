using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using Musika.Library;
using Musika.Models;
using Musika.Repository;
using Musika.Repository.GRepository;
using Musika.Repository.Interface;
using Musika.Library.PushNotfication;
using Musika.Library.Utilities;

namespace NotificationService
{
    public enum EUserSettings
    {
        Musika
    }
    public enum RecordStatus
    {
        Active,
        InActive,
        Deleted,

        MusicGraph,
        Instagram,
        SeatGeek,
        Eventful
    }

    public enum EUserLanguage
    {
        EN,
        ES
    }

    public enum DeviceType
    {
        IOS,
        Android
    }
    public class NotificationResponse
    {
        public static string OneDayEnglish = "Enjoy your concert with [artist_name] at [venue_name] today!";
        public static string TwoDayEnglish = "You’ve got an upcoming concert with [artist_name] at [venue_name] this [day_of_week]!";
        public static string OneDaySpanish = "Disfruta hoy de tu concierto con [artist_name] en el [venue_name]!";
        public static string TwoDaySpanish = "Recuerda que el [day_of_week] tienes el concierto de [artista] en el [venue_name]!";
    }

    public class Response
    {
        public static string Reg_InvalUserName = "Invalid User Name Min 4 Max 100 characters allowed, Regex for field is ^[a-zA-Z0-9._-]";
        public static string InvalidArtistName = "Invalid Artist Name";
        public static string InvalidArtistID = "Invalid Artist ID";
        public static string ArtistNotFound = "Artist not found";
        public static string InvalidSearchCriteria = "Invalid Search Criteria";
        public static string EventNotDound = "Event not found";
        public static string VenueNotFound = "Venue not found";
        public static string UserRegistration = "User Registered Successfully";
        public static string ArtistRegistration = "New Artist has been Added";
        public static string ArtistUpdate = "Artist Details have been Updated";
        public static string EventNew = "New Event have been Added";
        public static string EventUpdate = "Event have been Updated";

        public static string PasswordOrEmail = "The password or email you entered is not correct. Please try again.";
        public static string UserNameAlreadyExists = "User name already exists";
        public static string Invalidemail = "Invalid email";
        public static string EmailAlreadyExists = "Email already exists";
        public static string UserSettingNotFound = "User setting not found";
        public static string UserNotFound = "User not found";
        public static string CurrentPasswordNotMatched = "Current password is not matched";
        public static string RecordNotFound = "Record Not Found";
        public static string Success = "Success";
        public static string Failure = "Failure";
        public static string Available = "Available";
    }


    public enum EMusicSource
    {
        Spotify,
        AppleMusic,
        Lastfm,
        Radio,
        Pandora,
        GooglePlayMusic,
        FacebookMusicLikes
    }


    public enum EventSearchType
    {
        Venue,
        Artist,
        Event
    }

    public partial class PushNotification : ServiceBase
    {
        private readonly IUnitOfWork _unitOfWork;
        public PushNotification()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            SendNotification();
        }

        protected override void OnStop()
        {
        }

        public string SetMessageLang(string lang, DateTime? tourDate)
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

                    //if (message.Contains("[artist_name]"))
                    //    message = message.Replace("[artist_name]", artistName);

                    //if (message.Contains("[venue_name]"))
                    //    message = message.Replace("[venue_name]", venueName);
                }
            }

            return message;
        }

        public static DateTime GetMonday(DateTime time)
        {
            if (time.DayOfWeek != DayOfWeek.Monday)
                return time.Subtract(new TimeSpan((int)time.DayOfWeek - 1, 0, 0, 0));

            return time;
        }

        public void SendNotification()
        {
            MusikaEntities db = new MusikaEntities();
            List<Users> lstUsers = new List<Users>();
            string type = string.Empty;
            string message = string.Empty;

            GenericRepository<Users> _UsersRepo = new GenericRepository<Users>(_unitOfWork);
            GenericRepository<Artists> _ArtistRepo = new GenericRepository<Artists>(_unitOfWork);
            GenericRepository<UserSettings> _UserSettingsRepo = new GenericRepository<UserSettings>(_unitOfWork);
            GenericRepository<UserDevices> _UserDevicesRepo = new GenericRepository<UserDevices>(_unitOfWork);
            GenericRepository<TourDate> _TourDateRepo = new GenericRepository<TourDate>(_unitOfWork);

            List<Artists> lstArtists = new List<Artists>();
            DateTime dt = GetMonday(DateTime.Now);
            lstArtists = _ArtistRepo.Repository.GetAll(u => u.RecordStatus == "Active").Where(t => t.CreatedDate >= dt).ToList();

            string artists = string.Empty;
            artists += "Newly Added Artists : ";
            foreach(Artists str in lstArtists)
            {
                artists += str.ArtistName + ",";
            }

            string events = string.Empty;
            List<TourDate> lstEvents = new List<TourDate>();
            lstEvents = _TourDateRepo.Repository.GetAll(u => u.RecordStatus == "Active").Where(t => t.CreatedDate >= dt).ToList();
            events += "Newly Added Events : ";
            foreach(TourDate str in lstEvents)
            {
                events += str.EventName + ",";
            }

            string finalMessage = string.Empty;
            finalMessage += artists + "\n" + events;

            //Users entity;
            lstUsers = _UsersRepo.Repository.GetAll(u => u.RecordStatus == "Active");
            if (lstUsers.Count > 0)
            {
                try
                {                    
                    Users _UsersTo = null;
                    UserSettings _UserSettings = null;
                    TourDate _TourEntity = null;                    

                    for (int i = 0; i < lstUsers.Count; i++)
                    {
                        _UsersTo = _UsersRepo.Repository.Get(p => p.UserID == lstUsers[i].UserID);

                        //_UsersTo.UserLanguage
                        _UserSettings = _UserSettingsRepo.Repository.Get(p => p.UserID == _UsersTo.UserID && p.SettingKey == EUserSettings.Musika.ToString());

                        type = lstUsers[i].DeviceType;

                        PushNotifications pNoty = new PushNotifications();
                        
                        var deviceList = _UserDevicesRepo.Repository.GetAll(x => x.UserId == lstUsers[i].UserID);
                                                
                        //for multiple devices
                        if (deviceList != null && deviceList.Count > 0 && _UserSettings.SettingValue == true && _UsersTo.RecordStatus == RecordStatus.Active.ToString() && _TourEntity != null)
                        {
                            bool updateCount = false;

                            message = SetMessageLang(_UsersTo.UserLanguage, _TourEntity.Tour_Utcdate);

                            foreach (var d in deviceList)
                            {
                                if (string.IsNullOrEmpty(d.DeviceToken))
                                {
                                    LogHelper.CreateLog2(message + " - Device Token Not found - ToUserID : " + lstUsers[i].UserID.ToString(), Musika.Library.Utilities.LogHelper.ErrorType.Notification);
                                }

                                if (d.DeviceType == "IOS" && !string.IsNullOrEmpty(d.DeviceToken))
                                {
                                    LogHelper.CreateLog2(message + " - IOS - ToUserID : " + lstUsers[i].UserID.ToString(), Musika.Library.Utilities.LogHelper.ErrorType.Notification);

                                    pNoty.SendNotification_IOS(d.DeviceToken, finalMessage, type);
                                    updateCount = true;
                                }
                                else if (d.DeviceType == "Android" && !string.IsNullOrEmpty(d.DeviceToken))
                                {
                                    LogHelper.CreateLog2(message + " - ANDROID - ToUserID : " + lstUsers[i].UserID.ToString(), Musika.Library.Utilities.LogHelper.ErrorType.Notification);
                                    pNoty.SendNotification_Android(d.DeviceToken, finalMessage, type);
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
                            message = SetMessageLang(_UsersTo.UserLanguage, _TourEntity.Tour_Utcdate);

                            if (string.IsNullOrEmpty(_UsersTo.DeviceToken))
                            {
                                LogHelper.CreateLog2(message + " - Device Token Not found - ToUserID : " + lstUsers[i].UserID.ToString(), Musika.Library.Utilities.LogHelper.ErrorType.Notification);
                            }

                            if (_UsersTo.DeviceType == "IOS" && !string.IsNullOrEmpty(_UsersTo.DeviceToken))
                            {
                                LogHelper.CreateLog2(message + " - IOS - ToUserID : " + lstUsers[i].UserID.ToString() + " devicetoken= " + _UsersTo.DeviceToken, Musika.Library.Utilities.LogHelper.ErrorType.Notification);

                                pNoty.SendNotification_IOS(_UsersTo.DeviceToken, message, type);
                                _UserSettings.NotificationCount += 1;
                                _UserSettingsRepo.Repository.Update(_UserSettings);
                            }
                            else if (_UsersTo.DeviceType == "Android" && !string.IsNullOrEmpty(_UsersTo.DeviceToken))
                            {
                                LogHelper.CreateLog2(message + " - ANDROID - ToUserID : " + lstUsers[i].UserID.ToString(), Musika.Library.Utilities.LogHelper.ErrorType.Notification);

                                pNoty.SendNotification_Android(_UsersTo.DeviceToken, message, type);
                                _UserSettings.NotificationCount += 1;
                                _UserSettingsRepo.Repository.Update(_UserSettings);
                            }
                        }                        
                    }
                }
                catch (Exception ex)
                {
                    
                }
            }
        }
    }
}
