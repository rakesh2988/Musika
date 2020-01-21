using Newtonsoft.Json;

namespace SpotifyWebAPI.Web.Models
{
    public class CategoryList : BasicModel
    {
        [JsonProperty("categories")]
        public Paging<Category> Categories { get; set; }
    }
}