using UnityEngine;
using UnityEngine.UI;

//L: This is a stupid class, but I got tired of adding the same fields over and over.
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
        Debug.Log("Sprite Swapper Off");
        sr.sprite = off;
    }
}

