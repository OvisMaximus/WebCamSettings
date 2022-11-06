namespace RestoreWebCamConfig.ConsoleAdapter;

public class TextWriterAdapter : ITextWriter
{
    private readonly TextWriter _textWriterImplementation;

    public TextWriterAdapter(TextWriter aDelegate)
    {
        _textWriterImplementation = aDelegate;
    }

    public void Write(string text)
    {
        _textWriterImplementation.Write(text);
    }

    public void WriteLine(string text)
    {
        _textWriterImplementation.WriteLine(text);
    }
}
