cbuffer Transform : register(b0)
{
    matrix worldViewProj;
};

struct VS_IN
{
    float3 pos : POSITION;
    float2 uv : TEXCOORD0;
};

struct VS_OUT
{
    float4 pos : SV_POSITION;
    float2 uv : TEXCOORD0;
};

VS_OUT VSMain(VS_IN input)
{
    VS_OUT output;
    output.pos = mul(float4(input.pos, 1.0f), worldViewProj);
    output.uv = input.uv;
    return output;
}