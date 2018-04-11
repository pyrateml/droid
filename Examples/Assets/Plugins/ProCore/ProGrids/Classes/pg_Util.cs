#if UNITY_EDITOR || !(UNITY_WP_8_1 || UNITY_WSA || UNITY_WSA_8_1 || UNITY_WSA_10_0 || UNITY_WINRT || UNITY_WINRT_8_1 || UNITY_WINRT_10_0)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ProGrids {
  public static class pg_Util {
    const float EPSILON = .0001f;

    static readonly Dictionary<Transform, SnapEnabledOverride> m_SnapOverrideCache =
        new Dictionary<Transform, SnapEnabledOverride>();

    static readonly Dictionary<Type, bool> m_NoSnapAttributeTypeCache = new Dictionary<Type, bool>();

    static readonly Dictionary<Type, MethodInfo> m_ConditionalSnapAttributeCache =
        new Dictionary<Type, MethodInfo>();

    public static Color ColorWithString(string value) {
      var valid = "01234567890.,";
      value = new string(value.Where(c => valid.Contains(c)).ToArray());
      var rgba = value.Split(',');

      // BRIGHT pink
      if (rgba.Length < 4)
        return new Color(1f, 0f, 1f, 1f);

      return new Color(
          float.Parse(rgba[0]),
          float.Parse(rgba[1]),
          float.Parse(rgba[2]),
          float.Parse(rgba[3]));
    }

    static Vector3 VectorToMask(Vector3 vec) {
      return new Vector3(
          Mathf.Abs(vec.x) > Mathf.Epsilon ? 1f : 0f,
          Mathf.Abs(vec.y) > Mathf.Epsilon ? 1f : 0f,
          Mathf.Abs(vec.z) > Mathf.Epsilon ? 1f : 0f);
    }

    static Axis MaskToAxis(Vector3 vec) {
      var axis = Axis.None;
      if (Mathf.Abs(vec.x) > 0) axis |= Axis.X;
      if (Mathf.Abs(vec.y) > 0) axis |= Axis.Y;
      if (Mathf.Abs(vec.z) > 0) axis |= Axis.Z;
      return axis;
    }

    static Axis BestAxis(Vector3 vec) {
      var x = Mathf.Abs(vec.x);
      var y = Mathf.Abs(vec.y);
      var z = Mathf.Abs(vec.z);

      return x > y && x > z ? Axis.X : (y > x && y > z ? Axis.Y : Axis.Z);
    }

    public static Axis CalcDragAxis(Vector3 movement, Camera cam) {
      var mask = VectorToMask(movement);

      if (mask.x + mask.y + mask.z == 2)
        return MaskToAxis(Vector3.one - mask);
      switch (MaskToAxis(mask)) {
        case Axis.X:
          if (Mathf.Abs(Vector3.Dot(cam.transform.forward, Vector3.up))
              < Mathf.Abs(Vector3.Dot(cam.transform.forward, Vector3.forward)))
            return Axis.Z;
          else
            return Axis.Y;

        case Axis.Y:
          if (Mathf.Abs(Vector3.Dot(cam.transform.forward, Vector3.right))
              < Mathf.Abs(Vector3.Dot(cam.transform.forward, Vector3.forward)))
            return Axis.Z;
          else
            return Axis.X;

        case Axis.Z:
          if (Mathf.Abs(Vector3.Dot(cam.transform.forward, Vector3.right))
              < Mathf.Abs(Vector3.Dot(cam.transform.forward, Vector3.up)))
            return Axis.Y;
          else
            return Axis.X;
        default:

          return Axis.None;
      }
    }

    public static float ValueFromMask(Vector3 val, Vector3 mask) {
      if (Mathf.Abs(mask.x) > .0001f)
        return val.x;
      if (Mathf.Abs(mask.y) > .0001f)
        return val.y;
      return val.z;
    }

    public static Vector3 SnapValue(Vector3 val, float snapValue) {
      float _x = val.x, _y = val.y, _z = val.z;
      return new Vector3(Snap(_x, snapValue), Snap(_y, snapValue), Snap(_z, snapValue));
    }

    /**
     *	Fetch a type with name and optional assembly name.  `type` should include namespace.
     */
    static Type GetType(string type, string assembly = null) {
      var t = Type.GetType(type);

      if (t == null) {
        IEnumerable<Assembly> assemblies = AppDomain.CurrentDomain.GetAssemblies();

        if (assembly != null)
          assemblies = assemblies.Where(x => x.FullName.Contains(assembly));

        foreach (var ass in assemblies) {
          t = ass.GetType(type);

          if (t != null)
            return t;
        }
      }

      return t;
    }

    public static void SetUnityGridEnabled(bool isEnabled) {
      try {
        var annotationUtility = GetType("UnityEditor.AnnotationUtility");
        var pi = annotationUtility.GetProperty("showGrid", BindingFlags.NonPublic | BindingFlags.Static);
        pi.SetValue(null, isEnabled, BindingFlags.NonPublic | BindingFlags.Static, null, null, null);
      } catch { }
    }

    public static bool GetUnityGridEnabled() {
      try {
        var annotationUtility = GetType("UnityEditor.AnnotationUtility");
        var pi = annotationUtility.GetProperty("showGrid", BindingFlags.NonPublic | BindingFlags.Static);
        return (bool)pi.GetValue(null, null);
      } catch { }

      return false;
    }

    public static Vector3 SnapValue(Vector3 val, Vector3 mask, float snapValue) {
      float _x = val.x, _y = val.y, _z = val.z;
      return new Vector3(
          Mathf.Abs(mask.x) < EPSILON ? _x : Snap(_x, snapValue),
          Mathf.Abs(mask.y) < EPSILON ? _y : Snap(_y, snapValue),
          Mathf.Abs(mask.z) < EPSILON ? _z : Snap(_z, snapValue));
    }

    public static Vector3 SnapToCeil(Vector3 val, Vector3 mask, float snapValue) {
      float _x = val.x, _y = val.y, _z = val.z;
      return new Vector3(
          Mathf.Abs(mask.x) < EPSILON ? _x : SnapToCeil(_x, snapValue),
          Mathf.Abs(mask.y) < EPSILON ? _y : SnapToCeil(_y, snapValue),
          Mathf.Abs(mask.z) < EPSILON ? _z : SnapToCeil(_z, snapValue));
    }

    public static Vector3 SnapToFloor(Vector3 val, float snapValue) {
      float _x = val.x, _y = val.y, _z = val.z;
      return new Vector3(SnapToFloor(_x, snapValue), SnapToFloor(_y, snapValue), SnapToFloor(_z, snapValue));
    }

    public static Vector3 SnapToFloor(Vector3 val, Vector3 mask, float snapValue) {
      float _x = val.x, _y = val.y, _z = val.z;
      return new Vector3(
          Mathf.Abs(mask.x) < EPSILON ? _x : SnapToFloor(_x, snapValue),
          Mathf.Abs(mask.y) < EPSILON ? _y : SnapToFloor(_y, snapValue),
          Mathf.Abs(mask.z) < EPSILON ? _z : SnapToFloor(_z, snapValue));
    }

    public static float Snap(float val, float round) { return round * Mathf.Round(val / round); }

    public static float SnapToFloor(float val, float snapValue) {
      return snapValue * Mathf.Floor(val / snapValue);
    }

    public static float SnapToCeil(float val, float snapValue) {
      return snapValue * Mathf.Ceil(val / snapValue);
    }

    public static Vector3 CeilFloor(Vector3 v) {
      v.x = v.x < 0 ? -1 : 1;
      v.y = v.y < 0 ? -1 : 1;
      v.z = v.z < 0 ? -1 : 1;

      return v;
    }

    public static void ClearSnapEnabledCache() { m_SnapOverrideCache.Clear(); }

    public static bool SnapIsEnabled(Transform t) {
      SnapEnabledOverride so;

      if (m_SnapOverrideCache.TryGetValue(t, out so))
        return so.IsEnabled();

      object[] attribs = null;

      foreach (Component c in t.GetComponents<MonoBehaviour>()) {
        if (c == null)
          continue;

        var type = c.GetType();

        bool hasNoSnapAttrib;

        if (m_NoSnapAttributeTypeCache.TryGetValue(type, out hasNoSnapAttrib)) {
          if (hasNoSnapAttrib) {
            m_SnapOverrideCache.Add(t, new SnapIsEnabledOverride(!hasNoSnapAttrib));
            return true;
          }
        } else {
          attribs = type.GetCustomAttributes(true);
          hasNoSnapAttrib = attribs.Any(x => x != null && x.ToString().Contains("ProGridsNoSnap"));
          m_NoSnapAttributeTypeCache.Add(type, hasNoSnapAttrib);

          if (hasNoSnapAttrib) {
            m_SnapOverrideCache.Add(t, new SnapIsEnabledOverride(!hasNoSnapAttrib));
            return true;
          }
        }

        MethodInfo mi;

        if (m_ConditionalSnapAttributeCache.TryGetValue(type, out mi)) {
          if (mi != null) {
            m_SnapOverrideCache.Add(
                t,
                new ConditionalSnapOverride(
                    () => {
                      return (bool)mi.Invoke(c, null);
                    }));
            return (bool)mi.Invoke(c, null);
          }
        } else {
          if (attribs.Any(x => x != null && x.ToString().Contains("ProGridsConditionalSnap"))) {
            mi = type.GetMethod(
                "IsSnapEnabled",
                BindingFlags.Instance
                | BindingFlags.FlattenHierarchy
                | BindingFlags.NonPublic
                | BindingFlags.Public);

            m_ConditionalSnapAttributeCache.Add(type, mi);

            if (mi != null) {
              m_SnapOverrideCache.Add(
                  t,
                  new ConditionalSnapOverride(
                      () => {
                        return (bool)mi.Invoke(c, null);
                      }));
              return (bool)mi.Invoke(c, null);
            }
          } else
            m_ConditionalSnapAttributeCache.Add(type, null);
        }
      }

      m_SnapOverrideCache.Add(t, new SnapIsEnabledOverride(true));

      return true;
    }

    abstract class SnapEnabledOverride {
      public abstract bool IsEnabled();
    }

    class SnapIsEnabledOverride : SnapEnabledOverride {
      readonly bool m_SnapIsEnabled;

      public SnapIsEnabledOverride(bool snapIsEnabled) { this.m_SnapIsEnabled = snapIsEnabled; }

      public override bool IsEnabled() { return this.m_SnapIsEnabled; }
    }

    class ConditionalSnapOverride : SnapEnabledOverride {
      public readonly Func<bool> m_IsEnabledDelegate;

      public ConditionalSnapOverride(Func<bool> d) { this.m_IsEnabledDelegate = d; }

      public override bool IsEnabled() { return this.m_IsEnabledDelegate(); }
    }
  }

  public static class PGExtensions {
    public static bool Contains(this Transform[] t_arr, Transform t) {
      for (var i = 0; i < t_arr.Length; i++) {
        if (t_arr[i] == t)
          return true;
      }

      return false;
    }

    public static float Sum(this Vector3 v) { return v[0] + v[1] + v[2]; }

    public static bool InFrustum(this Camera cam, Vector3 point) {
      var p = cam.WorldToViewportPoint(point);
      return p.x >= 0f && p.x <= 1f && p.y >= 0f && p.y <= 1f && p.z >= 0f;
    }
  }
}
#endif
