Shader "Hidden/INab/DetailerBIRP/Stencil"
{
   Properties
	{
		[HideInInspector] __dirty( "", Int ) = 1
	}

		SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Back
		Stencil
		{
			 Ref 1
             Comp Always
             Pass Replace
		}

		CGINCLUDE
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		struct Input
		{
			half filler;
		};

		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			float4 _Color0 = float4(0,0,0,0);

			o.Emission = _Color0.rgb;
			o.Alpha = _Color0.a;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Unlit alpha:fade keepalpha fullforwardshadows noambient novertexlights nolightmap  nodynlightmap nodirlightmap nofog nometa noforwardadd 
		ENDCG
		
	}
	Fallback "Diffuse"
}
