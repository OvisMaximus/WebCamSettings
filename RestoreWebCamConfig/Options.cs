namespace RestoreWebCamConfig;

internal record Options
{
    public string? CameraName { get; set; }
    public string? FileName { get; set; }
}