using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PrerenderingCamera))]
public class PrerenderingCameraEditor : Editor
{
    private PrerenderingCamera prerendCam;

    public override void OnInspectorGUI()
    {
        prerendCam = (PrerenderingCamera)target;

        if(GUILayout.Button("Pre-render scene and save"))
        {
            prerendCam.PreRender();
        }

        if(GUILayout.Button("Load pre-rendered scene"))
        {
            prerendCam.BlitPreRenderedTextures();
        }
    }
}
