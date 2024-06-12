using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CreditsMossManager : MonoBehaviour
{
    [SerializeField] private Tilemap mossMap;
    [SerializeField] private float mossFadeSpeed = 3f;

    private void Start()
    {
        foreach(Vector3Int position in mossMap.cellBounds.allPositionsWithin)
        {
            mossMap.SetTileFlags(position, TileFlags.None);
        }
    }

    public void FadeOutMoss()
    {
        StartCoroutine(FadeMoss());
    }

    private IEnumerator FadeMoss()
    {
        float t = mossFadeSpeed;
        while(t > 0)
        {
            SetAlpha(t/mossFadeSpeed);
            t -= Time.deltaTime;
            yield return null;
        }
        SetAlpha(0);
    }

    private void SetAlpha(float alpha)
    {
        Color c = new Color(1.0f, 1.0f, 1.0f, alpha);
        foreach(Vector3Int position in mossMap.cellBounds.allPositionsWithin)
        {
            mossMap.SetColor(position, c);
        }
    }
}
