using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainTextureSwapper : MonoBehaviour
{
    GameObject label; 
    // Start is called before the first frame update
    public void handleTexture(int input)
    {
        terrain.terrainTextureState = input;
    }
    private void Awake()
    {
        label=GameObject.Find("ViewType");
    }
    private void Update()
    {
        if (master.currentState == programStates.planetaryTerrain)
        {
            label.active = true;
        }
        else
        {
            label.active = false;
        }
    }
}
