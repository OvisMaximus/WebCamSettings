namespace RestoreWebCamConfig.JsonFileAdapter;

public interface IJsonFile<T>
{
    void Save(T content);
    T Load();
}