using Musika.Models;
using Musika.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
//using System.Linq;
using Spire.Barcode.WebUI;
using System.Web.Mvc;
using System.Drawing;

namespace Musika.Areas.Admin.Controllers
{
    public class UsersController : Controller
    {
        // GET: Admin/Users

        private readonly IUnitOfWork _unitOfWork;
        SqlConnection con;

        string sqlconn;
        public ActionResult Index()
        {
            return View();
        }
        private void connection()
        {
            sqlconn = ConfigurationManager.ConnectionStrings["MusikaEntitiesADO"].ConnectionString;
            con = new SqlConnection(sqlconn);

        }
        [HttpPost]
        public JsonResult AddNewAdsWithImage()
        {
            try
            {
                var filePath = "";
                // HttpResponseMessage result = null;
                var httpRequest = Request;
                if (httpRequest.Files.Count > 0)
                {
                    var docfiles = new List<string>();
                    foreach (string av in httpRequest.Files)
                    {
                        var postedFile = httpRequest.Files[av];
                        
                        filePath = Server.MapPath("~/Content/Upload/BulkTickets" + postedFile.FileName);
                        postedFile.SaveAs(filePath);
                        docfiles.Add(filePath);
                    }
                    //result = Request.CreateResponse(HttpStatusCode.Created, docfiles);
                }
                else
                {
                    //result = Request.CreateResponse(HttpStatusCode.BadRequest);
                }
                
                //Creating object of datatable  
                DataTable tblcsv = new DataTable();
                //creating columns  
                tblcsv.Columns.Add("Barcode");
                tblcsv.Columns.Add("Nombre");
                tblcsv.Columns.Add("Seccion");
                tblcsv.Columns.Add("Fila");
                tblcsv.Columns.Add("Asiento");
                tblcsv.Columns.Add("Precio");
                tblcsv.Columns.Add("Orden");
                tblcsv.Columns.Add("TicketStatus");
                tblcsv.Columns.Add("CreatedOn");
                string ReadCSV = System.IO.File.ReadAllText(filePath);
                //spliting row after new line  
                foreach (string csvRow in ReadCSV.Split('\n'))
                {

                    if (!string.IsNullOrEmpty(csvRow))
                    {
                        //Adding each row into datatable  
                        tblcsv.Rows.Add();
                        int count = 0;
                        foreach (string FileRec in csvRow.Split(','))
                        {
                            // GenerateBacode(FileRec.Replace("\"", "'"), "");
                            tblcsv.Rows[tblcsv.Rows.Count - 1][count] = FileRec.Replace("\"", "").Replace("''", "");
                            if (count == 6)
                            {
                                tblcsv.Rows[tblcsv.Rows.Count - 1][7] = false;
                                tblcsv.Rows[tblcsv.Rows.Count - 1][8] = DateTime.Now;
                            }
                            count++;
                        }
                    }
                }
                InsertCSVRecords(tblcsv);
                string MessageError = "";
                MessageError = "success";
                return Json(MessageError, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json("error", JsonRequestBehavior.AllowGet);
            }
        }
        private void InsertCSVRecords(DataTable csvdt)
        {
            connection();
            //creating object of SqlBulkCopy    
            SqlBulkCopy objbulk = new SqlBulkCopy(con);
            //assigning Destination table name    
            objbulk.DestinationTableName = "ImportBulkTicket";
            //Mapping Table column    
            objbulk.ColumnMappings.Add("Name", "Nombre");
            objbulk.ColumnMappings.Add("Section", "Seccion");
            objbulk.ColumnMappings.Add("Row", "Fila");
            objbulk.ColumnMappings.Add("Seat", "Asiento");
            objbulk.ColumnMappings.Add("Price", "Precio");
            objbulk.ColumnMappings.Add("order", "Orden");
            objbulk.ColumnMappings.Add("BarCode", "BarCode");
            objbulk.ColumnMappings.Add("TicketStatus", "TicketStatus");
            objbulk.ColumnMappings.Add("CreatedOn", "CreatedOn");
            //inserting Datatable Records to DataBase    
            con.Open();
            objbulk.WriteToServer(csvdt);
            con.Close();


        }
        //private void GenerateBacode(string _data, string _filename)
        //{
        //    string barCode = _data;
        //    System.Web.UI.WebControls.Image imgBarCode = new System.Web.UI.WebControls.Image();
        //    using (Bitmap bitMap = new Bitmap(barCode.Length * 20, 80))
        //    {
        //        using (Graphics graphics = Graphics.FromImage(bitMap))
        //        {
        //            Font oFont = new Font("IDAutomationHC39M", 16);
        //            PointF point = new PointF(2f, 2f);
        //            SolidBrush blackBrush = new SolidBrush(Color.Black);
        //            SolidBrush whiteBrush = new SolidBrush(Color.White);
        //            graphics.FillRectangle(whiteBrush, 0, 0, bitMap.Width, bitMap.Height);
        //            graphics.DrawString("*" + barCode + "*", oFont, blackBrush, point);
        //        }
        //        using (MemoryStream ms = new MemoryStream())
        //        {
        //            bitMap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
        //            byte[] byteImage = ms.ToArray();

        //            Convert.ToBase64String(byteImage);
        //            imgBarCode.ImageUrl = "data:image/png;base64," + Convert.ToBase64String(byteImage);
        //        }
        //        //PlaceHolder1.Controls.Add(imgBarCode);
        //    }
        //}
        //Function to Insert Records  

        //public bool InsertNewAds(string ImageURL, string LinkURL, string City)
        //{
        //    bool result = false;
        //    try
        //    {
        //        Ads objAds = new Ads();
        //        objAds.ImageURL = ImageURL;
        //        objAds.ThumbnailURL = "";
        //        objAds.LinkURL = LinkURL;
        //        objAds.CreatedDate = DateTime.UtcNow;
        //        objAds.City = City;
        //        objAds.Recordstatus = Musika.Enums.RecordStatus.Active.ToString();
        //        using (var db = new MusikaEntities())
        //        {
        //            db.Ads.Add(objAds);
        //            db.SaveChanges();
        //        }

        //        result = true;
        //    }
        //    catch (Exception)
        //    {
        //        result = false;
        //        throw;
        //    }
        //    return result;
        //}
    }
}