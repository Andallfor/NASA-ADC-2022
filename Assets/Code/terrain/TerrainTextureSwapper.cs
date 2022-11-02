using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class TerrainTextureSwapper : MonoBehaviour
{
    private GameObject regionalView,labels,earth;
    private TMP_Text text;
    
    // Start is called before the first frame update
    public void handleTexture(int input)
    {
        terrain.terrainTextureState = input;
    }
    private void Awake()
    {
        text= GameObject.Find("Region").GetComponent<TMP_Text>();
        regionalView=GameObject.Find("RegionalView");
        labels = GameObject.Find("labels");
        earth = GameObject.Find("earth");
    }
    private void Update()
    {
        if (terrain.currentCrater != null)
        {
            text.text = terrain.currentCrater.name;
        }
        if (master.currentState == programStates.planetaryTerrain)
        {
            regionalView.active = true;
            labels.active = false;
            earth.active = false;
            
            
        }
        else
        {
            regionalView.active = false;
            labels.active = true;
            earth.active = true;
        }
    }
}
