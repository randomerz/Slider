using System.Collections.Generic;
using UnityEngine;

public class InvokeChadChirp : MonoBehaviour
{
    public void InvokeChirp(string id)
    {
        ChadChirp.OnTryChirp?.Invoke(this, new ChadChirp.ChadChirpArgs { id = id });
    }
}