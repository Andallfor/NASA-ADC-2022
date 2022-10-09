using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;

/// <summary> Class that holds general, non-backend, information that is needed by multiple scripts. </summary>
/// <remarks> See <see cref="master"/> for the backend version of this class. </summary>
public static class general {
    #region PREFABS
    /// <summary> Prefab of a body. </summary>
    public static GameObject bodyPrefab = Resources.Load("prefabs/body") as GameObject;
    public static GameObject labelPrefab = Resources.Load("prefabs/label") as GameObject;
    public static GameObject facilityPrefab = Resources.Load("prefabs/facility") as GameObject;
    #endregion

    #region MATERIALS
    public static Material defaultMat = Resources.Load("materials/default") as Material;
    public static Material earthMat = Resources.Load("materials/earth") as Material;
    public static Material moonMat = Resources.Load("materials/moon") as Material;
    #endregion

    #region OBJECTS IN SCENE
    /// <summary> Parent that holds all bodies. </summary>
    public static Transform bodyParent = GameObject.FindGameObjectWithTag("bodyParent").transform;
    public static Transform labelParent = GameObject.FindGameObjectWithTag("ui/labels").transform;
    public static Canvas canvas = GameObject.FindGameObjectWithTag("canvas").GetComponent<Canvas>();
    public static Camera camera = Camera.main;
    #endregion

    #region TERRAIN
    public static string[] regionalFileLocations = (Resources.Load("regionalFileLocations") as TextAsset).text.Split('\n');
    public static string regionalFileHostLocation = ((Resources.Load("regionalFileLocations") as TextAsset).text.Split('\n')).FirstOrDefault(x => x.Contains(Application.streamingAssetsPath.Split('/')[2]));
    public static string host = Application.streamingAssetsPath.Split('/')[2];
    #endregion

    #region STATIC METHODS
    public static void drawTextOverObject(TextMeshProUGUI text, Vector3 dest) {
        Vector3 p = getScreenPosition(dest);
        text.rectTransform.anchoredPosition = p;

        if (p.z < 0) text.enabled = false;
        else if (!text.enabled) text.enabled = true;
    }

    public static Vector3 getScreenPosition(Vector3 pos) {
        Vector3 screenSize = new Vector3(Screen.width, Screen.height, 0);
        Vector3 screenPos = general.camera.WorldToScreenPoint(pos) - (screenSize / 2f);
        
        screenPos /= canvas.scaleFactor;

        return screenPos;
    }

    public static float screenSize(MeshRenderer mr, Vector3 pos) {
        float diameter = mr.bounds.extents.magnitude;
        float distance = Vector3.Distance(pos, general.camera.transform.position);
        float angularSize = (diameter / distance) * Mathf.Rad2Deg;
        return ((angularSize * Screen.height) / general.camera.fieldOfView);
    }
    #endregion
}
