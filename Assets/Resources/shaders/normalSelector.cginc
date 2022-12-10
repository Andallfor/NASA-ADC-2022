#ifndef MYHLSLINCLUDE_INCLUDED
#define MYHLSLSINCLUDE_INCLUDED

void Sample_Texture_float(float Selection, float2 UV, UnitySamplerState SS,
UnityTexture2D Tex_0, UnityTexture2D Tex_1, UnityTexture2D Tex_2, UnityTexture2D Tex_3, UnityTexture2D Tex_4, UnityTexture2D Tex_5, UnityTexture2D Tex_6, UnityTexture2D Tex_7, UnityTexture2D Tex_8, UnityTexture2D Tex_9, UnityTexture2D Tex_10, UnityTexture2D Tex_11, UnityTexture2D Tex_12, UnityTexture2D Tex_13, UnityTexture2D Tex_14, UnityTexture2D Tex_15, UnityTexture2D Tex_16, UnityTexture2D Tex_17, 
    out float4 New){

    if (Selection == 0) {New = SAMPLE_TEXTURE2D_LOD(Tex_0, SS, UV, 0);}
    else if (Selection == 1) {New = SAMPLE_TEXTURE2D_LOD(Tex_1, SS, UV, 0);}
    else if (Selection == 2) {New = SAMPLE_TEXTURE2D_LOD(Tex_2, SS, UV, 0);}
    else if (Selection == 3) {New = SAMPLE_TEXTURE2D_LOD(Tex_3, SS, UV, 0);}
    else if (Selection == 4) {New = SAMPLE_TEXTURE2D_LOD(Tex_4, SS, UV, 0);}
    else if (Selection == 5) {New = SAMPLE_TEXTURE2D_LOD(Tex_5, SS, UV, 0);}
    else if (Selection == 6) {New = SAMPLE_TEXTURE2D_LOD(Tex_6, SS, UV, 0);}
    else if (Selection == 7) {New = SAMPLE_TEXTURE2D_LOD(Tex_7, SS, UV, 0);}
    else if (Selection == 8) {New = SAMPLE_TEXTURE2D_LOD(Tex_8, SS, UV, 0);}
    else if (Selection == 9) {New = SAMPLE_TEXTURE2D_LOD(Tex_9, SS, UV, 0);}
    else if (Selection == 10) {New = SAMPLE_TEXTURE2D_LOD(Tex_10, SS, UV, 0);}
    else if (Selection == 11) {New = SAMPLE_TEXTURE2D_LOD(Tex_11, SS, UV, 0);}
    else if (Selection == 12) {New = SAMPLE_TEXTURE2D_LOD(Tex_12, SS, UV, 0);}
    else if (Selection == 13) {New = SAMPLE_TEXTURE2D_LOD(Tex_13, SS, UV, 0);}
    else if (Selection == 14) {New = SAMPLE_TEXTURE2D_LOD(Tex_14, SS, UV, 0);}
    else if (Selection == 15) {New = SAMPLE_TEXTURE2D_LOD(Tex_15, SS, UV, 0);}
    else if (Selection == 16) {New = SAMPLE_TEXTURE2D_LOD(Tex_16, SS, UV, 0);}
    else {New = SAMPLE_TEXTURE2D_LOD(Tex_17, SS, UV, 0);}
}

#endif