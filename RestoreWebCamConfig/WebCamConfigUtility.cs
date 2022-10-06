using System.Text.Json;
using NDesk.Options;

namespace RestoreWebCamConfig;

internal class WebCamConfigUtility
{
    private readonly List<string> _commands;
    private readonly Options _options;

    public WebCamConfigUtility(string[] args)
    {
        _options = new Options();
        var optionSet = CreateOptionSet(_options);
        _commands = optionSet.Parse(args);
        Console.WriteLine($"Options: {_options}");
        Console.Write("Commands to execute: ");
        _commands.ForEach(v => Console.Write($" {v},"));
        Console.WriteLine("");
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
        Console.WriteLine("         save - dump the settings of connected cameras into the file referred by option -f");
        Console.WriteLine("                  May be restricted to the camera identified by -c.");
        Console.WriteLine("         names - show the names of connected cameras");
        Console.WriteLine("         load - set the cams to the settings provided in the file referred by option -f.");
        Console.WriteLine("                  May be restricted to the camera identified by -c.");

        CreateOptionSet(new Options()).WriteOptionDescriptions(Console.Out);
        Environment.Exit(0);
    }

    public void Run()
    {
        var numberOfCommands = _commands.Count;
        if (numberOfCommands > 1)
            throw new ArgumentException($"Only one command per run is possible. Found {_commands}");
        if (numberOfCommands == 0)
            throw new ArgumentException($"At least one command per run is possible. Found {_commands}");
        else
            PerformCommand(_commands[0]);
    }

    private void PerformCommand(string command)
    {
        switch (command)
        { 
            case "names":
                DumpCameraNames();
                break;
            case "save":
                SaveCameraSettings();
                break;
            case "load":
                LoadCameraPropertiesFromFile();
                break; 
            default:
                throw new ArgumentException($"Command '{command}' is not supported.");
        }
    }


    private void SaveCameraSettings()
    {
        DumpCameraInformation(controller => controller.GetDeviceProperties());
    }


    private void DumpCameraInformation(Func<CameraController, object> camInformationFactory)
    {
        var cameraNameList = DetermineListOfCamerasToProcess();
        if (cameraNameList.Count == 0)
            throw new MissingMemberException("No camera device found.");
        var fileName = _options.FileName ??
                       throw new ArgumentException("Filename must be set to dump camera settings");
        var stream = File.Create(fileName);
        var cameraSettingsList = GetCameraSettingsList(cameraNameList, camInformationFactory);
        WriteObjectAsJsonToStream(cameraSettingsList, stream);
        stream.Dispose();
    }

    private List<object> GetCameraSettingsList(List<string> cameraNameList,
        Func<CameraController, object> camInformationFactory)
    {
        var result = new List<object>();
        foreach (var cameraName in cameraNameList)
        {
            Console.WriteLine($"get config for {cameraName}");
            try
            {
                var camera = CameraController.FindCamera(cameraName);
                var settings = camInformationFactory(camera);
                result.Add(settings);
            }
            catch (ArgumentException)
            {
                Console.WriteLine($"Camera '{cameraName}' seems to be no real camera. I will skip it...");
            }
        }

        return result;
    }

    private void WriteObjectAsJsonToStream(object cameraSettings, Stream stream)
    {
        var jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        JsonSerializer.Serialize(stream, cameraSettings, jsonOptions);
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

    private void LoadCameraPropertiesFromFile()
    {
        var cameraList = ReadCameraPropertiesFromFile();
        if (_options.CameraName != null)
            cameraList =
                cameraList.FindAll(camera => camera.Name == _options.CameraName);
        cameraList.ForEach(RestorePropertiesOfCamera);
    }

    private List<CameraDto> ReadCameraPropertiesFromFile()
    {
        var fileName = _options.FileName;
        if (fileName == null) throw new ArgumentException("File name must be provided.");
        using var stream = File.OpenRead(fileName);
        var cameraList =
            JsonSerializer.Deserialize(stream, typeof(List<CameraDto>)) as List<CameraDto> ??
            throw new InvalidOperationException($"{fileName} could not be read as camera settings");
        stream.Dispose();
        return cameraList;
    }
    private void RestorePropertiesOfCamera(CameraDto camera)
    {
        var cameraName = camera.Name;
        Console.WriteLine($"Initializing {cameraName}");
        var camController = CameraController.FindCamera(cameraName);
        camController.RestoreProperties(camera);
        Console.WriteLine($"{cameraName} configured.");
    }
}