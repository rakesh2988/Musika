using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Musika
{
    public partial class paypal : System.Web.UI.Page
    {
        protected string cmd = "_xclick";
        protected string business = ConfigurationManager.AppSettings["BusinessEmail"];
        protected string item_name = "";
        protected string amount;
        protected string user_id;
        protected string return_url = ConfigurationManager.AppSettings["ReturnUrl"];
        protected string notify_url = ConfigurationManager.AppSettings["NotifyUrl"];
        protected string cancel_url = ConfigurationManager.AppSettings["CancelPurchaseUrl"];
        protected string currency_code = ConfigurationManager.AppSettings["CurrencyCode"];
        protected string no_shipping = "2"; //2 to show shiping adress
        protected string item_number;
        protected string URL;
        protected string request_id;
        protected string rm;
        protected string address_city;
        protected string address_state;
        protected string address_zip;
        int userId;
        int tourDateId;
        string deviceId;
        string ticketType;
        decimal price;
        int qty;
        string package;
        string currency;
        string personalData;string lang = "es";

        protected void Page_Load(object sender, EventArgs e)
        {
            userId = Convert.ToInt32(Request.QueryString["userId"].ToString());
            tourDateId = Convert.ToInt32(Request.QueryString["tourDateId"].ToString());
            deviceId = Convert.ToString(Request.QueryString["deviceId"].ToString());
            ticketType = Convert.ToString(Request.QueryString["ticketType"].ToString());
            price = Convert.ToDecimal(Request.QueryString["Price"], System.Globalization.CultureInfo.InvariantCulture);
            qty = Convert.ToInt32(Request.QueryString["qty"].ToString());
            package = Convert.ToString(Request.QueryString["package"].ToString());
            currency = Convert.ToString(Request.QueryString["currency"].ToString());
            personalData = Convert.ToString(Request.QueryString["data"].ToString());
            lang= Convert.ToString(Request.QueryString["lang"].ToString());
            if (ConfigurationManager.AppSettings["UseSandbox"].ToString() == "true")
                URL = "https://www.sandbox.paypal.com/cgi-bin/webscr";
            else
                URL = "https://www.paypal.com/cgi-bin/webscr";

            if (ConfigurationManager.AppSettings["SendToReturnURL"].ToString() == "true")
                rm = "2";
            else
                rm = "1";

            amount = price.ToString();
            item_name = package.ToString();
            request_id = tourDateId.ToString()+ "," + ticketType + "," + personalData.ToString() + "," +deviceId.ToString() + ","+ qty + "," + lang.ToString();
            item_number = userId.ToString();
            business = ConfigurationManager.AppSettings["BusinessEmail"];
            return_url = ConfigurationManager.AppSettings["ReturnUrl"];
            notify_url = ConfigurationManager.AppSettings["NotifyUrl"];
            cancel_url = ConfigurationManager.AppSettings["CancelPurchaseUrl"];
            currency_code = currency.ToString();
        }
    }
}