using UnityEngine;

public class JungleSpawner : JungleBox
{
    [SerializeField] private JungleSignAnimator shapeSignAnimator;
    [SerializeField] private Shape[] INSIGNIA_SHAPES;
    [SerializeField] private SpriteRenderer INSIGNIA_SPRITE_RENDERER;
    [SerializeField] private Sprite[] INSIGNIA_SPRITES;

    private const string SHAPE_SAVE_SUFFIX = "_CurrShape";

    public int CurrentShapeIndex { get; protected set; }
    private int mySourceId = 0;

    protected override void Awake()
    {
        base.Awake();

        SetInsignia(CurrentShapeIndex);

        AssignSourceId();
    }

    public override void Save()
    {
        base.Save();

        if (!IsSaveStringOkay())
        {
            return;
        }

        SaveSystem.Current.SetInt(saveString + SHAPE_SAVE_SUFFIX, CurrentShapeIndex);
    }

    public override void Load(SaveProfile profile)
    {
        base.Load(profile);

        if (!IsSaveStringOkay())
        {
            return;
        }

        CurrentShapeIndex = profile.GetInt(saveString + SHAPE_SAVE_SUFFIX);
    }

    private void AssignSourceId()
    {
        mySourceId = numSourceIds;
        usedSourceIds.Add(mySourceId);
        numSourceIds += 1;
    }

    public override bool IsValidInput(JungleBox other, Direction fromDirection)
    {
        return false;
    }

    public override bool AddInput(JungleBox other, Direction fromDirection)
    {
        return IsValidInput(other, fromDirection);
    }

    public override void RemoveInput(Direction fromDirection)
    {
        if (direction == fromDirection && targetBox != null)
        {
            // The other box should already have this as an input, but is not using it
            targetBox.AddInput(this, DirectionUtil.Inv(fromDirection));
            targetBox.UpdateBox();
        }
    }

    public override bool UpdateBox(int depth = 0)
    {
        if (depth >= DEPTH_LIMIT)
        {
            Debug.LogError("Jungle Box depth limit exceeded!");
            return false;
        }

        if (targetBox is JungleSpawner)
        {
            // Checking targetBox.IsValidInput() didn't work for me for some reason
            return false;
        }

        // Update next box
        if (ProducedShape != null && targetBox != null)
        {
            targetBox.UpdateBox(depth + 1);
        }

        return true;
    }

    public void IncrementInsignia()
    {
        SetInsignia((CurrentShapeIndex + 1) % 3);
    }

    private void SetInsignia(int index)
    {
        CurrentShapeIndex = index;

        ProducedShape = INSIGNIA_SHAPES[index];

        UpdateBox();
        UpdateSprites();
        pathController.UpdateMarchingShape(direction, ProducedShape);
    }

    protected override void UpdateSprites()
    {
        base.UpdateSprites();
        
        shapeSignAnimator.SetShapeIndex(CurrentShapeIndex);
        INSIGNIA_SPRITE_RENDERER.sprite = INSIGNIA_SPRITES[CurrentShapeIndex];
    }

    public void IsShapeTriangle(Condition c) => c.SetSpec(ProducedShape.shapeName == "Triangle");
    public void IsShapeSemiCircle(Condition c) => c.SetSpec(ProducedShape.shapeName == "Semicircle");
    public void IsShapeLine(Condition c) => c.SetSpec(ProducedShape.shapeName == "Line");

}