sampler2D _MainTex;
float4 _MainTex_TexelSize;

float2 TC2UV(float2 uv, float2 scale, float2 offset)
{
    float2 halfOffs = _MainTex_TexelSize.xy / float2(-2, 2);
    return (uv + halfOffs) * scale + offset - halfOffs;
}

void Vertex(float4 vertex : POSITION,
            float2 texCoord : TEXCOORD,
            out float4 outVertex : SV_Position,
            out float2 outTexCoord : TEXCOORD)
{
    outVertex = float4(vertex.x * 2 - 1, 1 - vertex.y * 2, 1, 1);
    outTexCoord = texCoord;
}

#ifdef RCAM_DEMUX_COLOR

float4 Fragment(float4 vertex : SV_Position,
                float2 texCoord : TEXCOORD0) : SV_Target
{
    float3 rgb = tex2D(_MainTex, TC2UV(texCoord, float2(0.5, 1), 0)).xyz;
    float mask = tex2D(_MainTex, TC2UV(texCoord, 0.5, float2(0.5, 0))).x;
    return float4(rgb, mask);
}

#endif

#ifdef RCAM_DEMUX_DEPTH

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

// Rcam parameters
float2 _DepthRange;

// Rcam constants
static const float DepthHueMargin = 0.01;
static const float DepthHuePadding = 0.01;

// Hue value calculation
float RGB2Hue(float3 c)
{
    float minc = min(min(c.r, c.g), c.b);
    float maxc = max(max(c.r, c.g), c.b);
    float div = 1 / (6 * max(maxc - minc, 1e-5));
    float r = (c.g - c.b) * div;
    float g = 1.0 / 3 + (c.b - c.r) * div;
    float b = 2.0 / 3 + (c.r - c.g) * div;
    return lerp(r, lerp(g, b, c.g < c.b), c.r < max(c.g, c.b));
}

// Depth decoding
float DecodeDepth(float3 rgb, float2 range)
{
    // Hue decoding
    float depth = RGB2Hue(rgb);
    // Padding/margin
    depth = (depth - DepthHueMargin ) / (1 - DepthHueMargin  * 2);
    depth = (depth - DepthHuePadding) / (1 - DepthHuePadding * 2);
    // Depth range
    return lerp(range.x, range.y, depth);
}

// Depth calculation
float RGB2Depth(float3 rgb)
{
    float hue = RGB2Hue(LinearToSRGB(rgb));
    return lerp(_DepthRange.x, _DepthRange.y, hue);
}

float4 Fragment(float4 vertex : SV_Position,
                float2 texCoord : TEXCOORD) : SV_Target
{
    float3 rgb = tex2D(_MainTex, TC2UV(texCoord, 0.5, 0.5)).xyz;
    return DecodeDepth(LinearToSRGB(rgb), _DepthRange);
}

#endif
