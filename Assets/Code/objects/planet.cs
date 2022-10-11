using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> Main class that holds all data needed for planets (including moons). </summary>
/// <remarks> Related classes: <see cref="satellite"/>, <see cref="facility"/> </remarks>
public class planet : body {
    #region VARIABLES
    #endregion

    #region CONSTRUCTORS
    public planet(bodyInfo info, bodyRepresentationInfo repInfo) : base(info, repInfo) {
        master.registeredPlanets.Add(this);

        if (info.bodyID == bodyType.sun) master.sun = this;
    }
    #endregion

    #region INSTANCE METHODS
    #endregion

    #region OVERRIDES/OPERATORS
        public override void updatePosition() {
        position p = (base.requestWorldPosition(master.getCurrentTime()) - master.referenceFrame.requestWorldPosition(master.getCurrentTime()) - master.playerPosition) / master.scale;
        p.swapAxis();
        representation.transform.position = (Vector3) p;
    }

    public override void updateScale() {
        representation.transform.localScale = new Vector3(
            Mathf.Max((float) (information.radius / master.scale), 1),
            Mathf.Max((float) (information.radius / master.scale), 1),
            Mathf.Max((float) (information.radius / master.scale), 1));
    }
    #endregion
}
