using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public Vector3 worldPos;
    public bool walkable;
    public int hcost, gcost, gridX, gridY;
    public Node parent;

    public Node(bool walkable_, Vector3 worldPos_, int gridX_, int gridY_)
    {
        this.walkable = walkable_;
        this.worldPos = worldPos_;
        this.gridY = gridY_;
        this.gridX = gridX_;
    }
    public int fcost
    {
        get
        {
            return gcost + hcost;
        }
    }
}
