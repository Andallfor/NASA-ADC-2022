using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary> Possible states of the program. See <see cref="master.currentState"/> </summary>
public enum programStates {
    /// <summary> View of multiple planets. </summary>
    interplanetary,
    planetaryTerrain
}

/// <summary> Information about the recent program state change. </summary>
public class stateChangeEvent : EventArgs {
    public programStates previousState, newState;

    public stateChangeEvent(programStates old, programStates young) {
        this.previousState = old;
        this.newState = young;
    }
}