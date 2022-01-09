using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;
using UnityEngine.Events;

public class DepthSavePass : CustomPass
{
    private Shader depthWriteShader;
    private Material depthWriteMaterial;

    private bool saveRequested = false;
    private UnityAction<RenderTexture> saveCompletedCallback;

    private RenderTexture depthSaveRT = null;

    protected override bool executeInSceneView => false;
    
    public void SaveDepthTexture(UnityAction<RenderTexture> onCompleted)
    {
        saveRequested = true;
        saveCompletedCallback = onCompleted;
    }

    protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
    {
        depthWriteShader = Shader.Find("Custom/DepthWrite");
        depthWriteMaterial = CoreUtils.CreateEngineMaterial(depthWriteShader);

        Debug.Assert(depthWriteMaterial != null, "Failed to create custom pass material");

        int resWidth = Camera.main.pixelWidth;
        int resHeight = Camera.main.pixelHeight;
        depthSaveRT = new RenderTexture(resWidth, resHeight, 24);
    }

    protected override void Execute(CustomPassContext ctx)
    {
        ctx.propertyBlock.SetTexture("_ImmediateTexture", ctx.cameraDepthBuffer);
        CoreUtils.SetRenderTarget(ctx.cmd, depthSaveRT, ClearFlag.None);
        CoreUtils.DrawFullScreen(ctx.cmd, depthWriteMaterial, ctx.propertyBlock, shaderPassId: 0);

        if(saveCompletedCallback != null)
        {
            saveCompletedCallback(depthSaveRT);
            saveCompletedCallback = null;
        }
    }

    protected override void Cleanup()
    {
        CoreUtils.Destroy(depthWriteMaterial);
    }
}
