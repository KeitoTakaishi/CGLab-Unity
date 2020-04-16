Shader "Unlit/Demo04/depth"
{
	SubShader{
		// アルファを使う
		//ZWrite On
		//Blend SrcAlpha OneMinusSrcAlpha

		Pass {
			CGPROGRAM

			// シェーダーモデルは5.0を指定
			#pragma target 5.0

			// シェーダー関数を設定 
			#pragma vertex vert
			#pragma geometry geom
			#pragma fragment frag

			#include "UnityCG.cginc"

			// テクスチャ
			sampler2D _MainTex;

			// 弾の構造体
			struct ParticleData
			{
				float3 pos;
			};

			// 弾の構造化バッファ
			StructuredBuffer<ParticleData> particles;

			
			struct v2g {
				float4 position : POSITION_SV;
			};

			// 頂点シェーダ
			v2g vert(uint id : SV_VertexID)
			{
				// idを元に、弾の情報を取得
				v2g output = (v2g) 0;
				output.position = float4(particles[id].pos, 1.0);

				return output;
			}

			struct g2f
			{
				float4 position : POSITION;
				float  size : TEXCOORD0;
				float2 uv       : TEXCOORD1;
				float3 vpos     : TEXCOORD2;
			};

			static const float3 g_positions[4] =
			{
				float3(-1, 1, 0),
				float3(1, 1, 0),
				float3(-1,-1, 0),
				float3(1,-1, 0),
			};
			// 各頂点のUV座標値
			static const float2 g_texcoords[4] =
			{
				float2(0, 1),
				float2(1, 1),
				float2(0, 0),
				float2(1, 0),
			};
			float particleSize;
			// ジオメトリシェーダ
			[maxvertexcount(4)]
			void geom(point v2g input[1], inout TriangleStream<g2f> outStream)
			{
				g2f o = (g2f)0;
				float3 vertpos = input[0].position.xyz;
				
				[unroll]
				for (int i = 0; i < 4; i++)
				{
					float3 pos = g_positions[i] * particleSize;
					pos = pos + vertpos;
					o.position = UnityObjectToClipPos(float4(pos, 1.0));
					o.uv = g_texcoords[i];
					o.vpos = UnityObjectToViewPos(float4(pos, 1.0)).xyz * float3(1, 1, 1);
					o.size = particleSize;
					outStream.Append(o);
				}
				outStream.RestartStrip();
			}


			struct fragmentOut
			{
				float  depthBuffer : SV_Target0;
				float  depthStencil : SV_Depth;
			};

			
			
			fragmentOut frag(g2f i)
			{
				// 法線を計算
				float3 N = (float3)0;
				
				N.xy = i.uv.xy * 2.0 - 1.0;
				float radius_sq = dot(N.xy, N.xy);
				if (radius_sq > 1.0) discard;
				N.z = sqrt(1.0 - radius_sq);

				// クリップ空間でのピクセルの位置
				float4 viewPos = float4(i.vpos.xyz + N * 1.0, 1.0);
				float4 clipSpacePos = mul(UNITY_MATRIX_P, viewPos);
				// 深度
				float  depth = clipSpacePos.z / clipSpacePos.w; // 正規化

				fragmentOut o = (fragmentOut)0;
				o.depthBuffer = 1000.0 *depth;
				o.depthStencil = depth;

				return o;
			}

			/*
			fixed4 frag(VSOut i) : COLOR
			{
				// 出力はテクスチャカラーと頂点色
				float4 col = tex2D(_MainTex, i.tex) * i.col;

				float2 uv = i.tex.xy * 2.0 - float2(1.0, 1.0);
				float l = dot(uv, uv);
				float th = 0.8;
				if (l > th) {
					discard;
				}
				col = float4(0.0, 0.8, 0.8, 1.0) * smoothstep(0.0 , th , l );
				//float2 uv = i.tex;
				// アルファが一定値以下なら中断
				//if (col.a < 0.3) discard;

				// 色を返す
				return col;
			}
			*/

		

		ENDCG
		}
	
}}
