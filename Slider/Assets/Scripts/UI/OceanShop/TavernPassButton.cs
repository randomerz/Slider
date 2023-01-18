using UnityEngine;
using UnityEngine.UI;

public class TavernPassButton : MonoBehaviour 
{
    public bool isComplete;

    public string rewardName;
    public Image image;

    public Sprite defaultSprite;
    public Sprite completedSprite;
    public Sprite selectedSprite;

    public void Deselect()
    {
        image.sprite = isComplete ? completedSprite : defaultSprite;
    }

    public void Select()
    {
        image.sprite = selectedSprite;
    }
}