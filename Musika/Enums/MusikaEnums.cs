using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Musika.Enums
{
    public enum RecordStatus
    {
        Active,
        InActive,
        Deleted,
        Spotify,
        MusicGraph,
        Instagram,
        SeatGeek,
        Eventful,
        MusikaEvents
    }

    public enum ThirdPartyType
    {
        Facebook
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

    public enum EUserGoing
    {
        Going,
        NotGoing
    }

    public enum ETracking
    {
        Tracking,
        NotTracking
    }


    public enum EUserSettings
    {
        Musika
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
        public static string AcccountAlreadyExistsSignUp = "Account already exists but currently inactive.";
        public static string InactiveAccountSignIn = "Your account is currently inactive.";
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
}