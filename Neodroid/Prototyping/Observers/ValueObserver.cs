﻿using Neodroid.Utilities.Interfaces;
using Neodroid.Utilities.Structs;
using UnityEngine;

namespace Neodroid.Prototyping.Observers {
  [AddComponentMenu(PrototypingComponentMenuPath._ComponentMenuPath + "Observers/Value")]
  [ExecuteInEditMode]
  public class ValueObserver : Observer,
                               IHasSingle {
    [Header("Observation", order = 103)]
    [SerializeField]
    float _observation_value;

    [SerializeField] ValueSpace _observation_value_space;

    public override string Identifier { get { return this.name + "Value"; } }

    public float ObservationValue {
      get { return this._observation_value; }
      set {
        this._observation_value = this.NormaliseObservationUsingSpace
                                      ? this._observation_value_space.ClipNormaliseRound(value)
                                      : value;
      }
    }

    protected override void InnerSetup() { this.FloatEnumerable = new[] {this.ObservationValue}; }

    public override void UpdateObservation() { this.FloatEnumerable = new[] {this.ObservationValue}; }
  }
}
