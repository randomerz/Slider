using UnityEngine;

public class DinoSpriteRendererCopier : MonoBehaviour
{
    public SpriteRenderer reference;
    public SpriteRenderer mySpriteRenderer;

    private void Update()
    {
        mySpriteRenderer.enabled = reference.enabled;
    }
}