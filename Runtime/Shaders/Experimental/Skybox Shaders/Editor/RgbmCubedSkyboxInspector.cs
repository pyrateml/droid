﻿using UnityEditor;
using UnityEngine;

namespace droid.Runtime.Shaders.Experimental.Skybox_Shaders.Editor {
    public class RgbmCubedSkyboxInspector : MaterialEditor
    {
        public override void OnInspectorGUI ()
        {
            base.OnInspectorGUI ();

            if (this.isVisible)
            {
                var material = this.target as Material;

                bool useLinear = false;
                foreach (var keyword in material.shaderKeywords)
                {
                    if (keyword == "USE_LINEAR")
                    {
                        useLinear = true;
                        break;
                    }
                }

                EditorGUI.BeginChangeCheck ();

                useLinear = EditorGUILayout.Toggle("Linear Space Lighting", useLinear);

                if (EditorGUI.EndChangeCheck())
                {
                    if (useLinear)
                    {
                        material.EnableKeyword("USE_LINEAR");
                        material.DisableKeyword("USE_GAMMA");
                    }
                    else
                    {
                        material.DisableKeyword("USE_LINEAR");
                        material.EnableKeyword("USE_GAMMA");
                    }
                    EditorUtility.SetDirty(this.target);
                }
            }
        }
    }
}
