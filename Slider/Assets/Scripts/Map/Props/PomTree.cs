using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PomTree : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public Collider2D myCollider;

    private void Awake() 
    {
        if (spriteRenderer.flipX)
        {
            Vector2 o = myCollider.offset;
            myCollider.offset = new Vector2(-o.x, o.y);
        }
    }
}
