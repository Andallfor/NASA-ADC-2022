using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tempCOde : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject camera,moon,earth;
    void Start()
    {
        moon = GameObject.Find("moon");
        earth = GameObject.Find("earth");
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(master.scale);
        //Debug.Log(Vector3.Distance(moon.transform.position, Vector3.zero));
        camera.transform.position = earth.transform.position; 
        camera.transform.LookAt(moon.transform);
    }
}
