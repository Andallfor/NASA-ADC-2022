using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System;
using System.Linq;

public class Pathfind : MonoBehaviour
{
    public GameObject seeker, hider;
    public void find(Node start, Node end)
    {
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
        Color[] colors = new Color[craterTerrainController.gridSizeX * craterTerrainController.gridSizeY];
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
                tex.SetPixel(i.gridY, i.gridX, Color.red);
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
        seeker.SetActive(master.currentState == programStates.planetaryTerrain && craterTerrainController.mode == 4);
        hider.SetActive(master.currentState == programStates.planetaryTerrain && craterTerrainController.mode == 4);
        if (Input.GetKeyDown(KeyCode.Return) && master.currentState == programStates.planetaryTerrain)
        {
            generateTexture();
            find(craterTerrainController.worldPosToNode(new Vector3(seeker.transform.position.z, 0, seeker.transform.position.x * -1)), craterTerrainController.worldPosToNode(new Vector3(hider.transform.position.z, 0, hider.transform.position.x * -1)));
            byte[] bytes = generateTexture().EncodeToPNG();
            if (general.host == "ltriv") File.WriteAllBytes("C:/Users/ltriv/Downloads/texturetest.png", bytes);

            generateComLinks(8);
        }
    }

    private void generateComLinks(int maxDist) {
        foreach (GameObject go in previousLinks) Destroy(go);

        // get list of all possible com link positions
        Queue<ComLinkNode> nodes = new Queue<ComLinkNode>();
        Dictionary<Vector2Int, ComLinkNode> visited = new Dictionary<Vector2Int, ComLinkNode>();
        foreach (Node n in craterTerrainController.path) {
            ComLinkNode cn = new ComLinkNode(new Vector2Int(n.gridX, n.gridY), 0, n);
            visited.Add(cn.pos, cn);
            nodes.Enqueue(cn);
        }

        // get all valid comlink pos
        int dist = 0;
        while (true) {
            dist++;
            Queue<ComLinkNode> frontier = new Queue<ComLinkNode>();
            while (nodes.Count != 0) {
                ComLinkNode n = nodes.Dequeue();
                visited[n.pos] = n;

                List<Node> neighbors = craterTerrainController.getNeighbors(n.n);
                foreach (Node nn in neighbors) {
                    if (!nn.walkable) continue;
                    ComLinkNode cn = new ComLinkNode(new Vector2Int(nn.gridX, nn.gridY), dist + 1, nn);
                    bool visible = UnityEngine.Random.Range(0, 10) == 5;
                    if (!visited.ContainsKey(cn.pos) && visible) frontier.Enqueue(cn); // TODO: add visibility check
                }
            }

            if (frontier.Count == 0) break;
            if (dist > maxDist) break;
            nodes = frontier;
        }

        Vector2Int[] bestPos = new Vector2Int[10];
        for (int i = 0; i < 10; i++) {
            Node n = craterTerrainController.path[i * (craterTerrainController.path.Count / 10)];

            float bestScore = 10000000;
            ComLinkNode? bestNode = null;
            foreach (ComLinkNode cn in visited.Values) {
                float score = cn.dist * 3 + Vector2.Distance(cn.pos, new Vector2(n.gridX, n.gridY)); // + what ever other modifier you want
                float distToOtherNodes = 0;

                foreach (Vector2Int p in bestPos) distToOtherNodes = Math.Max(Vector2.Distance(cn.pos, p), distToOtherNodes);

                //score -= distToOtherNodes * distToOtherNodes * distToOtherNodes;

                if (score < bestScore) {
                    bestScore = score;
                    bestNode = cn;
                }
            }

            bestPos[i] = (bestNode.HasValue) ? ((ComLinkNode) bestNode).pos : Vector2Int.zero;
            GameObject go = GameObject.Instantiate(general.defaultPrefab);
            go.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            go.name = bestPos[i].ToString();

            go.transform.position = craterTerrainController.grid[bestPos[i].x, bestPos[i].y].worldPos;

            previousLinks.Add(go);
        }
    }

    private List<GameObject> previousLinks = new List<GameObject>();
}
