﻿using System;
using Neodroid.Runtime.Utilities.Structs;

namespace Neodroid.Runtime.Messaging.Messages.Displayables {
  /// <inheritdoc />
  /// <summary>
  /// </summary>
  public class DisplayableValuedVector3S : Displayable {
    public DisplayableValuedVector3S(String displayable_name, Points.ValuePoint[] displayable_value) {
      this.DisplayableName = displayable_name;
      this.DisplayableValue = displayable_value;
    }
  }
}