using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using B83.MeshTools;
using System.Linq;
using Newtonsoft.Json;

public static class craterTerrainController
{
    public static int mode=0;
    public static List<GameObject> activeMeshes = new List<GameObject>();
    public static crater currentCrater;
    public static Dictionary<string, terrainFilesInfo> craterData = new Dictionary<string, terrainFilesInfo>();
    public static Vector2 worldSize;
    public static float nodeRadius, nodeDiameter;
    public static int gridSizeX, gridSizeY;
    public static Vector3 bottomRight;
    public static Node[,] grid;
    public static List<Node> path=new List<Node>();
    private static Mesh m;
    public static Texture2D pathTexture;


    public static void processRegion(string region, int r, int n)
    {

        regionalMeshGenerator reg = new regionalMeshGenerator(region, r, n, 1737.4);
        var regData = reg.generate();

        Dictionary<string, Dictionary<string, long[]>> pos = new Dictionary<string, Dictionary<string, long[]>>();

        // cleanup output
        string output = general.regionalFileHostLocation.Split(',').Last().Trim();
        if (!doesFolderExist(new terrainFilesInfo(region, new List<Vector2Int>() { new Vector2Int(r, n) })))
        { // shhhhhh
            Debug.Log("passed");
            Directory.CreateDirectory(output);
            Directory.CreateDirectory(Path.Combine(output, region));
            Directory.CreateDirectory(Path.Combine(output, region, r.ToString()));
        }

        DirectoryInfo di = new DirectoryInfo(Path.Combine(output, region, r.ToString()));
        di.Delete(true);
        // dont question it
        Directory.CreateDirectory(Path.Combine(output, region, r.ToString()));

        string pathToOutput = Path.Combine(output, region, r.ToString());

        foreach (var kvp in regData.meshes)
        {
            string name = getTerrainFileName(kvp.Key.x, kvp.Key.y, region);
            byte[] data = MeshSerializer.SerializeMesh(kvp.Value, name, ref pos);

            File.WriteAllBytes(Path.Combine(pathToOutput, name + ".trn"), data);
        }
        File.WriteAllText(Path.Combine(pathToOutput, "data.json"), JsonConvert.SerializeObject(pos));
        File.WriteAllText(Path.Combine(output, region, "bounds.json"), JsonConvert.SerializeObject(regData.bounds));
        File.WriteAllBytes(Path.Combine(output, region, region.Trim() + "_map.png"), regData.map.EncodeToPNG());
    }

    public static async void generate(string region, int resolution, int x, int y)
    {

        string name = $"{region}={x}={y}.trn";
        string output = general.regionalFileHostLocation.Split(',').Last().Trim();
        string path = Path.Combine(output, region, resolution.ToString());

        Dictionary<string, Dictionary<string, long[]>> sp = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, long[]>>>(
            File.ReadAllText(Path.Combine(path, "data.json")));

        deserializedMeshData dmd = await MeshSerializer.quickDeserialize(Path.Combine(path, name), sp);

        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            GameObject go = GameObject.Instantiate(general.craterTerrainPrefab);
            go.name = "terrain";
            go.transform.parent = general.bodyParent;
            go.transform.localPosition = Vector3.zero;

            m = dmd.generate();

            m.RecalculateBounds();

            // TODO generate uvs not here
            // TODO: bug -> if mesh num != 1, uvs do not map correctly
            Vector3[] verts = m.vertices;
            Vector2[] uvs = new Vector2[verts.Length];
            float minX = 0;
            float minY = 0;
            for (int i = 0; i < verts.Length; i++)
            {
                if (verts[i].x > minX)
                {
                    minX = verts[i].x;
                }
                if (verts[i].z > minY)
                {
                    minY = verts[i].z;
                }
            }
            for (int i = 0; i < verts.Length; i++)
            {
                verts[i].x += minX;
                verts[i].z += minY;
            }
            for (int i = 0, z = 0; i < Mathf.RoundToInt(Mathf.Sqrt(verts.Length)); i++)
            {
                for (int n = 0; n < Mathf.RoundToInt(Mathf.Sqrt(verts.Length)); n++)
                {

                    uvs[z] = new Vector2((float)(i / Mathf.Sqrt(verts.Length)), (float)(n / Mathf.Sqrt(verts.Length)));
                    z++;
                }


            }

            m.uv = uvs;

            go.GetComponent<MeshFilter>().mesh = m;

            Material mat = go.GetComponent<MeshRenderer>().sharedMaterial;

            mat.SetInt("_map", mode);
           
            mat.SetTexture("_mainTex", craterData[region].map);

            // the meshes were saved with a master.scale of 1000, however the current scale may not match
            // adjust the scale of the meshes so that it matches master.scale
            float diff = 1000f / (float)master.scale;
            go.transform.localScale *= diff;
            craterTerrainController.registerMesh(go);
            worldSize = new Vector2(m.bounds.size.x * go.transform.localScale.x, m.bounds.size.z * go.transform.localScale.z);
            nodeRadius = .01f;
            nodeDiameter = nodeRadius * 2;
            bottomRight = Vector3.zero - Vector3.right * worldSize.x / 2 - Vector3.forward * worldSize.y / 2;
            gridSizeX = Mathf.RoundToInt(worldSize.x / nodeDiameter);
            gridSizeY = Mathf.RoundToInt(worldSize.y / nodeDiameter);
            grid = new Node[gridSizeX, gridSizeY];
            generateGrid(region);

        });
    }
    public static void generateGrid(string region)
    {
        
        float meshSize = craterData[region].map.width;
        float increment = 4000 / gridSizeX;
        bool walkable = true;
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                
                walkable = true;
                Vector3 worldPoint = bottomRight + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (nodeDiameter * y + nodeRadius);
                float startx = Mathf.Floor(increment * x);
                float starty = Mathf.Floor(increment * y);
                /*
                while (startx - increment * (x + 1) < 1 / increment)
                {
                    while (starty - increment * (y + 1) < 1 / increment)
                    {

                        if (getNodeData(new Vector2Int((int)startx, (int)starty), region).slope > 15) walkable = false;
                        starty++;

                    }
                    startx += 1;
                }
                */
                if (getNodeData(new Vector2Int((int)startx, (int)starty), region).slope > 15) walkable = false;
                grid[x, y] = new Node(walkable, worldPoint, x, y);
            }
        }
    }
    public static List<Node> getNeighbors(Node n)
    {
        List<Node> neighbors = new List<Node>();
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {

                if (y == 0 && x == 0)
                    continue;
                int x1 = n.gridX + x;
                int y1 = n.gridY + y;
                if (x1 >= 0 && x1 < gridSizeX && y1 >= 0 && y1 < gridSizeY)
                {
                    neighbors.Add(grid[x1, y1]);
                }
            }
        }
        return neighbors;
    }
    public static Node worldPosToNode(Vector3 pos)
    {
        float percentX = (pos.x + worldSize.x / 2) / worldSize.x;
        float percentY = (pos.z + worldSize.y / 2) / worldSize.y;
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);
        int x = Mathf.RoundToInt(percentX * (gridSizeX - 1));
        int y = Mathf.RoundToInt(percentY * (gridSizeY - 1));
        return (grid[x, y]);

    }

    public static void generate(terrainFilesInfo data, int level)
    {
        int r = data.folderData[level].x;
        int n = data.folderData[level].y;

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                generate(data.name, r, i, j);
            }
        }
    }

    public static void clearMeshes()
    {
        foreach (GameObject go in activeMeshes) GameObject.Destroy(go);
        activeMeshes = new List<GameObject>();
    }

    public static void registerMesh(GameObject go)
    {
        activeMeshes.Add(go);
    }

    public static void registerCrater(string name, terrainFilesInfo data)
    {
        craterData[name] = data;
    }

    public static node[] getNodeData(Vector2Int start, Vector2Int end, string name)
    {
        Color[] cs = craterData[name].map.GetPixels(start.x, start.y, end.x - start.x, end.y - start.y);

        return colorToNode(cs, craterData[name]);
    }

    public static node getNodeData(Vector2Int v, string name)
    {

        Color c = craterData[name].map.GetPixel(v.x, v.y);


        return colorToNode(new Color[1] { c }, craterData[name])[0];
    }

    private static node[] colorToNode(Color[] cs, terrainFilesInfo info)
    {
        node[] output = new node[cs.Length];
        for (int i = 0; i < cs.Length; i++)
        {
            Color c = cs[i];
            node n = new node();
            n.height = c.r * (info.bounds["height"][1] - info.bounds["height"][0]) + info.bounds["height"][0];
            n.slope = c.g * (info.bounds["slope"][1] - info.bounds["slope"][0]) + info.bounds["slope"][0];
            n.elevation = c.b * (info.bounds["elevation"][1] - info.bounds["elevation"][0]) + info.bounds["elevation"][0];
            n.azimuth = c.a * (info.bounds["azimuth"][1] - info.bounds["azimuth"][0]) + info.bounds["azimuth"][0];

            output[i] = n;
        }

        return output;
    }
    public static void colorUpdate()
    {
        if (activeMeshes.Count == 0) return;
        
        foreach (GameObject go in activeMeshes)
        {
            Material mat = go.GetComponent<MeshRenderer>().sharedMaterial;
            if (mode != 4)
            {
                
                mat.shader = Shader.Find("Custom/mapShader");
                mat.SetInt("_map", mode);

            }
            if (mode == 4&&pathTexture!=null)
            {
                
                mat.shader = Shader.Find("Unlit/Texture");
                mat.SetTexture("_MainTex", pathTexture);
            }
        }
        
    }
    public static void update(object sender, EventArgs e)
    {
        if (activeMeshes.Count == 0) return;
        if (activeMeshes[0].transform.localScale.x != 1000f / (float)master.scale)
        {
            foreach (GameObject go in activeMeshes)
            {
                Material mat = go.GetComponent<MeshRenderer>().sharedMaterial;
                //mat.SetInt("_map", mode);
                
                go.transform.localScale = new Vector3(
                    1000f / (float)master.scale,
                    1000f / (float)master.scale,
                    1000f / (float)master.scale);
            }
        }
    }

    public static bool doesFolderExist(terrainFilesInfo data)
    {
        string output = general.regionalFileHostLocation.Split(',').Last().Trim();
        foreach (Vector2Int v in data.folderData)
        {
            if (!Directory.Exists(Path.Combine(output, data.name, v.x.ToString()))) return false;
        }
        return true;
    }

    public static string getTerrainFileName(int x, int y, string name) => $"{name}={x}={y}";

    public static void onStateChange(object s, stateChangeEvent e)
    {
        if (e.newState == programStates.planetaryTerrain)
        { // setup
            master.scale = 4;
            craterTerrainController.generate(currentCrater.terrainData, 0);
            master.onScaleChange += update;

            master.playerPosition = currentCrater.parent.rotateLocalGeo(currentCrater.geo, 10).swapAxis();
            //general.bodyParent.transform.localEulerAngles = currentCrater.geo.rotateToUp();

            general.camera.transform.localPosition = new Vector3(0, 0, -5);
            general.camera.transform.localEulerAngles = new Vector3(0, 0, 0);
            general.camera.transform.RotateAround(Vector3.zero, general.camera.transform.right, -150);

            foreach (crater c in master.registeredCraters)
            {
                c.parent.representation.SetActive(false);
                c.label.gameObject.SetActive(false);
                c.button.enabled = false;
            }
        }
        else if (e.previousState == programStates.planetaryTerrain)
        { // cleanup
            master.scale = 1000;
            master.onScaleChange -= update;

            master.playerPosition = new position(0, 0, 0);

            general.camera.transform.localPosition = new Vector3(0, 0, -5);
            general.camera.transform.localEulerAngles = Vector3.zero;
            general.bodyParent.transform.localEulerAngles = Vector3.zero;

            foreach (crater c in master.registeredCraters)
            {
                c.parent.representation.SetActive(true);
                c.label.gameObject.SetActive(true);
                c.button.enabled = true;
            }

            currentCrater = null;

            clearMeshes();
        }
    }
}
