using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> All relevant, non-changing information for anything that is a child of parent. </summary>
public readonly struct bodyInfo {
    #region VARIABLES
    /// <summary> Name of the body. Acts as the ID, so should be unique. </summary>
    public readonly string name;
    /// <summary> Radius of the body, in km. </summary>
    public readonly double radius;
    /// <summary> Keplerian elements of the body. </summary>
    public readonly timeline positions;
    /// <summary> The bodies type. </summary>
    public readonly bodyType bodyID;
    /// <summary> Children of the object in the solar system. Has no impact on any of the positional variables, only used for information. </summary>
    public readonly List<body> children;
    /// <summary> Parent of the object in the solar system. Has no impact on any of the positional variables, only used for information. </summary>
    public readonly body parent;
    #endregion

    #region CONSTRUCTORS
    public bodyInfo(string name, double radius, timeline positions, bodyType bodyID) {
        this.name = name;
        this.radius = radius;
        this.positions = positions;
        this.bodyID = bodyID;

        children = new List<body>();
        parent = null;
    }

    public bodyInfo(string name, double radius, timeline positions, bodyType bodyID, body parent, List<body> children) {
        this.name = name;
        this.radius = radius;
        this.positions = positions;
        this.bodyID = bodyID;
        this.parent = parent;
        this.children = children;
    }
    #endregion
}

/// <summary> The type of the body, a moon, planet, sun, or satellite. </summary>
public enum bodyType {
    moon, planet, sun, satellite
}

public readonly struct bodyRepresentationInfo {
    public readonly Material mat;
    public readonly bool visible;

    public bodyRepresentationInfo(Material mat, bool visible = true) {
        this.mat = mat;
        this.visible = visible;
    }
}