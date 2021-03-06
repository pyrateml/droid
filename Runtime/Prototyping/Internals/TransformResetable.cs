﻿using UnityEngine;

namespace Neodroid.Runtime.Prototyping.Internals {
  /// <inheritdoc />
  /// <summary>
  /// </summary>
  [AddComponentMenu(
      ResetableComponentMenuPath._ComponentMenuPath + "Transform" + ResetableComponentMenuPath._Postfix)]
  public class TransformResetable : Resetable {
    /// <inheritdoc />
    /// <summary>
    /// </summary>
    public override void EnvironmentReset() {
      this.transform.position = this._original_position;
      this.transform.rotation = this._original_rotation;
    }

    /// <inheritdoc />
    /// <summary>
    /// </summary>
    public override string PrototypingTypeName {
      get { return "Transform"; }
    }

    /// <summary>
    /// 
    /// </summary>
    Vector3 _original_position;

    /// <summary>
    /// 
    /// </summary>
    Quaternion _original_rotation;

    /// <summary>
    /// 
    /// </summary>
    protected override void Setup() {
      this._original_position = this.transform.position;
      this._original_rotation = this.transform.rotation;
    }
  }
}