namespace RestoreWebCamConfig;

internal abstract class DsProperty
{
    protected readonly CameraController CameraController;
    protected readonly string Name;
    protected readonly int PropertyId;
    protected bool CanAutoAdapt;
    protected int Default;
    protected bool IsAutomaticallyAdapting;
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

    protected abstract void SetAutomaticInternal(bool automatic);
    
    public void SetAutomatic(bool automatic)
    {
        if (!CanAdaptAutomatically())
        {
            if (automatic)
                throw new NotSupportedException($"{Name} can not adapt automatically");
            return;
        }

        var setting = automatic ? "automatic adapting." : "manual setting";
        Console.WriteLine($"Setting property {Name} to {setting}");
        Update();
        SetAutomaticInternal(automatic);
    }
    
    

    public int GetValue()
    {
        Update();
        return Value;
    }

    public bool IsAutomatic()
    {
        Update();
        return IsAutomaticallyAdapting;
    }

    public bool CanAdaptAutomatically()
    {
        return CanAutoAdapt;
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