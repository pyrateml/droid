﻿using System;
using Neodroid.Runtime.Interfaces;
using Neodroid.Runtime.Utilities.BoundingBoxes;
using Neodroid.Runtime.Utilities.Enums;
using Neodroid.Runtime.Utilities.Structs;
using UnityEngine;
using Random = System.Random;

namespace Neodroid.Runtime.Prototyping.Configurables {
  /// <inheritdoc cref="Configurable" />
  ///  <summary>
  ///  </summary>
  [AddComponentMenu(
      ConfigurableComponentMenuPath._ComponentMenuPath
      + "TransformConfigurable1Dof"
      + ConfigurableComponentMenuPath._Postfix)]
  public class TransformConfigurable1Dof : Configurable,
                                           IHasSingle {
    /// <inheritdoc />
    /// <summary>
    /// </summary>
    public override string PrototypingTypeName {
      get { return "Transform" + this._axis_of_configuration + "Configurable"; }
    }

    public float ObservationValue {
      get { return this._observation_value; }
      private set { this._observation_value = value; }
    }

    [SerializeField] ValueSpace _single_value_space;

    /// <inheritdoc />
    ///  <summary>
    ///  </summary>
    public ValueSpace SingleSpace {
      get { return this._single_value_space; }
      set { this._single_value_space = value; }
    }

    public override void UpdateCurrentConfiguration() {
      var pos = this.transform.position;
      var dir = this.transform.forward;
      var rot = this.transform.up;
      if (this._use_environments_space) {
        pos = this.ParentEnvironment.TransformPosition(pos);
        dir = this.ParentEnvironment.TransformDirection(dir);
        rot = this.ParentEnvironment.TransformDirection(rot);
      }

      switch (this._axis_of_configuration) {
        case Axis.X_:
          this.ObservationValue = pos.x;
          break;
        case Axis.Y_:
          this.ObservationValue = pos.y;
          break;
        case Axis.Z_:
          this.ObservationValue = pos.z;
          break;
        case Axis.Dir_x_:
          this.ObservationValue = dir.x;
          break;
        case Axis.Dir_y_:
          this.ObservationValue = dir.y;
          break;
        case Axis.Dir_z_:
          this.ObservationValue = dir.z;
          break;
        case Axis.Rot_x_:
          this.ObservationValue = rot.x;
          break;
        case Axis.Rot_y_:
          this.ObservationValue = rot.y;
          break;
        case Axis.Rot_z_:
          this.ObservationValue = rot.z;
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }
    }

    /// <summary>
    ///
    /// </summary>
    protected override void PreSetup() {
      if (this._use_bounding_box_for_range) {
        if (this._bounding_box != null) {
          var valid_input = new ValueSpace {
              _Max_Value = Math.Min(
                  this._bounding_box.Bounds.size.x,
                  Math.Min(this._bounding_box.Bounds.size.y, this._bounding_box.Bounds.size.z))
          };
          valid_input._Min_Value = -valid_input._Max_Value;
          this.SingleSpace = valid_input;
        }
      }
    }

    public override void ApplyConfiguration(IConfigurableConfiguration simulator_configuration) {
      if (simulator_configuration.ConfigurableValue < this.SingleSpace._Min_Value
          || simulator_configuration.ConfigurableValue > this.SingleSpace._Max_Value) {
        Debug.Log(
            $"It does not accept input, outside allowed range {this.SingleSpace._Min_Value} to {this.SingleSpace._Max_Value}");
        return; // Do nothing
      }

      #if NEODROID_DEBUG
      if (this.Debugging) {
        Debug.Log("Applying " + simulator_configuration + " To " + this.Identifier);
      }
      #endif

      var pos = this.transform.position;
      var dir = this.transform.forward;
      var rot = this.transform.up;
      if (this._use_environments_space) {
        pos = this.ParentEnvironment.TransformPosition(pos);
        dir = this.ParentEnvironment.TransformDirection(dir);
        rot = this.ParentEnvironment.TransformDirection(rot);
      }

      switch (this._axis_of_configuration) {
        case Axis.X_:
          if (this.RelativeToExistingValue) {
            pos.Set(simulator_configuration.ConfigurableValue - pos.x, pos.y, pos.z);
          } else {
            pos.Set(simulator_configuration.ConfigurableValue, pos.y, pos.z);
          }

          break;
        case Axis.Y_:
          if (this.RelativeToExistingValue) {
            pos.Set(pos.x, simulator_configuration.ConfigurableValue - pos.y, pos.z);
          } else {
            pos.Set(pos.x, simulator_configuration.ConfigurableValue, pos.z);
          }

          break;
        case Axis.Z_:
          if (this.RelativeToExistingValue) {
            pos.Set(pos.x, pos.y, simulator_configuration.ConfigurableValue - pos.z);
          } else {
            pos.Set(pos.x, pos.y, simulator_configuration.ConfigurableValue);
          }

          break;
        case Axis.Dir_x_:
          if (this.RelativeToExistingValue) {
            dir.Set(simulator_configuration.ConfigurableValue - dir.x, dir.y, dir.z);
          } else {
            dir.Set(simulator_configuration.ConfigurableValue, dir.y, dir.z);
          }

          break;
        case Axis.Dir_y_:
          if (this.RelativeToExistingValue) {
            dir.Set(dir.x, simulator_configuration.ConfigurableValue - dir.y, dir.z);
          } else {
            dir.Set(dir.x, simulator_configuration.ConfigurableValue, dir.z);
          }

          break;
        case Axis.Dir_z_:
          if (this.RelativeToExistingValue) {
            dir.Set(dir.x, dir.y, simulator_configuration.ConfigurableValue - dir.z);
          } else {
            dir.Set(dir.x, dir.y, simulator_configuration.ConfigurableValue);
          }

          break;
        case Axis.Rot_x_:
          if (this.RelativeToExistingValue) {
            rot.Set(simulator_configuration.ConfigurableValue - rot.x, rot.y, rot.z);
          } else {
            rot.Set(simulator_configuration.ConfigurableValue, rot.y, rot.z);
          }

          break;
        case Axis.Rot_y_:
          if (this.RelativeToExistingValue) {
            rot.Set(rot.x, simulator_configuration.ConfigurableValue - rot.y, rot.z);
          } else {
            rot.Set(rot.x, simulator_configuration.ConfigurableValue, rot.z);
          }

          break;
        case Axis.Rot_z_:
          if (this.RelativeToExistingValue) {
            rot.Set(rot.x, rot.y, simulator_configuration.ConfigurableValue - rot.z);
          } else {
            rot.Set(rot.x, rot.y, simulator_configuration.ConfigurableValue);
          }

          break;
        default:
          throw new ArgumentOutOfRangeException();
      }

      var inv_pos = pos;
      var inv_dir = dir;
      var inv_rot = rot;
      if (this._use_environments_space) {
        inv_pos = this.ParentEnvironment.InverseTransformPosition(inv_pos);
        inv_dir = this.ParentEnvironment.InverseTransformDirection(inv_dir);
        inv_rot = this.ParentEnvironment.InverseTransformDirection(inv_rot);
      }

      this.transform.position = inv_pos;
      this.transform.rotation = Quaternion.identity;
      this.transform.rotation = Quaternion.LookRotation(inv_dir, inv_rot);
    }


    #region Fields

    [SerializeField] Axis _axis_of_configuration;
    [SerializeField] BoundingBox _bounding_box;
    [SerializeField] bool _use_bounding_box_for_range;
    [SerializeField] float _observation_value;
    [SerializeField] bool _use_environments_space;

    #endregion
  }
}