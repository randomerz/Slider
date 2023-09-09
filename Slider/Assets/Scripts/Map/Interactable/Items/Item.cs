using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Item : MonoBehaviour, ISavable
{

    public string itemName;
    public SpriteRenderer spriteRenderer;
    [SerializeField] private Collider2D myCollider;
    public bool canKeep = false;
    public float itemRadius = 0.5f;

    // animation
    
    [SerializeField] private float pickUpDuration;
    [SerializeField] private AnimationCurve xPickUpMotion;
    [SerializeField] private AnimationCurve yPickUpMotion;
    private Vector3 spriteOffset; // for sprite pivot stuff

    [SerializeField] protected GameObject[] enableOnDrop;

    // events
    public UnityEvent OnPickUp;
    public UnityEvent OnDrop;

    public string saveString;
    private bool shouldLoadSavedDataOnStart;
    private int savedIslandIdBuffer;
    private Vector3 savedPositionBuffer;
    private bool enableColliderOnTriggerExit;

    public virtual void Awake()
    {
        spriteOffset = spriteRenderer.transform.localPosition;
    }

    private void Start()
    {
        if (shouldLoadSavedDataOnStart)
        {
            if (savedIslandIdBuffer == -1)
            {
                transform.SetParent(SGrid.Current.transform);
            }
            else
            {
                transform.SetParent(SGrid.Current.GetStile(savedIslandIdBuffer).transform);
            }

            transform.localPosition = savedPositionBuffer;
        }
    }

    public virtual void Save()
    {
        if (saveString != null && saveString != "")
        {
            STile stile = SGrid.GetSTileUnderneath(gameObject);
            if (stile == null)
                SaveSystem.Current.SetInt($"{saveString}_STile", -1);
            else
                SaveSystem.Current.SetInt($"{saveString}_STile", stile.islandId);

            // We have to handle a bunch of edge cases in case players quit while holding the object
            Vector3 globalPosition = transform.position;
            Vector3 stileParentPosition = stile == null ? Vector3.zero : stile.transform.position;

            SaveSystem.Current.SetFloat($"{saveString}_LocalX", (globalPosition - stileParentPosition).x);
            SaveSystem.Current.SetFloat($"{saveString}_LocalY", (globalPosition - stileParentPosition).y);
            SaveSystem.Current.SetFloat($"{saveString}_LocalZ", (globalPosition - stileParentPosition).z);
            
            bool isPlayerHoldingThis = PlayerInventory.GetCurrentItem() == this;
            SaveSystem.Current.SetBool($"{saveString}_WasPlayerHolding", isPlayerHoldingThis);
            if (isPlayerHoldingThis)
            {
                SaveSystem.Current.SetFloat($"{saveString}_LocalY", 
                    (globalPosition - stileParentPosition - new Vector3(0, 0.75f)).y);
            }
        }
    }

    public virtual void Load(SaveProfile profile)
    {
        if (saveString != null && saveString != "")
        {
            if (profile.GetInt($"{saveString}_STile", 0) == 0) // check if it's the default value
                return;

            // SGrid.Current.GetSTile isn't accessible at this time of loading
            shouldLoadSavedDataOnStart = true;
            savedIslandIdBuffer = profile.GetInt($"{saveString}_STile");
            float x = profile.GetFloat($"{saveString}_LocalX");
            float y = profile.GetFloat($"{saveString}_LocalY");
            float z = profile.GetFloat($"{saveString}_LocalZ");

            if (profile.GetBool($"{saveString}_WasPlayerHolding"))
            {
                SetCollider(false);
                enableColliderOnTriggerExit = true;
            }

            savedPositionBuffer = new Vector3(x, y, z);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (enableColliderOnTriggerExit)
        {
            enableColliderOnTriggerExit = false;
            SetCollider(true);
        }
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
        STile hitStile = SGrid.GetSTileUnderneath(gameObject);
        
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

    public void SetSortingOrder(int num)
    {
        spriteRenderer.sortingOrder = num;
    }

    public virtual void SetLayer(int layer)
    {
        spriteRenderer.gameObject.layer = layer;
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

        AnimatePickUpEnd(target.position);
        callback();
    }

    public void AnimatePickUpEnd(Vector3 targetPosition)
    {
        transform.position = targetPosition;
        spriteRenderer.transform.position = targetPosition + spriteOffset;
        myCollider.enabled = false;
        OnPickUp?.Invoke();
    }

    protected IEnumerator AnimateDrop(Vector3 target, System.Action callback = null)
    {
        float t = pickUpDuration;

        //Create 2 dummy transforms for the animation.
        GameObject start = new GameObject("ItemDropStart");
        start.transform.position = transform.position;
        GameObject end = new GameObject("ItemDropEnd");
        end.transform.position = target;

        STile hitStile = SGrid.GetSTileUnderneath(end);
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