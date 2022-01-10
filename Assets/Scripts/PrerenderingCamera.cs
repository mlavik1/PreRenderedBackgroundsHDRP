using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

[RequireComponent(typeof(CustomPassVolume))]
[RequireComponent(typeof(Camera))]
[ExecuteInEditMode]
public class PrerenderingCamera : MonoBehaviour
{
    private CustomPassVolume customPassVolumeCached;
    private Camera cameraCached;

    private RenderTexture depthSaveRT;
    private RenderTexture colourSaveRT;

    private string oldResPath = "";

    public void PreRender()
    {
        CustomPassVolume customPassVolume = GetCustomPassVolume();
        customPassVolume.customPasses?.Clear();
        customPassVolume.injectionPoint = CustomPassInjectionPoint.BeforeTransparent;

        PreRenderPass preRenderPass = (PreRenderPass)customPassVolume.AddPassOfType<PreRenderPass>();
        preRenderPass.PreRenderScene(OnDepthRendered);
    }

    public void BlitPreRenderedTextures()
    {
        CustomPassVolume customPassVolume = GetCustomPassVolume();
        customPassVolume.customPasses?.Clear();
        customPassVolume.injectionPoint = CustomPassInjectionPoint.BeforeRendering;

        byte[] bytes = System.IO.File.ReadAllBytes(ResourcePathUtils.GetDepthTexturePath(this));
        Texture2D depthTexture = new Texture2D(1, 1);
        ImageConversion.LoadImage(depthTexture, bytes);

        bytes = System.IO.File.ReadAllBytes(ResourcePathUtils.GetBackgroundTexturePath(this));
        Texture2D backgroundTexture = new Texture2D(1, 1);
        ImageConversion.LoadImage(backgroundTexture, bytes);

        BlitPass blitPass = (BlitPass)customPassVolume.AddPassOfType<BlitPass>();
        blitPass.BlitTextures(depthTexture, backgroundTexture);
    }

    private void OnDepthRendered(RenderTexture depthRT, RenderTexture colRT)
    {
        depthSaveRT = depthRT;
        colourSaveRT = colRT;
    }

    private void FinaliseDepthSave()
    {
        ResourcePathUtils.EnsureResourceFolderExists(this);
        
        string depthTexturePath = ResourcePathUtils.GetDepthTexturePath(this);
        string backgroundTexturePath = ResourcePathUtils.GetBackgroundTexturePath(this);

        RenderTexture.active = depthSaveRT;
        Texture2D depthTexture = new Texture2D(depthSaveRT.width, depthSaveRT.height, TextureFormat.RGB24, false);
        depthTexture.ReadPixels(new Rect(0, 0, depthSaveRT.width, depthSaveRT.height), 0, 0); 
        RenderTexture.active = null;
        byte[] bytes = depthTexture.EncodeToPNG(); 
        System.IO.File.WriteAllBytes(depthTexturePath, bytes);

        RenderTexture.active = colourSaveRT;
        Texture2D colourTexture = new Texture2D(colourSaveRT.width, colourSaveRT.height, TextureFormat.RGB24, false);
        colourTexture.ReadPixels(new Rect(0, 0, colourSaveRT.width, colourSaveRT.height), 0, 0); 
        RenderTexture.active = null;
        bytes = colourTexture.EncodeToPNG(); 
        System.IO.File.WriteAllBytes(backgroundTexturePath, bytes);

        depthSaveRT = null;

        CustomPassVolume customPassVolume = GetCustomPassVolume();
        customPassVolume.customPasses?.Clear();

        Debug.Log($"Depth texture saved to: {depthTexturePath}");
    }

    private CustomPassVolume GetCustomPassVolume()
    {
        if(customPassVolumeCached == null)
        {
            customPassVolumeCached = GetComponent<CustomPassVolume>();
        }
        return customPassVolumeCached;
    }

    private Camera GetCamera()
    {
        if(cameraCached == null)
        {
            cameraCached = GetComponent<Camera>();
        }
        return cameraCached;
    }

    void Awake()
    {
        CustomPassVolume customPassVolume = GetCustomPassVolume();
        customPassVolume.customPasses?.Clear();
    }

    void Update()
    {
        if(depthSaveRT != null)
            FinaliseDepthSave();

#if UNITY_EDITOR
        // TODO: Only do this on serialisation (and compare with old serialised value)
        string newResPath = ResourcePathUtils.GetCameraResourcesPath(this);
        if (newResPath != oldResPath && oldResPath != "")
        {
            ResourcePathUtils.MoveOldResourceFiles(this, oldResPath);
        }
        oldResPath = newResPath;
#endif
    }
}