using System.Text;

namespace RestoreWebCamConfig.CameraAdapter;

public record CameraDto(string Name)
{
    public string Name { get; set; } = Name;
    public IList<CameraPropertyDto>? Properties { get; set; }

    public override string ToString()
    {
        var result = new StringBuilder("Camera ");
        result.Append(Name);
        result.Append(": ");
        result.Append(Properties == null ? "no properties set." : RenderPropertyDescription(Properties));
        return result.ToString();
    }

    private string RenderPropertyDescription(IList<CameraPropertyDto> properties)
    {
        StringBuilder result = new StringBuilder();
        var delimiter = "";
        foreach (var property in properties)
        {
            result.Append(delimiter);
            result.Append(property.Name);
            result.Append("=");
            result.Append(property.Value);
            if (property.CanAdaptAutomatically)
            {
                result.Append(" (");
                result.Append(property.IsAutomaticallyAdapting ? "auto" : "manual");
                result.Append(')');
            }

            delimiter = ", ";
        }

        return result.ToString();
    }
}
