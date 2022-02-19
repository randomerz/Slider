using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FindDirToRunNode : Node
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
        RaycastHit2D hit = Physics2D.Raycast(ai.transform.position, ai.transform.position - player.transform.position, ai.obstacleMinRange);
    }


}
