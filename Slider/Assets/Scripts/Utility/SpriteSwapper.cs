using UnityEngine;
using UnityEngine.UI;

//This is a really common operation, so why not.
public class SpriteSwapper : MonoBehaviour
{
    [SerializeField] private Sprite on;
    [SerializeField] private Sprite off;
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private Image image;

    private void Awake()
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
        sr.sprite = on;
    }

    public void TurnOff()
    {
        sr.sprite = off;
    }
}

