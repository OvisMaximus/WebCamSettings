using System;
using System.Collections.Generic;
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
    struct TestFixture
    {
        internal readonly CameraManager CameraManager;
        internal readonly ITextWriter StdOut;
        internal readonly ITextWriter StdErr;
        internal readonly WebCamConfigUtility ObjectUnderTest;
        internal readonly CommandLineParser Parser;

        public TestFixture(
            CommandLineParser parser, 
            CameraManager cameraManager, 
            ITextWriter stdOut, 
            ITextWriter stdErr, 
            WebCamConfigUtility objectUnderTest)
        {
            Parser = parser;
            CameraManager = cameraManager;
            StdOut = stdOut;
            StdErr = stdErr;
            ObjectUnderTest = objectUnderTest;
        }

    }
    
    private readonly ITestOutputHelper _testOutputHelper;

    private static TestFixture CreateTestFixtureForCommandLineArguments(string[] args)
    {
        CommandLineParser parser = CommandLineParser.GetCommandLineParserFor(args);
        CameraManager cameraManager = new CameraManager(DirectShowMock.CreateDirectShowMock());
        ITextWriter stdOut = Substitute.For<ITextWriter>();
        ITextWriter stdErr = Substitute.For<ITextWriter>();
        WebCamConfigUtility objectUnderTest = new WebCamConfigUtility(cameraManager, parser, stdOut, stdErr);
        
        return new TestFixture(parser, cameraManager, stdOut, stdErr, objectUnderTest);
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
        
        Assert.Contains(DirectShowMock.CamNameCamOne, ConcatenateCallContentToString(testFixture.StdOut));
        Assert.Contains(DirectShowMock.CamNameCamTwo, ConcatenateCallContentToString(testFixture.StdOut));
    }
}