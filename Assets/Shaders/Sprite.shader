Shader "Custom/Sprite" {
	Properties {
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1, 1, 1, 1)
	}

	SubShader {
		Tags { 
			"Queue" = "Transparent" 
			"IgnoreProjector" = "True" 
			"RenderType" = "Transparent" 
			"PreviewType" = "Plane"
			"CanUseSpriteAtlas" = "True"
		}

		Cull Off
		Lighting Off
		ZWrite Off
		Blend One OneMinusSrcAlpha

		Pass
		{
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#pragma multi_compile _ UNITY_SINGLE_PASS_STEREO STEREO_INSTANCING_ON STEREO_MULTIVIEW_ON

			#include "UnityCG.cginc"
			
			struct appdata_t {
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f {
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
				UNITY_VERTEX_OUTPUT_STEREO
			};
			
			fixed4 _Color;

			uniform sampler2D _MainTex;
			uniform float4 _MainTex_ST;

			v2f vert(appdata_t IN) {
				v2f OUT;

				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.texcoord = TRANSFORM_TEX(IN.texcoord, _MainTex);
				OUT.color = IN.color * _Color;

				return OUT;
			}

			fixed4 frag(v2f IN) : SV_Target {
			    fixed4 c = tex2D(_MainTex, IN.texcoord);

			    // Cannot use pure black ;)
			    // #101010 is 0 and #111111 is not 0 (Use #111111 as black)
			    if (c.a != 0 && c.r == 0 && c.g == 0 && c.b == 0) {
			        // Font
			        c.rgb = IN.color.rgb;
			    } else {
			        // Sprite
			        c *= IN.color;
			    }

			    c.rgb *= c.a;

				return c;
			}
		ENDCG
		}
	}
}