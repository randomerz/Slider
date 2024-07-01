using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

/// TODO
/// - Make follow path better (have waypoints that are sampled as player moves so it makes 
///   a path, rather than just following the player)
///     > its convincing enough right now prob not worth the time to further improve
/// - Chirps
/// - Work in Desert + Factory scenes
public class ChadFollowPlayer : MonoBehaviour, ISavable
{
    public enum FollowState
    {
        Idle,
        Following,
    }

    private const string FOLLOW_SAVE_STRING = "MiscChadIsFollowingPlayer";

    public bool isFollowingEnabled;
    private FollowState state;
    private STile currentSTileUnderneath = null;

    private float CloseFollowDist { // Stop if this close
        get {
            if (OnDifferentSTileThanPlayer())
            {
                return 0.25f;
            }
            if (AreObstaclesNearPlayer())
            {
                return 0.75f;
            }
            return 1.5f;
        }
    }
    private float FarFollowDist { // Start walking if this far
        get {
            if (OnDifferentSTileThanPlayer())
            {
                return 0.5f;
            }
            if (AreObstaclesNearPlayer())
            {
                return 1.75f;
            }
            return 2.5f;
        }
    }
    [SerializeField] private float closeSpeed = 6.25f;
    [SerializeField] private float farSpeed = 7.75f;

    private Transform playerTransform;
    [SerializeField] private LayerMask obstaclesLayerMask;
    private ContactFilter2D contactFilter2D;

    [SerializeField] private bool doDebugLog;

    [Header("References")]
    public NPC npc;
    public Animator animator;
    public SpriteRenderer spriteRenderer;
    public SpriteRenderer npcPingSpriteRenderer;
    public Collider2D npcCollider;
    public Collider2D npcTrigger;
    public Transform walkEnd;

    private void Start() 
    {
        playerTransform = Player.GetInstance().transform;

        contactFilter2D = new ContactFilter2D();
        contactFilter2D.SetLayerMask(obstaclesLayerMask);
        contactFilter2D.useTriggers = false;

        // for dev
        if (!PlayerInventory.Contains("Boots", Area.Jungle))
        {
            closeSpeed -= 2;
            farSpeed -= 2;
        }

        if (isFollowingEnabled)
        {
            SetFollowingPlayer(true);
            TeleportToPlayer();
        }

        if (walkEnd == null)
        {
            Debug.LogError($"walkEnd is not assigned! Why don't we just create it? I don't know probably something to do with execution order.");
        }
    }

    private void OnEnable() 
    {
        if (isFollowingEnabled)
        {
            SubscribeEvents();
        }
    }

    private void SubscribeEvents()
    {
        Portal.OnTimeChange += CheckOnTimeChange;
        Player.OnHousingChanged += CheckOnHousingChange;
    }

    private void OnDisable() 
    {
        UnsubscribeEvents();
    }

    private void UnsubscribeEvents()
    {
        Portal.OnTimeChange -= CheckOnTimeChange;
        Player.OnHousingChanged -= CheckOnHousingChange;
    }

    public void Save()
    {
        SaveSystem.Current.SetBool(FOLLOW_SAVE_STRING, isFollowingEnabled);
    }

    public void Load(SaveProfile profile)
    {
        isFollowingEnabled = profile.GetBool(FOLLOW_SAVE_STRING);
    }


    private void Update() 
    {
        if (isFollowingEnabled)
        {
            UpdateCurrentSTileUnderneath();
            HandleFollow();
        }
    }
    
    public void SetFollowingPlayer(bool value)
    {
        isFollowingEnabled = value;
        npcCollider.enabled = !value;
        npcTrigger.enabled = !value;
        if (npcPingSpriteRenderer != null)
            npcPingSpriteRenderer.enabled = !value;

        if (isFollowingEnabled)
        {
            SubscribeEvents();
        }
        else
        {
            UnsubscribeEvents();
        }
    }

    private void HandleFollow()
    {
        switch (state)
        {
            case FollowState.Idle:
                // Check if player is too far, then follow
                if (!IsWithinDistance(FarFollowDist))
                {
                    SetState(FollowState.Following);
                    break;
                }

                break;

            case FollowState.Following:
                // Check if close enough, then stop
                if (IsWithinDistance(CloseFollowDist))
                {
                    SetState(FollowState.Idle);
                    break;
                }

                UpdateWalkTransforms();
                UpdateSpeed();
                DoWalk();

                break;
        }
    }

    private bool IsWithinDistance(float distance)
    {
        return Vector3.Distance(transform.position, playerTransform.position) <= distance;
    }

    private void SetState(FollowState state)
    {
        this.state = state;

        switch (state)
        {
            case FollowState.Idle:

                animator.SetBool("isWalking", false);

                break;

            case FollowState.Following:

                UpdateWalkTransforms();
                UpdateSpeed();
                
                animator.SetBool("isWalking", true);

                DoWalk();

                break;
        }
    }

    private void UpdateWalkTransforms()
    {
        if(playerTransform == null) return;
        // walkStart.position = transform.position;
        Vector3 dirPlayerToMe = (transform.position - playerTransform.position).normalized;
        float distToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        walkEnd.position = playerTransform.position + Mathf.Min(distToPlayer, CloseFollowDist / 2) * dirPlayerToMe;
    }

    private void UpdateSpeed()
    {
        if(playerTransform == null) return;
        float distToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        float newSpeed = Map(CloseFollowDist, FarFollowDist, closeSpeed, farSpeed, distToPlayer);
        npc.speed = newSpeed;
    }

    private void DoWalk()
    {
        float distToTarget = Vector3.Distance(transform.position, walkEnd.position);
        float speed = npc.speed;
        if (speed * Time.deltaTime >= distToTarget)
        {
            // Reached target
            transform.position = walkEnd.transform.position;
        }
        else
        {
            // Move towards target
            Vector3 dirToTarget = (walkEnd.position - transform.position).normalized;
            transform.position = transform.position + speed * Time.deltaTime * dirToTarget;

            bool isWalkingLeft = dirToTarget.x <= 0;
            spriteRenderer.flipX = isWalkingLeft != npc.spriteDefaultFacingLeft;
        }
    }

    private float Map(float a, float b, float x, float y, float value)
    {
        return Mathf.Lerp(x, y, Mathf.InverseLerp(a, b, value));
    }

    private void CheckOnTimeChange(object sender, Portal.OnTimeChangeArgs e) => TeleportToPlayer();
    private void CheckOnHousingChange(object sender, Player.HousingChangeArgs e) => TeleportToPlayer();

    private void TeleportToPlayer()
    {
        if(playerTransform == null) return;
        UpdateWalkTransforms();
        UpdateSpeed();
        transform.position = playerTransform.position;
        UpdateCurrentSTileUnderneath();
        SetState(FollowState.Idle);
    }

    private bool AreObstaclesNearPlayer()
    {
        Collider2D[] hits = new Collider2D[5];
        int numHits = Physics2D.OverlapCircle(playerTransform.position, 1.5f, contactFilter2D, hits);

        if (doDebugLog)
        {
            Debug.Log($"[ChadFollowPlayer] Hit {hits.Length} objects near player.");
            if (hits.Length > 0)
            {
                Debug.Log($"[ChadFollowPlayer] First object hit: {hits[0].gameObject.name}");
            }
        }

        return numHits > 0;
    }

    private void UpdateCurrentSTileUnderneath()
    {
        STile stileUnderneath = SGrid.GetSTileUnderneath(transform, currentSTileUnderneath);
        if (stileUnderneath != currentSTileUnderneath)
        {
            currentSTileUnderneath = stileUnderneath;
            if(stileUnderneath == null)
                transform.SetParent(null);
            else
                transform.SetParent(stileUnderneath.transform);
        }
    }

    private bool OnDifferentSTileThanPlayer()
    {
        return currentSTileUnderneath != Player.GetInstance().GetSTileUnderneath();
    }

    public void IsFollowingPlayerEnabled(Condition c) => c.SetSpec(isFollowingEnabled);
}
