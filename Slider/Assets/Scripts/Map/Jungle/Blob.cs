using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blob : MonoBehaviour
{

    public Animator animator; 

    public Direction direction;
    public float travelDistance = 10;
    private float traveledDistance = 0;
    bool flip = false;

    public float speed = 1f;

    public void changeFaceDir(bool defaultAnim)
    {
        this.flip = !defaultAnim;
        if (flip)
        {
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.flipX = flip;
        }
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 new_distance = DirectionUtil.D2V(direction) * (speed * 0.01f);
        traveledDistance += new_distance.magnitude;
        this.transform.position = this.transform.position + new Vector3(new_distance.x, new_distance.y, 0);

        if (traveledDistance >= travelDistance)
        {
            Destroy(this.gameObject);
        }
    }
}
