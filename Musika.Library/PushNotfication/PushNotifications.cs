using Newtonsoft.Json;
//using PushSharp;
//using PushSharp.Apple;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Musika.Models;


namespace Musika.Library.PushNotfication
{
    public class PushNotifications
    {
        public const string Success = "Success";
        public const string success = "success";
        public const string Failure = "Failure";
        public const string NoAccountExists = "NoAccountExists";
        protected string SetStringValue(string str)
        {
            return str.ToLower() == "null" ? string.Empty : str;
        }

        #region "Old Unused Code"
        //public string SendNotification_IOS(string DeviceToken, string _Message, int BatchCount)
        //{
        //    try
        //    {
        //        var push = new PushBroker();
        //        string p12File = ConfigurationManager.AppSettings["PushNotification"];
        //        var appleCert = File.ReadAllBytes(p12File);
        //        bool APNSProduction = Convert.ToBoolean(Convert.ToInt32(ConfigurationManager.AppSettings["APNSProduction"]));
        //        push.RegisterAppleService(new ApplePushChannelSettings(APNSProduction, appleCert, "",true));
        //        push.QueueNotification(new AppleNotification()
        //                                .ForDeviceToken(DeviceToken)
        //                                .WithAlert(_Message)
        //                                .WithBadge(BatchCount)
        //                                .WithSound("default"));

        //        push.OnChannelCreated += push_OnChannelCreated;
        //        push.OnChannelDestroyed += push_OnChannelDestroyed;
        //        push.OnChannelException += push_OnChannelException;
        //        push.OnDeviceSubscriptionChanged += push_OnDeviceSubscriptionChanged;
        //        push.OnDeviceSubscriptionExpired += push_OnDeviceSubscriptionExpired;
        //        push.OnNotificationFailed += push_OnNotificationFailed;
        //        //push.OnNotificationRequeue += push_OnNotificationRequeue;
        //        push.OnNotificationSent += push_OnNotificationSent;
        //        push.OnServiceException += push_OnServiceException;
        //        return "success";
        //    }
        //    catch (Exception ex)
        //    {
        //        return ex.ToString();
        //    }
        //}
        #endregion

        #region "New Code for iOS Notification"
        public string SendNotification_IOS(string DeviceToken, string _Message, string NotificationType)
        {
            String sResponseFromServer = String.Empty;
            try
            {
                if (!String.IsNullOrEmpty(DeviceToken))
                {
                    WebRequest tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send");
                    tRequest.Method = "post";
                    string GoogleAppID = ConfigurationManager.AppSettings["GoogleAppIDIos"];
                    tRequest.Headers.Add(string.Format("Authorization: key={0}", GoogleAppID));
                    tRequest.Headers.Add(string.Format("Sender: id={0}", ConfigurationManager.AppSettings["SENDER_ID"]));
                    tRequest.ContentType = "application/json";

                    var payload = new
                    {
                        to = DeviceToken,
                        priority = "high",
                        //content_available = true,
                        //type = NotificationType,
                        notification = new
                        {
                            body = _Message,
                            title = "Musika App",
                            //badge = 1,
                            //type = NotificationType
                        },
                        data = new
                        {
                            //type = NotificationType
                        }
                    };

                    string postbody = JsonConvert.SerializeObject(payload).ToString();
                    Byte[] byteArray = Encoding.UTF8.GetBytes(postbody);
                    tRequest.ContentLength = byteArray.Length;
                    
                    using (Stream dataStream = tRequest.GetRequestStream())
                    {
                        dataStream.Write(byteArray, 0, byteArray.Length);
                        using (WebResponse tResponse = tRequest.GetResponse())
                        {
                            using (Stream dataStreamResponse = tResponse.GetResponseStream())
                            {
                                if (dataStreamResponse != null) using (StreamReader tReader = new StreamReader(dataStreamResponse))
                                {
                                    sResponseFromServer = tReader.ReadToEnd();
                                }
                            }
                        }
                    }
                }
                return sResponseFromServer;                
            }
            catch (Exception ex)
            {
                return ex.Message + "\n" + ex.StackTrace;
            }
        }
        #endregion

        public string SendNotification_Android(string DeviceToken, string DeviceMessage, string NotificationType)
        {
            #region "New Code"
            String sResponseFromServer = String.Empty;
            try
            {
                if (!String.IsNullOrEmpty(DeviceToken))
                {
                    WebRequest tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send");
                    tRequest.Method = "post";
                    string GoogleAppID = ConfigurationManager.AppSettings["GoogleAppID"];
                    tRequest.Headers.Add(string.Format("Authorization: key={0}", GoogleAppID));
                    tRequest.Headers.Add(string.Format("Sender: id={0}", ConfigurationManager.AppSettings["SENDER_ID"]));
                    tRequest.ContentType = "application/json";

                    DeviceMessage = "{\"message\": \"" + DeviceMessage + "\",\"title\": \"Musica App\", \"type\" : \"" + NotificationType + "\" }";

                    var payload = new
                    {
                        to = DeviceToken,
                        priority = "high",
                        content_available = true,
                        data = new
                        {
                            body = DeviceMessage,
                            title = "Test",
                            badge = 1
                        },
                    };

                    string postbody = JsonConvert.SerializeObject(payload).ToString();
                    Byte[] byteArray = Encoding.UTF8.GetBytes(postbody);
                    tRequest.ContentLength = byteArray.Length;

                    using (Stream dataStream = tRequest.GetRequestStream())
                    {
                        dataStream.Write(byteArray, 0, byteArray.Length);
                        using (WebResponse tResponse = tRequest.GetResponse())
                        {
                            using (Stream dataStreamResponse = tResponse.GetResponseStream())
                            {
                                if (dataStreamResponse != null) using (StreamReader tReader = new StreamReader(dataStreamResponse))
                                {
                                    sResponseFromServer = tReader.ReadToEnd();
                                }
                            }
                        }
                    }
                }
                return sResponseFromServer;
            }
            catch (Exception ex)
            {
                return ex.Message + "\n" + ex.StackTrace;
            }

            #endregion

            #region "Old Unused Code"
            #region "Unused Code"
            //try
            //{
            //    var value = DeviceMessage;
            //    WebRequest tRequest;
            //    string GoogleAppID = ConfigurationManager.AppSettings["GoogleAppID"];
            //    string SENDER_ID = ConfigurationManager.AppSettings["SENDER_ID"];
            //    //tRequest = WebRequest.Create("https://android.googleapis.com/gcm/send");
            //    tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send");
            //    tRequest.Method = "post";
            //    tRequest.ContentType = "application/x-www-form-urlencoded;charset=UTF-8";
            //    tRequest.Headers.Add(string.Format("Authorization: key={0}", GoogleAppID));

            //    tRequest.Headers.Add(string.Format("Sender: id={0}", SENDER_ID));

            //    string postedJson = "{\"Message\":\"" + DeviceMessage + "\"}";
            //    string postData = "time_to_live=108&delay_while_idle=1&data.message=" + postedJson + "&registration_id=" + DeviceToken;
            //    Console.WriteLine(postData);
            //    Byte[] byteArray = Encoding.UTF8.GetBytes(postData);
            //    tRequest.ContentLength = byteArray.Length;

            //    Stream dataStream = tRequest.GetRequestStream();
            //    dataStream.Write(byteArray, 0, byteArray.Length);
            //    dataStream.Close();

            //    WebResponse tResponse = tRequest.GetResponse();
            //    dataStream = tResponse.GetResponseStream();
            //    StreamReader tReader = new StreamReader(dataStream);
            //    String sResponseFromServer = tReader.ReadToEnd();

            //    tReader.Close();
            //    dataStream.Close();
            //    tResponse.Close();
            //    return "success";
            //}
            //catch (Exception ex)
            //{
            //    return ex.Message;
            //}
            #endregion
            //try
            //{
            //    var webAddr = "https://fcm.googleapis.com/fcm/send";
            //    string GoogleAppID = "APA91bF2Ehy0pP2LjfVZWMEkaJ-7W0PfxAAcieNDXJL_DIgsLgIVQVw3UFUEmo0JaRInGdoi35OUmyI8gKGk7XKv7oM5o6dl-rMldWGNK9yUzZ-bFZ-spHw";
            //    string senderId = "356123364162";

            //    var result = "-1";
            //    var httpWebRequest = (HttpWebRequest)WebRequest.Create(webAddr);
            //    httpWebRequest.ContentType = "application/json";
            //    httpWebRequest.Headers.Add(string.Format("Authorization: key={0}", GoogleAppID));
            //    httpWebRequest.Headers.Add(string.Format("Sender: id={0}", senderId));
            //    httpWebRequest.Method = "POST";

            //    var payload = new
            //    {
            //        to = DeviceToken,
            //        priority = "high",
            //        content_available = true,
            //        notification = new
            //        {
            //            body = DeviceMessage
            //        },
            //    };
            //    var serializer = new JavaScriptSerializer();
            //    using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            //    {
            //        string json = serializer.Serialize(payload);
            //        streamWriter.Write(json);
            //        streamWriter.Flush();
            //    }

            //    var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            //    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            //    {
            //        result = streamReader.ReadToEnd();
            //    }
            //    return result;
            //}
            //catch (Exception ex)
            //{
            //    return ex.ToString();
            //}

            //********************************************
            //    //var result = "-1";
            //    //var webAddr = "https://fcm.googleapis.com/fcm/send";
            //    //string GoogleAppID = ConfigurationManager.AppSettings["GoogleAppID"];
            //    ////string SENDER_ID = ConfigurationManager.AppSettings["SENDER_ID"];
            //    ////string SENDER_ID = "695898364276";

            //    //var httpWebRequest = (HttpWebRequest)WebRequest.Create(webAddr);
            //    //httpWebRequest.ContentType = "application/json";
            //    //httpWebRequest.Headers.Add("Authorization:key=" + GoogleAppID);
            //    ////httpWebRequest.Headers.Add(string.Format("Sender: id={0}", SENDER_ID));
            //    //httpWebRequest.Method = "POST";

            //    //using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            //    //{
            //    //    string json = "{\"to\": \"" + DeviceToken + "\",\"data\": {\"message\": \"" + DeviceMessage + "\",}}";
            //    //    streamWriter.Write(json);
            //    //    streamWriter.Flush();
            //    //}

            //    //var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            //    //using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            //    //{
            //    //    result = streamReader.ReadToEnd();
            //    //}

            //    //return "success";
            //    var result = "-1";
            //    var webAddr = "https://fcm.googleapis.com/fcm/send";
            //    string GoogleAppID = "AIzaSyBjYaIp0KC2AKq4T_Eg5pn_3nyPRtVH0Qg";
            //    string SENDER_ID = "356123364162";

            //    var httpWebRequest = (HttpWebRequest)WebRequest.Create(webAddr);
            //    httpWebRequest.ContentType = "application/json";
            //    httpWebRequest.Headers.Add("Authorization:key=" + GoogleAppID);
            //    httpWebRequest.Headers.Add(string.Format("Sender: id={0}", SENDER_ID));
            //    httpWebRequest.Method = "POST";

            //    using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            //    {
            //        //string json = "{\"to\": \"/topics/news\",\"data\": {\"message\": \"This is a Firebase Cloud Messaging Topic Message!\",}}";
            //        string json = "{\"to\": \"" + DeviceToken + "\",\"data\": {\"message\": \"" + DeviceMessage + "\",}}";

            //        streamWriter.Write(json);
            //        streamWriter.Flush();
            //    }

            //    var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            //    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            //    {
            //        result = streamReader.ReadToEnd();
            //    }

            //    return result;
            //}
            //catch (Exception ex)
            //{
            //    return ex.ToString();
            //}

            #region "Unused Code"
            //try
            //{
            //    var result = "-1";
            //    string GoogleAppID = ConfigurationManager.AppSettings["GoogleAppID"];
            //    string SENDER_ID = ConfigurationManager.AppSettings["SENDER_ID"];

            //    WebRequest tRequest;
            //    tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send");

            //    tRequest.Method = "post";
            //    tRequest.ContentType = "application/json";
            //    tRequest.Headers.Add(string.Format("Authorization: key={0}", GoogleAppID));
            //    tRequest.Headers.Add(string.Format("Sender: id={0}", SENDER_ID));

            //    using (var streamWriter = new StreamWriter(tRequest.GetRequestStream()))
            //    {
            //        string strNJson = @"{
            //        ""to"": ""/topics/ServiceNow"",
            //        ""data"": {
            //            ""ShortDesc"": ""DeviceMessage"",
            //            ""IncidentNo"": ""any number"",
            //            ""Description"": ""detail desc""
            //        },
            //        ""notification"": {
            //            ""title"": ""ServiceNow: Incident No. number"",
            //            ""text"": ""This is Notification"",
            //            ""sound"":""default""
            //            }
            //        }";
            //        streamWriter.Write(strNJson);
            //        streamWriter.Flush();
            //    }

            //    var httpResponse = (HttpWebResponse)tRequest.GetResponse();
            //    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            //    {
            //        result = streamReader.ReadToEnd();
            //    }


            //    return "success";
            //}
            //catch (Exception ex)
            //{
            //    return ex.ToString();
            //}
            #endregion


            #region "Old Code"
            //try
            //{
            //    string GoogleAppID = ConfigurationManager.AppSettings["GoogleAppID"];
            //    //string SENDER_ID = ConfigurationManager.AppSettings["SENDER_ID"];

            //    string SENDER_ID = "356123364162";

            //    WebRequest tRequest;
            //    tRequest = WebRequest.Create("https://android.googleapis.com/gcm/send");
            //    //tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send");

            //    tRequest.Method = "post";
            //    tRequest.ContentType = "application/x-www-form-urlencoded;charset=UTF-8";
            //    tRequest.Headers.Add(string.Format("Authorization: key={0}", GoogleAppID));
            //    tRequest.Headers.Add(string.Format("Sender: id={0}", SENDER_ID));

            //    string postedJson = "{\"Message\":\"" + DeviceMessage + "\"}";
            //    string postData = "time_to_live=108&delay_while_idle=1&data.message=" + postedJson + "&registration_id=" + DeviceToken;
            //    Console.WriteLine(postData);
            //    Byte[] byteArray = Encoding.UTF8.GetBytes(postData);
            //    tRequest.ContentLength = byteArray.Length;

            //    Stream dataStream = tRequest.GetRequestStream();
            //    dataStream.Write(byteArray, 0, byteArray.Length);
            //    dataStream.Close();

            //    WebResponse tResponse = tRequest.GetResponse();
            //    dataStream = tResponse.GetResponseStream();
            //    StreamReader tReader = new StreamReader(dataStream);
            //    String sResponseFromServer = tReader.ReadToEnd();

            //    tReader.Close();
            //    dataStream.Close();
            //    tResponse.Close();

            //    return "success";
            //}
            //catch (Exception ex)
            //{
            //    return ex.ToString();
            //}
            #endregion
            #endregion
        }

        //void push_OnDeviceSubscriptionExpired(object sender, string expiredSubscriptionId, DateTime expirationDateUtc, PushSharp.Core.INotification notification)
        //{
        //    //throw new NotImplementedException();
        //}

        //void push_OnDeviceSubscriptionChanged(object sender, string oldSubscriptionId, string newSubscriptionId, PushSharp.Core.INotification notification)
        //{
        //    //throw new NotImplementedException();
        //}

        //void push_OnChannelException(object sender, PushSharp.Core.IPushChannel pushChannel, Exception error)
        //{
        //    //throw new NotImplementedException();
        //}

        void push_OnChannelDestroyed(object sender)
        {
            //throw new NotImplementedException();
        }

        #region "Commented Code"
        //void push_OnChannelCreated(object sender, PushSharp.Core.IPushChannel pushChannel)
        //{
        //    //throw new NotImplementedException();
        //}
        #endregion

        void push_OnServiceException(object sender, Exception error)
        {
            //throw new NotImplementedException();
        }

        #region "Commented Code"
        //void push_OnNotificationSent(object sender, PushSharp.Core.INotification notification)
        //{
        //    //throw new NotImplementedException();
        //}

        //void push_OnNotificationFailed(object sender, PushSharp.Core.INotification notification, Exception error)
        //{
        //    //throw new NotImplementedException();
        //}
        #endregion
    }
}