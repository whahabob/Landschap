Shader "Custom/Terrain" {
	Properties{
		_MainTex ("Base (RGB)", 2D) = "white" {}
		testTexture("Texture", 2D) = "white"{}
		_Tess ("Tessellation", Range(1,32)) = 4
		
		_DispTex ("Disp Texture", 2D) = "gray" {}
		_NormalMap ("Normalmap", 2D) = "bump" {}
		_Displacement ("Displacement", Range(0, 1.0)) = 0.3
		_Color ("Color", color) = (1,1,1,0)
		_SpecColor ("Spec color", color) = (0.5,0.5,0.5,0.5)
	testScale("Scale", Float) = 1

	}
		SubShader{
		Tags{ "RenderType" = "Opaque" }
		LOD 200

		CGPROGRAM

			
	// Physically based Standard lighting model, and enable shadows on all light types
	#pragma surface surf Standard vertex:CustomVS fullforwardshadows addshadow 
	

	// Use shader model 3.0 target, to get nicer looking lighting
	#pragma target 3.0

	const static int maxLayerCount = 8;
	const static float epsilon = 1E-4;

	int layerCount;
	float3 baseColours[maxLayerCount];
	float baseStartHeights[maxLayerCount];
	float baseBlends[maxLayerCount];
	float baseColourStrength[maxLayerCount];
	float baseTextureScales[maxLayerCount];

	float minHeight;
	float maxHeight;

	sampler2D testTexture;
	sampler2D _MainTex;
	float testScale;
	half _Curvature;

	UNITY_DECLARE_TEX2DARRAY(baseTextures);

	struct Input {
		float2 uv_MainTex;
		float3 worldPos;
		float3 worldNormal;
		float4 color : COLOR;
	};
	struct appdata{
		float4 vertex : POSITION;
		float4 tangent: TANGENT;
	};

	void CustomVS(inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);
			float4 worldPosition = mul(unity_ObjectToWorld, v.vertex); // get world space position of vertex
			half2 wpToCam = _WorldSpaceCameraPos.xz; // get vector to camera and dismiss vertical component
			half distance = dot(wpToCam, wpToCam); // distance squared from vertex to the camera, this power gives the curvature
			worldPosition.y -= unity_DeltaTime.x; // offset vertical position by factor and square of distance.
			// the default 0.01 would lower the position by 1cm at 1m distance, 1m at 10m and 100m at 100m
			v.vertex = mul(unity_WorldToObject, worldPosition); // reproject position into object space
		}

	

	float inverseLerp(float a, float b, float value) {
		return saturate((value - a) / (b - a));
	}

	float3 triplanar(float3 worldPos, float scale, float3 blendAxes, int textureIndex) {
		float3 scaledWorldPos = worldPos / scale;
		float3 xProjection = UNITY_SAMPLE_TEX2DARRAY(baseTextures, float3(scaledWorldPos.y, scaledWorldPos.z, textureIndex)) * blendAxes.x;
		float3 yProjection = UNITY_SAMPLE_TEX2DARRAY(baseTextures, float3(scaledWorldPos.x, scaledWorldPos.z, textureIndex)) * blendAxes.y;
		float3 zProjection = UNITY_SAMPLE_TEX2DARRAY(baseTextures, float3(scaledWorldPos.x, scaledWorldPos.y, textureIndex)) * blendAxes.z;
		return xProjection + yProjection + zProjection;
	}

	void vert (inout appdata_full v) {
			float4 o = v.vertex;
			o = UnityObjectToClipPos(v.vertex);
			float3 normal = v.normal;
			float3 normalized = normalize(normal);
			
          
      }

	void surf(Input IN, inout SurfaceOutputStandard o) {
		half4 c = tex2D(_MainTex, IN.uv_MainTex);
		c *= IN.color;
		float heightPercent = inverseLerp(minHeight,maxHeight, IN.worldPos.y);
		float3 blendAxes = abs(IN.worldNormal);
		blendAxes /= blendAxes.x + blendAxes.y + blendAxes.z;

		for (int i = 0; i < layerCount; i++) {
			float drawStrength = inverseLerp(-baseBlends[i] / 2 - epsilon, baseBlends[i] / 2, heightPercent - baseStartHeights[i]);

			float3 baseColour = baseColours[i] * baseColourStrength[i];
			float3 textureColour = triplanar(IN.worldPos, baseTextureScales[i], blendAxes, i) * (1 - baseColourStrength[i]);
			
				o.Albedo = (o.Albedo * (1 - drawStrength) + (baseColour + textureColour) * drawStrength);
				o.Alpha = c.a;
		}


	}


	ENDCG
	}
		FallBack "Diffuse"
}