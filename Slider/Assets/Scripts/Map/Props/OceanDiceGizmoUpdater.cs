using UnityEngine;

public class OceanDiceGizmoUpdater : MonoBehaviour
{
    public DiceGizmo diceGizmo;
    
    private void Update()
    {
        bool loudDice = (
            SaveSystem.Current.GetBool("OceanPlayerRolled") &&
            !PlayerInventory.Contains("Slider 3", Area.Ocean)
        );
        diceGizmo.alsoSoundOutOfHouse = loudDice;
        diceGizmo.onlySoundInHouse = !loudDice;
    }
}