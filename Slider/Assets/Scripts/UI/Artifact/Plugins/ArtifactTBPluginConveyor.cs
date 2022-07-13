using UnityEngine;


public class ArtifactTBPluginConveyor : ArtifactTBPlugin
{
    [SerializeField]
    private ConveyorUIData conveyorData;
    [SerializeField]
    private Conveyor[] conveyors;

    private void Awake()
    {
        if (conveyors == null || conveyors.Length == 0)
        {
            conveyors = FindObjectsOfType<Conveyor>();
        }

        if (conveyorData == null)
        {
            Debug.LogError("Need to set conveyor data in ArtifactTBPluginConveyor.");
        }
    }

    private void OnEnable()
    {
        foreach (Conveyor conveyor in conveyors)
        {
            conveyor.OnPowered.AddListener(OnConveyorPowered);
        }
    }

    private void OnDisable()
    {
        foreach (Conveyor conveyor in conveyors)
        {
            conveyor.OnPowered.RemoveListener(OnConveyorPowered);
        }
    }

    private void Start()
    {
        UpdateEmptySprite();
    }

    #region ArtifactTBPlugin Implementation
    public override void OnPosChanged()
    {
        UpdateEmptySprite();
    }
    #endregion

    private void OnConveyorPowered(ElectricalNode.OnPoweredArgs e)
    {
        UpdateEmptySprite();
    }

    private void UpdateEmptySprite()
    {
        if (!TrySetEmptySpriteToConveyor())
        {
            button.UseDefaultEmptySprite();
        }

        button.SetSpriteToIslandOrEmpty();
    }

    private bool TrySetEmptySpriteToConveyor()
    {
        Conveyor conveyor = ConveyorAt(button.x, button.y);
        if (conveyor != null)
        {
            Sprite conveyorSprite = GetConveyorButtonSprite(conveyor);
            if (conveyorSprite != null)
            {
                button.SetEmptySprite(conveyorSprite);
                return true;
            }
        }

        return false;
    }

    private Sprite GetConveyorButtonSprite(Conveyor conveyor)
    {
        foreach (var conveyorData in conveyorData.conveyors)
        {
            if (conveyorData.pos.Equals(conveyor.StartPos))
            {
                return conveyor.Powered ? conveyorData.emptyPowered : conveyorData.emptyUnpowered;
            }
        }

        Debug.LogError($"Conveyor data does not exist for conveyor at {conveyor.StartPos}");
        return null;
    }

    private Conveyor ConveyorAt(int x, int y)
    {
        foreach (Conveyor conveyor in conveyors)
        {
            if (conveyor.StartPos.Equals(new Vector2Int(x, y)))
            {
                return conveyor;
            }
        }

        return null;
    }
}
