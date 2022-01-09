using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PrerenderingEditorWindow : EditorWindow
{
    private PrerenderingManager prerenderingManager;

    [MenuItem("Prerendered backgrounds/Prerendering editor")]
    static void Init()
    {
        PrerenderingEditorWindow window = (PrerenderingEditorWindow)EditorWindow.GetWindow(typeof(PrerenderingEditorWindow));
        window.Show();
    }

    void OnGUI()
    {
        if(prerenderingManager == null)
            prerenderingManager = FindObjectOfType<PrerenderingManager>();

        if(prerenderingManager == null)
        {
            GUILayout.Label("Your scene has no PrerenderingManager. Please add one.");
            return;
        }

        if(GUILayout.Button("Save depth to texture"))
        {
            prerenderingManager.SaveDepthTexture();
        }

        if(GUILayout.Button("Load depth from texture"))
        {
            prerenderingManager.LoadDepthTexture();
        }
    }
}
