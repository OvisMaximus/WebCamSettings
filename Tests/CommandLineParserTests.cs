using RestoreWebCamConfig;
using Xunit;

namespace Tests;

public class CommandLineParserTests
{
    [Theory]
    [InlineData(new[] { "-f", "theFileName.txt" }, "theFileName.txt", null, false)]
    [InlineData(new[] { "--file", "theOtherFileName.txt" }, "theOtherFileName.txt", null, false)]
    [InlineData(new[] { "-c", "theCamName", "--help" }, null, "theCamName", true)]
    [InlineData(new[] { "-h", "--camera", "theOtherCamName" }, null, "theOtherCamName", true)]
    [InlineData(new[] { "--cam", "theThirdCamName" }, null, "theThirdCamName", false)]
    [InlineData(new[] { "-c", "theFourthCamName", "-?", "--file", "f.t" }, "f.t", "theFourthCamName", true)]
    public void TestParameterOptions(string[] argumentList, string expectedFileName, string expectedCameraName, bool isHelpRequested)
    {
        var parser = CommandLineParser.GetCommandLineParserFor(argumentList);
        Assert.Equal(expectedFileName, parser.GetFileName());
        Assert.Equal(expectedCameraName, parser.GetCameraName());
        Assert.Equal(isHelpRequested, parser.IsHelpRequested());
        Assert.Empty(parser.GetCommandList());
    }

    [Fact]
    public void TestCommands()
    {
        CommandLineParser parser = CommandLineParser.GetCommandLineParserFor(
            new[] { "-c", "aCamName", "command1", "-f", "aFileName", "command2" });
        Assert.Equal(2, parser.GetCommandList().Count);
        Assert.Equal("command1", parser.GetCommandList()[0]);
        Assert.Equal("command2", parser.GetCommandList()[1]);
    }
}