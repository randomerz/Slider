using UnityEngine;

public class JungleBlobJumpTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collider)
    {
        Blob blob = collider.gameObject.GetComponent<Blob>();
        if (blob != null)
        {
            if (blob.IsFadingIn())
            {
                blob.RemoveBlob();
            }
            else
            {
                blob.JumpIntoBin();
            }
        }
    }
}