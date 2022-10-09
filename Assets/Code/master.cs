using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> Class that holds important information about the current state of the program. Mainly for backend stuff. </summary>
/// <remarks> See <see cref="general"/> for the frontend version of this class. </summary>
public static class master {
    #region VARIABLES
    public static double scale = 100;
    public static body referenceFrame, sun;
    private static time sysTime = new time(2460806.5);
    public static position playerPosition;
    #endregion

    #region STATIC METHODS
    public static time getCurrentTime() => sysTime;
    public static void incrementTime(double add) {sysTime.addJulianTime(add);}
    #endregion
}
