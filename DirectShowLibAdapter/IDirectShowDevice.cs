namespace DirectShowLibAdapter;

public interface ICameraProperty
{
    string GetName();
    int GetValue();
    void SetValue(int value);
    bool HasAutoAdaptCapability();
    bool IsAutoAdapt();
    void SetAutoAdapt(bool autoAdapt);
    int GetDefaultValue();
    int GetMinValue();
    int GetMaxValue();
    int GetValueIncrementSize();
}

public interface ICameraDevice
{
    ICameraProperty GetPropertyByName(string name);
    IReadOnlyList<ICameraProperty> GetPropertiesList();
}

public interface IDirectShowDevice
{
    ICameraDevice GetCameraDeviceByName(string name);
    IReadOnlyList<ICameraDevice> GetCameraDevicesList();
}