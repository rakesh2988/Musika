using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Musika.Models
{
    public class ExportCsv
    {

        //public int TourID { get; set; }

        //public int ArtistID { get; set; }
        public string ArtistName { get; set; }
        public string About { get; set; }
        public string ImageURL { get; set; }

        public string BannerImage_URL { get; set; }

        // public bool OnTour { get; set; }
        public string Gender { get; set; }
        public string Main_Genre { get; set; }
        public string Decade { get; set; }
        public string Alternate_Names { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }

        public string Date_Local { get; set; }
        public string Time_Local { get; set; }

        // public string RecordStatus { get; set; }

        //public bool IsTracking { get; set; }

        // public string Instagram_Tag { get; set; }

        // public bool IsGoing { get; set; }

        public Int32 NoOfUserGoing { get; set; }

        public string VenueName { get; set; }
        public string VenueCity { get; set; }
        public string VenueState { get; set; }
        public string VenueCountry { get; set; }
        //  public List<viewEventTourPhoto> TourPhotos { get; set; }

        //  public List<viewEventRelated> ArtistRelated { get; set; }

        //  public ViewEventVenue Venue { get; set; }

        public List<ViewEventUsers> UsersGoing { get; set; }

        public string TicketURL { get; set; }

        // Added by Mukesh
        public string Event_Name { get; set; }
        public string Event_Description { get; set; }
        public string Organizer_Name { get; set; }
        public string Organizer_Description { get; set; }

        public List<Ticket> lstTicket { get; set; }


        public partial class ViewEventUsers
        {
             public int UserID { get; set; }

            public string GoingUserName { get; set; }

            public string Email { get; set; }

            // public string ThumbnailURL { get; set; }

            public string CreatedDate { get; set; }

        }
        public class Ticket
        {
            //public int EventId { get; set; }
            public string Currency { get; set; }
            public int CountryId { get; set; }
            public string RefundPolicy { get; set; }
            public List<TicketData> lstTicketData { get; set; }
        }
        public class TicketData
        {
            public int EventId { get; set; }
            public string TicketType { get; set; }
            public int Quantity { get; set; }
            public decimal Price { get; set; }
            public string TicketCategory { get; set; }
            public int BalanceTickets { get; set; }

            public string PackageStartDate { get; set; }
            public string PackageEndDate { get; set; }
            //public string Summary { get; set; }
        }
        public partial class ViewEventVenue
        {
            public int VenueID { get; set; }
            public string VenueName { get; set; }
            public string ImageURL { get; set; }
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

        public class viewEventTourPhoto
        {
            public int PhotoID { get; set; }
            public string HashTagName { get; set; }
            public string ImageThumbnailURL { get; set; }
            public string ImageURL { get; set; }
        }



        public partial class viewEventRelated
        {
            public string Musicgraph_ID { get; set; }
            public string RelatedArtistName { get; set; }
            public Nullable<decimal> Similarity { get; set; }
        }



        public class TicketBuyData
        {
            public int EventId { get; set; }
            public string UserName { get; set; }
            public string Email { get; set; }
            
            public string TicketType { get; set; }
            public string Mode { get; set; }

            public string Country { get; set; }
            public string Gender { get; set; }
            public string TicketCategory { get; set; }
        }
    }
}