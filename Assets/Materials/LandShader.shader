Shader "Custom/AntiqueMapPaperURP"
{
    Properties
    {
        _TopColor ("Kolor Lądu (Pergamin)", Color) = (0.92, 0.87, 0.78, 1.0)
        _SideColor ("Kolor Skarp (Ciemny Brąz)", Color) = (0.35, 0.25, 0.16, 1.0)
        _EdgeColor ("Kolor Krawędzi i Linii", Color) = (0.20, 0.14, 0.08, 1.0)
        _NoiseScale ("Skala Szumu Papieru", Float) = 25.0
        _LineIntensity ("Intensywność Linii Stylistycznych", Range(0, 1)) = 0.4
    }
 
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        LOD 100
 
        Pass
        {
            Name "PaperMapPass"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
 
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 
            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
            };
 
            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float3 normalWS     : TEXCOORD0;
                float3 positionWS   : TEXCOORD1;
            };
 
            CBUFFER_START(UnityPerMaterial)
                float4 _TopColor;
                float4 _SideColor;
                float4 _EdgeColor;
                float _NoiseScale;
                float _LineIntensity;
            CBUFFER_END
 
            // Funkcja generująca proceduralny szum
            float random(float2 st) {
                return frac(sin(dot(st.xy, float2(12.9898, 78.233))) * 43758.5453123);
            }
 
            float noise(float2 st) {
                float2 i = floor(st);
                float2 f = frac(st);
                float a = random(i);
                float b = random(i + float2(1.0, 0.0));
                float c = random(i + float2(0.0, 1.0));
                float d = random(i + float2(1.0, 1.0));
                float2 u = f * f * (3.0 - 2.0 * f);
                return lerp(a, b, u.x) + (c - a) * u.y * (1.0 - u.x) + (d - b) * u.x * u.y;
            }
 
            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);
 
                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.normalWS = normalInput.normalWS;
                return output;
            }
 
            float4 frag(Varyings input) : SV_Target
            {
                float3 normal = normalize(input.normalWS);
 
                // Rozróżnienie płaskiej góry od bocznych skarp
                float topFactor = smoothstep(0.7, 0.9, normal.y);
 
                // Szum proceduralny tworzący fakturę papieru na górze
                float n = noise(input.positionWS.xz * _NoiseScale);
                float paperTexture = lerp(0.88, 1.12, n); 
 
                float4 topColored = _TopColor * paperTexture;
                float4 baseColor = lerp(_SideColor, topColored, topFactor);
 
                // Automatyczne wykrywanie krawędzi lądu (tam gdzie zaczynają się skarpy)
                float edgeFactor = fwidth(topFactor) * 18.0; 
                edgeFactor = saturate(edgeFactor);
 
                // Okazyjne, organiczne linie stylistyczne na powierzchni lądu (efekt starego rysunku/pęknięć)
                float sketchyLines = noise(input.positionWS.xz * (_NoiseScale * 0.15));
                float crackLine = smoothstep(0.48, 0.5, abs(sketchyLines - 0.5));
                crackLine = (1.0 - crackLine) * _LineIntensity * topFactor; // pojawiają się tylko na górze
 
                // Składanie ostatecznego koloru
                float3 finalColor = baseColor.rgb;
 
                // Przyciemnienie krawędzi lądu (obrys wybrzeża)
                finalColor = lerp(finalColor, _EdgeColor.rgb, edgeFactor);
 
                // Dodanie losowych, subtelnych linii stylizowanych
                finalColor = lerp(finalColor, _EdgeColor.rgb, crackLine * 0.6);
 
                return float4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }
}