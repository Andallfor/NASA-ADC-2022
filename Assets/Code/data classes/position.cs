using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary> Replacement for Vector3. Generally the z axis here will correspond to the y axis in Unity (and vice versa). </summary>
/// <para> Related classes: <see cref="geographic"/> </para> </remarks>
public struct position {
    #region VARIABLES
    public double x, z, y;
    #endregion

    #region CONSTRUCTORS
    public position(double x, double y, double z) {
        this.x = x;
        this.y = y;
        this.z = z;
    }
    #endregion

    #region INSTANCE METHODS
    /// <summary> Swaps the z and y axis. </summary>
    public position swapAxis() => new position(x, z, y);

    // https://stackoverflow.com/questions/34050929/3d-point-rotation-algorithm
    /// <summary> Rotate a point by pitch, roll, yaw (radians). </summary>
    /// <remarks> Note that rotating (pi, pi/4, 0) is different from rotating it by (pi, 0, 0) then (0, pi/4, 0). </remarks>
    public void rotate(double pitch, double roll, double yaw)
    {
        double Axx = Math.Cos(yaw) * Math.Cos(pitch);
        double Axy = Math.Cos(yaw) * Math.Sin(pitch) * Math.Sin(roll) - Math.Sin(yaw) * Math.Cos(roll);
        double Axz = Math.Cos(yaw) * Math.Sin(pitch) * Math.Cos(roll) + Math.Sin(yaw) * Math.Sin(roll);

        double Ayx = Math.Sin(yaw) * Math.Cos(pitch);
        double Ayy = Math.Sin(yaw) * Math.Sin(pitch) * Math.Sin(roll) + Math.Cos(yaw) * Math.Cos(roll);
        double Ayz = Math.Sin(yaw) * Math.Sin(pitch) * Math.Cos(roll) - Math.Cos(yaw) * Math.Sin(roll);

        double Azx = -Math.Sin(pitch);
        double Azy = Math.Cos(pitch) * Math.Sin(roll);
        double Azz = Math.Cos(pitch) * Math.Cos(roll);


        double _x = Axx * x + Axy * y + Axz * z;
        double _y = Ayx * x + Ayy * y + Ayz * z;
        double _z = Azx * x + Azy * y + Azz * z;

        this.x = _x;
        this.y = _y;
        this.z = _z;
    }

    /// <summary> Return the Euclidean distance from this point to another. </summary>
    public double distanceTo(position p) => Math.Sqrt(
        Math.Pow(x - p.x, 2) +
        Math.Pow(y - p.y, 2) +
        Math.Pow(z - p.z, 2));    

    /// <summary> Normalize vector to have a length of 1. </summary>
    public position normalize() {
        double l = magnitude();
        return new position(x / l, y / l, z / l);
    }

    /// <summary> Get the magnitude of the vector. <summary>
    public double magnitude() => Math.Sqrt(x * x + y * y + z * z);
    #endregion

    #region STATIC METHODS

    /// <summary> Interpolate between two points. </summary>
    /// <returns> Returns a new position, does not modify the original. </returns>
    /// <param name="t"> Percentage to interpolate. Between 0 and 1. </param>
    public static position interpLinear(position p1, position p2, double t) => new position(
        p1.x + ((p2.x - p1.x) * t),
        p1.y + ((p2.y - p1.y) * t),
        p1.z + ((p2.z - p1.z) * t));
    
    public static position cross(position v1, position v2) => new position(
        v1.y * v2.z - v1.y * v2.x,
        v1.z * v2.x - v1.x * v2.z,
        v1.x * v2.y - v1.y * v2.x);

    public static double dot(position v1, position v2) => v1.x * v2.x + v1.y * v2.y + v1.z * v2.z;

    // https://stackoverflow.com/questions/5883169/intersection-between-a-line-and-a-sphere
    public static position[] lineSphereIntersection(position linePoint1, position linePoint2, position circleCenter, double radius) {
        double cx = circleCenter.x;
        double cy = circleCenter.y;
        double cz = circleCenter.z;

        double px = linePoint1.x;
        double py = linePoint1.y;
        double pz = linePoint1.z;

        double vx = linePoint2.x - px;
        double vy = linePoint2.y - py;
        double vz = linePoint2.z - pz;

        double A = vx * vx + vy * vy + vz * vz;
        double B = 2.0 * (px * vx + py * vy + pz * vz - vx * cx - vy * cy - vz * cz);
        double C = px * px - 2 * px * cx + cx * cx + py * py - 2 * py * cy + cy * cy +
                   pz * pz - 2 * pz * cz + cz * cz - radius * radius;

        // discriminant
        double D = B * B - 4 * A * C;

        if ( D < 0 ) return new position[0];

        double t1 = ( -B - Math.Sqrt ( D ) ) / ( 2.0 * A );

        position solution1 = new position(linePoint1.x * ( 1 - t1 ) + t1 * linePoint2.x,
                                          linePoint1.y * ( 1 - t1 ) + t1 * linePoint2.y,
                                          linePoint1.z * ( 1 - t1 ) + t1 * linePoint2.z );
        if ( D == 0 ) return new position[1] {solution1};

        double t2 = ( -B + Math.Sqrt( D ) ) / ( 2.0 * A );
        position solution2 = new position(linePoint1.x * ( 1 - t2 ) + t2 * linePoint2.x,
                                          linePoint1.y * ( 1 - t2 ) + t2 * linePoint2.y,
                                          linePoint1.z * ( 1 - t2 ) + t2 * linePoint2.z );

        // prefer a solution that's on the line segment itself

        if ( Math.Abs( t1 - 0.5 ) < Math.Abs( t2 - 0.5 ) ) return new position[2] {solution1, solution2};
        return new position[2] {solution2, solution1};
    }

    public static double distance(position p1, position p2) => Math.Sqrt(
        Math.Pow(p1.x - p2.x, 2) +
        Math.Pow(p1.y - p2.y, 2) +
        Math.Pow(p1.z - p2.z, 2));
    #endregion

    #region OVERRIDE/OPERATORS
    public override string ToString() => $"xyz: {x}, {y}, {z}";
    public static implicit operator position(Vector3 v) => new position(v.x, v.y, v.z);
    public static explicit operator Vector3(position p) => new Vector3((float) p.x, (float) p.y, (float) p.z);
    public static position operator+(position p1, position p2) => new position(
        p1.x + p2.x,
        p1.y + p2.y,
        p1.z + p2.z);
    public static position operator-(position p1, position p2) => new position(
        p1.x - p2.x,
        p1.y - p2.y,
        p1.z - p2.z);
    public static position operator*(position p1, double d) => new position(
        p1.x * d,
        p1.y * d,
        p1.z * d);
    public static position operator*(double d, position p1) => new position(
        p1.x * d,
        p1.y * d,
        p1.z * d);
    public static position operator/(position p1, double d) => new position(
        p1.x / d,
        p1.y / d,
        p1.z / d);
    public static position operator/(double d, position p1) => new position(
        d / p1.x,
        d / p1.y,
        d / p1.z);
    public static bool operator==(position p1, position p2) => (p1.x == p2.x && p1.y == p2.y && p1.z == p2.z);
    public static bool operator!=(position p1, position p2) => (p1.x != p2.x || p1.y != p2.y || p1.z != p2.z);

    public override bool Equals(object obj)
    {
        if (!(obj is position)) return false;
        position p = (position) obj;
        return this == p;
    }
    public override int GetHashCode()
    {
        unchecked
        {
            int hash = (int) 2166136261;
            hash = (hash * 16777619) ^ x.GetHashCode();
            hash = (hash * 16777619) ^ y.GetHashCode();
            hash = (hash * 16777619) ^ z.GetHashCode();

            return hash;
        }
    }
    #endregion
}
