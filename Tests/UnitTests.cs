using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DirectShowLibAdapter;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Tests;

public class UnitTests
{
    private const string CamNameCamOne = "Fan Corp Face Cam SD";
    private const string CamNameCamTwo = "Simple Motion Picture Sensor";
    private const string InvalidDeviceName = "Blind eye devices HD Pro";
    private const string PropertyNameBrightness = "Brightness";
    private const string PropertyNameExposure = "Exposure";
    private const string PropertyNameFocus = "Focus";
    private readonly IDirectShowDevice _dsDevice;
    private readonly IDirectShowDevice _dsDeviceNoCams;

    private readonly PropertyTestData[] _cam1Properties = {
        new(PropertyNameBrightness, 129,0,255,128,1,false, false),
        new(PropertyNameExposure, -5, -2,-11,-6,1,true,  false),
        new(PropertyNameFocus, 0,0,255,25,5,true,  true)
    };
    
    private record PropertyTestData ()
    {
        internal readonly string Name;
        internal readonly int Value, Min, Max, Default, Delta;
        internal readonly bool CanAuto, IsAuto;

        public PropertyTestData(
            string name, int value, int min, int max, int @default, int delta, bool canAuto, bool isAuto): this()
        {
            Name = name;
            Value = value;
            Min = min;
            Max = max;
            Default = @default;
            Delta = delta;
            IsAuto = isAuto;
            CanAuto = canAuto;
        }
    }
    
    public UnitTests()
    {
        _dsDevice = Substitute.For<IDirectShowDevice>();
        var dsCamera1 = BuildCameraDeviceSubstitute(_dsDevice, CamNameCamOne, _cam1Properties);
        var dsCamera2 = BuildCameraDeviceSubstitute(_dsDevice, CamNameCamTwo, _cam1Properties);
        var deviceList = new List<ICameraDevice> { dsCamera1, dsCamera2 };
        _dsDevice.GetCameraDevicesList().Returns(deviceList.AsReadOnly());

        _dsDeviceNoCams = Substitute.For<IDirectShowDevice>();
        _dsDeviceNoCams.GetCameraDevicesList().Returns(new List<ICameraDevice>().AsReadOnly());
 
        _dsDevice.GetCameraDeviceByName(InvalidDeviceName).Throws(new FileNotFoundException());
    }

    private ICameraDevice BuildCameraDeviceSubstitute(IDirectShowDevice device, string name,
        PropertyTestData[] propertyList)
    {
        var camera = Substitute.For<ICameraDevice>();
        camera.GetDeviceName().Returns(name);
        device.GetCameraDeviceByName(name).Returns(camera);
    
        BuildProperties(camera, propertyList);

        return camera;
    }

    private void BuildProperties(ICameraDevice camera, PropertyTestData[] propertyList)
    {
        var propertiesList = new List<ICameraProperty>();
        foreach(var property in propertyList) {
            propertiesList.Add(BuildCameraPropertySubstitute(camera, property));    
        };
        camera.GetPropertiesList().Returns(propertiesList.AsReadOnly());
    }

    private ICameraProperty BuildCameraPropertySubstitute(ICameraDevice camera, PropertyTestData testData)
    {
        var property = Substitute.For<ICameraProperty>();
        property.GetName().Returns(testData.Name);
        property.GetValue().Returns(testData.Value);
        property.GetMinValue().Returns(testData.Min);
        property.GetMaxValue().Returns(testData.Max);
        property.GetDefaultValue().Returns(testData.Default);
        property.GetValueIncrementSize().Returns(testData.Delta);
        property.HasAutoAdaptCapability().Returns(testData.CanAuto);
        property.IsAutoAdapt().Returns(testData.IsAuto);
        camera.GetPropertyByName(testData.Name).Returns(property);
        return property;
    }


    [Fact]
    public void TestListOfCameraNames()
    {
        var cameraManager = new CameraManager(_dsDevice);
        IReadOnlyList<string> cameraNames = cameraManager.GetListOfAvailableCameraNames();
        Assert.NotNull(cameraNames);
        Assert.NotEmpty(cameraNames);
        Assert.Contains(CamNameCamOne, cameraNames);
        Assert.Contains(CamNameCamTwo, cameraNames);

        cameraManager = new CameraManager(_dsDeviceNoCams);
        cameraNames = cameraManager.GetListOfAvailableCameraNames();
        Assert.NotNull(cameraNames);
        Assert.Empty(cameraNames);
    }
    
    [Fact]
    public void TestListOfCameras()
    {
        var cameraManager = new CameraManager(_dsDevice);
        IReadOnlyList<CameraDevice> listOfAvailableCameras = cameraManager.GetListOfAvailableCameras();
        Assert.NotNull(listOfAvailableCameras);
        Assert.NotEmpty(listOfAvailableCameras);
        Assert.NotNull(listOfAvailableCameras.First(c => c.GetDeviceName().Equals(CamNameCamOne)));
        Assert.NotNull(listOfAvailableCameras.First(c => c.GetDeviceName().Equals(CamNameCamTwo)));
        
        cameraManager = new CameraManager(_dsDeviceNoCams);
        listOfAvailableCameras = cameraManager.GetListOfAvailableCameras();
        Assert.NotNull(listOfAvailableCameras);
        Assert.Empty(listOfAvailableCameras);
    }

    [Fact]
    public void TestCameraInformation()
    {
        var cameraManager = new CameraManager(_dsDevice);
        Assert.Throws<FileNotFoundException>(
            () => { cameraManager.GetCameraByName(InvalidDeviceName); });

        CameraDevice camera = cameraManager.GetCameraByName(CamNameCamOne);
        Assert.NotNull(camera);
        Assert.Equal(CamNameCamOne, camera.GetDeviceName());

        CameraProperty property = camera.GetPropertyByName(PropertyNameFocus);
        Assert.NotNull(property);
        Assert.Equal(PropertyNameFocus, property.GetName());
        
        property = camera.GetPropertyByName(PropertyNameBrightness);
        Assert.NotNull(property);
        Assert.Equal(PropertyNameBrightness, property.GetName());
    }

}

public class CameraProperty
{
    private readonly ICameraProperty _dsProperty;

    internal CameraProperty(ICameraProperty dsProperty)
    {
        _dsProperty = dsProperty;
    }

    public string GetName()
    {
        return _dsProperty.GetName();
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

    public object GetPropertiesList()
    {
        throw new NotImplementedException();
    }

    public CameraProperty GetPropertyByName(string propertyName)
    {
        return new CameraProperty(_cameraDevice.GetPropertyByName(propertyName));
    }
}

