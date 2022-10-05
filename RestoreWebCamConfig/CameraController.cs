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

    public string GetName()
    {
        return _device.Name;
    }

    private int GetManualCamControl(CameraControlProperty propertyId)
    {
        var res = _cameraControl.Get(propertyId, out var value, out var _);
        Console.WriteLine($"Got control parameter {propertyId} as {value}. Return code is {res}");
        return value;
    }

    private void SetManualCamControl(CameraControlProperty property, int value)
    {
        Console.WriteLine($"Setting control parameter {property} to {value}");
        _cameraControl.Set(property, value, CameraControlFlags.Manual);
    }

    private int GetManualVideoProcessingProperty(VideoProcAmpProperty propertyId)
    {
        var res = _videoProcAmp.Get(propertyId, out var value, out _);
        Console.WriteLine($"Got video amplification parameter {propertyId} as {value}. Return code is {res}");
        return value;
    }

    private void SetManualVideoProcessingProperty(VideoProcAmpProperty property, int value)
    {
        Console.WriteLine($"Setting video processing parameter {property} to {value}");
        _videoProcAmp.Set(property, value, VideoProcAmpFlags.Manual);
    }

    public CameraDto GetDeviceProperties()
    {
        var res = new CameraDto(_device.Name);
        var properties = new List<CameraPropertyDto>();
        _properties.ForEach(property => properties.Add(CameraPropertyDto.CreateDtoFromDsProperty(property)));
        res.Properties = properties.AsReadOnly();
        return res;
    }

    public PowerlineFrequency GetPowerLineFrequency()
    {
        var value = GetManualVideoProcessingProperty((VideoProcAmpProperty)13);
        switch (value)
        {
            case (int)PowerlineFrequency.Hz50: return PowerlineFrequency.Hz50;
            case (int)PowerlineFrequency.Hz60: return PowerlineFrequency.Hz60;
            default: throw new InvalidDataException($"Unknown frequency code {value}");
        }
    }

    public void SetPowerLineFrequency(PowerlineFrequency frequency)
    {
        _videoProcAmp.Set((VideoProcAmpProperty)13, (int)frequency, VideoProcAmpFlags.Manual);
        Console.WriteLine($"Setting video processing parameter PowerLineFrequency to {frequency}");
    }

    public bool GetLowLightCompensation()
    {
        var res = GetManualCamControl((CameraControlProperty)19);
        return res != 0;
    }

    public void SetLowLightCompensation(bool onOff)
    {
        var lowLightCompensationValue = onOff ? 1 : 0;
        _cameraControl.Set((CameraControlProperty)19, lowLightCompensationValue, CameraControlFlags.Manual);
        Console.WriteLine($"Setting video processing parameter LowLightCompensation to {onOff}.");
    }

    public int GetManualZoom()
    {
        return GetManualCamControl(CameraControlProperty.Zoom);
    }

    public void SetManualZoom(int zoom)
    {
        SetManualCamControl(CameraControlProperty.Zoom, zoom);
    }

    public int GetManualFocus()
    {
        return GetManualCamControl(CameraControlProperty.Focus);
    }

    public void SetManualFocus(int focus)
    {
        SetManualCamControl(CameraControlProperty.Focus, focus);
    }

    public int GetExposure()
    {
        return GetManualCamControl(CameraControlProperty.Exposure);
    }

    public void SetExposure(int exposure)
    {
        SetManualCamControl(CameraControlProperty.Exposure, exposure);
    }

    public int GetPan()
    {
        return GetManualCamControl(CameraControlProperty.Pan);
    }

    public void SetPan(int pan)
    {
        SetManualCamControl(CameraControlProperty.Pan, pan);
    }

    public int GetTilt()
    {
        return GetManualCamControl(CameraControlProperty.Tilt);
    }

    public void SetTilt(int tilt)
    {
        SetManualCamControl(CameraControlProperty.Tilt, tilt);
    }

    public int GetBrightness()
    {
        return GetManualVideoProcessingProperty(VideoProcAmpProperty.Brightness);
    }

    public void SetBrightness(int brightness)
    {
        SetManualVideoProcessingProperty(VideoProcAmpProperty.Brightness, brightness);
    }

    public int GetContrast()
    {
        return GetManualVideoProcessingProperty(VideoProcAmpProperty.Contrast);
    }

    public void SetContrast(int contrast)
    {
        SetManualVideoProcessingProperty(VideoProcAmpProperty.Contrast, contrast);
    }

    public int GetSaturation()
    {
        return GetManualVideoProcessingProperty(VideoProcAmpProperty.Saturation);
    }

    public void SetSaturation(int saturation)
    {
        SetManualVideoProcessingProperty(VideoProcAmpProperty.Saturation, saturation);
    }

    public int GetSharpness()
    {
        return GetManualVideoProcessingProperty(VideoProcAmpProperty.Sharpness);
    }

    public void SetSharpness(int sharpness)
    {
        SetManualVideoProcessingProperty(VideoProcAmpProperty.Sharpness, sharpness);
    }

    public int GetWhiteBalance()
    {
        return GetManualVideoProcessingProperty(VideoProcAmpProperty.WhiteBalance);
    }

    public void SetWhiteBalance(int whiteBalance)
    {
        SetManualVideoProcessingProperty(VideoProcAmpProperty.WhiteBalance, whiteBalance);
    }

    public int GetBackLightCompensation()
    {
        return GetManualVideoProcessingProperty(VideoProcAmpProperty.BacklightCompensation);
    }

    public void SetBackLightCompensation(int backLightCompensation)
    {
        SetManualVideoProcessingProperty(VideoProcAmpProperty.BacklightCompensation, backLightCompensation);
    }

    public int GetGain()
    {
        return GetManualVideoProcessingProperty(VideoProcAmpProperty.Gain);
    }

    public void SetGain(int gain)
    {
        SetManualVideoProcessingProperty(VideoProcAmpProperty.Gain, gain);
    }
}