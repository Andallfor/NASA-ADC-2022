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
    private Vector3 seekerPos = new Vector3(1, 0, 0);
    private Vector3 hiderPos = new Vector3(0, 0, 0);
    private Plane elevationPlane;
    private bool selected = false;
    private GameObject selectedObject = null;
    private int height = 0;
    private bodyRotationalControls bodyRotation;
    float times;
    public void Awake() {
        bodyRotation = new bodyRotationalControls();
        elevationPlane = new Plane(Vector3.up, new Vector3(0, -height, 0));
        
    }

    void Update() {

        if (!master.initialized) return;
        if (master.currentState == programStates.planetaryTerrain && Input.GetMouseButtonDown(0)&&selected==false)
        {
            times = Time.deltaTime;
            RaycastHit hit;
            Ray ray = general.camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider == GameObject.Find("seeker").GetComponent<Collider>())
                {
                    selected = true;
                    selectedObject = GameObject.Find("seeker");

                }
                else if(hit.collider == GameObject.Find("hider").GetComponent<Collider>())
                {
                    selected = true;
                    selectedObject = GameObject.Find("hider");
                }
            }
        }
        if (selected)
        {
            float enter = 0.0f;
            if (Input.GetMouseButtonDown(0)&&times!=Time.deltaTime)
            {
                selected = false;
                selectedObject = null;
            }
            
            else if(elevationPlane.Raycast(general.camera.ScreenPointToRay(Input.mousePosition),out enter))
            {
                
                Vector3 hitPoint = general.camera.ScreenPointToRay(Input.mousePosition).GetPoint(enter);
                selectedObject.transform.position = hitPoint;
            }
        }
        if (master.currentState == programStates.interplanetary || master.currentState == programStates.planetaryTerrain) {
            
            if (m==cameraModes.typical||master.currentState==programStates.planetaryTerrain)
            {
                bodyRotation.update(cameraModes.typical);
                if (Input.GetKeyDown(KeyCode.C)&& master.currentState != programStates.planetaryTerrain)
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
        Vector3 endPos = new Vector3(0, 0, 5);
        float step = Vector3.Angle(general.camera.transform.forward, (master.moon.transform.position - endPos).normalized) / 120;
        float step1 = Vector3.Distance(general.camera.transform.position, endPos)/120.0f;
        while(Vector3.Distance(general.camera.transform.position, endPos) > .25f) { 
            general.camera.transform.rotation =Quaternion.LookRotation(Vector3.RotateTowards(general.camera.transform.forward,( master.moon.transform.position-endPos).normalized, Mathf.Deg2Rad * step*Time.deltaTime*100, 10000f));

            general.camera.transform.position = Vector3.MoveTowards(general.camera.transform.position, endPos, step1*100*Time.deltaTime);
            yield return null;
        }
    }
    
}

internal class bodyRotationalControls {
    
    private Vector3 planetFocusMousePosition, planetFocusMousePosition1, rotation;
    private float change;

    public void update(cameraModes mode) {
        if (master.currentState == programStates.planetaryTerrain&&Input.GetKeyDown(KeyCode.Alpha1)) { craterTerrainController.mode = 0; }
        if (master.currentState == programStates.planetaryTerrain && Input.GetKeyDown(KeyCode.Alpha2)) { craterTerrainController.mode = 1; }
        if (master.currentState == programStates.planetaryTerrain && Input.GetKeyDown(KeyCode.Alpha3)) { craterTerrainController.mode = 2; }
        if (master.currentState == programStates.planetaryTerrain && Input.GetKeyDown(KeyCode.Alpha4)) { craterTerrainController.mode = 3; }
        if (master.currentState == programStates.planetaryTerrain && Input.GetKeyDown(KeyCode.Alpha5)) { craterTerrainController.mode = 4; master.scale = 4; }
        
        
        craterTerrainController.colorUpdate();
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
                change = (float) (6f*((Vector3.Distance(Vector3.zero,general.camera.transform.position)+ 0.005f - master.moon.transform.localScale.x/2)/2 *Mathf.Sign(Input.mouseScrollDelta.y)));
                if (master.moon.transform.localScale.x > 7.5)
                {

                    if (general.camera.fieldOfView > 60)
                    {
                        master.scale -= change;
                        general.camera.fieldOfView = 60;
                    }
                    else
                    {
                        general.camera.fieldOfView -= change/5;
                    }
                }
                else
                {
                    master.scale -= change;
                }
            }
            
            else if(craterTerrainController.mode!=4)
            {
                change = -(float)(0.01 * (master.scale)) * Mathf.Sign(Input.mouseScrollDelta.y);
                master.scale -= change;

            }

            //master.scale -= change;

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
            general.camera.transform.position += .05f*Input.GetAxisRaw("Vertical")* general.camera.transform.forward+.05f * Input.GetAxisRaw("Horizontal") * general.camera.transform.right+0.0f*general.camera.transform.up;
            
        }

        if (rotation.magnitude < 0.01f) rotation = Vector3.zero;
        else rotation = new Vector3(
            rotation.x * Mathf.Pow(0.05f, Time.deltaTime),
            rotation.y * Mathf.Pow(0.05f, Time.deltaTime),
            rotation.z * Mathf.Pow(0.05f, Time.deltaTime));
    }
}
