using System;
using System.Collections.Generic;
using System.Text;
using DirectShowLibAdapter;
using NSubstitute;
using NSubstitute.Core;
using NSubstitute.Exceptions;
using RestoreWebCamConfig;
using RestoreWebCamConfig.CameraAdapter;
using RestoreWebCamConfig.ConsoleAdapter;
using RestoreWebCamConfig.JsonFileAdapter;
using Xunit;
using Xunit.Abstractions;

namespace Tests;

public class WebCamConfigUtilityTest
{
    private const string TestFilename = "fileName.txt";
    private readonly ITestOutputHelper _testOutputHelper;

    public WebCamConfigUtilityTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    struct TestFixture
    {
        public IJsonFile<IReadOnlyList<CameraDto>> JsonFile { get; private set; }
        internal readonly ITextWriter StdOut;
        internal readonly IJsonFileAccess<IReadOnlyList<CameraDto>> FileAccess;
        internal readonly WebCamConfigUtility ObjectUnderTest;
        internal readonly IDirectShowDevice DirectShowDevice;

        public TestFixture(IDirectShowDevice directShowDevice,
            ITextWriter stdOut,
            IJsonFileAccess<IReadOnlyList<CameraDto>> fileAccess,
            IJsonFile<IReadOnlyList<CameraDto>> jsonFile,
            WebCamConfigUtility objectUnderTest){
            DirectShowDevice = directShowDevice;
            StdOut = stdOut;
            FileAccess = fileAccess;
            JsonFile = jsonFile;
            ObjectUnderTest = objectUnderTest;
        }

    }

    private static TestFixture CreateTestFixtureForCommandLineArguments(string[] args)
    {
        CommandLineParser parser = CommandLineParser.GetCommandLineParserFor(args);
        IDirectShowDevice directShowDevice = DirectShowMock.CreateDirectShowMock();
        CameraManager cameraManager = new CameraManager(directShowDevice);
        ITextWriter stdOut = Substitute.For<ITextWriter>();
        IJsonFileAccess<IReadOnlyList<CameraDto>> jsonFileAccess = Substitute.For<IJsonFileAccess<IReadOnlyList<CameraDto>>>();
        IJsonFile<IReadOnlyList<CameraDto>> jsonFile = Substitute.For<IJsonFile<IReadOnlyList<CameraDto>>>();
        var cameraDtoList = CameraDtoList();
        jsonFile.Load().Returns(cameraDtoList);
        jsonFileAccess.CreateJsonFile(TestFilename).Returns(jsonFile);
        WebCamConfigUtility objectUnderTest = 
            new WebCamConfigUtility(cameraManager, parser, stdOut, jsonFileAccess);
        
        return new TestFixture(directShowDevice, stdOut, jsonFileAccess, jsonFile, objectUnderTest);
    }

    private static IReadOnlyList<CameraDto> CameraDtoList()
    {
        var result = new List<CameraDto>();
        AddCameraDtoToList(result, DirectShowMock.CamNameCamOne);
        AddCameraDtoToList(result, DirectShowMock.CamNameCamTwo);
        return result.AsReadOnly();
    }

    private static void AddCameraDtoToList(List<CameraDto> propertiesList, string cameraName)
    {
        var cameraDto = new CameraDto(cameraName)
        {
            Properties = new List<CameraPropertyDto>()
        };

        AddPropertyDtoToPropertiesList(cameraDto.Properties!, DirectShowMock.PropertyNameBrightness);
        AddPropertyDtoToPropertiesList(cameraDto.Properties!, DirectShowMock.PropertyNameExposure);
        AddPropertyDtoToPropertiesList(cameraDto.Properties!, DirectShowMock.PropertyNameFocus);

        propertiesList.Add(cameraDto);
    }

    private static void AddPropertyDtoToPropertiesList(IList<CameraPropertyDto> propertiesList,
        string propertyName)
    {
        var property = new CameraPropertyDto(propertyName)
        {
            CanAdaptAutomatically = true,
            Default = 17,
            IsAutomaticallyAdapting = true,
            MaxValue = 55,
            MinValue = -20,
            SteppingDelta = 1,
            Value = 42
        };
        propertiesList.Add(property);
    }

    private static String ConcatenateCallContentToString(ITextWriter writer)
    {
        var result = new StringBuilder();

        foreach (var receivedCall in (IEnumerable<ICall>)writer.ReceivedCalls())
        {
            foreach (var argument in receivedCall.GetArguments())
            {
                result.Append(argument);
                if (receivedCall.GetMethodInfo().Name == "WriteLine")
                {
                    result.AppendLine();
                }
            }
        }

        return result.ToString();
    }

    
    private string ConcatenateCallContentToString(IJsonFile<IReadOnlyList<CameraDto>> jsonFile)
    {
        var result = new StringBuilder();

        foreach (var receivedCall in (IEnumerable<ICall>)jsonFile.ReceivedCalls())
        {
            foreach (var argument in receivedCall.GetArguments())
            {
                if (argument is IReadOnlyList<CameraDto> list)
                    foreach (var cameraDto in list)
                    {
                        result.Append(cameraDto);
                    }
            }
        }

        return result.ToString();
    }

    private void ValidateAllPropertiesOfCamWereSet(TestFixture testFixture, string cameraName)
    {
        var camera = testFixture.DirectShowDevice.GetCameraDeviceByName(cameraName);
        ValidatePropertyWasSet(camera, DirectShowMock.PropertyNameBrightness);
        ValidatePropertyWasSet(camera, DirectShowMock.PropertyNameExposure);
        ValidatePropertyWasSet(camera, DirectShowMock.PropertyNameFocus);
    }

    private void ValidatePropertyWasSet(ICameraDevice cameraDevice, string propertyName)
    {
        var propertyUnderTest = cameraDevice.GetPropertyByName(propertyName);
        try
        {
            propertyUnderTest.ReceivedWithAnyArgs().SetValue(default);
            propertyUnderTest.ReceivedWithAnyArgs().SetAutoAdapt(default);
        }
        catch (ReceivedCallsException)
        {
            _testOutputHelper.WriteLine(
                $"Check for expected calls on '{cameraDevice.GetDeviceName()}'.{propertyName} failed.");
            throw;
        }
    }

    [Theory]
    [InlineData(new[]{"-c", "aCamName"}, "missing command")]
    [InlineData(new[]{"-f", "test.txt", "brickbat", "titanic"}, "too many commands")]
    [InlineData(new[]{"-f", "test.txt", "brickbat"}, "invalid command")]
#pragma warning disable xUnit1026
    public void TestErrorsOnCommandIssuesInCommandLine(string[] args, string dummyParameterToAllowStringArrayAsOnlyTheoryInput)
#pragma warning restore xUnit1026
    {
        var testFixture = CreateTestFixtureForCommandLineArguments(args);
        
        Assert.Throws<ArgumentException>(() => testFixture.ObjectUnderTest.Run());
    }

    [Fact]
    public void TestHelpIsAvailableAndSentToTheCorrectChannel()
    {
        var testFixture = CreateTestFixtureForCommandLineArguments(new[] { "-h" });

        testFixture.ObjectUnderTest.Run();

        var output = ConcatenateCallContentToString(testFixture.StdOut);
        Assert.NotEmpty(output);
        Assert.Contains("save", output);
        Assert.Contains("load", output);
        Assert.Contains("names", output);
        Assert.Contains("describe", output);
    }

    [Fact]
    public void TestPrintOutOfCameraNames()
    {
        var testFixture = CreateTestFixtureForCommandLineArguments(new[]{"names"});
        
        testFixture.ObjectUnderTest.Run();

        var output = ConcatenateCallContentToString(testFixture.StdOut);
        Assert.Contains(DirectShowMock.CamNameCamOne, output);
        Assert.Contains(DirectShowMock.CamNameCamTwo, output);
    }

    [Fact]
    public void TestPrintOutOfCameraDetails()
    {
        var testFixture = CreateTestFixtureForCommandLineArguments(new[]{"describe"});
        
        testFixture.ObjectUnderTest.Run();
        
        var output = ConcatenateCallContentToString(testFixture.StdOut);
        Assert.Contains(DirectShowMock.CamNameCamOne, output);
        Assert.Contains(DirectShowMock.CamNameCamTwo, output);
        Assert.Contains(DirectShowMock.PropertyNameBrightness, output);
        Assert.Contains(DirectShowMock.PropertyNameExposure, output);
        Assert.Contains(DirectShowMock.PropertyNameFocus, output);
    }

    [Fact]
    public void TestWriteCameraConfigurationToFileNeedsFileName()
    {
        var testFixture = CreateTestFixtureForCommandLineArguments(new[]{"safe"});
      
        Assert.Throws<ArgumentException>(() => testFixture.ObjectUnderTest.Run());
    }

    [Fact]
    public void TestWriteCameraConfigurationToFile()
    {
        var testFixture = CreateTestFixtureForCommandLineArguments(new[]{"save", "-f", TestFilename});
      
        testFixture.ObjectUnderTest.Run();

        testFixture.FileAccess.Received().CreateJsonFile(TestFilename);
        var output = ConcatenateCallContentToString(testFixture.JsonFile);
        Assert.Contains(DirectShowMock.CamNameCamOne, output);
        Assert.Contains(DirectShowMock.CamNameCamTwo, output);
        Assert.Contains(DirectShowMock.PropertyNameBrightness, output);
        Assert.Contains(DirectShowMock.PropertyNameExposure, output);
        Assert.Contains(DirectShowMock.PropertyNameFocus, output);
    }
    
    [Fact]
    public void LimitExportedCamerasWhenWritingCameraConfigurationToFile()
    {
        var testFixture = CreateTestFixtureForCommandLineArguments(new[]
            { "save", "-f", TestFilename, "-c", DirectShowMock.CamNameCamOne });
      
        testFixture.ObjectUnderTest.Run();

        testFixture.FileAccess.Received().CreateJsonFile(TestFilename);
        testFixture.DirectShowDevice.DidNotReceive().GetCameraDeviceByName(DirectShowMock.CamNameCamTwo);
        var unselectedCam = testFixture.DirectShowDevice.GetCameraDeviceByName(DirectShowMock.CamNameCamTwo);
        unselectedCam.DidNotReceive().GetPropertiesList();
        unselectedCam.DidNotReceiveWithAnyArgs().GetPropertyByName(default!);
        
        var output = ConcatenateCallContentToString(testFixture.JsonFile);
        Assert.Contains(DirectShowMock.CamNameCamOne, output);
        Assert.DoesNotContain(DirectShowMock.CamNameCamTwo, output);
        Assert.Contains(DirectShowMock.PropertyNameBrightness, output);
        Assert.Contains(DirectShowMock.PropertyNameExposure, output);
        Assert.Contains(DirectShowMock.PropertyNameFocus, output);
    }

    

    [Fact]
    public void TestLoadCameraConfigurationFromFileNeedsFileName()
    {
        var testFixture = CreateTestFixtureForCommandLineArguments(new[]{"load"});
      
        Assert.Throws<ArgumentException>(() => testFixture.ObjectUnderTest.Run());
    }

    [Fact]
    public void TestLoadCameraConfigurationFromFile()
    {
        var testFixture = CreateTestFixtureForCommandLineArguments(new[] { "load", "-f", TestFilename });

        testFixture.ObjectUnderTest.Run();

        testFixture.FileAccess.Received().CreateJsonFile(TestFilename);
        testFixture.JsonFile.Received().Load();

        ValidateAllPropertiesOfCamWereSet(testFixture, DirectShowMock.CamNameCamOne);
        ValidateAllPropertiesOfCamWereSet(testFixture, DirectShowMock.CamNameCamTwo);
    }
    
    [Fact]
    public void TestLoadedCameraIsLimitedToProvidedCameraNameWhenReadingConfigurationFromFile()
    {
        var testFixture = CreateTestFixtureForCommandLineArguments(new[]
            { "load", "-f", TestFilename, "-c", DirectShowMock.CamNameCamOne });

        testFixture.ObjectUnderTest.Run();

        testFixture.FileAccess.Received().CreateJsonFile(TestFilename);
        testFixture.JsonFile.Received().Load();
        testFixture.DirectShowDevice.Received().GetCameraDeviceByName(DirectShowMock.CamNameCamOne);
        testFixture.DirectShowDevice.DidNotReceive().GetCameraDeviceByName(DirectShowMock.CamNameCamTwo);
        var unselectedCam = testFixture.DirectShowDevice.GetCameraDeviceByName(DirectShowMock.CamNameCamTwo);
        unselectedCam.DidNotReceive().GetPropertiesList();
        unselectedCam.DidNotReceiveWithAnyArgs().GetPropertyByName(default!);
        
        ValidateAllPropertiesOfCamWereSet(testFixture, DirectShowMock.CamNameCamOne);
    }

}