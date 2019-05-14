using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using VisionCloud.API.Models;
using static VisionCloud.API.Models.LwM2mApiModels;

namespace VisionCloud.API.Services
{
    public class AppManagement
    {
        private Token bearerToken;

        public AppManagement(Token token)
        {
            bearerToken = token;
        }

        public async Task<Tuple<AppDefinition, Blob>> GetDataForPeriod(string endpoint, DateTime from, bool FileNamesRequired = false, DateTime? to = null)
        {
            var uri = $"{Constants.VisionApiUri}/api/app-data/data-for-period/{endpoint.Trim()}";

            var builder = new UriBuilder(uri);
            var query = HttpUtility.ParseQueryString(builder.Query);
            query["from"] = from.ToString();
            query["to"] = to.ToString();
            query["FileNamesRequired"] = FileNamesRequired.ToString();

            string queryString = query.ToString();
            string url = builder.ToString();
            var client = new HttpClient();
            //client.MaxResponseContentBufferSize = 999999;
            var result = new Blob();

            try
            {
                client.DefaultRequestHeaders.Add("Authorization", string.Format("Bearer {0}", bearerToken.access_token));
                HttpResponseMessage response = await client.GetAsync(url + "?" + queryString);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    result = JsonConvert.DeserializeObject<Blob>(content);
                }
                else
                {
                    ConsoleHelper.WriteFailure("Could not retrieve app data. Please try again. If issue persits, conatact your system admin");
                }
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteException(ex);
            }
            finally
            {
                client.Dispose();
            }

            if (result?.FileNames?.Count > 0)
            {
                AppDefinition def = await GetLwm2mModelsForClient(endpoint);
                if (def == null)
                {
                    ConsoleHelper.WriteFailure("Could not retrieve app definition");
                }

                if (!def.lwm2mModel.Any())
                {
                    ConsoleHelper.WriteFailure($"No LWM2M Models retrieved for endpoint {endpoint}. Ensure host device is available/connected.");
                }

                ConsoleHelper.WriteSuccess(string.Format("Total files to process for period: {0}", result?.FileNames?.Count));
                return new Tuple<AppDefinition, Blob>(def, result);
            }
            else
            {
                ConsoleHelper.WriteWarning("No files found for period.");
            }

            return null;
        }

        public async Task<AppDefinition> GetLwm2mModelsForClient(string endpoint)
        {
            var uri = $"{Constants.VisionApiUri}/api/app-management/get-client-models/{endpoint.Trim()}";

            var client = new HttpClient();
            var appDefinition = new AppDefinition();

            try
            {
                client.MaxResponseContentBufferSize = 999999;
                client.DefaultRequestHeaders.Add("Authorization", string.Format("Bearer {0}", bearerToken.access_token));

                HttpResponseMessage response = await client.GetAsync(uri);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    appDefinition = JsonConvert.DeserializeObject<AppDefinition>(content);
                }
                else
                {
                    ConsoleHelper.WriteFailure("Could not get models: " + response.ToString());
                }
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteException("Error retrieving models", ex);
                throw;
            }
            finally
            {
                client.Dispose();
            }
            return appDefinition;
        }

        public List<Lwm2mModel> MapDataToModels(AppDefinition deviceInfo, List<Dictionary<string, dynamic>> allData)
        {
            try
            {
                var dataToReturn = new List<Lwm2mModel>();
                foreach (var json in allData)
                {
                    var data = Json.Decode(json.Values.FirstOrDefault());
                    var actualData = data.Data;
                    var objId = Convert.ToInt32(data.ObjectId);
                    var model = deviceInfo.lwm2mModel.Where(x => x.id == objId).FirstOrDefault();
                    var modelToPush = new Lwm2mModel();
                    modelToPush.id = model.id;
                    modelToPush.name = model.name;
                    modelToPush.instancetype = model.instancetype;
                    modelToPush.instance = model.instance;
                    modelToPush.mandatory = model.mandatory;
                    modelToPush.description = model.description;
                    modelToPush.serverCreationTime = json.Keys.FirstOrDefault().Substring(0, json.Keys.FirstOrDefault().IndexOf("."));
                    modelToPush.resourcedefs = new List<ResourceDefnitions>();
                    if (actualData != null)
                    {
                        var checkIfInstance = actualData.resources;

                        // determine if instance 
                        var ObjectObserve = actualData.instances;
                        if (ObjectObserve != null && ObjectObserve is DynamicJsonArray)
                        {
                            foreach (var instance in ObjectObserve)
                            {
                                var instanceID = instance.id;
                                var resources = instance.resources;
                                foreach (var item in resources)
                                {
                                    var resourceID = Convert.ToInt32(item.id);
                                    var resDef = model.resourcedefs.Where(x => x.id == resourceID).FirstOrDefault();
                                    resDef.value = Convert.ToString(item.value);
                                    modelToPush.resourcedefs.Add(resDef);

                                }
                            }
                            dataToReturn.Add(modelToPush);
                        }
                        else
                        {  // determine if resource observe or instance observe data 
                            if (checkIfInstance != null && checkIfInstance is DynamicJsonArray)
                            {
                                var instance = Convert.ToInt32(actualData.id);
                                foreach (var item in checkIfInstance)
                                {
                                    var resourceID = Convert.ToInt32(item.id);
                                    var resDef = model.resourcedefs.Where(x => x.id == resourceID).FirstOrDefault();
                                    resDef.value = Convert.ToString(item.value);
                                    modelToPush.resourcedefs.Add(resDef);
                                }
                                dataToReturn.Add(modelToPush);
                            }
                            else
                            {
                                // its resource observe data
                                var resourceID = Convert.ToInt32(data.Data.id);
                                var resDef = model.resourcedefs.Where(x => x.id == resourceID).FirstOrDefault();
                                resDef.value = Convert.ToString(data.Data.value);
                                modelToPush.resourcedefs.Add(resDef);
                                dataToReturn.Add(modelToPush);
                            }
                        }
                    }
                }
                return dataToReturn;
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteException("Error mapping models", ex);
                return null;
            }
        }

        /// <param name="endpoint">The ID of the application connected to LWM2M Server</param>
        /// <param name="objectId">Object ID on which Read Operation needs to be Performed</param>
        /// <param name="instanceId">Instance ID on which Read Operation needs to be performed</param>
        /// <param name="resourceId">Resource ID on which Read Operation needs to be performed. Resource ID 
        /// is nullable and optional. In case of no resource ID is provided, read will happen at Instance Level</param>
        public async Task<object> PerformOperationAsync(string type, string endpoint, int objectId, int instanceId, int? resourceId = null)
        {
            var url = ComposeUri(type, endpoint, objectId, instanceId, resourceId);
            return url != null ? await SendRequestToDevice(url, type, resourceId != null) : null;
        }

        public async Task<object> SendRequestToDevice(string url, string type, bool resourceLevel)
        {
            var client = new HttpClient();
            client.MaxResponseContentBufferSize = 999999;
            try
            {
                client.DefaultRequestHeaders.Add("Authorization", string.Format("Bearer {0}", bearerToken.access_token));

                HttpResponseMessage response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var str = await response.Content.ReadAsStringAsync();

                    if(type == "read" || type == "1")
                    {
                        if (resourceLevel)
                        {
                            var readResource = JsonConvert.DeserializeObject<Lwm2mServerReadResourceModel>(str);
                            ConsoleHelper.WriteObjectInfo(readResource);
                            return readResource;
                        }
                        else
                        {
                            var readInstance = JsonConvert.DeserializeObject<Lwm2mServerReadInstanceModel>(str);
                            ConsoleHelper.WriteObjectInfo(readInstance);
                            return readInstance;
                        }
                    }
                    else if(type == "observe" || type == "4")
                    {
                        if (resourceLevel)
                        {
                            var observeResource = JsonConvert.DeserializeObject<InitialObserveResourceResponse>(str);
                            ConsoleHelper.WriteObjectInfo(observeResource);
                            return observeResource;
                        }
                        else
                        {
                            var observeInstance = JsonConvert.DeserializeObject<InitialObserveInstanceResponse>(str);
                            ConsoleHelper.WriteObjectInfo(observeInstance);
                            return observeInstance;
                        }
                    }
                    else
                    {
                        var serverResponse = JsonConvert.DeserializeObject<ServerResponse>(str);
                        if(serverResponse != null)
                        {
                            ConsoleHelper.WriteObjectInfo(serverResponse);
                        }
                        return serverResponse;
                    }
                }
                else
                {
                    ConsoleHelper.WriteError("Could not send. Please try again. If issue persits, conatact your system admin");
                    return null;
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
        }

        public async Task<ServerResponse> WriteLWM2MResource(string url, List<Resource> payload)
        {
            var client = new HttpClient();
            client.MaxResponseContentBufferSize = 999999;
            try
            {
                var json = JsonConvert.SerializeObject(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                client.DefaultRequestHeaders.Add("Authorization", string.Format("Bearer {0}", bearerToken.access_token));

                HttpResponseMessage response = client.PutAsync(url, content).Result;
                if (response.IsSuccessStatusCode)
                {
                    var str = await response.Content.ReadAsStringAsync();
                    var res = JsonConvert.DeserializeObject<ServerResponse>(str);
                    ConsoleHelper.WriteObjectInfo(res);
                    return res;
                }
                else
                {
                    ConsoleHelper.WriteError("Could not write resource. Please try again. If issue persits, conatact your system admin");
                    return null;
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
        }

        public async Task PostAttributes(string url, Lwm2mServerAttributesModel model)
        {
            var client = new HttpClient();
            client.MaxResponseContentBufferSize = 999999;
            try
            {
                var json = JsonConvert.SerializeObject(model);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                client.DefaultRequestHeaders.Add("Authorization", string.Format("Bearer {0}", bearerToken.access_token));

                HttpResponseMessage response = client.PostAsync(url, content).Result;
                if (response.IsSuccessStatusCode)
                {
                    ConsoleHelper.WriteSuccess("Attributes posted");
                    var str = await response.Content.ReadAsStringAsync();
                    var res = JsonConvert.DeserializeObject<ServerResponse>(str);
                    ConsoleHelper.WriteObjectInfo(res);
                }
                else
                {
                    ConsoleHelper.WriteError("Could not write attributes. Please try again. If issue persits, conatact your system admin");
                }
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteException(ex);
            }
            finally
            {
                client.Dispose();
            }
        }

        public string ComposeUri(string type ,string endpoint, int objectId, int instanceId, int? resourceId = null)
        {
            endpoint = endpoint.Trim();

            switch (type)
            {
                case "1":
                case "read": 
                    var readUri = $"{Constants.VisionApiUri}/api/app-management/read/{endpoint}/{objectId}/{instanceId}";

                    if (resourceId != null) {

                        var query = HttpUtility.ParseQueryString(string.Empty);
                        query["resourceId"] = string.Format("{0}", resourceId.Value);
                     
                        string queryString = query.ToString();
                        readUri = readUri + "/?" + queryString;
                    }
                    return readUri;
                case "2":
                case "write":
                    var writeUri = $"{Constants.VisionApiUri}/api/app-management/write/{endpoint}/{objectId}/{instanceId}";

                    if (resourceId != null)
                    {

                        var query = HttpUtility.ParseQueryString(string.Empty);
                        query["resourceId"] = string.Format("{0}", resourceId.Value);

                        string queryString = query.ToString();
                        writeUri = writeUri + "/?" + queryString;
                    }
                    return writeUri;
                case "3":
                case "execute":
                    var executeUri = $"{Constants.VisionApiUri}/api/app-management/execute/{endpoint}/{objectId}/{instanceId}";

                    if (resourceId != null)
                    {

                        var query = HttpUtility.ParseQueryString(string.Empty);
                        query["resourceId"] = string.Format("{0}", resourceId.Value);

                        string queryString = query.ToString();
                        executeUri = executeUri + "/?" + queryString;
                    }
                    return executeUri;
                case "4":
                case "observe":
                    var observeUri = $"{Constants.VisionApiUri}/api/app-management/observe/{endpoint}/{objectId}/{instanceId}";

                    if (resourceId != null)
                    {

                        var query = HttpUtility.ParseQueryString(string.Empty);
                        query["resourceId"] = string.Format("{0}", resourceId.Value);

                        string queryString = query.ToString();
                        observeUri = observeUri + "/?" + queryString;
                    }
                    return observeUri;
                case "5":
                case "stop-observe":
                    var stopObserveUri = $"{Constants.VisionApiUri}/api/app-management/stop-observe/{endpoint}/{objectId}/{instanceId}";

                    if (resourceId != null)
                    {
                        var query = HttpUtility.ParseQueryString(string.Empty);
                        query["resourceId"] = string.Format("{0}", resourceId.Value);

                        string queryString = query.ToString();
                        stopObserveUri = stopObserveUri + "/?" + queryString;
                    }
                    return stopObserveUri;
                case "6":
                case "write-attributes": 
                    return $"{Constants.VisionApiUri}/api/app-management/write-attributes";
                default:
                    return null;
            }
        }
    }
}
