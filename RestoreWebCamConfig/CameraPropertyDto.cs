namespace RestoreWebCamConfig;

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
    }
