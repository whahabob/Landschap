Shader "Custom/Terrain" {
	Properties{
		_MainTex ("Base (RGB)", 2D) = "white" {}
		
		_Tess ("Tessellation", Range(1,8)) = 4
		
		

	}
		SubShader{
		Tags{ "RenderType" = "Opaque" }
		

		CGPROGRAM

			
	// Physically based Standard lighting model, and enable shadows on all light types
	#pragma surface surf Standard fullforwardshadows addshadow
	

	// Use shader model 3.0 target, to get nicer looking lighting
	#pragma target 4.6

	const static int maxLayerCount = 8;
	const static float epsilon = 1E-4;

	int layerCount;
	float3 baseColours[maxLayerCount];
	float baseStartHeights[maxLayerCount];
	float baseBlends[maxLayerCount];
	float baseColourStrength[maxLayerCount];
	float baseTextureScales[maxLayerCount];

	float _Tess;
	float minHeight;
	float maxHeight;

	sampler2D testTexture;
	sampler2D _MainTex;
	sampler2D _CameraDepthTexture;
	float testScale;
	half _Curvature;
	float depth;

	UNITY_DECLARE_TEX2DARRAY(baseTextures);

	struct Input {
		float2 uv_MainTex;
		float3 worldPos;
		float3 worldNormal;
		
	};
	

	

	float inverseLerp(float a, float b, float value) {
		return saturate((value - a) / (b - a));
	}
	//texture equally spread over mesh
	float3 triplanar(float3 worldPos, float scale, float3 blendAxes, int textureIndex) {
		float3 scaledWorldPos = worldPos / scale;
		float3 xProjection = UNITY_SAMPLE_TEX2DARRAY(baseTextures, float3(scaledWorldPos.y, scaledWorldPos.z, textureIndex)) * blendAxes.x;
		float3 yProjection = UNITY_SAMPLE_TEX2DARRAY(baseTextures, float3(scaledWorldPos.x, scaledWorldPos.z, textureIndex)) * blendAxes.y;
		float3 zProjection = UNITY_SAMPLE_TEX2DARRAY(baseTextures, float3(scaledWorldPos.x, scaledWorldPos.y, textureIndex)) * blendAxes.z;
		return xProjection + yProjection + zProjection;
	}

	

	void surf(Input IN, inout SurfaceOutputStandard o) {
		half4 c = tex2D(_MainTex, IN.uv_MainTex);
		
		float heightPercent = inverseLerp(minHeight,maxHeight, IN.worldPos.y);
		float3 blendAxes = abs(IN.worldNormal);
		blendAxes /= blendAxes.x + blendAxes.y + blendAxes.z;
		
		for (int i = 0; i < layerCount; i++) {
			float drawStrength = inverseLerp(-baseBlends[i] / 2 - epsilon, baseBlends[i] / 2, heightPercent - baseStartHeights[i]);

			float3 baseColour = baseColours[i] * baseColourStrength[i];
			float3 textureColour = triplanar(IN.worldPos, baseTextureScales[i], blendAxes, i) * (1 - baseColourStrength[i]);
			
				o.Albedo = (o.Albedo * (1 - drawStrength) + (baseColour + textureColour) * drawStrength);
				
		}


	}


	ENDCG
	}
		FallBack "Diffuse"
}