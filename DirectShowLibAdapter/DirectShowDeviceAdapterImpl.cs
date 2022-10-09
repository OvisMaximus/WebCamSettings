using DirectShowLib;

namespace DirectShowLibAdapter;

public class DirectShowDeviceAdapterImpl : IDirectShowDevice
{
    public ICameraDevice GetCameraDeviceByName(string name)
    {
        if (name == null)
            throw new ArgumentNullException(nameof(name) + " must not be null");

        foreach (var ds in DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice))
            if (name.Equals(ds.Name))
                return new CameraDeviceImpl(ds);
        throw new FileNotFoundException($"Camera {name} not found.");
    }

    public IReadOnlyList<ICameraDevice> GetCameraDevicesList()
    {
        var res = new List<CameraDeviceImpl>();
        foreach (var ds in DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice))
            try
            {
                res.Add(new CameraDeviceImpl(ds));
            }
            catch (ArgumentException ae)
            {
                Console.WriteLine(ae.Message);
            }

        return res.AsReadOnly();
    }
}