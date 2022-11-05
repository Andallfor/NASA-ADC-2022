using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary> Class that holds important information about the current state of the program. Mainly for backend stuff. </summary>
/// <remarks> See <see cref="general"/> for the frontend version of this class. </summary>
public static class master {
    #region VARIABLES
    #region BODIES
    /// <summary> The sun in the program. Must exist in all scenarios. </summary>
    public static body sun;
    /// <summary> The current object that is being focused by the program. Will be at (0, 0, 0) in 3D space (if <see cref="playerPosition"/> is (0, 0, 0)). </summary>
    public static body referenceFrameBody;

    #endregion BODIES
    #region CURRENT STATE

    /// <summary> The current scale of the program, in km. </summary>
    /// <remarks><code> earth.representation.transform.position = (Vector3) (earth.worldPos / master.scale) </code></remarks>
    public static double scale {get => _scale; set {
        _scale = value;

        foreach (planet p in registeredPlanets) p.updateScale();
    }}
    private static double _scale = 1000;
    /// <summary> The current program's time. Use <see cref="getCurrentTime()"/> and <see cref="incrementTime(double)"/> </summary>
    private static time sysTime = new time(2460806.5);
    /// <summary> The position of the player in km. </summary>
    public static position playerPosition;
    public static position referenceFrame {get; private set;}
    /// <summary> Whether or not the program has started. </summary>
    public static bool initialized {get; private set;} = false;

    #endregion CURRENT STATE
    #region MISC

    /// <summary> All planets that exist in the program. </summary>
    public static List<planet> registeredPlanets = new List<planet>();
    /// <summary> All craters that exist in the program. </summary>
    public static List<crater> registeredCraters = new List<crater>();
    /// <summary> Remembers the current state of the program (ex if we are display terrain or not). </summary>
    /// <remarks> See <see cref="changeState(programStates)"/> and <see cref="onStateChange"/> </remarks>
    public static programStates currentState {get; private set;}
    #endregion
    #endregion VARIABLES

    #region STATIC METHODS
    /// <summary> Get the current system time. Modifying the return value will not affect the actual system. Use <see cref="incrementTime(double)"/> to change the time. </summary>
    public static time getCurrentTime() => sysTime;
    /// <summary> Change the current system time by a julian value. </summary>
    public static void incrementTime(double add) {
        sysTime.addJulianTime(add);
    }
    /// <summary> Mark the program as initialized. </summary>
    public static void markInit() {
        if (ReferenceEquals(master.sun, null)) throw new ArgumentException("Could not find a sun in the program.");

        initialized = true;
        // TODO: algorithm that starts from sun and descends tree to update, rather then depend on order that they are registered
        foreach (planet p in registeredPlanets) {
            p.updateScale();
            p.updatePosition();
        }
    }
    /// <summary> Changes the current program state. Calls <see cref="onStateChange()"/> </summary>
    public static void changeState(programStates state) {
        programStates old = currentState;
        currentState = state;

        onStateChange.Invoke(null, new stateChangeEvent(old, state));
    }
    /// <summary> Calls <see cref="onUpdateEnd"/> </summary>
    public static void notifyUpdateEnd() {
        onUpdateEnd(null, EventArgs.Empty);
    }

    public static void propagateUpdate() {
        Queue<body> queue = new Queue<body>();
        queue.Enqueue(master.sun);

        position frame = new position(0, 0, 0);
        if (referenceFrameBody.information.bodyID != bodyType.sun) {
            body b = referenceFrameBody;
            while (true) {
                frame += b.requestLocalPosition(master.sysTime);

                if (b.parent == master.sun) break;
                b = b.parent;
            }
        }

        referenceFrame = frame;

        while (queue.Count > 0) {
            body next = queue.Dequeue();

            next.updatePosition();

            foreach (body b in next.children) queue.Enqueue(b);
        }
    }
    #endregion STATIC METHODS

    #region EVENTS
    /// <summary> Event that is called whenever the state of the program is changed. </summary>
    public static event EventHandler<stateChangeEvent> onStateChange = delegate {};
    /// <summary> Event that is called at the end of every tick. </summary>
    public static event EventHandler onUpdateEnd = delegate {};
    #endregion EVENTS
}
