matrix translation : register(c0);

float4 main(float4 input : POSITION) : POSITION
{
	float4 output = mul(input, translation);
	return float4(0, 0, 0, 0);
}