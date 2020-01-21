using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Musika.Models
{
    public class TicketingUserModel
    {
        public int UserID { get; set; }
        public string UserType { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Addres { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string PhoneNumber { get; set; }
        public string FacebookID { get; set; }
        public string ThumbnailURL { get; set; }
        public string ImageURL { get; set; }
        public string DeviceType { get; set; }
        public string DeviceToken { get; set; }
        public Nullable<decimal> DeviceLat { get; set; }
        public Nullable<decimal> DeviceLong { get; set; }
        public string RecordStatus { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string SynFacebookID { get; set; }
        public string UserLanguage { get; set; }
        public string Country { get; set; }
        public string status { get; set; }
        public EventReponse MessageResponse { get; set; }
    }

    public class EventReponse
    {
        public int ResponseId { get; set; }
        public string ReturnMessage { get; set; }
    }
}