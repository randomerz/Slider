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

    private void Start()
    {
        playerTransform = Player.GetInstance().transform;
        AudioManager.PlayAmbience(ambienceName);
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

        AudioManager.SetGlobalParameter(ambienceGlobalParameterName, minDistance);
    }
}
