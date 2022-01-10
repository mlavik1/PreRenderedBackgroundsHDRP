using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

[RequireComponent(typeof(CustomPassVolume))]
[ExecuteInEditMode]
public class PrerenderingManager : MonoBehaviour
{
    private CustomPassVolume customPassVolumeCached;

    private RenderTexture depthSaveRT;

    public void SaveDepthTexture()
    {
        CustomPassVolume customPassVolume = GetCustomPassVolume();
        customPassVolume.customPasses?.Clear();
        customPassVolume.injectionPoint = CustomPassInjectionPoint.BeforeTransparent;

        DepthSavePass depthSavePass = (DepthSavePass)customPassVolume.AddPassOfType<DepthSavePass>();
        depthSavePass.SaveDepthTexture(OnDepthRendered);
    }

    public void LoadDepthTexture()
    {
        CustomPassVolume customPassVolume = GetCustomPassVolume();
        customPassVolume.customPasses?.Clear();
        customPassVolume.injectionPoint = CustomPassInjectionPoint.BeforeRendering;

        byte[] bytes = System.IO.File.ReadAllBytes(GetDepthTexturePath());
        Texture2D depthTexture = new Texture2D(1, 1);
        ImageConversion.LoadImage(depthTexture, bytes);

        bytes = System.IO.File.ReadAllBytes(GetBackgroundTexturePath());
        Texture2D backgroundTexture = new Texture2D(1, 1);
        ImageConversion.LoadImage(backgroundTexture, bytes);

        DepthReadPass depthReadPass = (DepthReadPass)customPassVolume.AddPassOfType<DepthReadPass>();
        depthReadPass.LoadDepthTexture(depthTexture, backgroundTexture);
    }

    public string GetDepthTexturePath()
    {
        return System.IO.Path.Combine(Application.streamingAssetsPath, "prerendered-depth.png");
    }

    public string GetBackgroundTexturePath()
    {
        return System.IO.Path.Combine(Application.streamingAssetsPath, "background.png");
    }

    private void OnDepthRendered(RenderTexture rt)
    {
        depthSaveRT = rt;
    }

    private void FinaliseDepthSave()
    {
        RenderTexture.active = depthSaveRT;
        Texture2D screenShot = new Texture2D(depthSaveRT.width, depthSaveRT.height, TextureFormat.RGB24, false);
        screenShot.ReadPixels(new Rect(0, 0, depthSaveRT.width, depthSaveRT.height), 0, 0); 
        RenderTexture.active = null; // JC: added to avoid errors 
        byte[] bytes = screenShot.EncodeToPNG(); 
        System.IO.File.WriteAllBytes(GetDepthTexturePath(), bytes);

        depthSaveRT = null;

        CustomPassVolume customPassVolume = GetCustomPassVolume();
        customPassVolume.customPasses?.Clear();

        Debug.Log($"Depth texture saved to: {GetDepthTexturePath()}");
    }

    private CustomPassVolume GetCustomPassVolume()
    {
        if(customPassVolumeCached == null)
        {
            customPassVolumeCached = GetComponent<CustomPassVolume>();
        }
        return customPassVolumeCached;
    }

    void Update()
    {
        if(depthSaveRT != null)
            FinaliseDepthSave();
    }
}
