using DirectShowLib;

namespace RestoreWebCamConfig;

public class CameraController
{
    private readonly DsDevice _device;
    private readonly IAMCameraControl _cameraControl;
    private readonly IAMVideoProcAmp _videoProcAmp;
		
    private CameraController(DsDevice videoInputDevice)
    {
        Guid iid = typeof(IBaseFilter).GUID;
        _device = videoInputDevice ?? throw 
            new ArgumentException("can not work without an device - it must not be null");
			
        _device.Mon.BindToObject(null!, null, ref iid, out var source);
        _cameraControl = source as IAMCameraControl ?? throw
            new ArgumentException($"could not handle {_device} as camera");
        _videoProcAmp = source as IAMVideoProcAmp ?? throw
            new ArgumentException($"could not handle {_device} as video proc amp");
    }

    public static List<string> GetKnownCameraNames()
    {
        var res = new List<string>();
        foreach (DsDevice ds in DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice))
        {
            res.Add(ds.Name);
        }

        return res;
    }

    public static CameraController FindCamera(string? humanReadableCameraName)
    {
        if (humanReadableCameraName == null)
            throw new ArgumentNullException(nameof(humanReadableCameraName) + " must not be null");

        foreach (DsDevice ds in DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice))
        {
            if (humanReadableCameraName.Equals(ds.Name))
            {
                return new CameraController(ds);
            }
        }
        throw new FileNotFoundException($"Camera {humanReadableCameraName} not found.");
    }

    public string getName()
    {
        return _device.Name;
    }

    private int GetManualCamControl(CameraControlProperty propertyId)
    {
        int res = _cameraControl.Get(propertyId, out var value, out var _);
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
        int res = _videoProcAmp.Get(propertyId,out var value, out _);
        Console.WriteLine($"Got video amplification parameter {propertyId} as {value}. Return code is {res}");
        return value;
    }
    private void SetManualVideoProcessingProperty(VideoProcAmpProperty property, int value)
    {
        Console.WriteLine($"Setting video processing parameter {property} to {value}");
        _videoProcAmp.Set(property, value, VideoProcAmpFlags.Manual);
    }

    public PowerlineFrequency GetPowerLineFrequency()
    {
        int value = GetManualVideoProcessingProperty((VideoProcAmpProperty)13);
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

    public Boolean GetLowLightCompensation()
    {
        int res = GetManualCamControl((CameraControlProperty)19);
        return res != 0;
    }

    public void SetLowLightCompensation(Boolean onOff)
    {
        int lowLightCompensationValue = onOff ? 1 : 0;
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