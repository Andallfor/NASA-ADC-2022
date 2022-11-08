using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> The controls of the player. </summary>
public class playerControls : MonoBehaviour {
    private bodyRotationalControls bodyRotation;
    public void Awake() {
        bodyRotation = new bodyRotationalControls();
    }

    void Update() {
        if (!master.initialized) return;

        if (master.currentState == programStates.interplanetary || master.currentState == programStates.planetaryTerrain) {
            bodyRotation.update();
        }

        if (master.currentState == programStates.planetaryTerrain) {
            if (Input.GetKeyDown(KeyCode.Escape)) {
                master.changeState(programStates.interplanetary);
            }
        }
    }
}

internal class bodyRotationalControls {
    private Vector3 planetFocusMousePosition, planetFocusMousePosition1, rotation;
    private float change;
    public void update() {
        if (Input.GetMouseButtonDown(0)) planetFocusMousePosition = Input.mousePosition;
        else if (Input.GetMouseButton(0)) {
            Vector3 difference = Input.mousePosition - planetFocusMousePosition;
            planetFocusMousePosition = Input.mousePosition;

            Vector2 adjustedDifference = new Vector2(-difference.y / Screen.height, difference.x / Screen.width);
            adjustedDifference *= 100f;

            if (master.currentState == programStates.interplanetary) adjustedDifference = new Vector2(
                (float) (adjustedDifference.x / (1500.0 / master.scale)),
                (float) (adjustedDifference.y / (1500.0 / master.scale)));

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
            if (master.currentState == programStates.interplanetary)
            {
                change = (float)(0.1 * (master.scale - 370)) * Mathf.Sign(Input.mouseScrollDelta.y);
            }
            else
            {
                change = -(float)(0.001 * (master.scale - 370)) * Mathf.Sign(Input.mouseScrollDelta.y);
            }
            
            master.scale -= change;
        }
        
        general.camera.transform.RotateAround(Vector3.zero, general.camera.transform.right, rotation.x);
        general.camera.transform.RotateAround(Vector3.zero, general.camera.transform.up, rotation.y);
        general.camera.transform.rotation *= Quaternion.AngleAxis(rotation.z, Vector3.forward);
        
        if (rotation.x != 0) {
            rotation.x = (float)(rotation.x * Mathf.Pow((float)0.1, Time.deltaTime));
            if (Mathf.Abs(rotation.x) < 0.1) { 
                rotation.x = 0;
            }
        }
        if (rotation.y != 0) {
            rotation.y = (float)(rotation.y * Mathf.Pow((float)0.1, Time.deltaTime));
            if (Mathf.Abs(rotation.y) < 0.1) { 
                rotation.y = 0;
            }
        }
    }
}
