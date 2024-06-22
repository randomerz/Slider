using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ArtifactUnitIconHandler : MonoBehaviour
{
    [SerializeField] private Image unitIcon;
    [SerializeField] private MilitaryArtifactButton militaryArtifactButton;

    [Header("Banners")]
    [SerializeField] private Sprite noUnit;
    [SerializeField] private Sprite rock;
    [SerializeField] private Sprite paper;
    [SerializeField] private Sprite scissors;

    private readonly Dictionary<MilitaryUnit.Type, Sprite> unitTypeToIcon = new();

    private void Awake()
    {
        unitTypeToIcon[MilitaryUnit.Type.Rock] = rock;
        unitTypeToIcon[MilitaryUnit.Type.Paper] = paper;
        unitTypeToIcon[MilitaryUnit.Type.Scissors] = scissors;
    }

    private void OnEnable()
    {
        // TODO: Make the unit icons lay on top of the tiles and be independent of them so they don't behave weirdly when moving.
        // For now I'm just making them invisible during the move as a lazy fix
        SGridAnimator.OnSTileMoveStart += (_, _) => { HideUnitIcon(); };
        SGridAnimator.OnSTileMoveEnd += (_, _) => { UpdateUnitIcon(); };
        UpdateUnitIcon();
    }

    private void OnDisable()
    {
        SGridAnimator.OnSTileMoveStart -= (_, _) => { HideUnitIcon(); };
        SGridAnimator.OnSTileMoveEnd -= (_, _) => { UpdateUnitIcon(); };
    }

    private void UpdateUnitIcon()
    {
        MilitaryUnit unitOnThisGridPosition = MilitaryUnit.ActiveUnits.Where(unit => unit.GridPosition == GridPosition())
                                                                      .FirstOrDefault();

        if (unitOnThisGridPosition != null)
        {
            unitIcon.sprite = unitTypeToIcon[unitOnThisGridPosition.UnitType];
            unitIcon.color = MilitaryUnit.ColorForUnitTeam(unitOnThisGridPosition.UnitTeam);
        }
        else
        {
            HideUnitIcon();
        }
    }

    private void HideUnitIcon()
    {
        unitIcon.sprite = noUnit;
    }

    private Vector2Int GridPosition()
    {
        return new Vector2Int(militaryArtifactButton.x, militaryArtifactButton.y);
    }
}
