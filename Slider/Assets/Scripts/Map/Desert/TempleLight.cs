using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempleLight : MonoBehaviour
{
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
