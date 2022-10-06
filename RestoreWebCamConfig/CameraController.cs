using DirectShowLib;

namespace RestoreWebCamConfig;

public partial class CameraController
{
    private static readonly string[] VideoProcPropertyNamesById =
    {
        "Brightness",
        "Contrast",
        "Hue",
        "Saturation",
        "Sharpness",
        "Gamma",
        "ColorEnable",
        "WhiteBalance",
        "BacklightCompensation",
        "Gain",
        "Undocumented 10",
        "Undocumented 11",
        "Undocumented 12",
        "PowerLineFrequency",
        "Undocumented 14",
        "Undocumented 15",
        "Undocumented 16",
        "Undocumented 17",
        "Undocumented 18",
        "Undocumented 19",
        "Undocumented 20"
    };

    private static readonly string[] CameraControlPropertyNamesById =
    {
        "Pan",
        "Tilt",
        "Roll",
        "Zoom",
        "Exposure",
        "Iris",
        "Focus",
        "Undocumented 7",
        "Undocumented 8",
        "Undocumented 9",
        "Undocumented 10",
        "Undocumented 11",
        "Undocumented 12",
        "Undocumented 13",
        "Undocumented 14",
        "Undocumented 15",
        "Undocumented 16",
        "Undocumented 17",
        "Undocumented 18",
        "LowLightCompensation",
        "Undocumented 20"
    };

    private readonly IAMCameraControl _cameraControl;
    private readonly DsDevice _device;
    private readonly List<DsProperty> _properties;
    private readonly IAMVideoProcAmp _videoProcAmp;


    private CameraController(DsDevice videoInputDevice)
    {
        var id = typeof(IBaseFilter).GUID;
        _device = videoInputDevice ?? throw
            new ArgumentException("can not work without an device - it must not be null");

        _device.Mon.BindToObject(null!, null, ref id, out var source);
        _cameraControl = source as IAMCameraControl ?? throw
            new ArgumentException($"could not handle {_device} as camera");
        _videoProcAmp = source as IAMVideoProcAmp ?? throw
            new ArgumentException($"could not handle {_device} as video proc amp");
        _properties = FetchDeviceProperties();
    }

    private List<DsProperty> FetchDeviceProperties()
    {
        IEnumerable<DsProperty> FetchDsProperties(Func<int, DsProperty> dsPropertyFactory)
        {
            var properties = new List<DsProperty>();
            for (var i = 0; i < CameraControlPropertyNamesById.Length; i++)
                try
                {
                    properties.Add(dsPropertyFactory(i));
                }
                catch (InvalidOperationException)
                {
                }

            return properties;
        }

        var propertiesList = new List<DsProperty>();
        propertiesList.AddRange(FetchDsProperties(i => new VideoProcProperty(this, i)));
        propertiesList.AddRange(FetchDsProperties(i => new CamControlProperty(this, i)));
        return new List<DsProperty>(propertiesList.OrderBy(p => p.GetName()));
    }

    public static List<string> GetKnownCameraNames()
    {
        var res = new List<string>();
        foreach (var ds in DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice)) res.Add(ds.Name);

        return res;
    }

    public static CameraController FindCamera(string? humanReadableCameraName)
    {
        if (humanReadableCameraName == null)
            throw new ArgumentNullException(nameof(humanReadableCameraName) + " must not be null");

        foreach (var ds in DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice))
            if (humanReadableCameraName.Equals(ds.Name))
                return new CameraController(ds);
        throw new FileNotFoundException($"Camera {humanReadableCameraName} not found.");
    }

    public CameraDto GetDeviceProperties()
    {
        var res = new CameraDto(_device.Name);
        var properties = new List<CameraPropertyDto>();
        _properties.ForEach(property => properties.Add(property.CreateDto()));
        res.Properties = properties.AsReadOnly();
        return res;
    }
    
    public void RestoreProperties(CameraDto camera)
    {
        if(camera.Name != _device.Name)
            throw new InvalidDataException(
                $"Camera name of record ({camera.Name}) does not match this device ({_device.Name})");
        var properties = camera.Properties 
                     ?? throw new InvalidDataException($"CameraDto.Properties of {_device.Name} is empty");
        foreach (var propertyDto in properties)
        {
            var property = GetPropertyByName(propertyDto.Name 
                     ?? throw new InvalidDataException("Property of {_device.Name} has no name."));
            if(property.CanAdaptAutomatically())
                property.SetAutomatic(propertyDto.IsAutomaticallyAdapting);
            property.SetValue(propertyDto.Value);
        }
    }

    private DsProperty GetPropertyByName(string propertyName)
    {
        return _properties.Find(p => propertyName == p.GetName())
               ?? throw new InvalidDataException($"Device {_device.Name} has a property without a name");
    }
    public void SetPowerLineFrequency(PowerlineFrequency frequency)
    {
        _videoProcAmp.Set((VideoProcAmpProperty)13, (int)frequency, VideoProcAmpFlags.Manual);
        Console.WriteLine($"Setting video processing parameter PowerLineFrequency to {frequency}");
    }

    public void SetLowLightCompensation(bool onOff)
    {
        var lowLightCompensationValue = onOff ? 1 : 0;
        _cameraControl.Set((CameraControlProperty)19, lowLightCompensationValue, CameraControlFlags.Manual);
        Console.WriteLine($"Setting video processing parameter LowLightCompensation to {onOff}.");
    }
}