using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;
using UnityEditor; // TODO

public class BlitPass : CustomPass
{
    private Shader shader;
    private Material material;

    private Texture2D depthTexture = null;
    private Texture2D backgroundTexture = null;

    protected override bool executeInSceneView => false;

    public void BlitTextures(Texture2D depthTex, Texture2D bkgTex)
    {
        depthTexture = depthTex;
        backgroundTexture = bkgTex;
    }

    protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
    {
        shader = Shader.Find("Custom/BlitPreRendered");
        material = CoreUtils.CreateEngineMaterial(shader);

        Debug.Assert(material != null, "Failed to create custom pass material");
    }

    protected override void Execute(CustomPassContext ctx)
    {
        ctx.propertyBlock.SetTexture("_DepthTexture", depthTexture);
        ctx.propertyBlock.SetTexture("_BackgroundTexture", backgroundTexture);
        CoreUtils.SetRenderTarget(ctx.cmd, ctx.cameraColorBuffer, ctx.cameraDepthBuffer, ClearFlag.None);
        //CoreUtils.ClearRenderTarget(ctx.cmd, ClearFlag.All, Color.black);
        CoreUtils.DrawFullScreen(ctx.cmd, material, ctx.propertyBlock, shaderPassId: 0);
    }

    protected override void Cleanup()
    {
        CoreUtils.Destroy(material);
    }
}
