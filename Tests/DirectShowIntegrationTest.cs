using System;
using System.Linq;
using Xunit;
using DirectShowLibAdapter;

namespace Tests;

public class DirectShowIntegrationTest
{
    [Fact]
    public void contain_Logitech_BRIO_in_cam_list()
    {
        var dsAdapter = new DirectShowDeviceAdapterImpl();
        var cameraList =  dsAdapter.GetCameraDevicesList();
        Assert.NotNull(cameraList);
        Assert.NotEmpty(cameraList);
        Assert.NotNull(cameraList.First(c => c.GetDeviceName() == "Logitech BRIO"));
    }


    [Theory]
    [InlineData("Logitech BRIO", "Focus", true, false, 0, 255, 0, 0, 5)]
    public void CameraProperties(string camName, string propertyName, bool canAuto, bool isAuto, int value, int max, int min,
        int defaultVal, int stepSize)
    {
        var cam = new DirectShowDeviceAdapterImpl().GetCameraDeviceByName(camName);
        Assert.Equal(camName,cam.GetDeviceName());
        var cameraProperty = cam.GetPropertyByName(propertyName);
        Assert.NotNull(cameraProperty);
        Assert.Equal(canAuto,cameraProperty.HasAutoAdaptCapability());
        Assert.Equal(isAuto,cameraProperty.IsAutoAdapt());
        Assert.Equal(value, cameraProperty.GetValue());
        Assert.Equal(max, cameraProperty.GetMaxValue());
        Assert.Equal(min, cameraProperty.GetMinValue());
        Assert.Equal(defaultVal, cameraProperty.GetDefaultValue());
        Assert.Equal(stepSize, cameraProperty.GetValueIncrementSize());
    }

    [Theory]
    [InlineData("Logitech BRIO", 14)]
    [InlineData("HD Pro Webcam C920", 14)]
    public void NumberOfSupportedProperties(string camName, int numberOfProps)
    {
        var cam = new DirectShowDeviceAdapterImpl().GetCameraDeviceByName(camName);
        var props = cam.GetPropertiesList();
        Assert.Equal(numberOfProps, props.Count);
    }

    [Fact]
    public void CamListShowsTwoCams()
    {
        var cams = new DirectShowDeviceAdapterImpl().GetCameraDevicesList();

        Assert.Equal(2, cams.Count);
    }

    [Fact]
    public void CanSwitchAutoToggleOfAProperty()
    {
        var cams = new DirectShowDeviceAdapterImpl().GetCameraDevicesList();
        var cut = cams.First();
        var prop = cut.GetPropertiesList().First(p => p.HasAutoAdaptCapability());
        bool currentState = prop.IsAutoAdapt();
        prop.SetAutoAdapt(! currentState);
        Assert.Equal(! currentState, prop.IsAutoAdapt());
        prop.SetAutoAdapt(currentState);
    }

    [Fact]
    public void CanChangeValueOfRandomProp()
    {
        var cams = new DirectShowDeviceAdapterImpl().GetCameraDevicesList();
        var cut = cams.Last();
        var prop = cut.GetPropertiesList().First();
        int currentValue = prop.GetValue();
        int min = prop.GetMinValue();
        int max = prop.GetMaxValue();
        var random = new Random();
        var newValue = random.Next(min, max);
        prop.SetValue(newValue);
        Assert.Equal(newValue, prop.GetValue());
        prop.SetValue(currentValue);
    }

}