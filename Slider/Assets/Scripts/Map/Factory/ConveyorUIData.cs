using UnityEngine;
using UnityEngine.Serialization;


[CreateAssetMenu(fileName ="Conveyor Data", menuName = "Scriptable Objects/Conveyor Data")]
class ConveyorUIData : ScriptableObject
{
    [System.Serializable]
    public struct ConveyorDataItem
    {
        public Vector2Int pos;
        public Sprite emptyPowered;
        public Sprite emptyUnpowered;
        public Sprite emptyOverridePowered;
        public Sprite emptyOverrideUnpowered;

        public ConveyorDataItem(Vector2Int pos, Sprite emptyPowered, Sprite emptyUnpowered, Sprite emptyOverridePowered, Sprite emptyOverrideUnpowered)
        {
            this.pos = pos;
            this.emptyPowered = emptyPowered;
            this.emptyUnpowered = emptyUnpowered;
            this.emptyOverridePowered = emptyOverridePowered;
            this.emptyOverrideUnpowered = emptyOverrideUnpowered;
        }
    }

    public ConveyorDataItem[] conveyors;
}


