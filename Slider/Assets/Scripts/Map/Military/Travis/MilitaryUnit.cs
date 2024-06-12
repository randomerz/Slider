using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class MilitaryUnit : MonoBehaviour
{
    public static System.EventHandler<UnitArgs> OnAnyUnitDeath;

    public const int STILE_WIDTH = 13;

    private static readonly List<MilitaryUnit> activeUnits = new();
    public static MilitaryUnit[] ActiveUnits { get => activeUnits.ToArray(); }

    [SerializeField] private Type _unitType;
    public Type UnitType { get => _unitType; }

    [SerializeField] private Team _unitTeam;
    public Team UnitTeam { get => _unitTeam; }

    [SerializeField] private Status _unitStatus;
    public Status UnitStatus { get => _unitStatus; }

    [SerializeField] private MilitaryNPCController _npcController;
    public MilitaryNPCController NPCController { get => _npcController; }

    [SerializeField] private Vector2Int _gridPosition;
    public Vector2Int GridPosition { 
        get => _gridPosition; 
        set
        {
            _gridPosition = value;
            StartCombatWithOverlappingEnemyUnit();
        }
    }

    [FormerlySerializedAs("attachedSTile")]
    [SerializeField] private STile _attachedSTile;
    public STile AttachedSTile { 
        get => _attachedSTile; 
        set
        {
            _attachedSTile = value;
            transform.SetParent(value == null ? null : value.transform);
        }
    }

    [SerializeField] private MilitaryUnitCommander _commander;
    public MilitaryUnitCommander Commander
    {
        get => _commander;
        set
        {
            if (_commander != null)
            {
                _commander.RemoveUnit(this);
            }
            _commander = value;
            _commander.AddUnit(this);
        }
    }
    
    public UnityEvent OnDeath;

    private void Awake()
    {
        RegisterUnit(this);
        if (Commander != null)
        {
            Commander.AddUnit(this);
        }
    }

    private void Start()
    {
        // TODO: Make this actually work
        STile parentSTile = SGrid.GetSTileUnderneath(gameObject);
        if (parentSTile != null)
        {
            GridPosition = new Vector2Int(parentSTile.x, parentSTile.y);
            // AttachedSTile = parentSTile;
        }

        MilitaryUITrackerManager.AddUnitTracker(this);
    }

    private void OnEnable()
    {
        SGridAnimator.OnSTileMoveEnd += OnTileMove;
    }

    private void OnDisable()
    {
        SGridAnimator.OnSTileMoveEnd -= OnTileMove;
    }

    public static void RegisterUnit(MilitaryUnit unit)
    {
        unit._unitStatus = Status.Active;
        activeUnits.Add(unit);
        Debug.Log($"Registered Unit '{unit.gameObject.name}'");
    }

    public static void UnregisterUnit(MilitaryUnit unit)
    {
        unit._unitStatus = Status.Inactive;
        activeUnits.Remove(unit);
        MilitaryUITrackerManager.RemoveUnitTracker(unit);
        Debug.Log($"Unregistered Unit '{unit.gameObject.name}'");
    }

    public static Vector2 GridPositionToWorldPosition(Vector2Int tilePosition)
    {
        return new Vector2(tilePosition.x * STILE_WIDTH, tilePosition.y * STILE_WIDTH);
    }

    public static Vector2Int WorldPositionToGridPosition(Vector2 worldPosition)
    {
        Debug.LogWarning($"WorldPositionToGridPosition might be unreliable -- try to use an alternative!");
        return new Vector2Int(Mathf.RoundToInt(worldPosition.x / STILE_WIDTH), Mathf.RoundToInt(worldPosition.y / STILE_WIDTH));
    }

    public void OnTileMove(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        if (AttachedSTile != null && e.stile == AttachedSTile)
        {
            GridPosition = new Vector2Int(e.stile.x, e.stile.y);
        }
    }

    public void InitializeNewUnit(Type type)
    {
        _unitType = type;
        if (_npcController != null)
            _npcController.UpdateSpriteTypes();
    }

    public void KillUnit()
    {
        _unitStatus = Status.Dead;
        MilitaryTurnAnimator.AddToQueueFront(new MGDeath(this));
    }

    public void DoDeathAnimation(System.Action onAnimationResolved)
    {
        if (_npcController != null)
        {
            _npcController.AnimateDeath();
        }

        CoroutineUtils.ExecuteAfterDelay(() => {
            Cleanup();
            onAnimationResolved?.Invoke();
        }, this, 0.25f);
    }

    public void KillImmediate()
    {
        Cleanup(immediate: true);
    }

    private void Cleanup(bool immediate=false)
    {        
        UnregisterUnit(this);
        if (Commander != null)
        {
            Commander.RemoveUnit(this);
        }

        if (!immediate)
        {
            OnDeath?.Invoke();
            OnAnyUnitDeath?.Invoke(this, new UnitArgs { unit = this });
        }

        gameObject.SetActive(false);
    }

    public void OnEnemyDeath()
    {
        SpawnSliderReward();
    }

    private void SpawnSliderReward()
    {
        MilitarySTile stile = (MilitarySTile)SGrid.Current.GetStileAt(GridPosition);

        // Dropped when enemies die
        if (stile == null)
        {
            Debug.LogError($"Couldn't find a stile at {GridPosition}!");
        }
        
        if (!stile.isTileActive)
        {
            Debug.LogError($"Tried spawning a slider on an inactive STile! (loc: {GridPosition}, island id: {stile.islandId}). Spawning on player's instead");
            stile = (MilitarySTile)SGrid.GetSTileUnderneath(Player.GetInstance().gameObject);
        }

        MilitaryCollectibleController.SpawnMilitaryCollectible(transform, stile);
    }

    private void StartCombatWithOverlappingEnemyUnit()
    {
        // Debug.Log($"Checking for combat start...");
        activeUnits.ForEach(unit =>
        {
            if (unit.GridPosition == GridPosition && unit.UnitTeam != UnitTeam)
            {
                MilitaryTurnAnimator.AddToQueueFront(new MGFight(this, unit));
                MilitaryCombat.ResolveBattle(this, unit);
            }
        });
    }

    public enum Type
    {
        Rock,
        Paper,
        Scissors
    }

    public enum Team
    {
        Player,
        Alien
    }

    public enum Status
    {
        Inactive,
        Active,
        Dead, // Killed in backend and in queue to die visually
    }

    public class UnitArgs : System.EventArgs
    {
        public MilitaryUnit unit;
    }

    public static Color ColorForUnitTeam(Team team)
    {
        return team switch
        {
            Team.Player => new Color(230f / 255f, 230f / 255f, 57f / 255f),
            Team.Alien => new Color(112f / 255f, 48f / 255f, 160f / 255f),
            _ => Color.white,
        };
    }

    public void CreateAndQueueMove(Vector2Int endCoords, STile endStile)
    {
        MGMove move = CreateMove(endCoords, endStile);
        MilitaryTurnAnimator.AddToQueue(move);
        NPCController.hasMoveQueuedOrIsExecuting = true;
    }

    public MGMove CreateMove(Vector2Int endCoords, STile endStile)
    {
        return new MGMove(this, GridPosition, endCoords, AttachedSTile, endStile);
    }
}
