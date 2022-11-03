using RestoreWebCamConfig;
using RestoreWebCamConfig.CameraAdapter;
using Xunit;

namespace Tests;

public class WebCamConfigUtilityTest
{
    [Fact]
    public void blah()
    {
        CommandLineParser parser = CommandLineParser.GetCommandLineParserFor(new []{"-f", "test.txt", "brutnick"});
        CameraManager cameraManager = new CameraManager(DirectShowMock.CreateDirectShowMock());
    }
}