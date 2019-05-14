using System.Collections.Generic;

namespace VisionCloud.API.Models
{
    public class DeviceProperties
    {
        public string DeviceID { get; set; }
        public string BatteryLevel { get; set; }
        public string DcIn { get; set; }
        public string Apn { get; set; }
        public List<RunningApps> AppsOnDevice { get; set; }
        public string Name { get; set; }
    }

    public class RunningApps
    {
        public string appName { get; set; }
        public string endpoint { get; set; }
        public string version { get; set; }
    }
}
