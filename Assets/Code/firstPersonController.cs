using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class firstPersonController : MonoBehaviour
{
    public float speed=1000f;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    Vector3 position = new Vector3(0f, 0f, 0f);
    // Update is called once per frame
    void Update()
    {
        
        position += new Vector3(Input.GetAxisRaw("Vertical")/speed,0, (float)Input.GetAxisRaw("Horizontal")/speed);
        transform.localPosition = position;
        

        
    }
}
