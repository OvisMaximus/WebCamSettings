using System.Collections.Generic;
using System.IO;
using RestoreWebCamConfig.JsonFileAdapter;
using Xunit;

namespace Tests;

public class JsonFileAdapterTest
{
    private const string TestFileName = "testFile.txt";

    // ReSharper disable UnusedAutoPropertyAccessor.Global
    public record DtoA
    {
        public string? FieldA { get; set; }
        public string? FieldB { get; set; }
        public string? FieldC { get; set; }
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public record DtoB
    {
        public string? FieldA { get; set; }
        public DtoA? FieldB { get; set; }
        public DtoA? FieldC { get; set; }
    }
    // ReSharper restore UnusedAutoPropertyAccessor.Global

    private static IReadOnlyList<DtoB> GetFixture()
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

        }.AsReadOnly();
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

        jsonFile.Save(GetFixture());
        
        Assert.True(File.Exists(TestFileName));
        Assert.Equal(FixtureAsJson, ReadFileToString(TestFileName));
    }

    [Fact]
    public void TestReadingFileToTypedObject()
    {
        JsonFileAccess<IReadOnlyList<DtoB>> jsonFileAccess = new();
        var jsonFile = jsonFileAccess.CreateJsonFile(TestFileName);

        var theObject = jsonFile.Load();
        
        Assert.NotNull(theObject);
        Assert.Equal(GetFixture(), theObject);
    }

}