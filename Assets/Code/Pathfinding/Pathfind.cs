using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfind : MonoBehaviour
{
    
    public GameObject seeker, hider;
    public void find(Node start, Node end)
    {
        //Debug.Log("yes");
        List<Node> open = new List<Node>();
        List<Node> closed = new List<Node>();
        open.Add(start);
        while (open.Count > 0)
        {
            Node current = open[0];
            for (int i = 1; i < open.Count; i++)
            {
                if (open[i].fcost < current.fcost || open[i].fcost == current.fcost && open[i].hcost < current.hcost)
                {
                    current = open[i];
                }
            }
            open.Remove(current);
            closed.Add(current);
            if (end == current)
            {
                //Debug.Log("yes");
                retrace(start, end);
                return;
            }
            foreach (Node n in craterTerrainController.getNeighbors(current))
            {
                if (n.walkable == false || closed.Contains(n))
                {
                    continue;
                }
                int cost = current.gcost + getDistance(current, n);
                if (cost < n.gcost || !open.Contains(n))
                {
                    n.gcost = cost;
                    n.hcost = getDistance(n, end);
                    n.parent = current;
                    if (!open.Contains(n))
                    {
                        open.Add(n);
                    }
                }
            }
        }
    }
    void retrace(Node start, Node end)
    {
        List<Node> path = new List<Node>();
        Node current = end;
        while (current != start)
        {
            path.Add(current);
            current = current.parent;

        }
        path.Reverse();
        craterTerrainController.path = path;
    }
    public int getDistance(Node n1, Node n2)
    {
        int dstX = Mathf.Abs(n1.gridX - n2.gridX);
        int dstY = Mathf.Abs(n1.gridY - n2.gridY);
        if (dstX > dstY)
        {
            return (14 * dstY + 10 * (dstX - dstY));
        }
        return (14 * dstX + 10 * (dstY - dstX));
    }
    // Start is called before the first frame update
    void Awake()
    {
        

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return)&&master.currentState==programStates.planetaryTerrain)
        {
            Debug.Log("yes");
            find(craterTerrainController.worldPosToNode(seeker.transform.position), craterTerrainController.worldPosToNode(hider.transform.position));

        }

    }
}
