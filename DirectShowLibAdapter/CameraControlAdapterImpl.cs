using DirectShowLib;

namespace DirectShowLibAdapter;

internal class CameraControlAdapterImpl : IControlAdapter
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
    };    
    private static readonly int FLAGS_AUTO = 0x1;
    private readonly IAMCameraControl _cameraControl;

    internal CameraControlAdapterImpl(object source, string deviceName)
    {
        _cameraControl = source as IAMCameraControl ?? throw
            new ArgumentException($"could not handle {deviceName} as camera");
    }

    public string GetControlTypeName()
    {
        return "CameraControl";
    }

    public string GetPropertyName(int propertyId)
    {
        if (propertyId >= PropertyNamesById.Length)
            return $"CamControl {propertyId}";
        return PropertyNamesById[propertyId];
    }

    public int Update(int propertyId, out int value, out bool isAutomaticallyAdapting)
    {
        int returnCode =  _cameraControl.Get((CameraControlProperty)propertyId, out value, out var flags);
        isAutomaticallyAdapting = ((int)flags & FLAGS_AUTO) != 0;
        return returnCode;
    }

    public int GetPropertyMetaData(int propertyId, out bool canAutoAdapt, 
        out int defaultValue, out int minValue, out int maxValue, out int steppingDelta)
    {
        int resultCode =  _cameraControl.GetRange((CameraControlProperty)propertyId, out minValue, out maxValue,
            out steppingDelta, out defaultValue, out var flags);
        canAutoAdapt = ((int)flags & FLAGS_AUTO) != 0;
        return resultCode;
    }

    public int SetPropertyState(int propertyId, int value, bool autoAdapt)
    {
        CameraControlFlags flags = autoAdapt ? CameraControlFlags.Auto : CameraControlFlags.Manual;
        return _cameraControl.Set((CameraControlProperty)propertyId, value, flags) ;
    }
}