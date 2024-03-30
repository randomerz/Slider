using UnityEngine;

public class WizardDuelSound : MonoBehaviour 
{
    private const string SAVE_STRING = "MagiTechWizardDuelSoundPlayed";
    private MagiTechGrid grid;

    private void Start() 
    {
        grid = SGrid.Current as MagiTechGrid;
    }

    private void OnTriggerEnter2D(Collider2D other) 
    {
        if (!SaveSystem.Current.GetBool(SAVE_STRING))
        {
            if (grid.FireHasStool() && grid.LightningHasStool())
            {
                PlaySound();
            }
        }
    }

    private void PlaySound()
    {
        SaveSystem.Current.SetBool(SAVE_STRING, true);

        AudioManager.Play("Puzzle Complete");
    }
}