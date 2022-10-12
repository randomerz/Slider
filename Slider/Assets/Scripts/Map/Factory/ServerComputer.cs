using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ServerComputer : MonoBehaviour
{
    [SerializeField] private ElectricalNode power;
    [SerializeField] private Animator anim;
    [SerializeField] private PlayerConditionals pConds;
    [SerializeField] private PlayerPositionChanger ppChanger;
    [SerializeField] private GameObject[] pastTileMaps;

    public UnityEvent OnInteract;

    private void Awake()
    {
        pConds?.DisableConditionals();
    }

    private void OnEnable()
    {
        anim.SetBool("Powered", power.Powered);
    }

    #region Called By Events
    public void TurnOn()
    {
        // Debug.Log("Powered Computer On");
        anim.SetBool("Powered", true);
        pConds?.EnableConditionals();
    }

    public void TurnOff()
    {
        // Debug.Log("Powered Computer Off");
        anim.SetBool("Powered", false);
        pConds?.DisableConditionals();
    }

    public void OnPlayerInteract()
    {
        OnInteract?.Invoke();
    }
    #endregion

    #region Send Player To Past
    public void StartSendToPastEvent()
    {
        StartCoroutine(SendToPastEvent());
    }

    private IEnumerator SendToPastEvent()
    {
        CameraShake.Shake(0.5f, 0.25f);
        AudioManager.PlayWithVolume("Slide Explosion", 0.25f);

        yield return new WaitForSeconds(2);

        for (int i = 0; i < 2; i++)
        {
            CameraShake.Shake(0.25f, 0.25f);
            AudioManager.PlayWithVolume("Slide Rumble", 0.25f);

            yield return new WaitForSeconds(1f);
        }
        
        for (int i = 0; i < 4; i++)
        {
            CameraShake.Shake(0.25f, 0.25f);
            AudioManager.PlayWithVolume("Slide Rumble", 0.25f);

            yield return new WaitForSeconds(0.5f);
        }
        
        for (int i = 0; i < 7; i++)
        {
            CameraShake.Shake(0.25f, 0.25f);
            AudioManager.PlayWithVolume("Slide Rumble", 0.25f);

            if (i == 1 || i == 4 || i == 6)
                FactoryLightManager.SwitchLights(true);
            if (i == 2 || i == 5)
                FactoryLightManager.SwitchLights(false);

            yield return new WaitForSeconds(0.25f);
        }

        yield return new WaitForSeconds(0.1f);

        // cut everything off
        CameraShake.StopShake();
        AudioManager.StopAllSoundAndMusic();
        
        UIEffects.FadeFromWhite();
        SpawnPlayerInPast();
    }

    private void SpawnPlayerInPast()
    {
        //Disable anchors so they don't interefere with the past section.
        Anchor[] anchors = FindObjectsOfType<Anchor>(); 
        foreach (var anchor in anchors)
        {
            anchor.UnanchorTile();
            anchor.gameObject.SetActive(false);
        }

        foreach (GameObject go in pastTileMaps)
        {
            go.SetActive(true);
        }

        ppChanger.UPPTransform();

        FactoryLightManager.SwitchLights(true);
    }
    #endregion
}
