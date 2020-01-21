using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http.Filters;

namespace Musika.Controllers.API
{
    public class DeflateCompressionAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(HttpActionExecutedContext actContext)
        {
            var content = actContext.Response.Content;

            var bytes = content == null ? null : content.ReadAsByteArrayAsync().Result;
            var zlibbedContent = bytes == null ? new byte[0] :
            CompressionHelper.GZipByte(bytes);
            actContext.Response.Content = new ByteArrayContent(zlibbedContent);
            actContext.Response.Content.Headers.Remove("Content-Type");

            if (content != null)
            {
                foreach (var httpContentHeader in content.Headers)
                {
                    actContext.Response.Content.Headers.Add(httpContentHeader.Key, httpContentHeader.Value);
                }
            }


            actContext.Response.Content.Headers.Add("Content-encoding", "gzip");

            base.OnActionExecuted(actContext);

            //var AcceptEncoding = actContext.Request.Headers.GetValues("Accept-Encoding");


            //***************************

            //Copy from (2nd option)
            //if ((AcceptEncoding.Contains("gzip")))
            //{
            //HttpResponse response = HttpContext.Current.Response;
            //response.Filter = new System.IO.Compression.GZipStream(response.Filter, System.IO.Compression.CompressionLevel.Optimal);
            //response.Headers.Remove("Content-Encoding");
            //response.AppendHeader("Content-Encoding", "gzip");

            //base.OnActionExecuted(actContext);

            //}
            //else if (AcceptEncoding.Contains("deflate"))
            //{
            //    HttpResponse response = HttpContext.Current.Response;
            //    response.Filter = new System.IO.Compression.DeflateStream(response.Filter, System.IO.Compression.CompressionLevel.Optimal);
            //    response.Headers.Remove("Content-Encoding");
            //    response.AppendHeader("Content-Encoding", "deflate");
            //}

        }

    }


    // DotNetZip library .This library can easily be downloaded from NuGet.

    public class CompressionHelper
    {
        //public static byte[] DeflateByte(byte[] str)
        //{
        //    if (str == null)
        //    {
        //        return null;
        //    }

        //    using (var output = new MemoryStream())
        //    {
        //        using (
        //            var compressor = new Ionic.Zlib.DeflateStream(
        //            output, Ionic.Zlib.CompressionMode.Compress,
        //            Ionic.Zlib.CompressionLevel.BestSpeed))
        //        {
        //            compressor.Write(str, 0, str.Length);
        //        }

        //        return output.ToArray();
        //    }
        //}


        public static byte[] GZipByte(byte[] str)
        {
            if (str == null)
            {
                return null;
            }

            using (var output = new MemoryStream())
            {
                using (
                    var compressor = new System.IO.Compression.GZipStream(output, System.IO.Compression.CompressionLevel.Optimal))
                {
                    compressor.Write(str, 0, str.Length);
                }

                return output.ToArray();
            }
        }

    }

}