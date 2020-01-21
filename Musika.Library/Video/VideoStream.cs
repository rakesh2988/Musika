using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Musika.Library.Video
{
    public class VideoStream
    {

        //To USE


        //public class VideosController : ApiController
        //{
        //    public HttpResponseMessage Get(string filename, string ext)
        //    {
        //        var video = new VideoStream(filename, ext);

        //        var response = Request.CreateResponse();
        //        response.Content = new PushStreamContent(video.WriteToStream, new MediaTypeHeaderValue("video/" + ext));

        //        return response;
        //    }
        //}



        //        <html>
        //<body>
        //    <video width="480" height="320" controls="controls" autoplay="autoplay">
        //        <source src="/api/videos/webm/CkY96QuiteBitterBeings" type="video/webm">
        //    </video>
        //</body>
        //</html>


        private readonly string _filename;

        public VideoStream(string filename, string ext)
        {
            _filename = @"C:\UsersFilipDownloads\" + filename + "." + ext;
        }

        public async void WriteToStream(Stream outputStream, HttpContent content, TransportContext context)
        {
            try
            {
                var buffer = new byte[65536];

                using (var video = File.Open(_filename, FileMode.Open, FileAccess.Read))
                {
                    var length = (int)video.Length;
                    var bytesRead = 1;

                    while (length > 0 && bytesRead > 0)
                    {
                        bytesRead = video.Read(buffer, 0, Math.Min(length, buffer.Length));
                        await outputStream.WriteAsync(buffer, 0, bytesRead);
                        length -= bytesRead;
                    }
                }
            }
            catch (HttpException ex)
            {
                return;
            }
            finally
            {
                outputStream.Close();
            }
        }
    }
}
