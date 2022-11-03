namespace RestoreWebCamConfig;

internal class CommandImpl : ICommand
{
    private readonly string _keyWord;
    private readonly string _description;
    private readonly Action _operation;
    
    public CommandImpl(string keyWord, string description, Action operation)
    {
        _keyWord = keyWord;
        _description = description;
        _operation = operation;
    }

    public string GetKeyWord()
    {
        return _keyWord;
    }

    public string GetDescription()
    {
        return _description;
    }

    public void Execute()
    {
        _operation();
    }
}