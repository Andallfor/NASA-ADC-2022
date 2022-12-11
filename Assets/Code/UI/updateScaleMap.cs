using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class updateScaleMap : MonoBehaviour {
    public static GameObject self;
    public void Awake() {self = this.gameObject; hide();}
    public static void update(Texture2D c, int map) {
        show();
        self.transform.GetChild(5).GetComponent<RawImage>().texture = c;

        string key = "azimuth";
        string suffix = "°";
        if (map == 0) {key = "height"; suffix = "m";}
        else if (map == 1) key = "slope";
        else if (map == 2) {key = "elevation"; suffix = "m";}

        double[] range = craterTerrainController.currentCrater.terrainData.bounds[key];
        for (int i = 0; i < 5; i++) {
            var tmp = self.transform.GetChild(i).GetComponent<TextMeshProUGUI>();

            tmp.text = Math.Round(range[0] + (range[1] - range[0]) * ((float) (5 - i - 1) / 5f), 1).ToString() + suffix + " - ";
        }

        self.transform.parent.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = key;
    }

    public static void update(Texture2D c) {
        show();
        self.transform.GetChild(5).GetComponent<RawImage>().texture = c;
    }

    public static void hide() {
        self.transform.parent.gameObject.SetActive(false);
    }

    public static void show() {
        self.transform.parent.gameObject.SetActive(true);
    }
}
