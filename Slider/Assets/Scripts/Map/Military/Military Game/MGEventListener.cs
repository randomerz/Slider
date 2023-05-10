
public interface MGEventListener
{
    public void ProcessEvent(MGEvent e);

    public bool EventFinishFlag
    {
        get;
        set;
    }
}

