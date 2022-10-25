using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class controller : MonoBehaviour {
    planet sun, earth, moon;
    crater haworth, shackletonPeak;
    public bool usingTerrain = false;
    public Gradient gradient;
    void Awake() {
        terrain.processRegion("haworth", 20, 1,1,gradient);
        //terrain.processRegion("peak near shackleton", 6, 1,1,gradient);
        
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
        haworth = new crater("Haworth", new geographic(-86.7515237574502, -22.7749958363969), moon, new terrainFilesInfo("haworth", new List<Vector2Int>() {new Vector2Int(6, 4)}));
        shackletonPeak = new crater("Peak Near Shackleton", new geographic(-88.8012678662351, 123.683478996976), moon, new terrainFilesInfo("peak near shackleton", new List<Vector2Int>() {new Vector2Int(6, 4)}));

        body.addFamilyNode(sun, earth);
        body.addFamilyNode(earth, moon);

        master.referenceFrame = moon;

        master.markInit();

        Coroutine mainClock = StartCoroutine(internalClock(3600, int.MaxValue, (tick) => {
            master.incrementTime(0.0001);

            earth.updatePosition();
            moon.updatePosition();
            haworth.update();
            shackletonPeak.update();
            
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
