using System.Collections.ObjectModel;
using RestoreWebCamConfig.CameraAdapter;
using RestoreWebCamConfig.ConsoleAdapter;
using RestoreWebCamConfig.JsonFileAdapter;

namespace RestoreWebCamConfig;

public class WebCamConfigUtility
{
    private readonly CameraManager _cameraManager;
    private readonly CommandLineParser _commandLineParser;
    private readonly ITextWriter _stdOut;
    private readonly ITextWriter _stdErr;
    private readonly Dictionary<string, ICommand> _commandByKeyword = new();
    private readonly IJsonFileAccess<IReadOnlyList<CameraDto>> _fileAccess;

    public WebCamConfigUtility(
        CameraManager cameraManager,
        CommandLineParser commandLineParser, 
        ITextWriter stdOut,
        ITextWriter stdErr,
        IJsonFileAccess<IReadOnlyList<CameraDto>> fileAccess)
    {
        _cameraManager = cameraManager;
        _commandLineParser = commandLineParser;
        _stdOut = stdOut;
        _stdErr = stdErr;
        _fileAccess = fileAccess;
        _commandByKeyword.Add("names", new CommandImpl("names",
            "Print out the names of found camera devices.", this.PrintCameraNames));
        _commandByKeyword.Add("describe", new CommandImpl("describe",
            "Print out the configuration of cameras.", this.PrintCameraConfiguration));
        _commandByKeyword.Add("safe", new CommandImpl("safe",
            "Print out the configuration of cameras.", this.WriteCameraConfigurationToFile));
    }

    public void Run()
    {
        if (_commandLineParser.IsHelpRequested())
        {
            PrintHelpToScreen();
            return;
        }

        ExecuteCommandByKeyWord();
    }

    private void ExecuteCommandByKeyWord()
    {
        var commandKeyWord = GetCommandKeyWord();
        try
        {
            _commandByKeyword[commandKeyWord].Execute();
        }
        catch (KeyNotFoundException)
        {
            throw new ArgumentException($"Command {commandKeyWord} is not known.");
        }
    }

    private string GetCommandKeyWord()
    {
        var commands = _commandLineParser.GetCommandList();
        var numberOfCommands = commands.Count;
        if (numberOfCommands > 1)
            throw new ArgumentException($"Only one command per run is possible. Found {commands}");
        if (numberOfCommands == 0)
            throw new ArgumentException($"At least one command per run is required. Found none.");
        return commands[0];
    }

    private void PrintHelpToScreen()
    {
        _stdOut.Write("Hello");
        _stdOut.Write(" ");
        _stdOut.WriteLine("World");
    }

    private void PrintCameraNames()
    {
        foreach (var cameraName in _cameraManager.GetListOfAvailableCameraNames())
        {
            _stdOut.WriteLine($"Cam found: {cameraName}");
        }
    }

    private void PrintCameraConfiguration()
    {
        foreach (var camera in _cameraManager.GetListOfAvailableCameras())
        {
            _stdOut.WriteLine($"{camera.GetDto()}");
        }
    }

    private void WriteCameraConfigurationToFile()
    {
        string fileName = _commandLineParser.GetFileName() ??
                          throw new ArgumentException("To write to a file a filename has to be provided.");
        var content = GetCamerasAsDtoList(); 
        var jsonFile = _fileAccess.CreateJsonFile(fileName);
        jsonFile.Save(content);
    }

    private ReadOnlyCollection<CameraDto> GetCamerasAsDtoList()
    {
        List<CameraDto> result = new();

        foreach (var camera in _cameraManager.GetListOfAvailableCameras())
        {
            var dto = camera.GetDto();
            result.Add(dto);
        }

        return result.AsReadOnly();
    }
}