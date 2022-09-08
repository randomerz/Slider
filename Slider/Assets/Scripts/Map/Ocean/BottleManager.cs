using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BottleManager : MonoBehaviour
{
    public GameObject bottlePrefab;
    public bool puzzleSolved = false;
    
    private int turncounter = 0;
    private GameObject bottle = null;
    private bool puzzleActive = false;
    private STile bottleParentStile = null;
    private string validTiles = "138"; //these tiles have a water path for the bottle
    [SerializeField] private Vector3 bottleInitialLocation = new Vector3(-7.5f,41.5f);
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

    private IEnumerator StartBottleMovementAnimation(Vector3 start, Vector3 end, float moveDuration)
    {
        float t = 0;
        while(t < moveDuration)
        {
            t += Time.deltaTime;
            bottle.transform.localPosition = Vector3.Lerp(start, end, t/moveDuration);
            yield return null;
        }
        bottle.transform.localPosition = end;
    }

    private void UpdateBottleLocation(object sender, SGridAnimator.OnTileMoveArgs tileMoveArgs){
        if(puzzleActive && tileMoveArgs.stile.islandId == bottleParentStile.islandId && !bottleParentStile.hasAnchor)
        {
            //Debug.Log("wha");
            turncounter++;
            if (turncounter >2)
            {
                DestroyBottle();
            }
            else
                StartCoroutine(StartBottleMovementAnimation(positions[turncounter-1] ,positions[turncounter], tileMoveArgs.moveDuration));
        }
    }

    private IEnumerator DestroyBottleExecutor()
    {
        //Debug.Log("deleting bottle");
        if(bottle != null)
            bottle.GetComponent<Animator>().SetBool("IsSinking", true);
        UITrackerManager.RemoveTracker(bottle);
        bottle = null;
        puzzleActive = false;
        bottleParentStile = null;
        yield return null;
        
    }

    //so that juliet can call it
    public void DestroyBottle()
    {
        StartCoroutine(DestroyBottleExecutor());
    }

    public void CreateNewBottle()
    {
        //create new bottle after talking to romeo and puzzle is not solved
        if(bottle == null && !puzzleSolved)
        {
            puzzleActive = true;

            //Debug.Log("Creating new bottle");
            bottle = GameObject.Instantiate(bottlePrefab, bottleInitialLocation, Quaternion.identity);

            bottleParentStile = SGrid.Current.GetGrid()[0, 2];
            //Debug.Log("bound stile: " + bottleParentStile);
            bottle.transform.SetParent(bottleParentStile.transform);

            if(CheckGrid.contains(SGrid.GetGridString(),$"[{validTiles}].._..._..."))
            {
                turncounter = 0;
                bottle.transform.localPosition = positions[turncounter];
                UITrackerManager.AddNewTracker(bottle, UITrackerManager.DefaultSprites.circleEmpty);
                //Debug.Log("valid tile");
            }
            else
            {
                //Debug.Log("invalid tile");
                DestroyBottle();
            }


            
        }
    }

    public void InvalidTile(Condition c)
    {
       //only tiles 1,3, and 8 have a water path for the bottle to go through
       if( CheckGrid.contains(SGrid.GetGridString(),$"[{validTiles}].._..._..."))
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
        if ((turncounter < 3 && bottleParentStile != null && bottleParentStile.x == 2 && bottleParentStile.y == 0) || puzzleSolved)
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


