float4x4 World;
float4x4 View;
float4x4 Projection;

texture DiffuseTexture;
texture BumpTexture;
texture NormalTexture;
float3 CameraPos;
float3 LightPosition;
float3 LightDiffuseColor; // intensity multiplier
float3 LightSpecularColor; // intensity multiplier
float LightDistanceSquared;
float3 DiffuseColor;
float3 AmbientLightColor;
float3 EmissiveColor;
float3 SpecularColor;
float SpecularPower;
bool BumpMapping = true;

sampler texsampler = sampler_state
{
  Texture = <DiffuseTexture>;
};

sampler bumpsampler = sampler_state
{
  Texture = <BumpTexture>;
};

sampler normalsampler = sampler_state
{
  Texture = <NormalTexture>;
};

struct VertexShaderInput
{
  float4 Position : POSITION0;
  float2 TexCoords : TEXCOORD0;
};

struct VertexShaderOutput
{
  float4 Position : POSITION0;
  float2 TexCoords : TEXCOORD0;
  float3 WorldPos : TEXCOORD1;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
  VertexShaderOutput output;

  // "Multiplication will be done in the pre-shader - so no cost per vertex"
  float4x4 viewprojection = mul(View, Projection);
  float4 posWorld = mul(input.Position, World);
  output.Position = mul(posWorld, viewprojection);
  output.TexCoords = input.TexCoords;

  // Passing information on to do both specular AND diffuse calculation in pixel shader
  output.WorldPos = posWorld;

  return output;
}

float4 PixelShaderFunctionWithTex(VertexShaderOutput input) : COLOR0
{
  // Get light direction for this fragment
  float3 lightDir = normalize(input.WorldPos - LightPosition); // per pixel diffuse lighting
  float3 Normal = normalize((2 * tex2D(normalsampler, input.TexCoords)) - 1.0);

  // Note: Non-uniform scaling not supported
  float diffuseLighting = saturate(dot(Normal, -lightDir));

  // Introduce fall-off of light intensity
  diffuseLighting *= (LightDistanceSquared / dot(LightPosition - input.WorldPos, LightPosition - input.WorldPos));

  // Using Blinn half angle modification for perofrmance over correctness
  float3 h = normalize(normalize(CameraPos - input.WorldPos) - lightDir);
  float specLighting = pow(saturate(dot(h, Normal)), SpecularPower);
  float4 texel = tex2D(texsampler, input.TexCoords);
  float4 bump = tex2D(bumpsampler, input.TexCoords);

  if (!BumpMapping)
  {
   bump = float4(1, 1, 1, 1);
  }

  return float4(saturate(
    AmbientLightColor +
    (texel.xyz * DiffuseColor * LightDiffuseColor * diffuseLighting * 0.6 * bump) + // Use light diffuse vector as intensity multiplier
    (SpecularColor * LightSpecularColor * specLighting * 0.5 * bump) // Use light specular vector as intensity multiplier
    ), texel.w);
  }

technique TechniqueWithTexture
{
  pass Pass1
  {
    VertexShader = compile vs_4_0_level_9_1 VertexShaderFunction();
    PixelShader = compile ps_4_0_level_9_1 PixelShaderFunctionWithTex();
  }
}