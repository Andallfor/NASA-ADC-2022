using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NumSharp;
using System.Net;
using UnityEngine.Rendering;
using System;
using Unity.Collections;
using System.Diagnostics;

public static class globalMeshGenerator {
    private static Dictionary<Vector2Int, int[]> trianglePresets = new Dictionary<Vector2Int, int[]>();
    private static bool alreadyInit = false;
    private static Vector2 stepSizeGeo = new Vector2(15, 30);
    private static Vector2 stepSizePoint = new Vector2(1024 * 15, 1024 * 30);
    private static serverConnection connection;
    private static ComputeShader computeShader = Resources.Load<ComputeShader>("shaders/heightToCart");
    public static void initialize() {
        if (alreadyInit) return;

        connection = new serverConnection(Dns.GetHostName(), 6969);

        trianglePresets = new Dictionary<Vector2Int, int[]>() {
            {new Vector2Int(250, 250), genTriangles(250, 250)},
            {new Vector2Int(251, 250), genTriangles(251, 250)},
            {new Vector2Int(250, 251), genTriangles(250, 251)},
            {new Vector2Int(251, 251), genTriangles(251, 251)}};

        alreadyInit = true;
    }

    private static int[] genTriangles(int x, int y) {
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

    public static async void generateTile(int layer, Vector2Int fileCoord, Vector3Int range, bool flush = false) {
        if (!((range.z != 0) && ((range.z & (range.z - 1)) == 0))) throw new ArgumentException("Resolution must be a power of 2");

        Stopwatch watch = new Stopwatch();
        watch.Start();

        globalMeshData data = await connection.requestLunarTerrainSocket(layer, fileCoord, range, flush);
        int[] heights = data.heights;

        // rewrite using dist to calc offset
        // pass in pixel start to buffer/modify bounds?
        // or maybe bounds is just incorrect due to incorrect bound values being passed in
        float distX = (float) stepSizeGeo.x * (((float) data.size.x * (float) range.z) / (float) stepSizePoint.x);
        float distY = (float) stepSizeGeo.y * (((float) data.size.y * (float) range.z) / (float) stepSizePoint.y);
        float offsetX = (float) fileCoord.x + (float) stepSizeGeo.x * ((float) range.x / stepSizePoint.x);
        float offsetY = (float) fileCoord.y + (float) stepSizeGeo.y * ((float) range.y / stepSizePoint.y);

        UnityEngine.Debug.Log($"{range} {offsetX} {offsetY} {distX} {distY}");

        Vector4 bounds = new Vector4(
            offsetX, offsetY,
            offsetX + distX, offsetY + distY);
        
        // TODO: get this to accept shorts/have requestLunarTerrain return shorts then convert to int
        GraphicsBuffer heightsBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, heights.Length, sizeof(int));
        GraphicsBuffer pointsBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, heights.Length, sizeof(float) * 3);
        
        int id = computeShader.FindKernel("heightToCart");

        computeShader.SetBuffer(id, "inHeights", heightsBuffer);
        computeShader.SetBuffer(id, "outPoints", pointsBuffer);
        computeShader.SetVector("bounds", bounds);
        computeShader.SetFloat("scale", (float) master.scale);
        computeShader.SetInt("boundX", data.size.x);
        computeShader.SetInt("boundY", data.size.y);
        heightsBuffer.SetData(heights);

        computeShader.GetKernelThreadGroupSizes(id, out uint threadX, out _, out _);
        computeShader.Dispatch(id, Mathf.CeilToInt((float) (data.size.x * data.size.y) / threadX), 1, 1);

        Mesh m = new Mesh();
        VertexAttributeDescriptor[] layout = new VertexAttributeDescriptor[1] {
            new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3)
        };
        AsyncGPUReadback.Request(pointsBuffer, (req) => {
            NativeArray<Vector3> arr = req.GetData<Vector3>();
            //m.SetVertexBufferParams(250 * 250, layout); // this causes mesh to hide itself/phase in and out of existance at different angles
            //m.SetVertexBufferData(arr, 0, 0, arr.Length);
            m.vertices = arr.ToArray();
            m.triangles = trianglePresets[data.size];
            m.RecalculateNormals();
            m.name = range.ToString();

            GameObject go = GameObject.Instantiate(general.bodyPrefab);
            go.GetComponent<MeshFilter>().mesh = m;

            heightsBuffer.Dispose();
            pointsBuffer.Release();

            watch.Stop();
            UnityEngine.Debug.Log($"(Total) {range}: layer {layer} and resolution {range.z} in {watch.ElapsedMilliseconds}ms");
        });
    }
}
