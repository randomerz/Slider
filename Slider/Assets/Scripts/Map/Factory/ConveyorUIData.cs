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

        public ConveyorDataItem(Vector2Int pos, Sprite emptyPowered, Sprite emptyUnpowered)
        {
            this.pos = pos;
            this.emptyPowered = emptyPowered;
            this.emptyUnpowered = emptyUnpowered;
        }
    }

    public ConveyorDataItem[] conveyors;
}


