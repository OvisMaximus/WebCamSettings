namespace RestoreWebCamConfig;

public partial class CameraController
{
    public record CameraPropertyDto
    {
        public string? Name { get; set; }
        public int Value { get; set; }
        public bool IsAutomaticallyAdapting { get; set; }
        public int MinValue { get; set; }
        public int MaxValue { get; set; }
        public int Default { get; set; }
        public int SteppingDelta { get; set; }
        public bool CanAdaptAutomatically { get; set; }

        public static CameraPropertyDto CreateDtoFromDsProperty(DsProperty property)
        {
            var res = new CameraPropertyDto
            {
                Name = property.GetName(),
                IsAutomaticallyAdapting = property.IsAutomatic(),
                Value = property.GetValue(),
                MinValue = property.GetMinValue(),
                MaxValue = property.GetMaxValue(),
                Default = property.GetDefault(),
                SteppingDelta = property.GetSteppingDelta(),
                CanAdaptAutomatically = property.CanAdaptAutomatically()
            };
            return res;
        }
    }
}