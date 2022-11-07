using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lighting : MonoBehaviour
{
    GameObject sun;
    GameObject centralLight, haloLight, directionalLight;
    Light lightComp;

    public float K = .00001f;
    public void handleLighting(int inp) {
        if (centralLight.activeSelf) {
            centralLight.SetActive(false);
            haloLight.SetActive(false);
            directionalLight.SetActive(true);
        } else {
            directionalLight.SetActive(false);
            centralLight.SetActive(true);
            haloLight.SetActive(true);
        }
    }
    // Start is called before the first frame update
    void Start()
    {

        directionalLight = GameObject.Find("Directional-Light");
        directionalLight.SetActive(false);
        sun = GameObject.Find("sun");
        centralLight = new GameObject("light"); // <- bruh
        haloLight = GameObject.Find("HaloLight");
        
        lightComp = centralLight.AddComponent<Light>();
        lightComp.type = LightType.Point;
        
        lightComp.intensity = K * Vector3.Distance(sun.transform.position, Vector3.zero);
        lightComp.intensity = 10;
        
    }

    // Update is called once per frame
    void Update()
    {
        lightComp.intensity = K * Vector3.Distance(sun.transform.position, Vector3.zero);
        if (master.currentState == programStates.planetaryTerrain) {
            //*Leo sighs and removes Liam's push priviledges*
            // yes
            centralLight.transform.position = sun.transform.position.normalized * 10000* 1/(int)master.scale*1000;
            lightComp.range = Vector3.Distance(sun.transform.position.normalized * 10000*1/(int)master.scale*1000, Vector3.zero) + 5000;

            haloLight.transform.position = sun.transform.position.normalized * 10000* 1/(int)master.scale*1000;
        }
        if (master.currentState == programStates.interplanetary)
        {
            centralLight.transform.position = sun.transform.position.normalized * 10000*1000/(int)master.scale;
            lightComp.range =Vector3.Distance(sun.transform.position.normalized * 1000*10000/(int)master.scale,Vector3.zero) + 5000;
            
            haloLight.transform.position = sun.transform.position.normalized * 1000*10000/ (int)master.scale;
        }

            
    }
}
