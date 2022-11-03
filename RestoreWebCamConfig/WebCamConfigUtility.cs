using RestoreWebCamConfig.CameraAdapter;
using RestoreWebCamConfig.ConsoleAdapter;

namespace RestoreWebCamConfig;

public class WebCamConfigUtility
{
    private readonly CameraManager _cameraManager;
    private readonly CommandLineParser _commandLineParser;
    private readonly ITextWriter _stdOut;
    private readonly ITextWriter _stdErr;


    public WebCamConfigUtility(CameraManager cameraManager, CommandLineParser commandLineParser, ITextWriter stdOut,
        ITextWriter stdErr)
    {
        _cameraManager = cameraManager;
        _commandLineParser = commandLineParser;
        _stdOut = stdOut;
        _stdErr = stdErr;
    }

    public void Run()
    {
        if (_commandLineParser.IsHelpRequested())
        {
            PrintHelpToScreen();
            return;
        }
        var commands = _commandLineParser.GetCommandList();
        var numberOfCommands = commands.Count;
        if (numberOfCommands > 1)
            throw new ArgumentException($"Only one command per run is possible. Found {commands}");
        if (numberOfCommands == 0)
            throw new ArgumentException($"At least one command per run is possible. Found none.");

        throw new ArgumentException($"Unknown Command '{commands[0]}'.");

    }

    private void PrintHelpToScreen()
    {
        _stdOut.Write("Hello");
        _stdOut.Write(" ");
        _stdOut.WriteLine("World");
    }
}