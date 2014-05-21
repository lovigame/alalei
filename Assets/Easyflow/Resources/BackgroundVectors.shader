
Shader "Hidden/Easyflow/BackgroundVectors" {
	Properties {
	}
	SubShader {
		Tags { "Queue"="Background" "RenderType"="Background" }
		Cull Off ZWrite Off Fog { Mode Off }
		Pass {
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma fragmentoption ARB_precision_hint_fastest
				#include "UnityCG.cginc"

				struct v2f
				{
					float4 pos : POSITION;
					float4 pos_prev : TEXCOORD0;
					float4 pos_curr : TEXCOORD1;
				};
				
				uniform float4x4 _EFLOW_PREV_MATRIX_MVP;
				uniform float4x4 _EFLOW_CURR_MATRIX_MVP;
				
				v2f vert( float4 vertex : POSITION )
				{
					v2f o;
					o.pos = mul( UNITY_MATRIX_MVP, vertex );
					o.pos_prev = mul( _EFLOW_PREV_MATRIX_MVP, vertex );
					o.pos_curr = mul( _EFLOW_CURR_MATRIX_MVP, vertex );
				#if SHADER_API_D3D9 || SHADER_API_D3D11 || SHADER_API_D3D11_9X || SHADER_API_XBOX360
					o.pos_prev.y = -o.pos_prev.y;
					o.pos_curr.y = -o.pos_curr.y;
				#endif
					return o;
				}

				half4 frag( v2f i ) : COLOR
				{	
					half4 pos_prev = i.pos_prev / i.pos_prev.w;
					half4 pos_curr = i.pos_curr / i.pos_curr.w;
					half3 motion = pos_curr - pos_prev;
					
					motion.z = length( motion.xy );
					motion.xy = ( motion.xy / motion.z ) * 0.5f + 0.5f;					
					
					return saturate( half4( motion.xy, motion.z * 8, 0 ) );
				}
			ENDCG
		}
	}
	FallBack Off
}