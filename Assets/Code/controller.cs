using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Text;

public class controller : MonoBehaviour {
    void Awake() {
        //terrain.processRegion("haworth", 20, 1);
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
        //terrain.processRegion("leibnitz beta plateau", 20, 1);
        //return;

        //globalMeshGenerator.initialize();
        //List<Vector2Int> areas = new List<Vector2Int>() {new Vector2Int(15, 30), new Vector2Int(30, 30), /*new Vector2Int(15, 60), new Vector2Int(30, 60)*/};
        //foreach (Vector2Int a in areas) {
        //    globalMeshGenerator.generateTile(3, a, new Vector3Int(0, 0, 32));
        //    globalMeshGenerator.generateTile(3, a, new Vector3Int(0, 250 * 32 - 1, 32));
        //    globalMeshGenerator.generateTile(3, a, new Vector3Int(250 * 32 - 1, 0, 32));
        //    globalMeshGenerator.generateTile(3, a, new Vector3Int(250 * 32 - 1, 250 * 32 - 1, 32));
        //}


        master.onStateChange += terrain.onStateChange;

        planet sun = new planet(
            new bodyInfo("sun", 696340, new timeline(), bodyType.sun),
            new bodyRepresentationInfo(general.defaultMat, false));
        planet earth = new planet(
            new bodyInfo("earth", 6371, new timeline(1.494757194768592E+08, 1.684950075464667E-02, 4.160341414638201E-03, 2.840153557215478E+02, 1.818203397569767E+02, 2.704822621765425E+02, time.strDateToJulian("2022 Oct 8 00:00:00.0000"), 3.986004418e14), bodyType.planet),
            new bodyRepresentationInfo(general.earthMat));
        planet moon = new planet(
            new bodyInfo("moon", 1737.4, new timeline(3.864958215095060E+05, 4.538937397897071E-02, 2.745404561156122E+01, 3.005845860250088E+02, 7.757615787462679, 5.234116546697739E+01, 2459861.5, 3.9860E+7), bodyType.moon),
            new bodyRepresentationInfo(general.moonMat));

        crater haworth =                  new crater("Haworth",                    new geographic(-86.7515237574502, -22.7749958363969), moon, new terrainFilesInfo("haworth",                    new List<Vector2Int>() {new Vector2Int(20, 1)}));
        crater amudsenRim =               new crater("Amundsen Rim",               new geographic(-84.227, 69.444),                      moon, new terrainFilesInfo("amundsen rim",               new List<Vector2Int>() {new Vector2Int(20, 1)}));
        crater shackletonPeak =           new crater("Peak Near Shackleton",       new geographic(-88.8012678662351, 123.683478996976),  moon, new terrainFilesInfo("peak near shackleton",       new List<Vector2Int>() {new Vector2Int(20, 1)}));
        crater connectingRidge =          new crater("Connecting Ridge",           new geographic(-89.4418, -137.5314),                  moon, new terrainFilesInfo("connecting ridge",           new List<Vector2Int>() {new Vector2Int(20, 1)}));
        crater connectingRidgeExtension = new crater("Connecting Ridge Extension", new geographic(-89.0134, -101.9614),                  moon, new terrainFilesInfo("connecting ridge extension", new List<Vector2Int>() {new Vector2Int(20, 1)}));
        crater deGerlacheKocherMassif =   new crater("De Gerlache Kocher Massif",  new geographic(-85.8252227835536, -116.321872646458), moon, new terrainFilesInfo("de gerlache kocher massif",  new List<Vector2Int>() {new Vector2Int(20, 1)}));
        crater deGerlacheRim1 =           new crater("De Gerlache Rim 1",          new geographic(-88.6745888041235, -67.9382548686084), moon, new terrainFilesInfo("de gerlache rim 1",          new List<Vector2Int>() {new Vector2Int(20, 1)}));
        crater deGerlacheRim2 =           new crater("De Gerlache Rim 2",          new geographic(-88.2197331954664, -64.6329487169338), moon, new terrainFilesInfo("de gerlache rim 2",          new List<Vector2Int>() {new Vector2Int(20, 1)}));

        body.addFamilyNode(sun, earth);
        body.addFamilyNode(earth, moon);

        master.referenceFrameBody = moon;

        master.markInit();

        master.scale = master.scale; // update all planets scale

        Coroutine mainClock = StartCoroutine(internalClock(3600, int.MaxValue, (tick) => {
            master.incrementTime(0.0001);

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
