Shader "Hidden/effect"
{
    Properties
    {
        _MainTex ("", 2D) = "white" {}
    }

		CGINCLUDE
		#include "UnityCG.cginc"
		sampler2D _MainTex;
		sampler2D _tex;
		fixed4 _MainTex_TexelSize;

		fixed4 frag(v2f_img i) : SV_Target
		{
			float2 uv = i.uv;
			fixed4 col;
			//uv = _MainTex_TexelSize.xy;
			col = tex2D(_MainTex, uv);
			//col = tex2D(_MainTex, uv);

			fixed4 ts = _MainTex_TexelSize;

			col = fixed4(ts.x *652.0, 0.0, 0.0, 1.0);
			return col;
		}
		ENDCG

		SubShader
		{
			Cull Off ZWrite Off ZTest Always

				Pass
			{
				CGPROGRAM
				#pragma vertex   vert_img
				#pragma fragment frag
				ENDCG
			}
		}


         
}
