using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JungleControllerHoverRenderManager : MonoBehaviour
{
    [SerializeField] private ArtifactTileButton currButton;
    [SerializeField] private ArtifactTileButton linkButton;

    [SerializeField] private Image linkButtonDashed;
    [SerializeField] private Image currButtonSolid;

    // Start is called before the first frame update
    void Start()
    {
        linkButtonDashed.gameObject.SetActive(false);
        currButtonSolid.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        //linkButtonDashed.transform.position = linkButton.transform.position;
        if (!currButton.TileIsActive)
        {
            linkButtonDashed.gameObject.SetActive(false);
            currButtonSolid.gameObject.SetActive(false);
        }
        else
        {
            linkButtonDashed.gameObject.SetActive(true);
            currButtonSolid.gameObject.SetActive(true);

            if (currButton.islandId == 2)
            {
                currButtonSolid.gameObject.transform.position = currButton.transform.position + new Vector3(4, 0, 0);
                linkButtonDashed.gameObject.transform.position = linkButton.transform.position - new Vector3(5, 0, 0);
            }
            else if (currButton.islandId == 3)
            {
                currButtonSolid.gameObject.transform.position = currButton.transform.position - new Vector3(4, 0, 0);
                linkButtonDashed.gameObject.transform.position = linkButton.transform.position + new Vector3(5, 0, 0);

            }

        }
    }

    public void SetDottedControllerFrames(bool v)
    {
        linkButtonDashed.enabled = v;
        currButtonSolid.enabled = v;
    }
}
