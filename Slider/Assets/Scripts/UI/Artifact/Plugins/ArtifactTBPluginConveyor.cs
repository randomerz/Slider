using UnityEngine;


public class ArtifactTBPluginConveyor : ArtifactTBPlugin
{
    [SerializeField] private ConveyorUIData conveyorData;
    [SerializeField] private Conveyor[] conveyors;
    [SerializeField] private ConveyorOverrideHandler conveyorOverride;


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
        UpdateEmptySprite();
        button.SetSpriteToIslandOrEmpty();
        foreach (Conveyor conveyor in conveyors)
        {
            conveyor.OnPowered.AddListener(OnConveyorPowered);
        }

        conveyorOverride.OnOverrideStart += OnConveyorOverrideChanged;
        conveyorOverride.OnOverrideEnd += OnConveyorOverrideChanged;
    }

    private void OnDisable()
    {
        foreach (Conveyor conveyor in conveyors)
        {
            conveyor.OnPowered.RemoveListener(OnConveyorPowered);
        }

        conveyorOverride.OnOverrideStart -= OnConveyorOverrideChanged;
        conveyorOverride.OnOverrideEnd -= OnConveyorOverrideChanged;
    }

    //I was gonna do this, but the artifact tiles were flickering :(
    //private void Update()
    //{
    //    UpdateEmptySprite();
    //    button.SetSpriteToIslandOrEmpty();
    //}

    #region ArtifactTBPlugin Implementation
    public override void OnPosChanged()
    {
        UpdateEmptySprite();
    }
    #endregion

    //Remember that this won't run if the artifact is disabled!
    private void OnConveyorPowered(ElectricalNode.OnPoweredArgs e)
    {
        UpdateEmptySprite();
        button.SetSpriteToIslandOrEmpty();
    }

    private void OnConveyorOverrideChanged()
    {
        UpdateEmptySprite();
        button.SetSpriteToIslandOrEmpty();
    }

    public void UpdateEmptySprite()
    {   
        bool isInPast = FactoryGrid.PlayerInPast;
        if (!isInPast && !TrySetEmptySpriteToConveyor())
        {
            button.RestoreDefaultEmptySprite();
        }
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
                Sprite sprite = null;
                if (conveyor.OverrideActive())
                {
                    bool conveyorPowered = conveyor.Powered;
                    sprite = conveyorPowered ? conveyorData.emptyOverridePowered : conveyorData.emptyOverrideUnpowered;
                } 
                else
                {
                    bool conveyorPowered = conveyor.ConveyorIsPoweredAndActive();
                    sprite = conveyorPowered ? conveyorData.emptyPowered : conveyorData.emptyUnpowered;
                }

                return sprite;
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
