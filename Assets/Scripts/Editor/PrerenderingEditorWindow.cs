using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PrerenderingEditorWindow : EditorWindow
{
    private PrerenderingCamera prerendCam;

    [MenuItem("Prerendered backgrounds/Prerendering editor")]
    static void Init()
    {
        PrerenderingEditorWindow window = (PrerenderingEditorWindow)EditorWindow.GetWindow(typeof(PrerenderingEditorWindow));
        window.Show();
    }

    void OnGUI()
    {
        if(prerendCam == null)
            prerendCam = FindObjectOfType<PrerenderingCamera>();

        if(prerendCam == null)
        {
            GUILayout.Label("Your scene has no PrerenderingCamera. Please add one.");
            return;
        }

        if(GUILayout.Button("Save depth to texture"))
        {
            prerendCam.PreRender();
        }

        if(GUILayout.Button("Load depth from texture"))
        {
            prerendCam.BlitPreRenderedTextures();
        }
    }
}
