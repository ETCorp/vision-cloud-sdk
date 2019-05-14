using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using VisionCloud.API;
using VisionCloud.API.Models;
using VisionCloud.API.Services;
using VisionCloud.SDK.Helpers;
using VisionCloud.SDK.Views;
using static VisionCloud.API.Models.LwM2mApiModels;

namespace VisionCloud.SDK.StartUp
{
    class StartUp
    {
        public static Token jwt;
        public static DeviceProperties deviceProperties;
        public static RunningApps selectedApp;
        static async Task Main(string[] args)
        {
            Init cli = new Init();
            var props = await cli.ViewsAsync(args);
            deviceProperties = props.Item1;
            jwt = props.Item2;
            bool sessionEnded = false;

            DeviceManagement dm = new DeviceManagement(jwt);
            AppManagement am = new AppManagement(jwt);

            do
            {
                ConsoleHelper.WritePrompt();

                string commandLine = Console.ReadLine();
                string[] commands = commandLine.Split(' ');
                try
                {
                    if (commands.Length > 0)
                    {
                        switch (commands[0].ToLower())
                        {
                            case "list":
                            case "ls":
                                if (commands.Length != 1)
                                {
                                    ConsoleHelper.WriteError("Invalid number of arguments in command");
                                }
                                else
                                {
                                    if (deviceProperties.AppsOnDevice.Count > 0)
                                    {
                                        ConsoleHelper.WriteApps(deviceProperties.AppsOnDevice);
                                    }
                                    else
                                    {
                                        ConsoleHelper.WriteError("No apps found for selected device");
                                    }
                                }

                                break;

                            case "deviceinfo":
                            case "di":
                                if (commands.Length < 2)
                                {
                                    ConsoleHelper.WriteError("Invalid number of arguments in command");
                                }
                                else
                                {
                                    var requestedDeviceInfo = await dm.GetDeviceInfoAsync(commands[1].Trim());

                                    if (requestedDeviceInfo?.DeviceID != null)
                                    {
                                        ConsoleHelper.WriteObjectInfo(requestedDeviceInfo);
                                    }
                                    else
                                    {
                                        ConsoleHelper.WriteError("No device found or device is offline");
                                    }
                                }

                                break;

                            case "selectapp":
                            case "sa":
                                if (commands.Length < 2)
                                {
                                    ConsoleHelper.WriteError("Invalid number of arguments in command");
                                }
                                else
                                {
                                    var allApps = deviceProperties.AppsOnDevice;
                                    var appToSelect = commands[1].Trim();
                                    if (allApps?.Count > 0)
                                    {
                                        selectedApp = allApps.Where(x => x.endpoint == appToSelect).FirstOrDefault();
                                    }

                                    if (selectedApp == null)
                                    {
                                        //TODO: Refresh cmd
                                        ConsoleHelper.WriteError("App not found on the device. If you installed an app after initializing session try refresh cmd");
                                    }
                                    else
                                    {
                                        ConsoleHelper.WriteNotice($"Selected app context {selectedApp.appName} on {selectedApp.endpoint}");
                                    }
                                }

                                break;

                            case "changedevice":
                            case "cd":
                                if (commands.Length < 2)
                                {
                                    ConsoleHelper.WriteError("Invalid number of arguments in command");
                                }
                                else
                                {
                                    var requestedDeviceInfo = await dm.GetDeviceInfoAsync(commands[1].Trim());

                                    if (requestedDeviceInfo?.DeviceID != null && !string.IsNullOrEmpty(requestedDeviceInfo?.DeviceID))
                                    {
                                        if (requestedDeviceInfo?.AppsOnDevice?.Count > 0)
                                        {
                                            deviceProperties = requestedDeviceInfo;
                                            deviceProperties.Name = commands[1].Trim();
                                            selectedApp = null;
                                            ConsoleHelper.WriteObjectInfo(requestedDeviceInfo);
                                            ConsoleHelper.WriteInstruction($"Please select app context to perform operations on the device {deviceProperties.Name}");
                                        }
                                        else
                                        {
                                            if (string.IsNullOrEmpty(requestedDeviceInfo?.DeviceID) || deviceProperties?.AppsOnDevice == null || deviceProperties?.AppsOnDevice.Count == 0)
                                            {
                                                ConsoleHelper.WriteError("Illegal device chosen in the context. Device doesn't exist or device is not online. Switching back to older context");
                                            }
                                        }
                                    }
                                    else
                                    {
                                        ConsoleHelper.WriteError("No device found or device is offline");
                                    }
                                }

                                break;

                            case "replay":
                            case "rp":
                                // from and to dates need to be passed

                                if (commands.Length != 1)
                                {
                                    ConsoleHelper.WriteError("Invalid number of arguments in command");
                                }
                                else
                                {
                                    if (selectedApp == null)
                                    {
                                        ConsoleHelper.WriteError("Select app context before running replay");
                                    }
                                    else
                                    {
                                        ConsoleHelper.WriteInstruction("Enter a from date (e.g. 10/22/1987): ");
                                        DateTime frmdate;

                                        if (DateTime.TryParse(Console.ReadLine(), out frmdate))
                                        {
                                            ConsoleHelper.WriteStepResult("The day of the week is: " + frmdate.DayOfWeek);

                                            ConsoleHelper.WriteInstruction("Enter a to date (e.g. 10/22/1987): ");
                                            DateTime todate;

                                            if (DateTime.TryParse(Console.ReadLine(), out todate))
                                            {
                                                ConsoleHelper.WriteStepResult("The day of the week is: " + todate.DayOfWeek);

                                                ConsoleHelper.WriteInstruction("Enter a location on local machine where you want to save the files (e.g. C:\\Users\\Desktop\\): ");
                                                var dirPath = @"" + Console.ReadLine();

                                                if (Directory.Exists(dirPath))
                                                {
                                                    var userPath = new DirectoryInfo(dirPath);
                                                    ConsoleHelper.WriteStepResult($"Path verified. Directory exists at {userPath.FullName}");

                                                    ConsoleHelper.WriteStep($"Getting app data for period from {frmdate} to {todate}");
                                                    var tup = await am.GetDataForPeriod(selectedApp.endpoint, frmdate, true, todate);
                                                    if (tup != null)
                                                    {
                                                        var appDef = tup.Item1;
                                                        var dataInfo = tup.Item2;
                                                        if (appDef != null && dataInfo != null)
                                                        {
                                                            ConsoleHelper.WriteStep(string.Format("Downloading {0} files", dataInfo.FileNames.Count.ToString()));

                                                            var data = new List<Lwm2mModel>();
                                                            var dataFromBlob = new List<Dictionary<string, dynamic>>();
                                                            dataFromBlob = BlobDownloadHelper.Download(dataInfo);

                                                            ConsoleHelper.WriteSuccess(string.Format("Downloading {0} files completed", dataInfo.FileNames.Count.ToString()));

                                                            if (dataFromBlob?.Count > 0)
                                                            {
                                                                ConsoleHelper.WriteStep("Mapping your data to LwM2m models");
                                                                data = am.MapDataToModels(appDef, dataFromBlob);
                                                            }
                                                            else
                                                            {
                                                                ConsoleHelper.WriteError("Unable to download. Please try again");
                                                            }

                                                            if (data?.Count > 0)
                                                            {
                                                                ConsoleHelper.WriteStep(string.Format("Writing to location {0}", userPath.FullName));

                                                                foreach (var item in data)
                                                                {
                                                                    ConsoleHelper.WriteStepIo(string.Format("Writing file from {0}", item.serverCreationTime));

                                                                    string jsondata = new JavaScriptSerializer().Serialize(item);
                                                                    File.WriteAllText(Path.Combine(userPath.FullName, $"{Guid.NewGuid()}.json"), jsondata);
                                                                }
                                                            }
                                                            else
                                                            {
                                                                ConsoleHelper.WriteError("Unable to map data. Please try again");
                                                            }
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    ConsoleHelper.WriteError("Directory not found: " + dirPath);
                                                }
                                            }
                                            else
                                            {
                                                ConsoleHelper.WriteError("You have entered an incorrect date value.");
                                            }
                                        }
                                        else
                                        {
                                            ConsoleHelper.WriteError("You have entered an incorrect date value.");
                                        }
                                    }
                                }

                                break;

                            case "op":
                            case "operation":

                                if(selectedApp != null)
                                {
                                    ConsoleHelper.WriteStep($"Performing operation on {selectedApp.appName}");
                                }
                                else
                                {
                                    ConsoleHelper.WriteError("Select app context (selectapp or sa) before choosing operation.");
                                    break;
                                }

                                var lst = new List<string>()
                                {
                                    "1: read",
                                    "2: write",
                                    "3: execute",
                                    "4: observe",
                                    "5: stop-observe",
                                    "6: writeAttribute"
                                };

                                ConsoleHelper.WriteInstruction("Please choose a valid operation");
                                ConsoleHelper.WriteOptions(lst);

                                Console.ForegroundColor = ConsoleColor.White;
                                var input = Console.ReadLine();
                                int objId = 0;
                                int instanceid = 0;
                                int? resourceId = null;
                                ConsoleHelper.WriteInstruction("Enter the path for your request.  (e.g. ObjectId/InstanceId/ResourceId");
                                Console.ForegroundColor = ConsoleColor.White;
                                var path = Console.ReadLine();

                                try
                                {
                                    var pathSplit = path.Split('/');
                                    if (pathSplit.Length < 2)
                                    {
                                        ConsoleHelper.WriteError("You have entered an incorrect path.");
                                    }
                                    else
                                    {
                                        if (int.TryParse(pathSplit[0].ToString(), out objId))
                                        {
                                            if (int.TryParse(pathSplit[1].ToString(), out instanceid))
                                            {
                                              
                                                if (pathSplit.Length == 3)
                                                {
                                                    var holder = 0;
                                                    if (int.TryParse(pathSplit[2].ToString(), out holder))
                                                    {
                                                        resourceId = holder;
                                                    }
                                                    else
                                                    {
                                                        ConsoleHelper.WriteError("You have entered an incorrect resource id in the path.");
                                                    }
                                                }
                                                else
                                                {
                                                    resourceId = null;
                                                }
                                            }
                                            else
                                            {
                                                ConsoleHelper.WriteError("You have entered an incorrect object id in the path.");
                                            }
                                        }
                                        else
                                        {
                                            ConsoleHelper.WriteError("You have entered an incorrect instance id in the path.");
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    ConsoleHelper.WriteError(string.Format("Error parsing provided path, {0}", ex.Message));
                                }

                                switch (input)
                                {
                                    case "1":
                                    case "read":
                                        var result = await am.PerformOperationAsync(input, selectedApp.endpoint, objId, instanceid, resourceId);

                                        break;

                                    case "3":
                                    case "execute":
                                        if(resourceId != null)
                                        {
                                            var exeRes = await am.PerformOperationAsync(input, selectedApp.endpoint, objId, instanceid, resourceId);
                                        }
                                        else
                                        {
                                            ConsoleHelper.WriteError("Cannot execute Instance. Provide Resource");
                                        }

                                        break;

                                    case "2":
                                    case "write":
                                        var ls = new List<Resource>();
                                        if (resourceId == null)
                                        {
                                            ConsoleHelper.WriteInstruction("Enter all the resource id : value sepeareted by comma for the resources you want to write");
                                            Console.ForegroundColor = ConsoleColor.White;
                                            var observeCollectionStr = Console.ReadLine();

                                            if (string.IsNullOrEmpty(observeCollectionStr) )
                                            {
                                                ConsoleHelper.WriteError("collection cannot be null");
                                            }
                                            else
                                            {
                                                string[] observeCollection = observeCollectionStr.Split(',');
                                                if (observeCollection.Length > 0) {
                                                    var dict = new Dictionary<int, string>();

                                                    foreach (var item in observeCollection)
                                                    {
                                                        var dictionary = item.Split(',');

                                                        if (dictionary.Length > 0) {

                                                            foreach (var idValue in dictionary)
                                                            {

                                                                var splitter = idValue.Split(':');

                                                                if (splitter.Length == 2)
                                                                {
                                                                    var r = new Resource();
                                                                    int valHol;
                                                                    if (int.TryParse(splitter[0].ToString(), out valHol) && !string.IsNullOrEmpty(splitter[1].ToString()))
                                                                    {
                                                                        r.id = valHol;
                                                                        r.value = splitter[1].ToString();
                                                                    }
                                                                    else
                                                                    {
                                                                        ConsoleHelper.WriteError("Unable to parse Resource Id or value in collecction");
                                                                    }

                                                                    ls.Add(r);
                                                                }
                                                                else
                                                                {
                                                                    ConsoleHelper.WriteError("Invalid number of arguments for properties in collection");
                                                                }
                                                            }
                                                        }
                                                        else
                                                        {
                                                            ConsoleHelper.WriteError("Invalid arguments");
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            ConsoleHelper.WriteInstruction("Enter the resource value");
                                            Console.ForegroundColor = ConsoleColor.White;
                                            var value = Console.ReadLine();

                                            if (string.IsNullOrEmpty(value))
                                            {
                                                ConsoleHelper.WriteError("Invalid argument for value");
                                            }
                                            else
                                            {
                                                var r = new Resource();
                                                r.id = resourceId.Value;
                                                r.value = value;

                                                ls.Add(r);
                                            }
                                        }

                                        if(ls.Count > 0)
                                        {
                                            var url = am.ComposeUri(input, selectedApp.endpoint, objId, instanceid, resourceId);
                                            await am.WriteLWM2MResource(url, ls);
                                        }

                                        break;

                                    case "4":
                                    case "observe":
                                        var observeResult = await am.PerformOperationAsync(input, selectedApp.endpoint, objId, instanceid, resourceId);

                                        break;

                                    case "5":
                                    case "stop-observe":
                                        var stopObserveResult = await am.PerformOperationAsync(input, selectedApp.endpoint, objId, instanceid, resourceId);

                                        break;

                                    case "6":
                                    case "write-attributes":
                                        Lwm2mServerAttributesModel modelToSend = new Lwm2mServerAttributesModel();

                                        ConsoleHelper.WriteInstruction("Enter value for Dimension (dim) or Press Enter to skip to next attribute");
                                        Console.ForegroundColor = ConsoleColor.White;
                                        var dim = Console.ReadLine();

                                        if (!string.IsNullOrEmpty(dim))
                                        {
                                            int valHol;
                                            if (int.TryParse(dim, out valHol))
                                            {
                                                modelToSend.dim = valHol;
                                            }
                                            else
                                            {
                                                ConsoleHelper.WriteError("Unable to parse Dim value");
                                            }
                                        }
                                        else
                                        {
                                            modelToSend.dim = -1;
                                        }

                                        ConsoleHelper.WriteInstruction("Enter value for Version (ver) or Press Enter to skip to next attribute");
                                        Console.ForegroundColor = ConsoleColor.White;
                                        var ver = Console.ReadLine();

                                        if(!string.IsNullOrEmpty(ver))
                                        {
                                            modelToSend.ver = ver;
                                            ConsoleHelper.WriteError("Unable to parse Ver value");
                                        }
                                        else
                                        {
                                            modelToSend.ver = null;
                                        }

                                        ConsoleHelper.WriteInstruction("Enter value for Minimum period (pmin) or Press Enter to skip to next attribute");
                                        Console.ForegroundColor = ConsoleColor.White;
                                        var pmin = Console.ReadLine();

                                        if (!string.IsNullOrEmpty(pmin))
                                        {
                                            int valHol;
                                            if (int.TryParse(pmin, out valHol))
                                            {
                                                modelToSend.pmin = valHol;
                                            }
                                            else
                                            {
                                                ConsoleHelper.WriteError("Unable to parse Pmin value");
                                            }
                                        }
                                        else
                                        {
                                            modelToSend.pmin = -1;
                                        }

                                        ConsoleHelper.WriteInstruction("Enter value for Maximum period (pmax) or Press Enter to skip to next attribute");
                                        Console.ForegroundColor = ConsoleColor.White;
                                        var pmax = Console.ReadLine();

                                        if (!string.IsNullOrEmpty(pmax))
                                        {
                                            int valHol;
                                            if (int.TryParse(pmax, out valHol))
                                            {
                                                modelToSend.pmax = valHol;
                                            }
                                            else
                                            {
                                                ConsoleHelper.WriteError("Unable to parse Pmax value");
                                            }
                                        }
                                        else
                                        {
                                            modelToSend.pmax = -1;
                                        }

                                        ConsoleHelper.WriteInstruction("Enter value for Greaterthan (gt) or Press Enter to skip to next attribute");
                                        Console.ForegroundColor = ConsoleColor.White;
                                        var gt = Console.ReadLine();

                                        if (!string.IsNullOrEmpty(gt))
                                        {
                                            int valHol;
                                            if (int.TryParse(gt, out valHol))
                                            {
                                                modelToSend.gt = valHol;
                                            }
                                            else
                                            {
                                                ConsoleHelper.WriteError("Unable to parse gt value");
                                            }

                                        }
                                        else
                                        {
                                            modelToSend.gt = -1;
                                        }

                                        ConsoleHelper.WriteInstruction("Enter value for Lesserthan (lt) or Press Enter to skip to next attribute");
                                        Console.ForegroundColor = ConsoleColor.White;
                                        var lt = Console.ReadLine();

                                        if (!string.IsNullOrEmpty(lt))
                                        {
                                            int valHol;
                                            if (int.TryParse(lt, out valHol))
                                            {
                                                modelToSend.lt = valHol;
                                            }
                                            else
                                            {
                                                ConsoleHelper.WriteError("Unable to parse lt value");
                                            }
                                        }
                                        else
                                        {
                                            modelToSend.lt = -1;
                                        }

                                        ConsoleHelper.WriteInstruction("Enter value for Step (st) or Press Enter to skip");

                                        var st = Console.ReadLine();
                                        if (!string.IsNullOrEmpty(st))
                                        {

                                            double valHol;
                                            if (double.TryParse(lt, out valHol))
                                            {
                                                modelToSend.st = valHol;
                                            }
                                            else
                                            {
                                                ConsoleHelper.WriteError("Unable to parse st value to double");
                                            }

                                        }
                                        else
                                        {
                                            modelToSend.st = -1;
                                        }
                                        modelToSend.clientEndpoint = selectedApp.endpoint;
                                        var uri = am.ComposeUri(input, selectedApp.endpoint, objId, instanceid, resourceId);
                                        await am.PostAttributes(uri, modelToSend);

                                        break;

                                    default:
                                        ConsoleHelper.WriteError("Choose a valid option");

                                        break;
                                }

                                break;

                            case "refresh":
                            case "rf":
                                if (commands.Length == 2)
                                {
                                    var requestedDeviceInfo = await dm.GetDeviceInfoAsync(deviceProperties.Name);

                                    if (requestedDeviceInfo?.DeviceID != null && !string.IsNullOrEmpty(requestedDeviceInfo?.DeviceID))
                                    {
                                        if (requestedDeviceInfo?.AppsOnDevice?.Count > 0)
                                        {
                                            deviceProperties = requestedDeviceInfo;
                                            deviceProperties.Name = commands[1].Trim();

                                            ConsoleHelper.WriteObjectInfo(requestedDeviceInfo);
                                        }
                                        else
                                        {
                                            if (string.IsNullOrEmpty(requestedDeviceInfo?.DeviceID) || deviceProperties?.AppsOnDevice == null || deviceProperties?.AppsOnDevice.Count == 0)
                                            {
                                                ConsoleHelper.WriteError("Illegal device chosen in the context. Device doesn't exists or device is not online or no apps are running on the device. Switching back to older context");
                                            }
                                        }
                                    }
                                    else
                                    {
                                        ConsoleHelper.WriteError("No device found or device is offline");
                                    }
                                }
                                else
                                {
                                    ConsoleHelper.WriteError("Invalid number of arguments in command");
                                }

                                break;

                            case "exit":
                                sessionEnded = true;

                                break;
                        }
                    }
                    else
                    {
                        ConsoleHelper.WriteError("Invalid number of arguments in command");
                    }
                }
                catch (Exception ex)
                {
                    ConsoleHelper.WriteException(ex);
                }

            } while (sessionEnded == false);
        }
    }
}
