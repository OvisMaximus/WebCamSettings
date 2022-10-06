namespace RestoreWebCamConfig;

internal class CamControlProperty : DsProperty
{
    private static readonly string[] PropertyNamesById =
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

    public CamControlProperty(CameraController cameraController, int propertyId)
        : base(cameraController, propertyId, PropertyNamesById[propertyId])
    {
        var res = CameraController.GetCamControlPropertyRange(PropertyId, out MinValue, out MaxValue,
            out SteppingDelta, out Default, out CanAutoAdapt);
        if (res != 0)
            throw new InvalidOperationException(
                $"CameraControlProperty {PropertyId} is not supported by this device.");
        Update();
    }

    protected sealed override void Update()
    {
        CameraController.GetCamControlProperty(PropertyId, out Value, out IsAutomaticallyAdapting);
    }

    public override void SetValue(int value)
    {
        Console.WriteLine($"Setting camera control property {Name} to {value}");
        CameraController.SetCamControlProperty(PropertyId, value, IsAutomaticallyAdapting);
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
        CameraController.SetCamControlProperty(PropertyId, Value, IsAutomaticallyAdapting);
    }

    public override string ToString()
    {
        return $"CamControlProperty {base.ToString()}";
    }
}