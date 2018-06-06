﻿using System;
using droid.Neodroid.Utilities.Structs;
using UnityEngine;
using UnityEngine.UI;

namespace droid.Neodroid.Prototyping.Displayers.Canvas {
  /// <inheritdoc />
  /// <summary>
  /// </summary>
  [ExecuteInEditMode]
  [RequireComponent(typeof(Text))]
  [AddComponentMenu(
      DisplayerComponentMenuPath._ComponentMenuPath
      + "Canvas/CanvasText"
      + DisplayerComponentMenuPath._Postfix)]
  public class CanvasTextDisplayer : Displayer {
    Text _text_component;

    protected override void Setup() {
      this._text_component = this.GetComponent<Text>();
      this._text_component.text = "TEEEEEXT!";
    }

    public override void Display(float value) { throw new NotImplementedException(); }
    public override void Display(Double value) { throw new NotImplementedException(); }
    public override void Display(float[] values) { throw new NotImplementedException(); }

    public override void Display(String value) {
      #if NEODROID_DEBUG
      if (this.Debugging) {
        Debug.Log("Applying " + value + " To " + this.name);
      }
      #endif

      this.SetText(value);
    }

    public override void Display(Vector3 value) { throw new NotImplementedException(); }
    public override void Display(Vector3[] value) { throw new NotImplementedException(); }

    public override void Display(Points.ValuePoint points) { throw new NotImplementedException(); }

    public override void Display(Points.ValuePoint[] points) { throw new NotImplementedException(); }

    public override void Display(Points.StringPoint point) { throw new NotImplementedException(); }
    public override void Display(Points.StringPoint[] points) { throw new NotImplementedException(); }

    public void SetText(string text) { this._text_component.text = text; }
  }
}
