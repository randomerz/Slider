using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DistanceBasedAmbience : MonoBehaviour
{
    public string ambienceName;
    public string ambienceGlobalParameterName;
    public float ambienceGlobalParameterDefaultValue;
    public List<Transform> distanceNodes = new List<Transform>();
    private Transform playerTransform;

    private static Dictionary<string, float> nameToMinDist = new();

    private void Start()
    {
        playerTransform = Player.GetInstance().transform;
        AudioManager.PlayAmbience(ambienceName);
        nameToMinDist[ambienceName] = Mathf.Infinity;
        UpdateParameter();
    }

    private void OnDestroy()
    {
        AudioManager.StopAmbience(ambienceName);
        AudioManager.SetGlobalParameter(ambienceGlobalParameterName, ambienceGlobalParameterDefaultValue);
    }

    private void Update()
    {
        UpdateParameter();
    }

    private void LateUpdate()
    {
        SendParameter();
    }

    private void UpdateParameter()
    {
        float minDistance = Mathf.Infinity;
        foreach (Transform t in distanceNodes)
        {
            if (t.gameObject.activeInHierarchy)
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
