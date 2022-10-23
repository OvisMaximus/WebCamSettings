using DirectShowLibAdapter;

namespace RestoreWebCamConfig.CameraAdapter;

public class CameraManager
{
    private readonly IDirectShowDevice _dsDevice;

    public CameraManager(IDirectShowDevice dsDevice)
    {
        _dsDevice = dsDevice;
    }

    public IReadOnlyList<string> GetListOfAvailableCameraNames()
    {
        var result = new List<string>();
        foreach (var cameraDevice in _dsDevice.GetCameraDevicesList())
        {
            result.Add(cameraDevice.GetDeviceName());
        }
        return result.AsReadOnly();
    }

    public IReadOnlyList<CameraDevice> GetListOfAvailableCameras()
    {
        var lowLevelCameraDevices = _dsDevice.GetCameraDevicesList();
        List<CameraDevice> result = new List<CameraDevice>();
        foreach (ICameraDevice device in lowLevelCameraDevices)
        {
            result.Add(new CameraDevice(device));
        }
        return result.AsReadOnly();
    }

    public CameraDevice GetCameraByName(string cameraName)
    {
        return new CameraDevice(_dsDevice.GetCameraDeviceByName(cameraName));
    }
}