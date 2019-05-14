using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisionCloud.API.Models
{
    public class LwM2mApiModels
    {
        public class Lwm2mServerAttributesModel
        {
            // Client Endpoint - Target app
            public string clientEndpoint { get; set; }

            // Object Id
            public int objectId { get; set; }

            // Object Instance Id
            public int objectInstanceId { get; set; }

            // Resource Id
            public int resourceId { get; set; }

            // DIMENSION
            public long dim { get; set; }

            // OBJECT_VERSION
            public string ver { get; set; }

            // MINIMUM_PERIOD
            public long pmin { get; set; }

            // MAXIMUM_PERIOD
            public long pmax { get; set; }

            // GREATER_THAN
            public double gt { get; set; }

            // LESSER_THAN
            public double lt { get; set; }

            // STEP
            public double st { get; set; }
        }

        public class Lwm2mServerReadInstanceModel
        {
            public string status { get; set; }

            public bool valid { get; set; }

            public bool success { get; set; }

            public bool failure { get; set; }

            public Contents content { get; set; }
        }

        public class Contents
        {
            public int id { get; set; }
            public List<Lw2mResources> resources { get; set; }
        }

        public class Lw2mResources
        {
            public int id { get; set; }
            public string value { get; set; }
        }

        public class Lwm2mServerReadResourceModel
        {
            public string status { get; set; }

            public bool valid { get; set; }

            public bool success { get; set; }

            public bool failure { get; set; }

            public Content content { get; set; }
        }

        public class Content
        {
            public int id { get; set; }

            public string value { get; set; }
        }

        public class RegisteredDevicesList  // Need this for deserializing
        {
            public List<RegisteredDevices> registeredDevicesList { get; set; }
        }

        /// <summary>
        /// This Class will get the Clients(App) Registration Details to the Server
        /// </summary>
        public class RegisteredDevices
        {
            /// <summary>
            /// Endpoint with which device is connected to LWM2M Server (Typically App-Guid)
            /// </summary>
            public string endpoint { get; set; }

            /// <summary>
            /// RegistrationId is issued by Server when client(App) connects to server or for any Registration Update
            /// </summary>
            public string registrationId { get; set; }

            /// <summary>
            /// RegistrationDate is when the client(App) Registeres with the Server
            /// </summary>
            public string registrationDate { get; set; }

            /// <summary>
            /// LastUpdate is when the client(App) issued Registration Update
            /// </summary>
            public string lastUpdate { get; set; }

            /// <summary>
            /// Address is Client's(App) IP Address
            /// </summary>
            public string address { get; set; }

            /// <summary>
            /// Version of LWM2M Version
            /// </summary>
            public string lwM2mVersion { get; set; }


            /// <summary>
            /// Lifetime is how frequent client(App) sends Registration Update to LWM2M Server
            /// </summary>
            public int lifetime { get; set; }


            /// <summary>
            /// BindingMode is what protocol the client(App) is using to connect to LWM2M Server
            /// </summary>
            public string bindingMode { get; set; }


            /// <summary>
            /// RootPath is Path to Clients Objects and their instances
            /// </summary>
            public string rootPath { get; set; }


            /// <summary>
            /// Secure is when the client issued Registration Update
            /// </summary>
            public bool secure { get; set; }


            /// <summary>
            /// ObjectLinks is Links to Clients Objects and their instances
            /// </summary>
            public List<ObjectLinks> objectLinks { get; set; }
        }

        public class ObjectLinks
        {
            /// <summary>
            /// URL is Link to Clients Object and their instance
            /// </summary>
            public string url { get; set; }

            /// <summary>
            /// Attributes
            /// </summary>
            public Attributes attributes { get; set; }
        }

        public class Attributes
        {
            /// <summary>
            /// ct
            /// </summary>
            public int ct { get; set; }
            /// <summary>
            /// rt
            /// </summary>
            public string rt { get; set; }
        }

        public class Resource
        {
            /// <summary>
            /// ResourceID
            /// </summary>
            public int id { get; set; }
            /// <summary>
            /// Resource Value
            /// </summary>
            public string value { get; set; }
        }

        public class WriteModel
        {
            public int id { get; set; }

            public string value { get; set; }
        }

        public class CreateInstance
        {
            public int id { get; set; }
            public List<Resource> resources { get; set; }
        }

        public class ServerResponse
        {
            public string status { get; set; }

            public bool valid { get; set; }

            public bool success { get; set; }

            public bool failure { get; set; }
        }

        public class ObserveInstancePayload
        {
            public string Endpoint { get; set; }
            public string Instance { get; set; }
            public Value Data { get; set; }
        }

        public class ObserveResourcePayload
        {
            public string Endpoint { get; set; }
            public string Instance { get; set; }
            public ResourceData Data { get; set; }
        }

        public class ObjInsRes
        {
            public string InstanceId { get; set; }
            public string ObjectId { get; set; }
            public string ResourceId { get; set; }
        }

        public class InitialObserveResourceResponse
        {
            public string status { get; set; }

            public bool valid { get; set; }

            public bool success { get; set; }

            public bool failure { get; set; }

            public ResourceData content { get; set; }
        }

        public class InitialObserveInstanceResponse
        {
            public string status { get; set; }

            public bool valid { get; set; }

            public bool success { get; set; }

            public bool failure { get; set; }

            public Value content { get; set; }
        }

        public class Value
        {
            public string id { get; set; }
            public List<ResourceData> resources { get; set; }
        }

        public class ResourceData
        {
            public string id { get; set; }
            public string value { get; set; }
        }

        public class DiscoverObject
        {
            public string status { get; set; }

            public bool valid { get; set; }

            public bool success { get; set; }

            public bool failure { get; set; }

            public List<ObjectLinks> objectLinks { get; set; }
        }
    }
}
