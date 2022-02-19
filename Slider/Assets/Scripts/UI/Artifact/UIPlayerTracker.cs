using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPlayerTracker : MonoBehaviour
{

    private Vector2 playerPosition;
    private ArtifactTileButton currentButton;
    private STile currentTile;
    private float scale = 26f/17f;
    private float centerScale = 36f/17f;
    public Image image;
    public UIArtifact artifact;
    public GameObject artifactPanel;
    public Vector2 center = new Vector2(17, 17);

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        playerPosition = Player.GetPosition();

        int sTileID = Player.GetStileUnderneath();
        Vector2 offsetPosition;

        if(sTileID == -1){
            image.rectTransform.SetParent(artifactPanel.GetComponent<RectTransform>());
            offsetPosition = (playerPosition - center) * centerScale;

            image.rectTransform.anchoredPosition = offsetPosition;

            return;
        }

        currentTile = SGrid.current.GetStile(sTileID);
        currentButton = artifact.GetButton(Player.GetStileUnderneath());

        offsetPosition = (playerPosition - (Vector2)currentTile.transform.position) * scale;
        image.rectTransform.SetParent(currentButton.GetComponent<RectTransform>());
        
        if(Player.GetIsInHouse())
        {
            image.rectTransform.anchoredPosition = Vector2.zero;
        }
        else
        {
            image.rectTransform.anchoredPosition = offsetPosition;
        }
    }
}
