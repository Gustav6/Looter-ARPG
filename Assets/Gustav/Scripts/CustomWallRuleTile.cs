using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu]
public class CustomWallRuleTile : RuleTile<CustomWallRuleTile.Neighbor> {
    public bool customField;

    public class Neighbor : RuleTile.TilingRule.Neighbor {
    }

    public override void RefreshTile(Vector3Int position, ITilemap tilemap)
    {
        if (MapManager.Instance.stopTileRefresh)
        {
            return;
        }

        base.RefreshTile(position, tilemap);
    }

    public override bool RuleMatch(int neighbor, TileBase tile) 
    {
        return base.RuleMatch(neighbor, tile);
    }
}