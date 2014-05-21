
Shader "Hidden/Easyflow/ClothVectorsMobile" {
	Properties {
	}
	SubShader {
		Tags { "Queue"="Geometry" "RenderType"="Opaque" }
		Blend Off Fog { Mode off }
		Offset -1, -1
		Pass {
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma fragmentoption ARB_precision_hint_fastest
				#include "UnityCG.cginc"

				struct appdata_t
				{
					float4 vertex : POSITION;
					float4 texcoord : TEXCOORD0;
				};

				struct v2f
				{
					float4 pos : POSITION;
					float4 motion : TEXCOORD0;
				};
				
				uniform float _EFLOW_OBJID;
				
				v2f vert( appdata_t v )
				{
					v2f o;
					o.pos = mul( UNITY_MATRIX_MVP, v.vertex );
					
					float3 motion = v.texcoord.xyz;
					motion.z = length( motion.xy );
					motion.xy = ( motion.xy / motion.z ) * 0.5f + 0.5f;
					
					o.motion = saturate( float4( motion.xy, motion.z * 8, _EFLOW_OBJID ) );
					return o;
				}

				half4 frag( v2f i ) : COLOR
				{	
					return i.motion;
				}
			ENDCG
		}
	}
	FallBack Off
}
