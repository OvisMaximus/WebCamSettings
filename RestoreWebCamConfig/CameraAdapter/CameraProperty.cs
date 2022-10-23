using DirectShowLibAdapter;

namespace RestoreWebCamConfig.CameraAdapter;

public class CameraProperty
{
    private readonly ICameraProperty _dsProperty;

    internal CameraProperty(ICameraProperty dsProperty)
    {
        _dsProperty = dsProperty;
    }

    public string GetName()
    {
        return _dsProperty.GetName();
    }

    public int GetValue()
    {
        return _dsProperty.GetValue();
    }

    public int GetMinValue()
    {
        return _dsProperty.GetMinValue();
    }

    public int GetMaxValue()
    {
        return _dsProperty.GetMaxValue();
    }

    public int GetDefaultValue()
    {
        return _dsProperty.GetDefaultValue();
    }

    public int GetIncrementSize()
    {
        return _dsProperty.GetValueIncrementSize();
    }

    public bool HasAutoAdaptCapability()
    {
        return _dsProperty.HasAutoAdaptCapability();
    }

    public bool IsAutomaticallyAdapting()
    {
        return _dsProperty.IsAutoAdapt();
    }

    public void SetValue(int newValue)
    {
        _dsProperty.SetValue(newValue);
    }

    public void SetAdaptAutomatically(bool adaptAutomatically)
    {
        _dsProperty.SetAutoAdapt(adaptAutomatically);
    }

    public CameraPropertyDto GetDto()
    {
        var dto = new CameraPropertyDto(GetName())
        {
            Value = GetValue(),
            MinValue = GetMinValue(),
            MaxValue = GetMaxValue(),
            Default = GetDefaultValue(),
            SteppingDelta = GetIncrementSize(),
            CanAdaptAutomatically = HasAutoAdaptCapability(),
            IsAutomaticallyAdapting = IsAutomaticallyAdapting()
        };
        return dto;
    }

    public void RestoreFromDto(CameraPropertyDto dto)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));
        if (null == dto.Name || dto.Name.Trim().Length == 0) 
            throw new InvalidDataException("Can not restore a property without a name.");
        if(_dsProperty.GetName() != dto.Name)
            throw new InvalidDataException($"Can not restore {_dsProperty.GetName()} with data of {dto.Name}.");
            
        SetValue(dto.Value);
        SetAdaptAutomatically(dto.IsAutomaticallyAdapting);
    }
}