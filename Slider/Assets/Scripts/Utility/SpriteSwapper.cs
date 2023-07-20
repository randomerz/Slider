using UnityEngine;
using UnityEngine.UI;

public class SpriteSwapper : MonoBehaviour
{
    [SerializeField] private Sprite on;
    [SerializeField] private Sprite off;
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] protected Image image;

    protected virtual void Awake()
    {
       if (sr == null)
        {
            sr = GetComponent<SpriteRenderer>();
        } 

       if (image == null)
        {
            image = GetComponent<Image>();
        }
    }

    public void TurnOn()
    {
        if (sr) sr.sprite = on;
        if (image) image.sprite = on;
    }

    public void TurnOff()
    {
        if (sr) sr.sprite = off;
        if (image) image.sprite = off;
    }
}

