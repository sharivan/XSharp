uniform sampler2D image;
uniform sampler1D palette;

float4 main(in float2 coord : TEXCOORD) : COLOR
{
	float4 N = tex2D(image, coord);
	return tex1D(palette, (N.r * 1. + N.a * 16.) * (255. / 256) + (0.5 / 256));
}