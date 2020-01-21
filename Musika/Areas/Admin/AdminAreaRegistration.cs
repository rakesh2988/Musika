using System.Web.Mvc;

namespace Musika.Areas.Admin
{
    public class AdminAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Admin";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Admin_default",
                "Admin/{controller}/{action}/{id}/{search}/{search2}",
                new { action = "Index", id = UrlParameter.Optional, search = "", search2 =""}
            );
        }
    }
}