using System.Collections.Generic;

namespace VisionCloud.API.Models
{
    public class AppDefinition
    {
        public string endpoint { get; set; }

        public string registrationId { get; set; }

        public string registrationDate { get; set; }

        public string lastUpdate { get; set; }

        public string address { get; set; }

        public string lwM2mVersion { get; set; }

        public int lifetime { get; set; }

        public string bindingMode { get; set; }

        public string rootPath { get; set; }

        public bool secure { get; set; }

        public List<Lwm2mModel> lwm2mModel { get; set; }
    }

    public class Lwm2mModel
    {
        public int id { get; set; }

        public string name { get; set; }

        public string instancetype { get; set; }

        public int instance { get; set; }

        public bool mandatory { get; set; }

        public string description { get; set; }

        public string serverCreationTime { get; set; }

        public List<ResourceDefnitions> resourcedefs { get; set; }
    }

    public class ResourceDefnitions
    {
        public int id { get; set; }

        public string name { get; set; }

        public string operations { get; set; }

        public string instancetype { get; set; }

        public string mandatory { get; set; }

        public string type { get; set; }

        public string range { get; set; }

        public string units { get; set; }

        public string description { get; set; }

        public string value { get; set; }
    }
}

