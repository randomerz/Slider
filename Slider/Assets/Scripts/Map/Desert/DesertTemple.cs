using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DesertTemple : MonoBehaviour, ISavable
{
    [SerializeField] private ArtifactHousingButtonsManager artifactHousingButtonsManager;
    [SerializeField] private GameObject templeTrapBlockingRoom;
    [SerializeField] private GameObject templeTrapBlockingRoomCollider;
    private Coroutine shuffleBuildUpCoroutine;

    public void Save() {}

    public void Load(SaveProfile profile)
    {
        if (profile.GetBool("desertIsInTemple"))
        {
            SetIsInTemple(true);
        }
    }

    private void OnEnable() {
        if (SaveSystem.Current.GetBool("desertTempleActivatedTrap") &&
            !SaveSystem.Current.GetBool("desertTempleTrapCleared"))
        {
            ArtifactTabManager.AfterScrollRearrage += OnScrollRearrage;
        }
    }

    private void OnDisable() {
        if (SaveSystem.Current.GetBool("desertTempleActivatedTrap"))
        {
            ArtifactTabManager.AfterScrollRearrage -= OnScrollRearrage;
        }
    }
    
    public void SetIsInTemple(bool isInTemple)
    {
        SaveSystem.Current.SetBool("desertIsInTemple", isInTemple);
        if (isInTemple)
        {
            SaveSystem.Current.SetBool("desertEnteredTemple", true);
        }
        artifactHousingButtonsManager.SetSpritesToHousing(isInTemple);
        Player.GetInstance().SetTracker(!isInTemple);
        Player.GetInstance().SetDontUpdateSTileUnderneath(isInTemple);
    }

    public void ActivateTrap()
    {
        AchievementManager.SetAchievementStat("collectedScroll", 1);
        SaveSystem.Current.SetBool("desertTempleTrapActivated", true);
        if (shuffleBuildUpCoroutine == null)
        {
            shuffleBuildUpCoroutine = StartCoroutine(ActivateTrapBuildUp());
        }
    }

    private IEnumerator ActivateTrapBuildUp()
    {
        templeTrapBlockingRoomCollider.SetActive(true);

        CameraShake.Shake(0.25f, 0.25f);
        AudioManager.Play("Slide Rumble");

        yield return new WaitForSeconds(1f);

        CameraShake.Shake(0.25f, 0.25f);
        AudioManager.Play("Slide Rumble");

        yield return new WaitForSeconds(1f);

        CameraShake.Shake(0.75f, 0.5f);
        AudioManager.Play("Slide Rumble");

        yield return new WaitForSeconds(1f);

        CameraShake.Shake(1.5f, 2.5f);
        AudioManager.PlayWithVolume("Slide Explosion", 0.2f);
        AudioManager.Play("TFT Bell");

        yield return new WaitForSeconds(0.25f);

        UIEffects.FlashWhite();
        SGrid.Current.SetGrid(SGrid.GridStringToSetGridFormat("815493672"));
        templeTrapBlockingRoom.SetActive(true);
        SaveSystem.Current.SetBool("desertTempleActivatedTrap", true);

        ArtifactTabManager.AfterScrollRearrage += OnScrollRearrage;

        yield return new WaitForSeconds(0.75f);

        CameraShake.Shake(2, 0.9f);
        shuffleBuildUpCoroutine = null;
    }

    private void OnScrollRearrage(object sender, System.EventArgs e)
    {
        templeTrapBlockingRoom.SetActive(false);
        templeTrapBlockingRoomCollider.SetActive(false);
        SaveSystem.Current.SetBool("desertTempleTrapCleared", true);
        ArtifactTabManager.AfterScrollRearrage -= OnScrollRearrage;
        AchievementManager.SetAchievementStat("completedDesert", 1);
    }
}
