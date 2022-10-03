using System.Text.Json;
using NDesk.Options;

namespace RestoreWebCamConfig;

class WebCamConfigUtility
{
    private readonly Options _options;
    private readonly List<string> _commands;

    public WebCamConfigUtility(string[] args)
    {
        _options = new Options();
        OptionSet optionSet = CreateOptionSet(_options);
        _commands = optionSet.Parse(args);
        Console.WriteLine($"Options: {_options}");
        Console.Write($"Commands to execute: ");
        _commands.ForEach(v => Console.Write($" {v},"));
        Console.WriteLine("");
        
    }

    private OptionSet CreateOptionSet(Options options)
    {
        var optionSet = new OptionSet () {
            { "camera|c=", "name of the camera to work with", (value) => { options.CameraName = value; } },
            { "file|f=", "name of the file to work with", (value) => { options.FileName = value; } },
            { "h|?|help",           "show help text", _ => { PrintHelp(); } },
        };
        return optionSet;
    }

    private void PrintHelp()
    {
        Console.WriteLine("RestoreWebCamConfig [<Option>...<Option>] command");
        Console.WriteLine("     Commands are");
        Console.WriteLine("         dump - dump the settings of connected cameras into the file referred by option -f");
        Console.WriteLine("         names - show the names of connected cameras");
        Console.WriteLine("         config - set the cams to the settings provided in the file referred by option -f.");
        Console.WriteLine("                  May be restricted to the camera identified by -c.");
        
        CreateOptionSet(new Options()).WriteOptionDescriptions(Console.Out);
        Environment.Exit(0);
    }
    
    public void Run()
    {
        int numberOfCommands = _commands.Count;
        if (numberOfCommands > 1)
        {
            throw new ArgumentException($"Only one command per run is possible. Found {_commands}");
        }
        if ( numberOfCommands == 0)
        {
            InitializeCamera("Logitech BRIO");
        }
        else
        {
            PerformCommand(_commands[0]);
        } 
    }

    private void PerformCommand(string command)
    {
        switch (command)
        {
            case "dump" : 
                DumpCameraSettings();
                break;
            case "names" :
                DumpCameraNames();
                break;
            case "config" :
                RestoreCameraSettingsFromFile();
                break;
            default:
                throw new ArgumentException($"Command '{command}' is not supported.");
        }
    }

    private void DumpCameraSettings()
    {
        List<string> DetermineListOfCamerasToProcess()
        {
            List<string> list;
            if (_options.CameraName != null)
            {
                list = new List<string>();
                list.Add(_options.CameraName);
            }
            else
            {
                list = CameraController.GetKnownCameraNames();
            }

            return list;
        }

        var cameraNameList = DetermineListOfCamerasToProcess();
        if (cameraNameList.Count == 0)
            throw new MissingMemberException("No camera device found.");
        string fileName = _options.FileName ?? 
                          throw new ArgumentException("Filename must be set to dump camera settings");
        var stream = File.Create(fileName);
        var cameraSettingsList = GetCameraSettingsList(cameraNameList);
        WriteObjectAsJsonToStream(cameraSettingsList, stream);
        stream.Dispose();
    }

    private List<CameraSettings> GetCameraSettingsList(List<string> cameraNameList)
    {
        var result = new List<CameraSettings>();
        foreach (var cameraName in cameraNameList)
        {
            Console.WriteLine($"get config for {cameraName}");
            try
            {
                var camera = CameraController.FindCamera(cameraName);
                CameraSettings settings = GetCameraSettings(camera);
                result.Add(settings);
            }
            catch (ArgumentException)
            {
                Console.WriteLine($"Camera '{cameraName}' seems to be no real camera. I will skip it...");
            }
        }

        return result;
    }

    private void WriteObjectAsJsonToStream(Object cameraSettings, Stream stream)
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
        {
            Console.WriteLine($"Cam found: {cameraName}");
        }
    }

    private void RestoreCameraSettingsFromFile()
    {
        var cameraSettingsList = ReadCameraSettingsListFromFile();
        if (_options.CameraName != null)
        {
            cameraSettingsList = 
                cameraSettingsList.FindAll(cameraSettings => cameraSettings.CameraName == _options.CameraName);
        }
        cameraSettingsList.ForEach(cameraSettings => InitializeCamera(cameraSettings));
    }

    private List<CameraSettings> ReadCameraSettingsListFromFile()
    {
        string? fileName = _options.FileName;
        if (fileName == null) throw new ArgumentException("File name must be provided.");
        using var stream = File.OpenRead(fileName);
        var cameraSettingsList = JsonSerializer.Deserialize(stream, typeof(List<CameraSettings>)) as List<CameraSettings> ??
                                 throw new InvalidOperationException($"{fileName} could not be read as camera settings");
        stream.Dispose();
        return cameraSettingsList;
    }

    private static void InitializeCamera(string? camName)
    {
        Console.WriteLine($"Initializing {camName} with hardcoded values");
        CameraController camController = CameraController.FindCamera(camName);
        camController.SetManualZoom(160);
        camController.SetManualFocus(0);
        camController.SetExposure(-6);
        camController.SetPan(4);
        camController.SetTilt(-5);
        camController.SetBrightness(129);
        camController.SetContrast(139);
        camController.SetSaturation(129);
        camController.SetSharpness(0);
        camController.SetWhiteBalance(2800);
        camController.SetBackLightCompensation(0);
        camController.SetGain(66);
        camController.SetPowerLineFrequency(PowerlineFrequency.Hz50);
        camController.SetLowLightCompensation(false);
        Console.WriteLine($"{camName} configured.");
    }

    private void InitializeCamera(CameraSettings settings)
    {
        string? cameraName = settings.CameraName;
        Console.WriteLine($"Initializing {cameraName}");
        CameraController camController = CameraController.FindCamera(cameraName);
        camController.SetManualZoom(settings.ManualZoom);
        camController.SetManualFocus(settings.ManualFocus);
        camController.SetExposure(settings.Exposure);
        camController.SetPan(settings.Pan);
        camController.SetTilt(settings.Tilt);
        camController.SetBrightness(settings.Brightness);
        camController.SetContrast(settings.Contrast);
        camController.SetSaturation(settings.Saturation);
        camController.SetSharpness(settings.Sharpness);
        camController.SetWhiteBalance(settings.WhiteBalance);
        camController.SetBackLightCompensation(settings.BackLightCompensation);
        camController.SetGain(settings.Gain);
		camController.SetPowerLineFrequency(settings.PowerlineFrequency);
        camController.SetLowLightCompensation(settings.LowLightCompensation);
        Console.WriteLine($"{cameraName} configured.");
    }
		
    private static CameraSettings GetCameraSettings(string? cameraName)
    {
        CameraController cameraController = CameraController.FindCamera(cameraName);
        return GetCameraSettings(cameraController);
    }

    private static CameraSettings GetCameraSettings(CameraController cameraController)
    {
        return new CameraSettings
        {
            CameraName = cameraController.getName(),
            ManualZoom = cameraController.GetManualZoom(),
            ManualFocus = cameraController.GetManualFocus(),
            Exposure = cameraController.GetExposure(),
            Pan = cameraController.GetPan(),
            Tilt = cameraController.GetTilt(),
            Brightness = cameraController.GetBrightness(),
            Contrast = cameraController.GetContrast(),
            Saturation = cameraController.GetSaturation(),
            Sharpness = cameraController.GetSharpness(),
            WhiteBalance = cameraController.GetWhiteBalance(),
            BackLightCompensation = cameraController.GetBackLightCompensation(),
            Gain = cameraController.GetGain(),
            PowerlineFrequency = cameraController.GetPowerLineFrequency(),
            LowLightCompensation = cameraController.GetLowLightCompensation()
        };
    }
}