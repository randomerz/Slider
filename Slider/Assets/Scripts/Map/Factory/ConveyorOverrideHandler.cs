using UnityEngine;

//L: lol
public class ConveyorOverrideHandler : MonoBehaviour
{
    public bool IsOn => GetComponent<ElectricalNode>().Powered;
    public void TurnOn()
    {
        AudioManager.Play("Power On");
    }

    public void TurnOff()
    {
        AudioManager.Play("Power Off");
    }
}
