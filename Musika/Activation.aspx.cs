using Musika.Library.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Musika
{
    public partial class Activation : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            DataSet ds;
            if (!this.IsPostBack)
            {
                string activationCode = !string.IsNullOrEmpty(Request.QueryString["ActivationCode"]) ? Request.QueryString["ActivationCode"] : Guid.Empty.ToString();
                ds = new Musika.Repository.SPRepository.SpRepository().UpdateUserActivationStatus(activationCode);

                if (ds.Tables[0].Rows.Count > 0)
                {
                    DataRow dr = ds.Tables[0].Rows[0];
                    ltMessage.Text = "Activation successful.";

                    #region "Mail for Activation"
                    
                    #region "Mail To Admin"
                    // Mail To admin

                    string html  = string.Empty;

                    html = "<p>Hi Administrator," + "</p>";
                    html += "<p>A new user is added in Musika application." + "</p>";
                    html += "<p><br>The details of the user is as follows :" + "<p>";
                    html += "<p><br>User Name : " + dr["UserName"].ToString() + "<p>";
                    html += "<p><br><br><strong>Thanks,<br><br>The " + WebConfigurationManager.AppSettings["AppName"] + " Team</strong></p>";

                    SendEmailHelper.SendMail(System.Configuration.ConfigurationManager.AppSettings["ADMIN_EMAIL"].ToString(), "New Musika User Registration", html,"");
                    #endregion

                    //#region "Mail To Event Organizer"
                    //// Mail To admin

                    string AdminEmail = System.Configuration.ConfigurationManager.AppSettings["ADMIN_EMAIL"].ToString();

                    html = string.Empty;

                    html = "<p>Hi " + dr["Email"].ToString() + "," + "</p>";
                    html += "<p>Thanks for using " + WebConfigurationManager.AppSettings["AppName"] + "! </p>";
                    html += "<p>Thanks for Registering on Musika Application." + "</p>";
                    html += "<p><br>As per registration your details are as follows :" + "<p>";
                    html += "<p><br>User Name : " + dr["UserName"].ToString() + "<p>";
                    html += "<p><br><br><strong>Thanks,<br><br>The " + WebConfigurationManager.AppSettings["AppName"] + " Team</strong></p>";

                    //#region "Send Mail Implementation"
                    SendEmailHelper.SendMail(dr["Email"].ToString(), "New Musika User Registration", html,"");                    
                    //#endregion
                    #endregion
                }
                else
                {
                    ltMessage.Text = "Invalid Activation code.";
                }
                #region "Unused Code"
                //string constr = @"Data Source=23.111.138.246,2728; Initial Catalog=Musika;App=Musika; User ID=sa; Password=sdsol99!;";
                //string activationCode = !string.IsNullOrEmpty(Request.QueryString["ActivationCode"]) ? Request.QueryString["ActivationCode"] : Guid.Empty.ToString();
                //using (SqlConnection con = new SqlConnection(constr))
                //{
                //    using (SqlCommand cmd = new SqlCommand("DELETE FROM UserActivation WHERE ActivationCode = @ActivationCode"))
                //    {
                //        using (SqlDataAdapter sda = new SqlDataAdapter())
                //        {
                //            cmd.CommandType = CommandType.Text;
                //            cmd.Parameters.AddWithValue("@ActivationCode", activationCode);
                //            cmd.Connection = con;
                //            con.Open();
                //            int rowsAffected = cmd.ExecuteNonQuery();
                //            con.Close();

                //        }
                //    }
                //}
                #endregion
            }
        }
    }
}