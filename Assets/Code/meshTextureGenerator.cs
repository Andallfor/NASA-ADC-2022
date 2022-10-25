using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public static class meshTextureGenerator {

    /// <summary> Generates a height map of a mesh in png format </summary>
    public static Color[] generateHeightMap(Mesh m,  int width, int height) {


        Vector3[] vertices;
        vertices = m.vertices;

        float maxVertice = -10000;
        float minVertice = 10000;
        
        // Finds the min and max vertice
        for (int i = 0; i < vertices.Length; i++) {
            float vert = vertices [i].y;
            if (vert > maxVertice) {
                maxVertice = vert;
            }

            if (vert < minVertice){
                minVertice = vert;
            } 
        }

        // converts each value to a scale of 0-1 using minVertice as 0, and maxVertice as 1
        // each value is then added to a list named 'allVertPerc'
        float range = maxVertice - minVertice;
        List<float> allVertPerc = new List<float>();

        for (int i = 0; i < vertices.Length; i++){
            float vert = vertices [i].y;
            float vertPerc = (vert-minVertice)/range;
            allVertPerc.Add(vertPerc);
        } 
       
        // creates new Gradient aptly named gradient
        Gradient gradient= new Gradient();
       
        // Populate the color keys at the relative time 0 and 1 (0 and 100%) 
        GradientColorKey[] colorKey = new GradientColorKey[6];
        colorKey[0].color = Color.red;
        colorKey[0].time = 0.0f;
        colorKey[1].color = Color.yellow;
        colorKey[1].time = 0.2f;
        colorKey[2].color = Color.white;
        colorKey[2].time = 0.4f;
        colorKey[3].color = Color.green;
        colorKey[3].time = 0.6f;
        colorKey[4].color = Color.cyan;
        colorKey[4].time = 0.8f;
        colorKey[5].color = Color.black;
        colorKey[5].time = 1.0f;

        // Populate the alpha  keys at relative time 0 and 1  (0 and 100%)
        GradientAlphaKey[] alphaKey = new GradientAlphaKey[6];
        alphaKey[0].alpha = 1.0f;
        alphaKey[0].time = 0.0f;
        alphaKey[1].alpha = 1.0f;
        alphaKey[1].time = 0.4f;
        alphaKey[2].alpha = 1.0f;
        alphaKey[2].time = 1.0f;
        alphaKey[3].alpha = 1.0f;
        alphaKey[3].time = 0.6f;
        alphaKey[4].alpha = 1.0f;
        alphaKey[4].time = 0.8f;
        alphaKey[5].alpha = 1.0f;
        alphaKey[5].time = 1.0f;


        gradient.SetKeys(colorKey, alphaKey);
       
       // makes new color array with a size equal to the list of vertices
        Color[] gradientArray = new Color[allVertPerc.Count];
        Debug.Log(allVertPerc.Count);

       // Gets color value for each vertice
       for (int i = 0; i < allVertPerc.Count; i++){

            gradientArray[i] = gradient.Evaluate(allVertPerc[i]);
        }
        return gradientArray;
          
    }
    
}