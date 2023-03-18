uniform sampler2D image;
uniform sampler1D palette;
uniform float4 fadingLevel;
uniform float4 fadingColor;

float4 main(in float2 coord : TEXCOORD) : COLOR
{
	float4 color = tex1D(palette, tex2D(image, coord).r * (255. / 256) + (0.5 / 256));
	color = lerp(color, fadingColor, fadingLevel);
	return color;
}