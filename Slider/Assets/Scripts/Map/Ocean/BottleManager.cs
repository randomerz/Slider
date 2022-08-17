using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BottleManager : MonoBehaviour
{
    // set in the editor
    public GameObject bottle_prefab; 

    private int turncounter = 0;
    private GameObject bottle = null;
    private bool puzzleActive = false;
    private bool puzzleSolved = false;
    private STile boundSTile = null;

    private List<Vector3> positions = new List<Vector3>{ new Vector3(-4.5f,7.5f), new Vector3(0,0), new Vector3(4.5f,-7.5f)};

    public void OnEnable()
    {
        //subscribe stile move end to update counter
        SGridAnimator.OnSTileMoveStart += UpdateBottleLocation;
    }


    public void OnDisable()
    {
        //unsubscribe stile move end to update counter
        SGridAnimator.OnSTileMoveStart -= UpdateBottleLocation;
    }

    protected IEnumerator StartBottleMovementAnimation(Vector3 start, Vector3 end, float moveDuration)
    {
        float t = 0;
        while(t < moveDuration)
        {
            t += Time.deltaTime;
            bottle.transform.localPosition = Vector3.Lerp(start, end, t/moveDuration);
            yield return null;
        }
    }

    private void UpdateBottleLocation(object sender, SGridAnimator.OnTileMoveArgs tileMoveArgs){
        if(puzzleActive && tileMoveArgs.stile.islandId == boundSTile.islandId)
        {
            turncounter++;
            if (turncounter >2)
            {
                DestroyBottle();
            }
            else
                StartCoroutine(StartBottleMovementAnimation(positions[turncounter-1] ,positions[turncounter], tileMoveArgs.moveDuration));
        }
    }

    private void DestroyBottle()
    {
        Debug.Log("deleting bottle");
        if(bottle != null)
            bottle.GetComponent<Animator>().SetBool("IsSinking", true);
        bottle = null;
        puzzleActive = false;
        boundSTile = null;
        
    }

    public void CreateNewBottle()
    {
        //create new bottle after talking to romeo and puzzle is not solved
        if(bottle == null && !puzzleSolved)
        {
            puzzleActive = true;

            Debug.Log("Creating new bottle");
            bottle = GameObject.Instantiate(bottle_prefab, new Vector3(-7.5f,41.5f), Quaternion.identity);

            boundSTile = SGrid.GetSTileUnderneath(bottle.transform, null);
            Debug.Log("bound stile: " + boundSTile);
            bottle.transform.SetParent(boundSTile.transform);

            if(boundSTile.islandId == 1 || boundSTile.islandId == 3 || boundSTile.islandId == 8)
            {
                turncounter = 0;
                bottle.transform.localPosition = positions[turncounter];
                UITrackerManager.AddNewTracker(bottle, UITrackerManager.DefaultSprites.circleEmpty);
                Debug.Log("valid tile");
            }
            else
            {
                Debug.Log("invalid tile");
                DestroyBottle();
            }


            
        }
    }

    public void InvalidTile(Condition c)
    {
       if( CheckGrid.contains(SGrid.GetGridString(),"[138].._..._..."))
        {
            c.SetSpec(false);
        }
        else
        {
            c.SetSpec(true);
        }
    }

    public void MessageDelivered(Condition c)
    {
        if (turncounter < 3 && boundSTile != null && boundSTile.x == 2 && boundSTile.y == 0)
        {
            c.SetSpec(true);
            puzzleSolved = true;
        }
        else
        {
            c.SetSpec(false);
        }
    }
}


