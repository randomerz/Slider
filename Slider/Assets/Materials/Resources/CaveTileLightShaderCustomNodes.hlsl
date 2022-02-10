void IsPosLit_float(float2 position, float4x4 _LightPos, float4x4 _LightDir, float4 _LightRadius, float4 _LightArcAngle, float4 _LightActive, out bool output) {
    output = false;
    for (int lIndex = 0; lIndex < 3; lIndex++) {
        if (_LightActive[lIndex] > 0.5f) {
            float4 pos = _LightPos[lIndex];
            float radius = _LightRadius[lIndex];

            output = length(pos.xy - position) < radius;
            if (output) {
                break;
            }
        }
    }
}

void GetShadowedPixel_float(float4 inColor, out float4 outColor) {
    if (inColor.r > 0.75) {
        outColor = float4(0.6, 0.6, 0.6, inColor.a);
    }
    else if (inColor.r > 0.5) {
        outColor = float4(0.4, 0.4, 0.4, inColor.a);
    }
    else {
        outColor = float4(0.1, 0.1, 0.1, inColor.a);
    }
}
