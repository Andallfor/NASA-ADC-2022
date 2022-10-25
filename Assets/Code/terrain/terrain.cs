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
    public static void processRegion(string region, int r, int n,int res,Gradient gradient) {
        regionalMeshGenerator reg = new regionalMeshGenerator(region, r, n, 1737.4,res,gradient);
        Dictionary<Vector2Int, Mesh> meshes = reg.generate();

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

        foreach (var kvp in meshes) {
            string name = getTerrainFileName(kvp.Key.x, kvp.Key.y, region);
            byte[] data = MeshSerializer.SerializeMesh(kvp.Value, name, ref pos);

            File.WriteAllBytes(Path.Combine(output, region, r.ToString(), name + ".trn"), data);   
        }
        File.WriteAllText(Path.Combine(output, region, r.ToString(), "data.json"), JsonConvert.SerializeObject(pos));
    }

    public static async void generate(string region, int resolution, int x, int y) {
        string name = $"{region}={x}={y}.trn";
        string path = Path.Combine(general.regionalFileHostLocation.Split(',').Last().Trim(), region, resolution.ToString());

        Dictionary<string, Dictionary<string, long[]>> sp = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, long[]>>>(
            File.ReadAllText(Path.Combine(path, "data.json")));

        deserializedMeshData dmd = await MeshSerializer.quickDeserialize(Path.Combine(path, name), sp);

        UnityMainThreadDispatcher.Instance().Enqueue(() => {
            GameObject go = GameObject.Instantiate(general.craterPrefab);
            go.name = "terrain";
            go.transform.parent = general.bodyParent;
            go.transform.localPosition = Vector3.zero;
            go.transform.localEulerAngles = Vector3.zero;
            go.GetComponent<MeshRenderer>().material = general.defaultMat;
            Mesh m = dmd.generate();
            go.GetComponent<MeshFilter>().mesh = m;

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

    public static bool doesDataExist(terrainFilesInfo data) {
        if (!doesFolderExist(data)) return false;

        string output = general.regionalFileHostLocation.Split(',').Last().Trim();
        foreach (Vector2Int v in data.folderData) {
            string path = Path.Combine(output, data.name, v.x.ToString());
            for (int i = 0; i < v.y; i++) {
                for (int j = 0; j < v.y; j++) {
                    string name = getTerrainFileName(i, j, data.name);
                    if (!File.Exists(Path.Combine(path, name))) return false;
                }
            }
        }

        return true;
    }

    public static string getTerrainFileName(int x, int y, string name) => $"{name}={x}={y}";

    public static void onStateChange(object s, stateChangeEvent e) {
        if (e.newState == programStates.planetaryTerrain) { // setup
            master.scale = 1;
            terrain.generate(currentCrater.terrainData, 0);
            master.onUpdateEnd += update;

            master.playerPosition = currentCrater.parent.rotateLocalGeo(currentCrater.geo, 10).swapAxis();
            general.bodyParent.transform.localEulerAngles = currentCrater.geo.rotateToUp();

            general.camera.transform.localPosition = new Vector3(0, 0, -5);
            general.camera.transform.localEulerAngles = Vector3.zero;
            general.camera.transform.RotateAround(Vector3.zero, general.camera.transform.right, 45);
            
            currentCrater.parent.representation.SetActive(false);
            currentCrater.label.gameObject.SetActive(false);
            currentCrater.button.enabled = false;
        } else if (e.previousState == programStates.planetaryTerrain) { // cleanup
            master.scale = 1000;
            master.onUpdateEnd -= update;

            master.playerPosition = new position(0, 0, 0);

            general.camera.transform.localPosition = new Vector3(0, 0, -5);
            general.camera.transform.localEulerAngles = Vector3.zero;
            general.bodyParent.transform.localEulerAngles = Vector3.zero;

            currentCrater.parent.representation.SetActive(true);
            currentCrater.label.gameObject.SetActive(true);
            currentCrater.button.enabled = true;

            currentCrater = null;

            clearMeshes();
        }
    }
}
