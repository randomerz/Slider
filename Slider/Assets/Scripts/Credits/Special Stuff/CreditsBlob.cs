using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreditsBlob : Blob
{
    protected override void InitSTile(JungleBlobPathController owner)
    {
        transform.SetParent(owner.gameObject.transform);
    }

    protected override void CheckSTile(Vector3 deltaPosition)
    {
        
    }
}
