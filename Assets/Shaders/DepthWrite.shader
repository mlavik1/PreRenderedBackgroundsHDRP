Shader "Custom/DepthWrite"
{
    HLSLINCLUDE

    #pragma vertex Vert

    #pragma target 4.5
    #pragma only_renderers d3d11 playstation xboxone xboxseries vulkan metal switch

    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/RenderPass/CustomPass/CustomPassCommon.hlsl"

    TEXTURE2D_X(_ImmediateTexture);

    float4 FullScreenPass(Varyings varyings) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(varyings);

        float depth = LoadCameraDepth(varyings.positionCS.xy);
        PositionInputs posInput = GetPositionInput(varyings.positionCS.xy, _ScreenSize.zw, depth, UNITY_MATRIX_I_VP, UNITY_MATRIX_V);
        
        // Get fragment colour of immediate texture (if not black => pixelate)
        float2 uv = posInput.positionNDC.xy * _RTHandleScale.xy;
        float4 immediateColour = SAMPLE_TEXTURE2D_X_LOD(_ImmediateTexture, s_linear_clamp_sampler, uv, 0);
        float pixelation = immediateColour.r;

        // Pixelate
        float x = floor(varyings.positionCS.x) - (varyings.positionCS.x % 5.0);
        float y = floor(varyings.positionCS.y) - (varyings.positionCS.y % 5.0);
        varyings.positionCS.xy = float2(x, y) * pixelation + varyings.positionCS * (1.0 - pixelation);

        posInput = GetPositionInput(varyings.positionCS.xy, _ScreenSize.zw, depth, UNITY_MATRIX_I_VP, UNITY_MATRIX_V);
        float4 color = float4(0.0, 0.0, 0.0, 0.0);

        // Load the camera color buffer at the mip 0 if we're not at the before rendering injection point
        if (_CustomPassInjectionPoint != CUSTOMPASSINJECTIONPOINT_BEFORE_RENDERING)
            color = float4(CustomPassLoadCameraColor(varyings.positionCS.xy, 0), 1);

        color.rgb = immediateColour.rgb;

        return float4(color.rgb, color.a);
    }

    ENDHLSL

    SubShader
    {
        Pass
        {
            Name "Custom Pass 0"

            ZWrite Off
            ZTest Always
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off

            HLSLPROGRAM
                #pragma fragment FullScreenPass
            ENDHLSL
        }
    }
    Fallback Off
}
