﻿using Neodroid.Prototyping.Actors;
using UnityEngine;

namespace Neodroid.Prototyping.Evaluation {
  [AddComponentMenu(PrototypingComponentMenuPath._ComponentMenuPath+"Evaluation/EuclideanDistance")]
  class EuclideanDistance : ObjectiveFunction {
    [SerializeField] Transform _g1;
    [SerializeField] Transform _g2;

    public override float InternalEvaluate() {
      return Vector3.Distance(this._g1.position, this._g2.position);
    }

    void Start() {
      if (this._g1 == null)
        this._g1 = FindObjectOfType<Actor>().transform;

      if (this._g2 == null)
        this._g2 = this.transform;
    }
  }
}