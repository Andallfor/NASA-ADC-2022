using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum cameraModes
{
    typical,
    drone
}

/// <summary> The controls of the player. </summary>
public class playerControls : MonoBehaviour {
    cameraModes m = cameraModes.typical;

    private bodyRotationalControls bodyRotation;
    public void Awake() {
        bodyRotation = new bodyRotationalControls();
    }

    void Update() {
        if (!master.initialized) return;

        if (master.currentState == programStates.interplanetary || master.currentState == programStates.planetaryTerrain) {
            
            if (m==cameraModes.typical)
            {
                bodyRotation.update(cameraModes.typical);
                if (Input.GetKeyDown(KeyCode.C))
                {
                    m=cameraModes.drone;
                }
            }
            else if (m == cameraModes.drone)
            {
                bodyRotation.update(cameraModes.drone);
                if (Input.GetKeyDown(KeyCode.C))
                {
            
                    StartCoroutine(yes());
                    m = cameraModes.typical;
                    
                }
            }
        }

        if (master.currentState == programStates.planetaryTerrain) {
            if (Input.GetKeyDown(KeyCode.Escape)) {
                master.changeState(programStates.interplanetary);
            }
        }
    }
    IEnumerator yes()
    {
        for (int i = 0; i < 120; i++)
        {
            general.camera.transform.rotation =Quaternion.LookRotation(Vector3.RotateTowards(general.camera.transform.forward,( master.registeredPlanets[2].representation.transform.position-general.camera.transform.position), Mathf.Deg2Rad * Vector3.Angle(general.camera.transform.forward, master.registeredPlanets[2].representation.transform.position - general.camera.transform.position)/120, 0.0f));
            yield return null;
        }
    }
    
}

internal class bodyRotationalControls {
    
    private Vector3 planetFocusMousePosition, planetFocusMousePosition1, rotation;
    private float change;
    public void update(cameraModes mode) {
        if (Input.GetMouseButtonDown(0)) planetFocusMousePosition = Input.mousePosition;
        else if (Input.GetMouseButton(0)) {
            Vector3 difference = Input.mousePosition - planetFocusMousePosition;
            planetFocusMousePosition = Input.mousePosition;
            Vector2 adjustedDifference = new Vector2(-difference.y / Screen.height, difference.x / Screen.width) * 180f;

            float percent = (float) master.scale / (1500000f / (float) master.scale);
            if (master.currentState == programStates.planetaryTerrain) percent = 1;
            rotation.x = adjustedDifference.x * percent;
            rotation.y = adjustedDifference.y * percent;
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

        // TODO -> have this scale based on screen size of planet, not some predefined ratio
        if (Input.mouseScrollDelta.y != 0) {
            if (master.currentState == programStates.interplanetary)
            {
                change = (float) (6f*((Vector3.Distance(Vector3.zero,general.camera.transform.position)+ 0.005f - (1737.4f/master.scale))/2 *Mathf.Sign(Input.mouseScrollDelta.y)));
            }
            else
            {
                change = -(float)(0.001 * (master.scale)) * Mathf.Sign(Input.mouseScrollDelta.y);
            }
            master.scale -= change;
            if (1737.4f/(master.scale-change) < Vector3.Distance(Vector3.zero, general.camera.transform.position))
            {
               
            }
        }
        
        if (mode == cameraModes.typical)
        {
            
            general.camera.transform.RotateAround(Vector3.zero, general.camera.transform.right, rotation.x);
            general.camera.transform.RotateAround(Vector3.zero, general.camera.transform.up, rotation.y);
            general.camera.transform.rotation *= Quaternion.AngleAxis(rotation.z, Vector3.forward);
            
        }
        if(mode == cameraModes.drone)
        {
            if(Input.GetMouseButton(0))general.camera.transform.eulerAngles += new Vector3(-Input.GetAxisRaw("Mouse Y"), Input.GetAxisRaw("Mouse X"), 0.0f);
            general.camera.transform.position += .05f*Input.GetAxisRaw("Vertical")* general.camera.transform.forward+.005f * Input.GetAxisRaw("Horizontal") * general.camera.transform.right+0.0f*general.camera.transform.up;
            
        }

        if (rotation.magnitude < 0.01f) rotation = Vector3.zero;
        else rotation = new Vector3(
            rotation.x * Mathf.Pow(0.05f, Time.deltaTime),
            rotation.y * Mathf.Pow(0.05f, Time.deltaTime),
            rotation.z * Mathf.Pow(0.05f, Time.deltaTime));
    }
}
