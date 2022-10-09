using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary> Represents latitude (-90 to 90) and longitude (-180 to 180). </summary>
/// <remarks> Unless otherwise specified, functions will modify the host (rather then return a new position). 
/// <para> Given in (lat, lon) format, NOT (lon, lat). </para> </remarks>
public struct geographic {
    #region VARIABLES
    public double lat, lon;
    #endregion

    #region CONSTRUCTORS
    /// <summary> Takes lat lon values and clamps them to -90 to 90 and -180 to 180 respectively. </summary>
    /// <remarks> Note that 0-180 and 0-360 are very different than the current -90 to 90/-180 to 180 system- the conversion is not just a +range/2. </remarks>
    public geographic(double lat, double lon) {
        this.lat = Math.Min(Math.Max(-90, lat), 90);
        this.lon = Math.Min(Math.Max(-180, lon), 180);
    }
    #endregion

    #region INSTANCE METHODS
    /// <summary> Converts lat, lon into a cartesian point centered on (0, 0) with length radius </summary>
    public position toCartesian(double radius) {
        this.lat = Math.Min(Math.Max(-90, lat), 90);
        this.lon = Math.Min(Math.Max(-180, lon), 180);

        double lt = this.lat * (Math.PI / 180.0);
        double ln = this.lon * (Math.PI / 180.0);

        return new position(
            radius * Math.Cos(lt) * Math.Cos(ln),
            radius * Math.Cos(lt) * Math.Sin(ln),
            radius * Math.Sin(lt));
    }

    /// <summary> Gets the distance between two geographic points on a sphere. </summary>
    public double geographicDistanceTo(geographic g, double radius) {
        double lt1 = this.lat * (Math.PI / 180.0);
        double ln1 = this.lon * (Math.PI / 180.0);
        double lt2 = g.lat * (Math.PI / 180.0);
        double ln2 = g.lon * (Math.PI / 180.0);

        // haversine formula
        return 2.0 * radius * Math.Asin(Math.Sqrt(
            (Math.Sin((lt2 - lt1) / 2.0) * Math.Sin((lt2 - lt1) / 2.0)) +
            Math.Cos(lt1) * Math.Cos(lt2) * 
            (Math.Sin((ln2 - ln1) / 2.0) * Math.Sin((ln2 - ln1) / 2.0))));
    }

    /// <summary> Gets the euclidean distance to another geographic point. </summary>
    public double euclideanDistanceTo(geographic g) => Math.Sqrt((g.lat - this.lat) * (g.lat - this.lat) + (g.lon - this.lon) * (g.lon - this.lon));

    /// <summary> Treat this point as a 2D vector and get the magnitude of it. </summary>
    public double magnitude() => Math.Sqrt(lat * lat + lon * lon);
    #endregion

    #region STATIC METHODS
    /// <summary> Takes a point centered on (0, 0) with unknown length, and converts it into geo </summary>
    public static geographic toGeographic(position point, double radius) {
        // draw point onto planet
        double dist = new position(0, 0, 0).distanceTo(point);
        double div = radius / dist;

        position p = new position(
            point.x * div,
            point.y * div,
            point.z * div);

        return new geographic(
            Math.Asin(p.y / radius) * (180.0 / Math.PI),
            Math.Atan2(p.z, p.x) * (180.0 / Math.PI));
    }

    public static position toCartesian(double lat, double lon, double radius) {
        lat = Math.Min(Math.Max(-90, lat), 90);
        lon = Math.Min(Math.Max(-180, lon), 180);

        double lt = lat * (Math.PI / 180.0);
        double ln = lon * (Math.PI / 180.0);

        return new position(
            radius * Math.Cos(lt) * Math.Cos(ln),
            radius * Math.Cos(lt) * Math.Sin(ln),
            radius * Math.Sin(lt));
    }
    #endregion

    #region OVERRIDES/OPERATORS
    public static geographic operator+(geographic g1, geographic g2) => new geographic(g1.lat + g2.lat, g1.lon + g2.lon);
    public static geographic operator-(geographic g1, geographic g2) => new geographic(g1.lat - g2.lat, g1.lon - g2.lon);
    public static geographic operator*(geographic g1, double d) => new geographic(g1.lat * d, g1.lon * d);
    public static geographic operator*(double d, geographic g1) => g1 * d;
    public static geographic operator/(geographic g1, double d) => new geographic(g1.lat / d, g1.lon / d);
    public static geographic operator/(double d, geographic g1) => new geographic(d / g1.lat, d / g1.lon);
    public static bool operator==(geographic g1, geographic g2) => g1.lat == g2.lat && g1.lon == g2.lon;
    public static bool operator!=(geographic g1, geographic g2) => g1.lat != g2.lat || g1.lon != g2.lon;

    public override bool Equals(object obj)
    {
        if (!(obj is geographic)) return false;
        geographic p = (geographic) obj;
        return this == p;
    }
    public override int GetHashCode()
    {
        unchecked
        {
            int hash = (int) 2166136261;
            hash = (hash * 16777619) ^ lat.GetHashCode();
            hash = (hash * 16777619) ^ lon.GetHashCode();

            return hash;
        }
    }
    public override string ToString() => $"Latitude: {lat} | Longitude {lon}";
    #endregion
}
