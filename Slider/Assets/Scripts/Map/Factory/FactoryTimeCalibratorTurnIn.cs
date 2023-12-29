using UnityEngine;

public class FactoryTimeCalibratorTurnIn : MonoBehaviour
{
    public const string CALIBRATOR_SAVE_STRING = "factoryTimeCalibratorTurnedIn";

    [SerializeField] private GameObject timeCalibratorGO;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform timeCalibratorFinalPosition;

    private void Start()
    {
        if (SaveSystem.Current.GetBool(CALIBRATOR_SAVE_STRING))
        {
            MoveCalibratorToCorrectPosition();
        }
    }

    public void TurnInCalibrator()
    {
        if (SaveSystem.Current.GetBool(CALIBRATOR_SAVE_STRING))
        {
            return;
        }
        SaveSystem.Current.SetBool(CALIBRATOR_SAVE_STRING, true);

        PlayerInventory.RemoveItem();

        MoveCalibratorToCorrectPosition();

        ParticleManager.SpawnParticle(ParticleType.SmokePoof, timeCalibratorFinalPosition.position, timeCalibratorFinalPosition);
        AudioManager.Play("UI Click", timeCalibratorFinalPosition);
    }

    private void MoveCalibratorToCorrectPosition()
    {
        timeCalibratorGO.transform.position = timeCalibratorFinalPosition.position;
        timeCalibratorGO.transform.parent = timeCalibratorFinalPosition;

        animator.Play("On");
    }
}