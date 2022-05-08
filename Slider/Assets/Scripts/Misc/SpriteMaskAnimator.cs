using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteMaskAnimator : MonoBehaviour
{
    public SpriteMask spriteMask;

    public List<Sprite> sprites;

    public float delay = 0.5f;

    private void Start() 
    {
        StartCoroutine(Animate());
    }

    private IEnumerator Animate()
    {
        int i = 0;
        while (true)
        {
            spriteMask.sprite = sprites[i];
            i = (i + 1) % sprites.Count;

            yield return new WaitForSeconds(delay);
        }
    }
}
