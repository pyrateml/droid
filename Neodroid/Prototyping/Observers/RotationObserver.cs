﻿using System;
using Neodroid.Utilities.Interfaces;
using Neodroid.Utilities.Structs;
using UnityEngine;

namespace Neodroid.Prototyping.Observers {
  [AddComponentMenu(PrototypingComponentMenuPath._ComponentMenuPath+"Observers/Position")]
  [ExecuteInEditMode]
  [Serializable]
  public class RotationObserver : Observer,
                                  IHasQuadruple {
    [Header("Specfic", order = 102)]
    [SerializeField]
     ObservationSpace _space = ObservationSpace.Environment_;

    [Header("Observation", order = 103)]
    [SerializeField]
    Quaternion _rotation;

    public ObservationSpace Space { get { return this._space; } }

    public override string ObserverIdentifier { get { return this.name + "Position"; } }

    protected override void InnerSetup() {
      this.FloatEnumerable =
          new[] {this.ObservationValue.x, this.ObservationValue.y, this.ObservationValue.z};
    }

    public override void UpdateObservation() {
      if (this.ParentEnvironment && this._space == ObservationSpace.Environment_)
        this.ObservationValue = this.ParentEnvironment.TransformRotation(this.transform.rotation);
      else if (this._space == ObservationSpace.Local_)
        this.ObservationValue = this.transform.localRotation;
      else
        this.ObservationValue = this.transform.rotation;

      this.FloatEnumerable =
          new[] {this.ObservationValue.x, this.ObservationValue.y, this.ObservationValue.z};
    }

    public Quaternion ObservationValue { get { return this._rotation; } set { this._rotation = value; } }
  }
}