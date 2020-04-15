Shader "Hidden/SPH3DRender"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_Color("color", color) = (0.0, 0.0, 0.0, 1.0)
	}
		SubShader
	{

		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			struct FluidPartcile
			{
				float3 position;
				float3 velocity;
			};

			StructuredBuffer<FluidPartcile> particle;

			v2f vert(appdata v, uint id : SV_VertexID)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(float4(particle[id].position.xyz, 1.0));
				o.uv = v.uv;
				return o;
			}

			sampler2D _MainTex;
			fixed4 _Color;
			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv) * _Color;

				return col;
			}
			ENDCG
		}
	}
}
