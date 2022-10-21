namespace RestoreWebCamConfig.oldArch;

internal class VideoProcProperty : DsProperty
{
    internal static readonly string[] PropertyNamesById =
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
        "VideoProcAmp 10",
        "VideoProcAmp 11",
        "VideoProcAmp 12",
        "PowerLineFrequency",
        "VideoProcAmp 14",
        "VideoProcAmp 15",
        "VideoProcAmp 16",
        "VideoProcAmp 17",
        "VideoProcAmp 18",
        "VideoProcAmp 19",
        "VideoProcAmp 20"
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

    protected override void SetInternal(int value, bool automatic)
    {
        CameraController.SetVideoProcAmpProperty(PropertyId, value, automatic);
    }

    public override string ToString()
    {
        return $"VideoProcAmpProperty {base.ToString()}";
    }
}