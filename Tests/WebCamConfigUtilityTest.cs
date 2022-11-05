using System;
using System.Collections.Generic;
using System.Text;
using NSubstitute;
using NSubstitute.Core;
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
    private readonly ITestOutputHelper _TestOutputHelper;

    public WebCamConfigUtilityTest(ITestOutputHelper testOutputHelper)
    {
        _TestOutputHelper = testOutputHelper;
    }

    struct TestFixture
    {
        internal readonly CameraManager CameraManager;
        internal readonly ITextWriter StdOut;
        internal readonly ITextWriter StdErr;
        internal readonly IJsonFileAccess<IReadOnlyList<CameraDto>> FileAccess;
        internal readonly WebCamConfigUtility ObjectUnderTest;
        internal readonly CommandLineParser Parser;

        public TestFixture(
            CommandLineParser parser, 
            CameraManager cameraManager, 
            ITextWriter stdOut, 
            ITextWriter stdErr, 
            IJsonFileAccess<IReadOnlyList<CameraDto>> fileAccess,
            WebCamConfigUtility objectUnderTest
        ){
            Parser = parser;
            CameraManager = cameraManager;
            StdOut = stdOut;
            StdErr = stdErr;
            ObjectUnderTest = objectUnderTest;
            FileAccess = fileAccess;
        }

    }

    private static TestFixture CreateTestFixtureForCommandLineArguments(string[] args)
    {
        CommandLineParser parser = CommandLineParser.GetCommandLineParserFor(args);
        CameraManager cameraManager = new CameraManager(DirectShowMock.CreateDirectShowMock());
        ITextWriter stdOut = Substitute.For<ITextWriter>();
        ITextWriter stdErr = Substitute.For<ITextWriter>();
        IJsonFileAccess<IReadOnlyList<CameraDto>> jsonFileAccess = Substitute.For<IJsonFileAccess<IReadOnlyList<CameraDto>>>();
        IJsonFile<IReadOnlyList<CameraDto>> jsonFile = Substitute.For<IJsonFile<IReadOnlyList<CameraDto>>>();
        jsonFileAccess.CreateJsonFile(TestFilename).Returns(jsonFile);
        WebCamConfigUtility objectUnderTest = 
            new WebCamConfigUtility(cameraManager, parser, stdOut, stdErr, jsonFileAccess);
        
        return new TestFixture(parser, cameraManager, stdOut, stdErr, jsonFileAccess, objectUnderTest);
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
        TestFixture testFixture = CreateTestFixtureForCommandLineArguments(new[] { "-h" });

        testFixture.ObjectUnderTest.Run();

        Assert.NotEmpty(ConcatenateCallContentToString(testFixture.StdOut));        
        Assert.Empty(ConcatenateCallContentToString(testFixture.StdErr));        
    }

    [Fact]
    public void TestPrintOutOfCameraNames()
    {
        TestFixture testFixture = CreateTestFixtureForCommandLineArguments(new[]{"names"});
        
        testFixture.ObjectUnderTest.Run();

        var output = ConcatenateCallContentToString(testFixture.StdOut);
        Assert.Contains(DirectShowMock.CamNameCamOne, output);
        Assert.Contains(DirectShowMock.CamNameCamTwo, output);
    }

    [Fact]
    public void TestPrintOutOfCameraDetails()
    {
        TestFixture testFixture = CreateTestFixtureForCommandLineArguments(new[]{"describe"});
        
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
        TestFixture testFixture = CreateTestFixtureForCommandLineArguments(new[]{"safe"});
      
        Assert.Throws<ArgumentException>(() => testFixture.ObjectUnderTest.Run());
    }

    [Fact]
    public void TestWriteCameraConfigurationToFile()
    {
        TestFixture testFixture = CreateTestFixtureForCommandLineArguments(new[]{"safe", "-f", TestFilename});
      
        testFixture.ObjectUnderTest.Run();
        
        var fileAccess = testFixture.FileAccess;
        fileAccess.Received().CreateJsonFile(TestFilename);
        var outputFile = testFixture.FileAccess.CreateJsonFile(TestFilename);
        var output = ConcatenateCallContentToString(outputFile);

        Assert.Contains(DirectShowMock.CamNameCamOne, output);
        Assert.Contains(DirectShowMock.CamNameCamTwo, output);
        Assert.Contains(DirectShowMock.PropertyNameBrightness, output);
        Assert.Contains(DirectShowMock.PropertyNameExposure, output);
        Assert.Contains(DirectShowMock.PropertyNameFocus, output);
    }

}