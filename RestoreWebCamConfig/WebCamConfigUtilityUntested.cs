using System.Text.Json;
using NDesk.Options;
using RestoreWebCamConfig.CameraAdapter;
using RestoreWebCamConfig.oldArch;

namespace RestoreWebCamConfig;

internal class WebCamConfigUtilityUntested
{
    private readonly List<string> _commands;
    private readonly Options _options;
    private readonly Dictionary<string, ICommand> _operations;

    // ReSharper disable once UnusedType.Global
    internal class LoadCommand : CommandImpl
    {
        public LoadCommand(WebCamConfigUtilityUntested utilityUntested) : base(
            "load", 
            "set the cams to the settings provided in the file referred by option -f. May be restricted to the camera identified by -c.",
            utilityUntested.RestoreCameraPropertiesFromFile) {}
    }
    
    // ReSharper disable once UnusedType.Global
    internal class SaveCommand : CommandImpl
    {
        public SaveCommand(WebCamConfigUtilityUntested utilityUntested) : base(
            "save", 
            "dump the settings of connected cameras into the file referred by option -f. May be restricted to the camera identified by -c.",
            utilityUntested.WriteCameraPropertiesToFile) {}
    }

    // ReSharper disable once UnusedType.Global
    internal class NamesCommand : CommandImpl
    {
        public NamesCommand(WebCamConfigUtilityUntested utilityUntested) : base(
            "names", 
            "print out the names of connected cameras.",
            utilityUntested.DumpCameraNames) {}
    }

    // ReSharper disable once UnusedType.Global
    internal class DescribeCommand : CommandImpl
    {
        public DescribeCommand(WebCamConfigUtilityUntested utilityUntested) : base(
            "describe", 
            "print out the available properties of connected cameras. May be restricted to the camera identified by -c.",
            utilityUntested.DescribeCameraProperties) {}
    }


    public WebCamConfigUtilityUntested(string[] args)
    {
        _options = new Options();
        _operations = InitializeOperations();
        var optionSet = CreateOptionSet(_options);
        _commands = optionSet.Parse(args);
        Console.WriteLine($"Options: {_options}");
        Console.Write("Command to execute: ");
        _commands.ForEach(v => Console.Write($" {v} "));
        Console.WriteLine("");
    }

    private Dictionary<string, ICommand> InitializeOperations()
    {
        var result = new Dictionary<string, ICommand>();
        var operationType = typeof(ICommand);
        var types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => (operationType.IsAssignableFrom(p) && ! p.IsAbstract));
        foreach (var type in types)
        {
            if (Activator.CreateInstance(type, this) is not ICommand operation) continue;
            result.Add(operation.GetKeyWord(), operation);
        }
        return result;
    }

    private OptionSet CreateOptionSet(Options options)
    {
        var optionSet = new OptionSet
        {
            { "camera|c=", "name of the camera to work with", value => { options.CameraName = value; } },
            { "file|f=", "name of the file to work with", value => { options.FileName = value; } },
            { "h|?|help", "show help text", _ => { PrintHelp(); } }
        };
        return optionSet;
    }

    private void PrintHelp()
    {
        Console.WriteLine("RestoreWebCamConfig [<Option>...<Option>] command");
        Console.WriteLine("     Commands are");
        foreach (var operation in _operations.Values)
        {
            Console.WriteLine($"        {operation.GetKeyWord()} - {operation.GetDescription()}");
        }
        Console.WriteLine("     Options are");
        CreateOptionSet(_options).WriteOptionDescriptions(Console.Out);
        Environment.Exit(0);
    }

    public void Run()
    {
        var numberOfCommands = _commands.Count;
        if (numberOfCommands > 1)
            throw new ArgumentException($"Only one command per run is possible. Found {_commands}");
        if (numberOfCommands == 0)
            throw new ArgumentException($"At least one command per run is possible. Found none.");

        PerformCommand(_commands[0]);
    }

    private void PerformCommand(string command)
    {
        try
        {
            var operation = _operations[command];
            operation.Execute();
        }
        catch (KeyNotFoundException)
        {
            throw new ArgumentException($"Command '{command}' is not supported.");
        }
    }

    private void DescribeCameraProperties()
    {
        var cameraPropertiesList = GetPropertiesOfSelectedCameras();
        cameraPropertiesList.ForEach(Console.WriteLine);
    }

    private void WriteCameraPropertiesToFile()
    {
        var cameraPropertiesList = GetPropertiesOfSelectedCameras();

        var fileName = _options.FileName ??
                       throw new ArgumentException("Filename must be set to dump camera settings");
        WriteObjectAsJsonToFile(cameraPropertiesList, fileName);
    }

    private List<CameraDto> GetPropertiesOfSelectedCameras()
    {
        var cameraNameList = DetermineListOfCamerasToProcess();
        if (cameraNameList.Count == 0)
            throw new MissingMemberException("No camera device found.");
        return GetCameraPropertiesList(cameraNameList);
    }

    private List<CameraDto> GetCameraPropertiesList(List<string> cameraNameList)
    {
        var result = new List<CameraDto>();
        foreach (var cameraName in cameraNameList)
        {
            var cameraProperties = FetchCameraPropertiesByName(cameraName);
            if(cameraProperties != null) 
                result.Add(cameraProperties);
        }
        return result;
    }

    private static CameraDto? FetchCameraPropertiesByName(string cameraName)
    {
        CameraDto? cameraProperties = null;
        try
        {
            Console.Write($"get config for {cameraName} - ");
            var camera = CameraController.FindCamera(cameraName);
            cameraProperties = camera.GetDeviceProperties();
            Console.WriteLine("done.");
        }
        catch (ArgumentException)
        {
            Console.WriteLine($"seems to be no real camera. I will skip it...");
        }
        return cameraProperties;
    }

    private void WriteObjectAsJsonToFile(object o, string fileName)
    {
        var stream = File.Create(fileName);
        var jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        JsonSerializer.Serialize(stream, o, jsonOptions);
        stream.Dispose();
    }

    private void DumpCameraNames()
    {
        foreach (var cameraName in CameraController.GetKnownCameraNames())
            Console.WriteLine($"Cam found: {cameraName}");
    }

    private List<string> DetermineListOfCamerasToProcess()
    {
        var list = _options.CameraName != null
            ? new List<string> { _options.CameraName }
            : CameraController.GetKnownCameraNames();

        return list;
    }

    private void RestoreCameraPropertiesFromFile()
    {
        var cameraList = ReadCameraPropertiesFromFile();
        if (_options.CameraName != null)
            cameraList = cameraList.FindAll(camera => camera.Name == _options.CameraName);
        cameraList.ForEach(RestorePropertiesOfCamera);
    }

    private List<CameraDto> ReadCameraPropertiesFromFile()
    {
        var fileName = _options.FileName;
        if (fileName == null) 
            throw new ArgumentException("File name must be provided.");
        using var stream = File.OpenRead(fileName);
        var cameraList =
            JsonSerializer.Deserialize(stream, typeof(List<CameraDto>)) as List<CameraDto> ??
            throw new InvalidOperationException($"{fileName} could not be read as camera settings");
        stream.Dispose();
        return cameraList;
    }
    
    private void RestorePropertiesOfCamera(CameraDto cameraSettings)
    {
        if (cameraSettings.Name == null || cameraSettings.Properties == null)
            throw new InvalidDataException("Invalid json record with unset Name and/or Properties.");
        var cameraName = cameraSettings.Name;
        Console.WriteLine($"Initializing {cameraSettings}");
        var camController = CameraController.FindCamera(cameraName);
        camController.RestoreProperties(cameraSettings);
        Console.WriteLine($"Done. State: {camController.GetDeviceProperties()}");
    }
}