Shader "Custom/TerrainShader"
{
    Properties
    {
        _PathColor("PathColor", Color) = (1.0, 1.0, 1.0, 1.0)
    }
    SubShader
    {
        Tags 
        { 
            "RenderType"="Opaque"
            "Queue" = "Geometry"
            "RenderPipeline" = "UniversalPipeline"
        }
        
        Pass
        {
            Name "ForwardLit"
            Tags
            {
                "LightMode" = "UniversalForward"
            }

            HLSLPROGRAM 
            #pragma target 4.5

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile_fog

            #pragma vertex Vertex
            #pragma fragment Fragment

            #include "TerrainShaderForwardLitPass.hlsl"

            ENDHLSL
        }
        Pass
        {
            Name "ShadowCaster"
            Tags
            {
                "LightMode" = "ShadowCaster"
            }
            ZWrite On
            ColorMask 0

            HLSLPROGRAM

            #pragma vertex Vertex
            #pragma fragment Fragment

            #include "TerrainShaderShadowCasterPass.hlsl"

            ENDHLSL
        }
        Pass
        {
            Name "DepthOnly"
            Tags{"LightMode" = "DepthOnly"}
            ZWrite On
            ColorMask 0
 
            HLSLPROGRAM
 
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment
 
            #include "Packages/com.unity.render-pipelines.universal/Shaders/UnlitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
 
            ENDHLSL
        
        }
    }
        //UsePass "Universal Render Pipeline/Lit/DepthOnly"
}

//Shader "Custom Shader" {
//    Properties {
//        _Color ("Texture Colour", Color) = (1.0, 1.0, 1.0, 1.0)
//        _MainTex ("Base (RGB)", 2D) = "white" {}
//        _DetailTex ("Detail Texture", 2D) = "gray" {}
//        _DecalTex ("Decal Texture", 2D) = "black" {}
       
//        _DecalStr ("Decal Strength", float) = 1.0
//        _Rotation ("Decal Rotation", float) = 0.0
//    }
//    SubShader {
//        Tags { "RenderType"="Opaque" }
//        LOD 200
       
//        Pass {
//            CGPROGRAM
//            #pragma vertex vert
//            #pragma fragment frag
           
//            #include "UnityCG.cginc"
   
//            sampler2D _MainTex;
//            sampler _DetailTex;
//            sampler _DecalTex;
           
//            float4 _MainTex_ST;
//            float4 _DetailTex_ST;
//            float4 _DecalTex_ST;
           
//            float _DecalStr;
//            fixed4 _Color;
//            float _Rotation;
                       
//            struct vertexOutput {
//                float4 vertexPos : POSITION;
//                float2 texCoords : TEXCOORD0;
//                float2 decalTexCoords : TEXCOORD1;
//                float4 worldPos : TEXCOORD2;
//                float3 normalDir : TEXCOORD3;
//                //float3 viewDir : TEXCOORD4;
//            };
   
//            vertexOutput vert (appdata_base vIn)
//            {
//                vertexOutput vOut;
//                vOut.texCoords = vIn.texcoord;
               
//                // Angle to rotate the decal by
//                vIn.texcoord.xy -=0.5;
//                float s = sin ( radians( _Rotation ) );
//                float c = cos ( radians( _Rotation ) );
               
//                float2x2 rotationMatrix = float2x2( c, -s, s, c );
//                rotationMatrix *= 0.5;
//                rotationMatrix += 0.5;
//                rotationMatrix = (rotationMatrix * 2) - 1;
//                vOut.decalTexCoords.xy = mul ( vIn.texcoord.xy, rotationMatrix );
//                vOut.decalTexCoords.xy += 0.5;
               
//                vOut.worldPos = mul( _Object2World, vIn.vertex );
//                vOut.normalDir = normalize( mul( float4( vIn.normal, 0.0 ), _World2Object ).xyz );
//                vOut.vertexPos = mul( UNITY_MATRIX_MVP, vIn.vertex );
//                return vOut;
//            }
           
//            // Variables for lighting
//            float3 _LightPos;
//            fixed4 _LightColour;
//            half _LightIntensity;
           
//            fixed4 frag ( vertexOutput fIn ) : COLOR
//            {
//                // General Lighting Calculations -------------------------------------------------------------
//                float3 lightDir = normalize( _LightPos - fIn.worldPos.xyz );
               
//                fixed3 diffuseLighting = _LightColour * saturate( dot ( normalize(fIn.normalDir), lightDir ) ) * _LightIntensity;
//                // Texture Mapping ---------------------------------------------------------------------------
//                // Sample the main texture
//                fixed4 mainTex = tex2D(_MainTex, fIn.texCoords * _MainTex_ST.xy + _MainTex_ST.zw  ) * _Color;
               
//                // Sample the detail map texture
//                fixed4 detailTex = tex2D(_DetailTex, fIn.texCoords * _DetailTex_ST.xy + _DetailTex_ST.zw  );
               
//                // Sample the decal texture -- Offset the tiling methods by set amounts to centre the decal
//                fixed4 decalTex = tex2D(_DecalTex, fIn.decalTexCoords * float2(_DecalTex_ST.x * 3.0, _DecalTex_ST.y * 3.0) + float2(  _DecalTex_ST.z - 1.0f, _DecalTex_ST.w -1.0f )  );
               
//                //Test whether other areas of the decal are 0 (using < or > for floating point tests)
//                if (decalTex.a > 0.001f)
//                {
//                    // If there not then were sampling the decal, lerp between main texture and decal
//                    mainTex.xyz = lerp( mainTex.xyz, decalTex.xyz, _DecalStr );
//                }
   
//                mainTex.xyz *= (detailTex.xyz * 2.0f) * + diffuseLighting;
//                mainTex.xyz *= mainTex.a;
                               
//                return mainTex;
//            }
//            ENDCG
//        }
       
       
//    }
//    FallBack "Diffuse"
//}
