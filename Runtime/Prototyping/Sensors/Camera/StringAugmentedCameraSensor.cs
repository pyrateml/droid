﻿using System;
using droid.Runtime.Environments;
using droid.Runtime.Interfaces;
using droid.Runtime.Utilities.Misc;
using UnityEngine;

namespace droid.Runtime.Prototyping.Sensors.Camera {
  /// <inheritdoc cref="Sensor" />
  /// <summary>
  /// </summary>
  [AddComponentMenu(SensorComponentMenuPath._ComponentMenuPath
                    + "StringAugmentedCamera"
                    + SensorComponentMenuPath._Postfix)]
  [ExecuteInEditMode]
  [RequireComponent(typeof(UnityEngine.Camera))]
  public class StringAugmentedCameraSensor : CameraSensor,
                                             IHasString {
    const string _color_identifier = "Colors";

    string _colors;

    /// <summary>
    /// </summary>
    [Header("Observation", order = 103)]
    [SerializeField]
    protected string serialisedString;

    /// <summary>
    /// </summary>
    public String ObservationValue { get { return this.serialisedString; } }

    protected override void PreSetup() {
      base.PreSetup();
      this._colors = this.Identifier + _color_identifier;
    }

    /// <inheritdoc />
    /// <summary>
    /// </summary>
    protected override void RegisterComponent() {
      this.ParentEnvironment =
          NeodroidUtilities.RegisterComponent((PrototypingEnvironment)this.ParentEnvironment,
                                              this,
                                              this.Identifier);

      this.ParentEnvironment =
          NeodroidUtilities.RegisterComponent((PrototypingEnvironment)this.ParentEnvironment,
                                              this,
                                              this._colors);
    }

    /// <inheritdoc />
    /// <summary>
    /// </summary>
    protected override void UnRegisterComponent() {
      this.ParentEnvironment?.UnRegister(this, this.Identifier);
      this.ParentEnvironment?.UnRegister(this, this._colors);
    }

    /// <inheritdoc />
    /// <summary>
    /// </summary>
    public override void UpdateObservation() {
      base.UpdateObservation();
      this.serialisedString = "";
    }
  }
}
