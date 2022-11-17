sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
sampler uImage2 : register(s2);
float3 uColor;
float uSaturation;
float3 uSecondaryColor;
float uTime;
float uOpacity;
float4 uShaderSpecificData;
float4 uSourceRect; // The position and size of the currently drawn frame.
float2 uWorldPosition;
float2 uImageSize0;
float2 uImageSize1;
float2 uImageSize2;

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
    float4 color = tex2D(uImage0, coords);
    float frameY = (coords.y * uImageSize0.y - uSourceRect.y) / uSourceRect.w;
    float shaderTime = uShaderSpecificData.x;
    
    float sinTime = (sin(shaderTime) + 1) / 2;
    if (frameY < sinTime)
    {
        return float4(0, 0, 0, 0);
    }
    else
    {
        if (frameY < sinTime + 0.0125)
        {
            color.rgb *= float3(2, 2, 0);
            return color;
        }
    }
    
    return color;
}

technique Technique1
{
	pass PyramidNPCSpawn
	{
		PixelShader = compile ps_2_0 PixelShaderFunction();
	}
}