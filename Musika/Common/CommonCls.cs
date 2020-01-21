using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using Musika.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Web;

namespace Musika.Common
{
    public class CommonCls
    {
        const float spacingBetweenCells = 7;
        public MemoryStream PDFGenerate(string message, List<TicketQRCodeDetail> image)
        {

            using (MemoryStream stream = new System.IO.MemoryStream())
            {
                try
                {
                    StringReader sr = new StringReader(message);
                    Document pdfDoc = new Document(iTextSharp.text.PageSize.A4, 10f, 10f, 10f, 0f);
                    PdfWriter writer = PdfWriter.GetInstance(pdfDoc, stream);
                    
                    pdfDoc.Open();
                    XMLWorkerHelper.GetInstance().ParseXHtml(writer, pdfDoc, sr);
                    pdfDoc.Close();
                    return stream;
                }

                catch (Exception ex)
                {

                }
                return stream;
            }



        }
        private float ToPoints(float millimeters)
        {
            // converts millimeters to points
            return Utilities.MillimetersToPoints(millimeters);
        }
        public string Get8Digits()
        {
            var bytes = new byte[4];
            var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            uint random = BitConverter.ToUInt32(bytes, 0) % 100000000;
            return String.Format("{0:D8}", random);
        }
    }
}