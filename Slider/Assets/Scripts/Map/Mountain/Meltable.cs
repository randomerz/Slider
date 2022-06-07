using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class Meltable : MonoBehaviour
{
    public Sprite frozenSprite; //C: switch to animation later maybe? but that doesn't sound fun
    public Sprite meltedSprite;
    public SpriteRenderer spriteRenderer;
    public Collider2D collider;

    public bool isFrozen = true;

    public UnityEvent onMelt;
    public UnityEvent onFreeze;

    public int numLavaSources = 0;

    // Update is called once per frame
    void Update()
    {
        if(this.transform.position.y > 62 && !isFrozen && numLavaSources <= 0)
            Freeze();
    }

    public void Melt()
    {
        numLavaSources++;
        if(isFrozen && numLavaSources > 0) //C: the second check is pointless but maybe wacky things could happen
        {
            if (spriteRenderer)
                spriteRenderer.sprite = meltedSprite;
            isFrozen = false;
            onMelt.Invoke();
        }
    }

    public void RemoveLava()
    {
        numLavaSources--;
    }

    public void IsFrozen(Conditionals.Condition c) {
        c.SetSpec(isFrozen);
    }

    public void IsNotFrozen(Conditionals.Condition c) {
        c.SetSpec(!isFrozen);
    }

    public void Freeze()
    {
        if (spriteRenderer)
            spriteRenderer.sprite = frozenSprite;
        isFrozen = true;
        onFreeze.Invoke();
    }
}
