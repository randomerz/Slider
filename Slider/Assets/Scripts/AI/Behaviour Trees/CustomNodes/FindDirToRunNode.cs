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
            Vector2 currDir1 = Vector2.one;
            Vector2 currDir2 = Vector2.one;
            bool foundDir1 = false;
            bool foundDir2 = false;
            for (float theta = 0; theta < Mathf.PI - 0.0001f && !(foundDir1 && foundDir2); theta += dTheta)
            {
                //L: Check on both sides of playerToRat for openings.
                currDir1 = Vector2Rotate.Rotate(playerToRat, theta);
                if (CheckDirGood(currDir1))
                {
                    ai.SetDirection(currDir1);
                    foundDir1 = true;
                }
                currDir2 = Vector2Rotate.Rotate(playerToRat, theta);
                if (CheckDirGood(currDir2))
                {
                    ai.SetDirection(currDir2);
                    foundDir2 = true;
                }
            }

            //L: This logic is disgusting, but whatever.
            if (foundDir1 && foundDir2)
            {
                Vector2 dir = Vector2.Dot(currDir1, playerToRat) > Vector2.Dot(currDir2, playerToRat) ? currDir1 : currDir2;
                ai.SetDirection(dir);
                return NodeState.SUCCESS;
            } else if (foundDir1)
            {
                ai.SetDirection(currDir1);
                return NodeState.SUCCESS;
            } else if (foundDir2)
            {
                ai.SetDirection(currDir2);
                return NodeState.SUCCESS;
            } else
            {
                //L: Rat is cornered, face player and don't run.
                ai.SetDirection(-playerToRat);
                return NodeState.FAILURE;
            }
        }
    }

    private bool CheckDirGood(Vector2 dir)
    {
        RaycastHit2D hit = Physics2D.Raycast(ai.transform.position, dir, ai.obstacleMinRange, ~LayerMask.GetMask("Ignore Raycast", "Slider", "SlideableArea"));
        bool lit = true;
        if (LightManager.instance != null)
        {
            Vector2 pos = (Vector2)ai.transform.position + dir;
            lit = LightManager.instance.GetLightMaskAt((int)pos.x, (int)pos.y);
        }
        //Debug.Log("Hit? " + (hit.collider != null));
        if (hit.collider != null)
        {
            Debug.Log(hit.collider.gameObject);
        }
        //Debug.Log("Found Dir: " + dir);
        return hit.collider == null && lit;
    }
}
