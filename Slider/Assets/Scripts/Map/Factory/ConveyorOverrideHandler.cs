using UnityEngine;

//L: lol
public class ConveyorOverrideHandler : MonoBehaviour
{
    public bool IsOn => GetComponent<ElectricalNode>().Powered;

    public System.Action OnOverrideStart;
    public System.Action OnOverrideEnd;

    public void TurnOn()
    {
        AudioManager.Play("Power On");
        OnOverrideStart?.Invoke();
    }

    public void TurnOff()
    {
        OnOverrideEnd?.Invoke();
        AudioManager.Play("Power Off");
    }
}
