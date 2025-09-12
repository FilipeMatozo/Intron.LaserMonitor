sampler2D inputSampler : register(s0);

// Constantes vindas do WPF (registradores de constantes)
float4 KeyColor  : register(c0); // cor a remover (RGB), ex.: preto (0,0,0)
float  Tolerance : register(c1); // quão próximo da cor-chave vira transparente
float  Softness  : register(c2); // esfuma as bordas (faixa de transição)

// Retorna 0 perto da cor-chave e 1 longe (com faixa suave Softness)
float mask_keep(float3 rgb)
{
    float d = distance(rgb, KeyColor.rgb);
    float edge0 = Tolerance;
    float edge1 = Tolerance + Softness;
    // suaviza a transição pra evitar serrilhado/halo
    float t = saturate((d - edge0) / max(1e-5, (edge1 - edge0)));
    return t; // 0 = remover, 1 = manter
}

float4 main(float2 uv : TEXCOORD) : COLOR
{
    float4 c = tex2D(inputSampler, uv);

    // Calcula alpha recortando a cor-chave
    float keep = mask_keep(c.rgb);
    float a = c.a * keep;

    // Inverte as cores preservando alpha resultante
    float3 inv = 1.0 - c.rgb;
    return float4(inv, a);
}
