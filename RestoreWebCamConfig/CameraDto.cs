namespace RestoreWebCamConfig;

public partial class CameraController
{
    public record CameraDto(string Name)
    {
        public string Name { get; set; } = Name;
        public IList<CameraPropertyDto>? Properties { get; set; }
    }
}