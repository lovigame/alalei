
Shader "Hidden/Easyflow/Combine" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_MotionTex ("Motion (RGB)", 2D) = "white" {}
	}
	SubShader {
		Pass {		
			ZTest Always Cull Off ZWrite Off Fog { Mode off }
		
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma fragmentoption ARB_precision_hint_fastest
				#include "UnityCG.cginc"
			
				uniform float4 _MainTex_TexelSize;
				uniform sampler2D _MainTex;
				uniform sampler2D _MotionTex;
			
				struct v2f
				{
					float4 pos : POSITION;
					float4 uv : TEXCOORD0;
				};

				v2f vert( appdata_img v )
				{
					v2f o;
					o.pos = mul( UNITY_MATRIX_MVP, v.vertex );

					o.uv.xy = v.texcoord.xy;
					o.uv.zw = v.texcoord.xy;

				#if SHADER_API_D3D9 || SHADER_API_D3D11 || SHADER_API_D3D11_9X || SHADER_API_XBOX360			
					if ( _MainTex_TexelSize.y < 0 )
						o.uv.w = 1 - o.uv.w;
				#endif

					return o;
				}

				half4 frag( v2f i ) : COLOR
				{
					return half4( tex2D( _MainTex, i.uv.xy ).xyz, tex2D( _MotionTex, i.uv.zw ).a + 0.0000001f ); // hack to trick Unity into behaving
				}
			ENDCG
		}
	}

	Fallback Off
}