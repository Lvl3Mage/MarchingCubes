Shader "Unlit/TerrainShader"
{
	Properties
	{
		_LightVector ("_LightVector", Vector) = (1,1,1)
		_TopColor ("TopColor", Color ) = (1,1,1,1)
		_MiddleColor ("BottomColor", Color) = (0,0,0,0)
		_BottomColor ("BottomColor", Color) = (0,0,0,0)

		_Offset ("Offset", Float) = 0
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct MeshData
			{
				float4 vertex : POSITION;
				float3 normals : NORMAL;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 color : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};
			float _Offset;
			float4 _BottomColor;
			float4 _MiddleColor;
			float4 _TopColor;
			float3 _LightVector;
			v2f vert (MeshData v)
			{
				v2f o;

				float normalAllignment = saturate((dot(v.normals, float3(0,1,0))+1)/2) - _Offset;
				float topLerp = saturate(normalAllignment*2-1);
				float bottomLerp = saturate(normalAllignment*2);

				float lightAllignment = saturate((dot(_LightVector, v.normals)+1)/2);
				o.vertex = UnityObjectToClipPos(v.vertex);
				float4 TopMiddleLerp = lerp(_MiddleColor, _TopColor, topLerp);
				o.color = lerp(_BottomColor, TopMiddleLerp, bottomLerp) * lightAllignment;
				return o;
			}


			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				float4 col = i.color;
				return col;
			}
			ENDCG
		}
	}
}
