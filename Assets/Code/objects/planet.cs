using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> Main class that holds all data needed for planets (including moons). </summary>
/// <remarks> Related classes: <see cref="satellite"/>, <see cref="crater"/> </remarks>
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
    /// <summary> Get the position of the geographic point on this planet centered on the center of the planet, in world space and accounting for its rotation. </summary>
    public position rotateLocalGeo(geographic g, double alt) => geographic.toGeographic(representation.gameObject.transform.rotation * (Vector3) (g.toCartesian(information.radius + alt)).swapAxis(), information.radius).toCartesian(information.radius + alt).swapAxis();
    #endregion

    #region OVERRIDES/OPERATORS
        public override void updatePosition() {
        localPos = requestLocalPosition(master.getCurrentTime());
        worldPos = this.localPos + ((information.bodyID == bodyType.sun) ? new position(0, 0, 0) : parent.worldPos);

        position p = (worldPos - master.playerPosition - master.referenceFrame.worldPos) / master.scale;

        representation.transform.localPosition = (Vector3) p.swapAxis();
    }

    public override void updateScale() {
        representation.transform.localScale = new Vector3(
            Mathf.Max((float) ((information.radius * 2.0) / master.scale), 1),
            Mathf.Max((float) ((information.radius * 2.0) / master.scale), 1),
            Mathf.Max((float) ((information.radius * 2.0) / master.scale), 1));
    }
    #endregion
}
