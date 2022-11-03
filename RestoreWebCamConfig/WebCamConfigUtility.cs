using RestoreWebCamConfig.CameraAdapter;

namespace RestoreWebCamConfig;

public class WebCamConfigUtility
{
    private readonly CameraManager _cameraManager;
    private readonly CommandLineParser _commandLineParser;

    public WebCamConfigUtility(CameraManager cameraManager, CommandLineParser commandLineParser)
    {
        _cameraManager = cameraManager;
        _commandLineParser = commandLineParser;
    }
}