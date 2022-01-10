Shader "Custom/BlitPreRendered"
{
    HLSLINCLUDE

    #pragma vertex Vert

    #pragma target 4.5
    #pragma only_renderers d3d11 ps4 xboxone vulkan metal switch

    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/RenderPass/CustomPass/CustomPassCommon.hlsl"

    TEXTURE2D(_DepthTexture);
    TEXTURE2D(_BackgroundTexture);

    float4 FullScreenPass(Varyings varyings, out float outputDepth : SV_Depth) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(varyings);

        float depth = LoadCameraDepth(varyings.positionCS.xy);
        PositionInputs posInput = GetPositionInput(varyings.positionCS.xy, _ScreenSize.zw, depth, UNITY_MATRIX_I_VP, UNITY_MATRIX_V);
        
        float2 uv = posInput.positionNDC.xy * _RTHandleScale.xy;
        float4 depthColour = SAMPLE_TEXTURE2D_X_LOD(_DepthTexture, s_linear_clamp_sampler, uv, 0);
        float4 backgroundColour = SAMPLE_TEXTURE2D_X_LOD(_BackgroundTexture, s_linear_clamp_sampler, uv, 0);

        outputDepth = depthColour.r;
        return float4(backgroundColour.r, backgroundColour.g, backgroundColour.b, 1);
    }

    ENDHLSL

    SubShader
    {
        Pass
        {
            Name "Custom Pass 0"

            ZWrite On
            ZTest Always
            Blend Off
            Cull Off

            HLSLPROGRAM
                #pragma fragment FullScreenPass
            ENDHLSL
        }
    }
    Fallback Off
}
