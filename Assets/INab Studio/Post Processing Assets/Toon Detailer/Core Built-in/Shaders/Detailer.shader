Shader "Hidden/INab/DetailerBIRP"
{
     Properties{
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

      // General
      
       sampler2D _MainTex;
	   float4 _MainTex_TexelSize;

	   sampler2D _ReconstructedNormals;
	   half4 _ReconstructedNormals_ST;

       sampler2D _CameraDepthTexture;
	   half4 _CameraDepthTexture_ST;

       // Adjustments

       uniform float4 _ColorHue;
       uniform float _FadeStart;
       uniform float _FadeEnd;
       uniform float _BlackOffset;

       // Countours

       uniform float _ContoursIntensity;
       uniform float _ContoursThickness;
       uniform float _ContoursElevationStrength;
       uniform float _ContoursElevationSmoothness;
       uniform float _ContoursDepressionStrength;
       uniform float _ContoursDepressionSmoothness;

       // Cavity

       uniform float _CavityIntensity;
       uniform float _CavityRadius;
       uniform float _CavityStrength;
       uniform int _CavitySamples;

      // Static
       
       static const float kContrast = 0.6;
       #define TWO_PI          6.28318530718
       #define ATTENUATION      0.015625
       static const float kBeta = 0.002;


       static const float SSAORandomUV[40] =
       {
            0.00000000,
            0.33984375,
            0.75390625,
            0.56640625,
            0.98437500,
            0.07421875,
            0.23828125,
            0.64062500,
            0.35937500,
            0.50781250,
            0.38281250,
            0.98437500,
            0.17578125,
            0.53906250,
            0.28515625,
            0.23137260,
            0.45882360,
            0.54117650,
            0.12941180,
            0.64313730,
        
            0.92968750,
            0.76171875,
            0.13333330,
            0.01562500,
            0.00000000,
            0.10546875,
            0.64062500,
            0.74609375,
            0.67968750,
            0.35156250,
            0.49218750,
            0.12500000,
            0.26562500,
            0.62500000,
            0.44531250,
            0.17647060,
            0.44705890,
            0.93333340,
            0.87058830,
            0.56862750,
       };
       
       float2 CosSin(float theta)
       {
            float sn, cs;
            sincos(theta, sn, cs);
            return float2(cs, sn);
       }
       
       float InterleavedGradientNoise(float2 pixCoord, int frameCount)
       {
            const float3 magic = float3(0.06711056f, 0.00583715f, 52.9829189f);
            float2 frameMagicScale = float2(2.083f, 4.867f);
            pixCoord += frameCount * frameMagicScale;
            return frac(magic.z * frac(dot(pixCoord, magic.xy)));
       }

       float3 PickSamplePoint(float2 uv, float randAddon, int index)
       {
            float2 positionSS = uv;
            float gn = InterleavedGradientNoise(positionSS, index);
            float u = frac(gn) * 2.0 - 1.0;
            float theta = gn * TWO_PI;
            return float3(CosSin(theta) * sqrt(1.0 - u * u), u);
       }
       
       float RawToLinearDepth(float rawDepth)
       {
            #if defined(_ORTHOGRAPHIC)
                #if UNITY_REVERSED_Z
                    return ((_ProjectionParams.z - _ProjectionParams.y) * (1.0 - rawDepth) + _ProjectionParams.y);
                #else
                    return ((_ProjectionParams.z - _ProjectionParams.y) * (rawDepth) + _ProjectionParams.y);
                #endif
            #else
                return LinearEyeDepth(rawDepth);
            #endif
       }
       
       float GetLinearDepth(float2 uv)
       {
            float rawDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, UnityStereoScreenSpaceUVAdjust(uv, _CameraDepthTexture_ST)).r;
            return RawToLinearDepth(rawDepth);
       }

       float3 GetNormals(float2 uv)
       {          
           return tex2D(_ReconstructedNormals,uv).rgb;
       }

       float3 ReconstructViewPos(float2 uv, float depth, float2 p11_22, float2 p13_31)
       {
            #if defined(_ORTHOGRAPHIC)
                float3 viewPos = float3(((uv.xy * 2.0 - 1.0 - p13_31) * p11_22), depth);
            #else
                float3 viewPos = float3(depth * ((uv.xy * 2.0 - 1.0 - p13_31) * p11_22), depth);
            #endif
            return viewPos;
       }

       void GetDepthNormalView(float2 uv, float2 p11_22, float2 p13_31, out float depth, out float3 normal, out float3 vpos)
       {
            depth  = GetLinearDepth(uv);
            vpos = ReconstructViewPos(uv, depth, p11_22, p13_31);      
            normal = GetNormals(uv);
       }

       float3x3 GetCoordinateConversionParameters(out float2 p11_22, out float2 p13_31)
       {
            float3x3 camProj = (float3x3)unity_CameraProjection;
        
            p11_22 = rcp(float2(camProj._11, camProj._22));
            p13_31 = float2(camProj._13, camProj._23);
        
            return camProj;
       }
       
       float ContoursAdjust(float value, float strength, float smoothness)
       {
           value = clamp(value,0,smoothness);

           return value * strength;
       }
       
       float Contours(float2 uv)
       {
           float3 contourOffset = float3(_MainTex_TexelSize.x, _MainTex_TexelSize.y, 0.0) * _ContoursThickness;

           if(uv.y - contourOffset.y < 0.001f || uv.x - contourOffset.x < 0.001f)
           {
               return 0.0f; 
           }

           float upperNormal = GetNormals(uv + contourOffset.zy).g;
           float lowerNormal = GetNormals(uv - contourOffset.zy).g;

           float rightNormal = GetNormals(uv + contourOffset.xz).r;
           float leftNormal  = GetNormals(uv - contourOffset.xz).r;

           float greenDifference = upperNormal - lowerNormal;

           float redDifference = rightNormal - leftNormal;

           float combinedValue = greenDifference + redDifference;

           if (combinedValue >= 0.0f)
           {
               return ContoursAdjust(combinedValue, _ContoursElevationStrength, _ContoursElevationSmoothness);
           }
           else
           {
               return -ContoursAdjust(-combinedValue, _ContoursDepressionStrength, _ContoursDepressionSmoothness);
           }
       }
       
       float Cavity(float2 uv, float2 positionCS)
        {
            float sumDark = 0.0;
            float sumBright = 0.0;

            float2 aCoeff, bCoeff;
            float3x3 conversionMatrix = GetCoordinateConversionParameters(aCoeff, bCoeff);

            float depthVal;
            float3 normVal;
            float3 viewPosition;
            GetDepthNormalView(uv, aCoeff, bCoeff, depthVal, normVal, viewPosition);

            float noiseFactor = uv.x * 1e-10;
            float reciprocalSampleCount = rcp(_CavitySamples);

            UNITY_LOOP
            for (int counter = 0; counter < _CavitySamples; counter++)
            {
                float3 samplePoint = PickSamplePoint(positionCS, noiseFactor, counter);
                samplePoint *= sqrt((counter + 1.0) * reciprocalSampleCount) * _CavityRadius * 0.5;
                float3 shiftedPos = viewPosition + samplePoint;

                float3 projectedPos = mul(conversionMatrix, shiftedPos);
                float2 sampleUV;
                #if defined(_ORTHOGRAPHIC)
                    sampleUV = clamp((projectedPos.xy + 1.0) * 0.5, 0.0, 1.0);
                #else
                    sampleUV = clamp((projectedPos.xy / shiftedPos.z + 1.0) * 0.5, 0.0, 1.0);
                #endif

                float sampleDepth = GetLinearDepth(sampleUV);
                float3 reconstructedPos = ReconstructViewPos(sampleUV, sampleDepth, aCoeff, bCoeff);
                float3 directionVec = reconstructedPos - viewPosition;
                float magnitude = length(directionVec);
                float dotProduct = dot(directionVec, normVal);

                dotProduct = -dotProduct;

                float negValue = dotProduct - kBeta * depthVal;
                float posValue = -dotProduct - kBeta * depthVal;

                float biasTerm = 0.05 * magnitude + 0.0001;
                float attenuate = 1.0 / (magnitude * (1.0 + magnitude * magnitude * ATTENUATION));

                if (negValue > -biasTerm)
                {
                    sumDark += negValue * attenuate;
                }

                if (posValue > biasTerm)
                {
                    sumBright += posValue * attenuate;
                }
            }

            sumDark *= reciprocalSampleCount;
            sumBright *= reciprocalSampleCount;

            sumDark = pow(sumDark * reciprocalSampleCount, kContrast);
            sumBright = pow(sumBright * reciprocalSampleCount, kContrast);

            sumDark = saturate(sumDark * _CavityStrength);
            sumBright *= _CavityStrength;

            return (1.0 - sumDark) * (1.0 + sumBright);
        }
       
       float DepthFade(float2 uv)
       {
           float fadeValue = max(RawToLinearDepth(tex2D(_CameraDepthTexture, uv).r)-_FadeStart,0) / _FadeEnd;
           fadeValue = pow(fadeValue,2);
           return saturate(smoothstep(0,1,fadeValue));
       }

       float CombineDetails(float2 uv,float2 positionCS)
       {
            float cavity = 1.0;
            float contours = 0.0;
        
            #ifdef _USE_CONTOURS
            contours =  Contours(uv);
            #endif
            
            #ifdef _USE_CAVITY
            cavity = Cavity(uv,positionCS);
            #endif
                   
            #ifdef _FADE_COUNTOURS_ONLY
            contours = lerp (contours,0,DepthFade(uv));
            #endif

            cavity = lerp(1, cavity, _CavityIntensity);
        
            contours = lerp(0, contours, _ContoursIntensity);
            
            float value = clamp(cavity * (1.0 + contours),0,4);
        
            return value;
       }
       
       float4 Detailer_PostProcess(Varyings input) : SV_Target
       {
          UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
          
          float2 uv = UnityStereoTransformScreenSpaceTex(input.uv);
          float value = CombineDetails(uv, input.positionCS);
          float4 color = tex2D(_MainTex, uv);

          #ifdef _FADE_ON
          value = lerp (value,1,DepthFade(uv));
          #endif
         
          // Dark offset
          value = clamp(value,_BlackOffset,4);

          // Color hue
          color = lerp(color* _ColorHue,color,saturate(value));

          return float4(color.rgb * value, 1);
       }
       

    ENDCG

    SubShader
    {
        Cull Off 
        ZWrite Off 
        ZTest Always

         Pass
        {
            Name "Detailer_PostProcess_NoStencil"

            CGPROGRAM
            #pragma multi_compile_local _ _USE_CONTOURS
            #pragma multi_compile_local _ _USE_CAVITY
            #pragma multi_compile_local _ _ORTHOGRAPHIC
            #pragma multi_compile_local _ _FADE_COUNTOURS_ONLY
            #pragma multi_compile_local _ _FADE_ON
            #pragma vertex vert
            #pragma fragment Detailer_PostProcess
			ENDCG    
        }

        Pass
        {
            Name "Detailer_PostProcess_StencilNotEqual"
            
            Stencil
            {
                Ref 1
                Comp NotEqual
            }  

            CGPROGRAM
            #pragma multi_compile_local _ _USE_CONTOURS
            #pragma multi_compile_local _ _USE_CAVITY
            #pragma multi_compile_local _ _ORTHOGRAPHIC
            #pragma multi_compile_local _ _FADE_COUNTOURS_ONLY
            #pragma multi_compile_local _ _FADE_ON
            #pragma vertex vert
            #pragma fragment Detailer_PostProcess
			ENDCG    
        }

        Pass
        {
            Name "Detailer_PostProcess_StencilEqual"
            
             Stencil
             {
                 Ref 1
                 Comp Equal
             }  

            CGPROGRAM
            #pragma multi_compile_local _ _USE_CONTOURS
            #pragma multi_compile_local _ _USE_CAVITY
            #pragma multi_compile_local _ _ORTHOGRAPHIC
            #pragma multi_compile_local _ _FADE_COUNTOURS_ONLY
            #pragma multi_compile_local _ _FADE_ON
            #pragma vertex vert
            #pragma fragment Detailer_PostProcess
			ENDCG    
        }
    }
}

