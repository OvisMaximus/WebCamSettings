using System.Collections.Generic;
using System.IO;
using DirectShowLibAdapter;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Tests;

internal static class DirectShowMock
{
    internal const string CamNameCamOne = "Fan Corp Face Cam SD";
    internal const string CamNameCamTwo = "Simple Motion Picture Sensor";
    internal const string InvalidDeviceName = "Blind eye devices HD Pro";
    internal const string PropertyNameBrightness = "Brightness";
    internal const string PropertyNameExposure = "Exposure";
    internal const string PropertyNameFocus = "Focus";

    internal static readonly PropertyTestData[] Cam1Properties =
    {
        new(PropertyNameBrightness, 129, 1, 255, 128, 1, false, false),
        new(PropertyNameExposure, -5, -2, -11, -6, 1, true, false),
        new(PropertyNameFocus, 0, 0, 255, 25, 5, true, true)
    };

    private static readonly PropertyTestData[] Cam2Properties =
    {
        new(PropertyNameBrightness, 129, 1, 255, 128, 1, false, false),
        new(PropertyNameExposure, -5, -2, -11, -6, 1, true, false),
        new(PropertyNameFocus, 0, 0, 255, 25, 5, true, true)
    };

    internal record PropertyTestData(string Name)
    {
        internal readonly string Name = Name;
        internal readonly int Value, Min, Max, Default, Delta;
        internal readonly bool CanAuto, IsAuto;

        public PropertyTestData(
            string name, int value, int min, int max, int @default, int delta, bool canAuto, bool isAuto) : this(name)
        {
            Value = value;
            Min = min;
            Max = max;
            Default = @default;
            Delta = delta;
            IsAuto = isAuto;
            CanAuto = canAuto;
        }
    }

    internal static IDirectShowDevice CreateDirectShowMock()
    {
        var dsDevice = Substitute.For<IDirectShowDevice>();
        var dsCamera1 = BuildCameraDeviceSubstitute(dsDevice, CamNameCamOne, Cam1Properties);
        var dsCamera2 = BuildCameraDeviceSubstitute(dsDevice, CamNameCamTwo, Cam2Properties);
        var deviceList = new List<ICameraDevice> { dsCamera1, dsCamera2 };
        dsDevice.GetCameraDevicesList().Returns(deviceList.AsReadOnly());
        dsDevice.GetCameraDeviceByName(InvalidDeviceName).Throws(new FileNotFoundException());
        return dsDevice;
    }

    private static ICameraDevice BuildCameraDeviceSubstitute(IDirectShowDevice device, string name,
        PropertyTestData[] propertyList)
    {
        var camera = Substitute.For<ICameraDevice>();
        camera.GetDeviceName().Returns(name);
        device.GetCameraDeviceByName(name).Returns(camera);

        BuildProperties(camera, propertyList);

        return camera;
    }

    private static void BuildProperties(ICameraDevice camera, PropertyTestData[] propertyTestDataList)
    {
        var propertiesList = new List<ICameraProperty>();
        foreach (var propertyTestData in propertyTestDataList)
        {
            propertiesList.Add(BuildCameraPropertySubstitute(camera, propertyTestData));
        }

        camera.GetPropertiesList().Returns(propertiesList.AsReadOnly());

        camera.GetPropertyByName(InvalidDeviceName).Throws<InvalidDataException>();
    }

    private static ICameraProperty BuildCameraPropertySubstitute(ICameraDevice camera, PropertyTestData testData)
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
}