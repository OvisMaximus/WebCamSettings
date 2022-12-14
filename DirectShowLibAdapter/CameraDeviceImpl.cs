using DirectShowLib;

namespace DirectShowLibAdapter;

public class CameraDeviceImpl : ICameraDevice
{
    private static readonly int MAX_PROPERTY_ID = 20;
    private readonly string _name;
    private readonly List<ICameraProperty> _properties;
    private readonly IControlAdapter _cameraControlAdapter;
    private readonly IControlAdapter _videoProcAmpAdapter;

    public CameraDeviceImpl(DsDevice videoInputDevice)
    {
        var id = typeof(IBaseFilter).GUID;
        var device = videoInputDevice ?? throw
            new ArgumentException("can not work without an device - it must not be null");
        _name = device.Name;
        device.Mon.BindToObject(null!, null, ref id, out var source);
        _cameraControlAdapter = new CameraControlAdapterImpl(source, _name);
        _videoProcAmpAdapter = new VideoProcAmpAdapterImpl(source, _name);
        _properties = FetchDeviceProperties();
    }

    private List<ICameraProperty> FetchDeviceProperties()
    {
        IEnumerable<ICameraProperty> FetchDsProperties(Func<int, ICameraProperty> dsPropertyFactoryById)
        {
            var properties = new List<ICameraProperty>();
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

        var propertiesList = new List<ICameraProperty>();
        propertiesList.AddRange(FetchDsProperties(id => 
            new DevicePropertyImpl(_cameraControlAdapter, id)));
        propertiesList.AddRange(FetchDsProperties(id =>
            new DevicePropertyImpl(_videoProcAmpAdapter, id)));
        return new List<ICameraProperty>(propertiesList.OrderBy(p => p.GetName()));
    }

    public string GetDeviceName()
    {
        return _name;
    }

    public ICameraProperty GetPropertyByName(string name)
    {
        return _properties.Find(p => name == p.GetName())
               ?? throw new InvalidDataException($"Device {_name} has a property without a name");
    }

    public IReadOnlyList<ICameraProperty> GetPropertiesList()
    {
        return _properties.AsReadOnly();
    }
}