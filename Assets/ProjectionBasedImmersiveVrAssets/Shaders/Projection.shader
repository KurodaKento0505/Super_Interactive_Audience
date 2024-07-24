// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Projector' with 'unity_Projector'

// Upgrade NOTE: replaced '_Projector' with 'unity_Projector'

// Upgrade NOTE: replaced '_Projector' with 'unity_Projector'

// Upgrade NOTE: replaced '_Projector' with 'unity_Projector'

// Upgrade NOTE: replaced '_Projector' with 'unity_Projector'

Shader "Custom/Projection"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)
	/*[NoScaleOffset]*/ //_Tex01("texture01", 2D) = "grey" {}
	/*[NoScaleOffset]*/ //_Tex02("texture02", 2D) = "grey" {}
	}
	SubShader
	{
			//Tags{ "Queue" = "Background" "RenderType" = "Background" }
		Tags { "Queue" = "Transparent" }
		//LOD 100
		ZWrite Off
		ColorMask RGB //don't touch alpha channel (right?)
		Blend SrcAlpha OneMinusSrcAlpha //normal alpha blending?
		Offset -1, -1 //aboid z fighting

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
				float4 uvShadow : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			//sampler2D _Tex02;
			float4 _MainTex_ST;
			//float4 _Tex02_ST;
			fixed4 _Color;

			float4x4 unity_Projector;//

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex); //UnityObjectToClipPos(v.vertex); //UnityObjectToClipPos(v.vertex);//UnityObjectToClipPos(v.vertex);;//UnityObjectToClipPos(v.vertex);
				o.uvShadow = mul(unity_Projector, v.vertex);
				//o.uv = TRANSFORM_TEX(v.uv, _Tex01);
				//o.uvShadow = mul(unity_Projector, v.vertex);//
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			bool isInsideOfProjectedArea(float4 uvShadow)
			{
				if (
					uvShadow.w > 0.0 &&
					uvShadow.x / uvShadow.w>0 &&
					uvShadow.x / uvShadow.w<1 &&
					uvShadow.y / uvShadow.w>0 &&
					uvShadow.y / uvShadow.w<1
					) 
				{
					return true;
				}
				return false;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				//fixed4 col = tex2D(_Tex01, i.uv);
				fixed4 col;
				float4 coord = UNITY_PROJ_COORD(i.uvShadow);

				if (isInsideOfProjectedArea(i.uvShadow)) {
					col = tex2Dproj(_MainTex, UNITY_PROJ_COORD(i.uvShadow));
					col.a = 1.0;
				}
				else // behind projector
				{
					col = fixed4(0.0, 0.0, 0.0, 0.0);
				}
				// apply fog
				//UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}
