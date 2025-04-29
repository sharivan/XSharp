Texture2D image : register(t0);
SamplerState samp : register(s0);

cbuffer FadingParams : register(b0)
{
    float4 fadingLevel;
    float4 fadingColor;
};

struct PS_INPUT
{
    float4 pos : SV_POSITION;
    float2 uv : TEXCOORD;
};

float4 PSMain(PS_INPUT input) : SV_Target
{
    float4 color = image.Sample(samp, input.uv);
	color = lerp(color, fadingColor, fadingLevel);
	return color;
}