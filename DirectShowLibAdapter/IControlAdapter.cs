namespace DirectShowLibAdapter;

internal interface IControlAdapter
{
    string GetControlTypeName();
    string GetPropertyName(int propertyId);
    int Update(int propertyId, out int value, out bool isAutomaticallyAdapting);
    int GetPropertyMetaData(int propertyId, out bool canAutoAdapt, out int @default, out int minValue, out int maxValue, out int steppingDelta);
    int SetPropertyState(int propertyId, int value, bool autoAdapt);
}