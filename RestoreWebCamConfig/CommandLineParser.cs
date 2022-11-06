using NDesk.Options;

namespace RestoreWebCamConfig;

public class CommandLineParser
{
    private readonly OptionSet _optionSet;
    private readonly Options _options;
    private readonly List<string> _commands;

    private CommandLineParser(OptionSet optionSet, Options options, IEnumerable<string> commandLineArguments)
    {
        _options = options;
        _optionSet = optionSet;
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

    public string GetDescription()
    {
        var stringWriter = new StringWriter();
        _optionSet.WriteOptionDescriptions(stringWriter);
        return stringWriter.ToString();
    }
}