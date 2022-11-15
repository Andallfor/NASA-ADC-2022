using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using B83.MeshTools;
using System.Linq;
using Newtonsoft.Json;

public static class terrain {
    private static List<GameObject> activeMeshes = new List<GameObject>();
    public static crater currentCrater;
    public static Dictionary<string, terrainFilesInfo> craterData = new Dictionary<string, terrainFilesInfo>();
    
    public static void processRegion(string region, int r, int n) {
        regionalMeshGenerator reg = new regionalMeshGenerator(region, r, n, 1737.4);
        var regData = reg.generate();

        Dictionary<string, Dictionary<string, long[]>> pos = new Dictionary<string, Dictionary<string, long[]>>();

        // cleanup output
        string output = general.regionalFileHostLocation.Split(',').Last().Trim();
        if (!doesFolderExist(new terrainFilesInfo(region, new List<Vector2Int>() {new Vector2Int(r, n)}))) { // shhhhhh
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

        foreach (var kvp in regData.meshes) {
            string name = getTerrainFileName(kvp.Key.x, kvp.Key.y, region);
            byte[] data = MeshSerializer.SerializeMesh(kvp.Value, name, ref pos);

            File.WriteAllBytes(Path.Combine(pathToOutput, name + ".trn"), data);
        }
        File.WriteAllText(Path.Combine(pathToOutput, "data.json"), JsonConvert.SerializeObject(pos));
        File.WriteAllText(Path.Combine(output, region, "bounds.json"), JsonConvert.SerializeObject(regData.bounds));
        //File.WriteAllBytes(Path.Combine(output, region, region.Trim() + "_map.png"), regData.map.EncodeToPNG());
    }

    public static async void generate(string region, int resolution, int x, int y) {
        string name = $"{region}={x}={y}.trn";
        string output = general.regionalFileHostLocation.Split(',').Last().Trim();
        string path = Path.Combine(output, region, resolution.ToString());

        Dictionary<string, Dictionary<string, long[]>> sp = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, long[]>>>(
            File.ReadAllText(Path.Combine(path, "data.json")));

        deserializedMeshData dmd = await MeshSerializer.quickDeserialize(Path.Combine(path, name), sp);

        UnityMainThreadDispatcher.Instance().Enqueue(() => {
            GameObject go = GameObject.Instantiate(general.craterTerrainPrefab);
            go.name = "terrain";
            go.transform.parent = general.bodyParent;
            go.transform.localPosition = Vector3.zero;
            go.transform.localEulerAngles = Vector3.zero;

            Mesh m = dmd.generate();

            // TODO generate uvs not here
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
            for (int i = 0,z =0; i < Mathf.RoundToInt(Mathf.Sqrt(verts.Length)); i++)
            {
                for(int n=0;n< Mathf.RoundToInt(Mathf.Sqrt(verts.Length)); n++)
                {
                    
                    uvs[z] = new Vector2((float)(i / Mathf.Sqrt(verts.Length)), (float)(n / Mathf.Sqrt(verts.Length)));
                    z++;
                }
                

            }
            
            m.uv = uvs;
        
            go.GetComponent<MeshFilter>().mesh = m;

            Material mat = go.GetComponent<MeshRenderer>().sharedMaterial;

            mat.SetInt("_map", 2);
            mat.SetTexture("_mainTex", craterData[region].map);

            // the meshes were saved with a master.scale of 1000, however the current scale may not match
            // adjust the scale of the meshes so that it matches master.scale
            float diff = 1000f / (float) master.scale;
            go.transform.localScale *= diff;

            terrain.registerMesh(go);
        });
    }

    public static void generate(terrainFilesInfo data, int level) {
        int r = data.folderData[level].x;
        int n = data.folderData[level].y;

        for (int i = 0; i < n; i++) {
            for (int j = 0; j < n; j++) {
                generate(data.name, r, i, j);
            }   
        }
    }

    public static void clearMeshes() {
        foreach (GameObject go in activeMeshes) GameObject.Destroy(go);
        activeMeshes = new List<GameObject>();
    }

    public static void registerMesh(GameObject go) {
        activeMeshes.Add(go);
    }

    public static void registerCrater(string name, terrainFilesInfo data) {
        craterData[name] = data;
    }

    public static node[] getNodeData(Vector2Int start, Vector2Int end, string name) {
        Color[] cs = craterData[name].map.GetPixels(start.x, start.y, end.x - start.x, end.y - start.y);

        return colorToNode(cs, craterData[name]);
    }

    public static node getNodeData(Vector2Int v, string name) {
        Color c = craterData[name].map.GetPixel(v.x, v.y);

        return colorToNode(new Color[1] {c}, craterData[name])[0];
    }

    private static node[] colorToNode(Color[] cs, terrainFilesInfo info) {
        node[] output = new node[cs.Length];
        for (int i = 0; i < cs.Length; i++) {
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

    public static void update(object sender, EventArgs e) {
        if (activeMeshes.Count == 0) return;
        if (activeMeshes[0].transform.localScale.x != 1000f / (float) master.scale) {
            foreach (GameObject go in activeMeshes) {
                go.transform.localScale = new Vector3(
                    1000f / (float) master.scale,
                    1000f / (float) master.scale,
                    1000f / (float) master.scale);
            }
        }
    }

    public static bool doesFolderExist(terrainFilesInfo data) {
        string output = general.regionalFileHostLocation.Split(',').Last().Trim();
        foreach (Vector2Int v in data.folderData) {
            if (!Directory.Exists(Path.Combine(output, data.name, v.x.ToString()))) return false;
        }
        return true;
    }

    public static string getTerrainFileName(int x, int y, string name) => $"{name}={x}={y}";

    public static void onStateChange(object s, stateChangeEvent e) {
        if (e.newState == programStates.planetaryTerrain) { // setup
            master.scale = 4;
            terrain.generate(currentCrater.terrainData, 0);
            master.onUpdateEnd += update;

            master.playerPosition = currentCrater.parent.rotateLocalGeo(currentCrater.geo, 10).swapAxis();
            //general.bodyParent.transform.localEulerAngles = currentCrater.geo.rotateToUp();

            general.camera.transform.localPosition = new Vector3(0, 0, -5);
            general.camera.transform.localEulerAngles = new Vector3(0, 0, 0);
            general.camera.transform.RotateAround(Vector3.zero, general.camera.transform.right, -150);

            foreach (crater c in master.registeredCraters) {
                c.parent.representation.SetActive(false);
                c.label.gameObject.SetActive(false);
                c.button.enabled = false;
            }
        } else if (e.previousState == programStates.planetaryTerrain) { // cleanup
            master.scale = 1000;
            master.onUpdateEnd -= update;

            master.playerPosition = new position(0, 0, 0);

            general.camera.transform.localPosition = new Vector3(0, 0, -5);
            general.camera.transform.localEulerAngles = Vector3.zero;
            general.bodyParent.transform.localEulerAngles = Vector3.zero;

            foreach (crater c in master.registeredCraters) { 
                c.parent.representation.SetActive(true);
                c.label.gameObject.SetActive(true);
                c.button.enabled = true;
            }

            currentCrater = null;

            clearMeshes();
        }
    }
}
