using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerComputer : MonoBehaviour
{
    [SerializeField] private ElectricalNode power;
    [SerializeField] private PlayerConditionals pConds;
    [SerializeField] private Animator anim;
    [SerializeField] private PlayerPositionChanger ppChanger;
    [SerializeField] private GameObject[] pastTileMaps;

    private void Awake()
    {
        pConds.DisableConditionals();
    }

    private void OnEnable()
    {
        anim.SetBool("Powered", power.Powered);
    }

    #region Called By Events
    public void TurnOn()
    {
        Debug.Log("Powered Computer On");
        anim.SetBool("Powered", true);
        pConds.EnableConditionals();
    }

    public void TurnOff()
    {
        Debug.Log("Powered Computer Off");
        anim.SetBool("Powered", false);
        pConds.DisableConditionals();
    }

    public void OnPlayerInteract()
    {
        StartSendToPastEvent();
    }
    #endregion

    #region Send Player To Past
    public void StartSendToPastEvent()
    {
        UIEffects.FadeToWhite( () =>
        {
            StartCoroutine(SendToPastThenFadeIn());
        }, 3.0f, false);
    }

    private IEnumerator SendToPastThenFadeIn()
    {
        SpawnPlayerInPast();
        yield return new WaitForSeconds(2.0f);
        UIEffects.FadeFromWhite();
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
