using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MilitarySpriteTable", menuName = "Scriptable Objects/Military Sprite Table")]
public class MilitarySpriteTable : ScriptableObject
{
    [System.Serializable]
    public struct SpriteData {
        public MilitaryUnit.Team team;
        public MilitaryUnit.Type type;
        public Sprite sprite;
        public RuntimeAnimatorController animatorController;
    }

    // len 6 - Player half first, then alien half
    public List<SpriteData> spriteData;
    public List<Sprite> flags;

    public Sprite GetSpriteForUnit(MilitaryUnit unit)
    {
        return spriteData[GetIndexForUnit(unit)].sprite;
    }

    public RuntimeAnimatorController GetAnimatorControllerForUnit(MilitaryUnit unit)
    {
        return spriteData[GetIndexForUnit(unit)].animatorController;
    }

    public Sprite GetFlagSpriteForUnit(MilitaryUnit unit)
    {
        return flags[(int)unit.UnitType];
    }

    private int GetIndexForUnit(MilitaryUnit unit)
    {
        int index = 0;
        if (unit.UnitTeam == MilitaryUnit.Team.Alien)
        {
            index += 3;
        }

        index += (int)unit.UnitType;

        return index;
    }
}