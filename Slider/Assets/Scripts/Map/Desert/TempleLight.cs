using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempleLight : MonoBehaviour
{
    private float scale;
    public float delta = 0.1f;
    public float flickerSpeed = 0.5f;
    private float currScale;
    private float randOffset;

    private void Awake() {
        scale = transform.localScale.x;
        randOffset = Random.Range(0, 2 * Mathf.PI);
    }

    private void Update() {
        currScale = scale + delta * Mathf.Sin(Time.time * flickerSpeed + randOffset);
        transform.localScale = new Vector3(currScale, currScale, 1);
    } 
    
    
    private int radius = 2;
    public bool dynamic = false;

    public void UpdateLightMask(ref Color[] lightMask, int worldToMaskDX, int worldToMaskDY, int maskSizeX, int maskSizeY, bool dynamicOnly)
    {
        //if(dynamicOnly && !dynamic){
         //   print("aborted: not dynamic");
       // return; }
        if(dynamic)
            print("dynamic");
        Vector2Int lightPos = new Vector2Int((int)transform.position.x, (int)transform.position.y);
        for(int x = -1 * radius; x <= radius; x++)
        {
            for(int y = -1 * radius; y <= radius; y++)
            {
                if((x * x) + (y * y) < (radius * radius))
                {
                    int maskX = lightPos.x + x + worldToMaskDX;
                    int maskY = lightPos.y + y + worldToMaskDY;
                    if (maskX < 0 || maskX >= maskSizeX || maskY < 0 || maskY >= maskSizeY);
                    else
                        lightMask[maskY * maskSizeX + maskX] = Color.white;


                }
                else
                {
                    int res = (x * x) + (y * y);
                   // print($"Fail: {x}, {y}. {res}");
                }
            }
        }
    }
}
