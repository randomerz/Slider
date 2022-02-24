using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FindDirToRunNode : BehaviourTreeNode
{
    //The minimum range the rat can be from an obstacle before it no longer runs that way
    RatAI ai;
    Transform player;

    public FindDirToRunNode(RatAI ai, Transform player)
    {
        this.ai = ai;
        this.player = player;
    }

    public override NodeState Evaluate()
    {
        //L: raycast away from player
        Vector2 playerToRat = (ai.transform.position - player.transform.position).normalized;

        if (CheckDirGood(playerToRat))
        {
            ai.SetDirection(playerToRat);
            return NodeState.SUCCESS;
        } else
        {     
            const float dTheta = Mathf.PI / 8;
            Vector2 currDir1 = Vector2.zero;
            Vector2 currDir2 = Vector2.zero;
            for (float theta = 0; theta < Mathf.PI - 0.0001f; theta += dTheta)
            {
                //L: Check on both sides of playerToRat for openings.
                currDir1 = Vector2Rotate.Rotate(playerToRat, theta);
                if (CheckDirGood(currDir1))
                {
                    ai.SetDirection(currDir1);
                    return NodeState.SUCCESS;
                }
                currDir2 = Vector2Rotate.Rotate(playerToRat, -theta);
                if (CheckDirGood(currDir2))
                {
                    ai.SetDirection(currDir2);
                    return NodeState.SUCCESS;
                }
            }

            //L: This logic is disgusting, but whatever.
            /*
            if (foundDir1 && foundDir2)
            {
                Vector2 dir = Vector2.Dot(currDir1, playerToRat) > Vector2.Dot(currDir2, playerToRat) ? currDir1 : currDir2;
                ai.SetDirection(dir);
                return NodeState.SUCCESS;
            } else if (foundDir1 || foundDir2)
            {
                ai.SetDirection(foundDir1 ? currDir1 : currDir2);
                return NodeState.SUCCESS;
            } else
            {
                //L: Rat is cornered, face player and don't run.
                ai.SetDirection(-playerToRat);
                return NodeState.FAILURE;
            }
            */
            return NodeState.FAILURE;
        }
    }

    private bool CheckDirGood(Vector2 dir)
    {
        RaycastHit2D[] hits = new RaycastHit2D[1];  //We only care about the first hit.
        int numResults = Physics2D.Raycast(ai.transform.position, dir, GetRaycastFilter(), hits, ai.moveSpeed * ai.raycastDistFactor);
        bool lit = true;
        if (LightManager.instance != null)
        {
            Vector2 pos = (Vector2)ai.transform.position + dir;
            lit = LightManager.instance.GetLightMaskAt((int)pos.x, (int)pos.y);
        }

        if (numResults > 0)
        {
            Debug.Log(hits[0].collider.gameObject);
        }

        return numResults == 0 && lit;
    }

    private ContactFilter2D GetRaycastFilter()
    {
        ContactFilter2D filter = new ContactFilter2D();
        filter = filter.NoFilter();
        filter.useTriggers = false;
        filter.useLayerMask = true;
        filter.layerMask = ~LayerMask.GetMask("Ignore Raycast", "SlideableArea", "Rat");

        return filter;
    }
}
