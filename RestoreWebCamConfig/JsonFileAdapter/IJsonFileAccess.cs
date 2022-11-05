namespace RestoreWebCamConfig.JsonFileAdapter;

public interface IJsonFileAccess<T>
{
    IJsonFile<T> CreateJsonFile(string fileName);
}