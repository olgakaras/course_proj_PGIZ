struct pixelData
{
	float4	positionScreenSpace : SV_Position;
	float4	positionWorldSpace	: POSITION0;
	float4	normalWorldSpace	: POSITION1;
	float4	color				: COLOR0;
	float2	texCoord0			: TEXCOORD0;
	float2	texCoord1			: TEXCOORD1;
};

#define MAX_LIGHTS 8

// Light types.
#define DIRECTIONAL_LIGHT	0
#define POINT_LIGHT			1
#define SPOT_LIGHT			2

Texture2D	whiteTexture	: register(t0);
sampler		whiteSampler	: register(s0);

Texture2D	meshTexture		: register(t1);
sampler		meshSampler		: register(s1);

cbuffer materialProperties : register(b0) {
	float4	emissive;
	float4  ambient;
	float4  diffuse;
	float4  specular;
	float   specularPower;
	bool	textured;
	float2  mPadding; // Padding to 16 byte boundary
}

struct LightSourceProperties
{
	float4	position;
	float4	direction;
	float4	color;
	int		lightType;
	float	spotAngle;
	float	constantAttenuation;
	float	linearAttenuation;
	float	quadraticAttenuation;
	int		enabled;
	float2	lPadding; // Padding to 16 byte boundary
};

cbuffer lightProperties : register(b1)
{
	float4					eyePosition;
	float4					globalAmbient;
	LightSourceProperties	lights[MAX_LIGHTS];
};

float4 computeDiffusePart(LightSourceProperties light, float3 l, float3 n)				// l - to light, n - normal
{
	float n_dot_l = max(0, dot(n, l));
	return light.color * n_dot_l;
}

float4 computeSpecularPart(LightSourceProperties light, float3 v, float3 l, float3 n)	// v - to eye, l - to light, n - normal, r - reflection, h - half between l & v
{
	// Phong lighting.
	//float3 r = normalize(reflect(-l, n));
	//float r_dot_v = max(0, dot(r, v));

	// Blinn-Phong lighting
	float3 h = normalize(l + v);
	float n_dot_h = max(0, dot(n, h));

	return light.color * pow(n_dot_h, specularPower);
}

float computeAttenuation(LightSourceProperties light, float d)						// Fade off based on distance. d - distance
{
	return 1.0f / (light.constantAttenuation + light.linearAttenuation * d + light.quadraticAttenuation * d * d);
}

struct LightingResult												// Parts of light colors
{
	float4 diffusePart;
	float4 specularPart;
};

LightingResult computePointLight(LightSourceProperties light, float3 v, float4 p, float3 n) // v - to eye, p - point where pixel, n - normal
{
	LightingResult result;

	float3 l = (light.position - p).xyz;
	float distance = length(l);
	l = l / distance;

	float attenuation = computeAttenuation(light, distance);

	result.diffusePart = computeDiffusePart(light, l, n) * attenuation;
	//result.specularPart = (float4)0;
	result.specularPart = computeSpecularPart(light, v, l, n) * attenuation;

	return result;
}

LightingResult computeDirectionalLight(LightSourceProperties light, float3 v, float4 p, float3 n) // v - to eye, p - point where pixel, n - normal
{
	LightingResult result;

	float3 l = -light.direction.xyz;

	result.diffusePart = computeDiffusePart(light, l, n);
	result.specularPart = computeSpecularPart(light, v, l, n);

	return result;
}

LightingResult computeSpotLight(LightSourceProperties light, float3 v, float4 p, float3 n)	// v - to eye, p - point where pixel, n - normal
{
	LightingResult result;

	float3 l = (light.position - p).xyz;
	float distance = length(l);
	l = l / distance;

	float minCos = cos(light.spotAngle);
	float maxCos = (minCos + 1.0f) / 2.0f;
	float cosAngle = dot(light.direction.xyz, -l);
	float spotIntensity = smoothstep(minCos, maxCos, cosAngle);

	float attenuation = computeAttenuation(light, distance);

	result.diffusePart = computeDiffusePart(light, l, n) * attenuation * spotIntensity;
	result.specularPart = computeSpecularPart(light, v, l, n) * attenuation * spotIntensity;

	return result;
}

LightingResult computeLighting(float4 p, float3 n) // p - point, where pixel, n - normal
{
	float3 v = normalize(eyePosition - p).xyz; // eyePosition - p

	LightingResult totalResult = { {0, 0, 0, 0}, {0, 0, 0, 0} };

	[unroll]
	for (int i = 0; i < MAX_LIGHTS; ++i)
	{
		LightingResult result = { {0, 0, 0, 0}, {0, 0, 0, 0} };

		if (lights[i].enabled == 1) {
			switch (lights[i].lightType)
			{
			case DIRECTIONAL_LIGHT:
			{
				result = computeDirectionalLight(lights[i], v, p, n);
				//result.diffusePart.x += 0.1f;
			}
			break;
			case POINT_LIGHT:
			{
				result = computePointLight(lights[i], v, p, n);
				//result.diffusePart.y += 0.1f;
			}
			break;
			case SPOT_LIGHT:
			{
				result = computeSpotLight(lights[i], v, p, n);
				//result.diffusePart.z += 0.1f;
			}
			break;
			}
			totalResult.diffusePart += result.diffusePart;
			totalResult.specularPart += result.specularPart;
		}
	}

	totalResult.diffusePart = saturate(totalResult.diffusePart);
	totalResult.specularPart = saturate(totalResult.specularPart);

	return totalResult;
}

float4 pixelShader(pixelData input) : SV_Target
{
	LightingResult light = computeLighting(input.positionWorldSpace, normalize(input.normalWorldSpace).xyz);

	float4 texColor = { 1, 1, 1, 1 };

	if (textured)
	{
		texColor = meshTexture.Sample(meshSampler, input.texCoord0);
	}
	else
	{
		texColor = whiteTexture.Sample(whiteSampler, input.texCoord0) * input.color;
	}

	float4 finalColor = (
		emissive +
		ambient * globalAmbient +
		diffuse * light.diffusePart +
		specular * light.specularPart
		) * texColor;
	
	//finalColor.x = lights[1].enabled;
	//finalColor.y = (float)lights[1].lightType / 2;
	//finalColor = light.diffusePart;
	//finalColor.w = 1.0f;
	//finalColor = globalAmbient;
	//finalColor = normalize(eyePosition);

	return finalColor;

	// depth = screenSpace.z / screenSpace.w
}
