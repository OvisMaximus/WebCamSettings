using DirectShowLib;

namespace RestoreWebCamConfig;

public class CameraController
{
    private static readonly string[] VideoProcPropertyNamesById = new string[] {
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
        "Undocumented 20",
    };

    private static readonly string[] CameraControlPropertyNamesById = new string[]
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
        "Undocumented 20",
    };
    
    
    private readonly DsDevice _device;
    private readonly IAMCameraControl _cameraControl;
    private readonly IAMVideoProcAmp _videoProcAmp;

    abstract class DsProperty
    {
        private static readonly int FLAGS_AUTO = 0x1; 
        protected readonly CameraController CameraController;
        protected readonly int PropertyId;
        protected readonly string Name;
        protected int Value;
        protected int MinValue;
        protected int MaxValue;
        protected int SteppingDelta;
        protected int Default;
        private bool _canAdaptAutomatically;
        private bool _isAutomaticallyAdapting;

        protected DsProperty(CameraController cameraController, int propertyId, string name)
        {
            CameraController = cameraController;
            PropertyId = propertyId;
            Name = name;
        }

        protected abstract void Update();
        
        public abstract void SetValue(int value);

        public abstract void SetAutomatic(bool automatic);
        
        public int GetValue()
        {
            Update();
            return Value;
        }

        public bool IsAutomatic()
        {
            Update();
            return _isAutomaticallyAdapting;
        }

        protected void SetIsAutomaticallyAdaptingFromFlags(int flags)
        {
            _isAutomaticallyAdapting = (flags & FLAGS_AUTO) != 0;
        }

        public bool CanAdaptAutomatically()
        {
            return _canAdaptAutomatically;
        }

        protected void SetCanAdaptAutomaticallyFromFlags(int flags)
        {
            _canAdaptAutomatically = (flags & FLAGS_AUTO) != 0;
        }

        public int GetMinValue()
        {
            return MinValue;
        }
        public int GetMaxValue()
        {
            return MaxValue;
        }
        public string GetName()
        {
            return Name;
        }

        public override string ToString()
        {
            return
                $"{Name}, value={Value}, isAuto={IsAutomatic()}, min={MinValue}, max={MaxValue}, " +
                $"default={Default}, steppingDelta={SteppingDelta}, canAuto={CanAdaptAutomatically()}";
        }
    }

    class VideoProcProperty : DsProperty
    {
        public VideoProcProperty(CameraController cameraController, int propertyId) 
            : base(cameraController, propertyId, VideoProcPropertyNamesById[propertyId])
        {
            var res = CameraController._videoProcAmp.GetRange( TypedPropertyId(), out MinValue, out MaxValue,
                out SteppingDelta, out Default, out var flags);
            if (res != 0)
            {
                throw new InvalidOperationException(
                    $"VideoProcProperty {PropertyId} is not supported by this device.");
            }
            
            SetCanAdaptAutomaticallyFromFlags((int)flags);
            Update();
        }

        private VideoProcAmpProperty TypedPropertyId()
        {
            return (VideoProcAmpProperty)PropertyId;
        }
        
        protected sealed override void Update()
        {
            CameraController._videoProcAmp.Get(TypedPropertyId(), out Value, out var flags);
            SetIsAutomaticallyAdaptingFromFlags((int)flags);
        }

        public override void SetValue(int value)
        {
            Console.WriteLine($"Setting video processing parameter {Name} to {value}");
            CameraController._videoProcAmp.Set(TypedPropertyId(), value, AsProcAmpFlags(IsAutomatic()));
        }

        public override void SetAutomatic(bool automatic)
        {
            if (!CanAdaptAutomatically())
            {
                if (automatic)
                    throw new NotSupportedException($"{Name} can not adapt automatically");
                return;
            }

            var setting = automatic ? "automatic adapting." : "manual setting";
            Console.WriteLine($"Setting video processing parameter {Name} to {setting}");
            Update();
            CameraController._videoProcAmp.Set(TypedPropertyId(), Value, AsProcAmpFlags(automatic));
        }

        private VideoProcAmpFlags AsProcAmpFlags(bool automatic)
        {
            return automatic ? VideoProcAmpFlags.Auto : VideoProcAmpFlags.Manual;
        }

        public override string ToString()
        {
            return $"VideoProcAmpProperty {base.ToString()}";
        }

    }
    
    class CamControlProperty : DsProperty
    {
        public CamControlProperty(CameraController cameraController, int propertyId) 
            : base(cameraController, propertyId, CameraControlPropertyNamesById[propertyId])
        {
            var res = CameraController._cameraControl.GetRange(TypedPropertyId(), out MinValue, out MaxValue,
                out SteppingDelta, out Default, out var flags);
            if (res != 0)
            {
                throw new InvalidOperationException(
                    $"CameraControlProperty {PropertyId} is not supported by this device.");
            }

            SetCanAdaptAutomaticallyFromFlags((int)flags);
            Update();
        }

        private CameraControlProperty TypedPropertyId()
        {
            return (CameraControlProperty)PropertyId;
        }
        
        protected sealed override void Update()
        {
            CameraController._cameraControl.Get(TypedPropertyId(), out Value, out var flags);
            SetIsAutomaticallyAdaptingFromFlags((int)flags);
        }

        public override void SetValue(int value)
        {
            Console.WriteLine($"Setting camera control property {Name} to {value}");
            CameraController._cameraControl.Set(TypedPropertyId(), value, AsCameraControlFlags(IsAutomatic()));
        }

        public override void SetAutomatic(bool automatic)
        {
            if (!CanAdaptAutomatically())
            {
                if (automatic)
                    throw new NotSupportedException($"{Name} can not adapt automatically");
                return;
            }

            var setting = automatic ? "automatic adapting." : "manual setting";
            Console.WriteLine($"Setting video processing parameter {Name} to {setting}");
            Update();
            CameraController._cameraControl.Set(TypedPropertyId(), Value, AsCameraControlFlags(automatic));
        }

        private CameraControlFlags AsCameraControlFlags(bool automatic)
        {
            return automatic ? CameraControlFlags.Auto : CameraControlFlags.Manual;
        }

        public override string ToString()
        {
            return $"CamControlProperty {base.ToString()}";
        }

    }
    
    private CameraController(DsDevice videoInputDevice)
    {
        Guid id = typeof(IBaseFilter).GUID;
        _device = videoInputDevice ?? throw 
            new ArgumentException("can not work without an device - it must not be null");
			
        _device.Mon.BindToObject(null!, null, ref id, out var source);
        _cameraControl = source as IAMCameraControl ?? throw
            new ArgumentException($"could not handle {_device} as camera");
        _videoProcAmp = source as IAMVideoProcAmp ?? throw
            new ArgumentException($"could not handle {_device} as video proc amp");
        FetchDeviceProperties();
    }

    private void FetchDeviceProperties()
    {
        List<DsProperty>  FetchDsProperties(Func<int, DsProperty> dsPropertyFactory)
        {
            var properties = new List<DsProperty>();
            for (var i = 0; i < CameraControlPropertyNamesById.Length; i++)
            {
                try
                {
                    properties.Add(dsPropertyFactory(i));
                }
                catch (InvalidOperationException)
                {
                }
            }
            return properties;

        }

        var propertiesList = new List<DsProperty>(); 
        propertiesList.AddRange(FetchDsProperties(i => new VideoProcProperty(this, i)));
        propertiesList.AddRange(FetchDsProperties(i => new CamControlProperty(this, i)));
        foreach (var property in propertiesList)
        {
            Console.WriteLine(property);
        }
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

    public string GetName()
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