using UnityEngine;

public class MessageInBottleController : MonoBehaviour
{

    public void DestroyBottle()
    {
        if(gameObject != null)
        {
            Destroy(gameObject);
        }
        
    }
}