using UnityEngine;

public class MinecartAnimationEventHolder : MonoBehaviour 
{
    public Minecart minecart;

    // Called during animations as an event
    public void SetCornerPercent(float percent)
    {
        minecart.SetCornerPercent(percent);
    }
}