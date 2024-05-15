using UnityEngine;
using FMODUnity;

public class DesertTempleMusic : MonoBehaviour
{
    [SerializeField] private Transform snailTransform;
    [SerializeField] private EventReference reverbSnapshot;

    private StudioEventEmitter reverbEmitter;

    private bool isInTemple;

    private bool didInit;

    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        if (didInit)
            return;
        didInit = true;

        reverbEmitter = gameObject.AddComponent<FMODUnity.StudioEventEmitter>();
        reverbEmitter.EventReference = reverbSnapshot;
    }

    private void Update()
    {
        if (isInTemple)
        {
            float distToSnail = Vector3.Distance(Player.GetPosition(), snailTransform.position);
            AudioManager.SetGlobalParameter("DesertDistToSnail", distToSnail);
        }
    }

    public void SetIsInTemple(bool isInTemple)
    {
        if (reverbEmitter == null)
            Init();

        this.isInTemple = isInTemple;
        if (isInTemple)
        {
            AudioManager.PlayMusic("Desert Snail", false);
            reverbEmitter.Play();
        }
        else
        {
            AudioManager.StopMusic("Desert Snail");
            reverbEmitter.Stop();
        }
    }
}