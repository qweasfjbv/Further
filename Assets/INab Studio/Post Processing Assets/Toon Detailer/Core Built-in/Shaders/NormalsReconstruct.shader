Shader "Hidden/INab/DetailerBIRP/NormalsReconstruct"
{
    Properties
    {
    }

    CGINCLUDE
        #include "UnityCG.cginc"
       
        struct Attributes
	    {
	    	float4 positionOS : POSITION;
	    	float2 uv : TEXCOORD0;
	    	UNITY_VERTEX_INPUT_INSTANCE_ID
	    };
	    
	    struct Varyings
	    {
	    	float4 positionCS : SV_POSITION;
	    	float2 uv : TEXCOORD0;
	    	UNITY_VERTEX_INPUT_INSTANCE_ID
               UNITY_VERTEX_OUTPUT_STEREO
	    };

	    Varyings vert(Attributes input)
	    {
	    	Varyings output = (Varyings)0;

	    	UNITY_SETUP_INSTANCE_ID(input);
	    	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

	    	float4 positionCS = UnityObjectToClipPos(input.positionOS.xyz);
               output.positionCS = positionCS;

	    	output.uv = input.uv;

	    	return output;
	    }

	    UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
        float4 _CameraDepthTexture_TexelSize;

	   

        // Normals reconstruction by bgolus: https://gist.github.com/bgolus/a07ed65602c009d5e2f753826e8078a0 

        float getRawDepth(float2 uv) 
        { 
            return SAMPLE_DEPTH_TEXTURE_LOD(_CameraDepthTexture, float4(uv, 0.0, 0.0)); 
        }

        // inspired by keijiro's depth inverse projection
        // https://github.com/keijiro/DepthInverseProjection
        // constructs view space ray at the far clip plane from the screen uv
        // then multiplies that ray by the linear 01 depth
        float3 viewSpacePosAtScreenUV(float2 uv)
        {
            float3 viewSpaceRay = mul(unity_CameraInvProjection, float4(uv * 2.0 - 1.0, 1.0, 1.0) * _ProjectionParams.z);
            float rawDepth = getRawDepth(uv);
            return viewSpaceRay * Linear01Depth(rawDepth);
        }
        float3 viewSpacePosAtPixelPosition(float2 vpos)
        {
            float2 uv = vpos * _CameraDepthTexture_TexelSize.xy;
            return viewSpacePosAtScreenUV(uv);
        }
       

        // unity's compiled fragment shader stats: 66 math, 9 tex
         half3 viewNormalAtPixelPosition(float2 vpos)
            {
                // naive 3 tap normal reconstruction

                #ifdef _FAST_MODE
                // get current pixel's view space position
                half3 viewSpacePos_c = viewSpacePosAtPixelPosition(vpos + float2( 0.0, 0.0));

                // get view space position at 1 pixel offsets in each major direction
                half3 viewSpacePos_r = viewSpacePosAtPixelPosition(vpos + float2( 1.0, 0.0));
                half3 viewSpacePos_u = viewSpacePosAtPixelPosition(vpos + float2( 0.0, 1.0));

                // get the difference between the current and each offset position
                half3 hDeriv = viewSpacePos_r - viewSpacePos_c;
                half3 vDeriv = viewSpacePos_u - viewSpacePos_c;

                // get view space normal from the cross product of the diffs
                half3 viewNormal = normalize(cross(hDeriv, vDeriv));

                return viewNormal;

                // base on János Turánszki's Improved Normal Reconstruction
                #endif
                
                #ifndef _FAST_MODE

                // get current pixel's view space position
                half3 viewSpacePos_c = viewSpacePosAtPixelPosition(vpos + float2( 0.0, 0.0));

                // get view space position at 1 pixel offsets in each major direction
                half3 viewSpacePos_l = viewSpacePosAtPixelPosition(vpos + float2(-1.0, 0.0));
                half3 viewSpacePos_r = viewSpacePosAtPixelPosition(vpos + float2( 1.0, 0.0));
                half3 viewSpacePos_d = viewSpacePosAtPixelPosition(vpos + float2( 0.0,-1.0));
                half3 viewSpacePos_u = viewSpacePosAtPixelPosition(vpos + float2( 0.0, 1.0));

                // get the difference between the current and each offset position
                half3 l = viewSpacePos_c - viewSpacePos_l;
                half3 r = viewSpacePos_r - viewSpacePos_c;
                half3 d = viewSpacePos_c - viewSpacePos_d;
                half3 u = viewSpacePos_u - viewSpacePos_c;

                // pick horizontal and vertical diff with the smallest z difference
                half3 hDeriv = abs(l.z) < abs(r.z) ? l : r;
                half3 vDeriv = abs(d.z) < abs(u.z) ? d : u;

                // get view space normal from the cross product of the two smallest offsets
                half3 viewNormal = normalize(cross(hDeriv, vDeriv));

                return viewNormal;

                #endif

            }

        float4 frag (Varyings i) : SV_Target
        {
            // get view space normal at the current pixel position
            half3 viewNormal = viewNormalAtPixelPosition(i.positionCS.xy);

            // transform normal from view space to world space
            //half3 WorldNormal = mul((float3x3)unity_MatrixInvV, viewNormal * half3(1.0, 1.0, -1.0));
            
            // View normals work with cavity
            return float4(viewNormal,1);
        }
    ENDCG

    SubShader
    {
        Cull Off 
        ZWrite Off 
        ZTest Always
		//Blend SrcAlpha OneMinusSrcAlpha
        
        Pass
        {
            Name "NormalsReconstruct"

            CGPROGRAM
            #pragma multi_compile_local _ _FAST_MODE
            #pragma vertex vert
            #pragma fragment frag
			ENDCG    
        }
    }
}
