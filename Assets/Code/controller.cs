using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class controller : MonoBehaviour {
    planet sun, earth, moon;
    facility haworth;
    public bool usingTerrain = false;
    void Awake() {
        //terrain.processRegion("haworth", 4, 6);

        sun = new planet(
            new bodyInfo("sun", 696340, new timeline(), bodyType.sun),
            new bodyRepresentationInfo(general.defaultMat, false));
        earth = new planet(
            new bodyInfo("earth", 6371, new timeline(1.494757194768592E+08, 1.684950075464667E-02, 4.160341414638201E-03, 2.840153557215478E+02, 1.818203397569767E+02, 2.704822621765425E+02, time.strDateToJulian("2022 Oct 8 00:00:00.0000"), 3.986004418e14), bodyType.planet),
            new bodyRepresentationInfo(general.earthMat));
        moon = new planet(
            new bodyInfo("moon", 1737.4, new timeline(0.3844e5, 0.0549, 5.145, 308.92, 0, 0, 0, 4902.800066), bodyType.moon),
            new bodyRepresentationInfo(general.moonMat));
        haworth = new facility("Haworth", new geographic(-86.9, -4), moon);

        body.addFamilyNode(sun, earth);
        body.addFamilyNode(earth, moon);

        master.referenceFrame = moon;

        master.markInit();

        Coroutine mainClock = StartCoroutine(internalClock(3600, int.MaxValue, (tick) => {
            earth.updatePosition();
            moon.updatePosition();
            haworth.update();

            master.incrementTime(0.0001);
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

    public void Update() {
        if (!usingTerrain) {
            Vector3 screenPos = general.camera.WorldToScreenPoint(haworth.representation.transform.position);
            float dist = Vector3.Distance(screenPos, Input.mousePosition);
            float size = general.screenSize(haworth.representation.GetComponent<MeshRenderer>(), haworth.representation.transform.position) / 2.5f;
            if (dist < size) {
                haworth.indicateSelection();

                if (Input.GetMouseButtonDown(0)) {
                    usingTerrain = true;
                    haworth.parent.representation.SetActive(false);
                    haworth.label.gameObject.SetActive(false);
                    general.camera.transform.position = new Vector3(0, 0, -20);
                    general.camera.transform.eulerAngles = new Vector3(0, 0, 0);

                    for (int i = 0; i < 3; i++) {
                        for (int j = 0; j < 3; j++) {
                            terrain.generate("haworth", 6, i, j);
                        }
                    }
                }
            } else haworth.indicateDeselection();
        } else {
            terrain.update();

            if (Input.GetKeyDown(KeyCode.Escape)) {
                usingTerrain = false;
                haworth.parent.representation.SetActive(true);
                terrain.clearMeshes();
                haworth.label.gameObject.SetActive(true);
                master.playerPosition = new position(0, 0, 0);
            }
        }
    }
}
