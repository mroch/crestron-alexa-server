using Newtonsoft.Json;
using System;
using System.Text;
using Crestron.SimplSharp;      // For Basic SIMPL# Classes
using Crestron.SimplSharp.Net.Http;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Crestron.SimplSharp.CrestronIO;

namespace AlexaServer
{

    public static class DeviceList
    {
        private static Dictionary<String, Appliance> devices;
        static DeviceList()
        {
            devices = new Dictionary<String, Appliance>();
        }

        public static void registerSwitch(AlexaSwitch device)
        {
            lock (devices)
            {
                devices.Add(device.applianceId, device);
            }
        }

        public static void registerDimmer(AlexaDimmer device)
        {
            lock (devices)
            {
                devices.Add(device.applianceId, device);
            }
        }

        public static void registerThermostat(AlexaThermostat device)
        {
            lock (devices)
            {
                devices.Add(device.applianceId, device);
            }
        }

        public static void registerLock(AlexaLock device)
        {
            lock (devices)
            {
                devices.Add(device.applianceId, device);
            }
        }

        public static IEnumerable<Appliance> GetDevices()
        {
            lock (devices)
            {
                return devices.Values;
            }
        }

        public static Appliance GetDevice(string applianceId)
        {
            lock (devices)
            {
                return devices[applianceId];
            }
        }

        public static AlexaThermostat GetThermostat(string applianceId)
        {
            AlexaThermostat device = DeviceList.GetDevice(applianceId) as AlexaThermostat;
            if (device == null)
            {
                throw new Exception(String.Format(
                    "Device {0} is not a thermostat",
                    applianceId
                ));
            }
            return device;
        }

        public static AlexaLock GetLock(string applianceId)
        {
            AlexaLock device = DeviceList.GetDevice(applianceId) as AlexaLock;
            if (device == null)
            {
                throw new Exception(String.Format(
                    "Device {0} is not a lock",
                    applianceId
                ));
            }
            return device;
        }
    }

    public class AlexaProcessor
    {
        HttpServer Server;

        public delegate void stringToSimplPlus(SimplSharpString text);

        public stringToSimplPlus stringToPlus
        {
            get;
            set;
        }

        /// <summary>
        /// SIMPL+ can only execute the default constructor. If you have variables that require initialization, please
        /// use an Initialize method
        /// </summary>
        public AlexaProcessor()
        {
        }

        public void Start()
        {
            Server.Active = true;
        }

        public void Stop()
        {
            Server.Active = false;
        }

        public void InitializeHTTPServer(int port)
        {
            ErrorLog.Notice("Initializing Alexa server on port {0}\n", port);
            Server = new HttpServer();
            Server.Port = port;
            Server.OnHttpRequest += new OnHttpRequestHandler(HTTPRequestEventHandler);
        }

        private Object HandleTurnOnRequest(JObject request)
        {
            string DeviceID = (string)request["payload"]["appliance"]["applianceId"];
            ErrorLog.Notice("Turning on device {0}\n", DeviceID);
            Appliance device = DeviceList.GetDevice(DeviceID);
            ISwitchable switchableDevice = device as ISwitchable;
            if (switchableDevice != null)
            {
                switchableDevice.On();
            }
            // TODO: return an error if not switchable
            return new Response()
            {
                Header = new Header()
                {
                    Namespace = "Alexa.ConnectedHome.Control",
                    Name = DirectiveName.TurnOnConfirmation,
                    PayloadVersion = "2",
                    MessageID = Guid.NewGuid(),
                },
                Payload = new Dictionary<string, string> { },
            };
        }

        private Object HandleTurnOffRequest(JObject request)
        {
            string DeviceID = (string)request["payload"]["appliance"]["applianceId"];
            ErrorLog.Notice("Turning off device {0}\n", DeviceID);
            Appliance device = DeviceList.GetDevice(DeviceID);
            ISwitchable switchableDevice = device as ISwitchable;
            if (switchableDevice != null)
            {
                switchableDevice.Off();
            }
            // TODO: return an error if not switchable
            return new Response()
            {
                Header = new Header()
                {
                    Namespace = "Alexa.ConnectedHome.Control",
                    Name = DirectiveName.TurnOffConfirmation,
                    PayloadVersion = "2",
                    MessageID = Guid.NewGuid(),
                },
                Payload = new Dictionary<string, string> { },
            };
        }

        private Object HandleSetPercentageRequest(JObject request)
        {
            SetPercentageRequestPayload payload =
                                JsonConvert.DeserializeObject<SetPercentageRequestPayload>(
                                    request["payload"].ToString()
                                );
            string DeviceID = payload.Appliance.ApplianceId;
            double setPercent = payload.PercentageState.Value;
            ushort scaledPercent = Convert.ToUInt16(Math.Round(setPercent * 655.35, 0));
            Appliance device = DeviceList.GetDevice(DeviceID);
            ILevelable levelableDevice = device as ILevelable;
            if (levelableDevice != null)
            {
                levelableDevice.SetLevel(scaledPercent);
            }
            // TODO: return an error if not levelable
            return new Response()
            {
                Header = new Header()
                {
                    Namespace = "Alexa.ConnectedHome.Control",
                    Name = DirectiveName.SetPercentageConfirmation,
                    PayloadVersion = "2",
                    MessageID = Guid.NewGuid(),
                },
                Payload = new Dictionary<string, string> { },
            };
        }

        private Object HandleDeltaPercentageRequest(JObject request, string eventName, DirectiveName responseType)
        {
            DeltaPercentageRequestPayload payload =
                                JsonConvert.DeserializeObject<DeltaPercentageRequestPayload>(
                                    request["payload"].ToString()
                                );
            string DeviceID = payload.Appliance.ApplianceId;
            double setPercent = payload.DeltaPercentage.Value;
            ushort scaledPercent = Convert.ToUInt16(Math.Round(setPercent * 655.35, 0));
            
            Appliance device = DeviceList.GetDevice(DeviceID);
            ILevelable levelableDevice = device as ILevelable;
            if (levelableDevice != null)
            {
                if (eventName == "raise")
                {
                    levelableDevice.RaiseLevel(scaledPercent);
                }
                else if (eventName == "lower")
                {
                    levelableDevice.LowerLevel(scaledPercent);
                }
            }

            return new Response()
            {
                Header = new Header()
                {
                    Namespace = "Alexa.ConnectedHome.Control",
                    Name = responseType,
                    PayloadVersion = "2",
                    MessageID = Guid.NewGuid(),
                },
                Payload = new Dictionary<string, string> { },
            };
        }

        private Object HandleIncrementPercentageRequest(JObject request)
        {
            return HandleDeltaPercentageRequest(
                request,
                "raise",
                DirectiveName.IncrementPercentageConfirmation
            );
        }

        private Object HandleDecrementPercentageRequest(JObject request)
        {
            return HandleDeltaPercentageRequest(
                request,
                "lower",
                DirectiveName.DecrementPercentageConfirmation
            );
        }

        private Object HandleGetTargetTemperatureRequest(
            DirectiveName responseName,
            AlexaThermostat device
        ) {
            TemperatureMode mode = new TemperatureMode()
            {
                Value = device.mode.ToString(),
            };
            GetTargetTemperatureResponsePayload payload;
            if (device.mode == ThermostatMode.OFF)
            {
                payload = new GetTargetTemperatureResponsePayload()
                {
                    TemperatureMode = mode
                };
            }
            else
            {
                payload = new GetTargetTemperatureResponsePayload()
                {
                    TargetTemperature = new TargetTemperature()
                    {
                        Value = device.targetTemperatureC,
                    },
                    TemperatureMode = mode
                };
            }
            return new Response()
            {
                Header = new Header()
                {
                    Namespace = "Alexa.ConnectedHome.Query",
                    Name = responseName,
                    PayloadVersion = "2",
                    MessageID = Guid.NewGuid(),
                },
                Payload = payload,
            };
        }

        private Object HandleTemperatureChangeRequest(
            DirectiveName responseName,
            AlexaThermostat device,
            double setTempC
        )
        {
            if (device.mode == ThermostatMode.OFF)
            {
                return new Response()
                {
                    Header = new Header()
                    {
                        Namespace = "Alexa.ConnectedHome.Control",
                        Name = DirectiveName.UnwillingToSetValueError,
                        PayloadVersion = "2",
                        MessageID = Guid.NewGuid(),
                    },
                    Payload = new UnwillingToSetValueErrorPayload()
                    {
                        ErrorInfo = new ErrorInfo()
                        {
                            Code = "THERMOSTAT_IS_OFF",
                            Description = "Can't complete requested operation because the thermostat is off",
                        }
                    },
                };
            }

            double setTempF = setTempC * 1.8 + 32.0;
            ushort convertedTemp = Convert.ToUInt16(Math.Round(setTempF * 10.0, 0));

            device.SetTemp(convertedTemp);

            return new Response()
            {
                Header = new Header()
                {
                    Namespace = "Alexa.ConnectedHome.Control",
                    Name = responseName,
                    PayloadVersion = "2",
                    MessageID = Guid.NewGuid(),
                },
                Payload = new SetTargetTemperatureConfirmationPayload()
                {
                    TargetTemperature = new TargetTemperature()
                    {
                        Value = setTempC,
                    },
                    TemperatureMode = new TemperatureMode()
                    {
                        Value = device.mode.ToString(),
                    },
                    PreviousState = new TargetTemperaturePreviousState()
                    {
                        TargetTemperature = new TargetTemperature()
                        {
                            Value = device.targetTemperatureC,
                        },
                        TemperatureMode = new TemperatureMode()
                        {
                            Value = device.mode.ToString(),
                        }
                    },
                },
            };
        }

        private Object HandleGetTemperatureReadingRequest(
            DirectiveName responseName,
            AlexaThermostat device
        )
        {
            return new Response()
            {
                Header = new Header()
                {
                    Namespace = "Alexa.ConnectedHome.Query",
                    Name = responseName,
                    PayloadVersion = "2",
                    MessageID = Guid.NewGuid(),
                },
                Payload = new GetTemperatureReadingResponsePayload()
                {
                    TemperatureReading = new TargetTemperature()
                    {
                        Value = device.currentTemperatureC,
                    }
                },
            };
        }

        private Object HandleGetTemperatureReadingRequest(JObject request)
        {
            GetTemperatureReadingRequestPayload payload =
                JsonConvert.DeserializeObject<GetTemperatureReadingRequestPayload>(
                    request["payload"].ToString()
                );
            return HandleGetTemperatureReadingRequest(
                DirectiveName.GetTemperatureReadingResponse,
                DeviceList.GetThermostat(payload.Appliance.ApplianceId)
            );
        }

        private Object HandleGetTargetTemperatureRequest(JObject request)
        {
            GetTargetTemperatureRequestPayload payload =
                JsonConvert.DeserializeObject<GetTargetTemperatureRequestPayload>(
                    request["payload"].ToString()
                );
            return HandleGetTargetTemperatureRequest(
                DirectiveName.GetTargetTemperatureResponse,
                DeviceList.GetThermostat(payload.Appliance.ApplianceId)
            );
        }

        private Object HandleSetTargetTemperatureRequest(JObject request)
        {
            SetTargetTemperatureRequestPayload payload =
                JsonConvert.DeserializeObject<SetTargetTemperatureRequestPayload>(
                    request["payload"].ToString()
                );
            return HandleTemperatureChangeRequest(
                DirectiveName.SetTargetTemperatureConfirmation,
                DeviceList.GetThermostat(payload.Appliance.ApplianceId),
                payload.TargetTemperature.Value
            );
        }

        private Object HandleIncrementTargetTemperatureRequest(JObject request)
        {
            DeltaTargetTemperatureRequestPayload payload =
                JsonConvert.DeserializeObject<DeltaTargetTemperatureRequestPayload>(
                    request["payload"].ToString()
                );

            AlexaThermostat device = DeviceList.GetThermostat(payload.Appliance.ApplianceId);
            double targetTempC = device.targetTemperatureC + payload.DeltaTemperature.Value;

            return HandleTemperatureChangeRequest(
                DirectiveName.IncrementTargetTemperatureConfirmation,
                device,
                targetTempC
            );
        }

        private Object HandleDecrementTargetTemperatureRequest(JObject request)
        {
            DeltaTargetTemperatureRequestPayload payload =
                JsonConvert.DeserializeObject<DeltaTargetTemperatureRequestPayload>(
                    request["payload"].ToString()
                );

            AlexaThermostat device = DeviceList.GetThermostat(payload.Appliance.ApplianceId);
            double targetTempC = device.targetTemperatureC - payload.DeltaTemperature.Value;

            return HandleTemperatureChangeRequest(
                DirectiveName.DecrementTargetTemperatureConfirmation,
                device,
                targetTempC
            );
        }

        private Object HandleGetLockStateRequest(AlexaLock device)
        {
            return new Response()
            {
                Header = new Header()
                {
                    Namespace = "Alexa.ConnectedHome.Query",
                    Name = DirectiveName.GetLockStateResponse,
                    PayloadVersion = "2",
                    MessageID = Guid.NewGuid(),
                },
                Payload = new GetLockStateResponsePayload()
                {
                    LockState = device.isLocked == 1 ? "LOCKED" : "UNLOCKED"
                },
            };
        }

        private Object HandleGetLockStateRequest(JObject request)
        {
            GetLockStateRequestPayload payload =
                JsonConvert.DeserializeObject<GetLockStateRequestPayload>(
                    request["payload"].ToString()
                );
            return HandleGetLockStateRequest(
                DeviceList.GetLock(payload.Appliance.ApplianceId)
            );
        }

        private Object HandleSetLockStateRequest(AlexaLock device, bool shouldLock)
        {
            if (shouldLock)
            {
                device.Lock();
            }

            return new Response()
            {
                Header = new Header()
                {
                    Namespace = "Alexa.ConnectedHome.Control",
                    Name = DirectiveName.SetLockStateConfirmation,
                    PayloadVersion = "2",
                    MessageID = Guid.NewGuid(),
                },
                Payload = new SetLockStateResponsePayload()
                {
                    LockState = "LOCKED"
                },
            };
        }

        private Object HandleSetLockStateRequest(JObject request)
        {
            SetLockStateRequestPayload payload =
                JsonConvert.DeserializeObject<SetLockStateRequestPayload>(
                    request["payload"].ToString()
                );
            return HandleSetLockStateRequest(
                DeviceList.GetLock(payload.Appliance.ApplianceId),
                payload.LockState == "LOCKED"
            );
        }

        public Object HTTPPutEventHandler(JObject request)
        {
            Header header = JsonConvert.DeserializeObject<Header>(request["header"].ToString());
            switch (header.Namespace)
            {
                case "Alexa.ConnectedHome.Control":
                    switch (header.Name)
                    {
                        case DirectiveName.TurnOnRequest:
                            return HandleTurnOnRequest(request);
                        case DirectiveName.TurnOffRequest:
                            return HandleTurnOffRequest(request);
                        case DirectiveName.SetPercentageRequest:
                            return HandleSetPercentageRequest(request);
                        case DirectiveName.IncrementPercentageRequest:
                            return HandleIncrementPercentageRequest(request);
                        case DirectiveName.DecrementPercentageRequest:
                            return HandleDecrementPercentageRequest(request);
                        case DirectiveName.SetTargetTemperatureRequest:
                            return HandleSetTargetTemperatureRequest(request);
                        case DirectiveName.IncrementTargetTemperatureRequest:
                            return HandleIncrementTargetTemperatureRequest(request);
                        case DirectiveName.DecrementTargetTemperatureRequest:
                            return HandleDecrementTargetTemperatureRequest(request);
                        case DirectiveName.SetLockStateRequest:
                            return HandleSetLockStateRequest(request);
                        default:
                            throw new Exception("unsupported name in Control request");
                    }
                default:
                    throw new Exception("unsupported namespace in PUT request");
            }
        }

        public Object HTTPPostEventHandler(JObject request)
        {
            Header header = JsonConvert.DeserializeObject<Header>(request["header"].ToString());
            switch (header.Namespace)
            {
                case "Alexa.ConnectedHome.Discovery":
                    return new Response()
                    {
                        Header = new Header()
                        {
                            Namespace = "Alexa.ConnectedHome.Discovery",
                            Name = DirectiveName.DiscoverAppliancesResponse,
                            PayloadVersion = "2",
                            MessageID = Guid.NewGuid(),
                        },
                        Payload = new DiscoverAppliancesResponsePayload()
                        {
                            DiscoveredAppliances = DeviceList.GetDevices()
                        }
                    };
                case "Alexa.ConnectedHome.Query":
                    switch (header.Name) { 
                        case DirectiveName.GetTargetTemperatureRequest:
                            return HandleGetTargetTemperatureRequest(request);
                        case DirectiveName.GetTemperatureReadingRequest:
                            return HandleGetTemperatureReadingRequest(request);
                        case DirectiveName.GetLockStateRequest:
                            return HandleGetLockStateRequest(request);
                        default:
                            throw new Exception("unsupported name in Query request");
                    }
                default:
                    throw new Exception("unsupported namespace in POST request");
            }
        }

        public Object HandleRequest(string requestType, string content)
        {
            JObject request = JObject.Parse(content);
            switch (requestType)
            {
                case "POST":
                    return HTTPPostEventHandler(request);
                case "PUT":
                    return HTTPPutEventHandler(request);
                default:
                    throw new Exception("unsupported HTTP method");
            }
        }

        public void HTTPRequestEventHandler(Object sender, OnHttpRequestArgs requestArgs)
        {
            try
            {
                switch (requestArgs.Request.Header.RequestPath)
                {
                    case "/alexa/":
                        Object response = null;
                        String message = "not handled";
                        try
                        {
                            response = HandleRequest(
                                 requestArgs.Request.Header.RequestType,
                                 requestArgs.Request.ContentString
                            );
                        }
                        catch (Exception e)
                        {
                            message = e.ToString();
                        }
                        if (response == null)
                        {
                            response = new Response()
                            {
                                Header = new Header()
                                {
                                    Namespace = "Alexa.ConnectedHome.Control",
                                    Name = DirectiveName.UnsupportedOperationError,
                                    PayloadVersion = "2",
                                    MessageID = Guid.NewGuid(),
                                },
                                Payload = new Dictionary<string, string> { { "error", message } },
                            };
                        }
                        requestArgs.Response.ContentString = JsonConvert.SerializeObject(response);
                        return;
                    default:
                        break;
                }
                requestArgs.Response.Code = 404;
                requestArgs.Response.ResponseText = "File Not Found";
                requestArgs.Response.ContentString = "File Not Found";
            }
            catch (Exception e)
            {
                requestArgs.Response.Code = 500;
                requestArgs.Response.ResponseText = "Server Error";
                requestArgs.Response.ContentString = e.ToString();
            }
        }
    }
}