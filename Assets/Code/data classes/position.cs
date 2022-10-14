using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary> Replacement for Vector3. Generally the z axis here will correspond to the y axis in Unity (and vice versa). </summary>
/// <remarks> Unless otherwise specified, functions will modify the host (rather then return a new position).
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
    /// <returns> Both modifies itself and also returns itself. </summary>
    public position swapAxis() {
        double a = y;
        y = z;
        z = a;

        return this;
    }

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
    
    /// <summary> Force the vector to have a length of 1. </summary>
    public void normalize() {
        double l = magnitude();
        x /= l;
        y /= l;
        z /= l;
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
