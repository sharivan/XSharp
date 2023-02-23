uniform sampler2D image;
uniform float4 fadingLevel;
uniform float4 fadingColor;

float4 main(in float2 coord : TEXCOORD) : COLOR
{
	float4 color = tex2D(image, coord);

	color.a = color.a * (1 - fadingLevel.a) + fadingLevel.a * fadingColor.a;
	color.r = color.r * (1 - fadingLevel.r) + fadingLevel.r * fadingColor.r;
	color.g = color.g * (1 - fadingLevel.g) + fadingLevel.g * fadingColor.g;
	color.b = color.b * (1 - fadingLevel.b) + fadingLevel.b * fadingColor.b;

	return color;
}