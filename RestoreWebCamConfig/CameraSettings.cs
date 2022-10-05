namespace RestoreWebCamConfig;

public class CameraSettings
{
    public string? CameraName { get; set; }
    public int ManualZoom { get; set; }
    public int ManualFocus { get; set; }
    public int Exposure { get; set; }
    public int Pan { get; set; }
    public int Tilt { get; set; }
    public int Brightness { get; set; }
    public int Contrast { get; set; }
    public int Saturation { get; set; }
    public int Sharpness { get; set; }
    public int WhiteBalance { get; set; }
    public int BackLightCompensation { get; set; }
    public int Gain { get; set; }
    public PowerlineFrequency PowerlineFrequency { get; set; }
    public bool LowLightCompensation { get; set; }
}