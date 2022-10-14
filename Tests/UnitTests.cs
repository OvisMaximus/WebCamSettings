using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DirectShowLibAdapter;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using RestoreWebCamConfig;
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
        new(PropertyNameBrightness, 129,1,255,128,1,false, false),
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
        }
        camera.GetPropertiesList().Returns(propertiesList.AsReadOnly());

        camera.GetPropertyByName(InvalidDeviceName).Throws<InvalidDataException>();
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

        IReadOnlyList<CameraProperty> propertyList = camera.GetPropertiesList();
        Assert.NotNull(propertyList);
        Assert.NotEmpty(propertyList);
    }

    [Fact]
    public void TestPropertyInformation()
    {
        var cameraManager = new CameraManager(_dsDevice);
        var camera = cameraManager.GetCameraByName(CamNameCamOne);

        Assert.Throws<InvalidDataException>(() => camera.GetPropertyByName(InvalidDeviceName));

        foreach (var expected in _cam1Properties)
        {
            var property = camera.GetPropertyByName(expected.Name);
            AssertCameraPropertyValues(expected, property);
        }
    }

    private void AssertCameraPropertyValues(PropertyTestData expected, CameraProperty property)
    {
        Assert.NotNull(property);
        Assert.Equal(expected.Name, property.GetName());
        Assert.Equal(expected.Value, property.GetValue());
        Assert.Equal(expected.Min, property.GetMinValue());
        Assert.Equal(expected.Max, property.GetMaxValue());
        Assert.Equal(expected.Default, property.GetDefaultValue());
        Assert.Equal(expected.Delta, property.GetIncrementSize());
        Assert.Equal(expected.CanAuto, property.HasAutoAdaptCapability());
        Assert.Equal(expected.IsAuto, property.IsAutomaticallyAdapting());
    }

    [Fact]
    public void TestPropertyModification()
    {
        var cameraManager = new CameraManager(_dsDevice);
        var camera = cameraManager.GetCameraByName(CamNameCamOne);
        var property = camera.GetPropertyByName(PropertyNameBrightness);
        var testDouble = 
            _dsDevice.GetCameraDeviceByName(CamNameCamOne).GetPropertyByName(PropertyNameBrightness);

        property.SetValue(3);
        testDouble.Received(1).SetValue(3);

        property.SetAdaptAutomatically(true);
        testDouble.Received(1).SetAutoAdapt(true);
    }

    [Fact]
    public void TestGetPropertyDto()
    {
        var cameraManager = new CameraManager(_dsDevice);
        var camera = cameraManager.GetCameraByName(CamNameCamOne);
        
        foreach (var expected in _cam1Properties)
        {
            var property = camera.GetPropertyByName(expected.Name);
            var propertyDto = property.GetDto();
            AssertPropertyDtoValues(expected, propertyDto);
        }
    }

    private void AssertPropertyDtoValues(PropertyTestData expected, CameraPropertyDto dto)
    {
        Assert.NotNull(dto.Name);
        Assert.Equal(expected.Name, dto.Name);
        Assert.Equal(expected.Value, dto.Value);
        Assert.Equal(expected.Min, dto.MinValue);
        Assert.Equal(expected.Max, dto.MaxValue);
        Assert.Equal(expected.Default, dto.Default);
        Assert.Equal(expected.Delta, dto.SteppingDelta);
        Assert.Equal(expected.CanAuto, dto.CanAdaptAutomatically);
        Assert.Equal(expected.IsAuto, dto.IsAutomaticallyAdapting);
    }

    [Fact]
    public void TestRestorePropertyFromDto()
    {
        var cameraManager = new CameraManager(_dsDevice);
        var camera = cameraManager.GetCameraByName(CamNameCamOne);
        var property = camera.GetPropertyByName(PropertyNameBrightness);
        var testDouble = 
            _dsDevice.GetCameraDeviceByName(CamNameCamOne).GetPropertyByName(PropertyNameBrightness);
        var dto = property.GetDto();

        property.RestoreFromDto(dto);
        
        testDouble.Received(1).SetValue(dto.Value);
        testDouble.Received(1).SetAutoAdapt(dto.IsAutomaticallyAdapting);
    }

    [Fact]
    public void TestDenyRestorePropertyFromDtoWhenNameIsNotValid()
    {
        var cameraManager = new CameraManager(_dsDevice);
        var camera = cameraManager.GetCameraByName(CamNameCamOne);
        var property = camera.GetPropertyByName(PropertyNameBrightness);
        var dto = property.GetDto();

        Assert.Throws<ArgumentNullException>(() => property.RestoreFromDto(null!));
        dto.Name = null;
        Assert.Throws<InvalidDataException>(() => property.RestoreFromDto(dto));
        dto.Name = InvalidDeviceName;
        Assert.Throws<InvalidDataException>(() => property.RestoreFromDto(dto));
    }

    [Fact]
    public void TestGetCameraDto()
    {
        var cameraManager = new CameraManager(_dsDevice);
        var camera = cameraManager.GetCameraByName(CamNameCamOne);
        var dto = camera.GetDto();
        
        Assert.NotNull(dto);
        Assert.Equal(CamNameCamOne, dto.Name);

        var propertyDtoList = dto.Properties;
        Assert.NotNull(propertyDtoList);
        Assert.NotEmpty(propertyDtoList);
        Assert.Contains(propertyDtoList, propertyDto => PropertyNameBrightness == propertyDto.Name);
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

    public int GetValue()
    {
        return _dsProperty.GetValue();
    }

    public int GetMinValue()
    {
        return _dsProperty.GetMinValue();
    }

    public int GetMaxValue()
    {
        return _dsProperty.GetMaxValue();
    }

    public int GetDefaultValue()
    {
        return _dsProperty.GetDefaultValue();
    }

    public int GetIncrementSize()
    {
        return _dsProperty.GetValueIncrementSize();
    }

    public bool HasAutoAdaptCapability()
    {
        return _dsProperty.HasAutoAdaptCapability();
    }

    public bool IsAutomaticallyAdapting()
    {
        return _dsProperty.IsAutoAdapt();
    }

    public void SetValue(int newValue)
    {
        _dsProperty.SetValue(newValue);
    }

    public void SetAdaptAutomatically(bool adaptAutomatically)
    {
        _dsProperty.SetAutoAdapt(adaptAutomatically);
    }

    public CameraPropertyDto GetDto()
    {
        var dto = new CameraPropertyDto
        {
            Name = GetName(),
            Value = GetValue(),
            MinValue = GetMinValue(),
            MaxValue = GetMaxValue(),
            Default = GetDefaultValue(),
            SteppingDelta = GetIncrementSize(),
            CanAdaptAutomatically = HasAutoAdaptCapability(),
            IsAutomaticallyAdapting = IsAutomaticallyAdapting()
        };
        return dto;
    }

    public void RestoreFromDto(CameraPropertyDto dto)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));
        if (null == dto.Name || dto.Name.Trim().Length == 0) 
            throw new InvalidDataException("Can not restore a property without a name.");
        if(_dsProperty.GetName() != dto.Name)
            throw new InvalidDataException($"Can not restore {_dsProperty.GetName()} with data of {dto.Name}.");
            
        SetValue(dto.Value);
        SetAdaptAutomatically(dto.IsAutomaticallyAdapting);
    }
}

