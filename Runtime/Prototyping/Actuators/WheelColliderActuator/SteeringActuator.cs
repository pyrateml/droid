﻿using droid.Runtime.Interfaces;
using UnityEngine;

namespace droid.Runtime.Prototyping.Actuators.WheelColliderActuator {
  /// <summary>
  /// </summary>
  [AddComponentMenu(ActuatorComponentMenuPath._ComponentMenuPath
                    + "WheelCollider/Steering"
                    + ActuatorComponentMenuPath._Postfix)]
  [RequireComponent(typeof(WheelCollider))]
  public class SteeringActuator : Actuator {
    /// <summary>
    /// </summary>
    [SerializeField]
    WheelCollider _wheel_collider;

    /// <summary>
    /// </summary>
    public override string PrototypingTypeName { get { return "Steering"; } }

    /// <summary>
    /// </summary>
    protected override void Setup() { this._wheel_collider = this.GetComponent<WheelCollider>(); }

    /// <summary>
    /// </summary>
    void FixedUpdate() { ApplyLocalPositionToVisuals(this._wheel_collider); }

    /// <summary>
    /// </summary>
    /// <param name="motion"></param>
    protected override void InnerApplyMotion(IMotion motion) {
      this._wheel_collider.steerAngle = motion.Strength;
    }

    /// <summary>
    /// </summary>
    /// <param name="col"></param>
    static void ApplyLocalPositionToVisuals(WheelCollider col) {
      if (col.transform.childCount == 0) {
        return;
      }

      var visual_wheel = col.transform.GetChild(0);

      Vector3 position;
      Quaternion rotation;
      col.GetWorldPose(out position, out rotation);

      visual_wheel.transform.position = position;
      visual_wheel.transform.rotation = rotation;
    }
  }
}