namespace RestoreWebCamConfig;

internal abstract class CommandBaseImpl : ICommand
{
    private readonly string _keyWord;
    private readonly string _description;
    private readonly Action _operation;
    
    protected CommandBaseImpl(string keyWord, string description, Action operation)
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

    static Dictionary<string, ICommand> CreateCommandDictionary()
    {
        return null;
    }
}