using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace AlexaServer
{

    public enum DirectiveName
    {
        DiscoverAppliancesRequest,
        DiscoverAppliancesResponse,
        TurnOnRequest,
        TurnOnConfirmation,
        TurnOffRequest,
        TurnOffConfirmation,
        SetPercentageRequest,
        SetPercentageConfirmation,
        IncrementPercentageRequest,
        IncrementPercentageConfirmation,
        DecrementPercentageRequest,
        DecrementPercentageConfirmation,
        GetTargetTemperatureRequest,
        GetTargetTemperatureResponse,
        SetTargetTemperatureRequest,
        SetTargetTemperatureConfirmation,
        GetTemperatureReadingRequest,
        GetTemperatureReadingResponse,
        IncrementTargetTemperatureRequest,
        IncrementTargetTemperatureConfirmation,
        DecrementTargetTemperatureRequest,
        DecrementTargetTemperatureConfirmation,
        GetLockStateRequest,
        GetLockStateResponse,
        SetLockStateRequest,
        SetLockStateConfirmation,
        UnsupportedOperationError,
        UnwillingToSetValueError,
    }

    public class Header
    {
        [JsonProperty("namespace", Required = Required.Always)]
        public string Namespace { get; set; }
        [JsonProperty("name", Required = Required.Always)]
        [JsonConverter(typeof(StringEnumConverter))]
        public DirectiveName Name { get; set; }
        [JsonProperty("payloadVersion", Required = Required.Always)]
        public string PayloadVersion { get; set; }
        [JsonProperty("messageID", Required = Required.Always)]
        public Guid MessageID { get; set; }
    }

    public class ApplianceRequest
    {
        [JsonProperty("additionalApplianceDetails")]
        public Object AdditionalApplianceDetails { get; set; }
        [JsonProperty("applianceId")]
        public string ApplianceId { get; set; }
    }

    public class PercentageStateRequest
    {
        [JsonProperty("value")]
        public double Value { get; set; }
    }

    public class SetPercentageRequestPayload
    {
        [JsonProperty("accessToken")]
        public string AccessToken { get; set; }
        [JsonProperty("appliance")]
        public ApplianceRequest Appliance { get; set; }
        [JsonProperty("percentageState")]
        public PercentageStateRequest PercentageState { get; set; }
    }

    public class DeltaPercentageRequestPayload
    {
        [JsonProperty("accessToken")]
        public string AccessToken { get; set; }
        [JsonProperty("appliance")]
        public ApplianceRequest Appliance { get; set; }
        [JsonProperty("deltaPercentage")]
        public PercentageStateRequest DeltaPercentage { get; set; }
    }

    public class TargetTemperature
    {
        [JsonProperty("value")]
        public double Value { get; set; }
    }

    public class TemperatureMode
    {
        [JsonProperty("value")]
        public string Value { get; set; }
    }

    public class GetTargetTemperatureRequestPayload
    {
        [JsonProperty("accessToken")]
        public string AccessToken { get; set; }
        [JsonProperty("appliance")]
        public ApplianceRequest Appliance { get; set; }
    }

    public class SetTargetTemperatureRequestPayload
    {
        [JsonProperty("accessToken")]
        public string AccessToken { get; set; }
        [JsonProperty("appliance")]
        public ApplianceRequest Appliance { get; set; }
        [JsonProperty("targetTemperature")]
        public TargetTemperature TargetTemperature { get; set; }
    }

    public class GetTemperatureReadingRequestPayload
    {
        [JsonProperty("accessToken")]
        public string AccessToken { get; set; }
        [JsonProperty("appliance")]
        public ApplianceRequest Appliance { get; set; }
    }

    public class DeltaTargetTemperatureRequestPayload
    {
        [JsonProperty("accessToken")]
        public string AccessToken { get; set; }
        [JsonProperty("appliance")]
        public ApplianceRequest Appliance { get; set; }
        [JsonProperty("deltaTemperature")]
        public TargetTemperature DeltaTemperature { get; set; }
    }

    public class TargetTemperaturePreviousState
    {
        [JsonProperty("targetTemperature")]
        public TargetTemperature TargetTemperature { get; set; }
        [JsonProperty("mode")]
        public TemperatureMode TemperatureMode { get; set; }
    }

    public class GetTemperatureReadingResponsePayload
    {
        [JsonProperty("temperatureReading")]
        public TargetTemperature TemperatureReading { get; set; }
    }

    public class GetTargetTemperatureResponsePayload
    {
        [JsonProperty("targetTemperature")]
        public TargetTemperature TargetTemperature { get; set; }
        [JsonProperty("temperatureMode")]
        public TemperatureMode TemperatureMode { get; set; }
    }

    public class SetTargetTemperatureConfirmationPayload
    {
        [JsonProperty("targetTemperature")]
        public TargetTemperature TargetTemperature { get; set; }
        [JsonProperty("temperatureMode")]
        public TemperatureMode TemperatureMode { get; set; }
        [JsonProperty("previousState")]
        public TargetTemperaturePreviousState PreviousState { get; set; }
    }

    public class GetLockStateRequestPayload
    {
        [JsonProperty("accessToken")]
        public string AccessToken { get; set; }
        [JsonProperty("appliance")]
        public ApplianceRequest Appliance { get; set; }
    }

    public class GetLockStateResponsePayload
    {
        [JsonProperty("lockState")]
        public string LockState { get; set; }
    }

    public class SetLockStateRequestPayload
    {
        [JsonProperty("accessToken")]
        public string AccessToken { get; set; }
        [JsonProperty("appliance")]
        public ApplianceRequest Appliance { get; set; }
        [JsonProperty("lockState")]
        public string LockState { get; set; }
    }

    public class SetLockStateResponsePayload
    {
        [JsonProperty("lockState")]
        public string LockState { get; set; }
    }

    public class ErrorInfo
    {
        [JsonProperty("code", Required = Required.Always)]
        public string Code { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
    }

    public class UnwillingToSetValueErrorPayload
    {
        [JsonProperty("errorInfo", Required = Required.Always)]
        public ErrorInfo ErrorInfo { get; set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    abstract public class Appliance
    {
        [JsonProperty]
        public string applianceId { get; set; }
        [JsonProperty]
        public string manufacturerName { get; set; }
        [JsonProperty]
        public string modelName { get; set; }
        [JsonProperty]
        public string version { get; set; }
        [JsonProperty]
        public string friendlyName { get; set; }
        [JsonProperty]
        public string friendlyDescription { get; set; }
        [JsonProperty]
        public bool isReachable { get { return true; } }
        [JsonProperty]
        abstract public List<string> actions { get; }
        [JsonProperty]
        abstract public List<string> applianceTypes { get; }
    }

    public delegate void DelegateOnFn();
    public delegate void DelegateOffFn();
    public delegate void DelegateSetLevelFn(ushort level);
    public delegate void DelegateRaiseLevelFn(ushort delta);
    public delegate void DelegateLowerLevelFn(ushort delta);
    public delegate void DelegateSetTempFn(ushort temp);
    public delegate void DelegateLockFn();

    public interface ISwitchable
    {
        void On();
        void Off();
    }

    public interface ILevelable
    {
        void SetLevel(ushort level);
        void RaiseLevel(ushort delta);
        void LowerLevel(ushort delta);
    }

    public class AlexaSwitch : Appliance, ISwitchable
    {
        public DelegateOnFn OnFn { get; set; }
        public DelegateOffFn OffFn { get; set; }

        public void On()
        {
            if (OnFn != null) OnFn();
        }

        public void Off()
        {
            if (OffFn != null) OffFn();
        }

        override public List<string> actions
        {
            get
            {
                return new List<string>()
                {
                    "turnOn",
                    "turnOff",
                };
            }
        }

        public override List<string> applianceTypes
        {
            get
            {
                return new List<string>()
                {
                    "SWITCH",
                };
            }
        }
    }

    public class AlexaDimmer : Appliance, ISwitchable, ILevelable
    {
        public DelegateOnFn OnFn { get; set; }
        public DelegateOffFn OffFn { get; set; }
        public DelegateSetLevelFn SetLevelFn { get; set; }
        public DelegateRaiseLevelFn RaiseLevelFn { get; set; }
        public DelegateLowerLevelFn LowerLevelFn { get; set; }

        public void On()
        {
            if (OnFn != null) OnFn();
        }

        public void Off()
        {
            if (OffFn != null) OffFn();
        }

        public void SetLevel(ushort level)
        {
            if (SetLevelFn != null) SetLevelFn(level);
        }

        public void RaiseLevel(ushort delta)
        {
            if (RaiseLevelFn != null) RaiseLevelFn(delta);
        }

        public void LowerLevel(ushort delta)
        {
            if (LowerLevelFn != null) LowerLevelFn(delta);
        }
        
        override public List<string> actions
        {
            get
            {
                return new List<string>()
                {
                    "turnOn",
                    "turnOff",
                    "setPercentage",
                    "incrementPercentage",
                    "decrementPercentage"
                };
            }
        }

        public override List<string> applianceTypes
        {
            get
            {
                return new List<string>()
                {
                    "SWITCH",
                };
            }
        }
    }

    public enum ThermostatMode
    {
        AUTO,
        HEAT,
        COOL,
        AWAY, // TODO: expose this
        OTHER,
        OFF
    }

    public class AlexaThermostat : Appliance, ISwitchable
    {
        public DelegateOnFn OnFn { get; set; }
        public DelegateOffFn OffFn { get; set; }
        public DelegateSetTempFn SetTempFn { get; set; }

        public void On()
        {
            if (OnFn != null) OnFn();
        }

        public void Off()
        {
            if (OffFn != null) OffFn();
        }

        public void SetTemp(ushort temp)
        {
            if (SetTempFn != null) SetTempFn(temp);
        }

        [JsonIgnoreAttribute()]
        public ushort currentTemperature { get; set; }
        [JsonIgnoreAttribute()]
        public double currentTemperatureC
        {
            get
            {
                double tempF = currentTemperature / 10.0;
                return (tempF - 32.0) * 5.0 / 9.0;
            }
        }

        [JsonIgnoreAttribute()]
        public ushort targetTemperature { get; set; }
        [JsonIgnoreAttribute()]
        public double targetTemperatureC
        {
            get
            {
                double tempF = targetTemperature / 10.0;
                return (tempF - 32.0) * 5.0 / 9.0;
            }
        }
        [JsonIgnoreAttribute()]
        public ThermostatMode mode { get; set; }

        override public List<string> actions
        {
            get
            {
                return new List<string>()
                {
                    "turnOn",
                    "turnOff",
                    "getTemperatureReading",
                    "getTargetTemperature",
                    "setTargetTemperature",
                    "incrementTargetTemperature",
                    "decrementTargetTemperature"
                };
            }
        }

        public override List<string> applianceTypes
        {
            get
            {
                return new List<string>()
                {
                    "THERMOSTAT",
                };
            }
        }
    }

    public class AlexaLock : Appliance
    {
        public DelegateLockFn LockFn { get; set; }

        public void Lock()
        {
            if (LockFn != null) LockFn();
        }

        [JsonIgnoreAttribute()]
        public ushort isLocked { get; set; }

        override public List<string> actions
        {
            get
            {
                return new List<string>()
                {
                    "getLockState",
                    "setLockState",
                };
            }
        }

        public override List<string> applianceTypes
        {
            get
            {
                return new List<string>()
                {
                    "SMARTLOCK",
                };
            }
        }
    }

    public class DiscoverAppliancesResponsePayload
    {
        [JsonProperty("discoveredAppliances", Required = Required.Always)]
        public IEnumerable<Appliance> DiscoveredAppliances { get; set; }
    }

    public class Response
    {
        [JsonProperty("header", Required = Required.Always)]
        public Header Header { get; set; }
        [JsonProperty("payload", Required = Required.Always)]
        public Object Payload { get; set; }
    }

}