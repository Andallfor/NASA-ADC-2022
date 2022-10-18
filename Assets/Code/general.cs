using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;

/// <summary> Class that holds general, non-backend, information that is needed by multiple scripts. </summary>
/// <remarks> See <see cref="master"/> for the backend version of this class. </summary>
public static class general {
    #region PREFABS
    public static GameObject bodyPrefab = Resources.Load("prefabs/body") as GameObject;
    public static GameObject labelPrefab = Resources.Load("prefabs/label") as GameObject;
    public static GameObject craterPrefab = Resources.Load("prefabs/crater") as GameObject;
    public static GameObject buttonPrefab = Resources.Load("prefabs/button") as GameObject;
    #endregion

    #region MATERIALS
    public static Material defaultMat = Resources.Load("materials/default") as Material;
    public static Material earthMat = Resources.Load("materials/earth") as Material;
    public static Material moonMat = Resources.Load("materials/moon") as Material;
    #endregion

    #region OBJECTS IN SCENE
    /// <summary> Parent that holds all bodies. </summary>
    public static Transform bodyParent = GameObject.FindGameObjectWithTag("bodyParent").transform;
    /// <summary> Parent that holds all labels. </summary>
    public static Transform labelParent = GameObject.FindGameObjectWithTag("ui/labels").transform;
    /// <summary> The UI canvas in the program. </summary>
    public static Canvas canvas = GameObject.FindGameObjectWithTag("canvas").GetComponent<Canvas>();
    /// <summary> The main camera in the scene. Use this instead of Camera.main, as it is much more efficient. </summary>
    public static Camera camera = Camera.main;
    #endregion

    #region TERRAIN
    public static string[] regionalFileLocations = (Resources.Load("regionalFileLocations") as TextAsset).text.Split('\n');
    public static string regionalFileHostLocation = ((Resources.Load("regionalFileLocations") as TextAsset).text.Split('\n')).FirstOrDefault(x => x.Contains(Application.streamingAssetsPath.Split('/')[2]));
    public static string host = Application.streamingAssetsPath.Split('/')[2];
    #endregion

    #region STATIC METHODS
    /// <summary> Center a piece of text over an object in 3D space. </summary>
    public static void drawTextOverObject(RectTransform rt, TextMeshProUGUI text, Vector3 dest) {
        Vector3 p = getScreenPosition(dest);
        rt.anchoredPosition = p;

        if (p.z < 0) text.enabled = false;
        else if (!text.enabled) text.enabled = true;
    }

    /// <summary> Get the screen position of an object in 3D space. </summary>
    public static Vector3 getScreenPosition(Vector3 pos) {
        Vector3 screenSize = new Vector3(Screen.width, Screen.height, 0);
        Vector3 screenPos = general.camera.WorldToScreenPoint(pos) - (screenSize / 2f);
        
        screenPos /= canvas.scaleFactor;

        return screenPos;
    }

    /// <summary> Get a rough estimate of the pixel size of an object in 3D space on the hosts screen. </summary>
    public static float screenSize(MeshRenderer mr, Vector3 pos) {
        float diameter = mr.bounds.extents.magnitude;
        float distance = Vector3.Distance(pos, general.camera.transform.position);
        float angularSize = (diameter / distance) * Mathf.Rad2Deg;
        return ((angularSize * Screen.height) / general.camera.fieldOfView);
    }
    #endregion
}
