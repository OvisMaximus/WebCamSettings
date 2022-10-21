namespace RestoreWebCamConfig.oldArch;

internal class CamControlProperty : DsProperty
{
    internal static readonly string[] PropertyNamesById =
    {
        "Pan",
        "Tilt",
        "Roll",
        "Zoom",
        "Exposure",
        "Iris",
        "Focus",
        "CamControl 7",
        "CamControl 8",
        "CamControl 9",
        "CamControl 10",
        "CamControl 11",
        "CamControl 12",
        "CamControl 13",
        "CamControl 14",
        "CamControl 15",
        "CamControl 16",
        "CamControl 17",
        "CamControl 18",
        "LowLightCompensation",
        "CamControl 20"
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

    protected override void SetInternal(int value, bool automatic)
    { 
        CameraController.SetCamControlProperty(PropertyId, value, automatic);
    }

    public override string ToString()
    {
        return $"CamControlProperty {base.ToString()}";
    }
}