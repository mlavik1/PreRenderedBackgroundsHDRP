using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;
using UnityEngine.Events;

/// <summary>
/// CustomPass responsible for pre-remdering the background scene.
/// It will render the colour and depth to two separate render targets.
/// </summary>
public class PreRenderPass : CustomPass
{
    private Shader shader;
    private Material material;

    private bool saveRequested = false;
    private UnityAction<RenderTexture, RenderTexture> saveCompletedCallback;

    private RenderTexture depthRenderTarget = null;
    private RenderTexture colourRenderTarget = null;

    protected override bool executeInSceneView => false;
    
    public void PreRenderScene(UnityAction<RenderTexture, RenderTexture> onCompleted)
    {
        saveRequested = true;
        saveCompletedCallback = onCompleted;
    }

    protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
    {
        shader = Shader.Find("Custom/BlitToTexture");
        material = CoreUtils.CreateEngineMaterial(shader);

        Debug.Assert(material != null, "Failed to create custom pass material");

        int resWidth = Screen.width;
        int resHeight = Screen.height;
        depthRenderTarget = new RenderTexture(resWidth, resHeight, 24);
        colourRenderTarget = new RenderTexture(resWidth, resHeight, 24);
    }

    protected override void Execute(CustomPassContext ctx)
    {
        // Pre-render scene depth.
        ctx.propertyBlock.SetTexture("_MainTexture", ctx.cameraDepthBuffer);
        CoreUtils.SetRenderTarget(ctx.cmd, depthRenderTarget, ClearFlag.Depth);
        CoreUtils.DrawFullScreen(ctx.cmd, material, ctx.propertyBlock, shaderPassId: 0);

        // Pre-render scene colour.
        ctx.propertyBlock.SetTexture("_MainTexture", ctx.cameraColorBuffer);
        CoreUtils.SetRenderTarget(ctx.cmd, colourRenderTarget, ClearFlag.Color);
        CoreUtils.DrawFullScreen(ctx.cmd, material, ctx.propertyBlock, shaderPassId: 0);

        if(saveCompletedCallback != null)
        {
            saveCompletedCallback(depthRenderTarget, colourRenderTarget);
            saveCompletedCallback = null;
        }
    }

    protected override void Cleanup()
    {
        CoreUtils.Destroy(material);
    }
}
