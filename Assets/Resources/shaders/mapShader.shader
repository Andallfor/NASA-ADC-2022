Shader "Custom/mapShader" {
    Properties {
        _mainTex ("Data", 2D) = "white" {}
        _map ("Map to Use", Int) = 0 // 0 to 3
        _thres ("Use Threshold", Int) = 0 // 0 or 1
        // r, g, b, start of key
        _key1 ("Key One", Color) = (0.99824, 0.99212, 0.74902, 0)
        _key2 ("Key Two", Color) = (0.99824, 0.53725, 0.38039, 0.25)
        _key3 ("Key Three", Color) = (0.71764, 0.215686, 0.47451, 0.50)
        _key4 ("Key Four", Color) = (0.317647, 0.07059, 0.48627, 0.75)
        _key5 ("Key Five", Color) = (0.0, 0.0, 0.01569, 1)
    }
    SubShader {
        Pass {
            CGPROGRAM
            #include "UnityCustomRenderTexture.cginc"

            #pragma vertex InitCustomRenderTextureVertexShader
            #pragma fragment frag
            #pragma target 3.0

            sampler2D _mainTex;
            int _map, _thres, _output;
            float4 _key1, _key2, _key3, _key4, _key5;

            float4 frag(v2f_init_customrendertexture IN) : COLOR {
                float2 pos = float2(IN.texcoord.y, IN.texcoord.x);
                float channel = tex2D(_mainTex, pos)[_map];
                
                float4 start, end;
                if (channel <= 0.25) {start = _key1; end = _key2;}
                else if (channel <= 0.50) {start = _key2; end = _key3; channel -= 0.25;}
                else if (channel <= 0.75) {start = _key3; end = _key4; channel -= 0.50;}
                else {start = _key4; end = _key5; channel -= 0.75;}

                if (_thres == 0) {
                    channel *= 4;
                    float r = start.x + (end.x - start.x) * channel;
                    float g = start.y + (end.y - start.y) * channel;
                    float b = start.z + (end.z - start.z) * channel;

                    return float4(r, g, b, 0.1);
                } else return start;
            }
            ENDCG
        }
    }
}
