﻿#if UNITY_EDITOR
using droid.Neodroid.Utilities.ScriptableObjects;
using UnityEditor;
using UnityEngine;

namespace droid.Neodroid.Editor.ScriptableObjects {
  public static class CreateSegmentations {
    [MenuItem(EditorScriptableObjectMenuPath._ScriptableObjectMenuPath + "Segmentations")]
    public static void CreateSegmentationsAsset() {
      var asset = ScriptableObject.CreateInstance<Segmentation>();

      AssetDatabase.CreateAsset(asset, "Assets/NewSegmentations.asset");
      AssetDatabase.SaveAssets();

      EditorUtility.FocusProjectWindow();

      Selection.activeObject = asset;
    }
  }
}
#endif
