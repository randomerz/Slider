using System.Collections.Generic;
using UnityEngine;

public class SetDestToAvoidPlayerNode : BehaviourTreeNode
{
    //The minimum range the rat can be from an obstacle before it no longer runs that way
    private RatAI ai;

    private Dictionary<Vector2Int, int> _costs;

    private const int nearWallsPenalty = 10;

    public Dictionary<Vector2Int, int> Costs
    {
        get
        {
            if (_costs == null)
            {
                _costs = GenerateCostMap();
            }

            return _costs;
        }
    }

    public SetDestToAvoidPlayerNode(RatAI ai)
    {
        this.ai = ai;
    }

    public override NodeState Evaluate()
    {
        return NodeState.SUCCESS;
    }

    private Dictionary<Vector2Int, int> GenerateCostMap()
    {
        var nav = ai.GetComponentInParent<WorldNavigation>();
        HashSet<Vector2Int> validPts = nav.ValidPts;

        Vector2Int playerPosAsInt = TileUtil.WorldToTileCoords(ai.player.position);

        var costs = new Dictionary<Vector2Int, int>();
        foreach (var pt in validPts)
        {
            //Dark tiles and the tile with the player must be avoided at all costs.
            if (pt.Equals(playerPosAsInt) || !LightManager.instance.GetLightMaskAt(pt.x, pt.y))
            {
                costs.Add(pt, int.MaxValue);
            }
        }

        return costs;
    }
}