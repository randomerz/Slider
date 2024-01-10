using UnityEngine;


[CreateAssetMenu(fileName ="Conveyor Data", menuName = "Scriptable Objects/Conveyor Data")]
class ConveyorUIData : ScriptableObject
{
    [System.Serializable]
    public struct ConveyorDataItem
    {
        public Vector2Int pos;
        public Sprite emptyPowered;
        public Sprite emptyUnpowered;
        public Sprite emptyOverride;

        public ConveyorDataItem(Vector2Int pos, Sprite emptyPowered, Sprite emptyUnpowered, Sprite emptyOverride)
        {
            this.pos = pos;
            this.emptyPowered = emptyPowered;
            this.emptyUnpowered = emptyUnpowered;
            this.emptyOverride = emptyOverride;
        }
    }

    public ConveyorDataItem[] conveyors;
}


