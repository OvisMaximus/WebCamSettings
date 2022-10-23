using DirectShowLibAdapter;

namespace RestoreWebCamConfig.CameraAdapter;

public class CameraDevice
{
    private readonly ICameraDevice _cameraDevice;

    internal CameraDevice(ICameraDevice cameraDevice)
    {
        _cameraDevice = cameraDevice;
    }

    public string GetDeviceName()
    {
        return _cameraDevice.GetDeviceName();
    }

    public IReadOnlyList<CameraProperty> GetPropertiesList()
    {
        List<CameraProperty> result = new List<CameraProperty>();
        foreach (var property in _cameraDevice.GetPropertiesList())
        {
            result.Add(new CameraProperty(property));
        }
        return result.AsReadOnly();
    }

    public CameraProperty GetPropertyByName(string propertyName)
    {
        return new CameraProperty(_cameraDevice.GetPropertyByName(propertyName));
    }

    public CameraDto GetDto()
    {
        var dto = new CameraDto(GetDeviceName())
        {
            Properties = GetPropertyDtoList()
        };
        return dto;
    }

    private IList<CameraPropertyDto> GetPropertyDtoList()
    {
        var dtoList = new List<CameraPropertyDto>();
        var propertiesList = _cameraDevice.GetPropertiesList();
        foreach (var property in propertiesList)
        {
            dtoList.Add(new CameraProperty(property).GetDto());
        }
        return dtoList;
    }

    public void RestoreCameraDto(CameraDto dto)
    {
        if (GetDeviceName() != dto.Name)
            throw new InvalidDataException($"Device {GetDeviceName()} can not use data for '{dto.Name}'");
        var propertyDtoList = dto.Properties;

        RestoreProperties(ValidatePropertiesList(dto.Name, propertyDtoList));
    }

    private void RestoreProperties(IList<CameraPropertyDto> propertyDtoList)
    {
        foreach (var propertyDto in propertyDtoList)
        {
            var name = propertyDto.Name;
            var property = GetPropertyByName(name);
            property.SetValue(propertyDto.Value);
            property.SetAdaptAutomatically(propertyDto.IsAutomaticallyAdapting);
        }
    }

    private static IList<CameraPropertyDto> ValidatePropertiesList(String camName, IList<CameraPropertyDto>? propertyDtoList)
    {
        if (propertyDtoList == null)
            throw new InvalidDataException($"Property list of {camName} is null, can not restore anything.");
        if (propertyDtoList.Count == 0)
            throw new InvalidDataException($"Property list of {camName} is empty, can not restore anything.");
        return propertyDtoList;
    }
}