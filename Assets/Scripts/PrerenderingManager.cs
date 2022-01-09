using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

[RequireComponent(typeof(CustomPassVolume))]
[ExecuteInEditMode]
public class PrerenderingManager : MonoBehaviour
{
    private string depthTexturePath = "/home/matias/test/img.png"; // TODO
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

        byte[] bytes = System.IO.File.ReadAllBytes(depthTexturePath);
        Texture2D texture = new Texture2D(1, 1);
        ImageConversion.LoadImage(texture, bytes);

        DepthReadPass depthReadPass = (DepthReadPass)customPassVolume.AddPassOfType<DepthReadPass>();
        depthReadPass.LoadDepthTexture(texture);
    }

    private void OnDepthRendered(RenderTexture rt)
    {
        Debug.Log("OnDepthRendered");
        depthSaveRT = rt;
    }

    private void FinaliseDepthSave()
    {
        Debug.Log("FinaliseDepthSave");
        RenderTexture.active = depthSaveRT;
        Texture2D screenShot = new Texture2D(depthSaveRT.width, depthSaveRT.height, TextureFormat.RGB24, false);
        screenShot.ReadPixels(new Rect(0, 0, depthSaveRT.width, depthSaveRT.height), 0, 0); 
        RenderTexture.active = null; // JC: added to avoid errors 
        byte[] bytes = screenShot.EncodeToPNG(); 
        System.IO.File.WriteAllBytes(depthTexturePath, bytes);

        depthSaveRT = null;

        CustomPassVolume customPassVolume = GetCustomPassVolume();
        customPassVolume.customPasses?.Clear();
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