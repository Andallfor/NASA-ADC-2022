using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> Class that holds important information about the current state of the program. Mainly for backend stuff. </summary>
/// <remarks> See <see cref="general"/> for the frontend version of this class. </summary>
public static class master {
    #region VARIABLES
    /// <summary> The sun in the program. Must exist in all scenarios. </summary>
    public static body sun;
    /// <summary> The current scale of the program, in km. </summary>
    /// <remarks><code> earth.representation.transform.position = (Vector3) (earth.worldPos / master.scale) </code></remarks>
    public static double scale = 100;
    /// <summary> The current program's time. Use <see cref="getCurrentTime()"/> and <see cref="incrementTime(double)"/> </summary>
    private static time sysTime = new time(2460806.5);
    /// <summary> The current object that is being focused by the program. Will be at (0, 0, 0) in 3D space (if <see cref="playerPosition"/> is (0, 0, 0)). </summary>
    public static body referenceFrame;
    /// <summary> The position of the player in km. </summary>
    public static position playerPosition;
    #endregion

    #region STATIC METHODS
    /// <summary> Get the current system time. Modifying the return value will not affect the actual system. Use <see cref="incrementTime(double)"/> to change the time. </summary>
    public static time getCurrentTime() => sysTime;
    /// <summary> Change the current system time by a julian value. </summary>
    public static void incrementTime(double add) {sysTime.addJulianTime(add);}
    #endregion
}
