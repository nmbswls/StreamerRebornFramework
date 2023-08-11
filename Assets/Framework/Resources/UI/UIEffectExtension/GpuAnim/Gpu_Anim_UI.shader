Shader "UI/Gpu Frame Anim"
{
    Properties
    {
        [PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
        _Color("Tint", Color) = (1,1,1,1)
        
        _FrameCount("Frame Count", Range(2, 256)) = 2
        _FramesPerSecond("Frames per Second", Range(0, 30)) = 10

        _Row("Sprite Row", Range(1, 100)) = 1
        _Column("Sprite Column", Range(1, 100)) = 1
    }

        SubShader
        {
            Tags
            {
                "Queue" = "Transparent"
                "IgnoreProjector" = "True"
                "RenderType" = "Transparent"
                "PreviewType" = "Plane"
                "CanUseSpriteAtlas" = "True"
            }


            Cull Off
            Blend SrcAlpha OneMinusSrcAlpha

            Pass
            {
                Name "Default"
            CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma target 2.0

                #include "UnityCG.cginc"
                #include "UnityUI.cginc"

                #pragma multi_compile __ UNITY_UI_CLIP_RECT
                #pragma multi_compile __ UNITY_UI_ALPHACLIP

                struct appdata_t
                {
                    float4 vertex   : POSITION;
                    float4 color    : COLOR;
                    float2 texcoord : TEXCOORD0;
                    UNITY_VERTEX_INPUT_INSTANCE_ID

                    float2 uv1 : TEXCOORD1;
                };

                struct v2f
                {
                    float4 vertex   : SV_POSITION;
                    fixed4 color : COLOR;
                    float2 texcoord  : TEXCOORD0;
                    UNITY_VERTEX_OUTPUT_STEREO
                };

                sampler2D _MainTex;
                float4 _MainTex_ST;

                fixed4 _Color;

                float _FrameCount;
                float _FramesPerSecond;
                float _Row;
                float _Column;

                v2f vert(appdata_t v)
                {
                    v2f OUT;
                    UNITY_SETUP_INSTANCE_ID(v);
                    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                    OUT.vertex = UnityObjectToClipPos(v.vertex);

                    OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);

                    OUT.color = v.color * _Color;
                    return OUT;
                }

                fixed4 frag(v2f IN) : SV_Target
                {

                    float frameIdx = floor(fmod(_Time.y * _FramesPerSecond, _FrameCount));
                    // Calculate x offset of current frame in texture
                    float indexY = floor(frameIdx / _Column);
                    float indexX = frameIdx - indexY * _Column;

                    float2 spriteIndex = float2(indexX, indexY);
                    float2 spriteSize = float2(1.0f / _Column, 1.0f / _Row);
                    float2 offset = spriteIndex * spriteSize;

                    half4 color = tex2D(_MainTex, IN.texcoord * spriteSize + offset) * IN.color;
                    return color;
                }
            ENDCG
            }
        }
}