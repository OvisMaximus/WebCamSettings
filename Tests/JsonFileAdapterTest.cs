using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using RestoreWebCamConfig.JsonFileAdapter;
using Xunit;

namespace Tests;

public class JsonFileAdapterTest
{
    private const string TestFileName = "testFile.txt";

    public record DtoA
    {
        public string FieldA { get; set; }
        public string FieldB { get; set; }
        public string FieldC { get; set; }
    }

    public record DtoB
    {
        public string FieldA { get; set; }
        public DtoA FieldB { get; set; }
        public DtoA FieldC { get; set; }
    }

    private IReadOnlyList<DtoB> getFixture()
    {
        return new List<DtoB>()
        {
            new DtoB()
            {
                FieldA = "NameOfDtoB1",
                FieldB = new DtoA()
                {
                    FieldA = "NameOfDtoA1",
                    FieldB = "blah",
                    FieldC = "blurb"
                },
                FieldC = new DtoA()
                {
                    FieldA = "NameOfDtoA2",
                    FieldB = "naff",
                    FieldC = "sab"
                }
            },
            new DtoB()
            {
            FieldA = "NameOfDtoB2",
            FieldB = new DtoA()
            {
                FieldA = "NameOfDtoB2A1",
                FieldB = "black",
                FieldC = "rabbit"
            },
            FieldC = new DtoA()
            {
                FieldA = "NameOfDtoB2A2",
                FieldB = "green",
                FieldC = "grass"
            }
        }

        }.AsReadOnly() as IReadOnlyList<DtoB>;
    }

    private const string FixtureAsJson = @"[
  {
    ""FieldA"": ""NameOfDtoB1"",
    ""FieldB"": {
      ""FieldA"": ""NameOfDtoA1"",
      ""FieldB"": ""blah"",
      ""FieldC"": ""blurb""
    },
    ""FieldC"": {
      ""FieldA"": ""NameOfDtoA2"",
      ""FieldB"": ""naff"",
      ""FieldC"": ""sab""
    }
  },
  {
    ""FieldA"": ""NameOfDtoB2"",
    ""FieldB"": {
      ""FieldA"": ""NameOfDtoB2A1"",
      ""FieldB"": ""black"",
      ""FieldC"": ""rabbit""
    },
    ""FieldC"": {
      ""FieldA"": ""NameOfDtoB2A2"",
      ""FieldB"": ""green"",
      ""FieldC"": ""grass""
    }
  }
]";    
    
    private static string ReadFileToString(string fileName)
    {
        var streamReader = File.OpenText(fileName);
        var fileContent = streamReader.ReadToEnd();
        streamReader.Dispose();
        return fileContent;
    }
    
    [Fact]
    public void TestSavingObjectsToJsonFile()
    {
        JsonFileAccess<IReadOnlyList<DtoB>> jsonFileAccess = new();
        var jsonFile = jsonFileAccess.CreateJsonFile(TestFileName);

        jsonFile.Save(getFixture());
        
        Assert.True(File.Exists(TestFileName));
        Assert.Equal(FixtureAsJson, ReadFileToString(TestFileName));
    }

}

public class JsonFileAccess<T> : IJsonFileAccess<T>
{
    public IJsonFile<T> CreateJsonFile(string fileName)
    {
        return new JsonFile<T>(fileName);
    }
}

public class JsonFile<T> : IJsonFile<T>
{
    private readonly string _fileName;

    public JsonFile(string fileName)
    {
        _fileName = fileName;
    }

    public void Save(T content)
    {
        var stream = File.Create(_fileName);
        var jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        JsonSerializer.Serialize(stream, content, jsonOptions);
        stream.Dispose();
    }

    public T Load()
    {
        throw new System.NotImplementedException();
    }
}