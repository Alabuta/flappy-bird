// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Custom/SpriteScrollUV"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _UVScroll("UVs scroll value", Vector) = (0, 0, 0, 0)
        [HideInInspector] _SpriteCorners("Sprite UV corners", Vector) = (0, 0, 0, 0)
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
        CGPROGRAM
            #pragma target 2.0
            #pragma multi_compile_instancing
            #pragma multi_compile_local _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #include "UnitySprites.cginc"

            float4 _UVScroll;
            float4 _SpriteCorners;

            fixed4 frag(v2f IN) : SV_Target
            {
                float2 texcoord = IN.texcoord;

                texcoord += _UVScroll.xy;

                if (texcoord.x > _SpriteCorners.z)
                    texcoord.x = _SpriteCorners.x + texcoord.x - _SpriteCorners.z;

                fixed4 c = SampleSpriteTexture(texcoord) * IN.color;
                c.rgb *= c.a;

                return c;
            }

            #pragma vertex SpriteVert
            #pragma fragment frag
        ENDCG
        }
    }
}
