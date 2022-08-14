using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplodingBarrel : MonoBehaviour
{
    [SerializeField] private ConditionChecker doorChecker;
    [SerializeField] private ElectricalNode eNode;

    private void Awake()
    {
        eNode = GetComponent<ElectricalNode>();
    }
    public void Explode()
    {
        StartCoroutine(ExplodeRoutine());
    }

    private IEnumerator ExplodeRoutine()
    {
        //Do Ze Animation
        yield return new WaitForSeconds(2.0f);
        SaveSystem.Current.SetBool("doorExploded", true);
        doorChecker.CheckConditions();
        eNode.RemoveAllNeighbors();
        Destroy(gameObject);
    }
}