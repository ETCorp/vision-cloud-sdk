using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using VisionCloud.API.Models;

namespace VisionCloud.API.Services
{
    public class DeviceManagement
    {
        private Token bearerToken;

        public DeviceManagement(Token token)
        {
            bearerToken = token;
        }

        public async Task<DeviceProperties> GetDeviceInfoAsync(string deviceName)
        {
            ConsoleHelper.WriteStep("Fetching device...");

            var client = new HttpClient();
            var result = new DeviceProperties();

            try
            {
                client.MaxResponseContentBufferSize = 999999;
                client.DefaultRequestHeaders.Add("Authorization", string.Format("Bearer {0}", bearerToken.access_token));

                var uri = $"{Constants.VisionApiUri}/api/device-management/get-device-info/{deviceName.Trim()}";

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

                HttpResponseMessage response = await client.GetAsync(uri);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    result = JsonConvert.DeserializeObject<DeviceProperties>(content);
                    ConsoleHelper.WriteStepResult("Received device information");
                }
                else
                {
                    var content = await response.Content.ReadAsStringAsync();
                    ConsoleHelper.WriteError(string.Format("Fetch failed {0}", content));
                }
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteException(ex);
                return null;
            }
            finally
            {
                client.Dispose();
            }

            return result;
        }
    }
}
