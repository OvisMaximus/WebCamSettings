using System.Text;

namespace RestoreWebCamConfig;

    public record CameraDto(string Name)
    {
        public string Name { get; set; } = Name;
        public IList<CameraPropertyDto>? Properties { get; set; }

        public override string ToString()
        {
            var result = new StringBuilder("Camera ");
            var delimeter = "";
            result.Append(Name);
            result.Append(": ");
            foreach (var property in Properties)
            {
                result.Append(delimeter);
                result.Append(property.Name);
                result.Append("=");
                result.Append(property.Value);
                if (property.CanAdaptAutomatically)
                {
                    result.Append(" (");
                    result.Append(property.IsAutomaticallyAdapting ? "auto": "manual");
                    result.Append(')');
                }
                delimeter = ", ";
            }
            return result.ToString();
        }
    }
