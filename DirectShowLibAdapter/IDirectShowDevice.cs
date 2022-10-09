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
    string GetDeviceName();
    ICameraProperty GetPropertyByName(string propertyName);
    IReadOnlyList<ICameraProperty> GetPropertiesList();
}

public interface IDirectShowDevice
{
    ICameraDevice GetCameraDeviceByName(string deviceName);
    IReadOnlyList<ICameraDevice> GetCameraDevicesList();
}