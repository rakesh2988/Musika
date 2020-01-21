using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;
using System.Text;
using System.Net;
using System.Net.Mail;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Net.Mime;
using System.IO;
using Newtonsoft.Json;
using System.Net.Http;

namespace WebApplication64
{
    #region "Bulk Email Functionality"
    public partial class WebForm1 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        #region "Fetch the data from the Database to send the mail"
        protected void king_Click(object sender, EventArgs e)
        {
            SqlCommand cmd = new SqlCommand();
            //SqlConnection cn = new SqlConnection(@"Data Source=54.70.35.64; Initial Catalog=Musika;App=Musika; User ID=sa; Password=sdsol99!;");
            SqlConnection cn = new SqlConnection(@"Data Source=staging2.sdsol.com; Initial Catalog=Musika;App=Musika; User ID=sa; Password=sdsol99!;");
            //SqlDataAdapter adpt = new SqlDataAdapter("Select * from tempEmails", cn);
            SqlDataAdapter adpt = new SqlDataAdapter("Select UserId,Email,UserName as Name,DeviceType from Users", cn);
            DataSet ds = new DataSet();
            adpt.Fill(ds);
            GridView1.DataSource = ds;
            GridView1.DataBind();
            Label2.Text = GridView1.Rows.Count.ToString();
            cn.Close();
        }
        #endregion

        protected void Timer1_Tick(object sender, EventArgs e)
        {
        }

        protected void Function1()
        {
            //string pattern = @"[^a-zA-Z0-9áéíñóúüÁÉÍÑÓÚÜ\s]";
            //search = Regex.Replace(search, pattern, "");
            //Then Check Music Graph API
            //Task<List<MusicGraph.Datum>> _Search_ByName = Task.Run(async () => await Spotify_SearchArtist(search));
            //_Search_ByName.Wait();

            //string _MusicGrapgh_api_key = ConfigurationManager.AppSettings["MusicGrapgh_api_key"].ToString();
            //string _result;

            //var httpWebRequest = (HttpWebRequest)WebRequest.Create("http://api.musicgraph.com/api/v2/artist/suggest?api_key=" + _MusicGrapgh_api_key + "&limit=5&prefix=" + search);
            //httpWebRequest.ContentType = "application/json";
            //httpWebRequest.Method = "GET";

            //var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            //using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            //{
            //    _result = streamReader.ReadToEnd();
            //}


            //// deserializing 
            //var _Search_ByName = JsonConvert.DeserializeObject<MusicGraph.Search_ByName>(_result);
        }

        protected void AddDataToDB(dynamic list)
        {

        }

            #region "Clean up the HTML Tags"
            private string PopulateBody()
        {
            string body = string.Empty;
            using (StreamReader reader = new StreamReader(Server.MapPath("~/Mail.html")))
            {
                body = reader.ReadToEnd();
            }
            body = body.Replace("<html>", "");
            body = body.Replace("<body>", "");
            body = body.Replace("<pre>", "");
            body = body.Replace("</html>", "");
            body = body.Replace("</body>", "");
            body = body.Replace("</pre>", "");
            return body;
        }
        #endregion

        #region "Send Mail to All Registered Users"
        protected void Sendbtn_Click(object sender, EventArgs e)
        {
            foreach (GridViewRow grow in GridView1.Rows)
            {
                SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
                client.UseDefaultCredentials = false;
                client.EnableSsl = true;
                
                client.Credentials = new NetworkCredential("developer@musikaapp.com", "Mus1ka@12");
                StringBuilder message = new StringBuilder();
                string file = Server.MapPath("~/Mail.html");
                string mailbody = System.IO.File.ReadAllText(file);
                string Emails = grow.Cells[1].Text.Trim();
                MailMessage mailMessage = new MailMessage();

                mailMessage.From = new MailAddress("developer@musikaapp.com","Musika LLC");
                mailMessage.To.Add(Emails);
                mailMessage.Body = PopulateBody();
                mailMessage.Subject = "Musika App - Update";

                mailMessage.BodyEncoding = System.Text.Encoding.UTF8;

                client.Send(mailMessage);
            }
        }
        #endregion
    }
    #endregion
}