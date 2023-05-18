using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MGUISquare : MonoBehaviour
{
    [SerializeField] private int x;
    [SerializeField] private int y;
    [SerializeField] private Sprite supplyImage;

    private MGSimulator _sim;
    private Image _displayImg;

    private MGSpace _mgSpace;

    private void Awake()
    {
        _sim = FindObjectOfType<MGSimulator>();
        _displayImg = GetComponent<Image>();

        MGSimulator.AfterInit += SetupEventListeners;
    }

    private void SetupEventListeners(object sender, System.EventArgs e)
    {
        _mgSpace = _sim.GetSpace(x, y);
        _mgSpace.OnSupplyDropSpawn += OnSupplyDrop;
    }

    private void OnDisable()
    {
        Debug.Log("Disabled");
        _mgSpace.OnSupplyDropSpawn -= OnSupplyDrop;
    }

    public void OnSupplyDrop()
    {
        Debug.Log("Supply Drop UI Updated.");
        SetSpawnTile(true);
    }

    public void SetSpawnTile(bool enabled)
    {
        _displayImg.sprite = supplyImage;
    }
}
