using System.Text.Json;

namespace RestoreWebCamConfig.JsonFileAdapter;

public class JsonFile<T> : IJsonFile<T>
{
    private readonly string _fileName;

    public JsonFile(string fileName)
    {
        _fileName = fileName;
    }

    public void Save(T content)
    {
        using var stream = File.Create(_fileName);
        var jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        JsonSerializer.Serialize(stream, content, jsonOptions);
        stream.Dispose();
    }

    public T Load()
    {
        using var stream = File.OpenRead(_fileName);
        var fileContent = JsonSerializer.Deserialize(stream, typeof(T));
        var result = (fileContent is T content ? content : default) ??
                     throw new InvalidOperationException(
                         $"Content of {_fileName} does not match the expected type {typeof(T)}");
        stream.Dispose();
        return result;
    }
}