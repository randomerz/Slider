
public class MGUnit
{
    private MGUnitData.Data _data;
    public MGUnitData.Data Data => _data;
    private MGSpace _currSpace;
    public MGSpace CurrSpace => _currSpace;

    public MGUnit(MGUnitData.Data data, MGSpace currSpace)
    {
        _data = data;
        _currSpace = currSpace;
    }

    public MGUnit(MGUnitData.Data data) : this(data, null)
    {
    }

    public void SetSpace(MGSpace space)
    {
        _currSpace = space;
    }

    public string GetUnitDescriptor()
    {
        return $"{_data.side}-{_data.job}";
    }
}