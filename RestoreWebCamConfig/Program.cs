using DirectShowLibAdapter;
using RestoreWebCamConfig.CameraAdapter;
using RestoreWebCamConfig.JsonFileAdapter;
using RestoreWebCamConfig.ConsoleAdapter;

namespace RestoreWebCamConfig;

internal static class Program
{
    private static void Main(string[] args)
    {
        var dsDevice = new CameraManager(new DirectShowDeviceAdapterImpl());
        var commandLine = CommandLineParser.GetCommandLineParserFor(args);
        var jsonFileAccess = new JsonFileAccess<IReadOnlyList<CameraDto>>();
        var program = new WebCamConfigUtility(
            dsDevice, 
            commandLine,
            new TextWriterAdapter(Console.Out),
            jsonFileAccess);
        program.Run();
    }

    
    private static void OldMain(string[] args)
    {
        var program = new WebCamConfigUtilityUntested(args);
        program.Run();
    }
}