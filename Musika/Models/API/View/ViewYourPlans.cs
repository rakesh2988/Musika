
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Musika.Models.API.View
{
    public class ViewYourPlans
    {
        public long UserID { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string ThumbnailURL { get; set; }
        public string ImageURL { get; set; }
        public List<ViewYourplanlst> Plans { get; set; }
    }

    public class ViewYourTicketPlans
    {
        public long UserID { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string ThumbnailURL { get; set; }
        public string ImageURL { get; set; }
        public List<ViewYourTicketPlanlst> Plans { get; set; }
        public List<ViewYourGoingplanlst> GoingPlan { get; set; }
    }
    

    public class ViewYourplanlst
    {
        public int TourDateID { get; set; }
        public Nullable<int> ArtistID { get; set; }
        public string Date_Local { get; set; }
        public DateTime Datetime_Local { get; set; }
        public string Time_Local { get; set; }
        public string ArtistName { get; set; }
        public string ImageURL { get; set; }
        public string BannerImage_URL { get; set; }
        public int VenueID { get; set; }
        public string VenueName { get; set; }
        public string Extended_Address { get; set; }
        public string Display_Location { get; set; }
        public string Slug { get; set; }
        public string Postal_Code { get; set; }
        public string Address { get; set; }
        public string Timezone { get; set; }
        public string VenueCity { get; set; }
        public string VenueState { get; set; }
        public string VenueCountry { get; set; }
        public Nullable<decimal> VenueLat { get; set; }
        public Nullable<decimal> VenueLong { get; set; }

        public string QRCodeImage { get; set; }
        public string TicketId { get; set; }
    }
    public class ViewYourGoingplanlst
    {
        public int TourDateID { get; set; }
        public Nullable<int> ArtistID { get; set; }
        public string Date_Local { get; set; }
        public DateTime Datetime_Local { get; set; }
        public string Time_Local { get; set; }
        public string ArtistName { get; set; }
        public string ImageURL { get; set; }
        public string BannerImage_URL { get; set; }
        public int VenueID { get; set; }
        public string VenueName { get; set; }
        public string Extended_Address { get; set; }
        public string Display_Location { get; set; }
        public string Slug { get; set; }
        public string Postal_Code { get; set; }
        public string Address { get; set; }
        public string Timezone { get; set; }
        public string VenueCity { get; set; }
        public string VenueState { get; set; }
        public string VenueCountry { get; set; }
        public Nullable<decimal> VenueLat { get; set; }
        public Nullable<decimal> VenueLong { get; set; }
    }

    public class ViewYourTicketPlanlst
    {
        public int TourDateID { get; set; }
        public Nullable<int> ArtistID { get; set; }
        public string Date_Local { get; set; }
        public DateTime Datetime_Local { get; set; }
        public string Time_Local { get; set; }
        public string ArtistName { get; set; }
        public string ImageURL { get; set; }
        public bool IsApproved { get; set; }
        public string BannerImage_URL { get; set; }
        public int VenueID { get; set; }
        public string VenueName { get; set; }
        public string Extended_Address { get; set; }
        public string Display_Location { get; set; }
        public string Slug { get; set; }
        public string Postal_Code { get; set; }
        public string Address { get; set; }
        public string Timezone { get; set; }
        public string VenueCity { get; set; }
        public string VenueState { get; set; }
        public string VenueCountry { get; set; }
        public Nullable<decimal> VenueLat { get; set; }
        public Nullable<decimal> VenueLong { get; set; }

        public List<Tickets> tickets { get; set; }
    }

    public class Tickets
    {
        public string QRCodeImage { get; set; }
        public string TicketId { get; set; }
    }
}