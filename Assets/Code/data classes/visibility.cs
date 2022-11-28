using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class visibility : MonoBehaviour {
    private static Queue<visResponse> queue = new Queue<visResponse>();
    public static async Task<bool> getVisibility(int meshIndex, position worldPos, Vector3 heightOffset, int activeMeshNum = 0, bool drawDebug = false) {
        GameObject go = craterTerrainController.activeMeshes[activeMeshNum];

        if (go.GetComponent<MeshCollider>() == null) go.AddComponent<MeshCollider>();

        Vector3 v = go.GetComponent<MeshFilter>().sharedMesh.vertices[meshIndex];
        v = new Vector3(
            v.x * go.transform.lossyScale.x,
            v.y * go.transform.lossyScale.y,
            v.z * go.transform.lossyScale.z)
            + heightOffset;

        // adjust worldPos
        position to = ((worldPos - master.referenceFrame - master.playerPosition) / master.scale).swapAxis();
        
        visResponse response = new visResponse();
        response.start = v;
        response.dir = (Vector3) worldPos - v;
        response.finished = false;

        queue.Enqueue(response);

        while (!response.finished) await Task.Delay(32); // check every other frame

        if (drawDebug) Debug.DrawLine(v, (Vector3) worldPos - v, response.hit ? Color.red : Color.green, 10);

        return response.hit;
    }

    private void FixedUpdate() {
        for (int i = 0; i < queue.Count; i++) {
            visResponse v = queue.Dequeue();
            v.hit = Physics.Raycast(v.start, v.dir, float.MaxValue, 1 << 7);
            v.finished = true;
        }
    }
}

internal class visResponse {
    public Vector3 start, dir;
    public bool hit, finished;
}
