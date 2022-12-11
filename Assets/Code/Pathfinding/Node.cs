using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public Vector3 worldPos;
    public bool walkable, isVis;
    public int hcost, gcost, gridX, gridY;
    public Node parent;

    public Node(bool walkable_, Vector3 worldPos_, int gridX_, int gridY_, bool isVis)
    {
        this.walkable = walkable_;
        this.worldPos = worldPos_;
        this.gridY = gridY_;
        this.gridX = gridX_;
        this.isVis = isVis;
    }
    public int fcost
    {
        get
        {
            return gcost + hcost;
        }
    }
}

public struct ComLinkNode {
    public int dist;
    public Vector2Int pos;
    public Node n;
    public bool isVis;

    public ComLinkNode(Vector2Int pos, int dist, Node n, bool isVis) {
        this.pos = pos;
        this.dist = dist;
        this.n = n;
        this.isVis = isVis;
    }

    public override int GetHashCode() {
        return pos.GetHashCode();
    }

    public override bool Equals(object obj) {
        if (!(obj is ComLinkNode)) return false;
        var c = (ComLinkNode) obj;
        return c.pos == this.pos;
    }
}