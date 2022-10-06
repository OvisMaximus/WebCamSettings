namespace RestoreWebCamConfig;

internal class VideoProcProperty : DsProperty
{
    private static readonly string[] PropertyNamesById =
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

    public VideoProcProperty(CameraController cameraController, int propertyId)
        : base(cameraController, propertyId, PropertyNamesById[propertyId])
    {
        var res = cameraController.GetVideoProcAmpPropertyRange(propertyId, out MinValue, out MaxValue,
            out SteppingDelta, out Default, out CanAutoAdapt);
        if (res != 0)
            throw new InvalidOperationException(
                $"VideoProcProperty {PropertyId} is not supported by this device.");

        Update();
    }

    protected sealed override void Update()
    {
        CameraController.GetVideoProcAmpProperty(PropertyId, out Value, out IsAutomaticallyAdapting);
    }

    public override void SetValue(int value)
    {
        CameraController.SetVideoProcAmpProperty(PropertyId, value, IsAutomatic());
    }

    protected override void SetAutomaticInternal(bool automatic)
    {
        CameraController.SetVideoProcAmpProperty(PropertyId, Value, automatic);
    }

    public override string ToString()
    {
        return $"VideoProcAmpProperty {base.ToString()}";
    }
}