using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public static class globalMeshGenerator {
    private static Dictionary<Vector2Int, int[]> triangles = new Dictionary<Vector2Int, int[]>() {
        {new Vector2Int(250, 250), genTriangles(250, 250)},
        {new Vector2Int(200, 200), genTriangles(200, 200)}};
    public static string folder;

    public static decompTerrainData requestGlobalTerrain(string subFolder, Vector2Int fileStart, Vector2Int pStart, Vector2Int pEnd, int rlevel, int qual, bool isSmall) {
        double x = fileStart.x;
        if (fileStart.x < 0) x += 360f;

        decompTerrainData decomp = new decompTerrainData();
        decomp.offset = new geographic(fileStart.y, fileStart.x);
        decomp.srcSize = new Vector2Int(pEnd.x - pStart.x, pEnd.y - pStart.y);
        decomp.size = new Vector2Int(decomp.srcSize.x / (int) Math.Pow(2, rlevel), decomp.srcSize.y / (int) Math.Pow(2, rlevel));
        decomp.res = (int) Math.Pow(2, rlevel);
        decomp.start = pStart;
        decomp.end = pEnd;
        decomp.isSmall = isSmall;
        if (isSmall) {
            decomp.stepSizeGeoX = 60;
            decomp.stepSizeGeoY = 60;
            decomp.fileLengthX = 2000;
            decomp.fileLengthY = 2000;
        } else {
            decomp.stepSizeGeoX = 30;
            decomp.stepSizeGeoY = 15;
            decomp.fileLengthX = 32000;
            decomp.fileLengthY = 16000;
        }

        string prefix = isSmall ? "small_" : "";
        string ns = format(fileStart.y, 2);
        string ne = format(fileStart.y + (int) decomp.stepSizeGeoY, 2);
        string ss = format((int) x, 3, false);
        string se = format((int) x + (int) decomp.stepSizeGeoX, 3, false);
        string name = Path.Combine(folder, subFolder, prefix + $"trn_1024_{ns}_{ne}_{ss}_{se}.jp2");

        if (!File.Exists(name)) throw new ArgumentException("Unable to find specified file " + name);

        // TODO: add function that reads header of files to extract the allowed ranges of these numbers?
        int[] heights = openJpegWrapper.requestTerrain(name, pStart, pEnd, (uint) rlevel, (uint) qual);
        decomp.data = heights;

        return decomp;
    }

    public static GameObject generateDecompData(decompTerrainData data) {
        // TODO: pass in data as a percent of max height, that way we can use shaders (since the data will be 0-1)?
        // look into alt ways of minimizing stored data in jp2/write own jp2 writer
        int len = data.size.x * data.size.y;
        Vector3[] verts = new Vector3[len];
        for (int i = 0; i < len; i++) {
            int x = i % data.size.x;
            int y = (i - x) / data.size.x;
            geographic p = new geographic(
                data.offset.lat + (float) (data.start.y + y * data.res) / data.fileLengthY * data.stepSizeGeoY,
                data.offset.lon + (float) (data.start.x + x * data.res) / data.fileLengthX * data.stepSizeGeoX);
            
            position point = p.toCartesian(1737.1 - 32.767 + (float) data.data[i] / 1000f).swapAxis() / master.scale;
            verts[i] = (Vector3) point;
        }

        int[] tris;
        if (triangles.ContainsKey(data.size)) tris = triangles[data.size];
        else {
            Debug.LogWarning("Do not have pregenerated triangle array of size " + data.size.ToString() + ". Generating new triangle array of this size.");
            tris = genTriangles(data.size.x, data.size.y);
            triangles[data.size] = tris;
        }

        // TODO: pregenerate high resolution normal map!
        Mesh m = new Mesh();
        m.vertices = verts;
        m.triangles = tris;
        m.name = data.offset.ToString();
        m.RecalculateNormals();

        GameObject go = GameObject.Instantiate(general.globalTerrainPrefab);
        go.GetComponent<MeshFilter>().mesh = m;
        return go;
    }

    private static string format(int v, int c, bool useSuffix = true) {
        string suffix = useSuffix ? (v < 0 ? "s" : "n") : "";
        string av = Math.Abs(v).ToString();
        int len = av.Length;

        if (v == 0) return new String('0', c) + suffix;
        else if (len < c) return new String('0', c - len) + av + suffix;
        else return av + suffix;
    }

    private static int[] genTriangles(int x, int y) {
        int[] trianglePreset = new int[x * y * 6];
        int tri = 0;
        int ver = 0;

        for (int d = 0; d < y - 1; d++) {
            for (int i = 0; i < x - 1; i++) {
                trianglePreset[tri + 0] = 0 + ver;
                trianglePreset[tri + 1] = (x - 1) + 1 + ver;
                trianglePreset[tri + 2] = ver + 1;
                trianglePreset[tri + 3] = 0 + ver + 1;
                trianglePreset[tri + 4] = (x - 1) + 1 + ver;
                trianglePreset[tri + 5] = ver + (x - 1) + 2;
                ver++;
                tri += 6;
            }
            ver++;
        }

        return trianglePreset;
    }
}
