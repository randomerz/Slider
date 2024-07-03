using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LavaFallBottom : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other) 
    {
        Minecart mc = other.GetComponent<Minecart>();
        if(mc != null){
            MinecartState state = mc.mcState;
            if(mc.isMoving && (state == MinecartState.Empty || state == MinecartState.Crystal))
                mc.UpdateState(MinecartState.Lava);
        }

        LavaBucket lavaBucket = other.GetComponent<LavaBucket>();
        if(lavaBucket !=  null)
        {
            lavaBucket.FillBucket();
        }    
    }

}
