// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/terrainShader" {
    Properties {
        _scale ("Scale", Float) = 1000
    }
    SubShader {
        Pass {
            CGPROGRAM
            #include "UnityCustomRenderTexture.cginc"

            float _scale;

            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            struct vertInput {
                float4 pos : POSITION;
            };  

            struct vertOutput {
                float4 color : COLOR;
                float4 pos : SV_POSITION;
            };

            vertOutput vert(vertInput IN) {
                float length = sqrt(
                    IN.pos.x * IN.pos.x +
                    IN.pos.y * IN.pos.y +
                    IN.pos.z * IN.pos.z);
                
                float maxLength = (1737.4 + 10.786) / _scale;
                float minLength = (1737.4 - 9.0) / _scale;

                float percent = (length - minLength) / (maxLength - minLength);

                vertOutput o;
                o.color = float4(percent, percent, percent, 1);
                o.pos = UnityObjectToClipPos(IN.pos);
                return o;
            }

            //vertOutput vert(vertInput IN) {
                
//
            //    float percent = (length - minLength) / (maxLength - minLength);
//
            //    vertOutput o;
            //    //o.color = float4(percent, percent, percent, 0);
            //    o.color = float4(0.5, 0.5, 0.5, 1);
//
            //    return o;
            //}

            float4 frag(vertOutput output) : COLOR {
                //return output.color;
                return output.color;
            }
            ENDCG
        }
    }
}
