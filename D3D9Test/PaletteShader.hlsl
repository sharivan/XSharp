uniform sampler2D image;
uniform sampler1D palette;

float4 main(in float2 coord : TEXCOORD) : COLOR
{
	return tex1D(palette, tex2D(image, coord).r * (255. / 256) + (0.5 / 256));
}