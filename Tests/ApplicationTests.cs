using System.Collections.Generic;
using NDesk.Options;
using RestoreWebCamConfig;
using Xunit;

namespace Tests;

public class ApplicationTests
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

public class CommandLineParser
{
    private readonly Options _options;
    private readonly List<string> _commands;

    private CommandLineParser(OptionSet optionSet, Options options, IEnumerable<string> commandLineArguments)
    {
        _options = options;
        _commands = optionSet.Parse(commandLineArguments);
    }

    public static CommandLineParser GetCommandLineParserFor(string[] commandLineArguments)
    {
        var options = new Options();
        var optionSet = new OptionSet
        {
            { "file|f=", "name of the file to work with", value => { options.FileName = value; }},
            { "cam|camera|c=", "name of the camera device to work with", value => { options.CameraName = value; }},
            { "?|help|h", "dump this help text to the console", _ => { options.IsHelpRequested = true; }}
        };
        return new CommandLineParser(optionSet, options, commandLineArguments);
    }

    public IReadOnlyList<string> GetCommandList()
    {
        return _commands.AsReadOnly();
    }

    public string? GetFileName()
    {
        return _options.FileName;
    }

    public string? GetCameraName()
    {
        return _options.CameraName;
    }

    public bool IsHelpRequested()
    {
        return _options.IsHelpRequested;
    }
}