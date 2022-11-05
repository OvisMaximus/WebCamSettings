namespace RestoreWebCamConfig.JsonFileAdapter;

public class JsonFileAccess<T> : IJsonFileAccess<T>
{
    public IJsonFile<T> CreateJsonFile(string fileName)
    {
        return new JsonFile<T>(fileName);
    }
}