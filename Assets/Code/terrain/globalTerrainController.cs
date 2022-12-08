using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using UnityEditor;

public class globalTerrainController {
    private planet parent;
    private GameObject movementDetector;
    private Vector3 lastDetectorPos = Vector3.zero;
    private HashSet<geographic> currentDesiredMeshes = new HashSet<geographic>();
    private Dictionary<geographic, globalTerrainInstance> aliveMeshes = new Dictionary<geographic, globalTerrainInstance>();
    private Material mat;
    private double lastScale;
    private List<Vector2> directions = new List<Vector2>() {
        new Vector2(-1, 1),  new Vector2(0, 1),  new Vector2(1, 1),
        new Vector2(-1, 0),                      new Vector2(1, 0),
        new Vector2(-1, -1), new Vector2(0, -1), new Vector2(1, -1)};

    public globalTerrainController(planet parent, bool loadNormalMaps = false) {
        if (globalMeshGenerator.folder == "" || !Directory.Exists(globalMeshGenerator.folder)) {
            throw new FileNotFoundException("Could not find globalMeshGenerator.folder! It is currently " + globalMeshGenerator.folder);
        }

        this.parent = parent;
        master.onStateChange += onStateChange;

        movementDetector = GameObject.Instantiate(general.defaultPrefab, parent.representation.gameObject.transform);
        movementDetector.transform.position = Vector3.one;
        movementDetector.GetComponent<MeshRenderer>().enabled = false;
        parent.representation.GetComponent<MeshRenderer>().enabled = false;

        mat = Resources.Load("materials/globalTerrain") as Material;
    }

    public void onStateChange(object s, stateChangeEvent e) {
        if (e.newState == programStates.interplanetary) {
            // init
            master.onUpdateEnd += update;
        } else if (e.previousState == programStates.interplanetary) {
            // cleanup
            master.onUpdateEnd -= update;
        }
    }

    public void update(object s, EventArgs e) {
        if (master.referenceFrameBody.name != parent.name) return;

        // update shader to account for sun pos
        Vector3 v = master.sun.representation.transform.position.normalized;
        mat.SetVector("_lightDir", v);

        Vector3 detectorPos = general.camera.WorldToScreenPoint(movementDetector.transform.position);
        bool movement = Vector3.Distance(detectorPos, lastDetectorPos) < 0.05f;
        bool scale = Math.Abs(master.scale - lastScale) > 10;
        if (!movement && !scale) return;

        lastDetectorPos = detectorPos;
        lastScale = master.scale;

        HashSet<geographic> target = findDesiredMeshes();

        HashSet<geographic> current = new HashSet<geographic>(currentDesiredMeshes); // toKill
        HashSet<geographic> desired = new HashSet<geographic>(target); // toGen
        HashSet<geographic> ignore = new HashSet<geographic>(current.Intersect(desired).ToList());
        current.SymmetricExceptWith(ignore);
        desired.SymmetricExceptWith(ignore);

        currentDesiredMeshes = target;

        // kill
        foreach (geographic g in current) {
            aliveMeshes[g].requestKill();
            aliveMeshes.Remove(g);
        }
        
        // create new meshes
        foreach (geographic g in desired) {
            globalTerrainInstance inst = new globalTerrainInstance(
                parent.name, new Vector2Int((int) g.lon, (int) g.lat), new Vector2Int(0, 0), new Vector2Int(2000, 2000), 3, 3, true, parent.representation);
            inst.generate(true, false, false);
            aliveMeshes[g] = inst;
        }
    }

    private HashSet<geographic> findDesiredMeshes() {
        double scaledRadius = parent.information.radius / master.scale;

        position[] intersections = position.lineSphereIntersection(
            parent.representation.transform.position,
            general.camera.transform.position,
            parent.representation.transform.position,
            scaledRadius);
        
        // get the point closest to the camera, the other point would be obscured by the planet itself
        position intersection = intersections.OrderBy(x => x.distanceTo(general.camera.transform.position)).First();
        // convert the point into a geographic value
        geographic intersectionGeo = parent.localPosToLocalGeo(intersection);
        // get the tile that this geo would be contained in
        // TODO: dynamic resolution
        // currently assumes that size is 60 x 60
        geographic step = new geographic(60, 60);
        geographic start = new geographic(
            intersectionGeo.lat - intersectionGeo.lat % step.lat + 30, // bc it is not centered on 0 but +30 (due to step size)
            intersectionGeo.lon - intersectionGeo.lon % step.lon, true);

        // start flood fill alg
        Queue<geographic> frontier = new Queue<geographic>(getNearbyTiles(start, step));
        HashSet<geographic> visited = new HashSet<geographic>() {start};
        HashSet<geographic> visible = new HashSet<geographic>();

        bool allOffscreen = false;
        while (!allOffscreen) {
            allOffscreen = true;
            HashSet<geographic> nextFrontier = new HashSet<geographic>();
            while (frontier.Count != 0) {
                geographic ll = frontier.Dequeue();
                
                allOffscreen = false;

                foreach (Vector2 dir in directions) {
                    geographic next = new geographic(ll.lat + dir.x * step.lat, ll.lon + dir.y * step.lon, true);
                    if (!visited.Contains(next) && !nextFrontier.Contains(next)) {
                        visited.Add(next);
                        nextFrontier.Add(next);
                    }
                }
            }

            frontier = new Queue<geographic>(nextFrontier);
        }

        return visited;
    }

    private Vector2 vec3To2(Vector3 v) => new Vector2(v.x, v.y);

    private HashSet<geographic> getNearbyTiles(geographic start, geographic step) {
        HashSet<geographic> output = new HashSet<geographic>();

        foreach (Vector2 dir in directions) {
            // geographic automatically wraps coordinates and + operator returns a new geographic (rather than modifying values)
            output.Add(start + new geographic(dir.x * step.lat, dir.y * step.lon));
        }

        return output;
    }

    public static async Task generateNormalMaps(planet parent) {
        int res = 3;
        int pw = (int) Math.Pow(2, res);
        int length = (int) (1024 / pw);
        //Texture2DArray texArr = new Texture2DArray(length * 60, length * 60, (360 / 60) * (180 / 60), TextureFormat.RGBA32, 0, true);
        for (int sx = -180; sx < 180; sx += 60) {
            for (int sy = -90; sy < 90; sy += 60) {
                Vector2Int start = new Vector2Int(sx, sy);
                Dictionary<Vector2Int, globalTerrainInstance> map = new Dictionary<Vector2Int, globalTerrainInstance>();
                List<Task> tasks = new List<Task>();

                for (int x = start.x; x < start.x + 60; x += 30) {
                    for (int y = start.y; y < start.y + 60; y += 15) {
                        var a = new globalTerrainInstance("Luna", new Vector2Int(x, y), new Vector2Int(0, 0), new Vector2Int(16000, 16000), res, 0, false, parent.representation);
                        var b = new globalTerrainInstance("Luna", new Vector2Int(x, y), new Vector2Int(15360, 0), new Vector2Int(15360 + 16000, 16000), res, 0, false, parent.representation);

                        int aX = (int) (x - start.x) * length;
                        int aY = (int) (start.y + 45 - y) * length;
                        map[new Vector2Int(aX, aY)] = a;
                        map[new Vector2Int(aX + length * 15, aY)] = b;
                        tasks.Add(a.generate(false, true, true));
                        tasks.Add(b.generate(false, true, true));
                    }
                }

                await Task.WhenAll(tasks);

                await Task.Delay(2000);

                Texture2D tex = new Texture2D(length * 60, length * 60);
                tex.hideFlags = HideFlags.HideAndDontSave;
                foreach (var kvp in map) {
                    tex.SetPixels32(kvp.Key.x, kvp.Key.y, length * 15, length * 15, kvp.Value.generateNormalMap(0, 0, 15360 / pw, 15360 / pw, 16000 / pw), 0);
                    kvp.Value.requestKill();
                }
                tex.Apply();

                int index = ((sx + 180) / 60) * (180 / 60) + ((sy + 90) / 60);

                byte[] data = tex.EncodeToPNG();
                File.WriteAllBytes("C:/Users/leozw/Desktop/ADC/global/" + index.ToString() + ".png", data);
                GameObject.Destroy(tex);
            }
        }
    }
}

public class globalTerrainInstance {
    private CancellationTokenSource token;
    public bool currentlyRunning {get; private set;} = false;
    public bool exists {get; private set;} = true;
    private GameObject go, parent;
    private Vector2Int point, start, end;
    private int rlevel, qual;
    private string subFolder;
    private bool isSmall;
    private Mesh m;

    public globalTerrainInstance(string subFolder, Vector2Int point, Vector2Int start, Vector2Int end, int rlevel, int qual, bool isSmall, GameObject parent) {
        token = new CancellationTokenSource();
        this.subFolder = subFolder;
        this.point = point;
        this.start = start;
        this.end = end;
        this.rlevel = rlevel;
        this.qual = qual;
        this.isSmall = isSmall;
        this.parent = parent;
    }

    public Task generate(bool createGo = true, bool generateNormals = false, bool is32Bit = false) {
        if (!exists) return null;
        if (currentlyRunning) {
            Debug.LogWarning("Trying to generate mesh that's currently generating.");
            return null;
        }
        if (go != null) {
            Debug.LogWarning("Somehow GameObject exists despite not being recognized as so. This should not happen!");
            return null;
        }

        currentlyRunning = true;

        Task t = Task.Run(() => {
            try {
            // do not question the spam
            if (token.IsCancellationRequested) return;
            decompTerrainData d = globalMeshGenerator.requestGlobalTerrain(subFolder, point, start, end, rlevel, qual, isSmall);
            if (token.IsCancellationRequested) return;
            decompMeshData md = globalMeshGenerator.generateDecompData(d);
            if (token.IsCancellationRequested) return;

            UnityMainThreadDispatcher.Instance().Enqueue(() => {
                if (token.IsCancellationRequested) return;      
                m = new Mesh();
                if (is32Bit) m.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                m.vertices = md.verts;
                m.uv = md.uvs;
                m.triangles = md.tris;
                m.name = d.offset.ToString();

                if (generateNormals) m.RecalculateNormals();

                if (createGo) {
                    go = GameObject.Instantiate(general.globalTerrainPrefab);
                    go.GetComponent<MeshFilter>().mesh = m;
                    go.transform.parent = parent.transform;
                    go.name = d.offset.ToString();
                }
            });
            }
            catch (Exception e) {
                Debug.Log(e);
            }
        });

        return t;
    }

    public Color32[] generateNormalMap(int sx, int sy, int wx, int wy, int tx) {
        List<Vector3> normals = new List<Vector3>();
        m.GetNormals(normals);
        Color32[] cs = new Color32[wx * wy];
        for (int y = sy; y < sy + wy; y++) {
            for (int x = sx; x < sx + wx; x++) {
                int indexN = y * tx + x;
                int indexC = (y - sy) * wy + x - sx;
                Vector3 c = normals[indexN];
                cs[indexC].r = (byte) (255f * (c.x + 1f) / 2f);
                cs[indexC].g = (byte) (255f * (c.y + 1f) / 2f);
                cs[indexC].b = (byte) (255f * (c.z + 1f) / 2f);
                cs[indexC].a = 255;
            }
        }

        return cs;
    }

    public void requestKill() {
        if (!exists) return;
        if (go != null) {
            GameObject.Destroy(go.GetComponent<MeshFilter>().sharedMesh);
            GameObject.Destroy(go);
        }
        token.Cancel();
    }
}

[Flags] internal enum corner : int {
    ll = 0, tl = 1, tr = 2, lr = 3
}
