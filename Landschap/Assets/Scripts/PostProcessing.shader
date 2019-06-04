Shader "Hidden/Postprocessing"{
    //show values to edit in inspector
    Properties{
        [HideInInspector]_MainTex ("Texture", 2D) = "white" {}
	_BlurSize("Blur Size", Range(0,0.1)) = 0
		_BlurDistance("Blur Distance", Range(0,10)) = 4
		_BlurAmplitude("Blur amplitude", Range(0,15))= 7
    }

    SubShader{
        // markers that specify that we don't need culling 
        // or reading/writing to the depth buffer
        Cull Off
        ZWrite Off 
        ZTest Always

        Pass{
            CGPROGRAM
            //include useful shader functions
            #include "UnityCG.cginc"

            //define vertex and fragment shader
            #pragma vertex vert
            #pragma fragment frag

			float _BlurSize;
			float _BlurDistance;
			float _BlurAmplitude;
			sampler2D _CameraDepthTexture;


            //texture and transforms of the texture
            sampler2D _MainTex;

            //the object data that's put into the vertex shader
            struct appdata{
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            //the data that's used to generate fragments and can be read by the fragment shader
            struct v2f{
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            //the vertex shader
            v2f vert(appdata v){
                v2f o;
                //convert the vertex positions from object space to clip space so they can be rendered
                o.position = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
			float normpdf(float x, float sigma)
			{
				return 0.39894*exp(-0.5*x*x / (sigma*sigma)) / sigma;
			}

            //the fragment shader
			fixed4 frag(v2f i) : SV_TARGET{

				//init color variable
				float depth = tex2D(_CameraDepthTexture, i.uv).r;
				depth = Linear01Depth(depth);
				depth = depth * _ProjectionParams.z;
				fixed4 source = tex2D(_MainTex, i.uv);
              
				float4 col = 0;
				col += tex2D(_MainTex, i.uv);
				float normalizedDepth = (depth - 0) / (900 - 0);
				const float nBlur = normalizedDepth * 10;
					float turns = 1;
					if (nBlur >= _BlurDistance)
					{
						turns = 0;
						for (int k = -nBlur/2; k < nBlur/2; k++)
						{
							for (int l = -nBlur / 2; l < nBlur/2; l++)
							{
								float2 uv = i.uv + float2(k * _BlurSize, l * _BlurSize) * normpdf(float(k), _BlurAmplitude);
								col += tex2Dlod(_MainTex, float4(uv.x, uv.y, 0, 0));
								turns++;
							}
						}
					}
				col = col / turns;
				
				 

				return col;
                
			}

            ENDCG
        }
    }
}
