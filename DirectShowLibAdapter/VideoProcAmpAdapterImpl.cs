using DirectShowLib;

namespace DirectShowLibAdapter;

internal class VideoProcAmpAdapterImpl : IControlAdapter
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
        "VideoProcAmp 10",
        "VideoProcAmp 11",
        "VideoProcAmp 12",
        "PowerLineFrequency"
    };
    private static readonly int FLAGS_AUTO = 0x1;
    private readonly IAMVideoProcAmp _videoProcAmp;

    public VideoProcAmpAdapterImpl(object source, string deviceName)
    {
        _videoProcAmp = source as IAMVideoProcAmp ?? throw
            new ArgumentException($"could not handle {deviceName} as video proc amp");    
    }

    public string GetPropertyName(int propertyId)
    {
        if (propertyId >= PropertyNamesById.Length)
            return $"VideoProcAmp {propertyId}";
        return PropertyNamesById[propertyId];
    }

    public int Update(int propertyId, out int value, out bool isAutomaticallyAdapting)
    {
        int returnCode = _videoProcAmp.Get((VideoProcAmpProperty)propertyId, out value, out var flags);
        isAutomaticallyAdapting = ((int)flags & FLAGS_AUTO) != 0;
        return returnCode;
    }

    public int GetPropertyMetaData(int propertyId, out bool canAutoAdapt, 
        out int defaultValue, out int minValue, out int maxValue, out int steppingDelta)
    {
        int returnCode =  _videoProcAmp.GetRange((VideoProcAmpProperty)propertyId, out minValue, out maxValue,
            out steppingDelta, out defaultValue, out var flags);
        canAutoAdapt = ((int)flags & FLAGS_AUTO) != 0;
        return returnCode;
    }

    public int SetPropertyState(int propertyId, int value, bool autoAdapt)
    {
        VideoProcAmpFlags flags = autoAdapt ? VideoProcAmpFlags.Auto : VideoProcAmpFlags.Manual; 
        return _videoProcAmp.Set((VideoProcAmpProperty)propertyId, value, flags);
    }
}