Shader "Unlit/TerrainShader"
{
	Properties
	{
		_LightVector ("_LightVector", Vector) = (1,1,1)
		_Color1 ("Color 1", Color ) = (1,1,1,1)
		_Color2 ("Color 2", Color) = (0,0,0,0)
		_Color3 ("Color 3", Color) = (0,0,0,0)
		_Color4 ("Color 4", Color) = (0,0,0,0)
		_NormalOffset ("NormalOffset", Float) = 0
		_PositionOffset ("PositionOffset", Vector) = (0,0,0)
		_LoopTime ("Loop Time", Float) = 0

		// _Offset ("Offset", Float) = 0
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
			float3 _LightVector;
			// float _Offset;
			float4 _Color1;
			float4 _Color2;
			float4 _Color3;
			float4 _Color4;
			float _LoopTime;
			float3 _PositionOffset;
			float _NormalOffset;
			float4 GetColor(int time)
			{
				float4 color;
				switch(time)
				{
					case 0:
						color = _Color1;
						break;
					case 1:
						color = _Color2;
						break;
					case 2:
						color = _Color3;
						break;
					case 3:
						color = _Color4;
						break;
				}
				return color;
			}
			v2f vert (MeshData v)
			{
				v2f o;
				float time = _Time.y*4/_LoopTime;

				float normalAllignment = saturate((dot(v.normals, float3(0,1,0))+1)/2);

				time += normalAllignment*_NormalOffset;
				time += v.vertex.x*_PositionOffset.x + v.vertex.y*_PositionOffset.y + v.vertex.z*_PositionOffset.z;
				float4 curColor = GetColor(floor(time % 4));
				float4 nextColor = GetColor(floor((time+1) % 4));


				
				float4 col = lerp(curColor, nextColor, time % 1);

				float lightAllignment = saturate((dot(_LightVector, v.normals)+1)/2);

				o.vertex = UnityObjectToClipPos(v.vertex);
				o.color = col*lerp(0.2, 1, lightAllignment);
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
		Pass
        {
            Tags{ "LightMode" = "ShadowCaster" }
            CGPROGRAM
            #pragma vertex VSMain
            #pragma fragment PSMain
 
            float4 VSMain (float4 vertex:POSITION) : SV_POSITION
            {
                return UnityObjectToClipPos(vertex);
            }
 
            float4 PSMain (float4 vertex:SV_POSITION) : SV_TARGET
            {
                return 0;
            }
           
            ENDCG
        }
	}
}
