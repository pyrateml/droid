﻿using UnityEngine;

namespace Neodroid.Utilities.Interfaces {
  /// <summary>
  /// 
  /// </summary>
  public interface IHasRigidbody {
    /// <summary>
    /// 
    /// </summary>
    Vector3 Velocity { get; }

    /// <summary>
    /// 
    /// </summary>
    Vector3 AngularVelocity { get; }
  }
}
