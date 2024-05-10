using System.Collections.Generic;
using UnityEngine;

public class MilitaryNPCsController : MonoBehaviour
{
    public MilitaryUnit militaryUnit;
    public List<NPC> myNPCs;
    public MilitaryUnitFlag myFlag;
    
    private void Start()
    {
        if (myNPCs == null || myNPCs.Count == 0 || myNPCs.Count != gameObject.GetComponentsInChildren<NPC>().Length)
        {
            Debug.LogWarning($"Did you forget to assign NPCs?");
        }

        SetSprites();
    }

    public void SetSprites()
    {
        MilitarySpriteTable militarySpriteTable = (SGrid.Current as MilitaryGrid).militarySpriteTable;

        Sprite mySprite = militarySpriteTable.GetSpriteForUnit(militaryUnit);
        RuntimeAnimatorController myAnimatorController = militarySpriteTable.GetAnimatorControllerForUnit(militaryUnit);

        foreach (NPC npc in myNPCs)
        {
            npc.sr.sprite = mySprite;
            npc.animator.runtimeAnimatorController = myAnimatorController;
        }

        if (myFlag != null)
        {
            myFlag.spriteRenderer.sprite = militarySpriteTable.GetFlagSpriteForUnit(militaryUnit);
        }
    }
}