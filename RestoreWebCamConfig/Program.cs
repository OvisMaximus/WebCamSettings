namespace RestoreWebCamConfig;

internal static class Program
{
    private static void Main(string[] args)
    {
        var program = new WebCamConfigUtilityUntested(args);
        program.Run();
    }
}