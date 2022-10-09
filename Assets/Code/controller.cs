using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class controller : MonoBehaviour {
    planet sun, earth, moon;
    facility haworth;
    public bool usingTerrain = false;
    void Awake() {
        //terrain.processRegion("haworth", 4, 6);

        sun = new planet(
            new bodyInfo("sun", 696340, new timeline(), bodyType.sun),
            new bodyRepresentationInfo(general.defaultMat));
        earth = new planet(
            new bodyInfo("earth", 6371, new timeline(1.494757194768592E+08, 1.684950075464667E-02, 4.160341414638201E-03, 2.840153557215478E+02, 1.818203397569767E+02, 2.704822621765425E+02, time.strDateToJulian("2022 Oct 8 00:00:00.0000"), 3.986004418e14), bodyType.planet),
            new bodyRepresentationInfo(general.earthMat));
        moon = new planet(
            new bodyInfo("moon", 1737.4, new timeline(0.3844e5, 0.0549, 5.145, 308.92, 0, 0, 0, 4902.800066), bodyType.moon),
            new bodyRepresentationInfo(general.moonMat));
        haworth = new facility("Haworth", new geographic(-86.9, -4), moon);


        body.addFamilyNode(sun, earth);
        body.addFamilyNode(earth, moon);

        sun.representation.GetComponent<MeshRenderer>().enabled = false;
        sun.updateScale();
        earth.updateScale();
        moon.updateScale();

        master.sun = sun;
        master.referenceFrame = moon;
    }

    private Vector3 planetFocusMousePosition, planetFocusMousePosition1, rotation;

    public void Update() {
        earth.updatePosition();
        moon.updatePosition();
        haworth.update();

        master.incrementTime(0.0001);

        if (Input.GetMouseButtonDown(0)) planetFocusMousePosition = Input.mousePosition;
        else if (Input.GetMouseButton(0)) {
            Vector3 difference = Input.mousePosition - planetFocusMousePosition;
            planetFocusMousePosition = Input.mousePosition;

            Vector2 adjustedDifference = new Vector2(-difference.y / Screen.height, difference.x / Screen.width);
            adjustedDifference *= 100f;

            rotation.x = adjustedDifference.x;
            rotation.y = adjustedDifference.y;
            rotation.z = 0;
        }

        if (Input.GetMouseButtonDown(1)) planetFocusMousePosition1 = Input.mousePosition;
        if (Input.GetMouseButton(1)) {
            Vector3 difference = Input.mousePosition - planetFocusMousePosition1;
            planetFocusMousePosition1 = Input.mousePosition;

            float adjustedDifference = (difference.x / Screen.width) * 100;
            rotation.x = 0;
            rotation.y = 0;
            rotation.z = adjustedDifference;
        }

        if (Input.mouseScrollDelta.y != 0) {
            float change = (float) (0.1 * master.scale) * Mathf.Sign(Input.mouseScrollDelta.y);
            master.scale -= change;

            earth.updateScale();
            moon.updateScale();
            haworth.updateScale();
        }

        general.camera.transform.RotateAround(Vector3.zero, general.camera.transform.right, rotation.x);
        general.camera.transform.RotateAround(Vector3.zero, general.camera.transform.up, rotation.y);
        general.camera.transform.rotation *= Quaternion.AngleAxis(rotation.z, Vector3.forward);

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
