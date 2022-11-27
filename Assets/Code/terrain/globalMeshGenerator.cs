using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class globalMeshGenerator {
    private static Dictionary<Vector2Int, int[]> trianglePresets = new Dictionary<Vector2Int, int[]>();
    private static Vector2 stepSizeGeo = new Vector2(15, 30);

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
