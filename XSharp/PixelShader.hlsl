uniform sampler2D image;
uniform float4 fadingLevel;
uniform float4 fadingColor;

float4 main(in float2 coord : TEXCOORD) : COLOR
{
	float4 color = tex2D(image, coord);
	color = lerp(color, fadingColor, fadingLevel);
	return color;
}