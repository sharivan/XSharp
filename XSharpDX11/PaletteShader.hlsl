Texture2D image : register(t0);
Texture1D palette : register(t1);
SamplerState samp : register(s0);

cbuffer Params : register(b0)
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
    float4 color = palette.Sample(samp, image.Sample(samp, input.uv).r * (255. / 256) + (0.5 / 256));
	color = lerp(color, fadingColor, fadingLevel);
	return color;
}