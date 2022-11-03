using System;
using RestoreWebCamConfig;
using RestoreWebCamConfig.CameraAdapter;
using Xunit;

namespace Tests;

public class WebCamConfigUtilityTest
{
    [Theory]
    [InlineData(new[]{"-c", "aCamName"}, "missing command")]
    [InlineData(new[]{"-f", "test.txt", "brickbat", "titanic"}, "too many commands")]
    [InlineData(new[]{"-f", "test.txt", "brickbat"}, "invalid command")]
#pragma warning disable xUnit1026
    public void TestErrorsOnCommandIssuesInCommandLine(string[] args, string dummyParameterToAllowStringArrayAsOnlyTheoryInput)
#pragma warning restore xUnit1026
    {
        CommandLineParser parser = CommandLineParser.GetCommandLineParserFor(args);
        CameraManager cameraManager = new CameraManager(DirectShowMock.CreateDirectShowMock());
        WebCamConfigUtility objectUnderTest = new WebCamConfigUtility(cameraManager, parser);

        Assert.Throws<ArgumentException>(() => objectUnderTest.Run());
    }
}