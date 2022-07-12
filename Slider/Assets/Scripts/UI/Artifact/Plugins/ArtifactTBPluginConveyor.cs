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
        if (!button.isTileActive)
        {
            Conveyor conveyor = ConveyorAt(button.x, button.y);
            if (conveyor != null)
            {
                UpdateSpriteToConveyor(conveyor);
            }
            else
            {
                button.buttonAnimator.sliderImage.sprite = button.emptySprite;
            }
        }
    }

    private void UpdateSpriteToConveyor(Conveyor conveyor)
    {
        foreach (var item in conveyorData.conveyors)
        {
            if (item.pos.Equals(conveyor.StartPos))
            {
                Sprite conveyorSprite = conveyor.Powered ? item.emptyPowered : item.emptyUnpowered;
                button.buttonAnimator.sliderImage.sprite = conveyorSprite;
            }
        }
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
