uniform sampler2D image;
uniform sampler1D palette;

float4 main(in float2 coord : TEXCOORD) : COLOR
{
	float3 N = tex2D(image, coord);
	return tex1D(palette, ((int) (N.r * 256) + (int) (N.g * 256)) / 256. * (255. / 256) + (0.5 / 256));
	//return tex1D(palette, tex2D(image, coord).r * (255. / 256) + (0.5 / 256));
}