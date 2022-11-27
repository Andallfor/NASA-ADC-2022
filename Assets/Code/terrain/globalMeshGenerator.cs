using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public static class globalMeshGenerator {
    private static int[] triangles = genTriangles(250, 250);
    private const int stepSizeGeoX = 15, stepSizeGeoY = 30, meshSizeX = 250, meshSizeY = 250, fileLengthX = 32000, fileLengthY = 16000;
    public static string folder;

    public static decompTerrainData requestGlobalTerrain(Vector2Int fileStart, Vector2Int pStart, Vector2Int pEnd, int res, int qual) {
        double x = fileStart.x;
        if (fileStart.x < 0) x += 360f;

        string name = Path.Combine(folder, $"trn_1024_{formatLat(fileStart.y)}_{formatLat(fileStart.y + 15)}_{x}_{x + 30}.jp2");
        if (!File.Exists(name)) throw new ArgumentException("Unable to find specified file " + name);

        // TODO: add function that reads header of files to extract the allowed ranges of these numbers?
        return openJpegWrapper.requestTerrain(name, new geographic(fileStart.y, fileStart.x), pStart, pEnd, (uint) res, (uint) qual);
    }

    public static GameObject generateDecompData(decompTerrainData data) {
        // TODO: pass in data as a percent of max height, that way we can use shaders (since the data will be 0-1)?
        // look into alt ways of minimizing stored data in jp2/write own jp2 writer
        int len = data.height * data.width;
        Vector3[] verts = new Vector3[len];
        for (int i = 0; i < len; i++) {
            int x = i % 250;
            int y = (i - x) / 250;
            float px = (float) x / 250f;
            float py = (float) y / 250f;
            geographic p = new geographic(
                data.offset.lat + py * 15f,
                data.offset.lon + px * 30f);
            
            position point = p.toCartesian(1737.1 - 32.767 + (float) data.data[i] / 1000f) / master.scale;
            verts[i] = (Vector3) point;
        }

        Mesh m = new Mesh();
        m.vertices = verts;
        m.triangles = triangles;
        m.RecalculateNormals();

        GameObject go = GameObject.Instantiate(general.globalTerrainPrefab);
        go.GetComponent<MeshFilter>().mesh = m;
        return go;
    }

    private static string formatLat(int value) => $"{Math.Abs(value)}{(value >= 0 ? 'n' : 's')}";

    public static int[] genTriangles(int x, int y) {
        int[] trianglePreset = new int[x * y * 6];
        int tri = 0;
        int ver = 0;

        for (int d = 0; d < y - 1; d++) {
            for (int i = 0; i < x - 1; i++) {
                trianglePreset[tri + 5] = 0 + ver;
                trianglePreset[tri + 4] = (x - 1) + 1 + ver;
                trianglePreset[tri + 3] = ver + 1;
                trianglePreset[tri + 2] = 0 + ver + 1;
                trianglePreset[tri + 1] = (x - 1) + 1 + ver;
                trianglePreset[tri + 0] = ver + (x - 1) + 2;
                ver++;
                tri += 6;
            }
            ver++;
        }

        return trianglePreset;
    }
}
