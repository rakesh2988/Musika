﻿using Newtonsoft.Json;
using SpotifyWebAPI.Web.Enums;
using SpotifyWebAPI.Web.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SpotifyWebAPI
{
    public class ClientCredentialsAuth
    {
        public Scope Scope { get; set; }
        public String ClientId { get; set; }
        public String ClientSecret { get; set; }

        /// <summary>
        ///     Starts the auth process and
        /// </summary>
        /// <returns>A new Token</returns>
        public Token DoAuth()
        {
            using (WebClient wc = new WebClient())
            {
                wc.Proxy = null;
                wc.Headers.Add("Authorization",
                    "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(ClientId + ":" + ClientSecret)));

                wc.Headers.Add("Content-Type",
                   "application/x-www-form-urlencoded");

                NameValueCollection col = new NameValueCollection
                {
                    {"grant_type", "client_credentials"},
                    {"scope", Scope.GetStringAttribute(" ")}
                };

                byte[] data;
                try
                {
                    data = wc.UploadValues("https://accounts.spotify.com/api/token", "POST", col);
                }
                catch (WebException e)
                {
                    using (StreamReader reader = new StreamReader(e.Response.GetResponseStream()))
                    {
                        data = Encoding.UTF8.GetBytes(reader.ReadToEnd());
                    }
                }
                return JsonConvert.DeserializeObject<Token>(Encoding.UTF8.GetString(data));
            }
        }
    }
}