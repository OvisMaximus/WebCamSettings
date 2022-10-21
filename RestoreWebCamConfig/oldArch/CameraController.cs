using DirectShowLib;

namespace RestoreWebCamConfig.oldArch;

public class CameraController
{
    private static readonly int FLAGS_AUTO = 0x1;
    private static readonly int MAX_PROPERTY_ID = 20;
    private readonly IAMCameraControl _cameraControl;
    private readonly DsDevice _device;
    private readonly List<DsProperty> _properties;
    private readonly IAMVideoProcAmp _videoProcAmp;


    private CameraController(DsDevice videoInputDevice)
    {
        var id = typeof(IBaseFilter).GUID;
        _device = videoInputDevice ?? throw
            new ArgumentException("can not work without an device - it must not be null");

        _device.Mon.BindToObject(null!, null, ref id, out var source);
        _cameraControl = source as IAMCameraControl ?? throw
            new ArgumentException($"could not handle {_device} as camera");
        _videoProcAmp = source as IAMVideoProcAmp ?? throw
            new ArgumentException($"could not handle {_device} as video proc amp");
        _properties = FetchDeviceProperties();
    }

    private List<DsProperty> FetchDeviceProperties()
    {
        IEnumerable<DsProperty> FetchDsProperties(Func<int, DsProperty> dsPropertyFactoryById)
        {
            var properties = new List<DsProperty>();
            for (var id = 0; id <= MAX_PROPERTY_ID; id++)
            {
                try
                {
                    properties.Add(dsPropertyFactoryById(id));
                }
                catch (InvalidOperationException)
                {
                }
            }
            return properties;
        }

        var propertiesList = new List<DsProperty>();
        propertiesList.AddRange(FetchDsProperties(id => new VideoProcProperty(this, id)));
        propertiesList.AddRange(FetchDsProperties(id => new CamControlProperty(this, id)));
        return new List<DsProperty>(propertiesList.OrderBy(p => p.GetName()));
    }

    public static List<string> GetKnownCameraNames()
    {
        var res = new List<string>();
        foreach (var ds in DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice)) res.Add(ds.Name);

        return res;
    }

    public static CameraController FindCamera(string? humanReadableCameraName)
    {
        if (humanReadableCameraName == null)
            throw new ArgumentNullException(nameof(humanReadableCameraName) + " must not be null");

        foreach (var ds in DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice))
            if (humanReadableCameraName.Equals(ds.Name))
                return new CameraController(ds);
        throw new FileNotFoundException($"Camera {humanReadableCameraName} not found.");
    }

    public CameraDto GetDeviceProperties()
    {
        var res = new CameraDto(_device.Name);
        var properties = new List<CameraPropertyDto>();
        _properties.ForEach(property => properties.Add(property.CreateDto()));
        res.Properties = properties.AsReadOnly();
        return res;
    }
    
    public void RestoreProperties(CameraDto camera)
    {
        if(camera.Name != _device.Name)
            throw new InvalidDataException(
                $"Camera name of record ({camera.Name}) does not match this device ({_device.Name})");
        var properties = camera.Properties 
                     ?? throw new InvalidDataException($"CameraDto.Properties of {_device.Name} is empty");
        foreach (var propertyDto in properties)
        {
            var property = GetPropertyByName(propertyDto.Name 
                     ?? throw new InvalidDataException("Property of {_device.Name} has no name."));
            if(property.CanAdaptAutomatically())
                property.SetAutomatic(propertyDto.IsAutomaticallyAdapting);
            property.SetValue(propertyDto.Value);
        }
    }

    private DsProperty GetPropertyByName(string propertyName)
    {
        return _properties.Find(p => propertyName == p.GetName())
               ?? throw new InvalidDataException($"Device {_device.Name} has a property without a name");
    }

    public int GetVideoProcAmpPropertyRange(int id, out int minValue, out int maxValue, out int steppingDelta, 
        out int defaultValue, out bool canAutoAdapt)
    {
        int resultCode =  _videoProcAmp.GetRange((VideoProcAmpProperty)id, out minValue, out maxValue,
            out steppingDelta, out defaultValue, out var flags);
        canAutoAdapt = ((int)flags & FLAGS_AUTO) != 0;
        return resultCode;
    }

    public void GetVideoProcAmpProperty(int propertyId, out int value, out bool isAutomatic)
    {
        int resultCode =  _videoProcAmp.Get((VideoProcAmpProperty)propertyId, out value, out var flags);
        isAutomatic = ((int)flags & FLAGS_AUTO) != 0;
        InterpretReturnCode(resultCode, VideoProcProperty.PropertyNamesById[propertyId]);
    }

    public void SetVideoProcAmpProperty(int propertyId, int value, bool isAutomatic)
    {
        VideoProcAmpFlags flags = isAutomatic ? VideoProcAmpFlags.Auto : VideoProcAmpFlags.Manual; 
        int resultCode =  _videoProcAmp.Set((VideoProcAmpProperty)propertyId, value, flags) ;
        InterpretReturnCode(resultCode, VideoProcProperty.PropertyNamesById[propertyId]);
    }

    public int GetCamControlPropertyRange(int id, out int minValue, out int maxValue, out int steppingDelta, 
        out int defaultValue, out bool canAutoAdapt)
    {
        int resultCode =  _cameraControl.GetRange((CameraControlProperty)id, out minValue, out maxValue,
            out steppingDelta, out defaultValue, out var flags);
        canAutoAdapt = ((int)flags & FLAGS_AUTO) != 0;
        return resultCode;
    }

    public void GetCamControlProperty(int propertyId, out int value, out bool isAutomatic)
    {
        int resultCode =  _cameraControl.Get((CameraControlProperty)propertyId, out value, out var flags);
        isAutomatic = ((int)flags & FLAGS_AUTO) != 0;
        InterpretReturnCode(resultCode, CamControlProperty.PropertyNamesById[propertyId]);
    }

    public void SetCamControlProperty(int propertyId, int value, bool isAutomatic)
    {
        CameraControlFlags flags = isAutomatic ? CameraControlFlags.Auto : CameraControlFlags.Manual;
        int resultCode =  _cameraControl.Set((CameraControlProperty)propertyId, value, flags) ;
        InterpretReturnCode(resultCode, CamControlProperty.PropertyNamesById[propertyId]);
    }

    private void InterpretReturnCode(int returnCode, string propertyName)
    {
        if (returnCode != 0)
        {
            throw new InvalidOperationException($"failed to access {propertyName}");
        }
    }


}