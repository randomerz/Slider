using UnityEngine;

public class ChadChirpTriggerActivator : MonoBehaviour
{
    public string id;

    public bool useOnTriggerExitInstead;

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
            ChadChirp.OnTryChirp?.Invoke(this, new ChadChirp.ChadChirpArgs { id = this.id });
        }
    }
}