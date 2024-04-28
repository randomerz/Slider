using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JungleButtonMiddleAnimator : MonoBehaviour
{
    [SerializeField] private List<GameObject> gameObjectsToEnable;
    [SerializeField] private List<GameObject> gameObjectsToDisable;
    [SerializeField] private Image image;
    [SerializeField] private ArtifactTileButton tile2;

    private bool wasTile2ActiveLastFrame = false;

    public Sprite defaultConnectorSprite;
    public Sprite housingConnectorSprite;

    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    private void Start()
    {
        wasTile2ActiveLastFrame = tile2.TileIsActive;
        foreach (GameObject go in gameObjectsToEnable)
        {
            go.SetActive(tile2.TileIsActive);
        }
        foreach (GameObject go in gameObjectsToDisable)
        {
            go.SetActive(!tile2.TileIsActive);
        }
    }

    void Update()
    {
        if (wasTile2ActiveLastFrame != tile2.TileIsActive)
        {
            wasTile2ActiveLastFrame = true;
            foreach (GameObject go in gameObjectsToEnable)
            {
                go.SetActive(true);
            }
            foreach (GameObject go in gameObjectsToDisable)
            {
                go.SetActive(false);
            }
        }
        if(image != null)
            image.sprite = Player.GetIsInHouse() ? housingConnectorSprite : defaultConnectorSprite;
    }
}
