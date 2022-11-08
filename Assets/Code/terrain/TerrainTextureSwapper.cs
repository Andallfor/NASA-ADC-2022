using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class TerrainTextureSwapper : MonoBehaviour
{
    private GameObject regionalView,labels,earth;
    private TMP_Text text,date;
    
    
    // Start is called before the first frame update
    public void handleTexture(int input)
    {
        terrain.terrainTextureState = input;
    }
    
    private void Start()
    {
        //date= GameObject.Find("date").GetComponent<TMP_Text>();
        text = GameObject.Find("Region").GetComponent<TMP_Text>();
        regionalView = GameObject.Find("RegionalView");
        labels = GameObject.Find("labels");
        earth = GameObject.Find("earth");
    }
    private void Update()
    {
        //date.text = master.getCurrentTime().ToString();
        
        if (terrain.currentCrater != null)
        {
            text.text = terrain.currentCrater.name;

        }
        if (master.currentState == programStates.planetaryTerrain)
        {
            regionalView.SetActive(true);
            labels.SetActive(false);
            earth.SetActive(false);
        }
        else
        {
            regionalView.SetActive(false);
            labels.SetActive(true);
            earth.SetActive(true);
        }
    }
}
