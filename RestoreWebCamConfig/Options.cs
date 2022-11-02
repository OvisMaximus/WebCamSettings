namespace RestoreWebCamConfig;

public record Options
{
    public string? CameraName { get; set; }
    public string? FileName { get; set; }
    public bool IsHelpRequested { get; set; }
}