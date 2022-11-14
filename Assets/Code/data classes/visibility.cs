using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class visibility {

    private static double EPSILON = 0.0000001;

    public static bool rayIntersectsTriangle(position rayOrigin, position rayVector, position vertex0, position vertex1, position vertex2)
    {
        position edge1, edge2, h, s, q;
        double a, f, u, v;

        edge1 = vertex1 - vertex0;
        edge2 = vertex2 - vertex0;

        h = position.cross(rayVector, edge2);

        a = position.dot(edge1,h);
        if (a > -1 * EPSILON && a < EPSILON)
        {
            return false;    // This ray is parallel to this triangle.
        }
        f = 1.0 / a;
        s = (rayOrigin - vertex0);
        u = f * (position.dot(s,h));
        if (u < 0.0 || u > 1.0)
        {
            return false;
        }
        q = position.cross(s, edge1);
        v = f * position.dot(rayVector, q);
        if (v < 0.0 || u + v > 1.0)
        {
            return false;
        }
        // At this stage we can compute t to find out where the intersection point is on the line.
        double t = f * position.dot(edge2, q);
        if (t > EPSILON) // ray intersection
        {
            // to find where intersection
            //outIntersectionPoint = new position(0.0, 0.0, 0.0);
            //outIntersectionPoint.scaleAdd(t, rayVector, rayOrigin);
            return true;
        }
        else // This means that there is a line intersection but not a ray intersection.
        {
            return false;
        }
    }
}
