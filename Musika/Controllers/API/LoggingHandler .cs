using Musika.Library.Utilities;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Script.Serialization;
using static Musika.Library.Utilities.LogHelper;

namespace Musika.Controllers.API
{
    public class LoggingHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            //LogRequest(request);

            return base.SendAsync(request, cancellationToken).ContinueWith(task =>
            {


                var response = task.Result;

                LogResponse(response);

                return response;
            });

        }

        static string BytesToStringConverted(byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes))
            {
                using (var streamReader = new StreamReader(stream))
                {
                    return streamReader.ReadToEnd();
                }
            }
        }

        //public static byte[] DecompressGZIP(byte[] gzip)
        //{
        //    using (var stream = new Ionic.Zlib.DeflateStream(new MemoryStream(gzip), Ionic.Zlib.CompressionMode.Decompress))
        //    {
        //        var outStream = new MemoryStream();
        //        //const Int64 size = gzip.Length; //Playing around with various sizes didn't help

        //        byte[] buffer = new byte[gzip.Length];

        //        int read;
        //        while ((read = stream.Read(buffer, 0, gzip.Length)) > 0)
        //        {
        //            outStream.Write(buffer, 0, read);
        //            read = 0;
        //        }

        //        return outStream.ToArray();
        //    }
        //}

        static byte[] Decompress(byte[] gzip)
        {
            // Create a GZIP stream with decompression mode.
            // ... Then create a buffer and write into while reading from the GZIP stream.
            using (System.IO.Compression.GZipStream stream = new System.IO.Compression.GZipStream(new MemoryStream(gzip),
                System.IO.Compression.CompressionMode.Decompress))
            {
                const int size = 4096;
                byte[] buffer = new byte[gzip.Length];
                using (MemoryStream memory = new MemoryStream())
                {
                    int count = 0;
                    do
                    {
                        count = stream.Read(buffer, 0, gzip.Length);
                        if (count > 0)
                        {
                            memory.Write(buffer, 0, count);
                        }
                    }
                    while (count > 0);
                    return memory.ToArray();
                }
            }
        }



        private void LogRequest(HttpRequestMessage request)
        {
            if (Convert.ToBoolean(WebConfigurationManager.AppSettings["APILoggerRequest"].ToString()) == true)
            {
                try
                {
                    //HostingEnvironment.QueueBackgroundWorkItem(async cancellationToken =>
                    //{
                    if (!request.Content.IsMimeMultipartContent())
                    {
                        (request.Content ?? new StringContent("")).ReadAsStringAsync().ContinueWith(x =>
                        {
                            //debug.Info("{4:yyyy-MM-dd HH:mm:ss} {5} {0} request [{1}]{2} - {3}", request.GetCorrelationId(), request.Method, request.RequestUri, x.Result, DateTime.Now, Username(request));
                            var requestMessage = request.Content.ReadAsByteArrayAsync();

                            if (request.Method.ToString() == "GET")
                            {
                                LogHelper.CreateAPILog(""
                                                        + request.Method + " "
                                                        + request.GetCorrelationId() + " "
                                                        + " REQUEST: " + request.RequestUri
                                                        , " " + request.RequestUri.ToString()
                                                        , BytesToStringConverted(requestMessage.Result)
                                                        , ErrorType.APILog);
                            }
                            else
                            {
                                try
                                {
                                    LogHelper.CreateAPILog(""
                                                           + request.Method + " "
                                                           + request.GetCorrelationId() + " "
                                                           + " REQUEST: " + request.RequestUri
                                        , " " + request.RequestUri.ToString()
                                        , JObject.Parse(BytesToStringConverted(requestMessage.Result)).ToString()
                                        , ErrorType.APILog);
                                }
                                catch (Exception ex)
                                {
                                    var a = BytesToStringConverted(requestMessage.Result).ToString();
                                    var dict = HttpUtility.ParseQueryString(a);
                                    var json = new JavaScriptSerializer().Serialize(
                                                        dict.AllKeys.ToDictionary(k => k, k => dict[k])
                                               );

                                    LogHelper.CreateAPILog(""
                                                        + request.Method + " "
                                                        + request.GetCorrelationId() + " "
                                                        + " REQUEST: " + request.RequestUri
                                                        , request.RequestUri.ToString()
                                                        , JObject.Parse(json).ToString()
                                                        , ErrorType.APILog);
                                }

                            }
                        });
                    }
                    //});

                }
                catch (Exception ex)
                {
                    LogHelper.CreateLog(ex);
                }
            }
        }

        private void LogResponse(HttpResponseMessage response)
        {
            if (Convert.ToBoolean(WebConfigurationManager.AppSettings["APILoggerResponse"].ToString()) == true)
            {
                try
                {
                    //HostingEnvironment.QueueBackgroundWorkItem(async cancellationToken =>
                    //{
                    if (!response.Content.IsMimeMultipartContent())
                    {

                        var request = response.RequestMessage;
                        (response.Content ?? new StringContent("")).ReadAsStringAsync().ContinueWith(x =>
                        {
                            //  Logger.Info("{3:yyyy-MM-dd HH:mm:ss} {4} {0} response [{1}] - {2}", request.GetCorrelationId(), response.StatusCode, x.Result, DateTime.Now, Username(request));

                            dynamic responseMessage;

                            if (response.IsSuccessStatusCode)
                                responseMessage = response.Content.ReadAsByteArrayAsync();
                            else
                                responseMessage = Encoding.UTF8.GetBytes(response.ReasonPhrase);

                            //var AcceptEncoding = request.Headers.GetValues("Content-Check");

                            if (Convert.ToBoolean(WebConfigurationManager.AppSettings["APILogger"].ToString()) == true)
                            {

                                LogHelper.CreateAPILog(""
                                                      + request.Method + " "
                                                      + request.GetCorrelationId() + " "
                                                      + " **RESPONSE**: " + request.RequestUri
                                                      , " " + request.RequestUri.ToString()
                                                       , JObject.Parse(BytesToStringConverted(Decompress(responseMessage.Result))).ToString()
                                                      , ErrorType.APILog);
                            }
                            else
                            {
                                LogHelper.CreateAPILog(""
                                                 + request.Method + " "
                                                 + request.GetCorrelationId() + " "
                                                 + " **RESPONSE**: " + request.RequestUri
                                                 , " " + request.RequestUri.ToString()
                                                  , JObject.Parse(BytesToStringConverted(responseMessage.Result)).ToString()
                                                 , ErrorType.APILog);
                            }

                        });
                    }
                    // });
                }
                catch (Exception ex)
                {
                    LogHelper.CreateLog(ex);
                }
            }
        }

        private string Username(HttpRequestMessage request)
        {
            var values = new List<string>().AsEnumerable();
            if (request.Headers.TryGetValues("my-custom-header-for-current-user", out values) == false) return "<anonymous>";

            return values.First();
        }
    }
}