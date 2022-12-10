using System.Collections;
using System.Collections.Generic;
using System.IO;
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
    public Texture2D generateTexture()
    {
        Texture2D tex = new Texture2D(craterTerrainController.gridSizeX, craterTerrainController.gridSizeY);
        Color[] colors = new Color[craterTerrainController.gridSizeX* craterTerrainController.gridSizeY];
        foreach (Node i in craterTerrainController.grid)
        {
            /*
            Debug.Log(craterTerrainController.path.Contains(i));
            if (craterTerrainController.path.Contains(i))
            {
                colors[i.gridX* craterTerrainController.gridSizeY+i.gridY] = Color.blue;
            }
            else if (i.walkable == true)
            {
                colors[i.gridX * craterTerrainController.gridSizeY + i.gridY] = Color.green;
            }
            else
            {
                colors[i.gridX * craterTerrainController.gridSizeY + i.gridY] = Color.red;
            }
*/
            if (craterTerrainController.path.Contains(i))
            {
                tex.SetPixel(i.gridY, i.gridX, Color.blue);
            }
            else if (i.walkable == true)
            {
                tex.SetPixel(i.gridY, i.gridX, Color.green);
            }
            else
            {
                tex.SetPixel(i.gridY,i.gridX,Color.red);
            }
            
        }
        //tex.SetPixels(colors);
        tex.Apply();
        craterTerrainController.pathTexture = tex;
        return (tex);
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
        craterTerrainController.pathTexture = null;

    }

    // Update is called once per frame
    void Update()
    {
        seeker.active = (master.currentState == programStates.planetaryTerrain && craterTerrainController.mode == 4);
        hider.active = (master.currentState == programStates.planetaryTerrain && craterTerrainController.mode == 4);
        if (Input.GetKeyDown(KeyCode.Return)&&master.currentState==programStates.planetaryTerrain)
        {
            generateTexture();
            find(craterTerrainController.worldPosToNode(new Vector3(seeker.transform.position.z,0,seeker.transform.position.x*-1)), craterTerrainController.worldPosToNode(new Vector3(hider.transform.position.z , 0, hider.transform.position.x*-1)));
            byte[] bytes = generateTexture().EncodeToPNG();
            File.WriteAllBytes("C:/Users/ltriv/Downloads/texturetest.png", bytes);

        }

    }
}
