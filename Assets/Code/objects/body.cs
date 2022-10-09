using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> Parent class of <see cref="planet"/> and <see cref="satellite"/> </summary>
public abstract class body {
    #region VARIABLES
    /// <summary> Name of the body. Acts as the ID, so should be unique. </summary>
    public string name {get; protected set;}

    /// <summary> Position of body relative to sun. </summary>
    public position worldPos {get; protected set;}

    /// <summary> Position of body relative to its physical parent. </summary>
    public position localPos {get; protected set;}

    /// <summary> Physical object that the body is parented to. WorldPos will be given as the position of the current body + position of the parent. </summary>
    public body parent {get; protected set;} = null;

    /// <summary> Physical objects that are parented to this body. Used when calculating the worldPos of the children. </summary>
    public List<body> children = new List<body>();

    /// <summary> Relevant information of the body. </summary>
    public bodyInfo information {get; protected set;}

    /// <summary> Information used by the representation. </summary>
    public bodyRepresentationInfo repInformation {get; protected set;}

    /// <summary> Representation of the body in Unity. </summary>
    public GameObject representation;
    #endregion

    #region CONSTRUCTORS
    public body(bodyInfo info, bodyRepresentationInfo repInfo) {
        this.name = info.name;
        this.information = info;
        this.repInformation = repInfo;

        this.representation = GameObject.Instantiate(general.bodyPrefab, Vector3.zero, Quaternion.identity, general.bodyParent);
        this.representation.GetComponent<MeshRenderer>().material = repInfo.mat;
        this.representation.name = name;
    }
    #endregion

    #region INSTANCE METHODS
    /// </summary> Set the position of the body in accordance to the current system time. </summary>
    public abstract void updatePosition();

    /// </summary> Set the scale of the body in accordance to the current system scale. </summary>
    public abstract void updateScale();

    /// <summary> Get the local position of body at time t. </summary>
    public position requestLocalPosition(time t) => information.positions.find(t);

    /// <summary> Get the world position of body at time t. </summary>
    public position requestWorldPosition(time t) {
        position p = information.positions.find(t);

        // ascend parent tree until we reach end
        body b = this;
        while (true) {
            if (ReferenceEquals(b.parent, null)) break;
            if (b.parent.information.bodyID == bodyType.sun) break;

            p += b.parent.requestLocalPosition(t);
            b = b.parent.parent;
        }

        return p;
    }
    #endregion

    #region STATIC METHODS
    /// <summary> Set the parent and child of two bodies. </summary>
    public static void addFamilyNode(body parent, body child) {
        child.parent = parent;
        parent.children.Add(child);
    }

    /// <summary> Remove a parent from a child. </summary>
    public static void removeFamilyNode(body child) {
        child.parent.children.Remove(child);
        child.parent = null;
    }
    #endregion

    #region OVERRIDES/OPERATORS
    public override int GetHashCode() => name.GetHashCode();
    public override bool Equals(object obj)
    {
        if (!(obj is body)) return false;
        return ((body) obj).GetHashCode() == this.GetHashCode();
    }
    #endregion
}
