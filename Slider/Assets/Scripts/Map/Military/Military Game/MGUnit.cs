
using System;

public class MGUnit
{
    private MGUnitData _data;
    public MGUnitData Data => _data;
    private MGSpace _currSpace;
    public MGSpace CurrSpace => _currSpace;

    public delegate void _OnUnitMove(MGSpace oldSpace, MGSpace newSpace);
    public event _OnUnitMove OnUnitMove;

    public delegate void _OnUnitDestroy();
    public event _OnUnitDestroy OnUnitDestroy;

    public MGUnit(MGUnitData data, MGSpace currSpace)
    {
        _data = data;
        _currSpace = currSpace;
    }

    public MGUnit(MGUnitData data) : this(data, null)
    {
    }

    public void Destroy()
    {
        OnUnitDestroy?.Invoke();
    }

    public void SetSpace(MGSpace space)
    {
        _currSpace = space;
    }

    public void Move(MGSpace newSpace)
    {
        MGSpace oldSpace = _currSpace;
        SetSpace(newSpace);

        OnUnitMove?.Invoke(oldSpace, newSpace);
    }

    public string GetUnitDescriptor()
    {
        return $"{_data.side}-{_data.job}";
    }
}