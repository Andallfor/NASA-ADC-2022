using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public enum cameraModes
{
    typical,
    drone
}

/// <summary> The controls of the player. </summary>
public class playerControls : MonoBehaviour {
    cameraModes m = cameraModes.typical;
    public GameObject marker, player,seeker,hider;
    public Texture2D blind, see;
    public updateScaleMap uScaleMap;
    private bool colorblind,paused;
    private Vector2 mpos;
    private Plane elevationPlane;
    private bool selected = false;
    private GameObject selectedObject = null;
    private int height = 0, currentMap;
    private bodyRotationalControls bodyRotation;
    float times;
    GameObject canvas;
    public void Awake() {
        seeker = GameObject.FindGameObjectWithTag("seeker");
        hider = GameObject.FindGameObjectWithTag("hider");
        bodyRotation = new bodyRotationalControls();
        elevationPlane = new Plane(Vector3.up, new Vector3(0, -height, 0));
        
        bodyRotation.player = player;
    }

    void Update() {
        if (Input.GetKeyDown("p")) {
            master.incrementTime(0.5);
        } else if (Input.GetKeyDown("o")) {
            master.incrementTime(-0.5);
        }

        /*if (master.currentState == programStates.planetaryTerrain)
        {
            mpos += new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            marker.transform.position = new Vector3(mpos.x, (float)craterTerrainController.getNodeData(new Vector2Int((int)(craterTerrainController.craterData[craterTerrainController.region].map.width / craterTerrainController.worldSize.x * (mpos.x - craterTerrainController.worldSize.x)), (int)(craterTerrainController.craterData[craterTerrainController.region].map.height / craterTerrainController.worldSize.y * (mpos.y - craterTerrainController.worldSize.y))), craterTerrainController.region).height-(float)craterTerrainController.avg, mpos.y);

        }*/
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (paused)
            {
                master.timestep = .001f;
                paused = false;
            }
            else if (!paused)
            {
                master.timestep = 0f;
                paused = true;
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha6)) colorblind = !colorblind;
        
        if (craterTerrainController.craterMat != null)
        {
            Material crater = craterTerrainController.craterMat;
            if (colorblind)
            {
                crater.SetColor("_key5",new Color(100/255f,143/255f,255/255f,1));
                crater.SetColor("_key4",new Color(120/255f,94/255f,240/255f,1));
                crater.SetColor("_key3",new Color(220/255f,38/255f,127/255f,1));
                crater.SetColor("_key2",new Color(254/255f,97/255f,0,1));
                crater.SetColor("_key1",new Color(255/255f,176/255f,0,1));

                bodyRotation.coloreye = bodyRotation.colorblind;
            }
            else
            {
                crater.SetColor("_key5", new Color(252/255f,253/255f,191/255f));
                crater.SetColor("_key4", new Color(252/255f,137/255f,97/255f));
                crater.SetColor("_key3", new Color(183/255f,55/255f,121/255f));
                crater.SetColor("_key2", new Color(81/255f,18/255f,124/255f));
                crater.SetColor("_key1", new Color(0,0,4/255f));
/*                crater.SetColor("_key5", new Color(0,0,4));
*/                
                bodyRotation.coloreye = bodyRotation.colorsee;
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha6)) updateScaleMap.update(bodyRotation.coloreye);
        
        if (!master.initialized) return;
        if (master.currentState == programStates.planetaryTerrain && Input.GetMouseButtonDown(0)&&selected==false)
        {
            times = Time.deltaTime;
            RaycastHit hit;
            Ray ray = general.camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider != null)
                {
                    if (hit.collider == seeker.GetComponent<CapsuleCollider>())
                    {
                        selected = true;
                        selectedObject = seeker;

                    }
                    else if (hit.collider == hider.GetComponent<CapsuleCollider>())
                    {
                        selected = true;
                        selectedObject = hider;
                    }
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
    private bool inFirstPerson;

    public GameObject player;

    public Texture2D colorblind, colorsee, coloreye;

    public bodyRotationalControls() {
        colorblind = Resources.Load("textures/colorblind") as Texture2D;
        colorsee = Resources.Load("textures/colorsee") as Texture2D;
        coloreye = colorsee;
    }

    public void update(cameraModes mode) {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            master.canvas.active = !master.canvas.active;
        }
        if (master.currentState == programStates.planetaryTerrain && Input.GetKeyDown(KeyCode.Alpha1)) { craterTerrainController.mode = 0; updateScaleMap.update(coloreye, 0);}
        if (master.currentState == programStates.planetaryTerrain && Input.GetKeyDown(KeyCode.Alpha2)) { craterTerrainController.mode = 1; updateScaleMap.update(coloreye, 1);}
        if (master.currentState == programStates.planetaryTerrain && Input.GetKeyDown(KeyCode.Alpha3)) { craterTerrainController.mode = 2; updateScaleMap.update(coloreye, 2);}
        if (master.currentState == programStates.planetaryTerrain && Input.GetKeyDown(KeyCode.Alpha4)) { craterTerrainController.mode = 3; updateScaleMap.update(coloreye, 3);}
        if (master.currentState == programStates.planetaryTerrain && Input.GetKeyDown(KeyCode.Alpha5)) { craterTerrainController.mode = 4; master.scale = 4; updateScaleMap.hide();}
        if (master.currentState == programStates.planetaryTerrain && Input.GetKeyDown(KeyCode.Alpha7)) { craterTerrainController.mode = 5; updateScaleMap.hide();}
        craterTerrainController.colorUpdate();

        if (!inFirstPerson) {
            if (Input.GetKeyDown("e")) {
                inFirstPerson = true;
                foreach (var m in craterTerrainController.activeMeshes) {
                    m.AddComponent<MeshCollider>();
                    m.transform.localScale = new Vector3(5000, 5000, 5000);
                    m.transform.localEulerAngles = new Vector3(180, 0, 0);
                }

                cc = player.GetComponent<CharacterController>();

                player.SetActive(true);
                player.transform.position = new Vector3(0, 10, 0);

                playerSpeed = 2;
                gravityValue = -9.18f;
                mX = 0;
                mY = 0;
                Cursor.lockState = CursorLockMode.Locked;
            }

            if (Input.GetKey("i")) rotation.y = 0.05f;

            if (Input.GetMouseButtonDown(0)) planetFocusMousePosition = Input.mousePosition;
            else if (Input.GetMouseButton(0)) {
                Vector3 difference = Input.mousePosition - planetFocusMousePosition;
                planetFocusMousePosition = Input.mousePosition;
                Vector2 adjustedDifference = new Vector2(-difference.y / Screen.height, difference.x / Screen.width) * 180f;

                float percent = general.camera.fieldOfView / 125f;
                if (master.currentState == programStates.planetaryTerrain) percent = 1;
                rotation.x = adjustedDifference.x * percent;
                rotation.y = adjustedDifference.y * percent;
                rotation.z = 0;
            }
            if (Input.GetAxisRaw("Horizontal") != 0 && mode == cameraModes.typical)
            {
                rotation.y = Time.deltaTime * 1 * Input.GetAxisRaw("Horizontal");
            }
            else
            {
                if (Input.GetMouseButtonDown(1)) planetFocusMousePosition1 = Input.mousePosition;
                if (Input.GetMouseButton(1))
                {
                    Vector3 difference = Input.mousePosition - planetFocusMousePosition1;
                    planetFocusMousePosition1 = Input.mousePosition;

                    float adjustedDifference = (difference.x / Screen.width) * 100;
                    rotation.x = 0;
                    rotation.y = 0;
                    rotation.z = adjustedDifference;
                }
            }
            // TODO -> have this scale based on screen size of planet, not some predefined ratio
            if (Input.mouseScrollDelta.y != 0) {
                if (master.currentState == programStates.interplanetary)
                {
                    //change = (float) (6f*((Vector3.Distance(Vector3.zero,general.camera.transform.position)+ 0.005f - master.moon.transform.localScale.x/2)/2 *Mathf.Sign(Input.mouseScrollDelta.y)));
                    change = Input.mouseScrollDelta.y * general.camera.fieldOfView / 20f;
                    general.camera.fieldOfView -= change;
                    general.camera.fieldOfView = Mathf.Max(Mathf.Min(general.camera.fieldOfView, 60), 0.5f);
                }
                else if(craterTerrainController.mode!=4)
                {
                    change = Input.mouseScrollDelta.y * general.camera.fieldOfView / 50f;
                    general.camera.fieldOfView -= change;
                    general.camera.fieldOfView = Mathf.Max(Mathf.Min(general.camera.fieldOfView, 90), 0.5f);
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
                if (Input.GetMouseButton(0)) { general.camera.transform.eulerAngles += new Vector3(-Input.GetAxisRaw("Mouse Y"), Input.GetAxisRaw("Mouse X"), 0.0f); }
                
                general.camera.transform.position += .05f*Input.GetAxisRaw("Vertical")* general.camera.transform.forward * Time.deltaTime * 5 + .05f * Input.GetAxisRaw("Horizontal") * general.camera.transform.right * Time.deltaTime * 5 + 0.0f*general.camera.transform.up;
                
            }
            Cursor.visible = !Input.GetMouseButton(0);

            if (rotation.magnitude < 0.01f) rotation = Vector3.zero;
            else rotation = new Vector3(
                rotation.x * Mathf.Pow(0.05f, Time.deltaTime),
                rotation.y * Mathf.Pow(0.05f, Time.deltaTime),
                rotation.z * Mathf.Pow(0.05f, Time.deltaTime));
        } 
        else {
            if (Cursor.lockState == CursorLockMode.Locked) {
                mY -= Input.GetAxis("Mouse Y") * 500 * Time.deltaTime;
                mY = Mathf.Clamp(mY, -80, 80);
                mX += Input.GetAxis("Mouse X") * 500 * Time.deltaTime;
                general.camera.transform.localEulerAngles = new Vector3(mY, mX, 0);

                player.transform.eulerAngles = new Vector3(0, general.camera.transform.eulerAngles.y, 0);
            }

            general.camera.transform.position = player.transform.position + new Vector3(0, 0.5f, 0);
            // https://docs.unity3d.com/ScriptReference/CharacterController.Move.html

            if (cc.isGrounded) playerVelocity.y = 0;

            if (Input.GetKey("w")) cc.Move(player.transform.forward * Time.deltaTime * playerSpeed);
            if (Input.GetKey("s")) cc.Move(-player.transform.forward * Time.deltaTime * playerSpeed);
            if (Input.GetKey("d")) cc.Move(player.transform.right * Time.deltaTime * playerSpeed);
            if (Input.GetKey("a")) cc.Move(-player.transform.right * Time.deltaTime * playerSpeed);

            if (Input.GetKeyDown("q")) Cursor.lockState = (Cursor.lockState == CursorLockMode.Locked) ? CursorLockMode.None : CursorLockMode.Locked;

            playerVelocity.y += gravityValue * Time.deltaTime;
            cc.Move(playerVelocity * Time.deltaTime);
            
            if (Input.GetKeyDown("e")) {
                inFirstPerson = false;

                player.transform.position = new Vector3(0, 10, 0);
                player.SetActive(false);

                Cursor.lockState = CursorLockMode.None;

                master.playerPosition = craterTerrainController.currentCrater.parent.rotateLocalGeo(craterTerrainController.currentCrater.geo, 10).swapAxis();

                foreach (var m in craterTerrainController.activeMeshes) {
                    m.AddComponent<MeshCollider>();
                    m.transform.localScale = new Vector3(250, 250, 250);
                    m.transform.localEulerAngles = new Vector3(0, 0, 0);
                }

                general.camera.transform.localPosition = new Vector3(0, 0, -5);
                general.camera.transform.localEulerAngles = new Vector3(0, 0, 0);
                general.camera.transform.RotateAround(Vector3.zero, general.camera.transform.right, -150);
                general.camera.fieldOfView = 60;
            }
        }
    }

    private CharacterController cc;
    private Vector3 playerVelocity;
    private float playerSpeed = 2.0f;
    private float gravityValue = -9.81f;
    private float mX = 0f;
    private float mY = 0f;
}
