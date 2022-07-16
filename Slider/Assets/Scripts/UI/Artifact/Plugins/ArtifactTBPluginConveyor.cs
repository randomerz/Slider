﻿using UnityEngine;


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
        UpdateEmptySprite();
        button.SetSpriteToIslandOrEmpty();
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

    private void UpdateEmptySprite()
    {
        if (!TrySetEmptySpriteToConveyor())
        {
            button.UseDefaultEmptySprite();
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
