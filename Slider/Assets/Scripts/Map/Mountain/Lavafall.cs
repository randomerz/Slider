using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lavafall : MonoBehaviour
{
    private STile sTile;
    [SerializeField] private List<GameObject> objects;
    private bool isActive;

    private void OnEnable() {
        sTile = GetComponentInParent<STile>();
        SGridAnimator.OnSTileMoveEnd += CheckLava;
        SGridAnimator.OnSTileMoveStart += CheckLava;
        SGrid.OnSTileEnabled += CheckLava;
        CheckLava();
    }

    private void OnDisable() {
        SGridAnimator.OnSTileMoveEnd -= CheckLava;
        SGridAnimator.OnSTileMoveStart -= CheckLava;
        SGrid.OnSTileEnabled -= CheckLava;
    }

    public void CheckLava(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        CheckLava();
    }

    public void CheckLava(object sender, SGrid.OnSTileEnabledArgs e)
    {
        CheckLava();
    }

    public void CheckLava(){
        bool shouldBeActive = (sTile.y == 3 && !SGrid.Current.GetStileAt(sTile.x, 2).isTileActive);
        if(shouldBeActive != isActive)
        {
            isActive = shouldBeActive;
            foreach(GameObject go in objects)
                go.SetActive(isActive);
        }
    }
}


