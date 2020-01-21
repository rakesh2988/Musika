using Musika.Library.Utilities;
using Musika.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Musika
{
    public partial class PaypalResponse : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            ClientScript.RegisterStartupScript(this.GetType(), "Popup", "ShowMessagePaid();", true);

            Response.Write("<script type='text/javascript'>function showAndroidToast(toast) { Android.showToast(toast); }</script>");
        }
    }
}