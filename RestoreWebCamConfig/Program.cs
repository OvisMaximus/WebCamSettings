namespace RestoreWebCamConfig;

internal static class Program
{
    private static void Main(string[] args)
    {
        var program = new WebCamConfigUtility(args);
        program.Run();
    }
}