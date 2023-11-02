using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class minimapController : MonoBehaviour
{
    public GameObject minimapCam,image;
    // Start is called before the first frame update
    void Start()
    {
        general.minimapCam = minimapCam;
        minimapCam.active = false;
    }

    // Update is called once per frame
    void Update()
    {
        
        minimapCam.active = master.currentState == programStates.planetaryTerrain;
        image.active = master.currentState == programStates.planetaryTerrain;
        if (minimapCam.active)
        {
            minimapCam.GetComponent<Camera>().orthographicSize = .5f * craterTerrainController.worldSize.x;
        }
    }
}
 