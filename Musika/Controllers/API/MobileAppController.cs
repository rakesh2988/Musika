using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Spire;
using Spire.Barcode;

namespace Musika.Controllers.API
{
    public class MobileAppController : ApiController
    {
        [Route("api/v2/MobileApp/GenerateQRCode/{content}")]
        [HttpGet]
        public string GenerateQRCode(string content)
        {
            Image QRbarcode;
            BarcodeSettings.ApplyKey("DAAA84F486628A6DE4160D9E7465D733");
            BarcodeSettings settings = new BarcodeSettings();
            settings.Type = BarCodeType.QRCode;
            settings.Unit = GraphicsUnit.Pixel;
            settings.ShowText = false;
            settings.ResolutionType = ResolutionType.UseDpi;
            //input data
            settings.Data = content;
            
            //set fore color
            settings.ForeColor = Color.FromName("Black");
            
            //set back color
            settings.BackColor = Color.FromName("White");
            
            //set x
            settings.X = 10;

            //set left margin
            settings.LeftMargin = 1;

            //set right margin
            settings.RightMargin = 1;

            //set top margin
            settings.TopMargin = 1;

            //set bottom margin
            settings.BottomMargin = 1;

            //set correction level
            settings.QRCodeECL = QRCodeECL.L;
            
            //generate QR code
            BarCodeGenerator generator = new BarCodeGenerator(settings);
            QRbarcode = generator.GenerateImage();
            //return QRbarcode;
            return ImageToBase64(QRbarcode);
        }

        public string ImageToBase64(Image img)
        {
            string base64String = null;
            //Image image;
            //string path = img;
            //using (System.Drawing.Image image = System.Drawing.Image.FromFile(img))
            //{
                using (MemoryStream m = new MemoryStream())
                {
                    //image.Save(m, img.RawFormat);
                    byte[] bytes = (byte[])(new ImageConverter()).ConvertTo(img, typeof(byte[]));
                    //byte[] imageBytes = img.ToArray();

                    base64String = Convert.ToBase64String(bytes);
                    return base64String;
                }
            //}
        }
    }
}
