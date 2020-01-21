using PayPal.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Musika.Models
{
    public static class PaypalConfiguration
    {
        //Variables for storing the clientID and clientSecret key  
        public readonly static string ClientId;
        public readonly static string ClientSecret;
        //Constructor  
        static PaypalConfiguration()
        {
            var config = new Dictionary<string, string>();
            config.Add("mode", "sandbox");
            config.Add("clientId", "ARVWFcARPu_OEU3NKM3wJ0lyN4fNsLGsuGjTEofoPamvrsoTMx5qKLilrs_2dqIlE0ONObqcxiQ_TB78");
            config.Add("clientSecret", "EF75hok2kTEmZXcbFaqa7B8pvclNdmCabTFIQCkl99TEA0LfnEkstOmpsiOqqbIfcR3VhpZR8x-kCy4N");
            //var config = GetConfig();
            //ClientId = config["ARVWFcARPu_OEU3NKM3wJ0lyN4fNsLGsuGjTEofoPamvrsoTMx5qKLilrs_2dqIlE0ONObqcxiQ_TB78"];
            //ClientSecret = config["EF75hok2kTEmZXcbFaqa7B8pvclNdmCabTFIQCkl99TEA0LfnEkstOmpsiOqqbIfcR3VhpZR8x-kCy4N"];
        }
        // getting properties from the web.config  
        public static Dictionary<string, string> GetConfig()
        {
            return PayPal.Api.ConfigManager.Instance.GetProperties();
        }
        private static string GetAccessToken()
        {
            // getting accesstocken from paypal  
            string accessToken = new OAuthTokenCredential(ClientId, ClientSecret, GetConfig()).GetAccessToken();
            return accessToken;
        }
        public static APIContext GetAPIContext()
        {
            // return apicontext object by invoking it with the accesstoken  
            APIContext apiContext = new APIContext(GetAccessToken());
            apiContext.Config = GetConfig();
            return apiContext;
        }
    }
}