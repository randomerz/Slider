using UnityEngine;


class FactoryArtifactTileButton : ArtifactTileButton
{
    [SerializeField]
    private ConveyorUIData conveyorData;

    private Conveyor[] conveyors;

    protected new void Awake()
    {
        base.Awake();
        conveyors = FindObjectsOfType<Conveyor>();
    }

    protected new void Start()
    {
        base.Start();
        UpdateEmptySprite();
    }

    private void OnEnable()
    {
        //base.OnEnable();
        foreach (Conveyor conveyor in conveyors)
        {
            conveyor.OnPowered.AddListener(OnConveyorPowered);
        }
    }

    private new void OnDisable()
    {
        base.OnDisable();
        foreach (Conveyor conveyor in conveyors)
        {
            conveyor.OnPowered.RemoveListener(OnConveyorPowered);
        }
    }

    public override void SetPosition(int x, int y)
    {
        base.SetPosition(x, y);
        UpdateEmptySprite();
    }

    private void OnConveyorPowered(ElectricalNode.OnPoweredArgs e)
    {
        UpdateEmptySprite();
    }

    private void UpdateEmptySprite()
    {
        if (!isTileActive)
        {
            Conveyor conveyor = ConveyorAt(x, y);
            if (conveyor != null)
            {
                foreach (var item in conveyorData.conveyors)
                {
                    if (item.pos.Equals(conveyor.StartPos))
                    {
                        buttonAnimator.sliderImage.sprite = conveyor.Powered ? item.emptyPowered : item.emptyUnpowered;
                    }
                }
            }
            else
            {
                buttonAnimator.sliderImage.sprite = emptySprite;
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
