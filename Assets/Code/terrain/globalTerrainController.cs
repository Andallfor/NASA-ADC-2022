using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class globalTerrainController {
    private planet parent;
    private GameObject movementDetector;
    private Vector3 lastDetectorPos = Vector3.zero;
    private HashSet<geographic> currentDesiredMeshes = new HashSet<geographic>();
    private Dictionary<geographic, GameObject> aliveMeshes = new Dictionary<geographic, GameObject>();
    private List<Vector2> directions = new List<Vector2>() {
        new Vector2(-1, 1),  new Vector2(0, 1),  new Vector2(1, 1),
        new Vector2(-1, 0),                      new Vector2(1, 0),
        new Vector2(-1, -1), new Vector2(0, -1), new Vector2(1, -1)};

    public globalTerrainController(planet parent) {
        this.parent = parent;
        master.onStateChange += onStateChange;

        movementDetector = GameObject.Instantiate(general.defaultPrefab, parent.representation.gameObject.transform);
        movementDetector.transform.position = Vector3.one;
        movementDetector.GetComponent<MeshRenderer>().enabled = false;
    }

    public void onStateChange(object s, stateChangeEvent e) {
        if (e.newState == programStates.interplanetary) {
            // init
            master.onUpdateEnd += update;
        } else if (e.previousState == programStates.interplanetary) {
            // cleanup
            master.onUpdateEnd -= update;
        }
    }

    public void update(object s, EventArgs e) {
        if (master.referenceFrameBody.name != parent.name) return;
        Vector3 detectorPos = general.camera.WorldToScreenPoint(movementDetector.transform.position);
        if (Vector3.Distance(detectorPos, lastDetectorPos) < 0.05f) return;
        lastDetectorPos = detectorPos;

        HashSet<geographic> target = findDesiredMeshes();

        HashSet<geographic> current = new HashSet<geographic>(currentDesiredMeshes); // toKill
        HashSet<geographic> desired = new HashSet<geographic>(target); // toGen
        HashSet<geographic> ignore = new HashSet<geographic>();
        ignore = new HashSet<geographic>(current.Intersect(desired).ToList());
        current.SymmetricExceptWith(ignore);
        desired.SymmetricExceptWith(ignore);

        currentDesiredMeshes = target;

        // kill
        foreach (geographic g in current) {
            GameObject.Destroy(aliveMeshes[g]);
            aliveMeshes.Remove(g);
        }
        
        // create new meshes
        foreach (geographic g in desired) {
            decompTerrainData d = globalMeshGenerator.requestGlobalTerrain(
                parent.name,
                new Vector2Int((int) g.lon, (int) g.lat),
                new Vector2Int(0, 0),
                new Vector2Int(2000, 2000),
                3, 3, true);

            GameObject go = globalMeshGenerator.generateDecompData(d);
            aliveMeshes[g] = go;
        }
    }

    private HashSet<geographic> findDesiredMeshes() {
        /*
        Assumptions:
        1. planet is always at (0, 0, 0)
        
        Steps:
        1. draw a line from the center of the planet to the camera
        2. find the intersection of this line with the planet itself
        3. get the tile that contains said the point of intersection
        4. begin a flood fill algorithm
            a. Queue of starting positions (tiles directly next to intersected tile)
            b. check the four corners, if any of them are on screen, accept them
                -> lineSphereIntersection. If the corner is the point closer to the screen, then it is on screen
                -> also check if the box that they make intersects the camera rendering box
        */

        double scaledRadius = parent.information.radius / master.scale;

        position[] intersections = position.lineSphereIntersection(
            Vector3.zero, // see assumption 1
            general.camera.transform.position,
            Vector3.zero, // see assumption 1
            scaledRadius);
        
        // get the point closest to the camera, the other point would be obscured by the planet itself
        position intersection = intersections.OrderBy(x => x.distanceTo(general.camera.transform.position)).First();
        // convert the point into a geographic value
        geographic intersectionGeo = parent.localPosToLocalGeo(intersection);
        // get the tile that this geo would be contained in
        // TODO: dynamic resolution
        // currently assumes that size is 60 x 60
        geographic step = new geographic(60, 60);
        geographic start = new geographic(
            intersectionGeo.lat - intersectionGeo.lat % step.lat + 30, // bc it is not centered on 0 but +30 (due to step size)
            intersectionGeo.lon - intersectionGeo.lon % step.lon);

        // start flood fill alg
        Queue<geographic> frontier = new Queue<geographic>(getNearbyTiles(start, step));
        HashSet<geographic> visited = new HashSet<geographic>() {start};
        HashSet<geographic> visible = new HashSet<geographic>();
        while (frontier.Count != 0) {
            // check and replace all of toCheck at once rather than individually
            // TODO: use this to maybe (?) determine dynamically resolutions later
            //      allows one to say a tile is "more visible" depending on iteration
            HashSet<geographic> nextFrontier = new HashSet<geographic>();
            while (frontier.Count != 0) {
                // check if they are even possible to see (not on the other side of the planet)
                geographic ll = frontier.Dequeue();
                visited.Add(ll);
                geographic[] corners = new geographic[4] { // ll, tl, tr, lr
                    ll, ll + new geographic(step.lat, 0),
                    ll + new geographic(step.lat, step.lon), ll + new geographic(0, step.lon)};

                bool anyVisible = false;
                for (int i = 0; i < 4; i++) {
                    Vector3 v = parent.localGeoToUnityPos(corners[i], 0);
                    position[] ints = position.lineSphereIntersection(v, general.camera.transform.position, Vector3.zero, scaledRadius);
                    
                    if (ints.Length == 1 || ints.Length == 0) {
                        // tangent line, must be visible
                        // idk what happens what the length is 0
                        anyVisible = true;
                        break;
                    }

                    // check to see which point v is closer to
                    // 1st is the point closest to the camera
                    position[] sorted = ints.OrderBy(x => x.distanceTo(general.camera.transform.position)).ToArray();
                    if (position.distance(sorted[0], v) < position.distance(sorted[1], v)) {
                        anyVisible = true;
                        break;
                    }

                    // TODO: check if box formed by corners overlaps camera
                }

                // update queue
                if (anyVisible) {
                    HashSet<geographic> proposed = getNearbyTiles(ll, step);
                    proposed.ExceptWith(visited);
                    nextFrontier.Concat(proposed);

                    visible.Add(ll);
                }
            }

            frontier.Concat(nextFrontier);
        }

        return visible;
    }

    private HashSet<geographic> getNearbyTiles(geographic start, geographic step) {
        HashSet<geographic> output = new HashSet<geographic>();

        foreach (Vector2 dir in directions) {
            // geographic automatically wraps coordinates and + operator returns a new geographic (rather than modifying values)
            output.Add(start + new geographic(dir.x * step.lat, dir.y * step.lon));
        }

        return output;
    }

    private static Vector2 vec3To2(Vector3 v) => new Vector2(v.x, v.y);
}

[Flags] internal enum corner : int {
    ll = 0, tl = 1, tr = 2, lr = 3
}
