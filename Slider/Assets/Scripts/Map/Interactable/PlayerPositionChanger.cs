using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPositionChanger : MonoBehaviour
{
    public Vector3 dPos;

    public enum GizmoType 
    {
        None,
        Hard,
        RelativePlayer,
        RelativeMe
    }
    public GizmoType gizmoType;

    public void UPPHard() 
    {
        Player.SetPosition(dPos);
    }

    public void UPPRelativePlayer()
    {
        Player.SetPosition(Player.GetPosition() + dPos);
    }

    public void UPPRelativeMe()
    {
        Player.SetPosition(transform.position + dPos);
    }


    private void OnDrawGizmos() 
    {
        Gizmos.color = Color.blue;
        Vector3 pos = Vector3.zero;
        switch (gizmoType) 
        {
            case GizmoType.None:
                return;
            case GizmoType.Hard:
                pos = dPos;
                break;
            case GizmoType.RelativePlayer:
                pos = GameObject.Find("Player").transform.position + dPos;
                break;
            case GizmoType.RelativeMe:
                pos = transform.position + dPos;
                break;
        }
        // Gizmos.DrawSphere(pos, 0.4f);
        Gizmos.DrawWireCube(pos, new Vector3(0.75f, 0.75f, 0.75f));
    }
}
