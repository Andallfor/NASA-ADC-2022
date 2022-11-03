using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lighting : MonoBehaviour
{
    GameObject sun;
    GameObject light,haloLight,directionalLight;
    Light lightComp;

    public float K= .00001f;
    public void handleLighting(int inp)
    {
        if (light.active == true)
        {
            light.active = false;
            haloLight.active = false;
            directionalLight.active = true;
        }
        else
        {
            directionalLight.active = false;
            light.active = true;
            haloLight.active = true;
        }
    }
    // Start is called before the first frame update
    void Start()
    {

        directionalLight = GameObject.Find("Directional-Light");
        directionalLight.SetActive(false);
        sun = GameObject.Find("sun");
        light = new GameObject("light");
        haloLight = GameObject.Find("HaloLight");
        
        lightComp= light.AddComponent<Light>();
        lightComp.type = LightType.Point;
        
        lightComp.intensity = K * Vector3.Distance(sun.transform.position, Vector3.zero);
        lightComp.intensity = 10;
        
    }

    // Update is called once per frame
    void Update()
    {


        lightComp.intensity = K * Vector3.Distance(sun.transform.position, Vector3.zero);
        if (master.currentState == programStates.planetaryTerrain)
        {
            //*Leo sighs and removes Liam's push priviledges*
            light.transform.position = sun.transform.position.normalized * 1000* 1/(int)master.scale*1000;
            lightComp.range = Vector3.Distance(sun.transform.position.normalized * 1000*1/(int)master.scale*1000, Vector3.zero) + 5000;

            haloLight.transform.position = sun.transform.position.normalized * 1000* 1/(int)master.scale*1000;
        }
        if (master.currentState == programStates.interplanetary)
        {
            light.transform.position = sun.transform.position.normalized * 1000*1000/(int)master.scale;
            lightComp.range = Vector3.Distance(sun.transform.position.normalized * 1000/ (int)master.scale, Vector3.zero) + 5000;
            haloLight.transform.position = sun.transform.position.normalized * 1000/ (int)master.scale;
        }

            
    }
}
