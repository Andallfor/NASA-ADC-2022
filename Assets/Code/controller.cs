using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Threading.Tasks;

public class controller : MonoBehaviour {
    public Texture2D tex;
    public void Update() {
        if (Input.GetKeyDown("a")) {
            //test();
        }
    }

    private async void test() {
        int bx = Mathf.Min(255, (int) Math.Floor(craterTerrainController.currentCrater.terrainData.bounds["size"][0] / 20.0));
        int by = Mathf.Min(255, (int) Math.Floor(craterTerrainController.currentCrater.terrainData.bounds["size"][1] / 20.0));
        tex = new Texture2D(bx, by);
        Color[] c = new Color[bx * by];
        Task<bool>[] tasks = new Task<bool>[bx * by];
        for (int y = 0; y < by; y++) {
            for (int x = 0; x < bx; x++) {
                int index = x + y * bx;
                tasks[index] = visibility.getVisibility(index, new position(361000, 0, -42100), Vector3.zero, drawDebug: false);
            }
        }

        await Task.WhenAll(tasks);

        for (int i = 0; i < tasks.Length; i++) c[i] = tasks[i].Result ? Color.red : Color.green;

        tex.SetPixels(c);
        tex.Apply();

        byte[] data = tex.EncodeToPNG();
        File.WriteAllBytes("C:/Users/leozw/Desktop/out.png", data);
    }

    public static planet moon;

    void Awake() {
        master.canvas= GameObject.Find("Canvas");
        //terrain.processRegion("haworth", 20, 1); // TODO: 20 actually isnt big enough, some values are still cut off
        //terrain.processRegion("nobile rim 1", 20, 1);
        //terrain.processRegion("nobile rim 2", 20, 1);
        //terrain.processRegion("malapert massif", 20, 1);
        //terrain.processRegion("peak near shackleton", 20, 1);
        //terrain.processRegion("amundsen rim", 20, 1);
        //terrain.processRegion("connecting ridge", 20, 1);
        //terrain.processRegion("connecting ridge extension", 20, 1);
        //terrain.processRegion("de gerlache kocher massif", 20, 1);
        //terrain.processRegion("de gerlache rim 1", 20, 1);
        //terrain.processRegion("de gerlache rim 2", 20, 1);
        //terrain.processRegion("faustini rim a", 20, 1);
        //craterTerrainController.processRegion("leibnitz beta plateau", 20, 1);
        //craterTerrainController.processRegion("regional", 20, 1);

        //globalMeshGenerator.folder = "C:/Users/ltriv/Downloads/ADC Files/global/out";
        globalMeshGenerator.folder = "C:/Users/leozw/Desktop/ADC/global/out";

        master.onStateChange += craterTerrainController.onStateChange;

        planet sun = new planet(
            new bodyInfo("Sun", 696340, new timeline(), bodyType.sun),
            new bodyRepresentationInfo(general.defaultMat, false));
        planet earth = new planet(
            new bodyInfo("Earth", 6371, new timeline(1.494757194768592E+08, 1.684950075464667E-02, 4.160341414638201E-03, 2.840153557215478E+02, 1.818203397569767E+02, 2.704822621765425E+02, time.strDateToJulian("2022 Oct 8 00:00:00.0000"), 3.986004418e14), bodyType.planet),
            new bodyRepresentationInfo(general.earthMat));
        moon = new planet(
            new bodyInfo("Luna", 1737.4, new timeline(3.864958215095060E+05, 4.538937397897071E-02, 2.745404561156122E+01, 3.005845860250088E+02, 7.757615787462679, 5.234116546697739E+01, 2459861.5, 3.9860E+7), bodyType.moon),
            new bodyRepresentationInfo(general.moonMat));
        master.moon = moon.representation;

        crater haworth =                  new crater("Haworth",                    new geographic(-86.7515237574502, -22.7749958363969), moon, new terrainFilesInfo("haworth",                    new List<Vector2Int>() {new Vector2Int(20, 1)}));
        crater amudsenRim =               new crater("Amundsen Rim",               new geographic(-84.227, 69.444),                      moon, new terrainFilesInfo("amundsen rim",               new List<Vector2Int>() {new Vector2Int(20, 1)}));
        crater shackletonPeak =           new crater("Peak Near Shackleton",       new geographic(-88.8012678662351, 123.683478996976),  moon, new terrainFilesInfo("peak near shackleton",       new List<Vector2Int>() {new Vector2Int(20, 1)}));
        crater connectingRidge =          new crater("Connecting Ridge",           new geographic(-89.4418, -137.5314),                  moon, new terrainFilesInfo("connecting ridge",           new List<Vector2Int>() {new Vector2Int(20, 1)}));
        crater connectingRidgeExtension = new crater("Connecting Ridge Extension", new geographic(-89.0134, -101.9614),                  moon, new terrainFilesInfo("connecting ridge extension", new List<Vector2Int>() {new Vector2Int(20, 1)}));
        crater deGerlacheKocherMassif =   new crater("De Gerlache Kocher Massif",  new geographic(-85.8252227835536, -116.321872646458), moon, new terrainFilesInfo("de gerlache kocher massif",  new List<Vector2Int>() {new Vector2Int(20, 1)}));
        crater deGerlacheRim1 =           new crater("De Gerlache Rim 1",          new geographic(-88.6745888041235, -67.9382548686084), moon, new terrainFilesInfo("de gerlache rim 1",          new List<Vector2Int>() {new Vector2Int(20, 1)}));
        crater deGerlacheRim2 =           new crater("De Gerlache Rim 2",          new geographic(-88.2197331954664, -64.6329487169338), moon, new terrainFilesInfo("de gerlache rim 2",          new List<Vector2Int>() {new Vector2Int(20, 1)}));
        crater faustiniRimA =             new crater("Faustini Rim A",             new geographic(-87.8810364565, 90.0000000000112),     moon, new terrainFilesInfo("faustini rim a",             new List<Vector2Int>() {new Vector2Int(20, 1)}));
        crater leibnitzBetaPlateau =      new crater("Leibnitz Beta Plateau",      new geographic(-85.4240543764601, 31.7427274315016),  moon, new terrainFilesInfo("leibnitz beta plateau",      new List<Vector2Int>() {new Vector2Int(20, 1)}));
        crater malapertMassif =           new crater("Malapert Massif",            new geographic(-85.9898087775699, -0.23578026342289), moon, new terrainFilesInfo("malapert massif",            new List<Vector2Int>() {new Vector2Int(20, 1)}));
        crater nobileRim1 =               new crater("Nobile Rim 1",               new geographic(-85.4341491794868, 37.3666165277427),  moon, new terrainFilesInfo("nobile rim 1",               new List<Vector2Int>() {new Vector2Int(20, 1)}));
        crater nobileRim2 =               new crater("Nobile Rim 2",               new geographic(-83.9510469034538, 58.8220823262493),  moon, new terrainFilesInfo("nobile rim 2",               new List<Vector2Int>() {new Vector2Int(20, 1)}));
        crater mareCrisium =               new crater("mareCrisium",               new geographic(17, 59.1),  moon, new terrainFilesInfo("nobile rim 2",               new List<Vector2Int>() {new Vector2Int(20, 1)}));
        crater mareFecunditatis =               new crater("mareFecunditatis",               new geographic(-7.8, 51.3),  moon, new terrainFilesInfo("nobile rim 2",               new List<Vector2Int>() {new Vector2Int(20, 1)}));
        crater mareTranquillitatis =               new crater("mareTranquillitatis",               new geographic(8.5, 31.4),  moon, new terrainFilesInfo("nobile rim 2",               new List<Vector2Int>() {new Vector2Int(20, 1)}));
        crater kepler =               new crater("kepler",               new geographic(8.1, -38.0),  moon, new terrainFilesInfo("nobile rim 2",               new List<Vector2Int>() {new Vector2Int(20, 1)}));
        crater tycho =               new crater("tycho",               new geographic(-43.31, -11.36),  moon, new terrainFilesInfo("nobile rim 2",               new List<Vector2Int>() {new Vector2Int(20, 1)}));

        body.addFamilyNode(sun, earth);
        body.addFamilyNode(earth, moon);

        master.referenceFrameBody = moon;

        new globalTerrainController(moon, true);
        //await globalTerrainController.generateNormalMaps(moon);
        /*var d = globalMeshGenerator.requestGlobalTerrain("Luna", new Vector2Int(30, 45), new Vector2Int(0, 0), new Vector2Int(32000, 16000), 2, 4, false);
        var dd = globalMeshGenerator.generateDecompData(d);
        
        Mesh m = new Mesh();
        m.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        m.vertices = dd.verts;
        m.triangles = dd.tris;
        m.uv = dd.uvs;
        m.RecalculateNormals();
        GameObject go = GameObject.Instantiate(general.defaultPrefab);
        go.GetComponent<MeshRenderer>().sharedMaterial = (Material)Resources.Load("materials/globalTerrain");
        go.GetComponent<MeshFilter>().sharedMesh = m;*/


        var d = globalMeshGenerator.requestGlobalTerrain("Luna", new Vector2Int(30, 45), new Vector2Int(0, 0), new Vector2Int(32000, 16000), 2, 4, false);
        var dd = globalMeshGenerator.generateDecompData(d);

        master.markInit();

        master.scale = master.scale; // update all planets scale
        master.changeState(programStates.interplanetary);

        Coroutine mainClock = StartCoroutine(internalClock(3600, int.MaxValue, (tick) => {
            master.incrementTime(master.timestep);

            master.propagateUpdate();
            
            master.notifyUpdateEnd();
        }, null));
    }

    /// <summary> Accurate clock to ensure that everything updates at roughly the same interval. </summary>
    public IEnumerator internalClock(float tickRate, int requestedTicks, Action<int> callback, Action termination) {
        // https://gist.github.com/Andallfor/7c2e9d17e28391a9d800a24be7b3e375
        float timePerTick = 1000f * (60f / tickRate);
        float tickBucket = 0;
        int tickCount = 0;

        while (tickCount < requestedTicks)
        {
            tickBucket += UnityEngine.Time.deltaTime * 1000f;
            int ticks = (int) Mathf.Round((tickBucket - (tickBucket % timePerTick)) / timePerTick);
            tickBucket -= ticks *  timePerTick;

            // if we skip a tick(s), make sure to still call it. This behavior can be disabled by just removing
            // the for loop if undesired.
            for (int i = 0; i < ticks; i++)
            {
                callback(tickCount);
                tickCount++;
                if (tickCount < requestedTicks) break;
            }

            // using this timer method instead of WaitForSeconds as it is inaccurate for small numbers.
            yield return new WaitForEndOfFrame();
        }
        termination();
    }

}
