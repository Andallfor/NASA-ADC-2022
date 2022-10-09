using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class facility {
    public readonly string name;
    public GameObject representation;
    public readonly geographic geo;
    public readonly planet parent;

    public TextMeshProUGUI label;

    public facility(string name, geographic geo, planet parent) {
        this.name = name;
        this.geo = geo;
        this.parent = parent;

        this.representation = GameObject.Instantiate(general.facilityPrefab, Vector3.zero, Quaternion.identity, parent.representation.transform);
        //this.representation.GetComponent<MeshRenderer>().enabled = false;

        GameObject go = GameObject.Instantiate(general.labelPrefab, Vector3.zero, Quaternion.identity, general.labelParent);
        label = go.GetComponent<TextMeshProUGUI>();
        label.color = Color.red;
        label.text = name;

        update();
        updateScale();
    }

    public void update() {
        position p = geo.toCartesian(parent.information.radius + 5) / (2 * parent.information.radius);
        p.swapAxis();
        representation.transform.localPosition = (Vector3) p;

        RaycastHit hit;
        if (Physics.Raycast(general.camera.transform.position,
            representation.transform.position - general.camera.transform.position, out hit, 
            Vector3.Distance(representation.transform.position, general.camera.transform.position), 1 << 6)) {
            label.text = "";
        } else {
            label.text = name;
            general.drawTextOverObject(label, representation.transform.position);
        }
    }

    public void updateScale() {
        representation.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f) / (float) master.scale;
    }

    public void indicateSelection() {
        label.color = Color.green;
    }

    public void select() {

    }

    public void indicateDeselection() {
        label.color = Color.red;
    }
}
