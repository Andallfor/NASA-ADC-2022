using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class TerrainTextureSwapper : MonoBehaviour
{
    private GameObject label;
    private TMP_Text text;
    // Start is called before the first frame update
    public void handleTexture(int input)
    {
        terrain.terrainTextureState = input;
    }
    private void Awake()
    {
        text= GameObject.Find("Region").GetComponent<TMP_Text>();
        label=GameObject.Find("RegionalView");
    }
    private void Update()
    {
        if (terrain.currentCrater != null)
        {
            text.text = terrain.currentCrater.name;
        }
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
