using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using VisionCloud.API.Models;

namespace VisionCloud.API.Services
{
    public static class LoginService
    {
        public static Token GetToken(string userName, string password)
        {
            var pairs = new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string, string>( "grant_type", "password" ),
                        new KeyValuePair<string, string>( "username", userName ),
                        new KeyValuePair<string, string> ( "Password", password )
                    };
            var request = new HttpRequestMessage(HttpMethod.Post, "token");
            request.Content = new FormUrlEncodedContent(pairs);

            using (var client = new HttpClient())
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                var response = client.PostAsync($"{Constants.VisionApiUri}/Token", new FormUrlEncodedContent(pairs)).Result;
                var resStr = response.Content.ReadAsStringAsync().Result;
                var token = new Token();
                if (response.IsSuccessStatusCode)
                {
                    token = JsonConvert.DeserializeObject<Token>(resStr);
                    ConsoleHelper.WriteSuccess("Logged into Vision Device Management");
                    return token;
                }
                else
                {
                    ConsoleHelper.WriteError("Login failed");
                    return null;
                }
            }
        }
    }
}
