using UnityEngine;

public class AlienCommanderGun : MonoBehaviour
{
    public GameObject gunObject;
    public FlashWhiteSprite flashWhiteSprite;

    public void EnableGun()
    {
        gunObject.SetActive(true);
        flashWhiteSprite.Flash(1);
        AudioManager.Play("UI Click World", transform);
    }
}