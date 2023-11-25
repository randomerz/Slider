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

    [SerializeField] private Image pushedDownFrame;

    private FlashWhiteImage icon;

    private bool flickeringStarted;
    private bool flickeringDone;

    // Start is called before the first frame update
    void Awake()
    {
        flickeringStarted = false;
        connectorSprite = image.sprite;
        flickeringDone = false;
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("Connector script works");
        if (tile2 != null)
        {
            Debug.Log($"ICON 1: {icon}");
            if (!tile2.TileIsActive)
            {
                Debug.Log("HALP");
                image.gameObject.SetActive(false);
            } else
            {
                Debug.Log($"PLS / HAS FLICKERED: {flickeringStarted}");
                image.gameObject.SetActive(true);
                if (!flickeringStarted)
                {
                    Debug.Log($"ICON: {icon}");
                    StartCoroutine(NewButtonFlicker(3));
                    flickeringStarted = true;
                }
                if (flickeringDone)
                {
                    SetPushedDown(tile2.buttonAnimator.IsPushedDown);

                }
            }
        }
    }

    // yoinked from ArtifactTileButtonAnimator.cs
    public void SetPushedDown(bool value)
    {
        Debug.Log($"PUSHED DOWN: {value}");
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
        Debug.Log("Connector flicker activated");
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
    }

    public void SetSpriteToIslandOrEmpty()
    {
        if (tile2.TileIsActive)
        {
            // TODO:  correction ->                     should be completed sprite else default sprite
            //tile2.buttonAnimator.sliderImage.sprite = tile2.isComplete ? image.sprite : image.sprite;
            image.sprite = connectorSprite;
            
        }
        else
        {
            image.gameObject.SetActive(false);
        }
    }

}
