using System.Collections.Generic;
using DirectShowLibAdapter;
using NSubstitute;
using Xunit;

namespace Tests;

public class UnitTests
{
    private static readonly string NASE = "Nase";
    private static readonly string BOHREN = "Bohren";
    private readonly IDirectShowDevice _dsDevice;
    private readonly ICameraDevice _dsCamera1;
    private readonly ICameraDevice _dsCamera2;
    private readonly IDirectShowDevice _dsDeviceNoCams;

    public UnitTests()
    {
        _dsDevice = Substitute.For<IDirectShowDevice>();
        _dsCamera1 = Substitute.For<ICameraDevice>();
        _dsCamera2 = Substitute.For<ICameraDevice>();
        var deviceList = new List<ICameraDevice>() { _dsCamera1, _dsCamera2 };
        _dsDevice.GetCameraDevicesList().Returns(deviceList.AsReadOnly());
        _dsCamera1.GetDeviceName().Returns(NASE);
        _dsCamera2.GetDeviceName().Returns(BOHREN);

        _dsDeviceNoCams = Substitute.For<IDirectShowDevice>();
        _dsDeviceNoCams.GetCameraDevicesList().Returns(new List<ICameraDevice>().AsReadOnly());
    }
    
    [Fact]
    public void TestListOfCameraNames()
    {
        var cameraManager = new CameraManager(_dsDevice);
        IReadOnlyList<string> cameraNames = cameraManager.GetListOfAvailableCameraNames();
        Assert.NotNull(cameraNames);
        Assert.NotEmpty(cameraNames);
        Assert.Contains(NASE, cameraNames);
        Assert.Contains(BOHREN, cameraNames);

        cameraManager = new CameraManager(_dsDeviceNoCams);
        cameraNames = cameraManager.GetListOfAvailableCameraNames();
        Assert.NotNull(cameraNames);
        Assert.Empty(cameraNames);
    }
    
    [Fact]
    public void TestListOfCameras()
    {
        var cameraManager = new CameraManager(_dsDevice);
        IReadOnlyList<ICameraDevice> cameras = cameraManager.GetListOfAvailableCameras();
        Assert.NotNull(cameras);
        Assert.NotEmpty(cameras);
        Assert.Contains(_dsCamera1, cameras);
        Assert.Contains(_dsCamera2, cameras);

        cameraManager = new CameraManager(_dsDeviceNoCams);
        cameras = cameraManager.GetListOfAvailableCameras();
        Assert.NotNull(cameras);
        Assert.Empty(cameras);
    }

}

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

    public IReadOnlyList<ICameraDevice> GetListOfAvailableCameras()
    {
        return _dsDevice.GetCameraDevicesList();
    }
}