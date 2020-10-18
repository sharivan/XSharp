struct VS_IN
{
	float4 pos : POSITION;
	float4 col : COLOR;
	float2 textCoords : TEXCOORD;
};

struct VS_OUT
{
	float4 pos : POSITION;
	float4 col : COLOR;
	float2 textCoords : TEXCOORD;
};

VS_OUT main(in VS_IN input)
{
	VS_OUT output = (VS_OUT) 0;

	output.pos = input.pos;
	output.col = input.col;
	output.textCoords = float2 (input.textCoords.x, input.textCoords.y);

	return output;
}