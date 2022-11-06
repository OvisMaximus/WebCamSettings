using DirectShowLibAdapter;
using RestoreWebCamConfig.CameraAdapter;
using RestoreWebCamConfig.JsonFileAdapter;
using RestoreWebCamConfig.ConsoleAdapter;

namespace RestoreWebCamConfig;

internal static class Program
{
    private static void Main(string[] args)
    {
        var program = InstantiateWebCamConfigUtility(args);

        try
        {
            program.Run();
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.Message);
            Environment.Exit(-1);
        }
    }

    private static WebCamConfigUtility InstantiateWebCamConfigUtility(string[] args)
    {
        var dsDevice = new CameraManager(new DirectShowDeviceAdapterImpl());
        var commandLine = CommandLineParser.GetCommandLineParserFor(args);
        var jsonFileAccess = new JsonFileAccess<IReadOnlyList<CameraDto>>();
        var program = new WebCamConfigUtility(
            dsDevice,
            commandLine,
            new TextWriterAdapter(Console.Out),
            jsonFileAccess);
        return program;
    }

}