using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sunLight : MonoBehaviour
{
    public GameObject sun, flare;
    private const double distRatio = 0.00465040106952;

    public void Update() {
        if (!master.initialized) return;

        position pos = (master.sun.worldPos - master.referenceFrame - master.playerPosition) / master.scale;
        position spos = pos * ((100.0 + master.scale / 10.0) / pos.magnitude());
        double relativeSize = spos.magnitude() * (2.0 * distRatio);
        position fpos = pos * ((100.0 + master.scale / 10.0 - relativeSize * 2.0) / pos.magnitude());

        sun.transform.position = (Vector3) (spos.swapAxis());
        sun.transform.localScale = new Vector3(
            (float) (relativeSize * 2.0),
            (float) (relativeSize * 2.0),
            (float) (relativeSize * 2.0));
        flare.transform.position = (Vector3) (fpos.swapAxis());
        flare.transform.LookAt(Vector3.zero);
    }
}
