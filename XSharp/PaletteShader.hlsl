uniform sampler2D image;
uniform sampler1D palette;
uniform bool hasPalette;
uniform float fadingLevel;
uniform float4 fadingColor;

float4 main(in float2 coord : TEXCOORD) : COLOR
{
	float4 color = hasPalette ? tex1D(palette, tex2D(image, coord).r * (255. / 256) + (0.5 / 256)) : tex2D(image, coord);

	color.r = color.r * (1 - fadingLevel) + fadingLevel * fadingColor.r;
	color.g = color.g * (1 - fadingLevel) + fadingLevel * fadingColor.g;
	color.b = color.b * (1 - fadingLevel) + fadingLevel * fadingColor.b;

	return color;
}