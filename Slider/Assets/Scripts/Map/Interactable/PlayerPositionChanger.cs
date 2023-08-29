using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPositionChanger : MonoBehaviour
{
    public Vector3 dPos;
    public bool tpToHouse;
    public Transform goTo;

    public enum GizmoType 
    {
        None,
        Hard,
        RelativePlayer,
        RelativeMe,
        Transform
    }
    public GizmoType gizmoType;

    public void UPPHard() 
    {
        Player.SetPosition(dPos);
        Player.SetIsInHouse(tpToHouse);
    }

    public void UPPRelativePlayer()
    {
        Player.SetPosition(Player.GetPosition() + dPos);
        Player.SetIsInHouse(tpToHouse);
    }

    public void UPPRelativeMe()
    {
        Player.SetPosition(transform.position + dPos);
        Player.SetIsInHouse(tpToHouse);
    }

    public void UPPTransform()
    {
        Player.SetPosition(goTo.position + dPos);
        Player.SetIsInHouse(tpToHouse);
    }

    private void OnDrawGizmosSelected() 
    {
        Vector3 startPos = Vector3.zero;
        Vector3 endPos = Vector3.zero;
        switch (gizmoType) 
        {
            case GizmoType.None:
                return;
            case GizmoType.Hard:
                endPos = dPos;
                break;
            case GizmoType.RelativePlayer:
                startPos = GameObject.Find("Player").transform.position;
                endPos = GameObject.Find("Player").transform.position + dPos;
                break;
            case GizmoType.RelativeMe:
                startPos = transform.position;
                endPos = transform.position + dPos;
                break;
            case GizmoType.Transform:
                if (goTo != null)
                {
                    endPos = goTo.position + dPos;
                }
                break;
        }
        // Gizmos.DrawSphere(pos, 0.4f);
        Vector3 cubeSize = new Vector3(0.75f, 0.75f, 0.75f);
        Gizmos.color = Color.blue;  //L: Swap Beam
        Gizmos.DrawWireCube(endPos, cubeSize);

    }
}
