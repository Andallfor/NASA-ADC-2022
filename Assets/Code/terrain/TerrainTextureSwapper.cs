using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainTextureSwapper : MonoBehaviour
{
    // Start is called before the first frame update
    public void handleTexture(int input)
    {
        terrain.terrainTextureState = input;
    }
    
}
