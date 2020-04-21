struct vertexData
{
	float4	position	: POSITION;
	float4	normal		: NORMAL;
	float4	color		: COLOR0;
	float2	texCoord0	: TEXCOORD0;
	float2	texCoord1	: TEXCOORD1;
};

struct pixelData
{
	float4	positionScreenSpace : SV_Position;
	float4	positionWorldSpace	: POSITION0;
	float4	normalWorldSpace	: POSITION1;
	float4	color				: COLOR0;
	float2	texCoord0			: TEXCOORD0;
	float2	texCoord1			: TEXCOORD1;
};

cbuffer perObjectData : register(b0) {
	float4x4	worldMatrix;
	float4x4	worldViewMatrix;
	float4x4	inverseTransposeMatrix;
	float4x4	worldViewProjectionMatrix;
	float		time;
	bool		timeScaling;
	float2		oPadding;			// Padding to 16 byte boundary
}

pixelData vertexShader(vertexData input)
{
	pixelData output = (pixelData)0;
	float4 position = input.position;

	float scale = 0.5f * sin(time * 0.785f) + 1.0f;
	if (timeScaling > 0) {
		position.xyz = mul(scale, position.xyz);
	}

	output.positionScreenSpace = mul(position, worldViewProjectionMatrix);
	output.positionWorldSpace = mul(position, worldMatrix);
	output.normalWorldSpace = mul(input.normal, inverseTransposeMatrix);

	output.color = input.color;
	output.texCoord0 = input.texCoord0;
	output.texCoord1 = input.texCoord1;

	return output;
}
