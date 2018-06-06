﻿using System;
using droid.Neodroid.Utilities.Structs;
using UnityEngine;
using UnityEngine.UI;

namespace droid.Neodroid.Prototyping.Displayers.Canvas {
  [ExecuteInEditMode]
  [RequireComponent(typeof(Image))]
  [AddComponentMenu(
      DisplayerComponentMenuPath._ComponentMenuPath
      + "Canvas/CanvasBar"
      + DisplayerComponentMenuPath._Postfix)]
  public class CanvasBarDisplayer : Displayer {
    Image _image;
    [SerializeField] [Range(0.0f, 1.0f)] float _value;

    protected override void Setup() {
      this._image = this.GetComponent<Image>();
      this._image.fillAmount = 0.6f;
    }

    public void SetFillAmount(float amount) { this._image.fillAmount = amount; }

    public override void Display(float value) {
      #if NEODROID_DEBUG
      if (this.Debugging) {
        Debug.Log("Applying " + value + " To " + this.name);
      }
      #endif

      this.SetFillAmount(value);
    }

    public override void Display(Double value) {
      #if NEODROID_DEBUG
      if (this.Debugging) {
        Debug.Log("Applying " + value + " To " + this.name);
      }
      #endif

      this.SetFillAmount((float)value);
    }

    public override void Display(float[] values) { throw new NotImplementedException(); }
    public override void Display(String value) { throw new NotImplementedException(); }
    public override void Display(Vector3 value) { throw new NotImplementedException(); }
    public override void Display(Vector3[] value) { throw new NotImplementedException(); }

    public override void Display(Points.ValuePoint points) { throw new NotImplementedException(); }

    public override void Display(Points.ValuePoint[] points) { throw new NotImplementedException(); }

    public override void Display(Points.StringPoint point) { throw new NotImplementedException(); }
    public override void Display(Points.StringPoint[] points) { throw new NotImplementedException(); }
  }
}
