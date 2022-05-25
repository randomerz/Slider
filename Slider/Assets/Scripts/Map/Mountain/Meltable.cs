using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Meltable : MonoBehaviour
{
    public Sprite frozenSprite;
    public Sprite meltedSprite;

    public SpriteRenderer spriteRenderer;

    public bool isFrozen = true;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(this.transform.position.y > 62 && !isFrozen)
            Freeze();
    }

    public void Melt()
    {
        Debug.Log("melting");
        spriteRenderer.sprite = meltedSprite;
        isFrozen = false;
    }

    public void Freeze()
    {
        Debug.Log("freezing");
        spriteRenderer.sprite = frozenSprite;
        isFrozen = true;
    }

}
