﻿#if UNITY_EDITOR
using Neodroid.Editor.Windows;
using Neodroid.Runtime.Utilities.ScriptableObjects;
using UnityEditor;
using UnityEngine;

namespace Neodroid.Editor.ScriptableObjects {
  public static class CreatePlayerMotions {
    [MenuItem(EditorScriptableObjectMenuPath._ScriptableObjectMenuPath + "PlayerMotions")]
    public static void CreatePlayerMotionsAsset() {
      var asset = ScriptableObject.CreateInstance<PlayerMotions>();

      AssetDatabase.CreateAsset(asset, EditorWindowMenuPath._NewAssetPath + "Assets/NewPlayerMotions.asset");
      AssetDatabase.SaveAssets();

      EditorUtility.FocusProjectWindow();

      Selection.activeObject = asset;
    }
  }
}
#endif
