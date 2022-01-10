using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;
using UnityEditor; // TODO

public class DepthReadPass : CustomPass
{
    private Shader outputDepthShader;
    private Material outputDepthMaterial;

    private Texture2D depthTexture = null;
    private Texture2D backgroundTexture = null;

    protected override bool executeInSceneView => false;

    public void LoadDepthTexture(Texture2D depthTex, Texture2D bkgTex)
    {
        depthTexture = depthTex;
        backgroundTexture = bkgTex;
    }

    protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
    {
        outputDepthShader = Shader.Find("Custom/OutputDepth");
        outputDepthMaterial = CoreUtils.CreateEngineMaterial(outputDepthShader);

        Debug.Assert(outputDepthMaterial != null, "Failed to create custom pass material");
    }

    protected override void Execute(CustomPassContext ctx)
    {
        //CoreUtils.ClearRenderTarget(ctx.cmd, ClearFlag.All, Color.black);
        ctx.propertyBlock.SetTexture("_DepthTexture", depthTexture);
        ctx.propertyBlock.SetTexture("_BackgroundTexture", backgroundTexture);
        CoreUtils.SetRenderTarget(ctx.cmd, ctx.cameraColorBuffer, ctx.cameraDepthBuffer, ClearFlag.None);
        CoreUtils.DrawFullScreen(ctx.cmd, outputDepthMaterial, ctx.propertyBlock, shaderPassId: 0);
    }

    protected override void Cleanup()
    {
        CoreUtils.Destroy(outputDepthMaterial);
    }
}
