using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class Item : MonoBehaviour 
{

    public string itemName;
    public SpriteRenderer spriteRenderer;
    [SerializeField] private Collider2D myCollider;
    public bool canKeep = false;

    // animation
    
    [SerializeField] private float pickUpDuration;
    [SerializeField] private AnimationCurve xPickUpMotion;
    [SerializeField] private AnimationCurve yPickUpMotion;
    private Vector3 spriteOffset; // for sprite pivot stuff

    [SerializeField] protected GameObject[] enableOnDrop;

    // events
    public UnityEvent OnPickUp;
    public UnityEvent OnDrop;

    public virtual void Awake() {
        spriteOffset = spriteRenderer.transform.localPosition;
    }

    public virtual void PickUpItem(Transform pickLocation, System.Action callback=null) // pickLocation may be moving
    {
        StartCoroutine(AnimatePickUp(pickLocation, callback));
    }

    public virtual STile DropItem(Vector3 dropLocation, System.Action callback=null) 
    {
        StartCoroutine(AnimateDrop(dropLocation, callback));
        
        // Collider2D hit = Physics2D.OverlapPoint(dropLocation, LayerMask.GetMask("Slider"));
        // if (hit == null || hit.GetComponent<STile>() == null)
        // {
        //     gameObject.transform.parent = null;
        //     //Debug.LogWarning("Player isn't on top of a slider!");
        //     return null;
        // }

        // STile hitTile = hit.GetComponent<STile>();
        STile hitStile = SGrid.GetStileUnderneath(gameObject);
        
        if (hitStile == null) 
        {
            gameObject.transform.SetParent(null);
        }
        else 
        {
            gameObject.transform.SetParent(hitStile.transform);
        }

        return hitStile;
    }

    public void SetCollider(bool value)
    {
        myCollider.enabled = value;
    }

    public virtual void OnEquip()
    {
        // Player.SetMoveSpeedMultiplier(1f);
        foreach (GameObject go in enableOnDrop)
        {
            go.SetActive(false);
        }
    }


    protected IEnumerator AnimatePickUp(Transform target, System.Action callback=null)
    {
        foreach (GameObject go in enableOnDrop)
        {
            go.SetActive(false);
        }
        // spriteRenderer.sortingOrder = 1; // bring object to render above others

        float t = 0;

        Vector3 start = new Vector3(transform.position.x, transform.position.y);
        while (t < pickUpDuration)
        {
            float x = xPickUpMotion.Evaluate(t / pickUpDuration);
            float y = yPickUpMotion.Evaluate(t / pickUpDuration);
            Vector3 pos = new Vector3(Mathf.Lerp(start.x, target.transform.position.x, x),
                                      Mathf.Lerp(start.y, target.transform.position.y, y));
            
            spriteRenderer.transform.position = pos + spriteOffset;

            yield return null;
            t += Time.deltaTime;
        }

        transform.position = target.position;
        spriteRenderer.transform.position = target.position + spriteOffset;
        myCollider.enabled = false;
        OnPickUp?.Invoke();
        callback();
    }

    protected IEnumerator AnimateDrop(Vector3 target, System.Action callback = null)
    {
        float t = pickUpDuration;

        //Create 2 dummy transforms for the animation.
        GameObject start = new GameObject("ItemDropStart");
        start.transform.position = transform.position;
        GameObject end = new GameObject("ItemDropEnd");
        end.transform.position = target;

        STile hitStile = SGrid.GetStileUnderneath(end);
        start.transform.parent = hitStile == null ? null : hitStile.transform;
        end.transform.parent = hitStile == null ? null : hitStile.transform;

        myCollider.enabled = true;
        transform.position = end.transform.position;

        //transform.position = target;
        while (t >= 0)
        {
            float x = xPickUpMotion.Evaluate(t / pickUpDuration);
            float y = yPickUpMotion.Evaluate(t / pickUpDuration);
            Vector3 pos = new Vector3(Mathf.Lerp(end.transform.position.x, start.transform.position.x, x),
                                      Mathf.Lerp(end.transform.position.y, start.transform.position.y, y));
            
            spriteRenderer.transform.position = pos + spriteOffset;

            yield return null;
            t -= Time.deltaTime;
        }

        // spriteRenderer.sortingOrder = 0; // bring object to render below others

        spriteRenderer.transform.position = end.transform.position + spriteOffset;
        OnDrop?.Invoke();
        callback();
        Destroy(start);
        Destroy(end);
    }
    
    public virtual void dropCallback()
    {
        foreach (GameObject go in enableOnDrop)
        {
            go.SetActive(true);
        }
    }
}