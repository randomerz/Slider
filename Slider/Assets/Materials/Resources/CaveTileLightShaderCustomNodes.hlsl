void IsPosLit_float(Texture2D lightData, float2 position, out bool output) {
    uint width;
    uint height;
    lightData.GetDimensions(width, height);
    for (int lIndex = 0; lIndex < height; lIndex++) {
        float4 lightPos = lightData[float2(lIndex, 0)];
        float4 lightRadius = lightData[float2(lIndex, 2)];
        output = length(lightPos.xy - position) < lightRadius.x;
    }
}