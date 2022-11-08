using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class earthView : MonoBehaviour
{
    GameObject camera,earth,moon;
    // Start is called before the first frame update
    void Start()
    {
        camera = GameObject.Find("Camera");
        earth = GameObject.Find("earth");
        moon= GameObject.Find("moon");
    }

    // Update is called once per frame
    void Update()
    {
        camera.transform.position = earth.transform.position;
        camera.transform.LookAt(moon.transform.position);
    }
}
