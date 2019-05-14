using System;
using System.Security;
using System.Threading.Tasks;
using VisionCloud.API;
using VisionCloud.API.Models;
using VisionCloud.API.Services;

namespace VisionCloud.SDK.Views
{
    public class Init
    {
        public static bool isLoggedIn = false;
        public static Token jwt;
        public static DeviceProperties deviceProperties;
        public bool validUserEntry = false;
        public static string dn = string.Empty;
        public async Task<Tuple<DeviceProperties, Token>> ViewsAsync(string[] cmdInputs)
        {
            Console.WriteLine("VisionCloud SDK");

            do
            {
                var validUserName = false;
                var un = string.Empty;
                var pd = string.Empty;
                var validPswd = false;
                var validDeviceName = false;

                do
                {
                    ConsoleHelper.WriteInstruction("\nEnter username:\t");
                    un = Console.ReadLine();

                    if (string.IsNullOrEmpty(un))
                    {
                        ConsoleHelper.WriteError("Illegal entry for field: UserName");
                    }
                    else if (un.Length < 8)
                    {
                        ConsoleHelper.WriteError("Invalid length for field: UserName. Please try again...");
                    }
                    else
                    {
                        validUserName = true;
                    }

                } while (validUserName == false);

                do
                {
                    ConsoleHelper.WriteInstruction("\nEnter Password:\t");
                    Console.ForegroundColor = ConsoleColor.Black;
                    pd = Console.ReadLine();

                    if (string.IsNullOrEmpty(un))
                    {
                        ConsoleHelper.WriteError("Illegal entry for field: Password. Please try again...");
                    }
                    else if (un.Length < 8)
                    {
                        ConsoleHelper.WriteError("Invalid length for field: Password. Please try again...");
                    }
                    else {
                       validPswd = true;
                    }

                } while (validPswd == false);

                do
                {
                    ConsoleHelper.WriteInstruction("\nEnter Device Name:\t");
                    dn = Console.ReadLine();

                    if (string.IsNullOrEmpty(dn))
                    {
                        ConsoleHelper.WriteError("Illegal entry for field: Device Name");
                    }
                    else if (dn.Length < 4)
                    {
                        ConsoleHelper.WriteError("Invalid length for field: Device Name. Please try again...");
                    }
                    else
                    {
                        validDeviceName = true;
                    }

                } while (validDeviceName == false);

                var token = LoginService.GetToken(un, pd);
                jwt = token;
                if (jwt != null)
                {
                    isLoggedIn = true;
                    DeviceManagement dm = new DeviceManagement(jwt);
                    deviceProperties = await dm.GetDeviceInfoAsync(dn);
                    deviceProperties.Name = dn;
                }
                else
                {
                    ConsoleHelper.WriteError("Unable to authenticate. Please check your credentials");
                }

                if (deviceProperties != null)
                {
                    ConsoleHelper.WriteObjectInfo(deviceProperties);
                }

                if (jwt != null && (deviceProperties?.AppsOnDevice == null || deviceProperties?.AppsOnDevice.Count == 0 ))
                {
                    ConsoleHelper.WriteWarning("No Apps found that are running on the device. You have limted access through sdk when device is offline. Please try again later...");
                }

            } while (isLoggedIn == false || jwt == null || deviceProperties.DeviceID == null);

            return new Tuple<DeviceProperties, Token>(deviceProperties, jwt);
        }
    }
}
