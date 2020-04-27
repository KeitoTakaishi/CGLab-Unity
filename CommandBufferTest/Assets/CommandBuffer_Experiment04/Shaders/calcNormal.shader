Shader "Hidden/calcNormal"
{
    Properties
    {
        _MainTex ("", 2D) = "black" {}
    }
   
    CGINCLUDE
	#include "UnityCG.cginc"
	sampler2D _DepthBuffer;
	float4    _DepthBuffer_TexelSize;
	float4x4 _ViewMatrix;

	float3 uvToEye(float2 uv, float z)
	{
		float2 xyPos = uv * 2.0 - 1.0;
		float4 ndcPos = float4(xyPos.xy, z, 1.0);
		float4 viewPos = mul(unity_CameraInvProjection, ndcPos);
		viewPos.xyz = viewPos.xyz / viewPos.w;
		return viewPos.xyz;
	}

	float sampleDepth(float2 uv)
	{
#if UNITY_REVERSED_Z
		return 1.0 - tex2D(_DepthBuffer, uv).r;
#else
		return tex2D(_DepthBuffer, uv).r;
#endif
	}

	float3 getEyePos(float2 uv)
	{
		return uvToEye(uv, sampleDepth(uv));
	}

	float4 frag(v2f_img i) : SV_Target
	{
		float2 uv = i.uv.xy;
		float depth = tex2D(_DepthBuffer, uv);

#if UNITY_REVERSED_Z
		if (Linear01Depth(depth) > 1.0 - 1e-3)
			discard;
#else
		if (Linear01Depth(depth) < 1e-3)
			discard;
#endif
		float2 ts = _DepthBuffer_TexelSize.xy;
		float3 posEye = getEyePos(uv);

		float3 ddx = getEyePos(uv + float2(ts.x, 0.0)) - posEye;
		//float3 ddx2 = posEye - getEyePos(uv - float2(ts.x, 0.0));
		//ddx = abs(ddx.z) > abs(ddx2.z) ? ddx2 : ddx;

		float3 ddy = getEyePos(uv + float2(0.0, ts.y)) - posEye;
		//float3 ddy2 = posEye - getEyePos(uv - float2(0.0, ts.y));
		//ddy = abs(ddy.z) > abs(ddy2.z) ? ddy2 : ddy;

		float3 N = normalize(cross(ddx, ddy));
		float4x4 vm = _ViewMatrix;
		N = normalize(mul(vm, float4(N, 0.0)));
		float4 col = float4(N * 0.5 + 0.5, 1.0);
		return col;
	}
	ENDCG

	SubShader
	{
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma target   3.0
			#pragma vertex   vert_img
			#pragma fragment frag
			ENDCG
		}
	}
}
