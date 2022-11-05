using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary> Class that holds the position of celestial bodies at any given time. Uses Keplerian elements to calculate positions. </summary>
public class timeline {
    #region VARIABLES
    private const double degToRad = Math.PI / 180.0;
    private const double radToDeg = 180.0 / Math.PI;
    private double semiMajorAxis, eccentricity, inclination, argOfPerigee, longOfAscNode, mu, startingEpoch, orbitalPeriod, startingMeanAnom;
    private bool active = true;
    #endregion

    #region CONSTRUCTORS
    public timeline(double semiMajorAxis, double eccentricity, double inclination, double argOfPerigee, double longOfAscNode, double meanAnom, double startingEpoch, double mu) {
        this.semiMajorAxis = semiMajorAxis;
        this.eccentricity = eccentricity;
        this.inclination = inclination * degToRad;
        this.argOfPerigee = argOfPerigee * degToRad;
        this.longOfAscNode = longOfAscNode * degToRad;
        this.mu = mu;
        this.startingMeanAnom = meanAnom;
        this.orbitalPeriod = 2.0 * Math.PI * Math.Sqrt((semiMajorAxis * semiMajorAxis * semiMajorAxis) / mu);
        this.startingEpoch = startingEpoch;
    }

    /// <summary> Use when you want timeline to be inactive/the body does not move. </summary>
    public timeline() {
        active = false;
    }
    #endregion

    #region INSTANCE METHODS
    /// <summary> Find the position of a body at a time. Due to using keplerian elements, will give their localPos. </summary>
    public position find(time t) {
        
        if (!active) return new position(0, 0, 0);

        // https://drive.google.com/file/d/1so93guuhCO94PEU8vFvDLv_-k9vJBcFs/view
        double meanAnom = startingMeanAnom;

        if (t.julian == startingEpoch) meanAnom = startingMeanAnom;
        else meanAnom = startingMeanAnom + 86400.0 * (t.julian - startingEpoch) * Math.Sqrt((mu / Math.Pow(semiMajorAxis, 3)));

        double EA = meanAnom;
        for (int i = 0; i < 15; i++) EA = meanAnom + eccentricity * Math.Sin(EA);

        double trueAnom1 = Math.Sqrt(1 - eccentricity * eccentricity) * (Math.Sin(EA) / (1 - eccentricity * Math.Cos(EA)));
        double trueAnom2 = (Math.Cos(EA) - eccentricity) / (1 - eccentricity * Math.Cos(EA));

        double trueAnom = Math.Atan2(trueAnom1, trueAnom2);

        double theta = trueAnom + argOfPerigee;

        double radius = semiMajorAxis * (1 - eccentricity * eccentricity) / (1 + eccentricity * Math.Cos(trueAnom));

        double xp = radius * Math.Cos(theta);
        double yp = radius * Math.Sin(theta);

        return new position(
            xp * Math.Cos(longOfAscNode) - yp * Math.Cos(inclination) * Math.Sin(longOfAscNode),
            xp * Math.Sin(longOfAscNode) - yp * Math.Cos(inclination) * Math.Cos(longOfAscNode),
            yp * Math.Sin(inclination));
    }
    #endregion
}
