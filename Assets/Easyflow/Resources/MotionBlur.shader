
Shader "Hidden/Easyflow/MotionBlur" {
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
				#pragma exclude_renderers flash
				#include "UnityCG.cginc"			
			
				uniform float4 _MainTex_TexelSize;
				uniform sampler2D _MainTex;
				uniform sampler2D _MotionTex;
				uniform sampler2D _CameraDepthTexture;
				uniform float4 _BlurStep;
			
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
					// 5-TAP
					const half4 zero = half4( 0, 0, 0, 0 );									

					half3 motion = tex2D( _MotionTex, i.uv.zw ).xyz;

					half2 dir_step = _BlurStep.xy * ( motion.xy * 2.0 - 1.0 ) * motion.z;
					half2 dir_step1 = dir_step * 0.5;

					half4 color = tex2D( _MainTex, i.uv.xy );
					half depth = tex2D( _CameraDepthTexture, i.uv.xy ).r;
					half4 accum = half4( color.xyz, 1 );
			
					half ref_depth = depth;
					half ref_id = color.a;

					color = tex2D( _MainTex, i.uv.xy - dir_step );
					depth = tex2D( _CameraDepthTexture, i.uv.xy - dir_step ).r;
					accum += ( color.a == ref_id || depth > ref_depth ) ? half4( color.xyz, 1 ) : zero;
					
					color = tex2D( _MainTex, i.uv.xy - dir_step1 );
					depth = tex2D( _CameraDepthTexture, i.uv.xy - dir_step1 ).r;
					accum += ( color.a == ref_id || depth > ref_depth ) ? half4( color.xyz, 1 ) : zero;
					
					color = tex2D( _MainTex, i.uv.xy + dir_step1 );
					depth = tex2D( _CameraDepthTexture, i.uv.xy + dir_step1 ).r;
					accum += ( color.a == ref_id || depth > ref_depth ) ? half4( color.xyz, 1 ) : zero;
					
					color = tex2D( _MainTex, i.uv.xy + dir_step );
					depth = tex2D( _CameraDepthTexture, i.uv.xy + dir_step ).r;
					accum += ( color.a == ref_id || depth > ref_depth ) ? half4( color.xyz, 1 ) : zero;

					return half4( accum.xyz / accum.w, ref_id );
				}
			ENDCG
		}
	}

	Fallback Off
}