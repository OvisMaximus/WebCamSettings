using System;
using RestoreWebCamConfig;
using Xunit;

namespace Tests;

public class ApplicationTests
{
    [Fact]
    public void blah()
    {
        var options = new Options();
        var parser = CommandLineParser.getParserWritingInto(options);
    }

}

public class CommandLineParser
{
    public static CommandLineParser getParserWritingInto(Options options)
    {
        throw new NotImplementedException();
    }
}