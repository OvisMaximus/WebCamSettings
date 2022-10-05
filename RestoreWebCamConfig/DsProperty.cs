using DirectShowLib;

namespace RestoreWebCamConfig;

public partial class CameraController
{
    private class CamControlProperty : DsProperty
    {
        public CamControlProperty(CameraController cameraController, int propertyId)
            : base(cameraController, propertyId, CameraControlPropertyNamesById[propertyId])
        {
            var res = CameraController._cameraControl.GetRange(TypedPropertyId(), out MinValue, out MaxValue,
                out SteppingDelta, out Default, out var flags);
            if (res != 0)
                throw new InvalidOperationException(
                    $"CameraControlProperty {PropertyId} is not supported by this device.");

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
            Console.WriteLine($"Setting camera control property {Name} to {setting}");
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
}

public partial class CameraController
{
    private class VideoProcProperty : DsProperty
    {
        public VideoProcProperty(CameraController cameraController, int propertyId)
            : base(cameraController, propertyId, VideoProcPropertyNamesById[propertyId])
        {
            var res = CameraController._videoProcAmp.GetRange(TypedPropertyId(), out MinValue, out MaxValue,
                out SteppingDelta, out Default, out var flags);
            if (res != 0)
                throw new InvalidOperationException(
                    $"VideoProcProperty {PropertyId} is not supported by this device.");

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
            Console.WriteLine($"Setting video processing property {Name} to {value}");
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
            Console.WriteLine($"Setting video processing property {Name} to {setting}");
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
}

public abstract class DsProperty
{
    private static readonly int FLAGS_AUTO = 0x1;
    protected readonly CameraController CameraController;
    protected readonly string Name;
    protected readonly int PropertyId;
    private bool _canAdaptAutomatically;
    private bool _isAutomaticallyAdapting;
    protected int Default;
    protected int MaxValue;
    protected int MinValue;
    protected int SteppingDelta;
    protected int Value;

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

    public int GetSteppingDelta()
    {
        return SteppingDelta;
    }

    public override string ToString()
    {
        return
            $"{Name}, value={Value}, isAuto={IsAutomatic()}, min={MinValue}, max={MaxValue}, " +
            $"default={Default}, steppingDelta={SteppingDelta}, canAuto={CanAdaptAutomatically()}";
    }

    public int GetDefault()
    {
        return Default;
    }
    public CameraPropertyDto CreateDto()
    {
        var res = new CameraPropertyDto
        {
            Name = GetName(),
            IsAutomaticallyAdapting = IsAutomatic(),
            Value = GetValue(),
            MinValue = GetMinValue(),
            MaxValue = GetMaxValue(),
            Default = GetDefault(),
            SteppingDelta = GetSteppingDelta(),
            CanAdaptAutomatically = CanAdaptAutomatically()
        };
        return res;
    }

}