using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public class JungleButtonMiddleAnimator : MonoBehaviour
{
    [SerializeField] private Image image;
    //[SerializeField] private GameObject artifactPanel;
    [SerializeField] private ArtifactTileButton tile2;
    [SerializeField] private Sprite blankSprite;
    private Sprite connectorSprite;

    public Sprite ConnectorSprite
    {
        get { return connectorSprite; }
        set {
            connectorSprite = value; 
            image.sprite = connectorSprite;
        }
    }

    [SerializeField] private Image pushedDownFrame;

    private FlashWhiteImage icon;

    private bool flickeringStarted;
    private bool flickeringDone;

    // Start is called before the first frame update
    void Awake()
    {
        flickeringStarted = false;
        ConnectorSprite = image.sprite;
        flickeringDone = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (tile2 != null)
        {
            if (!tile2.TileIsActive)
            {
                image.gameObject.SetActive(false);
            } else
            {
                image.gameObject.SetActive(true);
                if (!flickeringStarted)
                {
                    StartCoroutine(NewButtonFlicker(3));
                    flickeringStarted = true;
                }
                if (flickeringDone)
                {
                    SetPushedDown(tile2.buttonAnimator.IsPushedDown);
                    image.sprite = connectorSprite;
                }
            }
        }
    }

    void OnDisable()
    {
        if (flickeringStarted)
        {
            flickeringDone = true;
        }
    }

    // yoinked from ArtifactTileButtonAnimator.cs
    public void SetPushedDown(bool value)
    {
        if (value)
        {
            image.rectTransform.anchoredPosition = new Vector3(18.5f, -1);
            //highlightedFrame.rectTransform.anchoredPosition = new Vector3(0, -1);
            pushedDownFrame.gameObject.SetActive(true);
            // if (!isLightning) highlightedFrame.gameObject.SetActive(false); //We don't disable the highlight if lightning
        }
        else
        {
            image.rectTransform.anchoredPosition = new Vector3(18.5f, 0);
            pushedDownFrame.gameObject.SetActive(false);
            //highlightedFrame.rectTransform.anchoredPosition = new Vector3(0, 0);
        }
    }

    // yoinked a couple of things from ArtifactTileButton.cs
    private IEnumerator NewButtonFlicker(int numFlickers, bool startOnBlank = false)
    {
        if (!startOnBlank)
        {
            SetSpriteToIslandOrEmpty();
            yield return new WaitForSeconds(.25f);
        }
        // Special case since there should only be one
        image.gameObject.SetActive(true);
        //if (icon.gameObject.activeSelf)
        //    icon.Flash(numFlickers);
        

        for (int i = 0; i < numFlickers; i++)
        {
            image.sprite = blankSprite;
            yield return new WaitForSeconds(.25f);
            SetSpriteToIslandOrEmpty();
            yield return new WaitForSeconds(.25f);
        }
        flickeringDone = true;
        image.sprite = connectorSprite;
    }

    // logic yoinked from same script
    public void SetSpriteToIslandOrEmpty()
    {
        if (tile2.TileIsActive)
        {
            image.sprite = connectorSprite;
            
        }
        else
        {
            image.gameObject.SetActive(false);
        }
    }

}
