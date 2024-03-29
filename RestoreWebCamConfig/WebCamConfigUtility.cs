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
    private readonly Dictionary<string, ICommand> _commandByKeyword = new();
    private readonly IJsonFileAccess<IReadOnlyList<CameraDto>> _fileAccess;

    public WebCamConfigUtility(
        CameraManager cameraManager,
        CommandLineParser commandLineParser, 
        ITextWriter stdOut,
        IJsonFileAccess<IReadOnlyList<CameraDto>> fileAccess)
    {
        _cameraManager = cameraManager;
        _commandLineParser = commandLineParser;
        _stdOut = stdOut;
        _fileAccess = fileAccess;
        _commandByKeyword.Add("names", new CommandImpl("names",
            "Print out the names of found camera devices.", this.PrintCameraNames));
        _commandByKeyword.Add("describe", new CommandImpl("describe",
            "Print out the configuration of cameras.", this.PrintCameraConfiguration));
        _commandByKeyword.Add("save", new CommandImpl("save",
            "Save the current configuration of cameras into a json file.", this.WriteCameraConfigurationToFile));
        _commandByKeyword.Add("load", new CommandImpl("load",
            "Load and restore the configuration of cameras from a json file.", this.LoadCameraConfigurationFromFile));
        _commandByKeyword.Add("increment", new CommandImpl("increment",
            "Increment a camera property", this.IncrementCameraProperty));
        _commandByKeyword.Add("decrement", new CommandImpl("decrement",
            "Decrement a camera property", this.DecrementCameraProperty));
    }
    private void IncrementCameraProperty()
    {
        var property = GetCameraPropertyFromCommandLineArguments("increment");
        IncrementCameraProperty(property);
    }

    private void DecrementCameraProperty()
    {
        var property = GetCameraPropertyFromCommandLineArguments("decrement");
        DecrementCameraProperty(property);
    }
    
    private CameraProperty GetCameraPropertyFromCommandLineArguments(string purpose)
    {
        var cameraName = _commandLineParser.GetCameraName();
        var propertyName = _commandLineParser.GetPropertyName();
        if (cameraName == null || propertyName == null)
            throw new ArgumentException(
                $"To {purpose} a camera property a camera name and a property name has to be provided.");
        var camera = _cameraManager.GetCameraByName(cameraName);
        var property = camera.GetPropertyByName(propertyName);
        _stdOut.WriteLine($"{purpose}ing {property} of {camera}.");
        return property;
    }

    private void IncrementCameraProperty(CameraProperty property)
    {
        var newValue = property.GetValue() + GetIncrementFromCommandLineForProperty(property);
        if (newValue > property.GetMaxValue())
            newValue = property.GetMaxValue();
        property.SetValue(newValue);
    }

    private void DecrementCameraProperty(CameraProperty property)
    {
        var newValue = property.GetValue() - GetIncrementFromCommandLineForProperty(property);
        if (newValue < property.GetMinValue())
            newValue = property.GetMinValue();
        property.SetValue(newValue);
    }
    
    private int GetIncrementFromCommandLineForProperty(CameraProperty property)
    {
        var increment = _commandLineParser.GetStepSize();
        if (increment == 0)
            increment = property.GetIncrementSize();
        else if (increment % property.GetIncrementSize() != 0) 
            increment = (increment / property.GetIncrementSize() + 1) * property.GetIncrementSize();
        
        return increment;
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
            throw new ArgumentException(
                $"Only one command per run is possible. Found '{_commandLineParser.GetCommandsAsText()}'.");
        if (numberOfCommands == 0)
            throw new ArgumentException($"At least one command per run is required. Found none.");
        return commands[0];
    }

    private void PrintHelpToScreen()
    {
        _stdOut.WriteLine("usage: ");
        _stdOut.WriteLine("WebCamConfigUtility <options> command");
        _stdOut.WriteLine("");
        _stdOut.WriteLine("Available options are:");
        _stdOut.WriteLine(_commandLineParser.GetDescription());
        
        _stdOut.WriteLine("Available commands are:");
        foreach (var command in _commandByKeyword.Values)
        {
            _stdOut.WriteLine($"{command.GetKeyWord()}\t\t{command.GetDescription()}");
        }
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
    
    private void LoadCameraConfigurationFromFile()
    {
        string fileName = _commandLineParser.GetFileName() ??
                          throw new ArgumentException("To read from a file a filename has to be provided.");
        var jsonFile = _fileAccess.CreateJsonFile(fileName); 
        foreach (var cameraDto in jsonFile.Load())
        {
            var requestedCameraName = _commandLineParser.GetCameraName();
            if (cameraDto.Name == null || cameraDto.Name.Trim().Length == 0)
                throw new InvalidDataException($"Data in {fileName} is not valid.");
            if (requestedCameraName == null || requestedCameraName.Equals(cameraDto.Name))
            {
                RestoreCameraSettingsFromDto(cameraDto);
            }
        }
    }

    private void RestoreCameraSettingsFromDto(CameraDto cameraDto)
    {
        var cameraName = cameraDto.Name;
        var cameraDevice = _cameraManager.GetCameraByName(cameraName);
        cameraDevice.RestoreCameraDto(cameraDto);
    }

    private ReadOnlyCollection<CameraDto> GetCamerasAsDtoList()
    {
        List<CameraDto> result = new();
        string? cameraName = _commandLineParser.GetCameraName();
        
        foreach (var camera in _cameraManager.GetListOfAvailableCameras())
        {
            if (cameraName == null || cameraName.Equals(camera.GetDeviceName()))
            {
                var dto = camera.GetDto();
                result.Add(dto);
            }
        }

        return result.AsReadOnly();
    }
}