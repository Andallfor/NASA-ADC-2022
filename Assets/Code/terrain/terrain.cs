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
    public static void processRegion(string region, int n, int r) {
        regionalMeshGenerator haworth = new regionalMeshGenerator("haworth", r, n, 1737.4);
        Dictionary<Vector2Int, Mesh> meshes = haworth.generate();

        Dictionary<string, Dictionary<string, long[]>> pos = new Dictionary<string, Dictionary<string, long[]>>();

        // cleanup output
        string output = general.regionalFileHostLocation.Split(',').Last().Trim();
        if (!Directory.Exists(Path.Combine(output, region, r.ToString()))) {
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
            string name = $"{region}={kvp.Key.x}={kvp.Key.y}";
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
            GameObject go = GameObject.Instantiate(general.facilityPrefab);
            go.name = "terrain";
            go.transform.parent = general.bodyParent;
            go.transform.localPosition = Vector3.zero;
            go.transform.localEulerAngles = Vector3.zero;
            go.transform.localScale = new Vector3(75, 75, 75);
            go.GetComponent<MeshRenderer>().material = general.defaultMat;
            Mesh m = dmd.generate();
            go.GetComponent<MeshFilter>().mesh = m;

            terrain.registerMesh(go);
        });
    }

    public static void clearMeshes() {
        foreach (GameObject go in activeMeshes) GameObject.Destroy(go);
        activeMeshes = new List<GameObject>();
    }

    public static void registerMesh(GameObject go) {
        activeMeshes.Add(go);
    }

    public static void update() {
        foreach (GameObject go in activeMeshes) {
            go.transform.localScale = new Vector3(
                75 * (100f / (float) master.scale),
                75 * (100f / (float) master.scale),
                75 * (100f / (float) master.scale));
        }
    }
}
