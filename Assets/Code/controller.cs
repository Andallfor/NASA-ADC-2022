using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class controller : MonoBehaviour {
    planet sun, earth, moon;
    crater haworth, shackletonPeak,regional,amudsenRim,connectingRidge,connectingRidgeExtension,deGerlacheKocherMassif,deGerlacheRim1,deGerlacheRim2;
    public bool usingTerrain = false;
    public Gradient gradient;
    
    void Awake() {
        //terrain.processRegion("haworth", 20, 1,1,gradient);
        //terrain.processRegion("regional", 20, 1, 1, gradient);
        //terrain.processRegion("peak near shackleton", 6, 1,1,gradient);
        //terrain.processRegion("amudsen rim", 20, 1, 1, gradient);
        //terrain.processRegion("connecting ridge", 20, 1, 1, gradient);
        //terrain.processRegion("connecting ridge extension", 20, 1, 1, gradient);
        //terrain.processRegion("de gerlache kocher massif", 20, 1, 1, gradient);
        //terrain.processRegion("de gerlache rim 1", 20, 1, 1, gradient);
        //terrain.processRegion("de gerlache rim 2", 20, 1, 1, gradient);
        //return;


        master.onStateChange += terrain.onStateChange;

        sun = new planet(
            new bodyInfo("sun", 696340, new timeline(), bodyType.sun),
            new bodyRepresentationInfo(general.defaultMat, false));
        earth = new planet(
            new bodyInfo("earth", 6371, new timeline(1.494757194768592E+08, 1.684950075464667E-02, 4.160341414638201E-03, 2.840153557215478E+02, 1.818203397569767E+02, 2.704822621765425E+02, time.strDateToJulian("2022 Oct 8 00:00:00.0000"), 3.986004418e14), bodyType.planet),
            new bodyRepresentationInfo(general.earthMat));
        moon = new planet(
            new bodyInfo("moon", 1737.4, new timeline(0.3844e5, 0.0549, 5.145, 308.92, 0, 0, 0, 4902.800066), bodyType.moon),
            new bodyRepresentationInfo(general.moonMat));
        haworth = new crater("Haworth", new geographic(-86.7515237574502, -22.7749958363969), moon, new terrainFilesInfo("haworth", new List<Vector2Int>() {new Vector2Int(20, 1)}));
        regional = new crater("regional", new geographic(-90, 0), moon, new terrainFilesInfo("regional", new List<Vector2Int>() { new Vector2Int(20, 1) }));
        amudsenRim = new crater("amudsen rim", new geographic(-84.227, 69.444), moon, new terrainFilesInfo("amudsen rim", new List<Vector2Int>() { new Vector2Int(20, 1) }));
        shackletonPeak = new crater("Peak Near Shackleton", new geographic(-88.8012678662351, 123.683478996976), moon, new terrainFilesInfo("peak near shackleton", new List<Vector2Int>() {new Vector2Int(6, 1)}));
        connectingRidge = new crater("Connecting Ridge", new geographic(-89.4418, -137.5314), moon, new terrainFilesInfo("connecting ridge", new List<Vector2Int>() { new Vector2Int(20, 1) }));
        connectingRidgeExtension = new crater("Connecting Ridge Extension", new geographic(-89.0134, -101.9614), moon, new terrainFilesInfo("connecting ridge extension", new List<Vector2Int>() { new Vector2Int(20, 1) }));
        deGerlacheKocherMassif = new crater("De Gerlache Kocher Massif", new geographic(-85.8252227835536, -116.321872646458), moon, new terrainFilesInfo("de gerlache kocher massif", new List<Vector2Int>() { new Vector2Int(20, 1) }));
        deGerlacheRim1 = new crater("De Gerlache Rim 1", new geographic(-88.6745888041235, -67.9382548686084), moon, new terrainFilesInfo("de gerlache rim 1", new List<Vector2Int>() { new Vector2Int(20, 1) }));
        deGerlacheRim2 = new crater("De Gerlache Rim 2", new geographic(-88.2197331954664, -64.6329487169338), moon, new terrainFilesInfo("de gerlache rim 2", new List<Vector2Int>() { new Vector2Int(20, 1) }));
        body.addFamilyNode(sun, earth);
        body.addFamilyNode(earth, moon);

        master.referenceFrame = moon;

        master.markInit();
        

        Coroutine mainClock = StartCoroutine(internalClock(3600, int.MaxValue, (tick) => {
            master.incrementTime(.001);//0.0001
            
            earth.updatePosition();
            sun.updatePosition();
            moon.updatePosition();
            haworth.update();
            regional.update();
            amudsenRim.update();
            shackletonPeak.update();
            connectingRidge.update();
            connectingRidgeExtension.update();
            deGerlacheKocherMassif.update();
            deGerlacheRim1.update();
            deGerlacheRim2.update();
            
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
