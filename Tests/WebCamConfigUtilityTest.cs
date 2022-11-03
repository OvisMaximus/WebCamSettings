using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NSubstitute;
using NSubstitute.Core;
using RestoreWebCamConfig;
using RestoreWebCamConfig.CameraAdapter;
using RestoreWebCamConfig.ConsoleAdapter;
using Xunit;
using Xunit.Abstractions;

namespace Tests;

public class WebCamConfigUtilityTest
{
    private readonly ITestOutputHelper _testOutputHelper;

    public WebCamConfigUtilityTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

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
        ITextWriter stdOut = Substitute.For<ITextWriter>();
        ITextWriter stdErr = Substitute.For<ITextWriter>();
        WebCamConfigUtility objectUnderTest = new WebCamConfigUtility(cameraManager, parser, stdOut, stdErr);

        Assert.Throws<ArgumentException>(() => objectUnderTest.Run());
    }

    [Fact]
    public void TestHelpIsAvailableAndSentToTheCorrectChannel()
    {
        CommandLineParser parser = CommandLineParser.GetCommandLineParserFor(new []{"-h"});
        CameraManager cameraManager = new CameraManager(DirectShowMock.CreateDirectShowMock());
        var stdOut = Substitute.For<ITextWriter>();
        var stdErr = Substitute.For<ITextWriter>();
        WebCamConfigUtility objectUnderTest = new WebCamConfigUtility(cameraManager, parser, stdOut, stdErr);

        objectUnderTest.Run();

        Assert.NotEmpty(ConcatenateCallContentToString(stdOut));        
        Assert.Empty(ConcatenateCallContentToString(stdErr));        
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
}