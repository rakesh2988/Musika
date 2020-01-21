using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Musika.Models.API.View
{
    public class ViewArtist
    {
        public int ArtistID { get; set; }
        public string ArtistName { get; set; }
        public string About { get; set; }
        public string ImageURL { get; set; }


        public string BannerImage_URL { get; set; }

        public bool OnTour { get; set; }
        public string Gender { get; set; }
        public string Main_Genre { get; set; }
        public string Decade { get; set; }
        public string Alternate_Names { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public string RecordStatus { get; set; }

        public string Spotify_URL { get; set; }
        public string Spotify_URL_Name { get; set; }

        public string Spotify_Follow { get; set; }

        public string Instagram_Tag { get; set; }


        
        public bool IsTracking { get; set; }

        public List<viewTour> TourDates { get; set; }

        public List<viewTourPhoto> TourPhotos { get; set; }

        public List<viewRelated> ArtistRelated { get; set; }
        

    }

    public class viewTourPhoto
    {
        public int PhotoID { get; set; }
        public string HashTagName { get; set; }
        public string ImageThumbnailURL { get; set; }
        public string ImageURL { get; set; }
    }

    public partial class viewTour
    {
        public int TourDateID { get; set; }
        public int VenueID { get; set; }
        public string Announce_Date { get; set; }
        public string Tour_Utcdate { get; set; }
        public string Datetime_Local { get; set; }

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

    public partial class viewRelated
    {
        public string Musicgraph_ID { get; set; }
        public string RelatedArtistName { get; set; }
        public Nullable<decimal> Similarity { get; set; }
    }
}