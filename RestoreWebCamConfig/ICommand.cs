namespace RestoreWebCamConfig;

internal interface ICommand
{
    public string GetKeyWord();
    public string GetDescription();
    public void Execute();
}