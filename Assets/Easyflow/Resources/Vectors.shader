
Shader "Hidden/Easyflow/Vectors" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Cutoff ("Alpha cutoff", Range(0,1)) = 0.25
	}

	SubShader {
		Tags { "Queue"="Geometry" "RenderType"="Opaque" }
		Blend Off Fog { Mode off }				
		Pass {
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma fragmentoption ARB_precision_hint_fastest
				#include "UnityCG.cginc"

				struct appdata_t
				{
					float4 vertex : POSITION;
					float2 texcoord1 : TEXCOORD1;
				};

				struct v2f
				{
					float4 pos : POSITION;
					float4 pos_prev : TEXCOORD0;
					float4 pos_curr : TEXCOORD1;
					float4 motion : TEXCOORD2;
				};
				
				uniform float4x4 _EFLOW_PREV_MATRIX_MVP;
				uniform float4x4 _EFLOW_CURR_MATRIX_MVP;
				uniform float _EFLOW_SKINNED;
				uniform float _EFLOW_OBJID;

				v2f vert( appdata_t v )
				{
					v2f o;
					o.pos = mul( UNITY_MATRIX_MVP, v.vertex );
					o.pos_prev = mul( _EFLOW_PREV_MATRIX_MVP, v.vertex );
					o.pos_curr = mul( _EFLOW_CURR_MATRIX_MVP, v.vertex );
				#if SHADER_API_D3D9 || SHADER_API_D3D11 || SHADER_API_D3D11_9X || SHADER_API_XBOX360				
					o.pos_prev.y = -o.pos_prev.y;
					o.pos_curr.y = -o.pos_curr.y;
				#endif
					o.motion = float4( v.texcoord1, 0, 1 );
					return o;
				}

				half4 frag( v2f i ) : COLOR
				{
					// TODO: move this to vert in mobile version
					half4 pos_prev = i.pos_prev / i.pos_prev.w;
					half4 pos_curr = i.pos_curr / i.pos_curr.w;
					half3 motion = lerp( pos_curr - pos_prev, i.motion, _EFLOW_SKINNED ).xyz;
					
					motion.z = length( motion.xy );
					motion.xy = ( motion.xy / motion.z ) * 0.5f + 0.5f;
					
					return saturate( half4( motion.xy, motion.z * 8, _EFLOW_OBJID ) );
				}
			ENDCG
		}
	}

	SubShader {		
		Tags { "Queue"="AlphaTest" "IgnoreProjector"="True" "RenderType"="TransparentCutout" }
		Blend Off Fog { Mode off }		
		Pass {			
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma fragmentoption ARB_precision_hint_fastest
				#include "UnityCG.cginc"

				struct appdata_t
				{
					float4 vertex : POSITION;
					float2 texcoord : TEXCOORD0;
					float2 texcoord1 : TEXCOORD1;
				};

				struct v2f
				{
					float4 pos : POSITION;
					float4 pos_prev : TEXCOORD0;
					float4 pos_curr : TEXCOORD1;
					float4 motion : TEXCOORD2;
					float2 uv : TEXCOORD3;
				};

				sampler2D _MainTex;
				float4 _MainTex_ST;
				float _Cutoff;

				uniform float4x4 _EFLOW_PREV_MATRIX_MVP;
				uniform float4x4 _EFLOW_CURR_MATRIX_MVP;
				uniform float _EFLOW_SKINNED;
				uniform float _EFLOW_OBJID;
				
				v2f vert( appdata_t v )
				{
					v2f o;
					o.pos = mul( UNITY_MATRIX_MVP, v.vertex );
					o.pos_prev = mul( _EFLOW_PREV_MATRIX_MVP, v.vertex );
					o.pos_curr = mul( _EFLOW_CURR_MATRIX_MVP, v.vertex );
				#if SHADER_API_D3D9 || SHADER_API_D3D11 || SHADER_API_D3D11_9X || SHADER_API_XBOX360
					o.pos_prev.y = -o.pos_prev.y;
					o.pos_curr.y = -o.pos_curr.y;
				#endif
					o.motion = float4( v.texcoord1, 0, 1 );
					o.uv = TRANSFORM_TEX( v.texcoord, _MainTex );
					return o;
				}

				half4 frag( v2f i ) : COLOR
				{
					clip( tex2D( _MainTex, i.uv ).a - _Cutoff );

					half4 pos_prev = i.pos_prev / i.pos_prev.w;
					half4 pos_curr = i.pos_curr / i.pos_curr.w;
					half3 motion = lerp( pos_curr - pos_prev, i.motion, _EFLOW_SKINNED ).xyz;
					
					motion.z = length( motion.xy );
					motion.xy = ( motion.xy / motion.z ) * 0.5f + 0.5f;
					
					return saturate( half4( motion.xy, motion.z * 8, _EFLOW_OBJID ) );
				}
			ENDCG
		}		
	}

	FallBack Off
}