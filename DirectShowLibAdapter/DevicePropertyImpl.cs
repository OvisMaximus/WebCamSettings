namespace DirectShowLibAdapter;

public class DevicePropertyImpl : ICameraProperty
{
    private readonly IControlAdapter _controlAdapter;
    private readonly string _name;
    private readonly int _propertyId;
    private readonly bool _canAutoAdapt;
    private readonly int _default;
    private readonly int _maxValue;
    private readonly int _minValue;
    private readonly int _steppingDelta;
    private int _value;
    private bool _isAutomaticallyAdapting;

    internal DevicePropertyImpl(IControlAdapter controlAdapter, int propertyId)
    {
        _controlAdapter = controlAdapter;
        _propertyId = propertyId;
        _name = controlAdapter.GetPropertyName(propertyId);
        var returnCode = controlAdapter.GetPropertyMetaData(propertyId, 
            out _canAutoAdapt, out _default, out _minValue, out _maxValue, out _steppingDelta);
        checkReturnCode(returnCode, $"Property {_name} is not supported.");
    }

    private void UpdatePropertyState()
    {
        var returnCode = _controlAdapter.Update(_propertyId, out _value, out _isAutomaticallyAdapting);
        checkReturnCode(returnCode, $"Failed to get state of {_name}");
    }

    private void checkReturnCode(int returnCode, string message)
    {
        if (returnCode != 0)
        {
            throw new InvalidOperationException(message);
        }
    }

    private void ChangeState(int value, bool autoAdapt)
    {
        var returnCode = _controlAdapter.SetPropertyState(_propertyId, value, autoAdapt);
        checkReturnCode(returnCode, $"Failed to change state of {_name}");
    }

    public string GetName()
    {
        return _name;
    }

    public int GetValue()
    {
        UpdatePropertyState();
        return _value;
    }

    public void SetValue(int value)
    {
        UpdatePropertyState();
        ChangeState(value, _isAutomaticallyAdapting);
    }

    public bool IsAutoAdapt()
    {
        UpdatePropertyState();
        return _isAutomaticallyAdapting;
    }

    public void SetAutoAdapt(bool autoAdapt)
    {
        UpdatePropertyState();
        ChangeState(_value, autoAdapt);
    }

    public bool HasAutoAdaptCapability()
    {
        return _canAutoAdapt;
    }

    public int GetDefaultValue()
    {
        return _default;
    }

    public int GetMinValue()
    {
        return _minValue;
    }

    public int GetMaxValue()
    {
        return _maxValue;
    }

    public int GetValueIncrementSize()
    {
        return _steppingDelta;
    }

    public override string ToString()
    {
        return $"{_controlAdapter.GetControlTypeName()} Property {_name}: canAutoAdapt: {_canAutoAdapt}, default: {_default}, max: {_maxValue}, min: {_minValue}, increment size: {_steppingDelta}, value: {_value}, isAuto: {_isAutomaticallyAdapting}";
    }
}