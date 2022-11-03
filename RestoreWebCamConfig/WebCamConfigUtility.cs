using RestoreWebCamConfig.CameraAdapter;

namespace RestoreWebCamConfig;

public class WebCamConfigUtility
{
    private readonly CameraManager _cameraManager;
    private readonly CommandLineParser _commandLineParser;
    

    public WebCamConfigUtility(CameraManager cameraManager, CommandLineParser commandLineParser)
    {
        _cameraManager = cameraManager;
        _commandLineParser = commandLineParser;
    }

    public void Run()
    {
        var commands = _commandLineParser.GetCommandList();
        var numberOfCommands = commands.Count;
        if (numberOfCommands > 1)
            throw new ArgumentException($"Only one command per run is possible. Found {commands}");
        if (numberOfCommands == 0)
            throw new ArgumentException($"At least one command per run is possible. Found none.");

        throw new ArgumentException($"Unknown Command '{commands[0]}'.");

    }
}