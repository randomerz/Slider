using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DistanceBasedAmbience : MonoBehaviour
{
    public string ambienceName;
    public string ambienceGlobalParameterName;
    [Tooltip("This is what value is set to OnDestroy() and SetParameterEnabled(false)")]
    public float ambienceGlobalParameterDefaultValue;
    public List<Transform> distanceNodes = new List<Transform>();
    
    private Transform playerTransform;
    [SerializeField] private bool isEnabled = true;
    public bool IsEnabled => isEnabled;

    private static Dictionary<string, float> nameToMinDist = new();

    private void Start()
    {
        playerTransform = Player.GetInstance().transform;
        AudioManager.PlayAmbience(ambienceName);
        nameToMinDist[ambienceName] = Mathf.Infinity;
        if (isEnabled)
        {
            UpdateParameter();
            SendParameter();
        }
    }

    private void OnDestroy()
    {
        AudioManager.StopAmbience(ambienceName);
        AudioManager.SetGlobalParameter(ambienceGlobalParameterName, ambienceGlobalParameterDefaultValue);
    }

    private void Update()
    {
        if (isEnabled)
        {
            UpdateParameter();
        }
    }

    private void LateUpdate()
    {
        if (isEnabled)
        {
            SendParameter();
        }
    }

    public void SetParameterEnabled(bool isEnabled)
    {
        this.isEnabled = isEnabled;

        if (isEnabled)
        {
            UpdateParameter();
            SendParameter();
        }
        else
        {
            AudioManager.SetGlobalParameter(ambienceGlobalParameterName, ambienceGlobalParameterDefaultValue);
            nameToMinDist[ambienceName] = Mathf.Infinity;
        }
    }

    private void UpdateParameter()
    {
        float minDistance = Mathf.Infinity;
        foreach (Transform t in distanceNodes)
        {
            if (playerTransform != null && t.gameObject.activeInHierarchy)
            {
                minDistance = Mathf.Min(minDistance, Vector3.Distance(playerTransform.position, t.position));
            }
        }

        nameToMinDist[ambienceName] = Mathf.Min(minDistance, nameToMinDist[ambienceName]);
    }

    private void SendParameter()
    {
        if (nameToMinDist[ambienceName] != Mathf.Infinity)
        {
            AudioManager.SetGlobalParameter(ambienceGlobalParameterName, nameToMinDist[ambienceName]);
            nameToMinDist[ambienceName] = Mathf.Infinity;
        }
    }
}
