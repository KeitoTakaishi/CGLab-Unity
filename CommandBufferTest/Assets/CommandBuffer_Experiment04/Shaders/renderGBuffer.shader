Shader "Unlit/Demo04/renderGBuffer"
{
	/*
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
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
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }


			struct gbufferOut
			{
				half4 diffuse  : SV_Target0; // 拡散反射
				half4 specular : SV_Target1; // 鏡面反射
				half4 normal   : SV_Target2; // 法線
				half4 emission : SV_Target3; // 放射光
				float depth : SV_Depth;   // 深度
			};

			sampler2D _DepthBuffer;
            void frag (v2f i, out gbufferOut o)
            {
				o = (gbufferOut)0;
				float d  = tex2D(_DepthBuffer, i.uv).r;
				o.diffuse = half4(d, d, d, 1.0);
				o.specular = half4(0.0, 1.0, 0.0, 1.0);
				o.emission = half4(0.2, 0.2, 0.2, 1.0);
				o.normal = half4(0.0, 0.0, 1.0, 1.0);
				o.depth = tex2D(_DepthBuffer, i.uv).r;
            }
            ENDCG
        }
    }
	*/


	CGINCLUDE
	#include "UnityCG.cginc"

	struct appdata
	{
		float4 vertex : POSITION;
		float2 uv : TEXCOORD0;
	};

	struct v2f
	{
		float4 vertex    : SV_POSITION;
		float4 screenPos : TEXCOORD;
	};

	v2f vert(appdata v)
	{
		v2f o = (v2f)0;
		o.vertex = o.screenPos = v.vertex;
		// 反転した射影行列で現在レンダリングしている場合はY軸について反転
		o.screenPos.y *= _ProjectionParams.x;
		return o;
	}


	// GBufferの構造体
	struct gbufferOut
	{
		half4 diffuse  : SV_Target0; // 拡散反射
		half4 specular : SV_Target1; // 鏡面反射
		half4 normal   : SV_Target2; // 法線
		half4 emission : SV_Target3; // 放射光
		float depth : SV_Depth;   // 深度
	};

	//sampler2D _ColorBuffer; // カラー
	sampler2D _DepthBuffer;
	sampler2D _NormalBuffer;// 法線

	fixed4 _Diffuse;  // 拡散反射光の色
	fixed4 _Specular; // 鏡面反射光の色
	float4 _Emission; // 放射光の色


	void frag(v2f i, out gbufferOut o)
	{
		//????
		float2 uv = i.screenPos.xy * 0.5 + 0.5;
		float  d = tex2D(_DepthBuffer, uv).r;
		float3 n = tex2D(_NormalBuffer, uv).xyz;
		
#if UNITY_REVERSED_Z
		if (Linear01Depth(d) > 1.0 - 1e-3)
			discard;
#else
		if (Linear01Depth(d) < 1e-3)
			discard;
#endif

		//d *= 200.0;
		o.diffuse = _Diffuse;
		o.specular = _Specular;
		o.normal = half4(n.x, n.y, n.z, 1.0);
		o.emission = _Emission;
		o.depth = d;
	}

	ENDCG

		SubShader
	{
		Tags{ "RenderType" = "Opaque" "DisableBatching" = "True" "Queue" = "Geometry+10" }
		//Tags{ "RenderType" = "Opaque" "Queue" = "Geometry+10" }
		Cull Off
		ZWrite On

		Pass
		{
			Tags{ "LightMode" = "Deferred" }

			
			Stencil
			{
				Comp Always
				Pass Replace
				Ref 255
			}
			
			CGPROGRAM
			#pragma target   5.0
			#pragma vertex   vert
			#pragma fragment frag
			#pragma multi_compile ___ UNITY_HDR_ON
			ENDCG
		}
	}
}
