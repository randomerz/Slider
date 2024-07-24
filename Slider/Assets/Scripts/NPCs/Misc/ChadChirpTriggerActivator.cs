using System.Collections.Generic;
using UnityEngine;

public class ChadChirpTriggerActivator : MonoBehaviour
{
    public string id;

    public bool useOnTriggerExitInstead;

    public List<Condition> conditions;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (useOnTriggerExitInstead)
        {
            return;
        }

        CheckTrigger(other);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (useOnTriggerExitInstead)
        {
            CheckTrigger(other);
        }
    }

    public void CheckTrigger(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (!CheckConditions())
            {
                return;
            }

            ChadChirp.OnTryChirp?.Invoke(this, new ChadChirp.ChadChirpArgs { id = this.id });
        }
    }
    
    public bool CheckConditions()
    {
        int numtrue = 0;
        foreach (Condition cond in conditions)
        {
            numtrue += cond.CheckCondition() ? 1 : 0;
        }
    
        bool pass = numtrue == conditions.Count;
        return pass;
    }
}