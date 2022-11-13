using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

/// <summary> Represents a region of terrain. Requires associated terrain data. </summary>
public class crater {
    public readonly string name;
    public GameObject representation;
    public readonly geographic geo;
    public readonly planet parent;

    public TextMeshProUGUI label;
    public Button button;
    private RectTransform labelRt;
    public terrainFilesInfo terrainData;

    public crater(string name, geographic geo, planet parent, terrainFilesInfo terrainData) {
        this.name = name;
        this.geo = geo;
        this.parent = parent;
        this.terrainData = terrainData;

        this.representation = GameObject.Instantiate(general.craterPrefab, Vector3.zero, Quaternion.identity, parent.representation.transform);

        GameObject go = GameObject.Instantiate(general.buttonPrefab, Vector3.zero, Quaternion.identity, general.labelParent);
        button = go.GetComponent<Button>();
        label = go.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();
        labelRt = go.GetComponent<RectTransform>();
        label.text = name;
        label.color = Color.red;
        button.onClick.AddListener(select);
        go.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        go.GetComponent<Image>().enabled = false;
        labelRt.sizeDelta = new Vector2(label.preferredWidth + 5f, label.preferredHeight + 5f);

        update();
        updateScale();

        master.registeredCraters.Add(this);
    }

    public void update() {
        position p = geo.toCartesian(parent.information.radius + 5.0) / (2.0 * parent.information.radius);
        representation.transform.localPosition = (Vector3) p.swapAxis();

        RaycastHit hit;
        if (Physics.Raycast(general.camera.transform.position,
            representation.transform.position - general.camera.transform.position, out hit, 
            Vector3.Distance(representation.transform.position, general.camera.transform.position), 1 << 6)) {
            label.text = "";
        } else {
            label.text = name;
            general.drawTextOverObject(labelRt, label, representation.transform.position);
        }
    }

    public void updateScale() {
        representation.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f) / (float) master.scale;
    }

    public void select() {
        terrain.currentCrater = this;
        master.changeState(programStates.planetaryTerrain);
    }
}

/// <summary> Holds all relevant information regarding a region's terrain. </summary>
/// <remarks> Expects the terrain data to be in the format provided by Assets/Resources/regionalFileLocationsInfo.txt. </remarks>
public struct terrainFilesInfo {
    /// <summary> Name of the terrain folder. </summary>
    public string name;
    /// <summary> Holds the information regarding each "level" of terrain generated. (x=resolution, y=numSubMeshes). </summary>
    public List<Vector2Int> folderData;

    public terrainFilesInfo(string name, List<Vector2Int> folderData) {
        this.name = name;
        this.folderData = folderData;
    }
}