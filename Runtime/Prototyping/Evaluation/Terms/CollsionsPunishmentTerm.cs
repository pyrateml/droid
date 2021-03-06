﻿using UnityEngine;

namespace Neodroid.Runtime.Prototyping.Evaluation.Terms {
  [AddComponentMenu(
      TermComponentMenuPath._ComponentMenuPath + "CollisionPunishment" + TermComponentMenuPath._Postfix)]
  public class CollsionsPunishmentTerm : Term {
    [SerializeField] Collider _a;

    [SerializeField] Collider _b;

    public override float Evaluate() {
      if (this._a.bounds.Intersects(this._b.bounds)) {
        return -1;
      }

      return 0;
    }

    public override string PrototypingTypeName {
      get { return "CollisionPunishment"; }
    }
  }
}